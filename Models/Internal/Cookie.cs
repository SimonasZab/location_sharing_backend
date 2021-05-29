using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Models.Internal
{
    public class Cookie
    {
		public string Key { get; set; }
		public string Value { get; set; }
		public CookieOptions Options { get; set; }

		public void AppendToResponse(HttpResponse httpResponse)
		{
			httpResponse.Cookies.Append(Key, Value, Options);
		}
		public void DeleteFromResponse(HttpResponse httpResponse)
		{
			httpResponse.Cookies.Delete(Key, Options);
		}
	}
}
