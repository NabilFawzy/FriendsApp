using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class LikesRepository : ILikesRepository
    {
        private readonly DataContext _context;
        public LikesRepository(DataContext context)
        {
            this._context = context;
        }

        public  async Task<UserLike> getUserLike(int SourceUserId, int LikedUserId)
        {
            return await _context.Likes.FindAsync(SourceUserId,LikedUserId);
        }

        public async Task<PagedList<LikeDto>> getUserLikes(LikesParams likesParams)
        {
            var users=_context.Users.OrderBy(u=>u.UserName).AsQueryable();

            var likes=_context.Likes.AsQueryable();
            if(likesParams.Predicate=="liked"){
                 likes=likes.Where(x=>x.SourceUserId==likesParams.UserId);
                 users=likes.Select(like=>like.LikedUser);
            }
            if(likesParams.Predicate=="likedBy"){
               likes=likes.Where(x=>x.LikedUserId==likesParams.UserId);
               users=likes.Select(like=>like.SourceUser);

            }
            var likedUsers = users.Select(user=>new LikeDto{
                Username=user.UserName,
                KnownAs=user.KnownAs,
                Age=user.DateOfBirth.CalculateAge(),
                PhotoUrl=user.Photos.FirstOrDefault(x=>x.isMain).Url,
                City=user.City,
                Id=user.Id
            });

            return await PagedList<LikeDto>.CreateAsync(likedUsers,likesParams.PageNumber,likesParams.PageSize);
        }

        public async Task<AppUser> getUserWithLikes(int UserId)
        {
            return await _context.Users
                         .Include(x=>x.LikedUsers)
                         .FirstOrDefaultAsync(x=>x.Id==UserId);
        }
    }
}