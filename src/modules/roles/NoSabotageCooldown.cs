using HarmonyLib;

namespace HydraMenu.modules.roles
{
	internal class NoSabotageCooldown : Module
	{
		public NoSabotageCooldown() : base("无破坏冷却")
		{
			base.Enabled = true;
		}

		private static NoSabotageCooldown Instance
		{
			get { return ModuleManager.noSabotageCooldown; }
		}

		[HarmonyPatch(typeof(InfectedOverlay), nameof(InfectedOverlay.CanUseSabotage), MethodType.Getter)]
		class AllowSabotageButton
		{
			static bool Prefix(ref bool __result)
			{
				if(!Instance.Enabled) return true;

				__result = true;
				return false;
			}
		}

		// SabotageSystemType 有检查以确保破坏不在冷却中
		// 要绕过此限制，我们需要强制调用破坏
		[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.RpcUpdateSystem), [ typeof(SystemTypes), typeof(byte) ])]
		class ForceSabotage
		{
			static bool Prefix(SystemTypes systemType, byte amount)
			{
				if(!Instance.Enabled || systemType != SystemTypes.Sabotage) return true;

				Sabotage.SabotageSystem((SystemTypes)amount);
				return false;
			}
		}
	}
}