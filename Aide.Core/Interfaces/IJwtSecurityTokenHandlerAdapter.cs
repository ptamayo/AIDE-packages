using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Aide.Core.Interfaces
{
	public interface IJwtSecurityTokenHandlerAdapter
	{
		SecurityToken CreateToken(SecurityTokenDescriptor tokenDescriptor);
		JwtSecurityToken ReadJwtToken(string token);
		string WriteToken(SecurityToken token);
	}
}
