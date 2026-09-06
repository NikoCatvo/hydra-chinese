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
			ModuleManager.ventAsCrewmate.Enabled = GUILayout.Toggle(ModuleManager.ventAsCrewmate.Enabled, "船员可进通风管道");
			ModuleManager.moveInVents.Enabled = GUILayout.Toggle(ModuleManager.moveInVents.Enabled, "可在通风管道移动");

			ModuleManager.unlockSabotageButton.SabotageAsCrewmate = GUILayout.Toggle(ModuleManager.unlockSabotageButton.SabotageAsCrewmate, "船员可破坏");
			ModuleManager.unlockSabotageButton.SabotageInVents = GUILayout.Toggle(ModuleManager.unlockSabotageButton.SabotageInVents, "允许内鬼在通风管道内破坏");
			ModuleManager.noSabotageCooldown.Enabled = GUILayout.Toggle(ModuleManager.noSabotageCooldown.Enabled, "无破坏冷却");

			ModuleManager.noShapeshiftAnimation.Enabled = GUILayout.Toggle(ModuleManager.noShapeshiftAnimation.Enabled, "禁用变身动画");
			// Roles.DisablePhantomEndAnimation = GUILayout.Toggle(Roles.DisablePhantomEndAnimation, "禁用幻影结束动画");

			GUILayout.Space(5);
			GUILayout.Label($"无击杀检查:");
			ModuleManager.noKillChecks.Enabled = GUILayout.Toggle(ModuleManager.noKillChecks.Enabled, "启用");
			ModuleManager.noKillChecks.KillOtherImpostors = GUILayout.Toggle(ModuleManager.noKillChecks.KillOtherImpostors, "可击杀其他内鬼");
			ModuleManager.noKillChecks.KillAsPhantom = GUILayout.Toggle(ModuleManager.noKillChecks.KillAsPhantom, "隐身时可击杀");
			ModuleManager.noKillChecks.NoKillCooldown = GUILayout.Toggle(ModuleManager.noKillChecks.NoKillCooldown, "无击杀冷却 (仅房主)");
			ModuleManager.noKillChecks.KillGhosts = GUILayout.Toggle(ModuleManager.noKillChecks.KillGhosts, "可击杀幽灵 (仅房主)");

			GUILayout.Label($"更改角色为: {selectedRole}");
			GUILayout.BeginHorizontal();
			selectedRole = Controls.HorizontalRoleSlider(selectedRole);

			if(GUILayout.Button("应用角色" + (AmongUsClient.Instance.AmHost ? "" : " (本地)")))
			{
				UpdateRole(selectedRole);
			}

			GUILayout.EndHorizontal();
		}

		public static void UpdateRole(RoleTypes role)
		{
			Hydra.Log.LogInfo($"正在更新角色为 {role}");

			bool isGhost = RoleManager.IsGhostRole(role);

			// 当玩家变成幽灵时，PlayerControl::CoSetRole 函数会隐藏报告按钮。此函数接着调用我们在这里调用的 RoleManager::SetRole 函数
			// 这意味着当我们在普通角色和幽灵角色之间切换时，报告按钮将无法正确添加/移除，所以我们必须在这里重新实现
			// 我们也不能直接使用 PlayerControl::CoSetRole，因为它会阻止非幽灵角色在游戏中被覆盖（我们可以直接打补丁禁用覆盖，但游戏开始时会发生黑屏）
			HudManager.Instance.ReportButton.gameObject.SetActive(!isGhost);

			RoleManager.Instance.SetRole(PlayerControl.LocalPlayer, role);

			if(AmongUsClient.Instance.AmHost)
			{
				Hydra.Log.LogInfo("因为我们是房主，可以发送 SetRole RPC 将新角色同步到服务器");
				PlayerControl.LocalPlayer.RpcSetRole(role, true);
			}

			Hydra.notifications.Send("更新角色", $"你的角色已更新为 {role}。");
		}
	}
}