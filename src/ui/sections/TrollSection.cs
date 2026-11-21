using BepInEx.Unity.IL2CPP.Utils.Collections;
using HydraMenu.assets;
using HydraMenu.modules;
using HydraMenu.network;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class TrollSection : Section
	{
		public TrollSection() : base("整蛊") { }

		private int selectedVent = 0;
		private readonly System.Random rnd = new System.Random();

		public override void Render()
		{
			bool hasAnticheat = Utilities.IsAnticheatPresent();

			if(PlayerControl.LocalPlayer == null)
			{
				GUILayout.Label("你当前不在对局中，这些选项将无法使用。");
			}

			ModuleManager.blackout.Enabled = GUILayout.Toggle(ModuleManager.blackout.Enabled, "全场停电");
			ModuleManager.crashLobby.Enabled = GUILayout.Toggle(ModuleManager.crashLobby.Enabled, "排队崩溃房间");
			ModuleManager.autoReportBodies.Enabled = Controls.PlayerSpecificToggle("自动报告尸体", PlayerControl.LocalPlayer, ref ModuleManager.autoReportBodies.target);
			Hydra.routines.autoTriggerSpores.Enabled = GUILayout.Toggle(Hydra.routines.autoTriggerSpores.Enabled, "自动触发孢子");
			ModuleManager.disableCameras.Enabled = GUILayout.Toggle(ModuleManager.disableCameras.Enabled, "禁用监控摄像头");
			ModuleManager.disableCloseDoors.Enabled = GUILayout.Toggle(ModuleManager.disableCloseDoors.Enabled, "禁用关门");
			ModuleManager.disableSabotages.Enabled = GUILayout.Toggle(ModuleManager.disableSabotages.Enabled, "禁用破坏");
			ModuleManager.disableVents.Enabled = GUILayout.Toggle(ModuleManager.disableVents.Enabled, "禁用管道");
			Hydra.routines.glitterBomb.Enabled = GUILayout.Toggle(Hydra.routines.glitterBomb.Enabled, "闪光弹");

			if(!hasAnticheat)
			{
				Hydra.routines.spamInfluencerCues.Enabled = GUILayout.Toggle(Hydra.routines.spamInfluencerCues.Enabled, "刷屏社媒提示");
				Hydra.routines.ziplineSpammer.Enabled = Controls.GlobalPlayerSpecificToggle("滑索刷屏", Hydra.routines.ziplineSpammer.targets);
			}

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("随机开始倒计时"))
			{
				sbyte timer = (sbyte)rnd.Next(sbyte.MinValue, sbyte.MaxValue);

				BatchedMessage batch = new BatchedMessage();
				batch.QueueSetStartCounter(PlayerControl.LocalPlayer, timer);
				batch.FinishBatch();
			}

			if(!hasAnticheat && GUILayout.Button("随机大厅过期时间"))
			{
				int timer = rnd.Next(0, int.MaxValue);

				BatchedMessage batch = new BatchedMessage();
				batch.QueueLobbyTimeExpiring(timer);
				batch.FinishBatch();
			}
			GUILayout.EndHorizontal();

			if(GUILayout.Button("投票踢出所有人"))
			{
				BatchedMessage batch = new BatchedMessage(AmongUsClient.Instance.HostId);
				int packingLimit = AmongUsClient.Instance.GetMaxMessagePackingLimit();

				// Ensure that the host is the last player to be kicked, as if the host gets kicked too early then some votekicks may not go through
				PlayerControl[] allPlayers = PlayerControl.AllPlayerControls.ToArray();
				allPlayers.Reverse();

				foreach(PlayerControl player in allPlayers)
				{
					if(batch.msgCount >= packingLimit)
					{
						batch.FinishBatch();
						batch = new BatchedMessage(AmongUsClient.Instance.HostId);
					}

					batch.QueueAddVote(AmongUsClient.Instance.ClientId, player.OwnerId);
				}

				batch.FinishBatch();
			}

			if(GUILayout.Button("踢出所有玩家"))
			{
				foreach(PlayerControl player in PlayerControl.AllPlayerControls)
				{
					if(player == PlayerControl.LocalPlayer || player.OwnerId == AmongUsClient.Instance.HostId) continue;

					Utilities.KickPlayer(player);
				}
			}

			if(GUILayout.Button("复制随机玩家"))
			{
				PlayerControl randomPl = Utilities.GetRandomPlayer();
				Utilities.CopyPlayer(randomPl);
			}

			if(GUILayout.Button("触发所有孢子"))
			{
				TriggerAllSpores();
			}

			if(GUILayout.Button("耗尽捉迷藏计时"))
			{
				AmongUsClient.Instance.StartCoroutine(DepleteSeekTimer().WrapToIl2Cpp());
			}

			SortedDictionary<int, string> vents = MapAssets.GetVents();

			GUILayout.Space(5);
			GUILayout.Label("管道传送：");
			Hydra.routines.teleportSpammer.Enabled = Controls.GlobalPlayerSpecificToggle("传送刷屏", Hydra.routines.teleportSpammer.targets);

			GUILayout.Label($"把所有人传送到管道：{vents.GetValueOrDefault(selectedVent, "无")}");
			selectedVent = Controls.HorizontalVentSlider(vents, selectedVent);

			if(GUILayout.Button("传送到该管道"))
			{
				foreach(PlayerControl player in PlayerControl.AllPlayerControls)
				{
					Teleporter.TeleportToVent(player, selectedVent);
				}
			}

			if(GUILayout.Button("传送到随机管道"))
			{
				foreach(PlayerControl player in PlayerControl.AllPlayerControls)
				{
					if(player == PlayerControl.LocalPlayer) continue;

					int ventId = rnd.Next(0, ShipStatus.Instance.AllVents.Count);

					Teleporter.TeleportToVent(player, ventId);
				}
			}

			GUILayout.Space(5);
			// Automatically close and open all doors at a set interval
			GUILayout.Label("门的整蛊：");
			Hydra.routines.doorTroller.Enabled = GUILayout.Toggle(Hydra.routines.doorTroller.Enabled, "启用");

			GUILayout.Label($"锁门与开门间隔：{Hydra.routines.doorTroller.LockAndUnlockDelay:F2} 秒");
			Hydra.routines.doorTroller.LockAndUnlockDelay = GUILayout.HorizontalSlider(Hydra.routines.doorTroller.LockAndUnlockDelay, 0.1f, 2.0f);

			GUILayout.Space(5);
			GUILayout.Label("自动曝光内鬼：");
			ModuleManager.autoExposeImpostors.Enabled = GUILayout.Toggle(ModuleManager.autoExposeImpostors.Enabled, "启用");
			ModuleManager.autoExposeImpostors.ExposeOnMurder = GUILayout.Toggle(ModuleManager.autoExposeImpostors.ExposeOnMurder, "击杀时曝光");
			ModuleManager.autoExposeImpostors.ExposeOnShapeshift = GUILayout.Toggle(ModuleManager.autoExposeImpostors.ExposeOnShapeshift, "变形时曝光");
			ModuleManager.autoExposeImpostors.ExposeOnPhantom = GUILayout.Toggle(ModuleManager.autoExposeImpostors.ExposeOnPhantom, "隐身时曝光");
		}

		private void TriggerAllSpores()
		{
			if(Utilities.GetCurrentMap() != MapNames.Fungle)
			{
				Hydra.notifications.Send("触发孢子", "该选项只在真菌丛林地图有效。");
				return;
			}

			FungleShipStatus shipStatus = ShipStatus.Instance.Cast<FungleShipStatus>();

			BatchedMessage batch = new BatchedMessage();

			foreach(Mushroom mushroom in shipStatus.sporeMushrooms.Values)
			{
				batch.QueueTriggerSpore(PlayerControl.LocalPlayer, mushroom);
			}

			batch.FinishBatch();

			Hydra.notifications.Send("触发孢子", "所有孢子已触发。", 5);
		}

		// In Hide and Seek, completing a task will reduce the HnS hide timer depending on the length of the task (short, common, or long)
		// The problem is the game reduces the task timer even if we have already completed the task
		// so we can spam the CompleteTask RPC with the same task, and reduce the task timer to zero seconds
		private IEnumerator DepleteSeekTimer()
		{
			if(!GameManager.Instance.IsHideAndSeek())
			{
				Hydra.notifications.Send("耗尽捉迷藏计时", "该功能只能在捉迷藏模式中使用。");
				yield break;
			}

			LogicGameFlowHnS gameFlow = GameManager.Instance.LogicFlow.Cast<LogicGameFlowHnS>();
			LogicOptionsHnS logicOptions = GameManager.Instance.LogicOptions.Cast<LogicOptionsHnS>();

			if(AmongUsClient.Instance.AmHost)
			{
				gameFlow.AdjustEscapeTimer(gameFlow.currentHideTime, true);
				yield break;
			}

			if(gameFlow.IsFinalCountdown)
			{
				Hydra.notifications.Send("耗尽捉迷藏计时", "最终躲藏阶段无法使用该功能。");
				yield break;
			}

			NormalPlayerTask applicableTask = null;
			foreach(PlayerTask task in PlayerControl.LocalPlayer.myTasks)
			{
				NormalPlayerTask normalTask = task.TryCast<NormalPlayerTask>();
				if(normalTask == null) continue;

				Hydra.Log.LogMessage($"Found applicable task {normalTask.TaskType}, task length of {normalTask.Length}");
				applicableTask = normalTask;

				if(normalTask.Length == NormalPlayerTask.TaskLength.Long) break;
			}

			if(applicableTask == null)
			{
				Hydra.notifications.Send("耗尽捉迷藏计时", "该功能要求你至少拥有一个任务。");
				yield break;
			}

			float completeDeduction;
			switch(applicableTask.Length)
			{
				case NormalPlayerTask.TaskLength.None:
				case NormalPlayerTask.TaskLength.Common:
				default:
					completeDeduction = logicOptions.GetCommonTaskTimeValue();
					break;

				case NormalPlayerTask.TaskLength.Short:
					completeDeduction = logicOptions.GetShortTaskTimeValue();
					break;

				case NormalPlayerTask.TaskLength.Long:
					completeDeduction = logicOptions.GetLongTaskTimeValue();
					break;
			}

			int requiredCompletions = (int)Math.Ceiling(gameFlow.currentHideTime / completeDeduction);

			Hydra.Log.LogInfo($"Current escape time is {gameFlow.currentHideTime} and each task completion reduces the timer by {completeDeduction}s. We need to send the CompleteTask RPC {requiredCompletions} times to deplete the HnS timer.");

			BatchedMessage batch = new BatchedMessage();

			for(int i = 0; i <= requiredCompletions; i++)
			{
				// The message packing limit for non-hosts should be ten, but the Among Us anticheat disconnects us if we have more than six CompleteTask RPCs in a single batch
				if(batch.msgCount >= 6)
				{
					batch.FinishBatch();
					batch = new BatchedMessage();

					yield return Effects.Wait(0.05f);
				}

				batch.QueueCompleteTask(PlayerControl.LocalPlayer, applicableTask.Id);
			}

			batch.FinishBatch();
		}
	}
}