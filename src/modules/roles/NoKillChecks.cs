using HarmonyLib;
using HydraMenu.network;

namespace HydraMenu.modules.roles
{
	internal class NoKillChecks : Module
	{
		public NoKillChecks() : base("无击杀检查") { }

		private static NoKillChecks Instance
		{
			get { return ModuleManager.noKillChecks; }
		}

		public bool KillOtherImpostors { get; set; } = false;
		public bool KillAsPhantom { get; set; } = false;
		public bool NoKillCooldown { get; set; } = false;
		public bool KillGhosts { get; set; } = false;

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetKillTimer))]
		class KillTimer
		{
			static void Prefix(PlayerControl __instance, ref float time)
			{
				if(!Instance.Enabled || !Instance.NoKillCooldown || __instance != PlayerControl.LocalPlayer) return;

				time = 0.0f;
			}
		}

		[HarmonyPatch(typeof(ImpostorRole), nameof(ImpostorRole.IsValidTarget))]
		class NoImpKillChecks
		{
			static bool Prefix(NetworkedPlayerInfo target, ref bool __result)
			{
				if(!Instance.Enabled) return true;

				__result = IsValidTarget(target);
				return false;
			}
		}

		[HarmonyPatch(typeof(PhantomRole), nameof(PhantomRole.IsValidTarget))]
		class NoPhantomKillChecks
		{
			static bool Prefix(PhantomRole __instance, NetworkedPlayerInfo target, ref bool __result)
			{
				if(!Instance.Enabled) return true;

				__result = IsValidTarget(target) && (!__instance.isInvisible || Instance.KillAsPhantom);
				return false;
			}
		}

		private static bool IsValidTarget(NetworkedPlayerInfo target)
		{
			return target != null &&
			       target != PlayerControl.LocalPlayer.Data &&
			       !target.Disconnected &&
			       (!target.IsDead || Instance.KillGhosts) &&
			       (!RoleManager.IsImpostorRole(target.RoleType) || Instance.KillOtherImpostors);
		}

		// CheckMurder RPC 处理器有针对击杀幽灵的检查
		// 所以我们需要直接发送 MurderPlayer RPC 来绕过它
		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckMurder))]
		class KillBypass
		{
			static bool Prefix(PlayerControl __instance, PlayerControl target)
			{
				if(!Instance.Enabled || (!AmongUsClient.Instance.AmHost && Utilities.IsAnticheatPresent())) return true;

				__instance.RpcMurderPlayer(target, true);
				return false;
			}
		}

		// 后端 AU 服务器依赖 CheckVanish 和 CheckAppear RPC 来知道玩家是否已消失
		// 此信息随后被服务器的 CheckMurder RPC 处理程序用于判断是否应授权击杀
		// 不知道为什么，但 Innersloth 把 CheckVanish 和 CheckAppear RPC 保护得极严，而 Vanish 和 Appear RPC 几乎零保护
		// 在大厅中作为非幽灵发送 CheckVanish、已幽灵时、冷却中、或 maxDuration 字段与游戏设置的幽灵持续时间不匹配将导致封禁
		// 在大厅中作为非幽灵发送 CheckAppear、或未消失时，也会导致被踢出大厅
		// 但在上述任何条件下发送 Vanish 或 Appear 都会导致 RPC 被中继给其他玩家
		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckVanish))]
		class VanishBypass
		{
			static bool Prefix(PlayerControl __instance)
			{
				if(!Instance.Enabled || !Instance.KillAsPhantom) return true;

				BatchedMessage batch = new BatchedMessage();
				batch.QueueVanish(__instance);
				batch.FinishBatch();
				return false;
			}
		}

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.CmdCheckAppear))]
		class AppearBypass
		{
			static bool Prefix(PlayerControl __instance, bool shouldAnimate)
			{
				if(!Instance.Enabled || !Instance.KillAsPhantom) return true;

				BatchedMessage batch = new BatchedMessage();
				batch.QueueAppear(__instance,  shouldAnimate);
				batch.FinishBatch();
				return false;
			}
		}
	}
}