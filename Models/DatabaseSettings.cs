using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace location_sharing_backend.Models {
    public class DatabaseSettings : IDatabaseSettings {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
        public string UsersCollectionName { get; set; }
        public string ConnectionsCollectionName { get; set; }
        public string UserBlocksCollectionName { get; set; }
        public string LocationsCollectionName { get; set; }
        public string UserSharesCollectionName { get; set; }
	}

    public interface IDatabaseSettings {
        string ConnectionString { get; set; }
        string DatabaseName { get; set; }
        string UsersCollectionName { get; set; }
        public string ConnectionsCollectionName { get; set; }
        public string UserBlocksCollectionName { get; set; }
        public string LocationsCollectionName { get; set; }
        public string UserSharesCollectionName { get; set; }
    }
}