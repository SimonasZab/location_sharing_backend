using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Models.IO.Auth
{
	public class RefreshOut
	{
		public DateTime? AccessTokenExpires { get; set; }

		public RefreshOut(DateTime? accessTokenExpires)
		{
			AccessTokenExpires = accessTokenExpires;
		}
	}
}
