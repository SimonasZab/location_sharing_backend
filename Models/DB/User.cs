using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace location_sharing_backend.Models.DB
{
	public class User : Entity
	{
		public string Username { get; set; }
		public string Password { get; set; }
		public string Email { get; set; }
		public string? ProfilePhotoURL { get; set; }
		public bool Deleted { get; set; }
	}
}