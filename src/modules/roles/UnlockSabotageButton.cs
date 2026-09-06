using HarmonyLib;

namespace HydraMenu.modules.roles
{
	internal class UnlockSabotageButton : Module
	{
		public UnlockSabotageButton() : base("解锁破坏按钮")
		{
			base.Enabled = true;
		}

		private static UnlockSabotageButton Instance
		{
			get { return ModuleManager.unlockSabotageButton; }
		}

		public bool SabotageAsCrewmate { get; set; } = false;
		public bool SabotageInVents { get; set; } = false;

		// 点击破坏按钮有检查以确保当前玩家确实是内鬼、不在通风管道中，且当前游戏模式支持破坏
		// 这意味着仅将 GameObject 的破坏按钮状态设为激活不会让船员单独破坏，我们需要重写 DoClick 函数以移除这些检查
		[HarmonyPatch(typeof(SabotageButton), nameof(SabotageButton.DoClick))]
		class SkipSabotageChecks
		{
			static bool Prefix()
			{
				PlayerControl player = PlayerControl.LocalPlayer;

				// 我们必须将此限制为内鬼，因为船员退出通风管道按钮会与内鬼破坏按钮位于同一位置
				if(!Instance.SabotageInVents && player.inVent && !RoleManager.IsImpostorRole(player.Data.RoleType)) return true;

				HudManager.Instance.ToggleMapVisible(new MapOptions { Mode = MapOptions.Modes.Sabotage });
				return false;
			}
		}
	}
}