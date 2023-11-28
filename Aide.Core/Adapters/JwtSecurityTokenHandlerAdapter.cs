using Aide.Core.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace Aide.Core.Adapters
{
	public class JwtSecurityTokenHandlerAdapter : IJwtSecurityTokenHandlerAdapter
	{
		private readonly JwtSecurityTokenHandler _tokenHandler;

		public JwtSecurityTokenHandlerAdapter()
		{
			_tokenHandler = new JwtSecurityTokenHandler();
		}

		public SecurityToken CreateToken(SecurityTokenDescriptor tokenDescriptor)
		{
			return _tokenHandler.CreateToken(tokenDescriptor);
		}

		public string WriteToken(SecurityToken token)
		{
			return _tokenHandler.WriteToken(token);
		}

		public JwtSecurityToken ReadJwtToken(string token)
		{
			return _tokenHandler.ReadJwtToken(token);
		}
	}
}
