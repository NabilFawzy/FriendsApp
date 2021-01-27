using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{

    public class AccountController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        public AccountController(UserManager<AppUser> userManager, 
       SignInManager<AppUser>signInManager, ITokenService tokenService, IMapper mapper)
        {
            this._mapper = mapper;
            this._userManager = userManager;
            this._signInManager = signInManager;
            this._tokenService = tokenService;
           
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await checkExist(registerDto.Username)) return BadRequest("Username is exist");
             
            var user=_mapper.Map<AppUser>(registerDto);

            user.UserName = registerDto.Username.ToLower();
            
            
            var result=await _userManager.CreateAsync(user,registerDto.password);
            
            if(!result.Succeeded)return BadRequest(result.Errors);

            var roleResult= await _userManager.AddToRoleAsync(user,"Member");

            if(!roleResult.Succeeded)return BadRequest(result.Errors);
 

            return new UserDto
            {
                Username = user.UserName,
                Token = await _tokenService.CreateToken(user),
                KnownAs=user.KnownAs,
                Gender=user.Gender
            };

        }
        private async Task<bool> checkExist(string username)
        {
            return await _userManager.Users.AnyAsync(x => x.UserName == username.ToLower());
        }


        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _userManager.Users
            .Include(p => p.Photos)
            .SingleOrDefaultAsync(
                x => x.UserName == loginDto.Username
                );
            if (user == null)
            {
                return Unauthorized("Invalid Username");
            }
       
           var result=await _signInManager.CheckPasswordSignInAsync(user,loginDto.password,false);

           if(!result.Succeeded)return Unauthorized();

           var d=new UserDto()
            {
                Username = user.UserName,
                Token = await _tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.isMain)?.Url,
                KnownAs=user.KnownAs,
                Gender=user.Gender
            };

            return d;
        }

    }
}