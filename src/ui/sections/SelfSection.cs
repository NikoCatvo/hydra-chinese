using AmongUs.Data;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HydraMenu.assets;
using HydraMenu.modules;
using HydraMenu.network;
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
				GUILayout.Label("你当前不在游戏中，这些选项将无法使用。");
			}
			else
			{
				GUILayout.Label($"角色: {PlayerControl.LocalPlayer.Data.RoleType}");
			}

			ModuleManager.alwaysShowTaskAnimations.Enabled = GUILayout.Toggle(ModuleManager.alwaysShowTaskAnimations.Enabled, "始终显示任务动画");
			ModuleManager.immortality.Enabled = GUILayout.Toggle(ModuleManager.immortality.Enabled, "获得不死之身");
			ModuleManager.noLadderCooldown.Enabled = GUILayout.Toggle(ModuleManager.noLadderCooldown.Enabled, "无梯子冷却");
			ModuleManager.noZiplineCooldown.Enabled = GUILayout.Toggle(ModuleManager.noZiplineCooldown.Enabled, "无滑索冷却");
			ModuleManager.unlimitedMeetings.Enabled = GUILayout.Toggle(ModuleManager.unlimitedMeetings.Enabled, "无限召集会议");
			ModuleManager.updateStatsFreeplay.Enabled = GUILayout.Toggle(ModuleManager.updateStatsFreeplay.Enabled, "自由模式下更新统计数据");

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
			if(GUILayout.Button("开始医疗扫描"))
			{
				RPCEmitter.SendSetScanner(true);
			}

			if(GUILayout.Button("完成医疗扫描"))
			{
				RPCEmitter.SendSetScanner(false);
			}
			GUILayout.EndHorizontal();

			Dictionary<string, TaskTypes> animations = MapAssets.GetAnimations();
			Controls.DrawButtonCell(animations, PlayAnimation, 2);

			GUILayout.Space(5);
			GUILayout.Label("角色控制:");
			if(GUILayout.Button("随机化角色"))
			{
				if(AmongUsClient.Instance.AmConnected)
				{
					Utilities.RandomizePlayer(true);

					Hydra.notifications.Send("角色随机化", "你的角色已在此局游戏中随机化。", 5);
				}
				else
				{
					Utilities.RandomizePlayer();

					Hydra.notifications.Send("角色随机化", "你的名称和角色已随机化。", 5);
				}
			}

			if(GUILayout.Button("随机化颜色"))
			{
				PlayerControl.LocalPlayer.CmdCheckColor((byte)Utilities.GetRandomUnusedColor());
			}

			if(GUILayout.Button("恢复角色"))
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

			Hydra.Log.LogInfo("正在完成所有任务...");
			foreach(PlayerTask task in allTasks)
			{
				if(task.IsComplete)
				{
					Hydra.Log.LogInfo($"任务 {task.Id} 已完成，跳过");
					continue;
				}

				Hydra.Log.LogInfo($"发送完成任务 RPC，任务 ID: {task.Id}");
				PlayerControl.LocalPlayer.RpcCompleteTask(task.Id);

				// 如果要完成超过六个任务，需要实现延迟
				// 否则原版反作弊会因违反速率限制而将我们踢出
				yield return Effects.Wait(0.05f);
			}

			Hydra.notifications.Send("任务完成器", "你的所有任务已完成。", 5);
		}

		public void PlayAnimation(TaskTypes task)
		{
			if(PlayerControl.LocalPlayer == null)
			{
				Hydra.notifications.Send("播放动画", "此选项只能在游戏中使用。");
				return;
			}

			if(ShipStatus.Instance == null)
			{
				Hydra.notifications.Send("播放动画", "必须存在 ShipStatus 实例才能使用此功能。");
				return;
			}

			RPCEmitter.SendPlayAnimation((byte)task);
		}
	}
}