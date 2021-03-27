using location_sharing_backend.Models;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static location_sharing_backend.Services.ConnectionService;

namespace location_sharing_backend.Services {
	public class UserBlockService : ServiceBase<UserBlock> {
        public UserBlockService(IDatabaseSettings settings) : base(settings, settings.UserBlocksCollectionName) {}

        public async Task<List<UserBlock>> GetList(string userId, int? pageOffset, int? pageSize, GetListConnectionTypeFilter? type) {
            if (type == GetListConnectionTypeFilter.BLOCKS) {
                return await collection.Find(x => x.Blocker.Id == userId).Skip(pageOffset).Limit(pageSize).ToListAsync();
            }
            return null;
        }

        public async Task<UserBlock> GetByUsers(string blockerId, string blockedUserId) {
            return await collection.Find(x => x.Blocker.Id == blockerId && x.BlockedUser.Id == blockedUserId).FirstOrDefaultAsync();
        }

        public bool Exists(string user1Id, string user2Id) {
            return collection.CountDocuments(x =>
                (x.Blocker.Id == user1Id && x.BlockedUser.Id == user2Id) ||
                (x.Blocker.Id == user2Id && x.BlockedUser.Id == user1Id)
            ) > 0;
        }
    }
}
