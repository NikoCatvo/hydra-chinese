using HydraMenu.modules;
using UnityEngine;

namespace HydraMenu.routines
{
	public class ReportBodySpam : Routine
	{
		public ReportBodySpam() : base("刷报尸体") { }

		public readonly float REPORT_DELAY = 2.5f;
		private float timeElapsed = 0f;

		public override void Run()
		{
			if(ShipStatus.Instance == null) return;

			timeElapsed += Time.deltaTime;
			if(timeElapsed < REPORT_DELAY) return;
			timeElapsed = 0f;

			PlayerControl player = Utilities.GetRandomPlayer(false, false, false, false);

			if(MeetingHud.Instance == null)
			{
				Utilities.OpenMeeting(PlayerControl.LocalPlayer, player.Data);
			}

			PlayerControl.LocalPlayer.RpcStartMeeting(player.Data);
		}

		private void OnDisconnect()
		{
			Hydra.notifications.Send("刷报尸体", "你离开了游戏，刷报尸体已禁用。", 10);
			Enabled = false;
		}

		protected override void OnEnable()
		{
			if(PlayerControl.LocalPlayer == null || ShipStatus.Instance == null)
			{
				Hydra.notifications.Send("刷报尸体", "游戏开始后刷报尸体才能使用。", 10);
				Enabled = false;
				return;
			}

			if(!AmongUsClient.Instance.AmHost)
			{
				Hydra.notifications.Send("刷报尸体", "刷报尸体仅可由大厅房主使用。", 10);
				Enabled = false;
				return;
			}

			EventCoordinator.OnDisconnect += OnDisconnect;
		}

		protected override void OnDisable()
		{
			EventCoordinator.OnDisconnect -= OnDisconnect;
		}
	}
}