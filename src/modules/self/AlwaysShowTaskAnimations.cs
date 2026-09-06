using HarmonyLib;
using HydraMenu.network;

namespace HydraMenu.modules.self
{
	internal class AlwaysShowTaskAnimations : Module
	{
		// 当调用 PlayerControl::RpcPlayAnimation 或 PlayerControl::RpcSetScanner 时，它们会在发送 RPC 前检查可视化任务是否开启
		// 如果我们想在可视化任务关闭时也能发送这些 RPC，则需要重新实现这些函数
		// 我们本可以只打补丁 LogicOptionsNormal::GetVisualTasks 和 LogicOptionsHnS::GetVisualTasks，但后者被 Il2Cpp 编译器优化掉了，所以补丁实际上不会生效
		// 这意味着这只会在普通游戏中显示任务动画，而不适用于躲猫猫模式
		public AlwaysShowTaskAnimations() : base("始终显示任务动画") { }

		private static AlwaysShowTaskAnimations Instance
		{
			get { return ModuleManager.alwaysShowTaskAnimations; }
		}

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcSetScanner))]
		class SetScanner
		{
			static bool Prefix(PlayerControl __instance, bool value)
			{
				if(!Instance.Enabled || __instance != PlayerControl.LocalPlayer) return true;

				RPCEmitter.SendSetScanner(value);
				return false;
			}
		}

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.RpcPlayAnimation))]
		class PlayAnimation
		{
			static bool Prefix(PlayerControl __instance, byte animType)
			{
				if(!Instance.Enabled || __instance != PlayerControl.LocalPlayer) return true;

				RPCEmitter.SendPlayAnimation(animType);
				return false;
			}
		}
	}
}