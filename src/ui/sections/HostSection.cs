using BepInEx.Unity.IL2CPP.Utils.Collections;
using HydraMenu.modules;
using HydraMenu.network;
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
				GUILayout.Label("你当前不在游戏中，这些选项将无法使用。");
			}
			else if(!AmongUsClient.Instance.AmHost)
			{
				GUILayout.Label("你不是当前大厅的房主。使用这些选项要么无效，要么会被反作弊封禁");
			}

			ModuleManager.banMidGame.Enabled = GUILayout.Toggle(ModuleManager.banMidGame.Enabled, "可在游戏中封禁玩家");

			ModuleManager.flipSkeld.Enabled = GUILayout.Toggle(ModuleManager.flipSkeld.Enabled, "使用翻转的 Skeld 地图");

			ModuleManager.disableGameEnd.Enabled = GUILayout.Toggle(ModuleManager.disableGameEnd.Enabled, "禁用游戏结束");
			ModuleManager.disableVentClean.Enabled = GUILayout.Toggle(ModuleManager.disableVentClean.Enabled, "禁用通风管道清理");

			ModuleManager.fakeShapeshiftBubble.Enabled = GUILayout.Toggle(ModuleManager.fakeShapeshiftBubble.Enabled, "假变身气泡");

			GUILayout.BeginHorizontal();
			ModuleManager.blockLowLevels.Enabled = GUILayout.Toggle(ModuleManager.blockLowLevels.Enabled, $"踢出等级低于 {ModuleManager.blockLowLevels.MinLevel} 级的玩家");
			ModuleManager.blockLowLevels.MinLevel = (uint)GUILayout.HorizontalSlider(ModuleManager.blockLowLevels.MinLevel, 0, 100);
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
				// 以防用户启用了此选项
				ModuleManager.disableGameEnd.Enabled = false;

				GameManager.Instance.RpcEndGame(GameOverReason.CrewmatesByTask, false);
				Hydra.notifications.Send("游戏结束", "你以船员胜利结束了游戏。", 5);
			}

			if(GUILayout.Button("强制内鬼胜利"))
			{
				// 以防用户启用了此选项
				ModuleManager.disableGameEnd.Enabled = false;

				GameManager.Instance.RpcEndGame(GameOverReason.ImpostorsByKill, false);
				Hydra.notifications.Send("游戏结束", "你以内鬼胜利结束了游戏。", 5);
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(5);
			GUILayout.Label("地图生成器:");

			GUILayout.Label($"选中地图: {(MapNames)selectedMap}");
			selectedMap = (byte)GUILayout.HorizontalSlider(selectedMap, 0, 5);

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("取消生成地图"))
			{
				if(shipList.Count != 0)
				{
					InnerNetObject ship = shipList.Dequeue();
					ship.Despawn();

					Hydra.notifications.Send("游戏地图", "当前地图已取消生成。", 5);
				}
				else
				{
					Hydra.notifications.Send("游戏地图", "游戏地图已被取消生成。", 10);
				}
			}

			if(GUILayout.Button("生成地图"))
			{
				AmongUsClient.Instance.StartCoroutine(SpawnMap(selectedMap).WrapToIl2Cpp());
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("取消生成大厅"))
			{
				if(lobbyList.Count > 0)
				{
					InnerNetObject lobby = lobbyList.Dequeue();
					lobby.Despawn();

					Hydra.notifications.Send("大厅地图", "大厅地图已取消生成。", 5);
				}
				else
				{
					Hydra.notifications.Send("大厅地图", "大厅地图已被取消生成。", 10);
				}
			}

			if(GUILayout.Button("生成大厅"))
			{
				SpawnLobby();
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(5);
			GUILayout.Label("分配下一局角色:");
			ModuleManager.assignRoles.Enabled = GUILayout.Toggle(ModuleManager.assignRoles.Enabled, "启用");
			GUILayout.Label($"分配角色: {ModuleManager.assignRoles.AssignedRole}");
			ModuleManager.assignRoles.AssignedRole = Controls.HorizontalRoleSlider(ModuleManager.assignRoles.AssignedRole);

			GUILayout.Space(5);
			GUILayout.Label("会议控制:");
			ModuleManager.disableMeetings.Enabled = GUILayout.Toggle(ModuleManager.disableMeetings.Enabled, "禁用会议");
			Hydra.routines.reportBodySpam.Enabled = GUILayout.Toggle(Hydra.routines.reportBodySpam.Enabled, "刷报尸体");
			ModuleManager.voteImmune.Enabled = Controls.PlayerSpecificToggle("投票免疫", PlayerControl.LocalPlayer, ModuleManager.voteImmune.targets);
			Hydra.routines.voteSpammer.Enabled = Controls.GlobalPlayerSpecificToggle("刷投票", Hydra.routines.voteSpammer.targets);

			if(GUILayout.Button("关闭会议"))
			{
				if(MeetingHud.Instance == null)
				{
					Hydra.notifications.Send("跳过会议", "此选项只能在会议中使用。");
				}
				else
				{
					MeetingHud.VoterState[] votes = Array.Empty<MeetingHud.VoterState>();

					BatchedMessage batch = new BatchedMessage();
					batch.QueueVotingComplete(votes, null, false, false, 0);
					batch.QueueCloseMeeting();
					batch.FinishBatch();
				}
			}

			GUILayout.Space(5);
			GUILayout.Label("变身控制:");
			if(GUILayout.Button("将所有人变身为我"))
			{
				AmongUsClient.Instance.StartCoroutine(ShapeshiftAll(PlayerControl.LocalPlayer).WrapToIl2Cpp());
			}

			if(GUILayout.Button("将所有人变身为随机玩家"))
			{
				PlayerControl target = Utilities.GetRandomPlayer(false, false, false, false);
				AmongUsClient.Instance.StartCoroutine(ShapeshiftAll(target).WrapToIl2Cpp());
			}

			if(GUILayout.Button("撤销所有变身"))
			{
				AmongUsClient.Instance.StartCoroutine(RevertAllShapeshift().WrapToIl2Cpp());
			}

			GUILayout.Space(5);
			GUILayout.Label("颜色控制:");

			GUILayout.Label($"将所有人颜色改为: {selectedColor}");
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

			GUILayout.Label($"颜色随机化延迟: {Hydra.routines.discoHost.RandomizationDelay:F2}秒");
			Hydra.routines.discoHost.RandomizationDelay = GUILayout.HorizontalSlider(Hydra.routines.discoHost.RandomizationDelay, 0.1f, 2.0f);
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
				Hydra.notifications.Send("击杀玩家", "此功能只能由大厅房主使用。");
				return;
			}

			// 在 +25 模组协议大厅中，我们可以作为非房主发送 SetColor RPC
			// 但我们仍受消息打包限制影响
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
			Hydra.Log.LogInfo($"尝试生成大厅");

			if(Utilities.IsAnticheatPresent() && !AmongUsClient.Instance.AmHost)
			{
				Hydra.notifications.Send("大厅生成器", "此功能只能由大厅房主使用。");
				return;
			}

			InnerNetObject lobbyPrefab = AmongUsClient.Instance.NonAddressableSpawnableObjects.First((obj) => obj.SpawnId == (uint)network.Constants.SpawnType.LobbyBehavior);
			if(lobbyPrefab == null)
			{
				Hydra.Log.LogError($"无法在 NonAddressableSpawnableObjects 中找到 LobbyBehavior 预制体");
				return;
			}

			LobbyBehaviour lobby = UnityEngine.Object.Instantiate(lobbyPrefab).Cast<LobbyBehaviour>();

			BatchedMessage batch = new BatchedMessage();
			batch.QueueSpawn(lobby, -2, SpawnFlags.None);
			batch.FinishBatch();

			Hydra.notifications.Send("大厅生成器", "已生成一个新的大厅实例", 5);
		}

		private static IEnumerator SpawnMap(byte mapId)
		{
			Hydra.Log.LogInfo($"尝试生成地图 ID {mapId}");

			// Utilities::IsAnticheatPresent 函数在这里不完全适用
			// 我们可以在本地或 skeld.net 大厅中生成任意地图而无需房主权限
			// 但 +25 模组协议大厅虽然禁用了大部分反作弊检查，仍会检查非房主发送生成消息
			if(Utilities.IsAnticheatPresent() && !AmongUsClient.Instance.AmHost)
			{
				Hydra.notifications.Send("地图生成器", "此功能只能由大厅房主使用。");
				yield break;
			}

			AsyncOperationHandle<GameObject> asyncHandle = AmongUsClient.Instance.ShipPrefabs[mapId].InstantiateAsync(null, false);
			yield return asyncHandle;

			ShipStatus ship = asyncHandle.Result.GetComponent<ShipStatus>();

			BatchedMessage batch = new BatchedMessage();
			batch.QueueSpawn(ship, -2, SpawnFlags.None);
			batch.FinishBatch();

			Hydra.notifications.Send("地图生成器", $"已生成 {(MapNames)mapId}。", 5);
		}

		private static IEnumerator ShapeshiftAll(PlayerControl target)
		{
			if(Utilities.IsAnticheatPresent() && !AmongUsClient.Instance.AmHost)
			{
				Hydra.notifications.Send("变身玩家", "你需要是大厅房主才能使用此功能。");
				yield break;
			}

			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				if(player == target || player.shapeshiftTargetPlayerId == target.PlayerId) continue;

				Utilities.ShapeshiftPlayer(player, target);

				// 此函数一次可发送多达 42 条可靠消息，所以需要实现延迟以避免被断开连接
				yield return Effects.Wait(0.05f);
			}
		}

		private static IEnumerator RevertAllShapeshift()
		{
			if(Utilities.IsAnticheatPresent() && !AmongUsClient.Instance.AmHost)
			{
				Hydra.notifications.Send("变身玩家", "你需要是大厅房主才能使用此功能。");
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