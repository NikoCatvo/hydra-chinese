namespace HydraMenu.modules.troll
{
	internal class CrashLobby : Module
	{
		public CrashLobby() : base("CrashLobby") { }

		private void OnGameStart()
		{
			PlayerControl.LocalPlayer.CmdReportDeadBody(null);
			Hydra.notifications.Send("房间崩溃", "房间已崩溃。");
			Enabled = false;
		}

		protected override void OnEnable()
		{
			Hydra.notifications.Send("房间崩溃", "房间崩溃功能已启用，游戏开始后将使房间崩溃。");

			EventCoordinator.OnGameStart += OnGameStart;
		}

		protected override void OnDisable()
		{
			EventCoordinator.OnGameStart -= OnGameStart;
		}
	}
}