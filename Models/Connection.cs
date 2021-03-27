using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace location_sharing_backend.Models {
	public class Connection : Entity{
		public MongoDBRef User1 { get; set; }
		public MongoDBRef User2 { get; set; }
		public ConnectionType Type { get; set; }
	}

	public enum ConnectionType{
		NONE,
		REQUEST,
		FRIENDS
	}
}
