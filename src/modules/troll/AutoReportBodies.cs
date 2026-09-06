namespace HydraMenu.modules.troll
{
	internal class AutoReportBodies : Module
	{
		public AutoReportBodies() : base("自动报尸体") { }

		// 应被栽赃报告尸体的玩家
		public PlayerControl target;

		private void OnPlayerMurder(PlayerControl murderer, PlayerControl victim, MurderResultFlags flags)
		{
			if(!flags.HasFlag(MurderResultFlags.Succeeded)) return;

			if(AmongUsClient.Instance.AmHost)
			{
				Utilities.OpenMeeting(target ?? PlayerControl.LocalPlayer, victim.Data);
				return;
			}

			if(PlayerControl.LocalPlayer.Data.IsDead) return;

			Hydra.notifications.Send("自动报尸体", $"{victim.Data.PlayerName} 被 {murderer.Data.PlayerName} ({Utilities.GetPlayerColor(murderer.Data)}) 击杀，其尸体已被自动报告。");
			PlayerControl.LocalPlayer.CmdReportDeadBody(victim.Data);
		}

		protected override void OnEnable()
		{
			EventCoordinator.OnPlayerMurder += OnPlayerMurder;
		}

		protected override void OnDisable()
		{
			EventCoordinator.OnPlayerMurder -= OnPlayerMurder;
		}
	}
}