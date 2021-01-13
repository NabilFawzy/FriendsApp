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
        private readonly IUserRepository _userRepository;
        private readonly ILikesRepository _likesRepository;

        public LikesController(IUserRepository userRepository,ILikesRepository likesRepository)
        {
            this._userRepository = userRepository;
            this._likesRepository = likesRepository;
        }


       //ex api/likes/lisa
        [HttpPost("{username}")]
        public async Task<ActionResult> addLike(string username){
            var SourceUserId=User.GetUserId();
            var likedUser=await _userRepository.GetUserByUserNameAsync(username);
            var  SourceUser=await _likesRepository.getUserWithLikes(SourceUserId);

            if(likedUser==null) return NotFound();

            if(SourceUser.UserName==username) return BadRequest("You cannot like yourself");

            var userLike=await _likesRepository.getUserLike(SourceUserId,likedUser.Id);

            if(userLike!=null) return BadRequest("You already liked before");

            userLike=new UserLike{
                SourceUserId=SourceUserId,
                LikedUserId=likedUser.Id
            };
           
           SourceUser.LikedUsers.Add(userLike);

           if(await _userRepository.SaveAllAsync()) return Ok();

           return BadRequest("Failed to like user");

        }
         [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeDto>>> GetUserLikes([FromQuery]LikesParams likesParams){
            likesParams.UserId=User.GetUserId();
            var users= await _likesRepository.getUserLikes(likesParams);
            
            Response.AddPaginationHeader(users.CurrentPage,users.PageSize,users.TotalCount,users.TotalPages);

            return Ok(users);
        }
        
    }
}