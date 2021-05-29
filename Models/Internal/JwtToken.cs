using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Models.Internal
{
    public class JwtToken
    {
        public string Value { get; set; }
        public DateTime? ExpirationDate { get; set; }
    }
}
