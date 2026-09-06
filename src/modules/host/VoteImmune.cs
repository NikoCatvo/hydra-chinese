using System.Collections.Generic;

namespace HydraMenu.modules.host
{
	internal class VoteImmune : Module
	{
		public VoteImmune() : base("投票免疫") { }

		public readonly HashSet<int> targets = new HashSet<int>();

		private void OnPlayerCastVote(NetworkedPlayerInfo voter, NetworkedPlayerInfo votee)
		{
			if(!targets.Contains(votee.Object.GetHashCode())) return;

			Hydra.Log.LogMessage($"{voter.PlayerName} 对投票免疫的玩家投票，将其投票改为跳过");

			// 找到给投票免疫玩家投票的玩家，让他们改投跳过
			// 民主的巅峰 :P
			foreach(PlayerVoteArea voteArea in MeetingHud.Instance.playerStates)
			{
				if(voteArea.PlayerId != voter.PlayerId) continue;

				voteArea.VotedForId = 253; // Skip
			}
		}

		protected override void OnEnable()
		{
			EventCoordinator.OnPlayerCastVote += OnPlayerCastVote;
		}

		protected override void OnDisable()
		{
			targets.Clear();

			EventCoordinator.OnPlayerCastVote -= OnPlayerCastVote;
		}
	}
}