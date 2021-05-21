using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace location_sharing_backend.Models.IO.Auth
{
	public class RegisterIn : LoginInBase
	{
		[Required]
		[EmailAddress]
		public string Email { get; set; }
	}
}
