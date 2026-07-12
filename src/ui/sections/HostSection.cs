using BepInEx.Unity.IL2CPP.Utils.Collections;
using HydraMenu.features;
using HydraMenu.network;
using InnerNet;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace HydraMenu.ui.sections
{
	internal class HostSection : ISection
	{
		public HostSection() : base("房主") { }

		private byte selectedMap = 0;

		public override void Render()
		{
			if(PlayerControl.LocalPlayer == null)
			{
				GUILayout.Label("你当前不在游戏中，这些选项将无法生效。");
			}
			else if(!AmongUsClient.Instance.AmHost)
			{
				GUILayout.Label("你不是当前房间的房主。使用这些选项要么不会生效，要么会被反作弊系统封禁。");
			}

			Host.BanMidGame.Enabled = GUILayout.Toggle(Host.BanMidGame.Enabled, "允许在游戏中封禁玩家");

			Host.FlippedSkeld = GUILayout.Toggle(Host.FlippedSkeld, "使用镜像 Skeld 地图");

			Host.DisableSabotages.Enabled = GUILayout.Toggle(Host.DisableSabotages.Enabled, "禁用破坏");
			Host.DisableCloseDoors.Enabled = GUILayout.Toggle(Host.DisableCloseDoors.Enabled, "禁用关门");
			Host.DisableCameras.Enabled = GUILayout.Toggle(Host.DisableCameras.Enabled, "禁用监控摄像头");
			Host.DisableGameEnd.Enabled = GUILayout.Toggle(Host.DisableGameEnd.Enabled, "禁用游戏结束");
			Host.NoKillCooldown.Enabled = GUILayout.Toggle(Host.NoKillCooldown.Enabled, "无击杀冷却");

			GUILayout.BeginHorizontal();
			Host.BlockLowLevels.Enabled = GUILayout.Toggle(Host.BlockLowLevels.Enabled, $"踢出等级低于 {Host.BlockLowLevels.MinLevel} 的玩家");
			Host.BlockLowLevels.MinLevel = (uint)GUILayout.HorizontalSlider(Host.BlockLowLevels.MinLevel, 0, 100);
			GUILayout.EndHorizontal();

			if(GUILayout.Button("强制开始游戏"))
			{
				AmongUsClient.Instance.StartGame();
			}

			if(GUILayout.Button("击杀所有人"))
			{
				KillAllPlayers();
			}

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("强制船员胜利"))
			{
				Host.DisableGameEnd.Enabled = false;

				GameManager.Instance.RpcEndGame(GameOverReason.CrewmatesByTask, false);
				Hydra.notifications.Send("游戏结束", "你以船员胜利结束了本局游戏。", 5);
			}

			if(GUILayout.Button("强制内鬼胜利"))
			{
				Host.DisableGameEnd.Enabled = false;

				GameManager.Instance.RpcEndGame(GameOverReason.ImpostorsByKill, false);
				Hydra.notifications.Send("游戏结束", "你以内鬼胜利结束了本局游戏。", 5);
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(5);
			GUILayout.Label("地图生成器:");

			GUILayout.Label($"已选地图: {(MapNames)selectedMap}");
			selectedMap = (byte)GUILayout.HorizontalSlider(selectedMap, 0, 5);

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("移除地图"))
			{
				if(ShipStatus.Instance != null)
				{
					ShipStatus.Instance.Despawn();
					Hydra.notifications.Send("游戏地图", "当前地图已被移除。", 5);
				}
				else
				{
					Hydra.notifications.Send("游戏地图", "游戏地图已经被移除了。", 5);
				}
			}

			if(GUILayout.Button("生成地图"))
			{
				AmongUsClient.Instance.StartCoroutine(SpawnMap(selectedMap).WrapToIl2Cpp());
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("移除大厅"))
			{
				if(LobbyBehaviour.Instance != null)
				{
					LobbyBehaviour.Instance.Despawn();
					Hydra.notifications.Send("大厅地图", "大厅地图已被移除。", 5);
				}
				else
				{
					Hydra.notifications.Send("大厅地图", "大厅地图已经被移除了。", 5);
				}
			}

			if(GUILayout.Button("生成大厅"))
			{
				LobbyBehaviour.Instance = UnityEngine.Object.Instantiate<LobbyBehaviour>(GameStartManager.Instance.LobbyPrefab);
				AmongUsClient.Instance.Spawn(LobbyBehaviour.Instance, -2, SpawnFlags.None);

				Hydra.notifications.Send("大厅地图", "已生成一个新的大厅地图实例。", 5);
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(5);
			GUILayout.Label("为下一局分配身份:");
			Host.AlwaysImposter.Enabled = GUILayout.Toggle(Host.AlwaysImposter.Enabled, "启用");
			GUILayout.Label($"要分配的身份: {Host.AlwaysImposter.assignedRole}");
			Host.AlwaysImposter.assignedRole = Controls.HorizontalRoleSlider(Host.AlwaysImposter.assignedRole);

			GUILayout.Space(5);
			GUILayout.Label("会议控制:");
			Host.DisableMeetings.Enabled = GUILayout.Toggle(Host.DisableMeetings.Enabled, "禁用会议");
			Hydra.routines.reportBodySpam.Enabled = GUILayout.Toggle(Hydra.routines.reportBodySpam.Enabled, "刷屏报告尸体");

			if(GUILayout.Button("结束会议"))
			{
				if(MeetingHud.Instance == null)
				{
					Hydra.notifications.Send("跳过会议", "此选项只能在会议中使用。");
				}
				else
				{
					MeetingHud.VoterState[] votes = Array.Empty<MeetingHud.VoterState>();

					BatchedMessage batch = new BatchedMessage();
					batch.QueueVotingComplete(votes, null, false);
					batch.QueueCloseMeeting();
					batch.FinishBatch();
				}
			}

			GUILayout.Space(5);
			GUILayout.Label("变形控制:");
			if(GUILayout.Button("让所有人变成我"))
			{
				AmongUsClient.Instance.StartCoroutine(ShapeshiftAll(PlayerControl.LocalPlayer).WrapToIl2Cpp());
			}

			if(GUILayout.Button("让所有人变成随机对象"))
			{
				PlayerControl target = Utilities.GetRandomPlayer(false, false, false, false);
				AmongUsClient.Instance.StartCoroutine(ShapeshiftAll(target).WrapToIl2Cpp());
			}

			if(GUILayout.Button("还原所有变形"))
			{
				AmongUsClient.Instance.StartCoroutine(RevertAllShapeshift().WrapToIl2Cpp());
			}

			GUILayout.Space(5);
			GUILayout.Label("蹦迪派对:");
			Hydra.routines.discoHost.Enabled = Controls.GlobalPlayerSpecificToggle("启用", ref Hydra.routines.discoHost.targets);

			GUILayout.Label($"颜色随机化延迟: {Hydra.routines.discoHost.randomizationDelay:F2}秒");
			Hydra.routines.discoHost.randomizationDelay = GUILayout.HorizontalSlider(Hydra.routines.discoHost.randomizationDelay, 0.1f, 2.0f);
		}

		private static void KillAllPlayers()
		{
			bool hasAnticheat = Utilities.IsAnticheatPresent();

			if(hasAnticheat && AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
			{
				Hydra.notifications.Send("击杀玩家", "此功能只能在游戏开始后使用。");
				return;
			}

			if(hasAnticheat && !AmongUsClient.Instance.AmHost)
			{
				Hydra.notifications.Send("击杀玩家", "此功能只能由房间的房主使用。");
				return;
			}

			BatchedMessage batch = new BatchedMessage();

			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				batch.QueueMurderPlayer(PlayerControl.LocalPlayer, player, MurderResultFlags.Succeeded);
			}

			batch.FinishBatch();
		}

		private static IEnumerator SpawnMap(byte mapId)
		{
			Hydra.Log.LogInfo($"Attempting to spawn in map id {mapId}");

			if(Utilities.IsAnticheatPresent() && !AmongUsClient.Instance.AmHost)
			{
				Hydra.notifications.Send("地图生成器", "此功能只能由房间的房主使用。");
				yield break;
			}

			AsyncOperationHandle<GameObject> asyncHandle = AmongUsClient.Instance.ShipPrefabs[mapId].InstantiateAsync(null, false);
			yield return asyncHandle;

			ShipStatus ship = asyncHandle.Result.GetComponent<ShipStatus>();

			BatchedMessage batch = new BatchedMessage();
			batch.QueueSpawn(ship, -2, SpawnFlags.None);
			batch.FinishBatch();

			Hydra.notifications.Send("地图生成器", $"{(MapNames)mapId} 已生成。", 5);
		}

		private static IEnumerator ShapeshiftAll(PlayerControl target)
		{
			if(Utilities.IsAnticheatPresent() && !AmongUsClient.Instance.AmHost)
			{
				Hydra.notifications.Send("变形玩家", "你需要成为房间的房主才能使用此功能。");
				yield break;
			}

			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				if(player == target || player.shapeshiftTargetPlayerId == target.PlayerId) continue;

				Utilities.ShapeshiftPlayer(player, target);

				yield return Effects.Wait(0.05f);
			}
		}

		private static IEnumerator RevertAllShapeshift()
		{
			if(Utilities.IsAnticheatPresent() && !AmongUsClient.Instance.AmHost)
			{
				Hydra.notifications.Send("变形玩家", "你需要成为房间的房主才能使用此功能。");
				yield break;
			}

			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				if(player.shapeshiftTargetPlayerId == -1) continue;

				Utilities.ShapeshiftPlayer(player, player);

				yield return Effects.Wait(0.05f);
			}
		}
	}
}
