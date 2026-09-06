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
				GUILayout.Label("你当前不在游戏中，这些选项将无法使用。");
			}

			ModuleManager.crashLobby.Enabled = GUILayout.Toggle(ModuleManager.crashLobby.Enabled, "排队大厅崩溃");
			ModuleManager.autoReportBodies.Enabled = Controls.PlayerSpecificToggle("自动报尸体", PlayerControl.LocalPlayer, ref ModuleManager.autoReportBodies.target);
			Hydra.routines.autoTriggerSpores.Enabled = GUILayout.Toggle(Hydra.routines.autoTriggerSpores.Enabled, "自动触发孢子");
			ModuleManager.disableCameras.Enabled = GUILayout.Toggle(ModuleManager.disableCameras.Enabled, "禁用监控摄像头");
			ModuleManager.disableCloseDoors.Enabled = GUILayout.Toggle(ModuleManager.disableCloseDoors.Enabled, "禁用关门");
			ModuleManager.disableSabotages.Enabled = GUILayout.Toggle(ModuleManager.disableSabotages.Enabled, "禁用破坏");
			ModuleManager.disableVents.Enabled = GUILayout.Toggle(ModuleManager.disableVents.Enabled, "禁用通风管道");
			Hydra.routines.glitterBomb.Enabled = GUILayout.Toggle(Hydra.routines.glitterBomb.Enabled, "亮片炸弹");
			Hydra.routines.ziplineSpammer.Enabled = Controls.GlobalPlayerSpecificToggle("滑索刷屏", Hydra.routines.ziplineSpammer.targets);

			if(GUILayout.Button("踢出所有玩家"))
			{
				Hydra.Log.LogInfo($"向所有玩家发送进入通风系统更新");

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
					Hydra.notifications.Send("触发孢子", "此选项仅在 Fungle 地图上有效。");
				}
				else
				{
					FungleShipStatus shipStatus = ShipStatus.Instance.Cast<FungleShipStatus>();

					foreach(Mushroom mushroom in shipStatus.sporeMushrooms.Values)
					{
						PlayerControl.LocalPlayer.RpcTriggerSpores(mushroom);
					}

					Hydra.notifications.Send("触发孢子", "所有孢子已触发。", 5);
				}
			}

			if(GUILayout.Button("耗尽躲藏计时器"))
			{
				AmongUsClient.Instance.StartCoroutine(DepleteSeekTimer().WrapToIl2Cpp());
			}

			SortedDictionary<int, string> vents = MapAssets.GetVents();

			GUILayout.Space(5);
			GUILayout.Label($"通风管道传送:");
			Hydra.routines.teleportSpammer.Enabled = Controls.GlobalPlayerSpecificToggle("传送泛滥", Hydra.routines.teleportSpammer.targets);

			GUILayout.Label($"将所有人传送到通风口: {vents.GetValueOrDefault(selectedVent, "不适用")}");
			selectedVent = Controls.HorizontalVentSlider(vents, selectedVent);

			if(GUILayout.Button("传送到通风口"))
			{
				foreach(PlayerControl player in PlayerControl.AllPlayerControls)
				{
					Teleporter.TeleportToVent(player, selectedVent);
				}
			}

			if(GUILayout.Button("传送到随机通风口"))
			{
				foreach(PlayerControl player in PlayerControl.AllPlayerControls)
				{
					if(player == PlayerControl.LocalPlayer) continue;

					int ventId = rnd.Next(0, ShipStatus.Instance.AllVents.Count);

					Teleporter.TeleportToVent(player, ventId);
				}
			}

			GUILayout.Space(5);
			// 以设定间隔自动关闭并打开所有门
			GUILayout.Label("门恶搞:");
			Hydra.routines.doorTroller.Enabled = GUILayout.Toggle(Hydra.routines.doorTroller.Enabled, "启用");

			GUILayout.Label($"锁定和解锁延迟: {Hydra.routines.doorTroller.LockAndUnlockDelay:F2}秒");
			Hydra.routines.doorTroller.LockAndUnlockDelay = GUILayout.HorizontalSlider(Hydra.routines.doorTroller.LockAndUnlockDelay, 0.1f, 2.0f);

			GUILayout.Space(5);
			GUILayout.Label("自动曝光内鬼:");
			ModuleManager.autoExposeImpostors.Enabled = GUILayout.Toggle(ModuleManager.autoExposeImpostors.Enabled, "启用");
			ModuleManager.autoExposeImpostors.ExposeOnMurder = GUILayout.Toggle(ModuleManager.autoExposeImpostors.ExposeOnMurder, "击杀时曝光");
			ModuleManager.autoExposeImpostors.ExposeOnShapeshift = GUILayout.Toggle(ModuleManager.autoExposeImpostors.ExposeOnShapeshift, "变身时曝光");
			ModuleManager.autoExposeImpostors.ExposeOnPhantom = GUILayout.Toggle(ModuleManager.autoExposeImpostors.ExposeOnPhantom, "幻影时曝光");
		}

		// 在躲猫猫模式中，完成任务会根据任务长度（短、普通、长）减少躲藏计时器
		// 问题在于即使我们已经完成了任务，游戏仍会减少任务计时器
		// 所以我们可以用同一个任务刷 CompleteTask RPC，将任务计时器减少到零秒
		private IEnumerator DepleteSeekTimer()
		{
			if(!GameManager.Instance.IsHideAndSeek())
			{
				Hydra.notifications.Send("耗尽躲藏计时器", "此功能仅可在躲猫猫模式中使用。");
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
				Hydra.notifications.Send("耗尽躲藏计时器", "此功能要求你至少有一个任务。");
				yield break;
			}

			// 如果躲猫猫躲藏计时器结束，我们所有任务都会被替换为 ImportantTextTask
			NormalPlayerTask normalTask = task.TryCast<NormalPlayerTask>();
			if(normalTask == null)
			{
				Hydra.notifications.Send("耗尽躲藏计时器", "此功能不能在最后躲藏时间使用。");
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

			Hydra.Log.LogInfo($"当前逃生时间为 {gameFlow.currentHideTime}，每次任务完成减少计时器 {completeDeduction} 秒。我们需要发送 CompleteTask RPC {requiredCompletions} 次来耗尽躲猫猫计时器。");

			while(totalCompletions < requiredCompletions)
			{
				BatchedMessage batch = new BatchedMessage();

				// 非房主的消息打包限制应为十，但 Among Us 反作弊会在单个批次中超过六个 CompleteTask RPC 时将我们断开连接
				for(byte i = 0; i < 6; i++)
				{
					batch.QueueCompleteTask(PlayerControl.LocalPlayer, (uint)task.Index);
				}

				batch.FinishBatch();

				// 每个批次恰好包含六个 CompleteTask RPC，可能超过所需任务完成次数，但这没问题
				totalCompletions += 6;

				yield return Effects.Wait(0.05f);
			}
		}
	}
}