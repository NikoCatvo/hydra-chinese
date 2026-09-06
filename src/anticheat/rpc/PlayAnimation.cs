using Hazel;

namespace HydraMenu.anticheat.rpc
{
	internal class PlayAnimation : RpcCheck
	{
		public override bool Validate(PlayerControl player, MessageReader reader)
		{
			TaskTypes animation = (TaskTypes)reader.ReadByte();

			if(LobbyBehaviour.Instance)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 在大厅内发送了任务 {animation} 的 PlayAnimation RPC。");
				return false;
			}

			if(RoleManager.IsImpostorRole(player.Data.RoleType))
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 作为内鬼时发送了任务 {animation} 的 PlayAnimation RPC。");
				return false;
			}

			if(!GameManager.Instance.LogicOptions.GetVisualTasks())
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 在可视化任务关闭时发送了任务 {animation} 的 PlayAnimation RPC。");
				return false;
			}

			return true;
		}

		public override RpcCalls GetId()
		{
			return RpcCalls.PlayAnimation;
		}
	}
}