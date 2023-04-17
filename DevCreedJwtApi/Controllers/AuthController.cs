using DevCreedJwtApi.Models;
using DevCreedJwtApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace DevCreedJwtApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController:ControllerBase
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly IAuthService authService;
		public AuthController(IAuthService authService, UserManager<ApplicationUser> userManager)
		{
			this.authService = authService;
			this._userManager = userManager;
		}

		[HttpPost("register")]
		public async Task<IActionResult> RegisterAsync([FromBody] RegisterModel model)
		{
			if(!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			var result = await authService.RegisterAsync(model);
			if(!result.IsAuthenticated)
			{
				return BadRequest(result.Message);
			}
			AddRefreshTokenToCookie(authModelLogInResult.RefreshTOken, authModelLogInResult.RefreshTokenExpiration);

			return Ok(result);
		}


		[HttpPost("LogIn")]
		public async Task<IActionResult> GetTokenAsync([FromBody] LogInModel model)
		{
			if(!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}
			var authModelLogInResult = await authService.GetToken(model);
			if(!authModelLogInResult.IsAuthenticated)
			{
				return BadRequest(authModelLogInResult.Message);
			}

			if(!string.IsNullOrEmpty(authModelLogInResult.RefreshTOken))
			{
				AddRefreshTokenToCookie(authModelLogInResult.RefreshTOken, authModelLogInResult.RefreshTokenExpiration);
			}


			return Ok(authModelLogInResult);
		}


		[HttpPost("AddRole")]

		public async Task<IActionResult> AddToRoleAsync([FromBody] AddRoleModel model)
		{
			if(!ModelState.IsValid)
			{
				return BadRequest(ModelState);
			}

			string result = await authService.AddRole(model);

			return Ok(result);
		}
		public void AddRefreshTokenToCookie(string refreshToken, DateTime Expiration)
		{
			CookieOptions cookieOptions = new CookieOptions()
			{
				Expires = Expiration.ToLocalTime(),
				HttpOnly = true,

			};

			Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
		}


		[HttpGet("RefreshToken")]
		public async Task<IActionResult> RefreshToken()
		{
			var refreshToken = Request.Cookies["refreshToken"];

			var result = await authService.RefreshTokenasync(refreshToken);

			if(!result.IsAuthenticated)
			{
				return BadRequest(result); 
			}
			AddRefreshTokenToCookie(result.RefreshTOken, result.RefreshTokenExpiration); 
			return Ok(result);
		}
	}
}
