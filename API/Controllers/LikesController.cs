using System.Threading.Tasks;
using API.Extensions;
using API.Interfaces;
using API.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using API.DTOs;
using API.Helpers;

namespace API.Controllers
{
    [Authorize]
    public class LikesController : BaseApiController
    {

        private readonly IUnitOfWork _unitOfWork;

        public LikesController(IUnitOfWork unitOfWork)
        {
            this._unitOfWork = unitOfWork;
        }


        //ex api/likes/lisa
        [HttpPost("{username}")]
        public async Task<ActionResult> addLike(string username)
        {
            var SourceUserId = User.GetUserId();
            var likedUser = await _unitOfWork.userRepository.GetUserByUserNameAsync(username);
            var SourceUser = await _unitOfWork.likesRepository.getUserWithLikes(SourceUserId);

            if (likedUser == null) return NotFound();

            if (SourceUser.UserName == username) return BadRequest("You cannot like yourself");

            var userLike = await _unitOfWork.likesRepository.getUserLike(SourceUserId, likedUser.Id);

            if (userLike != null) return BadRequest("You already liked before");

            userLike = new UserLike
            {
                SourceUserId = SourceUserId,
                LikedUserId = likedUser.Id
            };

            SourceUser.LikedUsers.Add(userLike);

            if (await _unitOfWork.Complete()) return Ok();

            return BadRequest("Failed to like user");

        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeDto>>> GetUserLikes([FromQuery] LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId();
            var users = await _unitOfWork.likesRepository.getUserLikes(likesParams);

            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(users);
        }

    }
}