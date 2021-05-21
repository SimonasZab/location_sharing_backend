using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace location_sharing_backend.Models.Settings
{
	public class DbInfo
	{
		public string DatabaseName { get; set; }
		public DbCollections Collections { get; set; }
	}
}
