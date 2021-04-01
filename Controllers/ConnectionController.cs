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
using static location_sharing_backend.IOModels.ConnectionModels;

namespace location_sharing_backend.Controllers {
	[ApiController]
	[Route(Settings.URL_PREFIX + "[controller]")]
	public class ConnectionController : ControllerBase {
		private readonly IDatabaseSettings databaseSettings;
		private readonly ConnectionService connectionService;
		private readonly UserBlockService userBlockService;
		private readonly UserService userService;
		public ConnectionController(IDatabaseSettings _databaseSettings,UserBlockService _userBlockService, ConnectionService _connectionService, UserService _userService) {
			databaseSettings = _databaseSettings;
			userBlockService = _userBlockService;
			connectionService = _connectionService;
			userService = _userService;
		}

		[HttpGet]
		public async Task<ActionResult<GetListOut>> GetCurrentUserConnectionsList([FromQuery] GetListIn getListIn) {
			AuthClaims authClaims = AuthClaims.ParseClaimsPrincipal(User);
			GetListOut getListOut = new GetListOut();
			List<string> userIds = new List<string>();
			if (getListIn.Type == GetListTypeFilter.REQUESTS_SENT ||
				getListIn.Type == GetListTypeFilter.REQUESTS_RECEIVED ||
				getListIn.Type == GetListTypeFilter.FRIENDS) {
				List<Connection> connections = await connectionService.GetList(authClaims.UserId, getListIn.PageOffset, getListIn.PageSize, getListIn.Type);
				foreach (Connection item in connections) {
					if (item.User1.Id.AsString != authClaims.UserId) {
						userIds.Add(item.User1.Id.AsString);
					}
					if (item.User2.Id.AsString != authClaims.UserId) {
						userIds.Add(item.User2.Id.AsString);
					}
				}
			} else if (getListIn.Type == GetListTypeFilter.BLOCKS) {
				List<UserBlock> userBlocks = await userBlockService.GetList(authClaims.UserId, getListIn.PageOffset, getListIn.PageSize, getListIn.Type);
				foreach (UserBlock item in userBlocks) {
					userIds.Add(item.BlockedUser.Id.AsString);
				}
			}
			List<User> users = await userService.GetByIds(userIds);
			foreach (User item in users) {
				getListOut.Users.Add(new GetListOut.UserData() {
					Id = item.Id,
					Username = item.Username,
					ProfilePhotoURL = item.ProfilePhotoURL
				});
			}
			return Ok(getListOut);
		}

		[HttpPost("request")]
		public async Task<IActionResult> RequestConnection(RequestConnectionIn friendRequestData) {
			User receiver = await userService.GetByUsername(friendRequestData.ReceiverUsername);
			if (receiver == null) {
				return BadRequest();
			}

			AuthClaims authClaims = AuthClaims.ParseClaimsPrincipal(User);
			User initiator = await userService.Get(authClaims.UserId);

			if (receiver.Id == initiator.Id) {
				return BadRequest();
			}

			if (userBlockService.Exists(receiver.Id, initiator.Id)) {
				return BadRequest();
			}

			Connection existingConnection = await connectionService.GetByUsers(initiator.Id, receiver.Id);
			if (existingConnection != null) {
				if (existingConnection.Type == ConnectionType.REQUEST && (existingConnection.User1.Id == receiver.Id && existingConnection.User2.Id == initiator.Id)) {
					existingConnection.Type = ConnectionType.FRIENDS;
					connectionService.Update(existingConnection);
					return Ok();
				} else {
					return BadRequest();
				}
			}

			Connection connection = new Connection() {
				User1 = new MongoDBRef(databaseSettings.UsersCollectionName, receiver.Id),
				User2 = new MongoDBRef(databaseSettings.UsersCollectionName, initiator.Id),
				Type = ConnectionType.REQUEST
			};
			connectionService.Create(connection);

			return Ok();
		}

		[HttpPost("update")]
		public async Task<IActionResult> Update(ConnectionUpdateDataIn connectionUpdateDataIn) {
			User otherUser = await userService.Get(connectionUpdateDataIn.OtherUserId);
			if (otherUser == null) {
				return BadRequest();
			}

			AuthClaims authClaims = AuthClaims.ParseClaimsPrincipal(User);
			User currentUser = await userService.Get(authClaims.UserId);

			if (otherUser.Id == currentUser.Id) {
				return BadRequest();
			}

			switch (connectionUpdateDataIn.Action) {
				case ConnectionUpdateDataIn.CAction.ACCEPT: {
						Connection connection = await connectionService.GetByInitiatorAndReceiver(otherUser.Id, currentUser.Id);
						if (connection == null) {
							return BadRequest();
						}
						connection.Type = ConnectionType.FRIENDS;
						connectionService.Update(connection);
					}
					break;
				case ConnectionUpdateDataIn.CAction.DENY: {
						Connection connection = await connectionService.GetByInitiatorAndReceiver(otherUser.Id, currentUser.Id);
						if (connection == null) {
							return BadRequest();
						}
						connectionService.Remove(connection.Id);
					}
					break;
				case ConnectionUpdateDataIn.CAction.BLOCK: {
						Connection connection = await connectionService.GetByUsers(otherUser.Id, currentUser.Id);
						bool wasFriend = false;
						if (connection != null) {
							if (connection.Type == ConnectionType.FRIENDS) {
								wasFriend = true;
							}
							connectionService.Remove(connection.Id);
						}
						UserBlock userBlock = new UserBlock() {
							Blocker = new MongoDBRef(databaseSettings.UsersCollectionName, currentUser.Id),
							BlockedUser = new MongoDBRef(databaseSettings.UsersCollectionName, otherUser.Id),
							WasFriend = wasFriend
						};
						userBlockService.Create(userBlock);
					}
					break;
				case ConnectionUpdateDataIn.CAction.UNBLOCK: {
						UserBlock userBlock = await userBlockService.GetByUsers(currentUser.Id, otherUser.Id);
						if (userBlock == null) {
							return BadRequest();
						}
						if (userBlock.WasFriend) {
							Connection connection = new Connection() {
								User1 = new MongoDBRef(databaseSettings.UsersCollectionName, currentUser.Id),
								User2 = new MongoDBRef(databaseSettings.UsersCollectionName, otherUser.Id),
								Type = ConnectionType.FRIENDS
							};
							connectionService.Create(connection);
						}
						userBlockService.Remove(userBlock.Id);
					}
					break;
			}

			return Ok();
		}
	}
}
