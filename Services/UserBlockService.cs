using Api.Models.DB;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Models.IO.Connection;

namespace Api.Services
{
	public class UserBlockService : ServiceBase<UserBlock>
	{
		public UserBlockService() : base(Assets.DbInfo.Collections.UserBlocks) { }

		public async Task<List<UserBlock>> GetList(string userId, int? pageOffset, int? pageSize, GetListTypeFilter? type)
		{
			if (type == GetListTypeFilter.BLOCKS)
			{
				return await collection.Find(x => x.Blocker.Id == userId).Skip(pageOffset).Limit(pageSize).ToListAsync();
			}
			return null;
		}

		public async Task<UserBlock> GetByUsers(string blockerId, string blockedUserId)
		{
			return await collection.Find(x => x.Blocker.Id == blockerId && x.BlockedUser.Id == blockedUserId).FirstOrDefaultAsync();
		}

		public bool Exists(string user1Id, string user2Id)
		{
			return collection.CountDocuments(x =>
				(x.Blocker.Id == user1Id && x.BlockedUser.Id == user2Id) ||
				(x.Blocker.Id == user2Id && x.BlockedUser.Id == user1Id)
			) > 0;
		}
	}
}
