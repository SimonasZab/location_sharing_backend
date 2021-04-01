using APIUtils;
using location_sharing_backend.Backends;
using location_sharing_backend.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace location_sharing_backend.Services {
	public class UserService : ServiceBase<User>{
        public UserService(IDatabaseSettings settings) : base(settings, settings.UsersCollectionName) {}

        public async Task<bool> UserExists(string username, string email) {
            long count = await collection.CountDocumentsAsync(x => x.Username == username || x.Email == email);
            return count > 0;
        }

        public async Task<User> GetByUsername(string username) {
            return await (await collection.FindAsync(x => x.Username == username)).FirstOrDefaultAsync();
        }
    }
}
