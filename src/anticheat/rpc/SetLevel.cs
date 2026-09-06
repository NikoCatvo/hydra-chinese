using Hazel;

namespace HydraMenu.anticheat.rpc
{
	internal class SetLevel : RpcCheck
	{
		public readonly uint MAX_PLAYER_LEVEL = 10000;

		// 我们不应阻止 SetLevel RPC
		public override bool Validate(PlayerControl player, MessageReader reader)
		{
			uint level = reader.ReadPackedUInt32();

			// 原版 Among Us 反作弊会在发送等级大于 100k 的 SetLevel RPC 时封禁玩家
			// 这相当宽松，我们只检查请求的玩家等级是否大于 10k
			if(level > MAX_PLAYER_LEVEL)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 发送了等级过高的 SetLevel RPC ({level})。");
				player.SetLevel(MAX_PLAYER_LEVEL);
				return false;
			}

			// SetLevel RPC 应仅在玩家在大厅加入游戏时发送
			if(ShipStatus.Instance)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 在游戏已开始时发送了 SetLevel RPC。");
				return false;
			}

			return true;
		}

		public override RpcCalls GetId()
		{
			return RpcCalls.SetLevel;
		}
	}
}