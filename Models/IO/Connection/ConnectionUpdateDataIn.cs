using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Models.IO.Connection
{
	public class ConnectionUpdateDataIn
	{
		public string OtherUserId { get; set; }
		public ConnectionUpdateDataInAction Action { get; set; }
	}
}
