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
    public class UserRepository : IUserRepository
    {
        private readonly DataContext _context ;
        private readonly IMapper _mapper;

        public UserRepository(DataContext _context,IMapper mapper )
        {
            this._context = _context;
            this._mapper = mapper;
        }

        public async Task<MemberDto> GetMemberAsync(string username)
        {
            return await _context.Users
            .Where(x=>x.UserName==username)
            .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();
            //shof b2a be map m3 meen w amlah
            
        }

        public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
        {
             var query= _context.Users.AsQueryable();

             query=query.Where(u=>u.UserName!=userParams.currentUserName);
             query=query.Where(u=>u.Gender==userParams.Gender);
             var minDob=DateTime.Today.AddYears(-userParams.MaxAge-1);
             var maxDob=DateTime.Today.AddYears(-userParams.MinAge);
             query=query.Where(u=>u.DateOfBirth>=minDob&&u.DateOfBirth<=maxDob);

            query=userParams.OrderBy switch{
                "created"=>query.OrderByDescending(u=>u.Created),
                 _=>query.OrderByDescending(u=>u.LastActive)//   _ default in switch
            };


             var projectedQuery=query.ProjectTo<MemberDto>(_mapper.ConfigurationProvider).AsNoTracking();

            return await PagedList<MemberDto>.CreateAsync(projectedQuery,userParams.PageNumber,userParams.PageSize);

        }

        public async Task<AppUser> GetUserByIdAsync(int id)
        {
            return await this._context.Users
            .FindAsync(id);
        }

        public async Task<AppUser> GetUserByUserNameAsync(string username)
        {
            return await _context.Users
            .Include(p=>p.Photos)
            .SingleOrDefaultAsync(x=>x.UserName==username);
        }

        public async Task<IEnumerable<AppUser>> GetUsersAsync()
        {
            return await _context.Users
            .Include(p=>p.Photos)
            .ToListAsync();
        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync()>0;
        }
        public async Task<bool> SaveAllAsync()
        {
            return await _context.SaveChangesAsync()>0;
        }

        public void Update(AppUser appUser)
        {
            _context.Entry(appUser).State=EntityState.Modified;//if changed make state of object changed
        }
    }
}