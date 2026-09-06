using HydraMenu.modules;
using HydraMenu.network;
using UnityEngine;

namespace HydraMenu.routines
{
	public class AutoTriggerSporesRoutine : Routine
	{
		public AutoTriggerSporesRoutine() : base("自动触发孢子") { }

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
			Hydra.notifications.Send("自动触发孢子", "你离开了游戏，自动触发孢子已禁用。", 10);
			Enabled = false;
		}

		protected override void OnEnable()
		{
			if(ShipStatus.Instance == null)
			{
				Hydra.notifications.Send("自动触发孢子", "游戏开始后自动触发孢子才能使用。", 10);
				Enabled = false;
				return;
			}

			if(Utilities.GetCurrentMap() != MapNames.Fungle)
			{
				Hydra.notifications.Send("自动触发孢子", "自动触发孢子仅可用于 Fungle 地图。", 10);
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