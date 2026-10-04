namespace HydraMenu.modules.troll
{
	internal class AutoReportBodies : Module
	{
		public AutoReportBodies() : base("AutoReportBodies") { }

		// The player that should be framed for reporting the body
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

			Hydra.notifications.Send("自动报告尸体", $"{victim.Data.PlayerName} 被 {murderer.Data.PlayerName}（{ui.Lang.Tr(Utilities.GetPlayerColor(murderer.Data))}）杀害，尸体已自动报告。");
			PlayerControl.LocalPlayer.CmdReportDeadBody(victim.Data);
		}

		protected override void OnEnable()
		{
			EventCoordinator.OnPlayerMurder += OnPlayerMurder;
		}

		protected override void OnDisable()
		{
			target = null;

			EventCoordinator.OnPlayerMurder -= OnPlayerMurder;
		}
	}
}