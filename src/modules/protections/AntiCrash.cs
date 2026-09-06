using HarmonyLib;

namespace HydraMenu.modules.protections
{
	internal class AntiCrash : Module
	{
		// 让 Among Us 大厅崩溃的一种方法是在游戏加载界面发送 ReportDeadBody RPC
		// ReportDeadBody RPC 的处理程序会检查游戏中的所有任务是否已完成
		// 由于尚未给玩家分配任务，这将返回 true 并导致游戏结束
		// 房主通过发送 EndGame 根消息告诉 AU 服务器游戏已结束（该函数被称为 RPC，但它不是 RPC）
		// 理论上这意味着游戏将被强制结束并所有人返回大厅，但实际上并非如此
		// 当大厅房主在加载界面发送 EndGame 消息时，Among Us 匹配服务器会删除大厅，而不是让所有人返回
		public AntiCrash() : base("防崩溃")
		{
			base.Enabled = true;
		}

		private static AntiCrash Instance
		{
			get { return ModuleManager.antiCrash; }
		}

		private bool gameFullyLoaded = false;

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
		class OnCallMeeting
		{
			static bool Prefix(PlayerControl __instance)
			{
				if(!Instance.Enabled || Instance.gameFullyLoaded) return true;

				Hydra.notifications.Send("防护警报", $"{__instance.Data.PlayerName} 试图使用大厅崩溃漏洞!");
				return false;
			}
		}

		private void OnGameStart()
		{
			gameFullyLoaded = false;
		}

		private void OnGameLoad()
		{
			gameFullyLoaded = true;
		}

		protected override void OnEnable()
		{
			EventCoordinator.OnGameStart += OnGameStart;
			EventCoordinator.OnGameLoad += OnGameLoad;
		}

		protected override void OnDisable()
		{
			EventCoordinator.OnGameStart -= OnGameStart;
			EventCoordinator.OnGameLoad -= OnGameLoad;
		}
	}
}