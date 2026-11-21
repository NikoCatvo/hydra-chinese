using AmongUs.Data;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HydraMenu.assets;
using HydraMenu.modules;
using HydraMenu.network;
using HydraMenu.ui;
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
				// Wait for player data to fully load
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
			playerName += $"\n<color=\"{GetRoleColor(player.Data.RoleType)}\">{Lang.Tr(player.Data.RoleType)}</color>";

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

			// If we want to get a player's name, we have to use NetworkedPlayerInfo::PlayerName instead of PlayerControl::name to avoid
			// getting the incorrect name if the player is shapeshifted to another player
			string playerInfo =
				$"名字：{target.Data.PlayerName}（{Lang.Tr(Utilities.GetPlayerColor(target.Data))}）" +
				$"\n角色：{Lang.Tr(target.Data.RoleType)}" +
				$"\n状态：" + (target.Data.IsDead ? "死亡" : "存活");

			ClientData clientData = AmongUsClient.Instance.GetClientFromCharacter(target);
			if(clientData != null)
			{
				PlatformSpecificData platform = clientData.PlatformData;

				bool streamerMode = DataManager.Settings.Gameplay.StreamerMode;

				playerInfo +=
					$"\n好友代码：" + (streamerMode ? "已隐藏" : target.Data.FriendCode) +
					$"\nPUID：" + (streamerMode ? "已隐藏" : target.Data.Puid) +
					$"\n等级：{target.Data.PlayerLevel + 1}" +
					$"\n设备：{GameAssets.GetPlatformName(platform.Platform)}" +
					(target.OwnerId == AmongUsClient.Instance.HostId ? "\n房主：是" : "");
			}

			GUILayout.Label(playerInfo);

			ModuleManager.spectatePlayer.Enabled = Controls.PlayerSpecificToggle("观战", target, ref ModuleManager.spectatePlayer.target);
			Hydra.routines.petPlayer.Enabled = Controls.PlayerSpecificToggle("抚摸玩家", target, ref Hydra.routines.petPlayer.target);
			Hydra.routines.playerFollower.Enabled = Controls.PlayerSpecificToggle("跟随", target, ref Hydra.routines.playerFollower.target);
			Hydra.routines.jailPlayer.Enabled = Controls.PlayerSpecificToggle("关进监狱", target, Hydra.routines.jailPlayer.targets);
			Hydra.routines.teleportSpammer.Enabled = Controls.PlayerSpecificToggle("刷屏传送", target, Hydra.routines.teleportSpammer.targets);

			if(!hasAnticheat)
			{
				Hydra.routines.ziplineSpammer.Enabled = Controls.PlayerSpecificToggle("刷屏滑索", target, Hydra.routines.ziplineSpammer.targets);
			}

			ModuleManager.whisper.Enabled = Controls.PlayerSpecificToggle("私聊", target, ref ModuleManager.whisper.target);

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("传送"))
			{
				// We do not want to use PlayerControl::GetTruePosition() here as it would teleport us to the player's feet
				Teleporter.TeleportTo(target.transform.position);
			}

			if(!hasAnticheat && GUILayout.Button("传送到我这"))
			{
				Teleporter.TeleportPlayerTo(target, PlayerControl.LocalPlayer.transform.position);
			}
			GUILayout.EndHorizontal();

			if(!hasAnticheat && GUILayout.Button("全部传送到"))
			{
				Teleporter.TeleportAllTo(target.transform.position);
			}

			if(GUILayout.Button("击杀"))
			{
				AttemptMurder(target);
			}

			if(GUILayout.Button("复制外观"))
			{
				Utilities.CopyPlayer(target);
			}

			if(GUILayout.Button("报告尸体"))
			{
				Utilities.AttemptStartMeeting(PlayerControl.LocalPlayer, target.Data);
			}

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("投票踢人"))
			{
				VoteBanSystem.Instance.CmdAddVote(target.OwnerId);

				Hydra.notifications.Send("投票踢人", $"{target.Data.PlayerName} 已被投票踢出。（{Utilities.GetVotekickCount(VoteBanSystem.Instance, target.OwnerId)}/3）");
			}

			if(GUILayout.Button("踢出玩家"))
			{
				Utilities.KickPlayer(target);
			}
			GUILayout.EndHorizontal();

			SortedDictionary<int, string> vents = MapAssets.GetVents();

			GUILayout.Label($"把玩家传送到管道：{vents.GetValueOrDefault(selectedVent, "无")}");
			selectedVent = Controls.HorizontalVentSlider(vents, selectedVent);
			if(GUILayout.Button("传送"))
			{
				Teleporter.TeleportToVent(target, selectedVent);
			}

			GUILayout.Space(5);
			GUILayout.Label("仅房主功能：" + (AmongUsClient.Instance.AmHost ? "" : "\n（使用这些功能会让你被踢出！）"));

			ModuleManager.autoReportBodies.Enabled = Controls.PlayerSpecificToggle("自动报告尸体为", target, ref ModuleManager.autoReportBodies.target);
			Hydra.routines.discoHost.Enabled = Controls.PlayerSpecificToggle("迪斯科模式", target, Hydra.routines.discoHost.targets);
			ModuleManager.voteImmune.Enabled = Controls.PlayerSpecificToggle("投票免疫", target, ModuleManager.voteImmune.targets);
			Hydra.routines.voteSpammer.Enabled = Controls.PlayerSpecificToggle("刷屏投票为", target, Hydra.routines.voteSpammer.targets);

			if(GUILayout.Button("强制召开会议为"))
			{
				Utilities.AttemptStartMeeting(target, null);
			}

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("把所有票投给"))
			{
				ForceAllVotesTo(target);
			}

			if(GUILayout.Button("放逐"))
			{
				EjectPlayer(target);
			}
			GUILayout.EndHorizontal();

			if(GUILayout.Button("栽赃变形"))
			{
				PlayerControl randomPl = Utilities.GetRandomPlayer(false, false, false, false);
				Utilities.ShapeshiftPlayer(target, randomPl);
			}

			if(GUILayout.Button("栽赃杀死所有人"))
			{
				target.StartCoroutine(AttemptFrameForKillingAll(target).WrapToIl2Cpp());
			}

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("给玩家刷满任务"))
			{
				byte[] tasks = new byte[255];

				for(byte i = 0; i < 255; i++)
				{
					tasks[i] = i;
				}

				AssignTasks(target, tasks);
			}

			if(GUILayout.Button("清除任务"))
			{
				AssignTasks(target, Array.Empty<byte>());
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(5);
			GUILayout.Label("游戏选项修改：");

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
			if(GUILayout.Button("慢速"))
			{
				IGameOptions gameOptions = GameOptions.CreateCloneOptions(GameManager.Instance.LogicOptions.currentGameOptions);
				gameOptions.SetFloat(FloatOptionNames.PlayerSpeedMod, 0.1f);

				GameOptions.SendGameOptionsToClient(gameOptions, target.OwnerId);
			}

			if(GUILayout.Button("超速"))
			{
				// The vanilla anticheat prevents us from being able to exceed speeds greater than 3.0f
				float maxSpeed = Utilities.IsAnticheatPresent() ? 3.0f : 5.0f;

				IGameOptions gameOptions = GameOptions.CreateCloneOptions(GameManager.Instance.LogicOptions.currentGameOptions);
				gameOptions.SetFloat(FloatOptionNames.PlayerSpeedMod, maxSpeed);

				GameOptions.SendGameOptionsToClient(gameOptions, target.OwnerId);
			}
			GUILayout.EndHorizontal();

			if(GUILayout.Button("冻结"))
			{
				IGameOptions gameOptions = GameOptions.CreateCloneOptions(GameManager.Instance.LogicOptions.currentGameOptions);
				gameOptions.SetFloat(FloatOptionNames.PlayerSpeedMod, 0.1e-10f);

				GameOptions.SendGameOptionsToClient(gameOptions, target.OwnerId);
			}

			/*
			// The problem with changing the TaskBarMode is that if we remove the task bar, we are not able to bring it back
			GUILayout.BeginHorizontal();
			if(GUILayout.Button("Hide Task Bar"))
			{
				IGameOptions gameOptions = GameOptions.CreateCloneOptions(GameManager.Instance.LogicOptions.currentGameOptions);
				gameOptions.SetInt(Int32OptionNames.TaskBarMode, (int)TaskBarMode.Invisible);

				GameOptions.SendGameOptionsToClient(gameOptions, target.OwnerId);
			}

			if(GUILayout.Button("Show Task Bar"))
			{
				IGameOptions gameOptions = GameOptions.CreateCloneOptions(GameManager.Instance.LogicOptions.currentGameOptions);
				gameOptions.SetInt(Int32OptionNames.TaskBarMode, (int)TaskBarMode.Normal);

				GameOptions.SendGameOptionsToClient(gameOptions, target.OwnerId);
			}
			GUILayout.EndHorizontal();
			*/

			if(GUILayout.Button("恢复默认"))
			{
				IGameOptions gameOptions = GameOptions.CreateCloneOptions(GameManager.Instance.LogicOptions.currentGameOptions);
				GameOptions.SendGameOptionsToClient(gameOptions, target.OwnerId);
			}

			GUILayout.Space(5);
			GUILayout.Label($"更改颜色为：{Lang.Tr(selectedColor)}");
			selectedColor = Controls.HorizontalColorSlider(selectedColor);

			if(GUILayout.Button("设置颜色"))
			{
				target.RpcSetColor((byte)selectedColor);
			}
		}

		private void AssignTasks(PlayerControl target, byte[] tasks)
		{
			bool hasAnticheat = Utilities.IsAnticheatPresent();
			if(hasAnticheat && !AmongUsClient.Instance.AmHost)
			{
				Hydra.notifications.Send("分配任务", "只有房主才能使用该功能。");
				return;
			}

			target.Data.RpcSetTasks(tasks);

			Hydra.notifications.Send("分配任务", $"已为 {target.Data.PlayerName} 分配 {tasks.Length} 个任务。", 5f);
		}

		private void AttemptMurder(PlayerControl target)
		{
			bool hasAnticheat = Utilities.IsAnticheatPresent();

			if(hasAnticheat && AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
			{
				Hydra.notifications.Send("击杀玩家", $"只有游戏开始后才能击杀玩家。");
				return;
			}

			if(AmongUsClient.Instance.AmHost)
			{
				Hydra.Log.LogInfo($"Attempting to murder {target.Data.PlayerName}, we are the host so we can use the MurderPlayer RPC");
				PlayerControl.LocalPlayer.RpcMurderPlayer(target, true);
				Hydra.notifications.Send("击杀玩家", $"已击杀 {target.Data.PlayerName}。", 5);
				return;
			}

			if(!hasAnticheat)
			{
				Hydra.Log.LogInfo($"Attempting to murder {target.Data.PlayerName}, we are are in a host-authoritative lobby so we can use the MurderPlayer RPC");
				PlayerControl.LocalPlayer.RpcMurderPlayer(target, true);
				Hydra.notifications.Send("击杀玩家", $"已击杀 {target.Data.PlayerName}。", 5);
				return;
			}

			Hydra.Log.LogInfo($"Attempting to kill {target.Data.PlayerName}, we are not the host so we have to use the CheckMurder RPC");

			// The CheckMurder RPC handler will not authorize kills if you are not the imposter, or you are inside a meeting
			// There are more checks, but I do not think it is worth adding them all here
			if(!RoleManager.IsImpostorRole(PlayerControl.LocalPlayer.Data.RoleType))
			{
				Hydra.notifications.Send("击杀玩家", "除非你是房间房主，否则只有内鬼才能击杀玩家。");
				return;
			}

			if(MeetingHud.Instance != null)
			{
				Hydra.notifications.Send("击杀玩家", "除非你是房间房主，否则只能在会议之外击杀玩家。");
				return;
			}

			Hydra.notifications.Send("击杀玩家", $"已尝试击杀 {target.Data.PlayerName}。", 5);
			PlayerControl.LocalPlayer.CmdCheckMurder(target);
		}

		private void ForceAllVotesTo(PlayerControl player)
		{
			if(Utilities.IsAnticheatPresent() && !AmongUsClient.Instance.AmHost)
			{
				Hydra.notifications.Send("强制投票", "你必须先成为房间房主才能使用该功能。");
				return;
			}

			if(MeetingHud.Instance == null)
			{
				Hydra.notifications.Send("强制投票", "该选项只能在有进行中的会议时使用。");
				return;
			}

			MeetingHud.VoterState[] array = new MeetingHud.VoterState[PlayerControl.AllPlayerControls.Count];

			for(int i = 0; i < array.Length; i++)
			{
				MeetingHud.VoterState state = array[i];

				state.VoterId = (byte)i;
				state.VotedForId = player.PlayerId;

				array[i] = state;
			}

			BatchedMessage batch = new BatchedMessage();
			batch.QueueVotingComplete(array, player.Data, false, false, 0);
			batch.FinishBatch();
		}

		private void EjectPlayer(PlayerControl player)
		{
			if(Utilities.IsAnticheatPresent() && !AmongUsClient.Instance.AmHost)
			{
				Hydra.notifications.Send("放逐玩家", "你必须先成为房间房主才能使用该功能。");
				return;
			}

			BatchedMessage batch = new BatchedMessage();

			if(MeetingHud.Instance == null)
			{
				MeetingHud.Instance = UnityEngine.Object.Instantiate<MeetingHud>(HudManager.Instance.MeetingPrefab);
				batch.QueueSpawn(MeetingHud.Instance, -2, SpawnFlags.None);
			}

			MeetingHud.VoterState[] votes = Array.Empty<MeetingHud.VoterState>();

			batch.QueueVotingComplete(votes, player.Data, false, false, 0);
			// If we created a MeetingHud object then it will be destroyed by the RpcClose function
			batch.QueueCloseMeeting();
			batch.FinishBatch();
		}

		private IEnumerator AttemptFrameForKillingAll(PlayerControl target)
		{
			Hydra.Log.LogInfo($"Attempting to frame {target.Data.PlayerName} for killing all players...");

			bool hasAnticheat = Utilities.IsAnticheatPresent();
			if(hasAnticheat && !AmongUsClient.Instance.AmHost)
			{
				Hydra.notifications.Send("栽赃", "你必须先成为房间房主才能使用该选项。");
				yield break;
			}

			if(hasAnticheat && AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
			{
				Hydra.notifications.Send("栽赃", "需要游戏已经开始，该选项才能生效。");
				yield break;
			}

			ModuleManager.disableGameEnd.Enabled = true;

			if(target != PlayerControl.LocalPlayer)
			{
				// On official servers, we are not able to send MurderPlayer RPCs with other player net IDs
				// so we need to shapeshift into our desired player and kill everyone ourselves
				Utilities.ShapeshiftPlayer(PlayerControl.LocalPlayer, target, false);
			}

			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				if(player == target) continue;

				PlayerControl.LocalPlayer.RpcMurderPlayer(player, true);
			}

			// Wait three seconds so all players can see which player we are framing
			yield return Effects.Wait(3.0f);

			ModuleManager.disableGameEnd.Enabled = false;
			Hydra.notifications.Send("栽赃", $"已栽赃 {target.Data.PlayerName} 杀死所有人！");
		}
	}
}
