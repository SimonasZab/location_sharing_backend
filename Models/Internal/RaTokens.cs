using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Models.Internal
{
    public class RaTokens
    {
		public JwtToken AccessTokenData { get; set; }
		public JwtToken RefreshTokenData { get; set; }

		public RaTokens(JwtToken accessTokenData, JwtToken refreshTokenData)
		{
			AccessTokenData = accessTokenData;
			RefreshTokenData = refreshTokenData;
		}
	}
}
