using Hazel;

namespace HydraMenu.anticheat.rpc
{
	internal class SetName : RpcCheck
	{
		// 我们将最大名称长度增加到 12 而不是 10，因为需要考虑到当有多个同名玩家时，玩家名称末尾的数字
		public readonly int MAX_NAME_LENGTH = 12;

		public override bool Validate(PlayerControl player, MessageReader reader)
		{
			// 在模组大厅中，玩家拥有自定义名称很常见
			if(!Utilities.IsAnticheatPresent()) return true;

			uint netId = reader.ReadUInt32();
			string requestedName = reader.ReadString();

			if(netId != GetExpectedNetId(player))
			{
				Anticheat.Flag(player, $"为 {requestedName} 发送的 SetName RPC 包含无效网络 ID，收到 {netId}，预期 {GetExpectedNetId(player)}。");
				return false;
			}

			if(requestedName.Length > MAX_NAME_LENGTH)
			{
				Anticheat.Flag(player, $"{requestedName} 尝试将名称设置得过长 ({requestedName.Length})。");
				return false;
			}

			if(requestedName.Contains('<'))
			{
				Anticheat.Flag(player, $"{requestedName} 请求了包含无效字符的名称。");
				return false;
			}

			return true;
		}

		private uint GetExpectedNetId(PlayerControl player)
		{
			// 在房主权威模式下，房主发送 SetName RPC 时使用玩家的 NetworkedPlayerInfo 网络对象的 net id
			// 在服务器权威模式下，服务器发送 SetName RPC 时使用玩家的 PlayerControl 网络对象的 net id
			// 我不知道为什么存在这个差异，也不明白 SetName RPC 为什么要包含 net id 字段
			return Utilities.IsAnticheatPresent() ? player.NetId : player.Data.NetId;
		}

		public override RpcCalls GetId()
		{
			return RpcCalls.SetName;
		}

// 在原版服务器上，由于服务器权威的工作方式，房主能够接收 SetName RPC
			// 在模组服务器上，房主永远不应接收 SetName RPC
		public override bool IsHostOnly()
		{
			return !Utilities.IsAnticheatPresent();
		}
	}
}