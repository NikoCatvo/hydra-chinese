using HydraMenu.modules;
using HydraMenu.network;
using System.Collections.Generic;
using UnityEngine;

namespace HydraMenu.routines
{
	internal class VoteSpammer : Routine
	{
		public VoteSpammer() : base("投票刷屏") { }

		public readonly HashSet<int> targets = new HashSet<int>();

		public readonly float VOTE_DELAY = 1.0f;
		private float timeElapsed = 0.0f;

		public override void Run()
		{
			timeElapsed += Time.deltaTime;
			if(timeElapsed < VOTE_DELAY) return;
			timeElapsed = 0.0f;

			int packingLimit = AmongUsClient.Instance.GetMaxMessagePackingLimit();
			BatchedMessage batch = new BatchedMessage();

			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				if(!IsGlobal && !targets.Contains(player.GetHashCode())) continue;

				if(batch.msgCount >= packingLimit)
				{
					batch.FinishBatch();
					batch = new BatchedMessage();
				}

				batch.QueueSendChatNote(PlayerControl.LocalPlayer, player.PlayerId, ChatNoteTypes.DidVote);
			}

			batch.FinishBatch();
		}

		private bool IsGlobal
		{
			get { return targets.Count == 1 && targets.Contains(int.MaxValue); }
		}

		private void OnMeetingEnd()
		{
			Hydra.notifications.Send("投票刷屏", "当前会议已结束，投票刷屏已禁用。");
			Enabled = false;
		}

		private void OnDisconnect()
		{
			Hydra.notifications.Send("投票刷屏", "你离开了游戏，投票刷屏已禁用。");
			Enabled = false;
		}

		protected override void OnEnable()
		{
			if(MeetingHud.Instance == null)
			{
				Hydra.notifications.Send("投票刷屏", "此功能需要有活跃的会议才能使用。");
				Enabled = false;
				return;
			}

			if(Utilities.IsAnticheatPresent() && !AmongUsClient.Instance.AmHost)
			{
				Hydra.notifications.Send("投票刷屏", "投票刷屏仅可由大厅房主使用。");
				Enabled = false;
				return;
			}

			EventCoordinator.OnMeetingEnd += OnMeetingEnd;
			EventCoordinator.OnDisconnect += OnDisconnect;
		}

		protected override void OnDisable()
		{
			targets.Clear();

			EventCoordinator.OnMeetingEnd -= OnMeetingEnd;
			EventCoordinator.OnDisconnect -= OnDisconnect;
		}
	}
}