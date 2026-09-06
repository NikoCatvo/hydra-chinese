using System;

namespace HydraMenu.network
{
	internal class Constants
	{
		public enum OwnerIds
		{
			// 严格来说不是真正的网络对象所有者 ID，仅用于 InnerNetClient::StartRpcImmediately 判断应使用 GameData 还是 GameDataTo 根消息
			Everyone = -1,
			Host = -2,
			// 也不是真正的网络对象所有者 ID，仅用于 AmongUsClient::Spawn 自动填充当前客户端的 ID
			Self = -3,
			Server = -4
		}

		public enum SpawnType
		{
			SkeldShipStatus = 0,
			MeetingHud = 1,
			LobbyBehavior = 2,
			PlayerControl = 4,
			MiraShipStatus = 5,
			PolusShipStatus = 6,
			DleksShipStatus = 7,
			AirshipShipStatus = 8,
			GameManager = 10,
			NetworkedPlayerInfo = 11,
			VoteBanSystem = 12,
			FungleShipStatus = 13
		}

		[Flags]
		public enum HudOverrideSystemOperation
		{
			// 所有其他位视为修复
			Sabotage = 128
		}

		[Flags]
		public enum LifeSuppSystemOperation
		{
			Fix = 16,
			RepairConsole = 64,
			Sabotage = 128
		}

		[Flags]
		public enum ReactorSystemOperation
		{
			Fix = 16,
			UnrepairConsole = 32,
			RepairConsole = 64,
			Sabotage = 128
		}
	}
}