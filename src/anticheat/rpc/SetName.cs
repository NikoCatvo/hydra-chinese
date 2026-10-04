using Hazel;

namespace HydraMenu.anticheat.rpc
{
	internal class SetName : RpcCheck
	{
		// We increase the max name length to 12 instead of 10 as we need to account for the numbers at the end of player names when there are multiple players with the same name
		public readonly int MAX_NAME_LENGTH = 12;

		public override bool Validate(PlayerControl player, MessageReader reader)
		{
			// On modded lobbies, it is common for players to have custom names
			if(!Utilities.IsAnticheatPresent()) return true;

			uint netId = reader.ReadUInt32();
			string requestedName = reader.ReadString();

			if(netId != GetExpectedNetId(player))
			{
				Anticheat.Flag(player, $"针对 {requestedName} 发送的 SetName RPC 包含无效的网络 ID：收到 {netId}，预期 {GetExpectedNetId(player)}。");
				return false;
			}

			if(requestedName.Length > MAX_NAME_LENGTH)
			{
				Anticheat.Flag(player, $"{requestedName} 尝试设置过长的名称（{requestedName.Length} 个字符）。");
				return false;
			}

			if(requestedName.Contains('<'))
			{
				Anticheat.Flag(player, $"{requestedName} 请求设置包含无效字符的名称。");
				return false;
			}

			return true;
		}

		private uint GetExpectedNetId(PlayerControl player)
		{
			// In host authority, the host sends the SetName RPC with the net id of the player's NetworkedPlayerInfo net object
			// In server authority, the server sends the SetName RPC with the net id of the player's PlayerControl net object
			// I don't know why this discrepancy exists, or why the SetName RPC even includes the net id field
			return Utilities.IsAnticheatPresent() ? player.NetId : player.Data.NetId;
		}

		public override RpcCalls GetId()
		{
			return RpcCalls.SetName;
		}

		// On vanilla servers, the host is able to receive the SetName RPC due to how server-authority works
		// On modded servers, the host should never receive the SetName RPC
		public override bool IsHostOnly()
		{
			return !Utilities.IsAnticheatPresent();
		}
	}
}