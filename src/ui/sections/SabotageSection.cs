using System.Collections.Generic;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class SabotageSection : Section
	{
		public SabotageSection() : base("破坏") { }

		public override void Render()
		{
			if(ShipStatus.Instance == null)
			{
				GUILayout.Label("你当前不在对局中，或游戏尚未开始。这些选项将无法使用。");
			}

			Sabotage.UpdateSystemsDirectly = GUILayout.Toggle(Sabotage.UpdateSystemsDirectly, "直接更新破坏系统");

			Dictionary<string, SystemTypes> sabotages = Sabotage.GetSabotages();
			Dictionary<string, SystemTypes> doors = Sabotage.GetDoors();

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("全部破坏"))
			{
				Sabotage.SabotageAll();
				Hydra.notifications.Send("破坏", "所有破坏已启用。", 5);
			}

			if(GUILayout.Button("关闭所有门"))
			{
				Sabotage.LockAll();
				Hydra.notifications.Send("破坏", "所有门已关闭。", 5);
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("修复所有破坏"))
			{
				Sabotage.FixAllSabotages();
				Hydra.notifications.Send("破坏", "所有破坏已修复。", 5);
			}

			if(GUILayout.Button("解锁所有门"))
			{
				if(Sabotage.CanUnlockDoors())
				{
					Sabotage.UnlockAll();
					Hydra.notifications.Send("破坏", "所有门已解锁。", 5);
				}
				else
				{
					Hydra.notifications.Send("破坏", "你当前所在的地图不支持解锁门。", 10);
				}
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(5);
			GUILayout.Label("破坏项：");
			foreach(var (key, value) in sabotages)
			{
				if(GUILayout.Button(key))
				{
					HandleSabotage(value);
				}
			}

			GUILayout.Label("关门：");
			if(doors.Count == 0)
			{
				GUILayout.Label("该地图没有可以关闭的门。");
			}
			else
			{
				Controls.DrawButtonCell(doors, HandleCloseDoor, 2);
			}
		}

		private void HandleSabotage(SystemTypes system)
		{
			if(PlayerControl.LocalPlayer == null)
			{
				Hydra.notifications.Send("破坏", "该选项只能在游戏中使用。");
				return;
			}

			if(ShipStatus.Instance == null)
			{
				Hydra.notifications.Send("破坏", "需要存在 ShipStatus 实例，该功能才能生效。");
				return;
			}

			Event currentEvent = Event.current;

			if(currentEvent.button == 0)
			{
				Sabotage.SabotageSystem(system);
				Hydra.notifications.Send("破坏", $"{Lang.Tr(system)} 已被破坏。", 5);
			}
			else if(currentEvent.button == 1)
			{
				Sabotage.FixSabotage(system);
				Hydra.notifications.Send("破坏", $"{Lang.Tr(system)} 已被修复。", 5);
			}
		}

		private void HandleCloseDoor(SystemTypes door)
		{
			if(PlayerControl.LocalPlayer == null)
			{
				Hydra.notifications.Send("破坏", "该选项只能在游戏中使用。");
				return;
			}

			if(ShipStatus.Instance == null)
			{
				Hydra.notifications.Send("破坏", "需要存在 ShipStatus 实例，该功能才能生效。");
				return;
			}

			Event currentEvent = Event.current;

			if(currentEvent.button == 0)
			{
				Sabotage.LockDoor(door);
				return;
			}

			if(!Sabotage.CanUnlockDoors())
			{
				Hydra.notifications.Send("破坏", "只有房主，或地图为波拉斯、飞艇、真菌丛林时才能解锁门。");
				return;
			}

			Sabotage.UnlockDoor(door);
		}
	}
}