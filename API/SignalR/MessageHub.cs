using System;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    public class MessageHub : Hub
    {
        private readonly IMessagesRepository _messageRepository;
        private readonly IMapper _mapper;
        
        private readonly IUserRepository _userRepository;
        private readonly IHubContext<PresenceHub> _presenceHub;
        private readonly PresenceTracker _tracker;

        public MessageHub(IMessagesRepository messageRepository, IMapper mapper
        ,IUserRepository userRepository,IHubContext<PresenceHub> presenceHub,
        PresenceTracker tracker)
        {
            this._userRepository = userRepository;
            this._presenceHub = presenceHub;
            this._tracker = tracker;
            this._mapper = mapper;
            this._messageRepository = messageRepository;

        }

      public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            var otherUser = httpContext.Request.Query["user"].ToString();
            var groupName = GetGroupName(Context.User.GetUserName(), otherUser);
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            var group=await AddToGroup(groupName);
             
            await Clients.Group(groupName).SendAsync("UpdatedGroup",group);

            var messages = await _messageRepository.
                GetMessageThread(Context.User.GetUserName(), otherUser);

            await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
        }


    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var group=await RemoveFromMessageGroup();
        await Clients.Group(group.Name).SendAsync("UpdatedGroup",group); 
        await base.OnDisconnectedAsync(exception);
    }

    public async Task SendMessage(CreateMessageDto createMessageDto)
    {

        var username = Context.User.GetUserName();

        if (username == createMessageDto.RecipentUsername.ToLower())
            throw new HubException("You can not send messages to yourself");

        var sender = await _userRepository.GetUserByUserNameAsync(username);
        var recipient = await _userRepository.GetUserByUserNameAsync(createMessageDto.RecipentUsername);

        if (recipient == null)  throw new HubException("Not found user");

        var message = new Message
        {
            Sender = sender,
            Recipent = recipient,
            SenderUsername = sender.UserName,
            RecipentUsername = recipient.UserName,
            Content = createMessageDto.Content
        };
       var groupName=GetGroupName(sender.UserName,recipient.UserName);

       var group=await _messageRepository.GetMessageGroup(groupName);
       if(group.Connections.Any(x=>x.Username==recipient.UserName)){
           message.DateRead=DateTime.UtcNow;
       }
       else{
           var connection= await _tracker.GetConnectionsForUser(recipient.UserName);
           if(connection!=null){
               await _presenceHub.Clients.Clients(connection).SendAsync("NewMessageReceived",
               new {username=sender.UserName,knownAs=sender.KnownAs});
           }
       }
        _messageRepository.AddMessage(message);

        if (await _messageRepository.SaveAllAsync()){
         
            await Clients.Group(groupName).SendAsync("NewMessage",_mapper.Map<MessageDto>(message));
        }
         
    }

    private async Task<Group> AddToGroup(string groupName){
      var group=await _messageRepository.GetMessageGroup(groupName);
      var connection=new Connection(Context.ConnectionId,Context.User.GetUserName());
      
      if(group==null){
          group=new Group(groupName);
          _messageRepository.AddGroup(group);
      }
      group.Connections.Add(connection);

      if( await _messageRepository.SaveAllAsync())return group;

      throw new HubException("Failed to join group");
    }

    private async Task<Group> RemoveFromMessageGroup(){
        var group=await _messageRepository.GetGroupForConnection(Context.ConnectionId);
        var connection=group.Connections.FirstOrDefault(x=>x.ConnectionId==Context.ConnectionId);
        _messageRepository.RemoveConnection(connection);

        if(await _messageRepository.SaveAllAsync())return group;

        throw new HubException("Failed to remove from group");

    }

    public string GetGroupName(string caller, string other)
    {
        var stringCompare = string.CompareOrdinal(caller, other) < 0;
        return stringCompare ? $"{caller},{other}" : $"{other},{caller}";
    }
}
}