using Hazel;

namespace HydraMenu.anticheat.rpc
{
	internal class SetStartCounter : RpcCheck
	{
		public override bool Validate(PlayerControl player, MessageReader reader)
		{
			reader.ReadPackedInt32();
			sbyte counter = reader.ReadSByte();

			// 非房主玩家发送 SetStartCounter RPC 时，counter 值必须始终为 -1
			// 我不确定为什么非房主玩家甚至需要发送此 RPC，这更像是只有房主应该发送的东西
			if(player.OwnerId != AmongUsClient.Instance.HostId && counter != -1)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 发送了无效值的 SetStartCounter RPC: {counter}。");

				// 恢复无效的开始计数器
				if(AmongUsClient.Instance.AmHost)
				{
					PlayerControl.LocalPlayer.RpcSetStartCounter(-1);
				}

				return false;
			}

			/*
			if(PlayerControl.LocalPlayer != null && LobbyBehaviour.Instance == null)
			{
				// 原版游戏在游戏开始时已忽略 SetStartCounter RPC，所以我们不需要恢复它
				Anticheat.Flag(player, $"{player.Data.PlayerName} 在大厅已取消生成时发送了 SetStartCounter RPC。");
				return false;
			}
			*/

			return true;
		}

		public override RpcCalls GetId()
		{
			return RpcCalls.SetStartCounter;
		}
	}
}