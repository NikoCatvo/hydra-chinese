using InnerNet;
using UnityEngine;

namespace HydraMenu.modules.visuals
{
	internal class SpectatePlayer : Module
	{
		public SpectatePlayer() : base("观战玩家") { }

		public PlayerControl target;

		private void OnPlayerDisconnect(ClientData client, DisconnectReasons reason)
		{
			if(client.Character != target) return;

			Hydra.notifications.Send("观战玩家", "你观战的玩家离开了游戏，观战功能已禁用。");
			Enabled = false;
		}

		protected override void OnEnable()
		{
			// 以防此模块作为配置一部分被启用
			if(target == null)
			{
				_enabled = false;
				return;
			}

			FollowerCamera camera = Camera.main.GetComponent<FollowerCamera>();
			camera.SetTarget(target);

			HudManager.Instance.ShadowQuad.gameObject.SetActive(false);

			EventCoordinator.OnPlayerDisconnect += OnPlayerDisconnect;
		}

		protected override void OnDisable()
		{
			if(PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.Data != null)
			{
				FollowerCamera camera = Camera.main.GetComponent<FollowerCamera>();
				camera.SetTarget(PlayerControl.LocalPlayer);

				bool shouldBeEnabled = !ModuleManager.fullbright.Enabled && !RoleManager.IsGhostRole(PlayerControl.LocalPlayer.Data.RoleType);
				HudManager.Instance.ShadowQuad.gameObject.SetActive(shouldBeEnabled);
			}

			EventCoordinator.OnPlayerDisconnect -= OnPlayerDisconnect;
		}
	}
}