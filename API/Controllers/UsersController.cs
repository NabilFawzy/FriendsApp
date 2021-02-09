

using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{

    public class UsersController : BaseApiController
    {

        private readonly IMapper _mapper;
        private readonly IPhotoService _PhotoService;
        private readonly IUnitOfWork _unitOfWork;

        public UsersController(IUnitOfWork unitOfWork, IMapper mapper,
         IPhotoService photoService)
        {
            this._unitOfWork = unitOfWork;
            this._PhotoService = photoService;
            this._mapper = mapper;
        }


        // [Authorize(Roles="Admin")]
        [HttpGet]

        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
        {

            var gender=await _unitOfWork.userRepository.GetUserGender(User.GetUserName());
            userParams.currentUserName = User.GetUserName();
            if (string.IsNullOrEmpty(userParams.Gender))
                userParams.Gender = gender == "male" ? "female" : "male";

            var users = await _unitOfWork.userRepository.GetMembersAsync(userParams);
            Response.AddPaginationHeader(
                users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);
            return Ok(users);
        }


        //[Authorize(Roles="Member")]
        [HttpGet("{username}", Name = "GetUser")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            return await _unitOfWork.userRepository.GetMemberAsync(username);

        }

        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var username = User.GetUserName();
            var user = await _unitOfWork.userRepository.GetUserByUserNameAsync(username);

            _mapper.Map(memberUpdateDto, user);
            _unitOfWork.userRepository.Update(user);

            if (await _unitOfWork.Complete())
            {
                return NoContent();
            }
            return BadRequest("Failed to update user");
        }
        [HttpPost("add-photo")]
        public async Task<ActionResult<PhotoDto>> AddPhoto(IFormFile file)
        {
            var user = await _unitOfWork.userRepository.GetUserByUserNameAsync(User.GetUserName());
            var result = await _PhotoService.AddPhotoAsync(file);
            if (result.Error != null) return BadRequest(result.Error.Message);

            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicID = result.PublicId

            };
            if (user.Photos.Count == 0)
            {
                photo.isMain = true;
            }
            user.Photos.Add(photo);

            if (await _unitOfWork.Complete())
            {
                // return _mapper.Map<PhotoDto> (photo);
                return CreatedAtRoute("GetUser", new { username = user.UserName }, _mapper.Map<PhotoDto>(photo));
            }

            return BadRequest("Problem adding Photo");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> setMainPhoto(int photoId)
        {
            var user = await _unitOfWork.userRepository.GetUserByUserNameAsync(User.GetUserName());
            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);
            if (photo.isMain) return BadRequest("This is already your main photo");

            var currentMain = user.Photos.FirstOrDefault(x => x.isMain);

            if (currentMain != null) currentMain.isMain = false;
            photo.isMain = true;

            if (await _unitOfWork.Complete()) return NoContent();

            return BadRequest("Failed to set main photo");
        }
        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await _unitOfWork.userRepository.GetUserByUserNameAsync(User.GetUserName());

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo == null) return NotFound();
            if (photo.isMain) return BadRequest("You can't delete your main photo");
            if (photo.PublicID != null)
            {
                var result = await _PhotoService.DeletePhotoAsync(photo.PublicID);
                if (result.Error != null) return BadRequest(result.Error.Message);
            }
            user.Photos.Remove(photo);
            if (await _unitOfWork.Complete()) return Ok();

            return BadRequest("Failed to delete Photo");

        }



    }
}