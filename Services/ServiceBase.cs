using Api.Models.DB;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Services
{
	public class ServiceBase<T> where T : Entity
	{
		protected readonly IMongoCollection<T> collection;

		public ServiceBase(string collectionName)
		{
			var client = new MongoClient(Assets.Secrets.DatabaseConnectionString);
			var database = client.GetDatabase(Assets.DbInfo.DatabaseName);

			collection = database.GetCollection<T>(collectionName);
		}

		public async Task<T> Get(string id) => await collection.Find(x => x.Id == id).FirstOrDefaultAsync();

		public async Task<List<T>> Get() => await collection.Find(x => true).ToListAsync();

		public async Task<List<T>> GetByIds(List<string> ids) => await collection.Find(x => ids.Contains(x.Id)).ToListAsync();

		public T Create(T item)
		{
			collection.InsertOne(item);
			return item;
		}

		public void Update(string id, T item) => collection.ReplaceOne(x => x.Id == id, item);

		public void Update(T item) => collection.ReplaceOne(x => x.Id == item.Id, item);

		public void Remove(T item) => collection.DeleteOne(x => x.Id == item.Id);

		public void Remove(string id) => collection.DeleteOne(x => x.Id == id);
	}
}
