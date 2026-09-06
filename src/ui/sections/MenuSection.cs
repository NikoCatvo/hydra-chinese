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
			// GUILayout.Label($"纹理 2D 内存使用量: {Texture2D.currentTextureMemory}");
			Hydra.notifications.disableNotifications = GUILayout.Toggle(Hydra.notifications.disableNotifications, "禁用通知");

			GUILayout.Label($"主色: {Styles.primaryColor}");
			Styles.primaryColor = (Styles.UIColors)GUILayout.HorizontalSlider((float)Styles.primaryColor, 0, Styles.ColorValues.Count - 1);

			GUILayout.Label($"菜单不透明度: {Styles.menuOpacity * 100:F0}%");
			Styles.menuOpacity = (float)Math.Round(GUILayout.HorizontalSlider(Styles.menuOpacity, 0, 1), 4);

			GUILayout.Label($"UI 缩放: {MainUI.scale:F2}倍");
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
			GUILayout.Label($"配置:\n当前配置: {Hydra.config.currentConfig}");

			GUILayout.Label($"选中配置: {Hydra.config.configList[configIndex]}");
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
				// 我怀疑真的会有人创建 255 个符合 "Hydra [1-255]" 模式的配置，但以防万一...
				if(configName == null)
				{
					Hydra.notifications.Send("配置", "找不到未使用的配置名称。");
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