using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Models.Settings
{
	public class DbCollections
	{
		public string Users { get; set; }
		public string Connections { get; set; }
		public string UserBlocks { get; set; }
		public string Locations { get; set; }
		public string UserShares { get; set; }
		public string UserVerifications { get; set; }
	}
}
