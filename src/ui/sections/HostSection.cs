using BepInEx.Unity.IL2CPP.Utils.Collections;
using HydraMenu.modules;
using HydraMenu.network;
using HydraMenu.ui;
using InnerNet;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace HydraMenu.ui.sections
{
	internal class HostSection : Section
	{
		public HostSection() : base("房主") { }

		private byte selectedMap = 0;
		private Controls.PlayerColors selectedColor = 0;

		public static readonly Queue<InnerNetObject> lobbyList = new Queue<InnerNetObject>();
		public static readonly Queue<InnerNetObject> shipList = new Queue<InnerNetObject>();

		public override void Render()
		{
			if(PlayerControl.LocalPlayer == null)
			{
				GUILayout.Label("你当前不在对局中，这些选项将无法使用。");
			}
			else if(!AmongUsClient.Instance.AmHost)
			{
				GUILayout.Label("你不是当前房间的房主。使用这些选项要么无效，要么会被反作弊封禁。");
			}

			ModuleManager.banMidGame.Enabled = GUILayout.Toggle(ModuleManager.banMidGame.Enabled, "可在游戏中途封禁玩家");

			ModuleManager.flipSkeld.Enabled = GUILayout.Toggle(ModuleManager.flipSkeld.Enabled, "使用翻转的太空飞船地图");

			ModuleManager.disableGameEnd.Enabled = GUILayout.Toggle(ModuleManager.disableGameEnd.Enabled, "禁用游戏结束");
			ModuleManager.disableVentClean.Enabled = GUILayout.Toggle(ModuleManager.disableVentClean.Enabled, "禁用管道清理");

			ModuleManager.fakeShapeshiftBubble.Enabled = GUILayout.Toggle(ModuleManager.fakeShapeshiftBubble.Enabled, "伪造变形气泡");

			GUILayout.BeginHorizontal();
			ModuleManager.blockLowLevels.Enabled = GUILayout.Toggle(ModuleManager.blockLowLevels.Enabled, $"踢出等级低于 {ModuleManager.blockLowLevels.MinLevel} 的玩家");
			ModuleManager.blockLowLevels.MinLevel = (uint)GUILayout.HorizontalSlider(ModuleManager.blockLowLevels.MinLevel, 0, 100);
			GUILayout.EndHorizontal();

			if(GUILayout.Button("强制开始游戏"))
			{
				ForceStartGame();
			}

			if(GUILayout.Button("杀死所有人"))
			{
				KillAllPlayers();
			}

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("强制船员胜利"))
			{
				// Just in case the user has this enabled
				ModuleManager.disableGameEnd.Enabled = false;

				GameManager.Instance.RpcEndGame(GameOverReason.CrewmatesByTask, false);
				Hydra.notifications.Send("游戏结束", "你以船员胜利结束了游戏。", 5);
			}

			if(GUILayout.Button("强制内鬼胜利"))
			{
				// Just in case the user has this enabled
				ModuleManager.disableGameEnd.Enabled = false;

				GameManager.Instance.RpcEndGame(GameOverReason.ImpostorsByKill, false);
				Hydra.notifications.Send("游戏结束", "你以内鬼胜利结束了游戏。", 5);
			}
			GUILayout.EndHorizontal();

			if(GUILayout.Button("临时封禁所有玩家"))
			{
				ModuleManager.tempBanAll.Enabled = true;
			}

			GUILayout.Space(5);
			GUILayout.Label("地图生成：");

			GUILayout.Label($"已选地图：{Lang.Tr((MapNames)selectedMap)}");
			selectedMap = (byte)GUILayout.HorizontalSlider(selectedMap, 0, 5);

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("移除地图"))
			{
				DespawnMap();
			}

			if(GUILayout.Button("生成地图"))
			{
				AmongUsClient.Instance.StartCoroutine(SpawnMap(selectedMap).WrapToIl2Cpp());
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("移除大厅"))
			{
				DespawnLobby();
			}

			if(GUILayout.Button("生成大厅"))
			{
				SpawnLobby();
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(5);
			GUILayout.Label("为下一局分配角色：");
			ModuleManager.assignRoles.Enabled = GUILayout.Toggle(ModuleManager.assignRoles.Enabled, "启用");
			GUILayout.Label($"要分配的角色：{Lang.Tr(ModuleManager.assignRoles.AssignedRole)}");
			ModuleManager.assignRoles.AssignedRole = Controls.HorizontalRoleSlider(ModuleManager.assignRoles.AssignedRole);

			GUILayout.Space(5);
			GUILayout.Label("会议控制：");
			ModuleManager.disableMeetings.Enabled = GUILayout.Toggle(ModuleManager.disableMeetings.Enabled, "禁用会议");
			Hydra.routines.reportBodySpam.Enabled = GUILayout.Toggle(Hydra.routines.reportBodySpam.Enabled, "刷屏报告尸体");
			ModuleManager.voteImmune.Enabled = Controls.PlayerSpecificToggle("投票免疫", PlayerControl.LocalPlayer, ModuleManager.voteImmune.targets);
			Hydra.routines.voteSpammer.Enabled = Controls.GlobalPlayerSpecificToggle("刷屏投票", Hydra.routines.voteSpammer.targets);

			if(GUILayout.Button("关闭会议"))
			{
				CloseMeeting();
			}

			GUILayout.Space(5);
			GUILayout.Label("变形控制：");
			if(GUILayout.Button("让所有人变成我"))
			{
				AmongUsClient.Instance.StartCoroutine(ShapeshiftAll(PlayerControl.LocalPlayer).WrapToIl2Cpp());
			}

			if(GUILayout.Button("让所有人变成随机玩家"))
			{
				PlayerControl target = Utilities.GetRandomPlayer(false, false, false, false);
				AmongUsClient.Instance.StartCoroutine(ShapeshiftAll(target).WrapToIl2Cpp());
			}

			if(GUILayout.Button("还原所有变形"))
			{
				AmongUsClient.Instance.StartCoroutine(RevertAllShapeshift().WrapToIl2Cpp());
			}

			GUILayout.Space(5);
			GUILayout.Label("颜色控制：");

			GUILayout.Label($"把所有人的颜色改为：{Lang.Tr(selectedColor)}");
			selectedColor = Controls.HorizontalColorSlider(selectedColor);

			if(GUILayout.Button("更改颜色"))
			{
				BatchedMessage batch = new BatchedMessage();

				foreach(PlayerControl player in PlayerControl.AllPlayerControls)
				{
					batch.QueueSetColor(player, (byte)selectedColor);
				}

				batch.FinishBatch();
			}

			Hydra.routines.discoHost.Enabled = Controls.GlobalPlayerSpecificToggle("迪斯科派对", Hydra.routines.discoHost.targets);

			GUILayout.Label($"颜色随机化延迟：{Hydra.routines.discoHost.RandomizationDelay:F2}s");
			Hydra.routines.discoHost.RandomizationDelay = GUILayout.HorizontalSlider(Hydra.routines.discoHost.RandomizationDelay, 0.1f, 2.0f);
		}

		private void ForceStartGame()
		{
			// Local lobbies are the only lobbies where we can start the game without host
			if(AmongUsClient.Instance.NetworkMode != NetworkModes.LocalGame && !AmongUsClient.Instance.AmHost)
			{
				Hydra.notifications.Send("开始游戏", "该功能仅限房间房主使用。");
				return;
			}

			// The vanilla anticheat prevents players from sending a ClientReady message more than once
			// If we attempt to start the game twice, then all players will send another ClientReady message, and the entire lobby will be kicked
			if(AmongUsClient.Instance.GameState == InnerNetClient.GameStates.Started && Utilities.IsAnticheatPresent())
			{
				Hydra.notifications.Send("开始游戏", "游戏已经开始。");
				return;
			}

			AmongUsClient.Instance.StartGame();

			// PlayerControl::RpcSetRole has checks against playing the intro cutscene in Freeplay
			// To avoid a blackscreen in Freeplay, we force the intro cutscene to start
			if(AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay)
			{
				HudManager.Instance.StartCoroutine(HudManager.Instance.CoShowIntro());
			}
		}

		private static void KillAllPlayers()
		{
			bool hasAnticheat = Utilities.IsAnticheatPresent();

			if(hasAnticheat && AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
			{
				Hydra.notifications.Send("击杀玩家", "该功能只能在游戏开始后使用。");
				return;
			}

			if(hasAnticheat && !AmongUsClient.Instance.AmHost)
			{
				Hydra.notifications.Send("击杀玩家", "该功能仅限房间房主使用。");
				return;
			}

			// On +25 modded protocol lobbies, we are able to send SetColor RPCs as non-host
			// however we are still affected by message packing limits
			int packingLimit = AmongUsClient.Instance.GetMaxMessagePackingLimit();
			BatchedMessage batch = new BatchedMessage();

			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				if(batch.msgCount >= packingLimit)
				{
					batch.FinishBatch();
					batch = new BatchedMessage();
				}

				batch.QueueMurderPlayer(PlayerControl.LocalPlayer, player, MurderResultFlags.Succeeded);
			}

			batch.FinishBatch();
		}

		private static void SpawnLobby()
		{
			Hydra.Log.LogInfo($"Attempting to spawn in lobby");

			if(Utilities.IsAnticheatPresent() && !AmongUsClient.Instance.AmHost)
			{
				Hydra.notifications.Send("生成大厅", "该功能仅限房间房主使用。");
				return;
			}

			InnerNetObject lobbyPrefab = AmongUsClient.Instance.NonAddressableSpawnableObjects.First((obj) => obj.SpawnId == (uint)SpawnType.LobbyBehavior);
			if(lobbyPrefab == null)
			{
				Hydra.Log.LogError($"Failed to find LobbyBehavior prefab in NonAddressableSpawnableObjects");
				return;
			}

			LobbyBehaviour lobby = UnityEngine.Object.Instantiate(lobbyPrefab).Cast<LobbyBehaviour>();

			BatchedMessage batch = new BatchedMessage();
			batch.QueueSpawn(lobby, -2, SpawnFlags.None);
			batch.FinishBatch();

			Hydra.notifications.Send("生成大厅", "已生成一个新的房间实例。", 5);
		}

		private static void DespawnLobby()
		{
			Hydra.Log.LogInfo($"Attempting to despawn lobby");

			if(Utilities.IsAnticheatPresent() && !AmongUsClient.Instance.AmHost)
			{
				Hydra.notifications.Send("移除大厅", "该功能仅限房间房主使用。");
				return;
			}

			if(lobbyList.Count == 0)
			{
				Hydra.notifications.Send("移除大厅", "大厅地图已经被移除了。", 10);
				return;
			}

			InnerNetObject lobby = lobbyList.Dequeue();
			lobby.Despawn();

			Hydra.notifications.Send("移除大厅", "大厅地图已被移除。", 5);
		}

		public static IEnumerator SpawnMap(byte mapId)
		{
			Hydra.Log.LogInfo($"Attempting to spawn in map id {mapId}");

			// The Utilities::IsAnticheatPresent function does not work perfectly here
			// We are able to spawn in any maps we want without host in local, or even skeld.net lobbies
			// however +25 modded protocol lobbies, while having much of their anticheat checks disabled, still has checks against non-hosts sending spawn messages
			if(Utilities.IsAnticheatPresent() && !AmongUsClient.Instance.AmHost)
			{
				Hydra.notifications.Send("生成地图", "该功能仅限房间房主使用。");
				yield break;
			}

			AsyncOperationHandle<GameObject> asyncHandle = AmongUsClient.Instance.ShipPrefabs[mapId].InstantiateAsync(null, false);
			yield return asyncHandle;

			ShipStatus ship = asyncHandle.Result.GetComponent<ShipStatus>();

			BatchedMessage batch = new BatchedMessage();
			batch.QueueSpawn(ship, -2, SpawnFlags.None);
			batch.FinishBatch();

			Hydra.notifications.Send("生成地图", $"{Lang.Tr((MapNames)mapId)} 已生成。", 5);
		}

		private static void DespawnMap()
		{
			Hydra.Log.LogInfo($"Attempting to despawn map");

			if(Utilities.IsAnticheatPresent() && !AmongUsClient.Instance.AmHost)
			{
				Hydra.notifications.Send("移除地图", "该功能仅限房间房主使用。");
				return;
			}

			if(shipList.Count == 0)
			{
				Hydra.notifications.Send("移除地图", "当前地图已经被移除了。", 10);
				return;
			}

			InnerNetObject ship = shipList.Dequeue();
			ship.Despawn();

			Hydra.notifications.Send("移除地图", "当前地图已被移除。", 5);
		}

		private void CloseMeeting()
		{
			if(Utilities.IsAnticheatPresent() && !AmongUsClient.Instance.AmHost)
			{
				Hydra.notifications.Send("关闭会议", "该功能仅限房间房主使用。");
				return;
			}

			if(MeetingHud.Instance == null)
			{
				Hydra.notifications.Send("关闭会议", "该选项只能在会议进行时使用。");
				return;
			}

			BatchedMessage batch = new BatchedMessage();
			batch.QueueVotingComplete(Array.Empty<MeetingHud.VoterState>(), null, false, false, 0);
			batch.QueueCloseMeeting();
			batch.FinishBatch();
		}

		private static IEnumerator ShapeshiftAll(PlayerControl target)
		{
			if(Utilities.IsAnticheatPresent() && !AmongUsClient.Instance.AmHost)
			{
				Hydra.notifications.Send("变形玩家", "你需要是房间房主才能使用该功能。");
				yield break;
			}

			PlayerControl[] allPlayers = PlayerControl.AllPlayerControls.ToArray();
			foreach(PlayerControl player in allPlayers)
			{
				if(player == target || player.shapeshiftTargetPlayerId == target.PlayerId) continue;

				Utilities.ShapeshiftPlayer(player, target);

				// This function can send up to 42 reliable messages at once, so we need to implement a delay to avoid getting disconnected
				yield return Effects.Wait(0.05f);
			}
		}

		private static IEnumerator RevertAllShapeshift()
		{
			if(Utilities.IsAnticheatPresent() && !AmongUsClient.Instance.AmHost)
			{
				Hydra.notifications.Send("变形玩家", "你需要是房间房主才能使用该功能。");
				yield break;
			}

			PlayerControl[] allPlayers = PlayerControl.AllPlayerControls.ToArray();
			foreach(PlayerControl player in allPlayers)
			{
				if(player.shapeshiftTargetPlayerId == -1) continue;

				Utilities.ShapeshiftPlayer(player, player);

				yield return Effects.Wait(0.05f);
			}
		}
	}
}
