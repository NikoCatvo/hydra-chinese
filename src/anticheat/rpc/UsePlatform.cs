using Hazel;

namespace HydraMenu.anticheat.rpc
{
	internal class UsePlatform : RpcCheck
	{
		public override bool Validate(PlayerControl player, MessageReader reader)
		{
			MapNames map = Utilities.GetCurrentMap();
			if(map != MapNames.Airship)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 在正确地图之外尝试使用平台。");
				return false;
			}

			if(ShipStatus.Instance == null)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 在不存在 ShipStatus 实例时尝试使用平台。");
				return false;
			}

			if(GameManager.Instance.IsHideAndSeek())
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 在躲猫猫模式中尝试使用平台。");
				return false;
			}

			return true;
		}

		public override RpcCalls GetId()
		{
			return RpcCalls.UsePlatform;
		}
	}
}