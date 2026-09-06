using HarmonyLib;

namespace HydraMenu.modules.visuals
{
	internal class ShowGhosts : Module
	{
		public ShowGhosts() : base("显示幽灵")
		{
			base.Enabled = true;
		}

		private static ShowGhosts Instance
		{
			get { return ModuleManager.showGhosts; }
		}

		// PlayerControl::FixedUpdate 在玩家死亡时将 PlayerControl::set_Visible 设为 false，存活时设为 true
		// set_Visible 函数运行 CosmeticsLayer::set_Visible 以隐藏或显示玩家装饰
		// 如果我们想在存活时也显示幽灵，则可以重新实现 PlayerControl::set_Visible 使玩家装饰始终可见
		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Visible), MethodType.Setter)]
		class SetVisible
		{
			static bool Prefix(PlayerControl __instance)
			{
				if(!Instance.Enabled || !__instance.Data.IsDead) return true;

				__instance.cosmetics.Visible = true;
				return false;
			}
		}
	}
}