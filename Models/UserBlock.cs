using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace location_sharing_backend.Models {
	public class UserBlock : Entity {
		public MongoDBRef Blocker { get; set; }
		public MongoDBRef BlockedUser { get; set; }
		public bool WasFriend { get; set; }
	}
}