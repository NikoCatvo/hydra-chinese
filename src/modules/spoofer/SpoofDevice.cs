using HarmonyLib;

namespace HydraMenu.modules.spoofer
{
	internal class SpoofDevice : Module
	{
		public SpoofDevice() : base("伪装设备")
		{
			// 此模块可始终启用
			base.Enabled = true;
		}

		private static SpoofDevice Instance
		{
			get { return ModuleManager.spoofDevice; }
		}

		public Platforms SpoofedPlatform { get; set; } = Constants.GetPlatformType();

		[HarmonyPatch(typeof(PlatformSpecificData), nameof(PlatformSpecificData.Serialize))]
		class SerializeDevice
		{
			static void Prefix(PlatformSpecificData __instance)
			{
				__instance.Platform = Instance.SpoofedPlatform;

				switch (Instance.SpoofedPlatform)
				{
					case Platforms.StandaloneWin10:
						__instance.XboxPlatformId = 2584878536129841;
						break;

					case Platforms.Xbox:
						// 可在 https://www.cxkes.me/xbox/xuid 找到 Xbox 玩家标签的正确 XUID
						__instance.PlatformName = "Major Nelson";
						__instance.XboxPlatformId = 2584878536129841;
						break;

					case Platforms.Playstation:
						__instance.PlatformName = "";
						__instance.PsnPlatformId = 0;
						break;

					case Platforms.Switch:
						__instance.PlatformName = "Sus";
						break;

					default:
						// 其他平台不发送额外的平台特定数据
						__instance.PlatformName = "TESTNAME";
						__instance.XboxPlatformId = 0;
						__instance.PsnPlatformId = 0;
						break;
				}
			}
		}
	}
}