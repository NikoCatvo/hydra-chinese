using HarmonyLib;

namespace HydraMenu.modules.visuals
{
	internal class NoSeekerAnimation : Module
	{
		public NoSeekerAnimation() : base("无搜寻者动画") { }

		private static NoSeekerAnimation Instance
		{
			get { return ModuleManager.noSeekerAnimation; }
		}

		[HarmonyPatch(typeof(LogicOptionsHnS), nameof(LogicOptionsHnS.GetCrewmateLeadTime))]
		class NoSeekerAnimationPatch
		{
			static bool Prefix(ref int __result)
			{
				if(!Instance.Enabled) return true;

				__result = 0;
				return false;
			}
		}
	}
}