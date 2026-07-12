using AmongUs.Data;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HydraMenu.assets;
using HydraMenu.features;
using HydraMenu.network;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class SelfSection : ISection
	{
		public SelfSection() : base("自身") { }

		public override void Render()
		{
			if(PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null)
			{
				GUILayout.Label("你当前不在游戏中，这些选项将无法生效。");
			}
			else
			{
				GUILayout.Label($"身份: {PlayerControl.LocalPlayer.Data.RoleType}");
			}

			Self.UpdateStatsFreeplay.Enabled = GUILayout.Toggle(Self.UpdateStatsFreeplay.Enabled, "在自由模式中更新统计数据");
			Immortality.Enabled = GUILayout.Toggle(Immortality.Enabled, "无敌");
			Self.AlwaysShowTaskAnimations = GUILayout.Toggle(Self.AlwaysShowTaskAnimations, "始终显示任务动画");
			Self.NoLadderCooldown.Enabled = GUILayout.Toggle(Self.NoLadderCooldown.Enabled, "无梯子冷却");
			Self.UnlimitedMeetings.enabled = GUILayout.Toggle(Self.UnlimitedMeetings.enabled, "无限会议");

			if(GUILayout.Button("召集会议"))
			{
				Utilities.AttemptStartMeeting(PlayerControl.LocalPlayer, null);
			}

			if(GUILayout.Button("完成所有任务"))
			{
				PlayerControl.LocalPlayer.StartCoroutine(CompleteAllTasks().WrapToIl2Cpp());
			}

			GUILayout.Label("任务动画:");
			GUILayout.BeginHorizontal();
			if(GUILayout.Button("开始医疗舱扫描"))
			{
				RPCEmitter.SendSetScanner(true);
			}

			if(GUILayout.Button("结束医疗舱扫描"))
			{
				RPCEmitter.SendSetScanner(false);
			}
			GUILayout.EndHorizontal();

			Dictionary<string, TaskTypes> animations = MapAssets.GetAnimations();
			Controls.DrawButtonCell(animations, PlayAnimation, 2);

			GUILayout.Space(5);
			GUILayout.Label("形象控制:");
			if(GUILayout.Button("随机化形象"))
			{
				if(AmongUsClient.Instance.AmConnected)
				{
					Utilities.RandomizePlayer(true);

					Hydra.notifications.Send("形象随机器", "你的形象已在本局游戏中随机化。", 5);
				}
				else
				{
					Utilities.RandomizePlayer();

					Hydra.notifications.Send("形象随机器", "你的名字和形象已被随机化。", 5);
				}
			}

			if(GUILayout.Button("随机化颜色"))
			{
				PlayerControl.LocalPlayer.CmdCheckColor((byte)Utilities.GetRandomUnusedColor());
			}

			if(GUILayout.Button("还原形象"))
			{
				PlayerControl.LocalPlayer.CmdCheckColor(DataManager.Player.Customization.Color);
				PlayerControl.LocalPlayer.RpcSetHat(DataManager.Player.Customization.Hat);
				PlayerControl.LocalPlayer.RpcSetVisor(DataManager.Player.Customization.Visor);
				PlayerControl.LocalPlayer.RpcSetSkin(DataManager.Player.Customization.Skin);
				PlayerControl.LocalPlayer.RpcSetPet(DataManager.Player.Customization.Pet);
			}
		}

		public IEnumerator CompleteAllTasks()
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

				yield return Effects.Wait(0.05f);
			}

			Hydra.notifications.Send("任务完成器", "你的所有任务已完成。", 5);
		}

		public void PlayAnimation(TaskTypes task)
		{
			if(PlayerControl.LocalPlayer == null)
			{
				Hydra.notifications.Send("播放动画", "此选项只能在游戏内使用。");
				return;
			}

			if(ShipStatus.Instance == null)
			{
				Hydra.notifications.Send("播放动画", "此功能需要存在 ShipStatus 实例才能生效。");
				return;
			}

			RPCEmitter.SendPlayAnimation((byte)task);
		}
	}
}
