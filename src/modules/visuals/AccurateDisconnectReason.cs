using HarmonyLib;

namespace HydraMenu.modules.visuals
{
	internal class AccurateDisconnectReason : Module
	{
		public AccurateDisconnectReason() : base("准确断开原因")
		{
			base.Enabled = true;
		}

		private static AccurateDisconnectReason Instance
		{
			get { return ModuleManager.accurateDisconnectReason; }
		}

		// GameData::ShowNotification 函数默认仅处理 ExitGame、Kicked 或 Banned 的断开原因
		// 任何其他断开原因自动默认为错误断开消息
		[HarmonyPatch(typeof(GameData), nameof(GameData.ShowNotification))]
		class ShowDisconnectNotification
		{
			static bool Prefix(string playerName, DisconnectReasons reason)
			{
				if(!Instance.Enabled) return true;

				switch(reason) {
					// GameData::ShowNotification 已处理这些断开消息
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

					// 此断开原因发生在玩家在游戏开始后未能及时发送 ClientReady 消息
					case DisconnectReasons.ClientTimeout:
						HudManager.Instance.Notifier.AddDisconnectMessage($"{playerName} 因超时被踢出。");
						return false;

					default:
						HudManager.Instance.Notifier.AddDisconnectMessage($"{playerName} 因 {reason} 断开连接。");
						return false;
				}
			}
		}
	}
}