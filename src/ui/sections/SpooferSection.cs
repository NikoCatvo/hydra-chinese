using HydraMenu.features;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class SpooferSection : ISection
	{
		public SpooferSection() : base("伪装") { }

		public readonly Dictionary<string, int> versions = new Dictionary<string, int>()
		{
			{ $"{Constants.AddressablesVersion} (当前)", Constants.GetBroadcastVersion() },
			{ "16.1.0", 50632950 },
			{ "17.1", 50643450 },
			{ "17.1.2", 50647000 },
			{ "17.2", 50645050 },
			{ "17.2.1", 50652900 },
			{ "17.2.2", 50653700 },
			{ "17.3", 50652400 },
			{ "17.4", 50656300 }
		};

		private int versionSelection = 0;

		public override void Render()
		{
			GUILayout.Label("版本伪装器:");
			Spoofer.shouldSpoofVersion = GUILayout.Toggle(Spoofer.shouldSpoofVersion, "启用版本伪装");

			GUILayout.Label($"伪装版本: {versions.ElementAt(versionSelection).Key} ({Spoofer.spoofedVersion})");
			versionSelection = (int)GUILayout.HorizontalSlider(versionSelection, 0, versions.Count - 1);
			Spoofer.spoofedVersion = versions.ElementAt(versionSelection).Value;

			Spoofer.useModdedProtocol = GUILayout.Toggle(Spoofer.useModdedProtocol, "使用模组协议");

			GUILayout.Space(5);
			GUILayout.Label("等级伪装器:");

			Spoofer.SpoofLevel.Enabled = GUILayout.Toggle(Spoofer.SpoofLevel.Enabled, "启用");
			GUILayout.Label($"伪装等级: {Spoofer.SpoofLevel.newLevel}");
			Spoofer.SpoofLevel.newLevel = (uint)GUILayout.HorizontalSlider(Spoofer.SpoofLevel.newLevel, 1, 200);

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("-100"))
			{
				ClampSelectedLevel(Spoofer.SpoofLevel.newLevel - 100);
			}

			if(GUILayout.Button("-10"))
			{
				ClampSelectedLevel(Spoofer.SpoofLevel.newLevel - 10);
			}

			if(GUILayout.Button("+10"))
			{
				ClampSelectedLevel(Spoofer.SpoofLevel.newLevel + 10);
			}

			if(GUILayout.Button("+100"))
			{
				ClampSelectedLevel(Spoofer.SpoofLevel.newLevel + 100);
			}
			GUILayout.EndHorizontal();

			if(GUILayout.Button("发送等级更新"))
			{
				PlayerControl.LocalPlayer.RpcSetLevel(Spoofer.SpoofLevel.newLevel - 1);
				Hydra.notifications.Send("等级更新器", $"你的等级已改为 {Spoofer.SpoofLevel.newLevel}", 5);
			}

			GUILayout.Space(5);
			GUILayout.Label("平台伪装器:");

			GUILayout.Label($"伪装平台: {Spoofer.spoofedPlatform}");
			Spoofer.spoofedPlatform = (Platforms)GUILayout.HorizontalSlider((float)Spoofer.spoofedPlatform, 0, 10);
		}

		private void ClampSelectedLevel(uint newLevel)
		{
			uint maxLevel = Utilities.IsAnticheatPresent() ? 100001 : uint.MaxValue - 1;

			Spoofer.SpoofLevel.newLevel = Math.Clamp(newLevel, 0, maxLevel);
		}
	}
}
