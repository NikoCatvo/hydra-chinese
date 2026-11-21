using HydraMenu.assets;
using HydraMenu.modules;
using System;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class SpooferSection : Section
	{
		public SpooferSection() : base("伪装") { }

		public override void Render()
		{
			GUILayout.Label("版本伪装：");
			ModuleManager.spoofVersion.Enabled = GUILayout.Toggle(ModuleManager.spoofVersion.Enabled, "启用版本伪装");

			GUILayout.Label($"伪装版本：{GameAssets.BroadcastVersions[ModuleManager.spoofVersion.SpoofedVersion]}（{ModuleManager.spoofVersion.SpoofedVersion}）");
			ModuleManager.spoofVersion.SpoofedVersion = Controls.HorizontalListSlider(GameAssets.BroadcastVersions, ModuleManager.spoofVersion.SpoofedVersion);

			ModuleManager.spoofVersion.UseModdedProtocol = GUILayout.Toggle(ModuleManager.spoofVersion.UseModdedProtocol, "使用模组协议");

			GUILayout.Label($"模组注册伪装：{GameAssets.ModRegistrationGuids[CurrentModRegistration.ModRegistrationGuidString]}");
			CurrentModRegistration.ModRegistrationGuidString = Controls.HorizontalListSlider(GameAssets.ModRegistrationGuids, CurrentModRegistration.ModRegistrationGuidString);

			GUILayout.Space(5);
			GUILayout.Label("等级伪装：");

			ModuleManager.spoofLevel.Enabled = GUILayout.Toggle(ModuleManager.spoofLevel.Enabled, "启用");
			GUILayout.Label($"伪装等级：{ModuleManager.spoofLevel.SpoofedLevel}");
			ModuleManager.spoofLevel.SpoofedLevel = (uint)GUILayout.HorizontalSlider(ModuleManager.spoofLevel.SpoofedLevel, 1, 200);

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("-100"))
			{
				ClampSelectedLevel(ModuleManager.spoofLevel.SpoofedLevel - 100);
			}

			if(GUILayout.Button("-10"))
			{
				ClampSelectedLevel(ModuleManager.spoofLevel.SpoofedLevel - 10);
			}

			if(GUILayout.Button("+10"))
			{
				ClampSelectedLevel(ModuleManager.spoofLevel.SpoofedLevel + 10);
			}

			if(GUILayout.Button("+100"))
			{
				ClampSelectedLevel(ModuleManager.spoofLevel.SpoofedLevel + 100);
			}
			GUILayout.EndHorizontal();

			if(GUILayout.Button("发送等级更新"))
			{
				PlayerControl.LocalPlayer.RpcSetLevel(ModuleManager.spoofLevel.SpoofedLevel - 1);
				Hydra.notifications.Send("等级更新", $"你的等级已更改为 {ModuleManager.spoofLevel.SpoofedLevel}", 5);
			}

			GUILayout.Space(5);
			GUILayout.Label("平台伪装：");

			GUILayout.Label($"伪装平台：{GameAssets.GetPlatformName(ModuleManager.spoofDevice.SpoofedPlatform)}");
			ModuleManager.spoofDevice.SpoofedPlatform = (Platforms)Controls.HorizontalListSlider(GameAssets.PlayerPlatforms, (int)ModuleManager.spoofDevice.SpoofedPlatform);
		}

		private void ClampSelectedLevel(uint newLevel)
		{
			// Do we really need to have an upper bounds on the level value?
			// I doubt anyone will press the +100 that much anyway
			uint maxLevel = Utilities.IsAnticheatPresent() ? 100001 : uint.MaxValue - 1;

			ModuleManager.spoofLevel.SpoofedLevel = Math.Clamp(newLevel, 0, maxLevel);
		}
	}
}