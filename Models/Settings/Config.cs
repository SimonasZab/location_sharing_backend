using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Models.Settings
{
	public class Config
	{
		public string PathBase { get; set; }
		public int AccessTokenValidityInMinutes { get; set; }
		public int RefreshTokenValidityInDays { get; set; }
	}
}
