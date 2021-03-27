using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace location_sharing_backend.Models {
	public class Secrets : ISecrets {
		public DatabaseSettings DatabaseSettings { get; set; }
		public byte[] SALT { get; set; }
	}

	public interface ISecrets {
		public DatabaseSettings DatabaseSettings { get; set; }
		public byte[] SALT { get; set; }
	}
}
