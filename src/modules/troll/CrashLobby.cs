namespace HydraMenu.modules.troll
{
	internal class CrashLobby : Module
	{
		public CrashLobby() : base("崩溃大厅") { }

		private void OnGameStart()
		{
			PlayerControl.LocalPlayer.CmdReportDeadBody(null);
			Hydra.notifications.Send("大厅崩溃器", "大厅已崩溃。");
		}

		protected override void OnEnable()
		{
			Hydra.notifications.Send("大厅崩溃器", "大厅崩溃已启用。游戏启动后大厅将会崩溃。");

			EventCoordinator.OnGameStart += OnGameStart;
		}

		protected override void OnDisable()
		{
			EventCoordinator.OnGameStart -= OnGameStart;
		}
	}
}