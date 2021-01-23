using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{

    [Authorize]
    public class MessagesController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly IMessagesRepository _messagesRepository;
        
        private readonly IMapper _mapper;
        public MessagesController(IUserRepository userRepository,
        IMessagesRepository messagesRepository, IMapper mapper)
        {
            this._mapper = mapper;
            this._messagesRepository = messagesRepository;
            this._userRepository = userRepository;

        }

    [HttpPost]
    public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
    {
        var username = User.GetUserName();

        if (username == createMessageDto.RecipentUsername.ToLower())
            return BadRequest("You can not send messages to yourself");

        var sender = await _userRepository.GetUserByUserNameAsync(username);
        var recipient = await _userRepository.GetUserByUserNameAsync(createMessageDto.RecipentUsername);

        if (recipient == null) return NotFound();

        var message = new Message
        {
            Sender = sender,
            Recipent = recipient,
            SenderUsername = sender.UserName,
            RecipentUsername = recipient.UserName,
            Content = createMessageDto.Content
        };

        _messagesRepository.AddMessage(message);

        if( await _messagesRepository.SaveAllAsync()) 
           return Ok(_mapper.Map<MessageDto>(message));

        
        return BadRequest("Failed to send a message");

    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser(
        [FromQuery] MessageParams messageParams){

        messageParams.Username=User.GetUserName();
        var messages=await _messagesRepository.GetMessagesForUser(messageParams);
        
       Response.AddPaginationHeader(messages.CurrentPage,messages.PageSize,messages.TotalCount,messages.TotalPages);

       return messages;
    }

    [HttpGet("thread/{username}")]
    public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username){
        var currentUserName=User.GetUserName();
       var messages=await _messagesRepository.GetMessageThread( currentUserName, username);
     
        return Ok(messages);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMessage(int id){
       var username=User.GetUserName();
       var message=await _messagesRepository.GetMessage(id);

       if(message.Sender.UserName !=username  && message.Recipent.UserName!=username){
           return Unauthorized();
       }

       if(message.Sender.UserName==username) {
           message.SenderDeleted=true;
       }
       if(message.Recipent.UserName==username) {
           message.RecipentDeleted=true;
       }

       if(message.RecipentDeleted  && message.SenderDeleted){
           _messagesRepository.DeleteMessage(message);
       }

       if(await _messagesRepository.SaveAllAsync()) return Ok();

       return BadRequest("Problem deleting the message");

    }
}
}