using location_sharing_backend.Backends;
using location_sharing_backend.Models;
using location_sharing_backend.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace location_sharing_backend.Controllers {
	[ApiController]
	[Route(Settings.URL_PREFIX + "[controller]")]
	public class ConnectionController : ControllerBase {
		private readonly ConnectionService _connectionService;
		private readonly UserBlockService _userBlockService;
		private readonly UserService _userService;
		public ConnectionController(UserBlockService userBlockService, ConnectionService connectionService, UserService userService) {
			_userBlockService = userBlockService;
			_connectionService = connectionService;
			_userService = userService;
		}

		public class GetListIn {
			[Required]
			public int? PageOffset { get; set; }
			[Required]
			public int? PageSize { get; set; }
			[Required]
			public ConnectionService.GetListConnectionTypeFilter? Type { get; set; }
		}

		public class GetListOut {
			public class UserData{
				public string Id { get; set; }
				public string Username { get; set; }
				public string ProfilePhotoURL { get; set; }
			}
			public List<UserData> Users { get; set; } = new List<UserData>();
		}

		[HttpGet]
		public async Task<ActionResult<GetListOut>> GetCurrentUserConnectionsList([FromQuery] GetListIn getListIn) {
			string currentUserId = UserBackend.GetUserIdFromClaims(User);
			GetListOut getListOut = new GetListOut();
			List<string> userIds = new List<string>();
			if (getListIn.Type == ConnectionService.GetListConnectionTypeFilter.REQUESTS_SENT ||
				getListIn.Type == ConnectionService.GetListConnectionTypeFilter.REQUESTS_RECEIVED ||
				getListIn.Type == ConnectionService.GetListConnectionTypeFilter.FRIENDS) {
				List<Connection> connections = await _connectionService.GetList(currentUserId, getListIn.PageOffset, getListIn.PageSize, getListIn.Type);
				foreach (Connection item in connections) {
					if (item.User1.Id.AsString != currentUserId) {
						userIds.Add(item.User1.Id.AsString);
					}
					if (item.User2.Id.AsString != currentUserId) {
						userIds.Add(item.User2.Id.AsString);
					}
				}
			} else if (getListIn.Type == ConnectionService.GetListConnectionTypeFilter.BLOCKS) {
				List<UserBlock> userBlocks = await _userBlockService.GetList(currentUserId, getListIn.PageOffset, getListIn.PageSize, getListIn.Type);
				foreach (UserBlock item in userBlocks) {
					userIds.Add(item.BlockedUser.Id.AsString);
				}
			}
			List<User> users = await _userService.GetByIds(userIds);
			foreach (User item in users) {
				getListOut.Users.Add(new GetListOut.UserData() {
					Id = item.Id,
					Username = item.Username,
					ProfilePhotoURL = item.ProfilePhotoURL
				});
			}
			return Ok(getListOut);
		}
		
		public class FriendRequestData {
			public string ReceiverUsername { get; set; }
		}

		[HttpPost("request")]
		public async Task<IActionResult> RequestConnection(FriendRequestData friendRequestData) {
			User receiver = await _userService.GetByUsername(friendRequestData.ReceiverUsername);
			if (receiver == null) {
				return BadRequest();
			}

			User initiator = await _userService.GetByClaims(User);

			if (receiver.Id == initiator.Id) {
				return BadRequest();
			}

			if (_userBlockService.Exists(receiver.Id, initiator.Id)) {
				return BadRequest();
			}

			Connection existingConnection = await _connectionService.GetByUsers(initiator.Id, receiver.Id);
			if (existingConnection != null) {
				if (existingConnection.Type == ConnectionType.REQUEST && (existingConnection.User1.Id == receiver.Id && existingConnection.User2.Id == initiator.Id)) {
					existingConnection.Type = ConnectionType.FRIENDS;
					_connectionService.Update(existingConnection);
					return Ok();
				} else {
					return BadRequest();
				}
			}

			Connection connection = new Connection() {
				User1 = new MongoDBRef("Users", receiver.Id),
				User2 = new MongoDBRef("Users", initiator.Id),
				Type = ConnectionType.REQUEST
			};
			_connectionService.Create(connection);

			return Ok();
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

		[HttpPost("update")]
		public async Task<IActionResult> Update(ConnectionUpdateDataIn connectionUpdateDataIn) {
			User otherUser = await _userService.Get(connectionUpdateDataIn.OtherUserId);
			if (otherUser == null) {
				return BadRequest();
			}

			User currentUser = await _userService.GetByClaims(User);

			if (otherUser.Id == currentUser.Id) {
				return BadRequest();
			}

			switch (connectionUpdateDataIn.Action) {
				case ConnectionUpdateDataIn.CAction.ACCEPT: {
						Connection connection = await _connectionService.GetByInitiatorAndReceiver(otherUser.Id, currentUser.Id);
						if (connection == null) {
							return BadRequest();
						}
						connection.Type = ConnectionType.FRIENDS;
						_connectionService.Update(connection);
					}
					break;
				case ConnectionUpdateDataIn.CAction.DENY: {
						Connection connection = await _connectionService.GetByInitiatorAndReceiver(otherUser.Id, currentUser.Id);
						if (connection == null) {
							return BadRequest();
						}
						_connectionService.Remove(connection.Id);
					}
					break;
				case ConnectionUpdateDataIn.CAction.BLOCK: {
						Connection connection = await _connectionService.GetByUsers(otherUser.Id, currentUser.Id);
						bool wasFriend = false;
						if (connection != null) {
							if (connection.Type == ConnectionType.FRIENDS) {
								wasFriend = true;
							}
							_connectionService.Remove(connection.Id);
						}
						UserBlock userBlock = new UserBlock() {
							Blocker = new MongoDBRef("Users", currentUser.Id),
							BlockedUser = new MongoDBRef("Users", otherUser.Id),
							WasFriend = wasFriend
						};
						_userBlockService.Create(userBlock);
					}
					break;
				case ConnectionUpdateDataIn.CAction.UNBLOCK: {
						UserBlock userBlock = await _userBlockService.GetByUsers(currentUser.Id, otherUser.Id);
						if (userBlock == null) {
							return BadRequest();
						}
						if (userBlock.WasFriend) {
							Connection connection = new Connection() {
								User1 = new MongoDBRef("Users", currentUser.Id),
								User2 = new MongoDBRef("Users", otherUser.Id),
								Type = ConnectionType.FRIENDS
							};
							_connectionService.Create(connection);
						}
						_userBlockService.Remove(userBlock.Id);
					}
					break;
			}

			return Ok();
		}
	}
}
