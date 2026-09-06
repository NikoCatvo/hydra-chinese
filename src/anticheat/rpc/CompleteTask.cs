using Hazel;

namespace HydraMenu.anticheat.rpc
{
	internal class CompleteTask : RpcCheck
	{
		public override bool Validate(PlayerControl player, MessageReader reader)
		{
			uint taskIndex = reader.ReadPackedUInt32();

			// 如果没有 ShipStatus 实例（例如游戏未开始或地图已取消生成），则无法完成任务
			// 技术上我们不需要这个来检测大厅中有人完成任务，因为任务 ID 大于任务总数的检查应该能检测到
			if(ShipStatus.Instance == null)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 在不存在有效 ShipStatus 实例时尝试完成任务 {taskIndex}。");
				return false;
			}

			if(RoleManager.IsImpostorRole(player.Data.RoleType))
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 作为内鬼时尝试完成任务 {taskIndex}。");
				return false;
			}

			// 任务 ID 从零开始
			if(taskIndex + 1 > player.Data.Tasks.Count)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 在只有 {player.Data.Tasks.Count} 个任务时尝试完成任务 {taskIndex}。");
				return false;
			}

			return true;
		}

		public override RpcCalls GetId()
		{
			return RpcCalls.CompleteTask;
		}
	}
}