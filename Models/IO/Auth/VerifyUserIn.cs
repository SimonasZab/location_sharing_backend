using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Models.IO.Auth
{
	public class VerifyUserIn
	{
		[Required]
		public string Token { get; set; }
	}
}
