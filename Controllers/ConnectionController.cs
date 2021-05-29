using Api.Models.DB;
using Api.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;
using Api.Models.IO.Connection;
using Microsoft.AspNetCore.Authorization;
using Api.Models.Internal;

namespace Api.Controllers
{
	[ApiController]
	[Route("[controller]")]
	public class ConnectionController : ControllerBase
	{
		private readonly ConnectionService connectionService;
		private readonly UserBlockService userBlockService;
		private readonly UserService userService;

		public ConnectionController(UserBlockService _userBlockService, ConnectionService _connectionService, UserService _userService)
		{
			userBlockService = _userBlockService;
			connectionService = _connectionService;
			userService = _userService;
		}

		[Authorize]
		[HttpGet]
		public async Task<ActionResult<GetListOut>> GetCurrentUserConnectionsList([FromQuery] GetListIn getListIn)
		{
			AuthClaims authClaims = AuthClaims.ParseClaimsPrincipal(User);
			List<string> userIds = new List<string>();
			if (getListIn.Type == GetListTypeFilter.REQUESTS_SENT ||
				getListIn.Type == GetListTypeFilter.REQUESTS_RECEIVED ||
				getListIn.Type == GetListTypeFilter.FRIENDS)
			{
				List<Connection> connections = await connectionService.GetList(authClaims.UserId, getListIn.PageOffset, getListIn.PageSize, getListIn.Type);
				foreach (var item in connections)
				{
					if (item.User1.Id.AsString != authClaims.UserId)
					{
						userIds.Add(item.User1.Id.AsString);
					}
					if (item.User2.Id.AsString != authClaims.UserId)
					{
						userIds.Add(item.User2.Id.AsString);
					}
				}
			}
			else if (getListIn.Type == GetListTypeFilter.BLOCKS)
			{
				List<UserBlock> userBlocks = await userBlockService.GetList(authClaims.UserId, getListIn.PageOffset, getListIn.PageSize, getListIn.Type);
				foreach (var item in userBlocks)
				{
					userIds.Add(item.BlockedUser.Id.AsString);
				}
			}
			GetListOut getListOut = new GetListOut();
			List<User> users = await userService.GetByIds(userIds);
			foreach (var item in users)
			{
				getListOut.Users.Add(new GetListOut.UserData()
				{
					Id = item.Id,
					Username = item.Username,
					ProfilePhotoURL = item.ProfilePhotoURL
				});
			}
			return Ok(getListOut);
		}

		[Authorize]
		[HttpPost("request")]
		public async Task<IActionResult> RequestConnection(RequestConnectionIn friendRequestData)
		{
			User receiver = await userService.GetByUsername(friendRequestData.ReceiverUsername);
			if (receiver == null)
			{
				return BadRequest();
			}

			AuthClaims authClaims = AuthClaims.ParseClaimsPrincipal(User);
			User initiator = await userService.Get(authClaims.UserId);

			if (receiver.Id == initiator.Id)
			{
				return BadRequest();
			}

			if (userBlockService.Exists(receiver.Id, initiator.Id))
			{
				return BadRequest();
			}

			Connection existingConnection = await connectionService.GetByUsers(initiator.Id, receiver.Id);
			if (existingConnection != null)
			{
				if (existingConnection.Type == ConnectionType.REQUEST && (existingConnection.User1.Id == receiver.Id && existingConnection.User2.Id == initiator.Id))
				{
					existingConnection.Type = ConnectionType.FRIENDS;
					connectionService.Update(existingConnection);
					return Ok();
				}
				else
				{
					return BadRequest();
				}
			}

			Connection connection = new Connection()
			{
				User1 = new MongoDBRef(Assets.DbInfo.Collections.Users, initiator.Id),
				User2 = new MongoDBRef(Assets.DbInfo.Collections.Users, receiver.Id),
				Type = ConnectionType.REQUEST
			};
			connectionService.Create(connection);

			return Ok();
		}

		[Authorize]
		[HttpPost("update")]
		public async Task<IActionResult> Update(ConnectionUpdateDataIn connectionUpdateDataIn)
		{
			User otherUser = await userService.Get(connectionUpdateDataIn.OtherUserId);
			if (otherUser == null)
			{
				return BadRequest();
			}

			AuthClaims authClaims = AuthClaims.ParseClaimsPrincipal(User);
			User currentUser = await userService.Get(authClaims.UserId);

			if (otherUser.Id == currentUser.Id)
			{
				return BadRequest();
			}

			switch (connectionUpdateDataIn.Action)
			{
				case ConnectionUpdateDataInAction.ACCEPT:
					{
						Connection connection = await connectionService.GetByInitiatorAndReceiver(otherUser.Id, currentUser.Id);
						if (connection == null)
						{
							return BadRequest();
						}
						connection.Type = ConnectionType.FRIENDS;
						connectionService.Update(connection);
					}
					break;
				case ConnectionUpdateDataInAction.DENY:
					{
						Connection connection = await connectionService.GetByUsers(otherUser.Id, currentUser.Id);
						if (connection == null)
						{
							return BadRequest();
						}
						connectionService.Remove(connection.Id);
					}
					break;
				case ConnectionUpdateDataInAction.BLOCK:
					{
						Connection connection = await connectionService.GetByUsers(otherUser.Id, currentUser.Id);
						bool wasFriend = false;
						if (connection != null)
						{
							if (connection.Type == ConnectionType.FRIENDS)
							{
								wasFriend = true;
							}
							connectionService.Remove(connection.Id);
						}
						UserBlock userBlock = new UserBlock()
						{
							Blocker = new MongoDBRef(Assets.DbInfo.Collections.Users, currentUser.Id),
							BlockedUser = new MongoDBRef(Assets.DbInfo.Collections.Users, otherUser.Id),
							WasFriend = wasFriend
						};
						userBlockService.Create(userBlock);
					}
					break;
				case ConnectionUpdateDataInAction.UNBLOCK:
					{
						UserBlock userBlock = await userBlockService.GetByUsers(currentUser.Id, otherUser.Id);
						if (userBlock == null)
						{
							return BadRequest();
						}
						if (userBlock.WasFriend)
						{
							Connection connection = new Connection()
							{
								User1 = new MongoDBRef(Assets.DbInfo.Collections.Users, currentUser.Id),
								User2 = new MongoDBRef(Assets.DbInfo.Collections.Users, otherUser.Id),
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
