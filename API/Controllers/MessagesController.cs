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

        private readonly IUnitOfWork _unitOfWork;

        private readonly IMapper _mapper;
        public MessagesController(IMapper mapper, IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
            this._mapper = mapper;

        }

        [HttpPost]
        public async Task<ActionResult<MessageDto>> CreateMessage(CreateMessageDto createMessageDto)
        {
            var username = User.GetUserName();

            if (username == createMessageDto.RecipentUsername.ToLower())
                return BadRequest("You can not send messages to yourself");

            var sender = await _unitOfWork.userRepository.GetUserByUserNameAsync(username);
            var recipient = await _unitOfWork.userRepository.GetUserByUserNameAsync(createMessageDto.RecipentUsername);

            if (recipient == null) return NotFound();

            var message = new Message
            {
                Sender = sender,
                Recipent = recipient,
                SenderUsername = sender.UserName,
                RecipentUsername = recipient.UserName,
                Content = createMessageDto.Content
            };

            _unitOfWork.messagesRepository.AddMessage(message);

            if (await _unitOfWork.Complete())
                return Ok(_mapper.Map<MessageDto>(message));


            return BadRequest("Failed to send a message");

        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessagesForUser(
            [FromQuery] MessageParams messageParams)
        {

            messageParams.Username = User.GetUserName();
            var messages = await _unitOfWork.messagesRepository.GetMessagesForUser(messageParams);

            Response.AddPaginationHeader(messages.CurrentPage, messages.PageSize, messages.TotalCount, messages.TotalPages);

            return messages;
        }

        /*[HttpGet("thread/{username}")]
        public async Task<ActionResult<IEnumerable<MessageDto>>> GetMessageThread(string username){
            var currentUserName=User.GetUserName();
           var messages=await _unitOfWork.messagesRepository.GetMessageThread( currentUserName, username);

            return Ok(messages);
        }*/

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteMessage(int id)
        {
            var username = User.GetUserName();
            var message = await _unitOfWork.messagesRepository.GetMessage(id);

            if (message.Sender.UserName != username && message.Recipent.UserName != username)
            {
                return Unauthorized();
            }

            if (message.Sender.UserName == username)
            {
                message.SenderDeleted = true;
            }
            if (message.Recipent.UserName == username)
            {
                message.RecipentDeleted = true;
            }

            if (message.RecipentDeleted && message.SenderDeleted)
            {
                _unitOfWork.messagesRepository.DeleteMessage(message);
            }

            if (await _unitOfWork.Complete()) return Ok();

            return BadRequest("Problem deleting the message");

        }
    }
}