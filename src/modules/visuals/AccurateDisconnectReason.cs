using HarmonyLib;

namespace HydraMenu.modules.visuals
{
	internal class AccurateDisconnectReason : Module
	{
		public AccurateDisconnectReason() : base("AccurateDisconnectReason")
		{
			base.Enabled = true;
		}

		private static AccurateDisconnectReason Instance
		{
			get { return ModuleManager.accurateDisconnectReason; }
		}

		// The GameData::ShowNotification function by default only handles disconnect reasons of ExitGame, Kicked, or Banned
		// Any other disconnection reasons automatically default to the error disconnection message
		[HarmonyPatch(typeof(GameData), nameof(GameData.ShowNotification))]
		class ShowDisconnectNotification
		{
			static bool Prefix(string playerName, DisconnectReasons reason)
			{
				if(!Instance.Enabled) return true;

				switch(reason) {
					// GameData::ShowNotification already handles these disconnect messages
					case DisconnectReasons.ExitGame:
					case DisconnectReasons.Kicked:
					case DisconnectReasons.Banned:
					case DisconnectReasons.Error:
						return true;

					case DisconnectReasons.Hacking:
						HudManager.Instance.Notifier.AddDisconnectMessage($"{playerName} 因作弊被 Among Us 反作弊封禁。");
						return false;

					case DisconnectReasons.DuplicateConnectionDetected:
						HudManager.Instance.Notifier.AddDisconnectMessage($"{playerName} 因重复登录被踢出。");
						return false;

					// This disconnect reason happens when a player does not send the ClientReady message after the game starts in time
					case DisconnectReasons.ClientTimeout:
						HudManager.Instance.Notifier.AddDisconnectMessage($"{playerName} 因连接超时被踢出。");
						return false;

					default:
						HudManager.Instance.Notifier.AddDisconnectMessage($"{playerName} 已断开连接，原因：{reason}。");
						return false;
				}
			}
		}
	}
}