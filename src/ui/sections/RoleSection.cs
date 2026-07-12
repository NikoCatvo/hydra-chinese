using AmongUs.GameOptions;
using HydraMenu.features;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class RolesSection : ISection
	{
		public RolesSection() : base("身份") { }

		private RoleTypes selectedRole = RoleTypes.Crewmate;

		public override void Render()
		{
			Roles.AllowVentingForCrewmates = GUILayout.Toggle(Roles.AllowVentingForCrewmates, "以船员身份使用通风口");
			Roles.MoveModifier.MoveInVents = GUILayout.Toggle(Roles.MoveModifier.MoveInVents, "在通风口中移动");

			Roles.SkipSabotageChecks.SabotageAsCrewmate = GUILayout.Toggle(Roles.SkipSabotageChecks.SabotageAsCrewmate, "以船员身份破坏");
			Roles.SkipSabotageChecks.SabotageInVents = GUILayout.Toggle(Roles.SkipSabotageChecks.SabotageInVents, "允许内鬼在通风口中破坏");

			Roles.DisableShapeshiftAnimation = GUILayout.Toggle(Roles.DisableShapeshiftAnimation, "禁用变形动画");

			Roles.NoKillChecks = GUILayout.Toggle(Roles.NoKillChecks, "无击杀检查");

			GUILayout.Label($"改变身份为: {selectedRole}");
			GUILayout.BeginHorizontal();
			selectedRole = Controls.HorizontalRoleSlider(selectedRole);

			if(GUILayout.Button("应用身份" + (AmongUsClient.Instance.AmHost ? "" : " (本地)")))
			{
				UpdateRole(selectedRole);
			}

			GUILayout.EndHorizontal();
		}

		public static void UpdateRole(RoleTypes role)
		{
			Hydra.Log.LogInfo($"Updating role to {role}");

			bool isGhost = RoleManager.IsGhostRole(role);

			HudManager.Instance.ReportButton.gameObject.SetActive(!isGhost);

			RoleManager.Instance.SetRole(PlayerControl.LocalPlayer, role);

			if(AmongUsClient.Instance.AmHost)
			{
				Hydra.Log.LogInfo("Since we are host, we can send the SetRole RPC to sync the new role to the server");
				PlayerControl.LocalPlayer.RpcSetRole(role, true);
			}

			Hydra.notifications.Send("更新身份", $"你的身份已更新为 {role}。");
		}
	}
}
