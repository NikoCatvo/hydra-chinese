using AmongUs.Data;
using AmongUs.GameOptions;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using HydraMenu.features;
using HydraMenu.network;
using InnerNet;
using System;
using System.Collections;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class PlayersSection : ISection
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

		private static Controls.PlayerColors selectedColor = Controls.PlayerColors.Red;
		private static int selectedVent = 0;

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
				style.normal.textColor = new Color(1.0f, 0.84f, 0.0f);
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

		private static void RenderPlayerControls(PlayerControl target)
		{
			if(target == null || target.Data == null)
			{
				GUILayout.Label("指定的目标无效。");
				return;
			}

			bool hasAnticheat = Utilities.IsAnticheatPresent();

			string playerInfo =
				$"名字: {target.Data.PlayerName} ({Utilities.GetPlayerColor(target.Data)})" +
				$"\n身份: {target.Data.RoleType}" +
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

			Hydra.routines.playerFollower.Enabled = Controls.PlayerSpecificToggle("跟随", target, ref Hydra.routines.playerFollower.following);
			Hydra.routines.jailPlayer.Enabled = Controls.PlayerSpecificToggle("关进监狱", target, ref Hydra.routines.jailPlayer.targets);

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("传送到"))
			{
				HydraMenu.Teleporter.TeleportTo(target.transform.position);
			}

			if(!hasAnticheat && GUILayout.Button("传送到我这"))
			{
				HydraMenu.Teleporter.TeleportPlayerTo(target, PlayerControl.LocalPlayer.transform.position);
			}
			GUILayout.EndHorizontal();

			if(!hasAnticheat && GUILayout.Button("将所有人传送到此"))
			{
				HydraMenu.Teleporter.TeleportAllTo(target.transform.position);
			}

			if(GUILayout.Button("击杀"))
			{
				AttemptMurder(target);
			}

			if(GUILayout.Button("复制形象"))
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

			GUILayout.Label($"将玩家传送到通风口: {selectedVent}");
			selectedVent = (int)GUILayout.HorizontalSlider(selectedVent, 0, (ShipStatus.Instance != null ? ShipStatus.Instance.AllVents.Count - 1 : 10));
			if(GUILayout.Button("传送"))
			{
				HydraMenu.Teleporter.TeleportToVent(target, selectedVent);
			}

			GUILayout.Space(5);
			GUILayout.Label("仅房主可用功能:" + (AmongUsClient.Instance.AmHost ? "" : "\n(使用这些会导致你被踢出！)"));

			Troll.AutoReportBodies.Enabled = Controls.PlayerSpecificToggle("以此身份自动报告尸体", target, ref Troll.AutoReportBodies.source);
			Hydra.routines.discoHost.Enabled = Controls.PlayerSpecificToggle("蹦迪模式", target, ref Hydra.routines.discoHost.targets);

			if(GUILayout.Button("以此身份强制召集会议"))
			{
				Utilities.AttemptStartMeeting(target, null);
			}

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("强制所有票投给"))
			{
				if(MeetingHud.Instance == null)
				{
					Hydra.notifications.Send("投票强制器", "此选项只能在有进行中的会议时使用。");
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
					batch.QueueVotingComplete(array, target.Data, false);
					batch.FinishBatch();
				}
			}

			if(GUILayout.Button("放逐"))
			{
				BatchedMessage batch = new BatchedMessage();

				if(MeetingHud.Instance == null)
				{
					MeetingHud.Instance = UnityEngine.Object.Instantiate<MeetingHud>(HudManager.Instance.MeetingPrefab);
					batch.QueueSpawn(MeetingHud.Instance, -2, SpawnFlags.None);
				}

				MeetingHud.VoterState[] votes = Array.Empty<MeetingHud.VoterState>();

				batch.QueueVotingComplete(votes, target.Data, false);
				batch.QueueCloseMeeting();
				batch.FinishBatch();
			}
			GUILayout.EndHorizontal();

			if(GUILayout.Button("嫁祸变形"))
			{
				PlayerControl randomPl = Utilities.GetRandomPlayer(false, false, false, false);
				Utilities.ShapeshiftPlayer(target, randomPl);
			}

			if(GUILayout.Button("嫁祸其杀光所有人"))
			{
				target.StartCoroutine(AttemptFrameForKillingAll(target).WrapToIl2Cpp());
			}

			GUILayout.BeginHorizontal();
			if(GUILayout.Button("给玩家灌满任务"))
			{
				byte[] taskIds = new byte[255];

				for(byte i = 0; i < 255; i++)
				{
					taskIds[i] = i;
				}

				target.Data.RpcSetTasks(taskIds);
			}

			if(GUILayout.Button("清空任务"))
			{
				target.Data.RpcSetTasks(Array.Empty<byte>());
			}
			GUILayout.EndHorizontal();

			GUILayout.Space(5);
			GUILayout.Label("游戏选项修改器:");

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

			if(GUILayout.Button("超速"))
			{
				float maxSpeed = Utilities.IsAnticheatPresent() ? 3.0f : 5.0f;

				IGameOptions gameOptions = GameOptions.CreateCloneOptions(GameManager.Instance.LogicOptions.currentGameOptions);
				gameOptions.SetFloat(FloatOptionNames.PlayerSpeedMod, maxSpeed);

				GameOptions.SendGameOptionsToClient(gameOptions, target.OwnerId);
			}
			GUILayout.EndHorizontal();

			if(GUILayout.Button("重置为默认"))
			{
				IGameOptions gameOptions = GameOptions.CreateCloneOptions(GameManager.Instance.LogicOptions.currentGameOptions);
				GameOptions.SendGameOptionsToClient(gameOptions, target.OwnerId);
			}

			GUILayout.Space(5);
			GUILayout.Label($"改变颜色为: {selectedColor}");
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

			if(!RoleManager.IsImpostorRole(PlayerControl.LocalPlayer.Data.RoleType))
			{
				Hydra.notifications.Send("击杀玩家", "除非你是房间的房主，否则只有身为内鬼时才能击杀玩家。");
				return;
			}

			if(MeetingHud.Instance != null)
			{
				Hydra.notifications.Send("击杀玩家", "除非你是房间的房主，否则只能在会议之外击杀玩家。");
				return;
			}

			Hydra.notifications.Send("击杀玩家", $"已尝试击杀 {target.Data.PlayerName}。", 5);
			PlayerControl.LocalPlayer.CmdCheckMurder(target);
		}

		private static IEnumerator AttemptFrameForKillingAll(PlayerControl target)
		{
			Hydra.Log.LogInfo($"Attempting to frame {target.Data.PlayerName} for killing all players...");

			bool hasAnticheat = Utilities.IsAnticheatPresent();
			if(hasAnticheat && !AmongUsClient.Instance.AmHost)
			{
				Hydra.notifications.Send("嫁祸者", "你必须是房间的房主才能使用此选项。");
				yield break;
			}

			if(hasAnticheat && AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
			{
				Hydra.notifications.Send("嫁祸者", "游戏必须已经开始，此选项才能生效。");
				yield break;
			}

			Host.DisableGameEnd.Enabled = true;

			if(target != PlayerControl.LocalPlayer)
			{
				Utilities.ShapeshiftPlayer(PlayerControl.LocalPlayer, target, false);
			}

			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				if(player == target) continue;

				PlayerControl.LocalPlayer.RpcMurderPlayer(player, true);
			}

			yield return Effects.Wait(3.0f);

			Host.DisableGameEnd.Enabled = false;
			Hydra.notifications.Send("嫁祸者", $"已嫁祸 {target.Data.PlayerName} 杀光所有玩家！");
		}
	}
}
