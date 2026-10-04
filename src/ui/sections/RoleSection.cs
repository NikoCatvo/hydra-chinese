using AmongUs.GameOptions;
using HydraMenu.modules;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class RolesSection : Section
	{
		public RolesSection() : base("角色") { }

		private RoleTypes selectedRole = RoleTypes.Crewmate;

		public override void Render()
		{
			ModuleManager.ventAsCrewmate.Enabled = GUILayout.Toggle(ModuleManager.ventAsCrewmate.Enabled, "船员身份钻管道");
			ModuleManager.moveInVents.Enabled = GUILayout.Toggle(ModuleManager.moveInVents.Enabled, "在管道中移动");

			ModuleManager.unlockSabotageButton.SabotageAsCrewmate = GUILayout.Toggle(ModuleManager.unlockSabotageButton.SabotageAsCrewmate, "船员身份破坏");
			ModuleManager.unlockSabotageButton.SabotageInVents = GUILayout.Toggle(ModuleManager.unlockSabotageButton.SabotageInVents, "允许内鬼在管道中破坏");
			ModuleManager.noSabotageCooldown.Enabled = GUILayout.Toggle(ModuleManager.noSabotageCooldown.Enabled, "无破坏冷却");

			ModuleManager.noShapeshiftAnimation.Enabled = GUILayout.Toggle(ModuleManager.noShapeshiftAnimation.Enabled, "禁用变形动画");
			// Roles.DisablePhantomEndAnimation = GUILayout.Toggle(Roles.DisablePhantomEndAnimation, "Disable Phantom End Animation");

			GUILayout.Space(5);
			GUILayout.Label($"击杀检查绕过：");
			ModuleManager.noKillChecks.Enabled = GUILayout.Toggle(ModuleManager.noKillChecks.Enabled, "启用");
			ModuleManager.noKillChecks.KillOtherImpostors = GUILayout.Toggle(ModuleManager.noKillChecks.KillOtherImpostors, "击杀其他内鬼");
			ModuleManager.noKillChecks.KillAsPhantom = GUILayout.Toggle(ModuleManager.noKillChecks.KillAsPhantom, "隐身时击杀");
			ModuleManager.noKillChecks.NoKillCooldown = GUILayout.Toggle(ModuleManager.noKillChecks.NoKillCooldown, "无击杀冷却（仅房主）");
			ModuleManager.noKillChecks.KillGhosts = GUILayout.Toggle(ModuleManager.noKillChecks.KillGhosts, "击杀幽灵（仅房主）");

			GUILayout.Label($"更改角色为：{Lang.Tr(selectedRole)}");
			GUILayout.BeginHorizontal();
			selectedRole = Controls.HorizontalRoleSlider(selectedRole);

			if(GUILayout.Button("应用角色" + (AmongUsClient.Instance.AmHost ? "" : "（本地）")))
			{
				UpdateRole(selectedRole);
			}

			GUILayout.EndHorizontal();
		}

		public static void UpdateRole(RoleTypes role)
		{
			Hydra.Log.LogInfo($"Updating role to {role}");

			bool isGhost = RoleManager.IsGhostRole(role);

			// When a player turns into the ghost, the PlayerControl::CoSetRole function hides the report button. This function then calls the RoleManager::SetRole function we call here
			// This means when we are changing between normal or ghost roles, the report button will not properly be added/removed, so we have to reimplement it here
			// We also cannot use PlayerControl::CoSetRole directly as it prevents in-game roles being overriden by non-ghosts ones (we could just patch it and disable overriding, however a blackout occurs when the game starts)
			HudManager.Instance.ReportButton.gameObject.SetActive(!isGhost);

			RoleManager.Instance.SetRole(PlayerControl.LocalPlayer, role);

			if(AmongUsClient.Instance.AmHost)
			{
				Hydra.Log.LogInfo("Since we are host, we can send the SetRole RPC to sync the new role to the server");
				PlayerControl.LocalPlayer.RpcSetRole(role, true);
			}

			Hydra.notifications.Send("更新角色", $"你的角色已更新为 {Lang.Tr(role)}。");
		}
	}
}