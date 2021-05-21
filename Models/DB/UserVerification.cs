using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace location_sharing_backend.Models.DB
{
	public class UserVerification : Entity
	{
		public MongoDBRef User { get; set; }
		public string Token { get; set; }
	}
}
