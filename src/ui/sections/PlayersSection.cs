using AmongUs.Data;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HydraMenu.assets;
using HydraMenu.modules;
using HydraMenu.network;
using InnerNet;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class PlayersSection : Section
	{
		public PlayersSection() : base("玩家") { }

		public static Vector2 PlayerPaneSize
		{
			get { return new Vector2(100 * MainUI.scale, MainUI.WindowSize.y - MainUI.HeaderSize.y); }
		}

		public static Vector2 PlayerPanePosition
		{
			get { return new Vector2(MainUI.SectionListPosition.x + MainUI.SectionListSize.x, MainUI.HeaderSize.y + MainUI.HeaderPosition.y); }
		}

		public static Vector2 PlayerButtonSize
		{
			get { return new Vector2(PlayerPaneSize.x, 30 * MainUI.scale); }
		}

		public static Vector2 PlayerOptionsSize
		{
			get { return new Vector2(MainUI.WindowSize.x - MainUI.SectionListSize.x - PlayerPaneSize.x, MainUI.WindowSize.y - MainUI.HeaderSize.y); }
		}

		public static Vector2 PlayerOptionsPosition
		{
			get { return new Vector2(PlayerPanePosition.x + PlayerPaneSize.x, MainUI.HeaderPosition.y + MainUI.HeaderSize.y); }
		}

		public static Vector2 PlayerColorBoxSize
		{
			get { return new Vector2(5 * MainUI.scale, PlayerButtonSize.y); }
		}

		public static PlayerControl selectedPlayer;
		private Vector2 subsectionScrollVector;

		private Controls.PlayerColors selectedColor = Controls.PlayerColors.Red;
		private int selectedVent = 0;

		public override void HandleSubsectionMove(int offset)
		{
			if(PlayerControl.AllPlayerControls.Count == 0) return;

			int currentPlayer = PlayerControl.AllPlayerControls.IndexOf(selectedPlayer);
			int newPosition = Math.Clamp(currentPlayer + offset, 0, PlayerControl.AllPlayerControls.Count - 1);

			selectedPlayer = PlayerControl.AllPlayerControls[newPosition];
		}

		public override void Render()
		{
			if(PlayerControl.AllPlayerControls.Count == 0)
			{
				GUILayout.Label("当前没有在线玩家。");
				return;
			}

			GUI.Box(new Rect(0, 0, PlayerPaneSize.x, PlayerPaneSize.y), "", Styles.MainBox);

			for(byte i = 0; i < PlayerControl.AllPlayerControls.Count; i++)
			{
				PlayerControl player = PlayerControl.AllPlayerControls[i];
				// 等待玩家数据完全加载
				if(player.Data == null) continue;

				RenderPlayerSelection(i, player);

				if(player == selectedPlayer)
				{
					GUILayout.BeginArea(new Rect(PlayerPaneSize.x, 0, PlayerOptionsSize.x, PlayerOptionsSize.y));
					subsectionScrollVector = GUILayout.BeginScrollView(subsectionScrollVector);

					RenderPlayerControls(player);

					GUILayout.EndScrollView();
					GUILayout.EndArea();
				}
			}
		}

		private void RenderPlayerSelection(byte position, PlayerControl player)
		{
			Rect playerInfo = new Rect(0, position * PlayerButtonSize.y, PlayerButtonSize.x, PlayerButtonSize.y);

			string playerName = player.Data.PlayerName;
			playerName += $"\n<color=\"{GetRoleColor(player.Data.RoleType)}\">{player.Data.RoleType}</color>";

			GUIStyle style = player == selectedPlayer ? Styles.PlayerBoxActive : Styles.PlayerBox;

			if(player.OwnerId == AmongUsClient.Instance.HostId)
			{
				style.normal.textColor = new Color(1.0f, 0.84f, 0.0f); // #FFD700
			}

			if(GUI.Button(playerInfo, playerName, style))
			{
				selectedPlayer = player;
			}

			Rect playerColor = new Rect(0, position * PlayerButtonSize.y, PlayerColorBoxSize.x, PlayerColorBoxSize.y);
			Controls.DrawCrewmateColorBox(playerColor, player.Data);
		}

		private string GetRoleColor(RoleTypes role)
		{
			return RoleManager.IsImpostorRole(role) ? "red" : "#8afcfc";
		}

		private void RenderPlayerControls(PlayerControl target)
		{
			if(target == null || target.Data == null)
			{
				GUILayout.Label("指定的目标无效。");
				return;
			}

			bool hasAnticheat = Utilities.IsAnticheatPresent();

			// 如果我们要获取玩家姓名，必须使用 NetworkedPlayerInfo::PlayerName 而非 PlayerControl::name，避免
			// 在玩家变身为另一名玩家时获取到错误的姓名
			string playerInfo =
				$"名称: {target.Data.PlayerName} ({Utilities.GetPlayerColor(target.Data)})" +
				$"\n角色: {target.Data.RoleType}" +
				$"\n状态: " + (target.Data.IsDead ? "死亡" : "存活");

			ClientData clientData = AmongUsClient.Instance.GetClientFromCharacter(target);
			if(clientData != null)
			{
				PlatformSpecificData platform = clientData.PlatformData;

				bool streamerMode = DataManager.Settings.Gameplay.StreamerMode;

				playerInfo +=
					$"\n好友码: " + (streamerMode ? "已隐藏" : target.Data.FriendCode) +
					$"\nPUID: " + (streamerMode ? "已隐藏" : target.Data.Puid) +
					$"\n等级: {target.Data.PlayerLevel + 1}" +
					$"\n设备: {platform.Platform}" +
					(target.OwnerId == AmongUsClient.Instance.HostId ? "\n房主: 是" : "");
			}

			GUILayout.Label(playerInfo);

			ModuleManager.spectatePlayer.Enabled = Controls.PlayerSpecificToggle("观战", target, ref ModuleManager.spectatePlayer.target);
			Hydra.routines.petPlayer.Enabled = Controls.PlayerSpecificToggle("宠物玩家", target, ref Hydra.routines.petPlayer.target);
			Hydra.routines.playerFollower.Enabled = Controls.PlayerSpecificToggle("跟随", target, ref Hydra.routines.playerFollower.target);
			Hydra.routines.jailPlayer.Enabled = Controls.PlayerSpecificToggle("关入牢房", target, Hydra.routines.jailPlayer.targets);
			Hydra.routines.teleportSpammer.Enabled = Controls.PlayerSpecificToggle("传送刷屏", target, Hydra.routines.teleportSpammer.targets);
			Hydra.routines.ziplineSpammer.Enabled = Controls.PlayerSpecificToggle("滑索刷屏", target, Hydra.routines.ziplineSpammer.targets);

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("传送"))
			{
				// 我们不在这里使用 PlayerControl::GetTruePosition()，因为它会把我们传送到玩家的脚底
				Teleporter.TeleportTo(target.transform.position);
			}

			if(!hasAnticheat && GUILayout.Button("传送到我这"))
			{
				Teleporter.TeleportPlayerTo(target, PlayerControl.LocalPlayer.transform.position);
			}
			GUILayout.EndHorizontal();

			if(!hasAnticheat && GUILayout.Button("将所有人传送至"))
			{
				Teleporter.TeleportAllTo(target.transform.position);
			}

			if(GUILayout.Button("击杀"))
			{
				AttemptMurder(target);
			}

			if(GUILayout.Button("复制角色"))
			{
				Utilities.CopyPlayer(target);
			}

			if(GUILayout.Button("报告尸体"))
			{
				Utilities.AttemptStartMeeting(PlayerControl.LocalPlayer, target.Data);
			}

			if(GUILayout.Button("踢出玩家"))
			{
				Utilities.KickPlayer(target);
			}

			SortedDictionary<int, string> vents = MapAssets.GetVents();

			GUILayout.Label($"将玩家传送到通风口: {vents.GetValueOrDefault(selectedVent, "不适用")}");
			selectedVent = Controls.HorizontalVentSlider(vents, selectedVent);
			if(GUILayout.Button("传送"))
			{
				Teleporter.TeleportToVent(target, selectedVent);
			}

			GUILayout.Space(5);
			GUILayout.Label("仅房主功能:" + (AmongUsClient.Instance.AmHost ? "" : "\n(使用这些会被踢出!)"));

			ModuleManager.autoReportBodies.Enabled = Controls.PlayerSpecificToggle("以该玩家自动报尸体", target, ref ModuleManager.autoReportBodies.target);
			Hydra.routines.discoHost.Enabled = Controls.PlayerSpecificToggle("迪斯科模式", target, Hydra.routines.discoHost.targets);
			ModuleManager.voteImmune.Enabled = Controls.PlayerSpecificToggle("投票免疫", target, ModuleManager.voteImmune.targets);
			Hydra.routines.voteSpammer.Enabled = Controls.PlayerSpecificToggle("以该玩家刷投票", target, Hydra.routines.voteSpammer.targets);

			if(GUILayout.Button("以该玩家强制召开会议"))
			{
				Utilities.AttemptStartMeeting(target, null);
			}

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("强制所有投票指向"))
			{
				if(MeetingHud.Instance == null)
				{
					Hydra.notifications.Send("投票强制器", "此选项只能在有活跃会议时使用。");
				}
				else
				{
					MeetingHud.VoterState[] array = new MeetingHud.VoterState[PlayerControl.AllPlayerControls.Count];

					for(int i = 0; i < array.Length; i++)
					{
						MeetingHud.VoterState state = array[i];

						state.VoterId = (byte)i;
						state.VotedForId = target.PlayerId;

						array[i] = state;
					}

					BatchedMessage batch = new BatchedMessage();
					batch.QueueVotingComplete(array, target.Data, false, false, 0);
					batch.FinishBatch();
				}
			}

			if(GUILayout.Button("驱逐"))
			{
				BatchedMessage batch = new BatchedMessage();

				if(MeetingHud.Instance == null)
				{
					MeetingHud.Instance = UnityEngine.Object.Instantiate<MeetingHud>(HudManager.Instance.MeetingPrefab);
					batch.QueueSpawn(MeetingHud.Instance, -2, SpawnFlags.None);
				}

				MeetingHud.VoterState[] votes = Array.Empty<MeetingHud.VoterState>();

				batch.QueueVotingComplete(votes, target.Data, false, false, 0);
				// 如果我们创建了 MeetingHud 对象，它会被 RpcClose 函数销毁
				batch.QueueCloseMeeting();
				batch.FinishBatch();
			}
			GUILayout.EndHorizontal();

			if(GUILayout.Button("伪装变身"))
			{
				PlayerControl randomPl = Utilities.GetRandomPlayer(false, false, false, false);
				Utilities.ShapeshiftPlayer(target, randomPl);
			}

			if(GUILayout.Button("栽赃其击杀所有人"))
			{
				target.StartCoroutine(AttemptFrameForKillingAll(target).WrapToIl2Cpp());
			}

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("以任务淹没玩家"))
			{
				byte[] taskIds = new byte[255];

				for(byte i = 0; i < 255; i++)
				{
					taskIds[i] = i;
				}

				target.Data.RpcSetTasks(taskIds);
			}

			if(GUILayout.Button("清除任务"))
			{
				target.Data.RpcSetTasks(Array.Empty<byte>());
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(5);
			GUILayout.Label("游戏选项修改:");

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("致盲"))
			{
				IGameOptions gameOptions = GameOptions.CreateCloneOptions(GameManager.Instance.LogicOptions.currentGameOptions);
				gameOptions.SetFloat(FloatOptionNames.CrewLightMod, -1.0f);
				gameOptions.SetFloat(FloatOptionNames.ImpostorLightMod, -1.0f);

				GameOptions.SendGameOptionsToClient(gameOptions, target.OwnerId);
			}

			if(GUILayout.Button("全亮"))
			{
				IGameOptions gameOptions = GameOptions.CreateCloneOptions(GameManager.Instance.LogicOptions.currentGameOptions);
				gameOptions.SetFloat(FloatOptionNames.CrewLightMod, 1000f);
				gameOptions.SetFloat(FloatOptionNames.ImpostorLightMod, 1000f);

				GameOptions.SendGameOptionsToClient(gameOptions, target.OwnerId);
			}
			GUILayout.EndHorizontal();

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("减速"))
			{
				IGameOptions gameOptions = GameOptions.CreateCloneOptions(GameManager.Instance.LogicOptions.currentGameOptions);
				gameOptions.SetFloat(FloatOptionNames.PlayerSpeedMod, 0.1f);

				GameOptions.SendGameOptionsToClient(gameOptions, target.OwnerId);
			}

			if(GUILayout.Button("极速"))
			{
				// 原版反作弊阻止我们超过 3.0f 的速度
				float maxSpeed = Utilities.IsAnticheatPresent() ? 3.0f : 5.0f;

				IGameOptions gameOptions = GameOptions.CreateCloneOptions(GameManager.Instance.LogicOptions.currentGameOptions);
				gameOptions.SetFloat(FloatOptionNames.PlayerSpeedMod, maxSpeed);

				GameOptions.SendGameOptionsToClient(gameOptions, target.OwnerId);
			}
			GUILayout.EndHorizontal();

			/*
			// 更改 TaskBarMode 的问题在于移除任务栏后我们无法恢复它
			GUILayout.BeginHorizontal();
			if(GUILayout.Button("隐藏任务栏"))
			{
				IGameOptions gameOptions = GameOptions.CreateCloneOptions(GameManager.Instance.LogicOptions.currentGameOptions);
				gameOptions.SetInt(Int32OptionNames.TaskBarMode, (int)TaskBarMode.Invisible);

				GameOptions.SendGameOptionsToClient(gameOptions, target.OwnerId);
			}

			if(GUILayout.Button("显示任务栏"))
			{
				IGameOptions gameOptions = GameOptions.CreateCloneOptions(GameManager.Instance.LogicOptions.currentGameOptions);
				gameOptions.SetInt(Int32OptionNames.TaskBarMode, (int)TaskBarMode.Normal);

				GameOptions.SendGameOptionsToClient(gameOptions, target.OwnerId);
			}
			GUILayout.EndHorizontal();
			*/

			if(GUILayout.Button("重置为默认值"))
			{
				IGameOptions gameOptions = GameOptions.CreateCloneOptions(GameManager.Instance.LogicOptions.currentGameOptions);
				GameOptions.SendGameOptionsToClient(gameOptions, target.OwnerId);
			}

			GUILayout.Space(5);
			GUILayout.Label($"改颜色为: {selectedColor}");
			selectedColor = Controls.HorizontalColorSlider(selectedColor);

			if(GUILayout.Button("设置颜色"))
			{
				target.RpcSetColor((byte)selectedColor);
			}
		}

		private static void AttemptMurder(PlayerControl target)
		{
			bool hasAnticheat = Utilities.IsAnticheatPresent();

			if(hasAnticheat && AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
			{
				Hydra.notifications.Send("击杀玩家", $"游戏开始后才能击杀玩家。");
				return;
			}

			if(AmongUsClient.Instance.AmHost)
			{
				Hydra.Log.LogInfo($"尝试击杀 {target.Data.PlayerName}，我们是房主，可以使用 MurderPlayer RPC");
				PlayerControl.LocalPlayer.RpcMurderPlayer(target, true);
				Hydra.notifications.Send("击杀玩家", $"已击杀 {target.Data.PlayerName}。", 5);
				return;
			}

			if(!hasAnticheat)
			{
				Hydra.Log.LogInfo($"尝试击杀 {target.Data.PlayerName}，我们在房主主导的大厅中，可以使用 MurderPlayer RPC");
				PlayerControl.LocalPlayer.RpcMurderPlayer(target, true);
				Hydra.notifications.Send("击杀玩家", $"已击杀 {target.Data.PlayerName}。", 5);
				return;
			}

			Hydra.Log.LogInfo($"尝试击杀 {target.Data.PlayerName}，我们不是房主，必须使用 CheckMurder RPC");

			// CheckMurder RPC 处理器不会授权非内鬼或在会议中的击杀
			// 还有更多检查，但我觉得没必要全部加在这里
			if(!RoleManager.IsImpostorRole(PlayerControl.LocalPlayer.Data.RoleType))
			{
				Hydra.notifications.Send("击杀玩家", "只有你是内鬼时才能击杀玩家，除非你是大厅房主。");
				return;
			}

			if(MeetingHud.Instance != null)
			{
				Hydra.notifications.Send("击杀玩家", "只能在会议外向玩家执行击杀，除非你是大厅房主。");
				return;
			}

			Hydra.notifications.Send("击杀玩家", $"已尝试击杀 {target.Data.PlayerName}。", 5);
			PlayerControl.LocalPlayer.CmdCheckMurder(target);
		}

		private static IEnumerator AttemptFrameForKillingAll(PlayerControl target)
		{
			Hydra.Log.LogInfo($"尝试栽赃 {target.Data.PlayerName} 击杀所有玩家...");

			bool hasAnticheat = Utilities.IsAnticheatPresent();
			if(hasAnticheat && !AmongUsClient.Instance.AmHost)
			{
				Hydra.notifications.Send("栽赃者", "你必须是大厅房主才能使用此选项。");
				yield break;
			}

			if(hasAnticheat && AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
			{
				Hydra.notifications.Send("栽赃者", "游戏必须已开始才能使用此选项。");
				yield break;
			}

			ModuleManager.disableGameEnd.Enabled = true;

			if(target != PlayerControl.LocalPlayer)
			{
				// 在官方服务器上，我们无法使用其他玩家的网络 ID 发送 MurderPlayer RPC
				// 所以我们需要先变身成目标玩家，再亲自击杀所有人
				Utilities.ShapeshiftPlayer(PlayerControl.LocalPlayer, target, false);
			}

			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				if(player == target) continue;

				PlayerControl.LocalPlayer.RpcMurderPlayer(player, true);
			}

			// 等待三秒，让所有玩家看清我们栽赃的是谁
			yield return Effects.Wait(3.0f);

			ModuleManager.disableGameEnd.Enabled = false;
			Hydra.notifications.Send("栽赃者", $"已将 {target.Data.PlayerName} 栽赃为击杀所有玩家的凶手!");
		}
	}
}