using Hazel;
using HydraMenu.network;
using System.Collections.Generic;
using UnityEngine;

namespace HydraMenu
{
	internal class Teleporter
	{
		// This exists for the same reason as why the UpdateSystemsDirectly option for sabotages exist
		public static bool UseSnapToRPC { get; set; } = true;

		public static readonly Dictionary<string, Vector2> skeldTeleportLocations = new Dictionary<string, Vector2>()
		{
			{ "自助餐厅", new Vector2(-0.78f, 2.48f) },
			{ "武器库", new Vector2(8.04f, 1.24f) },
			{ "医疗室", new Vector2(-8.61f, -4.30f) },
			{ "管理室", new Vector2(2.79f, -7.69f) },
			{ "氧气室", new Vector2(6.50f, -3.50f) },
			{ "导航室", new Vector2(16.77f, -4.67f) },
			{ "护盾室", new Vector2(9.26f, -12.19f) },
			{ "通讯室", new Vector2(5.10f, -15.24f ) },
			{ "储藏室", new Vector2(-3.72f, -14.61f ) },
			{ "电力室", new Vector2(-6.91f, -8.60f ) },
			{ "上引擎室", new Vector2(-17.61f, -0.72f) },
			{ "下引擎室", new Vector2(-17.33f, -13.10f ) },
			{ "安保室", new Vector2(-12.81f, -3.01f) },
			{ "反应堆", new Vector2(-20.53f, -5.39f) },
		};

		public static readonly Dictionary<string, Vector2> miraTeleportLocations = new Dictionary<string, Vector2>()
		{
			{ "发射台", new Vector2(-4.43f, 1.98f) },
			{ "医疗室", new Vector2(14.58f, 0.33f) },
			{ "通讯室", new Vector2(15.60f, 4.96f) },
			{ "更衣室", new Vector2(9.68f, 3.71f) },
			{ "净化室", new Vector2(6.12f, 6.34f) },
			{ "实验室", new Vector2(9.43f, 13.98f) },
			{ "反应堆", new Vector2(2.55f, 11.71f) },
			{ "办公室", new Vector2(14.68f, 20.63f) },
			{ "管理室", new Vector2(19.41f, 19.01f) },
			{ "温室", new Vector2(17.92f, 23.86f) },
			{ "自助餐厅", new Vector2(25.44f, 2.77f) },
			{ "储藏室", new Vector2(19.59f, 4.79f) },
			{ "武器库", new Vector2(19.94f, -1.96f) },
		};

		public static readonly Dictionary<string, Vector2> polusTeleportLocations = new Dictionary<string, Vector2>()
		{
			{ "运输飞船", new Vector2(16.61f, -1.17f) },
			{ "储藏室", new Vector2(20.35f, -11.46f) },
			{ "电力室", new Vector2(7.51f, -9.86f) },
			{ "安保室", new Vector2(2.98f, -12.18f) },
			{ "氧气室", new Vector2(1.55f, -16.81f ) },
			{ "锅炉房", new Vector2(2.14f, -23.55f) },
			{ "通讯室", new Vector2(11.70f, -15.87f ) },
			{ "武器库", new Vector2(10.71f, -22.90f) },
			{ "办公室", new Vector2(25.00f, -16.99f) },
			{ "管理室", new Vector2(22.76f, -22.32f) },
			{ "实验室", new Vector2(33.48f, -7.41f) },
			{ "标本室", new Vector2(36.78f, -19.28f) }
		};

		public static readonly Dictionary<string, Vector2> airshipTeleportLocations = new Dictionary<string, Vector2>()
		{
			{ "引擎室", new Vector2(-0.73f, 0.60f) },
			{ "通讯室", new Vector2(-13.03f, 1.31f ) },
			{ "驾驶舱", new Vector2(-20.98f, -1.15f ) },
			{ "军械库", new Vector2(-10.28f, -6.44f ) },
			{ "厨房", new Vector2(-4.31f, -11.08f ) },
			{ "观景台", new Vector2(-13.62f, -12.68f ) },
			{ "安保室", new Vector2(7.13f, -11.51f ) },
			{ "电力室", new Vector2(16.29f, -6.33f ) },
			{ "医务室", new Vector2(22.85f, -7.40f ) },
			{ "货舱", new Vector2(33.74f, -0.76f ) },
			{ "休息室", new Vector2(25.82f, 7.24f ) },
			{ "档案室", new Vector2(19.86f, 10.07f ) },
			{ "淋浴室", new Vector2(20.88f, 0.48f ) },
			{ "主厅", new Vector2(10.77f, -0.13f ) },
			{ "禁闭室", new Vector2(-0.94f, 8.66f ) },
			{ "金库", new Vector2(-8.78f, 8.13f ) },
			{ "空隙房", new Vector2(4.15f, 8.73f ) },
			{ "会议室", new Vector2(11.03f, 16.06f ) },
		};

		public static readonly Dictionary<string, Vector2> fungleTeleportLocations = new Dictionary<string, Vector2>()
		{
			{ "会议室", new Vector2(-3.08f, -0.41f) },
			{ "宿舍", new Vector2(1.66f, -1.53f) },
			{ "实验室", new Vector2(-4.31f, -8.56f) },
			{ "温室", new Vector2(9.34f, -9.92f) },
			{ "反应堆", new Vector2(21.38f, -7.48f) },
			{ "上引擎室", new Vector2(21.54f, 2.97f) },
			{ "瞭望台", new Vector2(7.79f, 1.53f) },
			{ "矿坑", new Vector2(12.56f, 9.28f) },
			{ "通讯室", new Vector2(21.17f, 13.70f) },
			{ "储藏室", new Vector2(-0.44f, 5.69f) },
			{ "运输飞船", new Vector2(-7.59f, 10.76f) },
			{ "自助餐厅", new Vector2(-16.49f, 7.14f) },
			{ "水花区", new Vector2(-17.89f, -0.08f) },
			{ "厨房", new Vector2(-15.55f, -7.52f) },
			{ "码头", new Vector2(-23.00f, -7.16f) },
		};

		public static readonly Dictionary<string, Vector2> dleksTeleportLocations = new Dictionary<string, Vector2>()
		{
			{ "自助餐厅", new Vector2(0.78f, 2.48f) },
			{ "武器库", new Vector2(-8.04f, 1.24f) },
			{ "医疗室", new Vector2(8.61f, -4.30f) },
			{ "管理室", new Vector2(-2.79f, -7.69f) },
			{ "氧气室", new Vector2(-6.50f, -3.50f) },
			{ "导航室", new Vector2(-16.77f, -4.67f) },
			{ "护盾室", new Vector2(-9.26f, -12.19f) },
			{ "通讯室", new Vector2(-5.10f, -15.24f ) },
			{ "储藏室", new Vector2(3.72f, -14.61f ) },
			{ "电力室", new Vector2(6.91f, -8.60f ) },
			{ "上引擎室", new Vector2(17.61f, -0.72f) },
			{ "下引擎室", new Vector2(17.33f, -13.10f ) },
			{ "安保室", new Vector2(12.81f, -3.01f) },
			{ "反应堆", new Vector2(20.53f, -5.39f) },
		};

		public static readonly Dictionary<PlayerControl, ushort> VentSeqIds = new Dictionary<PlayerControl, ushort>();

		public static Dictionary<string, Vector2> GetTeleportLocations()
		{
			MapNames currentMap = Utilities.GetCurrentMap();

			return currentMap switch
			{
				MapNames.Skeld => skeldTeleportLocations,
				MapNames.MiraHQ => miraTeleportLocations,
				MapNames.Polus => polusTeleportLocations,
				MapNames.Airship => airshipTeleportLocations,
				MapNames.Fungle => fungleTeleportLocations,
				MapNames.Dleks => dleksTeleportLocations,
				// If we don't have any teleport locations for the current map then just default to the Skeld ones
				_ => skeldTeleportLocations,
			};
		}

		public static void TeleportTo(Vector2 position)
		{
			if(UseSnapToRPC)
			{
				PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(position);
			}
			else
			{
				PlayerControl.LocalPlayer.NetTransform.SnapTo(position);
			}
		}

		public static void TeleportPlayerTo(PlayerControl player, Vector2 position)
		{
			BatchedMessage batch = new BatchedMessage();
			batch.QueueSnapTo(player, position);
			batch.FinishBatch();
		}

		public static void TeleportAllTo(Vector2 position)
		{
			// On +25 modded protocol lobbies, we are able to send SetColor RPCs as non-host
			// however we are still affected by message packing limits
			int packingLimit = AmongUsClient.Instance.GetMaxMessagePackingLimit();
			BatchedMessage batch = new BatchedMessage();

			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				if(batch.msgCount >= packingLimit)
				{
					batch.FinishBatch();
					batch = new BatchedMessage();
				}

				batch.QueueSnapTo(player, position);
			}

			batch.FinishBatch();
		}

		public static void TeleportToVent(PlayerControl player, int ventId)
		{
			if(ShipStatus.Instance == null)
			{
				Hydra.notifications.Send("管道传送", "需要游戏已经开始，该功能才能生效。");
				return;
			}

			bool hasAnticheat = Utilities.IsAnticheatPresent();
			if(!hasAnticheat || AmongUsClient.Instance.AmHost)
			{
				player.MyPhysics.RpcBootFromVent(ventId);
				return;
			}

			if(!VentSeqIds.ContainsKey(player))
			{
				// High enough value to supersede the actual sequence ID
				VentSeqIds.Add(player, 10000);
			}

			MessageWriter enterVent = MessageWriter.Get(SendOption.None);
			enterVent.Write(++VentSeqIds[player]);
			enterVent.Write((byte)VentilationSystem.Operation.Enter);
			enterVent.Write((byte)ventId);

			MessageWriter bootFromVent = MessageWriter.Get(SendOption.None);
			bootFromVent.Write(++VentSeqIds[player]);
			bootFromVent.Write((byte)VentilationSystem.Operation.BootImpostors);
			bootFromVent.Write((byte)ventId);

			BatchedMessage batch = new BatchedMessage(AmongUsClient.Instance.HostId);
			batch.QueueUpdateSystem(player, SystemTypes.Ventilation, enterVent);
			batch.QueueUpdateSystem(player, SystemTypes.Ventilation, bootFromVent);
			batch.FinishBatch();

			enterVent.Recycle();
			bootFromVent.Recycle();
		}
	}
}