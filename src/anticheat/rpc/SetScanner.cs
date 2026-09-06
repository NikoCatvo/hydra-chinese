using Hazel;

namespace HydraMenu.anticheat.rpc
{
	internal class SetScanner : RpcCheck
	{
		public override bool Validate(PlayerControl player, MessageReader reader)
		{
			bool scanning = reader.ReadBoolean();
			// byte seqId = reader.ReadByte();

			// 医疗扫描任务只有在地图存在时才能进行
			// 如果游戏尚未开始（原版反作弊本应已检查），或我们处于大厅到实际游戏的过渡期，或地图已取消生成，
			// 则我们知道 SetScanner RPC 是非法发送的
			if(ShipStatus.Instance == null && scanning)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 在地图尚未生成时发送了 SetScanner RPC。");
				return false;
			}

			// 玩家被击杀时，会发送 scanning 值为 false 的 SetScanner RPC
			// 如果内鬼以某种方式死亡也适用，所以我们需要考虑此误报
			if(RoleManager.IsImpostorRole(player.Data.RoleType) && scanning)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 作为内鬼时发送了 SetScanner RPC。");
				return false;
			}

			if(!GameManager.Instance.LogicOptions.GetVisualTasks())
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 在可视化任务禁用时发送了 SetScanner RPC。");
				return false;
			}

			bool hasMedbayScanTask = false;
			foreach(NetworkedPlayerInfo.TaskInfo task in player.Data.Tasks)
			{
				if(task.Id != (byte)TaskTypes.SubmitScan) continue;

				hasMedbayScanTask = true;
				break;
			}

			// 玩家死亡时会发送 SetScanner RPC，所以我们必须确保 scanning 值设为 true 以避免误报
			if(!hasMedbayScanTask && scanning)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 在未分配医疗扫描任务时发送了 SetScanner RPC。");
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