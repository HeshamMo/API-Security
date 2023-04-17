using DevCreedJwtApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DevCreedJwtApi.Services
{
	public class AuthService:IAuthService
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		private readonly JWT _jwt;
		public AuthService(UserManager<ApplicationUser> userManager, IOptions<JWT> jwt, RoleManager<IdentityRole> roleManager)
		{
			this._userManager = userManager;
			_jwt = jwt.Value;
			_roleManager = roleManager;
		}
		public async Task<AuthModel> RegisterAsync(RegisterModel model)
		{
			if(await _userManager.FindByEmailAsync(model.Email) != null)
			{
				return new AuthModel() { Message = "Email is already in taken" };
			}
			if(await _userManager.FindByNameAsync(model.UserName) != null)
			{
				return new AuthModel() { Message = "User name is already in taken" };
			}

			var user = new ApplicationUser()
			{
				UserName = model.UserName,
				Email = model.Email,
				FName = model.FName,
				LName = model.LName,

			};

			IdentityResult result = await _userManager.CreateAsync(user, model.Passowrd);
			if(!result.Succeeded)
			{
				string errors = "";
				foreach(var error in result.Errors)
				{
					errors += $"{ error.Description} , ";
				}

				return new AuthModel() { Message = errors };
			}
			await _userManager.AddToRoleAsync(user, "User");
			var uuser = await _userManager.FindByEmailAsync(user.Email);
			var token = await CreateToken(uuser);

		
			return new AuthModel()
			{
				Email = user.Email,
				//ExpiresOn = token.ValidTo,
				IsAuthenticated = true,
				Roles = new List<string>() { "User" },
				UserName = user.UserName,
				Token = new JwtSecurityTokenHandler().WriteToken(token)
			};
		}


		public async Task<JwtSecurityToken> CreateToken(ApplicationUser user)
		{
			SymmetricSecurityKey symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.secret));
			SigningCredentials signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);
		

			var claims = await _userManager.GetClaimsAsync(user);
			var RoleClaims = new List<Claim>();
			var roles = await _userManager.GetRolesAsync(user);
			foreach(var role in roles)
			{
				RoleClaims.Add(new Claim(ClaimTypes.Role, role.ToString()));
			}


			JwtSecurityToken token = new JwtSecurityToken(
				issuer: _jwt.Issuer
				, audience: _jwt.Audience,
				claims: new Claim[]
				{
					new Claim("UserName",user.UserName),


				}.Union(RoleClaims)
				, signingCredentials: signingCredentials,
				expires: DateTime.UtcNow.AddSeconds(10) 
				);

			return token;
		}


		public async Task<AuthModel> GetToken(LogInModel model)
		{

			AuthModel authModel = new AuthModel();
			var user = await _userManager.FindByEmailAsync(model.Email);
			if(user is null || !await _userManager.CheckPasswordAsync(user, model.Passowrd))
			{
				authModel.Message = "Passowrd or Email is Incorrect!";
				return authModel;
			}

			JwtSecurityToken token = await CreateToken(user);

			authModel.IsAuthenticated = true;
			authModel.Token = new JwtSecurityTokenHandler().WriteToken(token);
			authModel.Roles = await _userManager.GetRolesAsync(user) as List<string>;
			authModel.Email = user.Email;
			authModel.UserName = user.UserName;

			
			if(user.RefreshTokens.Any(rt => rt.IsActive))
			{
				
				var refreshToken = user.RefreshTokens.FirstOrDefault(t => t.IsActive);
				
				authModel.RefreshTOken = refreshToken.Token;
				authModel.RefreshTokenExpiration = refreshToken.ExpiresOn;
			
				user.RefreshTokens.Add(refreshToken);
			
				await _userManager.UpdateAsync(user);
			}
			
			else
			{
				var refreshToken = GenerateRefreshToken();
				authModel.RefreshTOken = refreshToken.Token; 
				authModel.RefreshTokenExpiration = refreshToken.ExpiresOn;
			
				user.RefreshTokens.Add(refreshToken);
			
				await _userManager.UpdateAsync(user); 
			}

			return authModel;

		}

		public async Task<string> AddRole(AddRoleModel model)
		{
			var user = await _userManager.FindByEmailAsync(model.UserEmail);
			if(user is null || !await _roleManager.RoleExistsAsync(model.RoleName))
			{
				return "User Or role is Incorrect";
			}

			if(await _userManager.IsInRoleAsync(user, model.RoleName))
			{
				return "user already in the role";
			}

			IdentityResult result = await _userManager.AddToRoleAsync(user, model.RoleName);

			return result.Succeeded ? "Succedded" : "Something went wrong";
		}



		private RefreshToken GenerateRefreshToken()
		{
			
			var randomNumber = new byte[32];

			
			using var generator = new RNGCryptoServiceProvider();

			
			generator.GetBytes(randomNumber);

			return new RefreshToken()
			{
				Token = Convert.ToBase64String(randomNumber),
				ExpiresOn = DateTime.UtcNow.AddMinutes(30),
				CreatedOn = DateTime.UtcNow,

			}
			;
		}

		public async Task<AuthModel> RefreshTokenasync(string token)
		{
			var user = await _userManager.Users.SingleOrDefaultAsync(u => u.RefreshTokens.Any(r => r.Token == token));
			AuthModel authModel = new AuthModel(); 
			if(user is null)
			{
				authModel.IsAuthenticated = false;
				authModel.Message = "Invalid Token!"; 
				return authModel;
			}

			RefreshToken refreshToken = user.RefreshTokens.Single(r => r.Token == token); 
			if(!refreshToken.IsActive)
			{
				authModel.IsAuthenticated = false;
				authModel.Message = "InActive Token"; 
			}

			refreshToken.RevokenOn = DateTime.UtcNow;

			var newRefreshToken = GenerateRefreshToken(); 
			user.RefreshTokens.Add(newRefreshToken);
			await _userManager.UpdateAsync(user);

			var jwtToken = await CreateToken(user); 

			authModel.IsAuthenticated = true;
			authModel.RefreshTOken = newRefreshToken.Token;
			authModel.RefreshTokenExpiration = newRefreshToken.ExpiresOn;
			authModel.Token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
			authModel.Email = user.Email;
			authModel.UserName = user.UserName; 
			var roles = await _userManager.GetRolesAsync(user);
			authModel.Roles = roles.ToList();

			return authModel; 
		}
	}

}
