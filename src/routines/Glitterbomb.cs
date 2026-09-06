using HydraMenu.modules;
using HydraMenu.network;
using UnityEngine;

namespace HydraMenu.routines
{
	public class GlitterBomb : Routine
	{
		public GlitterBomb() : base("亮片炸弹") { }

		private readonly float PHANTOM_DELAY = 0.05f;
		private float timeElapsed = 0.0f;

		public override void Run()
		{
			timeElapsed += Time.deltaTime;
			if(timeElapsed < PHANTOM_DELAY) return;
			timeElapsed = 0.0f;

			PlayerControl.LocalPlayer.CmdCheckColor((byte)Utilities.GetRandomUnusedColor());

			BatchedMessage batch = new BatchedMessage();
			batch.QueueAppear(PlayerControl.LocalPlayer);
			batch.QueueVanish(PlayerControl.LocalPlayer);
			batch.FinishBatch();
		}

		private void OnDisconnect()
		{
			Hydra.notifications.Send("亮片炸弹", "你离开了游戏，亮片炸弹已禁用。", 10);
			Enabled = false;
		}

		private void OnPlayerMurder(PlayerControl murder, PlayerControl target, MurderResultFlags result)
		{
			if(target != PlayerControl.LocalPlayer) return;

			Hydra.notifications.Send("亮片炸弹", "你已被击杀，亮片炸弹已禁用。", 10);
			Enabled = false;
		}

		protected override void OnEnable()
		{
			if(PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null)
			{
				Hydra.notifications.Send("亮片炸弹", "此功能必须身处游戏内才能使用。", 10);
				Enabled = false;
				return;
			}

			if(PlayerControl.LocalPlayer.Data.RoleType != AmongUs.GameOptions.RoleTypes.Phantom)
			{
				Hydra.notifications.Send("亮片炸弹", "此功能需要你是幻影才能使用。", 10);
				Enabled = false;
				return;
			}

			EventCoordinator.OnDisconnect += OnDisconnect;
			EventCoordinator.OnPlayerMurder += OnPlayerMurder;
		}

		protected override void OnDisable()
		{
			EventCoordinator.OnDisconnect -= OnDisconnect;
			EventCoordinator.OnPlayerMurder -= OnPlayerMurder;
		}
	}
}