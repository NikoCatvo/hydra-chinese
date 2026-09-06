using Hazel;

namespace HydraMenu.anticheat.rpc
{
	internal class ReportDeadBody : RpcCheck
	{
		public override bool Validate(PlayerControl player, MessageReader reader)
		{
			if(GameManager.Instance.IsHideAndSeek())
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 在躲猫猫模式中尝试召开会议");
				return false;
			}

			return true;
		}

		public override RpcCalls GetId()
		{
			return RpcCalls.ReportDeadBody;
		}
	}
}