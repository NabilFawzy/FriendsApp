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

        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages
            .Include(u=>u.Sender)
            .Include(u=>u.Recipent)
            .FirstOrDefaultAsync(x=>x.Id==id);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = _context.Messages.OrderByDescending(m => m.MessageSent).AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(u => u.Recipent.UserName == messageParams.Username 
                &&u.SenderDeleted==false),
                "Outbox" => query.Where(u => u.Sender.UserName == messageParams.Username
                &&u.SenderDeleted==false),
                _ => query.Where(u => u.Recipent.UserName == messageParams.Username
                 &&u.RecipentDeleted==false&& u.DateRead == null)
            };

            var messages=query.ProjectTo<MessageDto>(_mapper.ConfigurationProvider);

            return await PagedList<MessageDto>.CreateAsync(messages,messageParams.PageNumber,messageParams.PageSize);


        }

        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUserName,string recipientUserName)
        {
            var messages=await _context.Messages
                        .Include(u=>u.Sender).ThenInclude(p=>p.Photos)
                        .Include(u=>u.Recipent).ThenInclude(p=>p.Photos)
                        .Where( 
                            m=>m.Recipent.UserName==currentUserName&&m.RecipentDeleted==false
                            && m.Sender.UserName==recipientUserName 
                            ||
                            m.Recipent.UserName==recipientUserName
                            && m.Sender.UserName==currentUserName &&m.SenderDeleted==false
                            )
                            .OrderBy(m=>m.MessageSent)
                            .ToListAsync();

          var UnreadMessages=messages
                            .Where(m =>m.DateRead==null &&m.Recipent.UserName==currentUserName)
                            .ToList();
          
          if(UnreadMessages.Any()){
             foreach(var message in UnreadMessages){
                 message.DateRead=DateTime.Now;
             }
             await _context.SaveChangesAsync();
          }

          return _mapper.Map<IEnumerable<MessageDto>> (messages);
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}