using AmongUs.InnerNet.GameDataMessages;
using HarmonyLib;
using Hazel;
using HydraMenu.anticheat.gamedata;
using HydraMenu.anticheat.rpc;
using System;
using System.Collections.Generic;

namespace HydraMenu.anticheat
{
	internal class Anticheat
	{
		public static bool Enabled { get; set; } = true;

		public static readonly Dictionary<GameDataTypes, GameDataCheck> GameDataHandlers = new Dictionary<GameDataTypes, GameDataCheck>()
		{
			{ GameDataTypes.SceneChangeFlag, new SceneChange() },
			{ GameDataTypes.ReadyFlag, new ClientReady() }
		};

		public static readonly Dictionary<RpcCalls, RpcCheck> RpcHandlers = new Dictionary<RpcCalls, RpcCheck>()
		{
			// 此字典中的 RPC 处理程序应按其 RPC ID 排序
			{ RpcCalls.PlayAnimation, new PlayAnimation() },
			{ RpcCalls.CompleteTask, new CompleteTask() },
			{ RpcCalls.Exiled, new Exiled() },
			{ RpcCalls.CheckName, new CheckName() },
			{ RpcCalls.SetName, new SetName() },
			{ RpcCalls.SetColor, new SetColor() },
			{ RpcCalls.ReportDeadBody, new ReportDeadBody() },
			{ RpcCalls.SetScanner, new SetScanner() },
			{ RpcCalls.SetStartCounter, new SetStartCounter() },
			{ RpcCalls.EnterVent, new EnterVent() },
			{ RpcCalls.ExitVent, new ExitVent() },
			{ RpcCalls.SnapTo, new SnapTo() },
			{ RpcCalls.AddVote, new AddVote() },
			{ RpcCalls.CloseDoorsOfType, new CloseDoorsOfType() },
			{ RpcCalls.ClimbLadder, new ClimbLadder() },
			{ RpcCalls.UsePlatform, new UsePlatform() },
			{ RpcCalls.UpdateSystem, new UpdateSystem() },
			{ RpcCalls.SetLevel, new SetLevel() }
		};

		public static bool CheckSpoofedPlatforms { get; set; } = true;

		public enum Punishments
		{
			None,
			Kick,
			ErrorKick,
			Ban
		}

		public static float NotificationDuration = 10.0f;

		public static Punishments punishment = Punishments.None;
		public static bool sendNotification = true;
		public static bool discardRpc = true;

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.HandleRpc))]
		class OnPlayerControlRPC
		{
			static bool Prefix(PlayerControl __instance, byte callId, MessageReader reader)
			{
				return HandleRpc(typeof(PlayerControl), __instance, (RpcCalls)callId, reader);
			}
		}

		[HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.HandleRpc))]
		class OnPlayerPhysicsRPC
		{
			static bool Prefix(PlayerPhysics __instance, byte callId, MessageReader reader)
			{
				return HandleRpc(typeof(PlayerPhysics), __instance.myPlayer, (RpcCalls)callId, reader);
			}
		}

		[HarmonyPatch(typeof(CustomNetworkTransform), nameof(CustomNetworkTransform.HandleRpc))]
		class OnNetTransformRPC
		{
			static bool Prefix(CustomNetworkTransform __instance, byte callId, MessageReader reader)
			{
				return HandleRpc(typeof(CustomNetworkTransform), __instance.myPlayer, (RpcCalls)callId, reader);
			}
		}

		[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.HandleRpc))]
		class OnShipStatusRPC
		{
			static bool Prefix(byte callId, MessageReader reader)
			{
				return HandleRpc(typeof(ShipStatus), null, (RpcCalls)callId, reader);
			}
		}

		private static bool HandleRpc(Type sourceNetObj, PlayerControl player, RpcCalls rpc, MessageReader reader)
		{
			RpcHandlers.TryGetValue(rpc, out RpcCheck rpcCheck);
			if(!Enabled || rpcCheck == null || !rpcCheck.Enabled) return true;

			if(sourceNetObj != rpcCheck.GetExpectedNetObject())
			{
				// 收到本应发送给不同网络对象的 RPC，某种漏洞利用尝试？
				return false;
			}

			// 只有我们（房主）应该发送仅房主 RPC
			if(player != null && AmongUsClient.Instance.AmHost && rpcCheck.IsHostOnly())
			{
				Flag(player, $"{player.Data.PlayerName} 非房主却发送了 {rpc} RPC。");
				return false;
			}

			int oldReadPosition = reader.Position;

			bool isValid = rpcCheck.Validate(player, reader);

			// 将读取位置恢复到之前的位置，以免搞乱 HandleRpc 函数或其他补丁
			reader.Position = oldReadPosition;

			return isValid || !discardRpc;
		}

		public static bool HandleGameData(GameDataTypes type, MessageReader reader)
		{
			GameDataHandlers.TryGetValue(type, out GameDataCheck gameDataCheck);
			if(!Enabled || gameDataCheck == null || !gameDataCheck.Enabled) return true;

			int oldReadPosition = reader.Position;

			bool isValid = gameDataCheck.Validate(reader);

			// 将读取位置恢复到之前的位置
			reader.Position = oldReadPosition;

			return isValid || !discardRpc;
		}

		public static void Flag(PlayerControl player, string reason, bool shouldPunish = true)
		{
			// 理智检查，确保我们没有标记自己
			// 在没有网络对象冒充检查的服务器上，可能会代表房主发送无效 RPC
			// 这会导致 Hydra 反作弊标记自己并将自己从自己的大厅封禁
			if(player == PlayerControl.LocalPlayer) return;

			if(sendNotification)
			{
				Hydra.notifications.Send("反作弊", reason, NotificationDuration);
			}

			if(AmongUsClient.Instance.AmHost && shouldPunish)
			{
				Punish(player);
			}
		}

		// 如果我们不知道是哪个玩家导致的违规
		public static void Flag(string reason)
		{
			if(sendNotification)
			{
				Hydra.notifications.Send("反作弊", reason, NotificationDuration);
			}
		}

		private static void Punish(PlayerControl player)
		{
			switch(punishment)
			{
				case Punishments.None:
					break;

				case Punishments.Kick:
				case Punishments.ErrorKick:
					Hydra.Log.LogMessage($"{player.Data.PlayerName} 因作弊被 Hydra 反作弊踢出");

					// 原版反作弊在游戏尚未开始时不允许使用 ErrorKick 方法
					if(punishment == Punishments.Kick || AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started)
					{
						AmongUsClient.Instance.KickPlayer(player.OwnerId, false);
					}
					else
					{
						// 当游戏开始时，房主会等待约十秒等待所有客户端发送 ClientReady 游戏消息
						// 如果在计时器结束前房主未收到玩家的 ClientReady 游戏消息，房主会因超时将玩家踢出
						// 玩家看到的踢出消息会解释其网络连接不佳或设备过旧
						// 而游戏内其他玩家会看到该玩家因错误离开，而不是被踢出
						// 除 ClientTimeout 外的任何其他断开连接消息都会导致原版反作弊将我们从大厅踢出
						AmongUsClient.Instance.SendLateRejection(player.OwnerId, DisconnectReasons.ClientTimeout);
					}
					break;

				case Punishments.Ban:
					Hydra.Log.LogMessage($"{player.Data.PlayerName} 因作弊被 Hydra 反作弊自动封禁");
					AmongUsClient.Instance.KickPlayer(player.OwnerId, true);
					break;
			}
		}

		public class AnticheatConfigData
		{
			public bool AcEnabled { get; set; }
			public bool SendNotification { get; set; }
			public bool DiscardRpc { get; set; }
			public Punishments Punishment { get; set; }
		}

		public static AnticheatConfigData GetConfigData()
		{
			return new AnticheatConfigData
			{
				AcEnabled = Enabled,
				SendNotification = sendNotification,
				DiscardRpc =  discardRpc,
				Punishment = punishment,
			};
		}

		public static void LoadConfigData(AnticheatConfigData configData)
		{
			if(configData == null) return;

			Enabled = configData.AcEnabled;
			sendNotification = configData.SendNotification;
			discardRpc = configData.DiscardRpc;
			punishment = configData.Punishment;
		}
	}
}