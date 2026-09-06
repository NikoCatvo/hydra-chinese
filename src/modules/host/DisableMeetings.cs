using HarmonyLib;

namespace HydraMenu.modules.host
{
	internal class DisableMeetings : Module
	{
		public DisableMeetings() : base("禁用会议") { }

		private static DisableMeetings Instance
		{
			get { return ModuleManager.disableMeetings; }
		}

		// 当玩家报告尸体时，其客户端向房主发送 ReportDeadBody RPC。房主随后应验证 RPC 并开始会议
		// 要阻止会议，我们只需忽略任何接收到的 ReportDeadBody RPC
		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
		class OnReportBody
		{
			static bool Prefix()
			{
				return !Instance.Enabled;
			}
		}
	}
}