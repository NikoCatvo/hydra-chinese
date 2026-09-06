using Hazel;
using System;

namespace HydraMenu.anticheat.rpc
{
	internal class ClimbLadder : RpcCheck
	{
		public override bool Validate(PlayerControl player, MessageReader reader)
		{
			if(ShipStatus.Instance == null)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 在不存在 ShipStatus 实例时尝试爬梯子。");
				return false;
			}

			MapNames map = Utilities.GetCurrentMap();
			if(map != MapNames.Airship && map != MapNames.Fungle)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 在正确地图之外尝试爬梯子。");
				return false;
			}

			// 检查玩家是否在其角色不支持通风管道时通风（如果他们不是工程师或非幽灵内鬼）
			// 我们还想确保玩家未死亡，以避免玩家刚死就通风管道时的误报
			// （也许我们可以存储玩家死亡时间，如果 EnterVent RPC 在死亡 500ms 内发送则跳过此检查？）
			if(player.Data.IsDead)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 死亡时尝试爬梯子。");
				return false;
			}

			return true;
		}

		public override RpcCalls GetId()
		{
			return RpcCalls.ClimbLadder;
		}

		public override Type GetExpectedNetObject()
		{
			return typeof(PlayerPhysics);
		}
	}
}