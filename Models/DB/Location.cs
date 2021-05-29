using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Models.DB
{
	public class Location : Entity
	{
		public float Latitude { get; set; }
		public float Longitude { get; set; }
		public MongoDBRef LinkedObj { get; set; }
	}
}
