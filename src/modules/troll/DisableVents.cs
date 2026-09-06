namespace HydraMenu.modules.troll
{
	internal class DisableVents : Module
	{
		public DisableVents() : base("禁用通风管道") { }

		private void KickPlayerFromVent(PlayerControl player, byte ventId)
		{
			if(!Utilities.IsAnticheatPresent() || AmongUsClient.Instance.AmHost)
			{
				player.MyPhysics.RpcBootFromVent(ventId);
			}
			else
			{
				VentilationSystem.Update(VentilationSystem.Operation.BootImpostors, ventId);
			}
		}

		protected override void OnEnable()
		{
			EventCoordinator.OnPlayerEnterVent += KickPlayerFromVent;

			if(ShipStatus.Instance != null)
			{
				ShipStatus.Instance.Systems.TryGetValue(SystemTypes.Ventilation, out ISystemType system);
				VentilationSystem ventSystem = system.Cast<VentilationSystem>();

				// 踢出当前在通风管道中的所有玩家
				foreach(PlayerControl player in PlayerControl.AllPlayerControls)
				{
					if(!ventSystem.PlayersInsideVents.ContainsKey(player.PlayerId)) continue;
					KickPlayerFromVent(player, ventSystem.PlayersInsideVents[player.PlayerId]);
				}
			}
		}

		protected override void OnDisable()
		{
			EventCoordinator.OnPlayerEnterVent -= KickPlayerFromVent;
		}
	}
}