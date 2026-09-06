using AmongUs.GameOptions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HydraMenu.ui
{
	internal class Controls
	{
		// RoleTypes 枚举有一些奇怪的间隔，从 Crewmate (0) 到 Tracker (10) 都是正常的，但之后 Detective 是 12，而 Viper 是 18
		// https://www.innersloth.com/2026-roadmap-part-1/
		// Among Us 2026 路线图确实提到目前有 15 个原型角色正在开发中，
		// 这些间隔是否可以归因于尚未添加到游戏正式版本中的角色？
		public static readonly List<RoleTypes> RolesList = new List<RoleTypes>()
		{
			RoleTypes.Crewmate,
			RoleTypes.Scientist,
			RoleTypes.Engineer,
			RoleTypes.Noisemaker,
			RoleTypes.Tracker,
			RoleTypes.Detective,
			RoleTypes.Judge,
			RoleTypes.Impostor,
			RoleTypes.Shapeshifter,
			RoleTypes.Phantom,
			RoleTypes.Viper,
			RoleTypes.CrewmateGhost,
			RoleTypes.GuardianAngel,
			RoleTypes.ImpostorGhost
		};

		public enum PlayerColors
		{
			Red,
			Blue,
			Green,
			Pink,
			Orange,
			Yellow,
			Black,
			White,
			Purple,
			Brown,
			Cyan,
			Lime,
			Maroon,
			Rose,
			Banana,
			Gray,
			Tan,
			Coral,
			Fortegreen
		}

		public static RoleTypes HorizontalRoleSlider(RoleTypes currentRole)
		{
			int currentValue = RolesList.IndexOf(currentRole);

			byte newValue = (byte)GUILayout.HorizontalSlider(currentValue, 0, RolesList.Count - 1);

			return RolesList[newValue];
		}

		public static PlayerColors HorizontalColorSlider(PlayerColors currentColor)
		{
			return (PlayerColors)GUILayout.HorizontalSlider((int)currentColor, 0, Palette.ColorNames.Length);
		}

		public static int HorizontalVentSlider(SortedDictionary<int, string> vents, int currentVent)
		{
			// 如果当前通风管道超出范围则重置
			int firstKey = vents.Keys.First();
			if(currentVent < firstKey || currentVent >= vents.Count) currentVent = firstKey;

			return (int)GUILayout.HorizontalSlider(currentVent, firstKey, vents.Count - 1);
		}

		public static bool PlayerSpecificToggle(string label, PlayerControl selectedPlayer, ref PlayerControl currentPlayer)
		{
			GUIStyle toggle = new GUIStyle(GUI.skin.toggle);
			// 我们不想让开关显示为启用状态，如果 selectedPlayer 和 currentPlayer 都为 null
			bool isCurrentSelection = selectedPlayer != null && selectedPlayer == currentPlayer;

			if(isCurrentSelection)
			{
				toggle.normal = toggle.onNormal;
				toggle.active = toggle.onActive;
				toggle.hover = toggle.onHover;
			}

			// GUILayout::Toggle 函数总是返回开关的当前状态
			// 可以确定开关何时改变，但这需要涉及 getter 和 setter 的乱七八糟的黑客手段
			// 使用伪装成开关的 GUILayout.Button，仅在按钮被按下时触发，在这里更实用
			if(GUILayout.Button(label, toggle))
			{
				currentPlayer = isCurrentSelection ? null : selectedPlayer;
			}

			// 如果当前玩家不为 null，则模块（或例程）应启用
			return currentPlayer != null;
		}

		public static bool PlayerSpecificToggle(string label, PlayerControl selectedPlayer, HashSet<int> currentPlayers)
		{
			GUIStyle toggle = new GUIStyle(GUI.skin.toggle);
			bool isSelected = selectedPlayer != null && currentPlayers.Contains(selectedPlayer.GetHashCode());

			if(isSelected)
			{
				toggle.normal = toggle.onNormal;
				toggle.active = toggle.onActive;
				toggle.hover = toggle.onHover;
			}

			// GUILayout::Toggle 函数总是返回开关的当前状态
			// 可以确定开关何时改变，但这需要涉及 getter 和 setter 的乱七八糟的黑客手段
			// 使用伪装成开关的 GUILayout.Button，仅在按钮被按下时触发，在这里更实用
			if(GUILayout.Button(label, toggle) && selectedPlayer != null)
			{
				if(!isSelected)
				{
					currentPlayers.Add(selectedPlayer.GetHashCode());
				}
				else
				{
					currentPlayers.Remove(selectedPlayer.GetHashCode());
				}
			}

			return currentPlayers.Count != 0;
		}

		public static bool GlobalPlayerSpecificToggle(string label, HashSet<int> currentPlayers)
		{
			GUIStyle toggle = new GUIStyle(GUI.skin.toggle);
			bool enabled = currentPlayers.Count != 0;

			if(enabled)
			{
				toggle.normal = toggle.onNormal;
				toggle.active = toggle.onActive;
				toggle.hover = toggle.onHover;
			}

			if(GUILayout.Button(label, toggle))
			{
				if(!enabled)
				{
					currentPlayers.Add(int.MaxValue);
				}
				else
				{
					currentPlayers.Clear();
				}
			}

			return currentPlayers.Count != 0;
		}

		public static void DrawCrewmateColorBox(Rect rect, NetworkedPlayerInfo player)
		{
			string colorName = Utilities.GetPlayerColor(player);
			GUI.Box(rect, "", Styles.CreateCrewmateColorBox(colorName, colorName != "Fortegreen" ? player.Color : Color.black));
		}

		public static void DrawButtonCell<TKey, TValue>(Dictionary<TKey, TValue> buttons, Action<TValue> action, int columnsPerRow)
		{
			int currentColumn = 0;

			foreach(var (key, value) in buttons)
			{
				if(currentColumn == 0)
				{
					GUILayout.BeginHorizontal();
				}

				if(GUILayout.Button(key.ToString()))
				{
					action(value);
				}

				currentColumn++;
				if(currentColumn == columnsPerRow)
				{
					GUILayout.EndHorizontal();
					currentColumn = 0;
				}
			}

			// 如果按钮数量不能整除每行列数，则不会结束水平布局
			// 所以我们检查是否需要在此结束
			if(currentColumn != 0)
			{
				GUILayout.EndHorizontal();
			}
		}
	}
}