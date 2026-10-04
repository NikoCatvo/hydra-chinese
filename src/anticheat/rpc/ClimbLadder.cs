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
				Anticheat.Flag(player, $"{player.Data.PlayerName} 在 ShipStatus 实例不存在时尝试爬梯子。");
				return false;
			}

			MapNames map = Utilities.GetCurrentMap();
			if(map != MapNames.Airship && map != MapNames.Fungle)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 在不支持爬梯的地图上尝试爬梯子。");
				return false;
			}

			// Check if the player vents if their role does not support venting (if they are not engineer or non-ghost imposter)
			// We also want to make sure that the player is not dead to avoid false positives if the player vents as soon as they die
			// (Maybe we can store the time at which a player died and skip this check if an EnterVent RPC was sent within 500ms of dying?)
			if(player.Data.IsDead)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 在死亡状态下尝试爬梯子。");
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