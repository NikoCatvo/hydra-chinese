using HarmonyLib;
using Hazel;
using HydraMenu.ui.sections;
using Il2CppInterop.Runtime;
using InnerNet;
using System;
using System.Collections.Generic;

namespace HydraMenu.modules
{
	internal class EventCoordinator
	{
		// Game Events
		public static event Action OnGameStart;
		public static event Action OnGameLoad;
		public static event Action OnDisconnect;

		public static event Action OnMeetingEnd;
		public static event Action<Minigame> OnOpenMinigame;
		public static event Action<Ladder> OnUseLadder;
		public static event Action<ZiplineConsole> OnUseZipline;

		// Player Events
		public static event Action<PlayerControl, ClientData> OnPlayerJoin;
		public static event Action<ClientData, DisconnectReasons> OnPlayerDisconnect;
		public static event Action<PlayerControl, string> OnPlayerChat;

		public static event Action<PlayerControl, byte> OnPlayerEnterVent;
		public static event Action<PlayerControl, byte> OnPlayerExitVent;
		public static event Action<PlayerControl, byte, byte> OnPlayerMoveVent;

		public static event Action<PlayerControl> OnPlayerEnterCameras;
		public static event Action<PlayerControl> OnPlayerExitCameras;

		public static event Action<PlayerControl, PlayerControl, MurderResultFlags> OnPlayerMurder;
		public static event Action<PlayerControl, PlayerControl, bool> OnPlayerShapeshift;
		public static event Action<PlayerControl> OnPlayerPhantom;

		public static event Action<ClientData, ClientData> OnPlayerVotekick;

		public static event Action<NetworkedPlayerInfo, NetworkedPlayerInfo> OnPlayerCastVote;

		// Network Events
		public static event Action<InnerNetObject> OnNetObjectSpawn;

		private static readonly HashSet<Il2CppSystem.Type> ShipNetObjects = [Il2CppType.From(typeof(ShipStatus)), Il2CppType.From(typeof(SkeldShipStatus)), Il2CppType.From(typeof(MiraShipStatus)), Il2CppType.From(typeof(PolusShipStatus)), Il2CppType.From(typeof(AirshipStatus)), Il2CppType.From(typeof(FungleShipStatus))];

		[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.CoStartGame))]
		class GameStart
		{
			static void Prefix()
			{
				PublishEvent(OnGameStart);
			}
		}

		// 此函数在角色选择屏幕结束、游戏准备就绪时调用
		[HarmonyPatch(typeof(GameManager), nameof(GameManager.StartGame))]
		class GameLoad
		{
			static void Prefix()
			{
				PublishEvent(OnGameLoad);
			}
		}

		[HarmonyPatch(typeof(GameData), nameof(GameData.OnDisconnected))]
		class Disconnect
		{
			static void Prefix()
			{
				Hydra.Log.LogInfo("[Disconnect Logger] Our player was disconnected from the lobby");

				HostSection.lobbyList.Clear();
				HostSection.shipList.Clear();

				PublishEvent(OnDisconnect);
			}
		}

		[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Close))]
		class MeetingEnd
		{
			static void Prefix()
			{
				PublishEvent(OnMeetingEnd);
			}
		}

		[HarmonyPatch(typeof(Minigame), nameof(Minigame.Begin))]
		class MinigameOpen
		{
			static void Prefix(Minigame __instance)
			{
				Hydra.Log.LogMessage($"类型为 {__instance.GetIl2CppType().Name} 的小游戏已打开");

				PublishEvent(OnOpenMinigame, __instance);
			}
		}

		// 此函数足够晚，允许我们修改梯子冷却而不被游戏覆盖
		[HarmonyPatch(typeof(Ladder), nameof(Ladder.SetDestinationCooldown))]
		class LadderUsed
		{
			static void Postfix(Ladder __instance)
			{
				Hydra.Log.LogMessage($"梯子 {__instance.Id} 被使用");

				PublishEvent(OnUseLadder, __instance.Destination);
			}
		}

		// 此函数足够晚，允许我们修改梯子冷却而不被游戏覆盖
		// 存在 ZiplineConsole::SetDestinationCooldown 方法，但由于它被内联了，我们无法对其打补丁
		[HarmonyPatch(typeof(ZiplineBehaviour), nameof(ZiplineBehaviour.ResetTarget))]
		class ZiplineUsed
		{
			static void Postfix(ZiplineBehaviour __instance)
			{
				ZiplineConsole console = __instance.lastUsedConsole;
				if(console == null) return;

				Hydra.Log.LogMessage("滑索" + (__instance.lastUsedConsole.atTop ? "在顶部" : "在底部") + "被使用");

				PublishEvent(OnUseZipline, console);
			}
		}

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Start))]
		class PlayerJoin
		{
			static void Postfix(PlayerControl __instance)
			{
				if(__instance == PlayerControl.LocalPlayer || AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay) return;

				ClientData clientData = AmongUsClient.Instance.GetClientFromCharacter(__instance);
				if(clientData != null)
				{
					PlatformSpecificData platformData = clientData.PlatformData;
					Hydra.Log.LogMessage($"[玩家日志] {clientData.PlayerName} ({__instance.NetId}) 在 {platformData.Platform} 上加入。好友码 {clientData.FriendCode}，PUID {clientData.ProductUserId}");
				}
				else
				{
// 应尽可能使用 NetworkedPlayerInfo::PlayerName 而非 PlayerControl::name 来获取玩家名称
				// 然而如果 PlayerControl 对象刚生成，则不太可能已生成 NetworkedPlayerInfo 对象
					Hydra.Log.LogMessage($"[玩家日志] {__instance.name} ({__instance.NetId}) 已加入。");
				}

				PublishEvent(OnPlayerJoin, __instance, clientData);
			}
		}

		[HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerLeft))]
		class PlayerDisconnect
		{
			static void Prefix(ClientData data, DisconnectReasons reason)
			{
				if(data.Character == null) return;

				Hydra.Log.LogInfo($"[断开日志] {data.Character.Data.PlayerName} 因原因 {reason} 断开连接");

				PublishEvent(OnPlayerDisconnect, data, reason);
			}
		}

		[HarmonyPatch(typeof(ChatController), nameof(ChatController.AddChat))]
		class PlayerChat
		{
			static void Prefix(PlayerControl sourcePlayer, string chatText)
			{
				Hydra.Log.LogMessage($"[聊天日志] {sourcePlayer.Data.PlayerName}: {chatText}");

				PublishEvent(OnPlayerChat, sourcePlayer, chatText);
			}
		}

		[HarmonyPatch(typeof(VentilationSystem), nameof(VentilationSystem.Deserialize))]
		class PlayerVentNonHost
		{
			static void Prefix(VentilationSystem __instance, MessageReader reader) {
				int oldReadPosition = reader.Position;

				int ventCleans = reader.ReadPackedInt32();
				if(ventCleans > PlayerControl.AllPlayerControls.Count || ventCleans > reader.BytesRemaining) goto end;

// 跳过通风管道清理数据的读取
			// 1 字节玩家 ID，1 字节通风管道 ID，所以需要跳过 2 * 通风清理次数
				reader.Position += 2 * ventCleans;

				int ventedPlayers = reader.ReadPackedInt32();
				if(ventedPlayers > PlayerControl.AllPlayerControls.Count || ventedPlayers > reader.BytesRemaining) goto end;

				Dictionary<byte, byte> ventData = new Dictionary<byte, byte>();
				for(int i = 0; i < ventedPlayers; i++)
				{
					byte playerId = reader.ReadByte();
					byte ventId = reader.ReadByte();

					ventData[playerId] = ventId;
				}

				// Compare with what we have with new data to see vent changes
				foreach(PlayerControl player in PlayerControl.AllPlayerControls)
				{
					byte playerId = player.PlayerId;

					bool inOld = __instance.PlayersInsideVents.TryGetValue(playerId, out byte oldVent);
					bool inNew = ventData.TryGetValue(playerId, out byte newVent);

					if(!inOld && inNew)
					{
						Hydra.Log.LogMessage($"{player.Data.PlayerName} 进入通风管道 {newVent}");
						PublishEvent(OnPlayerEnterVent, player, newVent);
					}
					else if(inOld && !inNew)
					{
						Hydra.Log.LogMessage($"{player.Data.PlayerName} 离开通风管道 {oldVent}");
						PublishEvent(OnPlayerExitVent, player, oldVent);
					}
					else if(oldVent != newVent)
					{
						Hydra.Log.LogMessage($"{player.Data.PlayerName} 从通风管道 {oldVent} 移动到 {newVent}");
						PublishEvent(OnPlayerMoveVent, player, oldVent, newVent);
					}
				}

				end:
				reader.Position = oldReadPosition;
			}
		}

		[HarmonyPatch(typeof(VentilationSystem), nameof(VentilationSystem.UpdateSystem))]
		class PlayerVentHost
		{
			static void Prefix(VentilationSystem __instance, PlayerControl player, MessageReader msgReader)
			{
				int oldReadPosition = msgReader.Position;

				msgReader.ReadUInt16(); // Sequence ID
				VentilationSystem.Operation operation = (VentilationSystem.Operation)msgReader.ReadByte();
				byte ventId = msgReader.ReadByte();

				switch(operation)
				{
					case VentilationSystem.Operation.Enter:
						PublishEvent(OnPlayerEnterVent, player, ventId);
						break;

					case VentilationSystem.Operation.Exit:
						PublishEvent(OnPlayerExitVent, player, ventId);
						break;

					case VentilationSystem.Operation.Move:
						byte oldVent = __instance.PlayersInsideVents[player.PlayerId];
						PublishEvent(OnPlayerMoveVent, player, oldVent, ventId);
						break;
				}

				msgReader.Position = oldReadPosition;
			}
		}

		[HarmonyPatch(typeof(SecurityCameraSystemType), nameof(SecurityCameraSystemType.UpdateSystem))]
		class UpdateCamerasHost
		{
			static void Postfix(PlayerControl player, MessageReader msgReader)
			{
				msgReader.Position--;
				// 1 = 玩家开始观看摄像头，2（及其他所有值）= 玩家停止观看摄像头
				byte operation = msgReader.ReadByte();
				msgReader.Position++;

				if(operation == 1)
				{
					PublishEvent(OnPlayerEnterCameras, player);
				}
				else
				{
					PublishEvent(OnPlayerExitCameras, player);
				}
			}
		}

		[HarmonyPatch(typeof(SecurityCameraSystemType), nameof(SecurityCameraSystemType.Deserialize))]
		class UpdateCamerasNonHost
		{
			static void Prefix(SecurityCameraSystemType __instance, MessageReader reader)
			{
				int oldReadPosition = reader.Position;

				int playerCount = reader.ReadPackedInt32();
				if(playerCount > PlayerControl.AllPlayerControls.Count || playerCount > reader.BytesRemaining) goto end;

				HashSet<byte> players = new HashSet<byte>();

				for(int i = 0; i < playerCount; i++)
				{
					byte playerId = reader.ReadByte();
					players.Add(playerId);
				}

				foreach(PlayerControl player in PlayerControl.AllPlayerControls)
				{
					bool inOld = __instance.PlayersUsing.Contains(player.PlayerId);
					bool inNew = players.Contains(player.PlayerId);

					if(!inOld && inNew)
					{
						PublishEvent(OnPlayerEnterCameras, player);
					}
					else if(inOld && !inNew)
					{
						PublishEvent(OnPlayerExitCameras, player);
					}
				}

				end:
				reader.Position = oldReadPosition;
			}
		}

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
		class PlayerMurder
		{
			static void Prefix(PlayerControl __instance, PlayerControl target, MurderResultFlags resultFlags)
			{
				PublishEvent(OnPlayerMurder, __instance, target, resultFlags);
			}
		}

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.Shapeshift))]
		class PlayerShapeshift
		{
			static void Prefix(PlayerControl __instance, PlayerControl targetPlayer, bool animate)
			{
				PublishEvent(OnPlayerShapeshift, __instance, targetPlayer, animate);
			}
		}

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleServerVanish))]
		class PlayerPhantom
		{
			static void Prefix(PlayerControl __instance)
			{
				PublishEvent(OnPlayerPhantom, __instance);
			}
		}

		[HarmonyPatch(typeof(VoteBanSystem), nameof(VoteBanSystem.AddVote))]
		class PlayerVotekick
		{
			static void Postfix(int srcClient, int clientId)
			{
				Hydra.Log.LogInfo($"[踢人日志] {srcClient} 投票踢出 {clientId}");

				ClientData source = AmongUsClient.Instance.FindClientById(srcClient);
				ClientData target = AmongUsClient.Instance.FindClientById(clientId);
				if(source == null || target == null) return;

				if(clientId == AmongUsClient.Instance.ClientId)
				{
					Hydra.notifications.Send("投票踢人日志", $"{source.PlayerName} 投票将你踢出。");
				}

				PublishEvent(OnPlayerVotekick, source, target);
			}
		}

		[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.CastVote))]
		class PlayerCastVote
		{
			static void Postfix(PlayerId srcPlayerId, PlayerId suspectPlayerId)
			{
				NetworkedPlayerInfo voter = GameData.Instance.GetPlayerById(srcPlayerId);
				NetworkedPlayerInfo votee = GameData.Instance.GetPlayerById(suspectPlayerId);
				if(voter == null || votee == null) return;

				PublishEvent(OnPlayerCastVote, voter, votee);
			}
		}

		[HarmonyPatch(typeof(InnerNetObjectCollection), nameof(InnerNetObjectCollection.TryAddNetObject))]
		class NetObjectAdd
		{
			static void Postfix(InnerNetObject obj)
			{
				if(obj == null) return;

				Il2CppSystem.Type type = obj.GetIl2CppType();

				if(type == Il2CppType.From(typeof(LobbyBehaviour)))
				{
					HostSection.lobbyList.Enqueue(obj);
				}
				else if(ShipNetObjects.Contains(type))
				{
					HostSection.shipList.Enqueue(obj);
				}

				PublishEvent(OnNetObjectSpawn, obj);
			}
		}

		// These functions are to simplify having null-checks everywhere
		// 我知道可以用 evt?.Invoke 来避免检查事件是否为空，我只是不喜欢那种代码风格
		private static void PublishEvent(Action evt)
		{
			if(evt == null) return;
			evt();
		}

		private static void PublishEvent<T1>(Action<T1> evt, T1 arg1)
		{
			if(evt == null) return;
			evt(arg1);
		}

		private static void PublishEvent<T1, T2>(Action<T1, T2> evt, T1 arg1, T2 arg2)
		{
			if(evt == null) return;
			evt(arg1, arg2);
		}

		private static void PublishEvent<T1, T2, T3>(Action<T1, T2, T3> evt, T1 arg1, T2 arg2, T3 arg3)
		{
			if(evt == null) return;
			evt(arg1, arg2, arg3);
		}
	}
}