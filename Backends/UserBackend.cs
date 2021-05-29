using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Api.Models;

namespace Api.Backends
{
	public class UserBackend
	{
		/*public static string GetUserIdFromClaims(ClaimsPrincipal principal) {
			Claim? userIdClaim = principal.Claims.FirstOrDefault(x => x.Type == "UserId");
			if (userIdClaim == null) {
				throw new APIException(APIErrorCode.BAD_AUTH_TOKEN);
			}
			return userIdClaim.Value;
		}*/
	}
}
