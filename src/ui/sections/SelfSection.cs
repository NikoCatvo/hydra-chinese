using AmongUs.Data;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HydraMenu.assets;
using HydraMenu.modules;
using HydraMenu.network;
using InnerNet;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class SelfSection : Section
	{
		public SelfSection() : base("自身") { }

		public override void Render()
		{
			if(PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null)
			{
				GUILayout.Label("你当前不在对局中，这些选项将无法使用。");
			}
			else
			{
				GUILayout.Label($"角色：{Lang.Tr(PlayerControl.LocalPlayer.Data.RoleType)}");
			}

			ModuleManager.alwaysShowTaskAnimations.Enabled = GUILayout.Toggle(ModuleManager.alwaysShowTaskAnimations.Enabled, "始终显示任务动画");
			ModuleManager.immortality.Enabled = GUILayout.Toggle(ModuleManager.immortality.Enabled, "无敌");
			ModuleManager.noLadderCooldown.Enabled = GUILayout.Toggle(ModuleManager.noLadderCooldown.Enabled, "无梯子冷却");
			ModuleManager.noZiplineCooldown.Enabled = GUILayout.Toggle(ModuleManager.noZiplineCooldown.Enabled, "无滑索冷却");
			ModuleManager.unlimitedMeetings.Enabled = GUILayout.Toggle(ModuleManager.unlimitedMeetings.Enabled, "无限会议");
			ModuleManager.updateStatsFreeplay.Enabled = GUILayout.Toggle(ModuleManager.updateStatsFreeplay.Enabled, "在自由模式中更新数据");

			if(GUILayout.Button("召开会议"))
			{
				Utilities.AttemptStartMeeting(PlayerControl.LocalPlayer, null);
			}

			if(GUILayout.Button("完成所有任务"))
			{
				PlayerControl.LocalPlayer.StartCoroutine(CompleteAllTasks().WrapToIl2Cpp());
			}

			GUILayout.Label("任务动画：");
			GUILayout.BeginHorizontal();
			if(GUILayout.Button("开始医疗扫描"))
			{
				SetScanner(true);
			}

			if(GUILayout.Button("结束医疗扫描"))
			{
				SetScanner(false);
			}
			GUILayout.EndHorizontal();

			Dictionary<string, TaskTypes> animations = MapAssets.GetAnimations();
			Controls.DrawButtonCell(animations, PlayAnimation, 2);

			GUILayout.Space(5);
			GUILayout.Label("外观控制：");
			if(GUILayout.Button("随机外观"))
			{
				if(AmongUsClient.Instance.AmConnected)
				{
					Utilities.RandomizePlayer(true);

					Hydra.notifications.Send("随机外观", "本局你的外观已随机化。", 5);
				}
				else
				{
					Utilities.RandomizePlayer();

					Hydra.notifications.Send("随机外观", "你的名字和外观已随机化。", 5);
				}
			}

			if(GUILayout.Button("随机颜色"))
			{
				PlayerControl.LocalPlayer.CmdCheckColor((byte)Utilities.GetRandomUnusedColor());
			}

			if(GUILayout.Button("恢复外观"))
			{
				PlayerControl.LocalPlayer.CmdCheckColor(DataManager.Player.Customization.Color);
				PlayerControl.LocalPlayer.RpcSetHat(DataManager.Player.Customization.Hat);
				PlayerControl.LocalPlayer.RpcSetVisor(DataManager.Player.Customization.Visor);
				PlayerControl.LocalPlayer.RpcSetSkin(DataManager.Player.Customization.Skin);
				PlayerControl.LocalPlayer.RpcSetPet(DataManager.Player.Customization.Pet);
			}
		}

		private IEnumerator CompleteAllTasks()
		{
			Il2CppSystem.Collections.Generic.List<PlayerTask> allTasks = PlayerControl.LocalPlayer.myTasks;

			Hydra.Log.LogInfo("Completing all tasks...");
			foreach(PlayerTask task in allTasks)
			{
				if(task.IsComplete)
				{
					Hydra.Log.LogInfo($"Task {task.Id} has already been completed, skipping");
					continue;
				}

				Hydra.Log.LogInfo($"Sent CompleteTask RPC for task {task.Id}");
				PlayerControl.LocalPlayer.RpcCompleteTask(task.Id);

				// If we want to complete more than six tasks then a delay needs to be implemented
				// otherwise the vanilla anticheat will kick us for violating ratelimits
				yield return Effects.Wait(0.05f);
			}

			Hydra.notifications.Send("任务完成", "你的所有任务已完成。", 5);
		}

		private void SetScanner(bool scanning)
		{
			if(PlayerControl.LocalPlayer == null)
			{
				Hydra.notifications.Send("设置扫描仪", "该选项只能在游戏中使用。");
				return;
			}

			if(Utilities.IsAnticheatPresent() && AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
			{
				Hydra.notifications.Send("设置扫描仪", "需要游戏已经开始，该功能才能生效。");
				return;
			}

			BatchedMessage batch = new BatchedMessage();
			batch.QueueSetScanner(PlayerControl.LocalPlayer, scanning);
			batch.FinishBatch();
		}

		private void PlayAnimation(TaskTypes task)
		{
			if(PlayerControl.LocalPlayer == null)
			{
				Hydra.notifications.Send("播放动画", "该选项只能在游戏中使用。");
				return;
			}

			if(ShipStatus.Instance == null)
			{
				Hydra.notifications.Send("播放动画", "需要存在 ShipStatus 实例，该功能才能生效。");
				return;
			}

			if(Utilities.IsAnticheatPresent() && AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
			{
				Hydra.notifications.Send("播放动画", "需要游戏已经开始，该功能才能生效。");
				return;
			}

			RPCEmitter.SendPlayAnimation((byte)task);
		}
	}
}