using HydraMenu.modules;
using InnerNet;
using UnityEngine;

namespace HydraMenu.routines
{
	public class PlayerFollowerRoutine : Routine
	{
		public PlayerFollowerRoutine() : base("玩家跟随") { }

		public PlayerControl target;

		public override void Run()
		{
			if(PlayerControl.LocalPlayer == null) return;

			/*
			float distance = Vector3.Distance(target.transform.position, PlayerControl.LocalPlayer.transform.position);
			if(distance > 2)
			{
				Hydra.Log.LogInfo($"我们偏离跟随的玩家太远，传送回原位。距离: {distance}");
				Teleporter.TeleportTo(target.transform.position);
			}
			*/

			// 我们大概可以看看幽灵附身如何让跟随者走向玩家位置，这样我们就不必直接传送，但现在这样也可以
			PlayerControl.LocalPlayer.transform.position = target.transform.position;
		}

		private void OnDisconnect()
		{
			Hydra.notifications.Send("跟随玩家", "你离开了游戏，跟随玩家已禁用。", 10);
			Enabled = false;
		}

		private void OnPlayerDisconnect(ClientData client, DisconnectReasons reason)
		{
			if(client.Character != target) return;

			Hydra.notifications.Send("跟随玩家", "你跟随的玩家离开了游戏，跟随玩家已禁用。");
			Enabled = false;
		}

		protected override void OnEnable()
		{
			if(PlayerControl.LocalPlayer == null)
			{
				_enabled = false;
				return;
			}

			PlayerControl.LocalPlayer.moveable = false;
			PlayerControl.LocalPlayer.NetTransform.body.velocity = Vector2.zero;

			EventCoordinator.OnDisconnect += OnDisconnect;
			EventCoordinator.OnPlayerDisconnect += OnPlayerDisconnect;
		}

		protected override void OnDisable()
		{
			target = null;

			if(PlayerControl.LocalPlayer != null)
			{
				PlayerControl.LocalPlayer.moveable = true;
			}

			EventCoordinator.OnDisconnect -= OnDisconnect;
			EventCoordinator.OnPlayerDisconnect -= OnPlayerDisconnect;
		}
	}
}