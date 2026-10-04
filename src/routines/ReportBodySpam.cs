using HydraMenu.modules;
using UnityEngine;

namespace HydraMenu.routines
{
	public class ReportBodySpam : Routine
	{
		public ReportBodySpam() : base("ReportBodySpam") { }

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
			Hydra.notifications.Send("报告尸体刷屏", "报告尸体刷屏已关闭，因为你已离开游戏。", 10);
			Enabled = false;
		}

		protected override void OnEnable()
		{
			if(PlayerControl.LocalPlayer == null || ShipStatus.Instance == null)
			{
				Hydra.notifications.Send("报告尸体刷屏", "报告尸体刷屏只能在游戏开始后使用。", 10);
				Enabled = false;
				return;
			}

			if(!AmongUsClient.Instance.AmHost)
			{
				Hydra.notifications.Send("报告尸体刷屏", "报告尸体刷屏仅限房间房主使用。", 10);
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