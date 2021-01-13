using System.Collections.Generic;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces
{
    public interface ILikesRepository
    {
        Task<UserLike> getUserLike(int SourceUserId,int LikedUserId);

        Task<AppUser> getUserWithLikes(int UserId);


         //predicate to indicate want users who likes or users which liked him
         Task<PagedList<LikeDto>> getUserLikes(LikesParams likesParams);
        
    }
}