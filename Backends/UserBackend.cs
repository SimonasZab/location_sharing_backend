using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using APIUtils;
using location_sharing_backend.Models;

namespace location_sharing_backend.Backends
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
