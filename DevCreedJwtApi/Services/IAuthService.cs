using DevCreedJwtApi.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;

namespace DevCreedJwtApi.Services
{
	public interface IAuthService
	{
		Task<AuthModel> RegisterAsync(RegisterModel model);

		Task<JwtSecurityToken> CreateToken(ApplicationUser user);

		Task<AuthModel> GetToken(LogInModel model);

		Task<String> AddRole(AddRoleModel model);

		Task<AuthModel> RefreshTokenasync(string token);
	}
}
