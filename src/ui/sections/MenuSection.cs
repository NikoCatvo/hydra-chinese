using System;
using BepInEx;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class MenuSection : ISection
	{
		public MenuSection() : base("菜单") { }

		public override void Render()
		{
			Hydra.notifications.DisableNotifications = GUILayout.Toggle(Hydra.notifications.DisableNotifications, "禁用通知");

			GUILayout.Label($"主色调: {Styles.primaryColor}");
			Styles.primaryColor = (Styles.UIColors)GUILayout.HorizontalSlider((float)Styles.primaryColor, 0, Styles.ColorValues.Count - 1);

			GUILayout.Label($"菜单不透明度: {Styles.menuOpacity * 100:F0}%");
			Styles.menuOpacity = (float)Math.Round(GUILayout.HorizontalSlider(Styles.menuOpacity, 0, 1), 4);

			GUILayout.Label($"界面缩放: {MainUI.scale:F2}x");
			MainUI.scale = (float)Math.Round(GUILayout.HorizontalSlider(MainUI.scale, 0.5f, 2.0f), 2);

			if(GUILayout.Button("应用更改"))
			{
				Styles.ClearCache();
			}

			GUILayout.Space(10);
			GUILayout.Label("配置管理:");

			if(GUILayout.Button("保存到配置"))
			{
				Profile.Save();
				Hydra.notifications.Send("配置", "已保存所有开关状态。", 5);
			}

			if(GUILayout.Button("从配置载入"))
			{
				Profile.Load();
				Hydra.notifications.Send("配置", "已从配置文件载入。", 5);
			}

			if(GUILayout.Button("重新载入配置"))
			{
				Profile.Load();
				Hydra.notifications.Send("配置", "配置已重新载入。", 5);
			}

			if(GUILayout.Button("打开配置文件夹"))
			{
				Application.OpenURL(Paths.ConfigPath);
				Hydra.notifications.Send("配置", "已打开配置文件夹。", 5);
			}
		}
	}
}
