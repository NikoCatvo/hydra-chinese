using HarmonyLib;
using InnerNet;

namespace HydraMenu.anticheat
{
	internal class PlatformSpoofer
	{
		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Start))]
		class PlatformSpoof
		{
			static void Postfix(PlayerControl __instance)
			{
				if(!Anticheat.Enabled || !Anticheat.CheckSpoofedPlatforms) return;

				ClientData clientData = AmongUsClient.Instance.GetClientFromCharacter(__instance);
				if(clientData == null) return;

				PlatformSpecificData platformData = clientData.PlatformData;

				if(!IsValidPlatform(platformData))
				{
					Anticheat.Flag(__instance, $"检测到 {clientData.PlayerName} 使用了伪装的平台信息。平台: {platformData.Platform}，平台名称: {platformData.PlatformName}，XUID: {platformData.XboxPlatformId}，PSID: {platformData.PsnPlatformId}。");
				}
			}
		}

		public static bool IsValidPlatform(PlatformSpecificData platform)
		{
			string platformName = platform.PlatformName;
			ulong xuid = platform.XboxPlatformId;
			ulong psid = platform.PsnPlatformId;

			switch(platform.Platform)
			{
				case Platforms.StandaloneEpicPC:
				case Platforms.StandaloneSteamPC:
				case Platforms.StandaloneMac:
				case Platforms.StandaloneItch:
				case Platforms.IPhone:
				case Platforms.Android:
				// 平台 ID 112 由第三方 Starlight 程序使用，允许 Android 设备使用 BepInEx 模组
				case (Platforms)112:
					if(IsGenericPlatformName(platformName) && xuid == 0 && psid == 0) return true;
					break;

				case Platforms.StandaloneWin10:
					if(IsGenericPlatformName(platformName) && xuid != 0 && psid == 0) return true;
					break;

				case Platforms.Xbox:
					// Xbox 玩家标签必须在 3 到 16 字符范围内
					// 其他玩家标签规则：https://learn.microsoft.com/en-us/gaming/gdk/docs/store/policies/xr/xr046?view=gdk-2510
					// 我们可以尝试将 XUID 解析为玩家标签并看是否匹配，但 XUID->玩家标签的 Xbox Live API 端点需要认证
					if(!IsGenericPlatformName(platformName) && platformName.Length >= 3 && platformName.Length <= 16 && xuid != 0 && psid == 0) return true;
					break;

				case Platforms.Playstation:
					if(!IsGenericPlatformName(platformName) && xuid == 0 && psid != 0) return true;
					break;

				case Platforms.Switch:
					if(!IsGenericPlatformName(platformName) && xuid == 0 && psid == 0) return true;
					break;

				// 在本地大厅中，所有玩家的平台 ID 都是 255
				case (Platforms)255:
					if(AmongUsClient.Instance.NetworkMode == NetworkModes.LocalGame) return true;
					break;
			}

			// 如果平台 ID 无效，或每个平台的平台特定数据无效，则我们知道玩家的设备被伪装了
			return false;
		}

		public static bool IsGenericPlatformName(string platformName)
		{
			return platformName == "TESTNAME";
		}
	}
}