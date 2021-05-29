using Api.Models.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Api.Models.Internal
{
	public class AuthClaims
	{
		public string UserId { get; set; }
		public string Jti { get; set; }
		public bool Persist { get; set; }

		public static AuthClaims ParseClaimsPrincipal(ClaimsPrincipal claimsPrincipal)
		{
			AuthClaims authClaims = new AuthClaims()
			{
				UserId = TryGetClaim(claimsPrincipal, nameof(UserId)),
				Jti = TryGetClaim(claimsPrincipal, nameof(Jti)),
				Persist = TryGetClaim<bool>(claimsPrincipal, nameof(Persist)),
			};

			return authClaims;
		}

		private static T TryGetClaim<T>(ClaimsPrincipal claimsPrincipal, string name)
		{
			string strVal = TryGetClaim(claimsPrincipal, name);
			try
			{
				return (T)Convert.ChangeType(strVal, typeof(T));
			} catch{
				throw new ApiException();
			}
		}

		private static string TryGetClaim(ClaimsPrincipal claimsPrincipal, string name)
		{
			Claim? claim = claimsPrincipal.Claims.FirstOrDefault(x => x.Type == name);
			if (claim == null)
			{
				throw new ApiException();
			}
			return claim.Value;
		}

		public static ClaimsIdentity ToClaimsIdentity(User user, string jti, bool persist)
		{
			return new ClaimsIdentity(new List<Claim> {
				new Claim(nameof(UserId), user.Id),
				new Claim(nameof(Jti), jti),
				new Claim(nameof(Persist), persist.ToString()),
			});
		}

		public ClaimsIdentity ToClaimsIdentity()
		{
			return new ClaimsIdentity(new List<Claim> {
				new Claim(nameof(UserId), UserId),
				new Claim(nameof(Jti), Jti),
				new Claim(nameof(Persist), Persist.ToString()),
			});
		}
	}
}
