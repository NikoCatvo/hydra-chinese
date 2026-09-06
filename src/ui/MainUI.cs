using HydraMenu.ui.sections;
using System;
using UnityEngine;

namespace HydraMenu.ui
{
	public class MainUI : MonoBehaviour
	{
		// 当前窗口
		public KeyCode menuKey = KeyCode.Insert;
		public bool visible = false;
		public static float scale = 1.0f;

		private bool isDragging = false;
		private Vector2 mouseDelta = new Vector2();

		public static Vector2 windowPosition = new Vector2(250, 100);
		public static Vector2 WindowSize
		{
			get { return new Vector2(500, 470) * scale; }
		}

		// UI Header
		public static Vector2 HeaderSize
		{
			get { return new Vector2(WindowSize.x, 20 * scale); }
		}

		public static Vector2 HeaderPosition
		{
			get { return new Vector2(windowPosition.x, windowPosition.y); }
		}

		// UI Section Pane
		private readonly Section[] sections = { new GeneralSection(), new SelfSection(), new TrollSection(), new SabotageSection(), new HostSection(), new RolesSection(), new PlayersSection(), new MovementSection(), new VisualSection(), new ProtectionsSection(), new AnticheatSection(), new SpooferSection(), new MenuSection() };
		public byte activeTab = 0;

		public static Vector2 SectionListSize
		{
			get { return new Vector2(100 * scale, WindowSize.y - HeaderSize.y); }
		}

		public static Vector2 SectionListPosition
		{
			get { return new Vector2(windowPosition.x, windowPosition.y + HeaderSize.y); }
		}

		public static Vector2 SectionButtonSize
		{
			get { return new Vector2(SectionListSize.x, 25 * scale); }
		}

		// Feature Pane
		public static Vector2 FeaturePaneSize
		{
			get { return new Vector2(WindowSize.x - SectionListSize.x, WindowSize.y - HeaderSize.y); }
		}

		public static Vector2 FeaturePanePosition
		{
			get { return new Vector2(SectionListPosition.x + SectionListSize.x, HeaderPosition.y + HeaderSize.y); }
		}

		public void Update()
		{
			Event currentEvent = Event.current;
			if(currentEvent == null) return;

			// Input::GetKeyDown(KeyCodes.Insert) 如果按下专用 Insert 键返回 true，但小键盘 Insert 键不返回
			// 所以我们必须依赖 Event.current
			if(currentEvent.type == EventType.KeyDown && currentEvent.keyCode == menuKey)
			{
				visible = !visible;
			}

			// 用于测试通知系统的工具
			if(Input.GetKeyDown(KeyCode.F6))
			{
				System.Random random = new System.Random();
				Hydra.notifications.Send("测试", $"敏捷的棕色狐狸跳过了懒狗。{random.Next(0, 100)}");
			}

			if(!visible) return;

			// 通过方向键处理当前分区的切换
			if(Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.DownArrow))
			{
				int offset = Input.GetKeyDown(KeyCode.UpArrow) ? -1 : 1;

				activeTab = (byte)Math.Clamp(activeTab + offset, 0, sections.Length - 1);
			}

			if(Input.GetKeyDown(KeyCode.PageUp) || Input.GetKeyDown(KeyCode.PageDown))
			{
				int offset = Input.GetKeyDown(KeyCode.PageUp) ? -1 : 1;

				sections[activeTab].HandleSubsectionMove(offset);
			}

			HandleBoxMovement();
		}

		public void OnGUI()
		{
			// https://docs.unity3d.com/6000.3/Documentation/Manual/GUIScriptingGuide.html
			if(!visible) return;

			GUI.skin.label.fontSize = (int)(13 * scale);

			// 渲染 UI 盒子
			GUI.Box(new Rect(windowPosition.x, windowPosition.y, WindowSize.x, WindowSize.y), $"{MyPluginInfo.PLUGIN_NAME} - {MyPluginInfo.PLUGIN_VERSION} 汉化by小紫 上传by微生猫 https://github.com/NikoCatvo/hydra-chinese", Styles.MainBox);

			for(byte i = 0; i < sections.Length; i++)
			{
				Section section = sections[i];

				// 将标签页添加到左侧面板
				RenderTab(i, section);

				if(i == activeTab)
				{
					GUILayout.BeginArea(new Rect(FeaturePanePosition.x, FeaturePanePosition.y, FeaturePaneSize.x, FeaturePaneSize.y));
					section.scrollVector = GUILayout.BeginScrollView(section.scrollVector);

					section.Render();

					GUILayout.EndScrollView();
					GUILayout.EndArea();
				}
			}
		}

		private void HandleBoxMovement()
		{
			// https://docs.unity3d.com/6000.3/Documentation/ScriptReference/Event.html
			Event currentEvent = Event.current;
			Vector2 mousePos = currentEvent.mousePosition;

			switch(currentEvent.type)
			{
// 我尝试使用 currentEvent.delta 获取上一个鼠标位置和当前位置的增量，
			// 但我发现它会频繁"跳过"，导致窗口盒子无法正确对齐到实际拖拽位置
				case EventType.MouseDown:
					if(!IsInBox(mousePos)) break;

					isDragging = true;
					mouseDelta = currentEvent.mousePosition - windowPosition;
					break;

				case EventType.MouseDrag:
					if(!isDragging) break;

					windowPosition.x = mousePos.x - mouseDelta.x;
					windowPosition.y = mousePos.y - mouseDelta.y;
					break;

				case EventType.MouseUp:
					isDragging = false;
					break;
			}
		}

		private bool IsInBox(Vector2 mousePos)
		{
			return
				mousePos.x >= windowPosition.x &&
				mousePos.x <= (windowPosition.x + WindowSize.x) &&
				mousePos.y >= windowPosition.y &&
				mousePos.y <= (windowPosition.y + WindowSize.y);
		}

		private void RenderTab(byte position, Section section)
		{
			Rect rect = new Rect(
				SectionListPosition.x,
				SectionListPosition.y + (position * SectionButtonSize.y),
				SectionButtonSize.x,
				SectionButtonSize.y
			);

			GUIStyle style = activeTab == position ? Styles.SectionBoxActive : Styles.SectionBox;
			if(GUI.Button(rect, section.name, style))
			{
				activeTab = position;
			}
		}

		public class MainUIConfig
		{
			public KeyCode MenuKey { get; set; }
			public Styles.UIColors PrimaryColor { get; set; }
			public float MenuOpacity { get; set; }
			public float UiScale { get; set; }
			public bool DisableNotifications { get; set; }
		}

		public MainUIConfig GetConfigData()
		{
			return new MainUIConfig
			{
				MenuKey = menuKey,
				PrimaryColor = Styles.primaryColor,
				MenuOpacity = Styles.menuOpacity,
				UiScale = scale,
				DisableNotifications = Hydra.notifications.disableNotifications
			};
		}

		public void LoadConfigData(MainUIConfig configData)
		{
			if(configData == null) return;

			if(configData.MenuKey != KeyCode.None)
			{
				Hydra.mainUI.menuKey = configData.MenuKey;
			}

			Styles.primaryColor = (Styles.UIColors)Math.Clamp((int)configData.PrimaryColor, 0, Styles.ColorValues.Count - 1);
			Styles.menuOpacity = Mathf.Clamp(configData.MenuOpacity, 0.0f, 1.0f);
			scale = Mathf.Clamp(configData.UiScale, 0.5f, 2.0f);
			Hydra.notifications.disableNotifications = configData.DisableNotifications;
		}
	}
}