using location_sharing_backend.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace location_sharing_backend.IOModels {
	public class ConnectionModels {
		public enum GetListTypeFilter {
			REQUESTS_SENT,
			REQUESTS_RECEIVED,
			FRIENDS,
			BLOCKS
		}

		public class GetListIn {
			[Required]
			public int? PageOffset { get; set; }
			[Required]
			public int? PageSize { get; set; }
			[Required]
			public GetListTypeFilter? Type { get; set; }
		}

		public class GetListOut {
			public class UserData {
				public string Id { get; set; }
				public string Username { get; set; }
				public string ProfilePhotoURL { get; set; }
			}
			public List<UserData> Users { get; set; } = new List<UserData>();
		}

		public class RequestConnectionIn {
			[Required]
			public string ReceiverUsername { get; set; }
		}

		public class ConnectionUpdateDataIn {
			public string OtherUserId { get; set; }
			public enum CAction {
				ACCEPT,
				DENY,
				BLOCK,
				UNBLOCK
			}
			public CAction Action { get; set; }
		}
	}
}
