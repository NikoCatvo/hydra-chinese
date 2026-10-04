using HydraMenu.modules;
using UnityEngine;

namespace HydraMenu.routines
{
	public class DoorTrollerRoutine : Routine
	{
		public DoorTrollerRoutine() : base("DoorTroller") { }

		public float LockAndUnlockDelay { get; set; } = 0.5f;
		private float timeElapsed = 0f;
		private bool doorsLocked = false;

		public override void Run()
		{
			if(ShipStatus.Instance == null) return;

			timeElapsed += Time.deltaTime;
			if(timeElapsed < LockAndUnlockDelay) return;

			if(doorsLocked)
			{
				Sabotage.UnlockAll();
			}
			else
			{
				Sabotage.LockAll();
			}

			doorsLocked = !doorsLocked;
			timeElapsed = 0;
		}

		private void OnDisconnect()
		{
			Hydra.notifications.Send("门恶搞", "门恶搞已关闭，因为你已离开游戏。", 10);
			Enabled = false;
		}

		protected override void OnEnable()
		{
			if(PlayerControl.LocalPlayer == null || ShipStatus.Instance == null)
			{
				Hydra.notifications.Send("门恶搞", "门恶搞只能在游戏开始后使用。", 10);
				Enabled = false;
				return;
			}

			if(ShipStatus.Instance.AllDoors.Count == 0)
			{
				Hydra.notifications.Send("门恶搞", "当前地图没有任何门，无法使用门恶搞。", 10);
				Enabled = false;
				return;
			}

			if(!Sabotage.CanUnlockDoors())
			{
				Hydra.notifications.Send("门恶搞", "只有房主，或当前地图支持解锁门时，才能使用门恶搞。", 10);
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