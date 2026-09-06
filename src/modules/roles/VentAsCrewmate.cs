using HarmonyLib;
using UnityEngine;

namespace HydraMenu.modules.roles
{
	internal class VentAsCrewmate : Module
	{
		public VentAsCrewmate() : base("船员进通风管道")
		{
			base.Enabled = true;
		}

		private static VentAsCrewmate Instance
		{
			get { return ModuleManager.ventAsCrewmate; }
		}

		// 类似于船员能使用破坏按钮，通风管道按钮也有检查以确保当前玩家确实能进通风管道，所以我们必须重写 Vent::CanUse 函数
		// 普通函数也有检查以确保通风管道未被清理，但这不重要，所以我们不重写那些检查
		[HarmonyPatch(typeof(Vent), nameof(Vent.CanUse))]
		class SkipVentChecks
		{
			static bool Prefix(Vent __instance, NetworkedPlayerInfo pc, ref bool canUse, ref bool couldUse, ref float __result)
			{
				if(!Instance.Enabled) return true;

				PlayerControl player = pc.Object;
				if(pc.IsDead) return true;

				couldUse = true;
				__result = Vector2.Distance(player.Collider.bounds.center, __instance.transform.position);

				bool isObstructed = PhysicsHelpers.AnythingBetween(player.Collider, player.Collider.bounds.center, __instance.transform.position, Constants.ShipOnlyMask, false);
				if(__result <= __instance.UsableDistance && !isObstructed) canUse = true;

				return false;
			}
		}

		protected override void OnDisable()
		{
			if(PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null || RoleManager.IsImpostorRole(PlayerControl.LocalPlayer.Data.RoleType)) return;

			HudManager.Instance.ImpostorVentButton.gameObject.SetActive(false);
			if(Vent.currentVent != null)
			{
				Vent.currentVent.SetButtons(false);
				PlayerControl.LocalPlayer.MyPhysics.RpcExitVent(Vent.currentVent.Id);
			}
		}
	}
}