using System;
using System.Diagnostics;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class MenuSection : Section
	{
		public MenuSection() : base("菜单") { }

		private byte configIndex = 0;

		public override void Render()
		{
			// GUILayout.Label($"Texture 2D memory usage: {Texture2D.currentTextureMemory}");
			Hydra.notifications.DisableNotifications = GUILayout.Toggle(Hydra.notifications.DisableNotifications, "禁用通知");

			GUILayout.Label($"主题色：{Lang.Tr(Styles.primaryColor)}");
			Styles.primaryColor = (Styles.UIColors)GUILayout.HorizontalSlider((float)Styles.primaryColor, 0, Styles.ColorValues.Count - 1);

			GUILayout.Label($"菜单不透明度：{Styles.menuOpacity * 100:F0}%");
			Styles.menuOpacity = (float)Math.Round(GUILayout.HorizontalSlider(Styles.menuOpacity, 0, 1), 4);

			GUILayout.Label($"界面缩放：{MainUI.scale:F2}x");
			MainUI.scale = (float)Math.Round(GUILayout.HorizontalSlider(MainUI.scale, 0.5f, 2.0f), 2);

			if(GUILayout.Button("应用更改"))
			{
				Styles.ClearCache();
			}

			if(GUILayout.Button("卸载"))
			{
				Hydra.Eject();
			}

			GUILayout.Space(5);
			GUILayout.Label($"配置：\n当前配置：{Hydra.config.currentConfig}");

			GUILayout.Label($"已选配置：{Hydra.config.configList[configIndex]}");
			configIndex = (byte)GUILayout.HorizontalSlider(configIndex, 0, Hydra.config.configList.Count - 1);

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("保存"))
			{
				Hydra.config.SaveConfig(Hydra.config.configList[configIndex]);
			}

			if(GUILayout.Button("加载"))
			{
				Hydra.config.LoadConfig(Hydra.config.configList[configIndex]);
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("新建配置"))
			{
				string configName = Hydra.config.GetUnusedConfigName();
				// I doubt anyone will actually have 255 configs with the pattern of "Hydra [1-255]", but just in case...
				if(configName == null)
				{
					Hydra.notifications.Send("配置", "未能找到未使用的配置名称。");
					return;
				}

				Hydra.config.CreateNewConfig(configName);
			}

			if(GUILayout.Button("打开配置文件夹"))
			{
				Process.Start("explorer.exe", Hydra.config.CONFIG_PATH);
			}
			GUILayout.EndHorizontal();
		}
	}
}