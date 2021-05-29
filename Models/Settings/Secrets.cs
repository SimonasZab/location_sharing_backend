using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Models.Settings
{
	public class Secrets
	{
		public string DatabaseConnectionString { get; set; }
		public byte[] Salt { get; set; }
		public byte[] JWTSecret { get; set; }
		public string AccessTokenCookieName { get; set; }
		public string RefreshTokenCookieName { get; set; }
		public SmtpAuth SmtpAuth { get; set; }
		public string ServerUrl { get; set; }
	}
}