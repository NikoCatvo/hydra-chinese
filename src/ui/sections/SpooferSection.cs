using HydraMenu.modules;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class SpooferSection : Section
	{
		public SpooferSection() : base("伪装") { }

		public readonly Dictionary<string, int> versions = new Dictionary<string, int>()
		{
			// 当前运行时版本
			// VersionShower::Start 使用 ReferenceDataManager.Refdata.userFacingVersion 获取类似 "17.1" 的版本字符串
			// 但那似乎要等游戏完全加载后才有，所以我们必须使用 Constants::AddressablesVersion 获取较不易读的版本字符串
			{ $"{Constants.AddressablesVersion} (当前)", Constants.GetBroadcastVersion() },
			{ "16.1.0", 50632950 },
			{ "17.1", 50643450 },
			{ "17.1.2", 50647000 },
			{ "17.2", 50645050 },
			{ "17.2.1", 50652900 },
			{ "17.2.2", 50653700 },
			{ "17.3", 50652400 },
			{ "17.4", 50656300 },
			{ "18.0", 50663350 }
		};

		private int versionSelection = 0;

		public override void Render()
		{
			GUILayout.Label("版本伪装:");
			ModuleManager.spoofVersion.Enabled = GUILayout.Toggle(ModuleManager.spoofVersion.Enabled, "启用版本伪装");

			GUILayout.Label($"伪装版本: {versions.ElementAt(versionSelection).Key} ({ModuleManager.spoofVersion.spoofedVersion})");
			versionSelection = (int)GUILayout.HorizontalSlider(versionSelection, 0, versions.Count - 1);
			ModuleManager.spoofVersion.spoofedVersion = versions.ElementAt(versionSelection).Value;

			ModuleManager.spoofVersion.UseModdedProtocol = GUILayout.Toggle(ModuleManager.spoofVersion.UseModdedProtocol, "使用模组协议");

			GUILayout.Space(5);
			GUILayout.Label("等级伪装:");

			ModuleManager.spoofLevel.Enabled = GUILayout.Toggle(ModuleManager.spoofLevel.Enabled, "启用");
			GUILayout.Label($"伪装等级: {ModuleManager.spoofLevel.SpoofedLevel}");
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
				Hydra.notifications.Send("等级更新器", $"你的等级已更改为 {ModuleManager.spoofLevel.SpoofedLevel}", 5);
			}

			GUILayout.Space(5);
			GUILayout.Label("平台伪装:");

			GUILayout.Label($"伪装平台: {ModuleManager.spoofDevice.SpoofedPlatform}");
			ModuleManager.spoofDevice.SpoofedPlatform = (Platforms)GUILayout.HorizontalSlider((float)ModuleManager.spoofDevice.SpoofedPlatform, 0, 10);
		}

		private void ClampSelectedLevel(uint newLevel)
		{
			// 我们真的需要对等级值设置上限吗？
			// 我怀疑有多少人会把 +100 按那么多次
			uint maxLevel = Utilities.IsAnticheatPresent() ? 100001 : uint.MaxValue - 1;

			ModuleManager.spoofLevel.SpoofedLevel = Math.Clamp(newLevel, 0, maxLevel);
		}
	}
}