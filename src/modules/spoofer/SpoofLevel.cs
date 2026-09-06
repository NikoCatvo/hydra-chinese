using AmongUs.Data;
using AmongUs.InnerNet.GameDataMessages;
using HarmonyLib;
using Hazel;

namespace HydraMenu.modules.spoofer
{
	internal class SpoofLevel : Module
	{
		public SpoofLevel() : base("伪装等级") { }

		public uint SpoofedLevel { get; set; } = 200;

		private static SpoofLevel Instance
		{
			get { return ModuleManager.spoofLevel; }
		}

		// PlayerControl::RpcSetLevel 在 PlayerControl::Start 中被内联，所以我们无法直接打补丁该函数
		[HarmonyPatch(typeof(RpcSetLevelMessage), nameof(RpcSetLevelMessage.SerializeRpcValues))]
		class SerializeLevel
		{
			static bool Prefix(MessageWriter msg)
			{
				if(!Instance.Enabled) return true;
				uint level = Instance.SpoofedLevel - 1;

				msg.WritePacked(level);
				PlayerControl.LocalPlayer.SetLevel(level);
				return false;
			}
		}

		protected override void OnEnable()
		{
			if(PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.Data != null && PlayerControl.LocalPlayer.Data.PlayerLevel != SpoofedLevel)
			{
				PlayerControl.LocalPlayer.RpcSetLevel(SpoofedLevel);
			}
		}

		protected override void OnDisable()
		{
			uint trueLevel = DataManager.Player.Stats.Level;
			if(PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.Data != null && PlayerControl.LocalPlayer.Data.PlayerLevel != trueLevel)
			{
				PlayerControl.LocalPlayer.RpcSetLevel(trueLevel);
			}
		}
	}
}