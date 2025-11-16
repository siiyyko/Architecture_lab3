using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UserService.Data;
using UserService.DTOs;
using UserService.Models;

namespace UserService.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserServiceContext _context;

        public UsersController(UserServiceContext context)
        {
            _context = context;
        }

        // GET: api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUserEntity()
        {
            return await _context.UserEntity
                .Select(user => new UserDto
                {
                    Id = user.Id,
                    Email = user.Email,
                    UserName = user.UserName
                })
                .ToListAsync();
        }

        // GET: api/users/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUserEntity(Guid id)
        {
            var userEntity = await _context.UserEntity.FindAsync(id);

            if (userEntity == null)
            {
                return NotFound();
            }

            var userDto = new UserDto
            {
                Id = userEntity.Id,
                Email = userEntity.Email,
                UserName = userEntity.UserName
            };

            return userDto;
        }
    }
}