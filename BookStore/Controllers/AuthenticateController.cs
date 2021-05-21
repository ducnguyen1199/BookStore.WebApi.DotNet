﻿using AutoMapper;
using BookStore.Core.Entity;
using BookStore.Core.Enum;
using BookStore.Core.FilterModel;
using BookStore.Core.Repository;
using BookStore.Core.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace BookStore.Controllers
{
	[Route("[controller]")]
	[ApiController]
	public class AuthenticateController : ControllerBase
	{
		private readonly UserManager<User> _userManager;
		private readonly RoleManager<Role> _roleManager;
		private readonly IConfiguration _configuration;
		private readonly IUserReponsitory _userReponsitory;
		private readonly IMapper _mapper;
	

		public AuthenticateController(UserManager<User> userManager, RoleManager<Role> roleManager, IMapper mapper, IUserReponsitory userReponsitory, IConfiguration configuration)
		{
			_userManager = userManager;
			_roleManager = roleManager;
			_mapper = mapper;
			_userReponsitory = userReponsitory;
			_configuration = configuration;
		}

		[HttpPost("register")]
		public async Task<IActionResult> Register([FromBody] RegisterFilterModel filter)
		{
			User userExist = await _userReponsitory.GetByUserName(filter.UserName);
			if (userExist != null)
			{
				return BadRequest(new { Success = false, Message = "User already exists!" });
			}
			if (string.IsNullOrWhiteSpace(filter.UserName))
				return BadRequest(new { Success = false, Message = "UserName is required" });

			if (string.IsNullOrWhiteSpace(filter.Password))
				return BadRequest(new { Success = false, Message = "Password is required" });
			if (string.IsNullOrWhiteSpace(filter.Email))
				return BadRequest(new { Success = false, Message = "Email is required" });
			if (string.IsNullOrWhiteSpace(filter.PhoneNumber))
				return BadRequest(new { Success = false, Message = "PhoneNumber is required" });

			User user = new User()
			{
				UserName = filter.UserName,
				BirthDay = filter.BirthDay,
				PhoneNumber = filter.PhoneNumber,
				Email = filter.Email,
				FullName = filter.FullName,
				SecurityStamp = Guid.NewGuid().ToString(),
			};

			var result = await _userManager.CreateAsync(user, filter.Password);

			if (!result.Succeeded)
				return StatusCode(StatusCodes.Status500InternalServerError, new { Success = false, Message = "User creation failed! Please check user details and try again." });


			if (!await _roleManager.RoleExistsAsync(RoleType.Admin))
				await _roleManager.CreateAsync(new Role(RoleType.Admin));
			if (!await _roleManager.RoleExistsAsync(RoleType.Customer))
				await _roleManager.CreateAsync(new Role(RoleType.Customer));

			await _userManager.AddToRoleAsync(user, RoleType.Customer);
			return Ok(new { Success = true, Message = "User created successfully!" });
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginFilterModel filter)
		{
			User user = await _userReponsitory.GetByUserName(filter.UserName);
			if (user != null)
			{
				if (!await _userManager.CheckPasswordAsync(user, filter.Password)) return Unauthorized(new { Success = false, Message = "UserName or Password is not match" });

				var RoleType = await _userManager.GetRolesAsync(user);
				var authClaims = new List<Claim>
					{
						new Claim(ClaimTypes.Name,user.UserName),
						new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
					};
				foreach (var userRole in RoleType)
				{
					authClaims.Add(new Claim(ClaimTypes.Role, userRole));
				}
				var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));

				var token = new JwtSecurityToken(
					issuer: _configuration["JWT:Issuer"],
					audience: _configuration["JWT:Audience"],
					expires: DateTime.Now.AddHours(3),
					claims: authClaims,
					signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
				);
				return Ok(new
				{
					success = true,
					token = new JwtSecurityTokenHandler().WriteToken(token),
					expiration = token.ValidTo,
					user = _mapper.Map<UserViewModel>(user)
				});
			}
			return Unauthorized(new { Success = false, Message = "UserName or Password is not match" });
		}
	}
}