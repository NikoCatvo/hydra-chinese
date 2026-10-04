using Hazel;

namespace HydraMenu.anticheat.rpc
{
	internal class SetScanner : RpcCheck
	{
		public override bool Validate(PlayerControl player, MessageReader reader)
		{
			bool scanning = reader.ReadBoolean();
			// byte seqId = reader.ReadByte();

			// The Medbay scan task can only be done if the map itself exists
			// If the game has not started yet (which the vanilla anticheat should already check), or if we're in the lobby to actual game transition, or the map was despawned,
			// then we know that SetScanner RPC was sent illegitimately
			if(ShipStatus.Instance == null && scanning)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 在地图尚未生成时发送了 SetScanner RPC。");
				return false;
			}

			// When a player gets killed, a SetScanner RPC with the scanning value sent to false is sent
			// This applies to Imposters too if they were to somehow die, so we need to account for this false flag
			if(RoleManager.IsImpostorRole(player.Data.RoleType) && scanning)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 身为内鬼，却发送了 SetScanner RPC。");
				return false;
			}

			if(!GameManager.Instance.LogicOptions.GetVisualTasks())
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 在可视任务已关闭时发送了 SetScanner RPC。");
				return false;
			}

			bool hasMedbayScanTask = false;
			foreach(NetworkedPlayerInfo.TaskInfo task in player.Data.Tasks)
			{
				if(task.Id != (byte)TaskTypes.SubmitScan) continue;

				hasMedbayScanTask = true;
				break;
			}

			// SetScanner RPC is sent upon player death, so we have to make sure the scanning value is set to true to avoid false positives
			if(!hasMedbayScanTask && scanning)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 未被分配医疗室扫描任务，却发送了 SetScanner RPC。");
				return false;
			}

			return true;
		}

		public override RpcCalls GetId()
		{
			return RpcCalls.SetScanner;
		}
	}
}