using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Models.DB
{
	public class UserShare : Entity
	{
		public MongoDBRef Sharer { get; set; }
		public MongoDBRef Receiver { get; set; }
		public MongoDBRef SharedObj { get; set; }
	}
}
