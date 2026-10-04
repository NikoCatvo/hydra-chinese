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
				Anticheat.Flag(player, $"{player.Data.PlayerName} 在不支持移动平台的地图上尝试使用移动平台。");
				return false;
			}

			if(ShipStatus.Instance == null)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 在 ShipStatus 实例不存在时尝试使用移动平台。");
				return false;
			}

			if(GameManager.Instance.IsHideAndSeek())
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 在躲猫猫模式中尝试使用移动平台。");
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