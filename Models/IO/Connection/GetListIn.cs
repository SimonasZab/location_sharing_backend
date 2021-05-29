using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Models.IO.Connection
{
	public class GetListIn
	{
		[Required]
		public GetListTypeFilter? Type { get; set; }
		public int? PageOffset { get; set; }
		public int? PageSize { get; set; }
	}
}
