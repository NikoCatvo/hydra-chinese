using HydraMenu.modules;
using HydraMenu.network;
using UnityEngine;

namespace HydraMenu.routines
{
	public class AutoTriggerSporesRoutine : Routine
	{
		public AutoTriggerSporesRoutine() : base("AutoTriggerSpores") { }

		public readonly float SPORE_TRIGGER_DURATION = 5.0f;
		private float timeElapsed = 0f;

		public override void Run()
		{
			if(ShipStatus.Instance == null) return;

			timeElapsed += Time.deltaTime;
			if(timeElapsed < SPORE_TRIGGER_DURATION) return;
			timeElapsed = 0f;

			FungleShipStatus shipStatus = ShipStatus.Instance.Cast<FungleShipStatus>();

			BatchedMessage batch = new BatchedMessage();

			foreach(Mushroom mushroom in shipStatus.sporeMushrooms.Values)
			{
				batch.QueueTriggerSpore(PlayerControl.LocalPlayer, mushroom);
			}

			batch.FinishBatch();
		}

		private void OnDisconnect()
		{
			Hydra.notifications.Send("触发孢子", "自动触发孢子已关闭，因为你已离开游戏。", 10);
			Enabled = false;
		}

		protected override void OnEnable()
		{
			if(ShipStatus.Instance == null)
			{
				Hydra.notifications.Send("触发孢子", "自动触发孢子只能在游戏开始后使用。", 10);
				Enabled = false;
				return;
			}

			if(Utilities.GetCurrentMap() != MapNames.Fungle)
			{
				Hydra.notifications.Send("触发孢子", "自动触发孢子只能在真菌丛林使用。", 10);
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