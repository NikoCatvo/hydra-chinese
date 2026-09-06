using AmongUs.GameOptions;
using HarmonyLib;
using Il2CppSystem.Collections.Generic;

namespace HydraMenu.modules.host
{
	internal class AssignRoles : Module
	{
		public AssignRoles() : base("分配角色") { }

		private static AssignRoles Instance
		{
			get { return ModuleManager.assignRoles; }
		}

		public RoleTypes AssignedRole { get; set; } = RoleTypes.Viper;

		[HarmonyPatch(typeof(LogicRoleSelectionNormal), nameof(LogicRoleSelectionNormal.AssignRolesFromList))]
		class AlwaysImposter
		{
			// 确保从 Il2CppSystem 导入 List<T>，否则会出大问题！
			static void Prefix(ref List<NetworkedPlayerInfo> players, ref List<RoleTypes> roleList, ref int rolesAssigned)
			{
				if(!Instance.Enabled || !AmongUsClient.Instance.AmHost) return;

				RoleTypes assignedRole = Instance.AssignedRole;
				Hydra.Log.LogInfo($"尝试为自己分配 {assignedRole} 角色");

				// 处理 IL2Cpp 互操作的愚蠢把戏
				Il2CppSystem.Predicate<NetworkedPlayerInfo> predicate = (Il2CppSystem.Predicate<NetworkedPlayerInfo>)(player => player == PlayerControl.LocalPlayer.Data);
				int playerIndex = players.FindIndex(predicate);

				// AssignRolesFromList 函数会被多次调用，每次传入不同的玩家列表
				// 如果我们的 NetworkedPlayerInfo 不在此玩家列表中，则不应立即分配角色
				if(playerIndex == -1)
				{
					Hydra.Log.LogInfo("我们的 NetworkedPlayerInfo 不在此列表中，跳过");
					return;
				}

				Hydra.Log.LogInfo($"在玩家列表索引 {playerIndex} 处找到我们的 NetworkedPlayerInfo，从列表中移除");
				players.RemoveAt(playerIndex);

				Il2CppSystem.Predicate<RoleTypes> predicate2 = (Il2CppSystem.Predicate<RoleTypes>)(roleType => roleType == assignedRole);
				int roleIndex = roleList.FindIndex(predicate2);

				Hydra.Log.LogMessage($"玩家索引为 {roleIndex}");

				// 如果我们想分配给自己的角色存在于 roleList 中，则将其移除
				// 我们不想在打算有三个内鬼时游戏里出现四个内鬼
				if(roleIndex != -1)
				{
					Hydra.Log.LogInfo($"在角色列表索引 {roleIndex} 处找到我们角色的实例，从列表中移除");
					roleList.RemoveAt(roleIndex);
				}

				// 为决定是否播放开场过场动画，游戏会等待 SetRole RPC，检查分配的角色是否非幽灵角色，
				// 然后检查所有玩家是否都已分配角色或已断开连接
				// 问题在于如果我们试图给自己分配幽灵角色，且我们是最后一个被分配角色的玩家
				// 则 PlayerControl::CoSetRole 执行流不会显示开场过场动画
				// 导致整个大厅遇到黑屏
				// 为绕过此问题，我们检查此边缘情况，先给自己分配非房主角色，再将角色设为幽灵角色
				if(RoleManager.IsGhostRole(assignedRole) && players.Count == 0)
				{
					PlayerControl.LocalPlayer.RpcSetRole(RoleManager.IsImpostorRole(assignedRole) ? RoleTypes.Impostor : RoleTypes.Crewmate);
				}

				PlayerControl.LocalPlayer.RpcSetRole(assignedRole);
				rolesAssigned++;

				Hydra.Log.LogInfo($"已为自己分配 {assignedRole} 角色!");
			}
		}

		private void OnGameStart()
		{
			if(AmongUsClient.Instance.AmHost || Utilities.IsAnticheatPresent()) return;
			Hydra.Log.LogMessage($"我们处于房主主导的大厅，可以劫持分配的角色");

			// 如果我们在躲猫猫模式中，可以给所有人分配工程师角色，这样房主就不会分配第二个内鬼
			if(GameManager.Instance.IsHideAndSeek() && RoleManager.IsImpostorRole(AssignedRole))
			{
				foreach(PlayerControl player in PlayerControl.AllPlayerControls)
				{
					if(player == PlayerControl.LocalPlayer) continue;

					player.RpcSetRole(RoleTypes.Engineer, false);
				}
			}

			PlayerControl.LocalPlayer.RpcSetRole(AssignedRole, false);
		}

		protected override void OnEnable()
		{
			EventCoordinator.OnGameStart += OnGameStart;
		}

		protected override void OnDisable()
		{
			EventCoordinator.OnGameStart -= OnGameStart;
		}
	}
}