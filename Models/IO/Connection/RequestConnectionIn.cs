using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace location_sharing_backend.Models.IO.Connection
{
	public class RequestConnectionIn
	{
		[Required]
		public string ReceiverUsername { get; set; }
	}
}
