using Hazel;

namespace HydraMenu.anticheat.rpc
{
	internal class CompleteTask : RpcCheck
	{
		public override bool Validate(PlayerControl player, MessageReader reader)
		{
			uint taskIndex = reader.ReadPackedUInt32();

			// If there is no instance of ShipStatus (such as if the game has not started yet or the map was despawned), then it is not possible to complete tasks (
			// Technically we don't need this to detect if someone completes a task in the lobby, as the task ID being greater than the total amount of tasks check should detect it
			if(ShipStatus.Instance == null)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 在没有有效 ShipStatus 实例时尝试完成任务 {taskIndex}。");
				return false;
			}

			if(RoleManager.IsImpostorRole(player.Data.RoleType))
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 身为内鬼，却尝试完成任务 {taskIndex}。");
				return false;
			}

			// Task IDs are zero-indexed
			if(taskIndex + 1 > player.Data.Tasks.Count)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 仅有 {player.Data.Tasks.Count} 个任务，却尝试完成任务 {taskIndex}。");
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