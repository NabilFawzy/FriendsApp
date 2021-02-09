using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class MessagesRepository : IMessagesRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public MessagesRepository(DataContext context, IMapper mapper)
        {
            this._mapper = mapper;
            this._context = context;
        }

        public void AddGroup(Group group)
        {
            _context.Groups.Add(group);
        }

        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Connection> GetConnection(string connectionId)
        {
            return await _context.Connections.FindAsync(connectionId);
        }

        public async Task<Group> GetGroupForConnection(string connectionId)
        {
            return await _context.Groups
                   .Include(c=>c.Connections)
                   .Where(c=>c.Connections.Any(x=>x.ConnectionId==connectionId))
                   .FirstOrDefaultAsync();
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages
            .Include(u=>u.Sender)
            .Include(u=>u.Recipent)
            .FirstOrDefaultAsync(x=>x.Id==id);
        }

        public async Task<Group> GetMessageGroup(string groupName)
        {
            return await _context.Groups
            .Include(x=>x.Connections)
            .FirstOrDefaultAsync(x=>x.Name==groupName);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = _context.Messages
            .OrderByDescending(m => m.MessageSent)
            .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
            .AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(u => u.RecipentUsername == messageParams.Username 
                &&u.SenderDeleted==false),
                "Outbox" => query.Where(u => u.SenderUsername == messageParams.Username
                &&u.SenderDeleted==false),
                _ => query.Where(u => u.RecipentUsername == messageParams.Username
                 &&u.RecipentDeleted==false&& u.DateRead == null)
            };


            return await PagedList<MessageDto>.CreateAsync(query,messageParams.PageNumber,messageParams.PageSize);


        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName,string recipientUserName)
        {
            var messages=await _context.Messages
                        .Where( 
                            m=>m.Recipent.UserName==currentUserName&&m.RecipentDeleted==false
                            && m.Sender.UserName==recipientUserName 
                            ||
                            m.Recipent.UserName==recipientUserName
                            && m.Sender.UserName==currentUserName &&m.SenderDeleted==false
                            )
                            .OrderBy(m=>m.MessageSent)
                            .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
                            .ToListAsync();

          var UnreadMessages=messages
                            .Where(m =>m.DateRead==null &&m.RecipentUsername==currentUserName)
                            .ToList();
          
          if(UnreadMessages.Any()){
             foreach(var message in UnreadMessages){
                 message.DateRead=DateTime.UtcNow;
             }
             //await _context.SaveChangesAsync();
          }

          return messages;
        }

        public void RemoveConnection(Connection connection)
        {
            _context.Connections.Remove(connection);
        }

        /*public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }*/
    }
}