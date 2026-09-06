using HarmonyLib;

namespace HydraMenu.modules.protections
{
	internal class BlockServerTeleports : Module
	{
		public BlockServerTeleports() : base("阻止服务器传送")
		{
			base.Enabled = true;
		}

		private static BlockServerTeleports Instance
		{
			get { return ModuleManager.blockServerTeleports; }
		}

		[HarmonyPatch(typeof(CustomNetworkTransform), nameof(CustomNetworkTransform.HandleRpc))]
		class OnRpc
		{
			static bool Prefix(CustomNetworkTransform __instance, byte callId)
			{
				if(!Instance.Enabled || callId != (byte)RpcCalls.SnapTo || __instance.myPlayer != PlayerControl.LocalPlayer) return true;

				Hydra.Log.LogMessage($"收到针对我们玩家的 SnapTo RPC，由于已启用阻止服务器传送，将忽略此 RPC");
				return false;
			}
		}
	}
}