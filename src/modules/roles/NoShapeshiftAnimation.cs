using HarmonyLib;

namespace HydraMenu.modules.roles
{
	internal class NoShapeshiftAnimation : Module
	{
		public NoShapeshiftAnimation() : base("无变身动画") { }

		private static NoShapeshiftAnimation Instance
		{
			get { return ModuleManager.noShapeshiftAnimation; }
		}

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckShapeshift))]
		class StartShapeshift
		{
			static void Prefix(ref bool shouldAnimate)
			{
				if(!Instance.Enabled) return;

				shouldAnimate = false;
			}
		}

		// PlayerControl::CmdCheckRevertShapeshift 调用我们上面打补丁的 PlayerControl::CmdCheckShapeshift 函数，但该函数被内联了
		// 所以我们也必须给 PlayerControl::CmdCheckRevertShapeshift 打补丁
		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckRevertShapeshift))]
		class RevertShapeshift
		{
			static void Prefix(ref bool shouldAnimate)
			{
				if(!Instance.Enabled) return;

				shouldAnimate = false;
			}
		}
	}
}