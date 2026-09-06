using HarmonyLib;

namespace HydraMenu.modules.protections
{
	internal class BlockUnauthorizedUpdates : Module
	{
		// 所有 ShipStatus RPC（CloseDoorsOfType 和 UpdateSystem）应仅发送给房主
		// 非房主可能向任何人发送系统更新
		// 并导致实际游戏状态与其游戏状态之间的不同步
		public BlockUnauthorizedUpdates() : base("阻止未授权更新")
		{
			base.Enabled = true;
		}

		private static BlockUnauthorizedUpdates Instance
		{
			get { return ModuleManager.blockUnauthorizedUpdates; }
		}

		[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.HandleRpc))]
		class OnShipStatusRPC
		{
			static bool Prefix()
			{
				return !Instance.Enabled || AmongUsClient.Instance.AmHost;
			}
		}
	}
}