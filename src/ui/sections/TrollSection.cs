using Hazel;
using HydraMenu.features;
using HydraMenu.network;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class TrollSection : ISection
	{
		public TrollSection() : base("整蛊") { }

		public int selectedVent = 0;
		public System.Random rnd = new System.Random();

		public override void Render()
		{
			if(PlayerControl.LocalPlayer == null)
			{
				GUILayout.Label("你当前不在游戏中，这些选项将无法生效。");
			}

			Troll.AutoReportBodies.Enabled = Controls.PlayerSpecificToggle("自动报告尸体", PlayerControl.LocalPlayer, ref Troll.AutoReportBodies.source);
			Hydra.routines.autoTriggerSpores.Enabled = GUILayout.Toggle(Hydra.routines.autoTriggerSpores.Enabled, "自动触发孢子");
			Troll.BlockSabotages.Enabled = GUILayout.Toggle(Troll.BlockSabotages.Enabled, "阻止破坏");
			Troll.BlockVenting.Enabled = GUILayout.Toggle(Troll.BlockVenting.Enabled, "禁用通风口");

			if(GUILayout.Button("踢出所有玩家"))
			{
				Hydra.Log.LogInfo($"Sending Enter ventilation system update to all players");

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
					Hydra.notifications.Send("触发孢子", "此选项仅在蘑菇（Fungle）地图上生效。");
				}
				else
				{
					FungleShipStatus shipStatus = ShipStatus.Instance.Cast<FungleShipStatus>();

					foreach(Mushroom mushroom in shipStatus.sporeMushrooms.Values)
					{
						PlayerControl.LocalPlayer.RpcTriggerSpores(mushroom);
					}

					Hydra.notifications.Send("触发孢子", "所有孢子已被触发。", 5);
				}
			}

			GUILayout.Space(5);
			GUILayout.Label($"通风口传送:");
			Hydra.routines.teleportSpammer.Enabled = GUILayout.Toggle(Hydra.routines.teleportSpammer.Enabled, "传送刷屏器");

			GUILayout.Label($"将所有人传送到通风口: {selectedVent}");
			selectedVent = (int)GUILayout.HorizontalSlider(selectedVent, 0, ShipStatus.Instance != null ? ShipStatus.Instance.AllVents.Count - 1 : 10);

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
			GUILayout.Label("门户整蛊:");
			Hydra.routines.doorTroller.Enabled = GUILayout.Toggle(Hydra.routines.doorTroller.Enabled, "启用");

			GUILayout.Label($"开关门延迟: {Hydra.routines.doorTroller.lockAndUnlockDelay:F2}秒");
			Hydra.routines.doorTroller.lockAndUnlockDelay = GUILayout.HorizontalSlider(Hydra.routines.doorTroller.lockAndUnlockDelay, 0.1f, 2.0f);
		}
	}
}
