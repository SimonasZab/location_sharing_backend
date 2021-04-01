using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace location_sharing_backend.Models {
	public class Secrets : ISecrets {
		public DatabaseSettings DatabaseSettings { get; set; }
		public byte[] SALT { get; set; }
		public byte[] JWTSecret { get; set; }
		public string AccessTokenCookieName { get; set; }
		public string RefreshTokenCookieName { get; set; }
	}

	public interface ISecrets {
		public DatabaseSettings DatabaseSettings { get; set; }
		public byte[] SALT { get; set; }
		public byte[] JWTSecret { get; set; }
		public string AccessTokenCookieName { get; set; }
		public string RefreshTokenCookieName { get; set; }
	}
}