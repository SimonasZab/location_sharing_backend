using Api.Models.DB;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Api.Models.IO.Connection;

namespace Api.Services
{
	public class ConnectionService : ServiceBase<Connection>
	{
		public ConnectionService() : base(Assets.DbInfo.Collections.Connections) { }

		public async Task<List<Connection>> GetList(string userId, int? pageOffset, int? pageSize, GetListTypeFilter? type)
		{
			if (type == GetListTypeFilter.REQUESTS_SENT)
			{
				return await collection.Find(x => x.User1.Id == userId && x.Type == ConnectionType.REQUEST).Skip(pageOffset).Limit(pageSize).ToListAsync();
			}
			else if (type == GetListTypeFilter.REQUESTS_RECEIVED)
			{
				return await collection.Find(x => x.User2.Id == userId && x.Type == ConnectionType.REQUEST).Skip(pageOffset).Limit(pageSize).ToListAsync();
			}
			else if (type == GetListTypeFilter.FRIENDS)
			{
				return await collection.Find(x => (x.User1.Id == userId || x.User2.Id == userId) && x.Type == ConnectionType.FRIENDS).Skip(pageOffset).Limit(pageSize).ToListAsync();
			}
			return null;
		}

		public async Task<Connection> GetByInitiatorAndReceiver(string initiatorId, string receiverId)
		{
			return await collection.Find(x => x.User1.Id == initiatorId && x.User2.Id == receiverId).FirstOrDefaultAsync();
		}

		public async Task<Connection> GetByUsers(string user1Id, string user2Id)
		{
			return await collection.Find(x =>
				(x.User1.Id == user1Id && x.User2.Id == user2Id) ||
				(x.User1.Id == user2Id && x.User2.Id == user1Id)
			).FirstOrDefaultAsync();
		}

		public bool Exists(User user1, User user2)
		{
			return collection.CountDocuments(x =>
				(x.User1.Id == user1.Id && x.User2.Id == user2.Id) ||
				(x.User1.Id == user2.Id && x.User2.Id == user1.Id)
			) > 0;
		}
	}
}
