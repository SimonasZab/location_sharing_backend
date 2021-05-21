using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace location_sharing_backend.Models.IO.Auth
{
	public class LoginIn : LoginInBase
	{
		public bool Persist { get; set; }
	}
}
