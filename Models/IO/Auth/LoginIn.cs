using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Models.IO.Auth
{
	public class LoginIn : LoginInBase
	{
		public bool Persist { get; set; }
	}
}
