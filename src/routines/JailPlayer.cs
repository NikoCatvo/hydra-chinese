using HydraMenu.modules;
using System.Collections.Generic;
using UnityEngine;

namespace HydraMenu.routines
{
	// 这可以作为 CustomNetworkTransform::Deserialize 的后置补丁或作为例程实现
	// 我花了一段时间，但得出结论使用例程是更优雅的设计选择
	public class JailPlayerRoutine : Routine
	{
		public JailPlayerRoutine() : base("关入牢房") { }

		public readonly HashSet<int> targets = new HashSet<int>();

		// 为性能考虑，每 500ms 才检查一次玩家是否在牢房外
		public readonly float DELAY = 0.5f;
		private float timeElapsed = 0f;

		public override void Run()
		{
			timeElapsed += Time.deltaTime;
			if(timeElapsed < DELAY) return;
			timeElapsed = 0f;

			GetMapData(out SystemTypes jailRoom, out int ventId);

			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				if(!targets.Contains(player.GetHashCode())) continue;

				SystemTypes room = GetRoomForPlayer(player);
				if(room != jailRoom)
				{
					Teleporter.TeleportToVent(player, ventId);
				}
			}
		}

		// RoomTracker::GetRoomForPlayer 函数返回玩家当前位置的字符串
		// 但如果你不是侦探且玩家不在房间内而是在附近，它会抛出错误
		private SystemTypes GetRoomForPlayer(PlayerControl player)
		{
			foreach(PlainShipRoom room in ShipStatus.Instance.AllRooms)
			{
				if(room.roomArea == null) continue;

				int collisions = room.roomArea.OverlapCollider(HudManager.Instance.roomTracker.filter, HudManager.Instance.roomTracker.detectiveBuffer);
				if(RoomTracker.CheckHitsForPlayer(HudManager.Instance.roomTracker.detectiveBuffer, collisions, player))
				{
					return room.RoomId;
				}
			}

			return (SystemTypes)255;
		}

		private void GetMapData(out SystemTypes room, out int ventId)
		{
			MapNames currentMap = Utilities.GetCurrentMap();

			switch(currentMap)
			{
				case MapNames.Skeld:
				case MapNames.Dleks:
					room = SystemTypes.Nav;
					ventId = 12;
					break;

				case MapNames.MiraHQ:
					room = SystemTypes.Decontamination;
					ventId = 9;
					break;

				case MapNames.Polus:
					room = SystemTypes.Storage;
					ventId = 8;
					break;

				case MapNames.Airship:
					room = SystemTypes.GapRoom;
					ventId = 7;
					break;

				case MapNames.Fungle:
					room = SystemTypes.Laboratory;
					ventId = 4;
					break;

				// 默认使用 Skeld 的值
				default:
					room = SystemTypes.Nav;
					ventId = 12;
					break;
			}
		}

		private void OnDisconnect()
		{
			Hydra.notifications.Send("关入牢房", "你离开了游戏，关入牢房已禁用。", 10);
			Enabled = false;
		}

		protected override void OnEnable()
		{
			if(PlayerControl.LocalPlayer == null || ShipStatus.Instance == null)
			{
				Hydra.notifications.Send("关入牢房", "关入牢房只能在游戏中使用。", 10);
				Enabled = false;
				return;
			}

			EventCoordinator.OnDisconnect += OnDisconnect;
		}

		protected override void OnDisable()
		{
			targets.Clear();

			EventCoordinator.OnDisconnect -= OnDisconnect;
		}
	}
}