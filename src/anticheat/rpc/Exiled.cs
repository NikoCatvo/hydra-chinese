using Hazel;

namespace HydraMenu.anticheat.rpc
{
	internal class Exiled : RpcCheck
	{
		public override bool Validate(PlayerControl player, MessageReader reader)
		{
			// Exiled RPC 未使用且在游戏中从不发送
			Anticheat.Flag(player, $"{player.Data.PlayerName} 发送了无效的 Exiled RPC。");
			return false;
		}

		public override RpcCalls GetId()
		{
			return RpcCalls.Exiled;
		}
	}
}