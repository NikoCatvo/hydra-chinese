using AmongUs.Data.Player;
using HarmonyLib;

namespace HydraMenu.modules.protections
{
	internal class BypassDisconnectPenalty : Module
	{
		public BypassDisconnectPenalty() : base("绕过断开惩罚")
		{
			base.Enabled = true;
		}

		private static BypassDisconnectPenalty Instance
		{
			get { return ModuleManager.bypassDisconnectPenalty; }
		}

		// 开发此模块略有难度，因为许多封禁积分处理都在小的 getter 函数中
		// 这些大多被 Il2Cpp 编译器内联了
		// 我在 IDA 中查看了 GameAssembly.dll 文件，PlayerBanData::get_BanMinutesLeft 是少数未被内联的函数
		[HarmonyPatch(typeof(PlayerBanData), nameof(PlayerBanData.BanMinutesLeft), MethodType.Getter)]
		class GetBanMinutes
		{
			static void Prefix(PlayerBanData __instance)
			{
				if(!Instance.Enabled) return;
				__instance.banPoints = 0.0f;
			}
		}
	}
}