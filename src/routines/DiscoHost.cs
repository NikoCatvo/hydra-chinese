using HydraMenu.modules;
using HydraMenu.network;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HydraMenu.routines
{
	public class DiscoHostRoutine : Routine
	{
		public DiscoHostRoutine() : base("迪斯科派对") { }
		public readonly HashSet<int> targets = new HashSet<int>();

		public float RandomizationDelay { get; set; } = 0.5f;
		private float timeElapsed = 0f;

		private readonly System.Random rnd = new System.Random();

		public override void Run()
		{
			timeElapsed += Time.deltaTime;
			if(timeElapsed < RandomizationDelay) return;
			timeElapsed = 0f;

			List<int> colors = Enumerable.Range(0, 18).ToList();

			// 在 +25 模组协议大厅中，我们可以作为非房主发送 SetColor RPC
			// 但我们仍受消息打包限制影响
			int packingLimit = AmongUsClient.Instance.GetMaxMessagePackingLimit();
			BatchedMessage batch = new BatchedMessage();

			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				if(!IsGlobal && !targets.Contains(player.GetHashCode())) continue;

				// 为每个玩家分配唯一颜色
				int color;
				if(colors.Count != 0)
				{
					color = colors[rnd.Next(0, colors.Count)];
					colors.Remove(color);
				}
				else
				{
					// 为确保与超过 18 名玩家的大厅兼容
					color = rnd.Next(0, 18);
				}

				if(batch.msgCount >= packingLimit)
				{
					batch.FinishBatch();
					batch = new BatchedMessage();
				}

				batch.QueueSetColor(player, (byte)color);
			}

			batch.FinishBatch();
		}

		private bool IsGlobal
		{
			get { return targets.Count == 1 && targets.Contains(int.MaxValue); }
		}

		private void OnDisconnect()
		{
			Hydra.notifications.Send("迪斯科派对", "你离开了游戏，迪斯科派对已禁用。", 10);
			Enabled = false;
		}

		protected override void OnEnable()
		{
			if(PlayerControl.LocalPlayer == null)
			{
				Hydra.notifications.Send("迪斯科派对", "迪斯科派对只能在游戏中使用。", 10);
				Enabled = false;
				return;
			}

			if(Utilities.IsAnticheatPresent() && !AmongUsClient.Instance.AmHost)
			{
				Hydra.notifications.Send("迪斯科派对", "迪斯科派对仅可由大厅房主使用。", 10);
				Enabled = false;
				return;
			}

			EventCoordinator.OnDisconnect += OnDisconnect;
		}

		protected override void OnDisable()
		{
			targets.Clear();

			EventCoordinator.OnDisconnect -= OnDisconnect;
		}
	}
}