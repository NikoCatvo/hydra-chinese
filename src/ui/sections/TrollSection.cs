using BepInEx.Unity.IL2CPP.Utils.Collections;
using Hazel;
using HydraMenu.assets;
using HydraMenu.modules;
using HydraMenu.network;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class TrollSection : Section
	{
		public TrollSection() : base("恶搞") { }

		private int selectedVent = 0;
		private readonly System.Random rnd = new System.Random();

		public override void Render()
		{
			if(PlayerControl.LocalPlayer == null)
			{
				GUILayout.Label("你当前不在对局中，这些选项将无法使用。");
			}

			ModuleManager.crashLobby.Enabled = GUILayout.Toggle(ModuleManager.crashLobby.Enabled, "排队崩溃房间");
			ModuleManager.autoReportBodies.Enabled = Controls.PlayerSpecificToggle("自动报告尸体", PlayerControl.LocalPlayer, ref ModuleManager.autoReportBodies.target);
			Hydra.routines.autoTriggerSpores.Enabled = GUILayout.Toggle(Hydra.routines.autoTriggerSpores.Enabled, "自动触发孢子");
			ModuleManager.disableCameras.Enabled = GUILayout.Toggle(ModuleManager.disableCameras.Enabled, "禁用监控摄像头");
			ModuleManager.disableCloseDoors.Enabled = GUILayout.Toggle(ModuleManager.disableCloseDoors.Enabled, "禁用关门");
			ModuleManager.disableSabotages.Enabled = GUILayout.Toggle(ModuleManager.disableSabotages.Enabled, "禁用破坏");
			ModuleManager.disableVents.Enabled = GUILayout.Toggle(ModuleManager.disableVents.Enabled, "禁用管道");
			Hydra.routines.glitterBomb.Enabled = GUILayout.Toggle(Hydra.routines.glitterBomb.Enabled, "闪光弹");
			Hydra.routines.ziplineSpammer.Enabled = Controls.GlobalPlayerSpecificToggle("滑索刷屏", Hydra.routines.ziplineSpammer.targets);

			if(GUILayout.Button("踢出所有玩家"))
			{
				Hydra.Log.LogInfo($"Sending Enter ventilation system update to all players");

				MessageWriter writer = MessageWriter.Get(SendOption.Reliable);
				writer.Write((ushort)0);
				writer.Write((byte)VentilationSystem.Operation.Enter);
				writer.Write((byte)0);

				BatchedMessage batch = new BatchedMessage();
				batch.QueueUpdateSystem(PlayerControl.LocalPlayer, SystemTypes.Ventilation, writer);
				batch.FinishBatch();

				writer.Recycle();

				foreach(PlayerControl player in PlayerControl.AllPlayerControls)
				{
					if(player == PlayerControl.LocalPlayer || player.OwnerId == AmongUsClient.Instance.HostId) continue;

					Utilities.KickPlayer(player, true);
				}
			}

			if(GUILayout.Button("复制随机玩家"))
			{
				PlayerControl randomPl = Utilities.GetRandomPlayer();
				Utilities.CopyPlayer(randomPl);
			}

			if(GUILayout.Button("触发所有孢子"))
			{
				if(Utilities.GetCurrentMap() != MapNames.Fungle)
				{
					Hydra.notifications.Send("触发孢子", "该选项只在真菌丛林地图有效。");
				}
				else
				{
					FungleShipStatus shipStatus = ShipStatus.Instance.Cast<FungleShipStatus>();

					BatchedMessage batch = new BatchedMessage();

					foreach(Mushroom mushroom in shipStatus.sporeMushrooms.Values)
					{
						batch.QueueTriggerSpore(PlayerControl.LocalPlayer, mushroom);
					}

					batch.FinishBatch();

					Hydra.notifications.Send("触发孢子", "所有孢子已触发。", 5);
				}
			}

			if(GUILayout.Button("耗尽捉迷藏计时"))
			{
				AmongUsClient.Instance.StartCoroutine(DepleteSeekTimer().WrapToIl2Cpp());
			}

			SortedDictionary<int, string> vents = MapAssets.GetVents();

			GUILayout.Space(5);
			GUILayout.Label($"管道传送：");
			Hydra.routines.teleportSpammer.Enabled = Controls.GlobalPlayerSpecificToggle("传送刷屏", Hydra.routines.teleportSpammer.targets);

			GUILayout.Label($"把所有人传送到管道：{vents.GetValueOrDefault(selectedVent, "无")}");
			selectedVent = Controls.HorizontalVentSlider(vents, selectedVent);

			if(GUILayout.Button("传送到管道"))
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
			GUILayout.Label("门恶搞：");
			Hydra.routines.doorTroller.Enabled = GUILayout.Toggle(Hydra.routines.doorTroller.Enabled, "启用");

			GUILayout.Label($"锁门与解锁延迟：{Hydra.routines.doorTroller.LockAndUnlockDelay:F2}s");
			Hydra.routines.doorTroller.LockAndUnlockDelay = GUILayout.HorizontalSlider(Hydra.routines.doorTroller.LockAndUnlockDelay, 0.1f, 2.0f);

			GUILayout.Space(5);
			GUILayout.Label("自动揭发内鬼：");
			ModuleManager.autoExposeImpostors.Enabled = GUILayout.Toggle(ModuleManager.autoExposeImpostors.Enabled, "启用");
			ModuleManager.autoExposeImpostors.ExposeOnMurder = GUILayout.Toggle(ModuleManager.autoExposeImpostors.ExposeOnMurder, "击杀时揭发");
			ModuleManager.autoExposeImpostors.ExposeOnShapeshift = GUILayout.Toggle(ModuleManager.autoExposeImpostors.ExposeOnShapeshift, "变形时揭发");
			ModuleManager.autoExposeImpostors.ExposeOnPhantom = GUILayout.Toggle(ModuleManager.autoExposeImpostors.ExposeOnPhantom, "隐身时揭发");
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

			PlayerTask task = PlayerControl.LocalPlayer.myTasks[0];
			if(task == null)
			{
				Hydra.notifications.Send("耗尽捉迷藏计时", "该功能要求你至少拥有一个任务。");
				yield break;
			}

			// If the HnS hide timer is up, all our tasks are replaced with a task of ImportantTextTask
			NormalPlayerTask normalTask = task.TryCast<NormalPlayerTask>();
			if(normalTask == null)
			{
				Hydra.notifications.Send("耗尽捉迷藏计时", "该功能无法在最后的躲藏时间中使用。");
				yield break;
			}

			float completeDeduction;
			switch(normalTask.Length)
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

			int totalCompletions = 0;
			int requiredCompletions = (int)Math.Ceiling(gameFlow.currentHideTime / completeDeduction);

			Hydra.Log.LogInfo($"Current escape time is {gameFlow.currentHideTime} and each task completion reduces the timer by {completeDeduction}s. We need to send the CompleteTask RPC {requiredCompletions} times to deplete the HnS timer.");

			while(totalCompletions < requiredCompletions)
			{
				BatchedMessage batch = new BatchedMessage();

				// The message packing limit for non-hosts should be ten, but the Among Us anticheat disconnects us if we have more than six CompleteTask RPCs in a single batch
				for(byte i = 0; i < 6; i++)
				{
					batch.QueueCompleteTask(PlayerControl.LocalPlayer, (uint)task.Index);
				}

				batch.FinishBatch();

				// Each batch will contain exactly six CompleteTask RPCs, which may be more than the required task completions, but that is fine
				totalCompletions += 6;

				yield return Effects.Wait(0.05f);
			}
		}
	}
}