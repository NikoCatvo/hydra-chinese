using Hazel;
using HydraMenu.network;

namespace HydraMenu.modules.troll
{
	internal class DisableCameras : Module
	{
		// 通讯破坏激活时无法观看监控摄像头。我们可以利用这一点来禁用监控摄像头
		// 当玩家开始观看摄像头时，对该玩家破坏通讯；当玩家停止观看摄像头时，修复该玩家的通讯破坏
		public DisableCameras() : base("禁用摄像头") { }

		private void OnPlayerEnterCameras(PlayerControl player)
		{
			// 如果我们为房主更新通讯，则所有人都会受到通讯影响
			if(player.OwnerId == AmongUsClient.Instance.HostId || player == PlayerControl.LocalPlayer) return;

			Hydra.Log.LogMessage($"{player.Data.PlayerName} 开始观看摄像头，发送通讯系统更新");

			EnableCommsFor(player);
		}

		private void OnPlayerExitCameras(PlayerControl player)
		{
			// 如果我们为房主更新通讯，则所有人都会受到通讯影响
			if(player.OwnerId == AmongUsClient.Instance.HostId || player == PlayerControl.LocalPlayer) return;

			// 防止一种漏洞：如果通讯破坏处于激活状态，有人可能进入并离开安保摄像头以移除自身的通讯效果
			if(Sabotage.IsSabotageActive(SystemTypes.Comms))
			{
				// 存在一种边缘情况：如果有人在通讯被主动破坏时处于安保摄像头面板，且破坏被修复，
				// 则该玩家将能够观看安保摄像头
				// 考虑到此功能不太可能被任何人使用，我认为不值得修复此边缘情况
				Hydra.Log.LogMessage($"{player.Data.name} 更新了安保摄像头，通讯破坏已激活，无需操作");
				return;
			}

			Hydra.Log.LogMessage($"{player.Data.PlayerName} 停止观看摄像头，发送通讯系统更新");

			DisableCommsFor(player);
		}

		private void EnableCommsFor(PlayerControl player)
		{
			if(!AmongUsClient.Instance.AmHost)
			{
				Sabotage.SabotageSystem(SystemTypes.Comms, player.OwnerId);
				return;
			}

			BatchedMessage batch = new BatchedMessage(player.OwnerId);

			MessageWriter systemUpdate = MessageWriter.Get(SendOption.Reliable);
			systemUpdate.StartMessage((byte)SystemTypes.Comms);
			// 1 = 通讯破坏激活，0 = 通讯破坏未激活
			systemUpdate.Write(1);
			systemUpdate.EndMessage();

			batch.QueueDataFlag(ShipStatus.Instance.NetId, systemUpdate);

			batch.FinishBatch();
		}

		private void DisableCommsFor(PlayerControl player)
		{
			if(!AmongUsClient.Instance.AmHost)
			{
				Sabotage.FixSabotage(SystemTypes.Comms, player.OwnerId);
				return;
			}

			BatchedMessage batch = new BatchedMessage(player.OwnerId);

			MessageWriter systemUpdate = MessageWriter.Get(SendOption.Reliable);
			systemUpdate.StartMessage((byte)SystemTypes.Comms);
			// 1 = 通讯破坏激活，0 = 通讯破坏未激活
			systemUpdate.Write(0);
			systemUpdate.EndMessage();

			batch.QueueDataFlag(ShipStatus.Instance.NetId, systemUpdate);

			batch.FinishBatch();
		}

		protected override void OnEnable()
		{
			if(ShipStatus.Instance != null)
			{
				ISystemType system = ShipStatus.Instance.Systems[SystemTypes.Security];
				if(system == null) return;

				SecurityCameraSystemType securitySystem = system.Cast<SecurityCameraSystemType>();

				foreach(PlayerControl player in PlayerControl.AllPlayerControls)
				{
					if(player.OwnerId == AmongUsClient.Instance.HostId || player == PlayerControl.LocalPlayer || !securitySystem.PlayersUsing.Contains(player.PlayerId)) return;

					EnableCommsFor(player);
				}
			}

			EventCoordinator.OnPlayerEnterCameras += OnPlayerEnterCameras;
			EventCoordinator.OnPlayerExitCameras += OnPlayerExitCameras;
		}

		protected override void OnDisable()
		{
			EventCoordinator.OnPlayerEnterCameras -= OnPlayerEnterCameras;
			EventCoordinator.OnPlayerExitCameras -= OnPlayerExitCameras;
		}
	}
}