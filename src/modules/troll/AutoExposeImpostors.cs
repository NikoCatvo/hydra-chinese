using UnityEngine;

namespace HydraMenu.modules.troll
{
	internal class AutoExposeImpostors : Module
	{
		public AutoExposeImpostors() : base("自动曝光内鬼") { }

		public readonly float MIN_KILL_DISTANCE = 1.0f;
		public readonly float MAX_DISTANCE = 5.0f;

		public bool ExposeOnMurder { get; set; } = true;
		public bool ExposeOnShapeshift { get; set; } = true;
		// 仅在消失时触发，而非重现时
		// 重现动画短得多，所以一旦所有人被传送，他们可能注意不到气泡
		public bool ExposeOnPhantom { get; set; } = true;

		private void OnPlayerMurder(PlayerControl murderer, PlayerControl target, MurderResultFlags flags)
		{
			if(!ExposeOnMurder || ShipStatus.Instance == null || !flags.HasFlag(MurderResultFlags.Succeeded) || Sabotage.IsSabotageActive(SystemTypes.Electrical)) return;

			Vent selectedVent = FindClosestVent(murderer, MIN_KILL_DISTANCE, MAX_DISTANCE);
			if(selectedVent == null)
			{
				Hydra.Log.LogMessage("未找到可用的通风管道用于传送玩家");
				return;
			}

			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				if(player == murderer || player == target || player.Data.IsDead || RoleManager.IsImpostorRole(player.Data.RoleType)) continue;

				Teleporter.TeleportToVent(player, selectedVent.Id);
			}
		}

		private void OnPlayerShapeshift(PlayerControl shapeshifter, PlayerControl target, bool shouldAnimate)
		{
			if(!ExposeOnShapeshift || ShipStatus.Instance == null || Sabotage.IsSabotageActive(SystemTypes.Electrical)) return;

			Vent selectedVent = FindClosestVent(shapeshifter, MIN_KILL_DISTANCE, MAX_DISTANCE);
			if(selectedVent == null)
			{
				Hydra.Log.LogMessage("未找到可用的通风管道用于传送玩家");
				return;
			}

			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				if(player == shapeshifter || player.Data.IsDead || RoleManager.IsImpostorRole(player.Data.RoleType)) continue;

				Teleporter.TeleportToVent(player, selectedVent.Id);
			}
		}

		private void OnPlayerPhantom(PlayerControl phantom)
		{
			if(!ExposeOnPhantom || ShipStatus.Instance == null || Sabotage.IsSabotageActive(SystemTypes.Electrical)) return;

			Vent selectedVent = FindClosestVent(phantom, 0.0f, MAX_DISTANCE);
			if(selectedVent == null)
			{
				Hydra.Log.LogMessage("未找到可用的通风管道用于传送玩家");
				return;
			}

			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				if(player == phantom || player.Data.IsDead || RoleManager.IsImpostorRole(player.Data.RoleType)) continue;

				Teleporter.TeleportToVent(player, selectedVent.Id);
			}
		}

		private Vent FindClosestVent(PlayerControl player, float minDistance, float maxDistance)
		{
			foreach(Vent vent in ShipStatus.Instance.AllVents) {
				float distance = Vector2.Distance(player.transform.position, vent.transform.position);
				Hydra.Log.LogMessage($"通风管道 ID {vent.Id} 的距离为 {distance}");

				// 如果击杀离通风管道太远，传送的玩家将无法看到击杀
				// 如果击杀太近，玩家将无法在堆叠中确定是谁击杀了
				if(distance < minDistance || distance > maxDistance) continue;

				// 我们还想确保没有物体会阻挡传送玩家对击杀的视线
				// 不完美，因为很多物体允许你看穿它们
				if(PhysicsHelpers.AnythingBetween(player.Collider, player.Collider.bounds.center, vent.transform.position, Constants.ShipOnlyMask, false)) continue;

				return vent;
			}

			return null;
		}

		protected override void OnEnable()
		{
			EventCoordinator.OnPlayerMurder += OnPlayerMurder;
			EventCoordinator.OnPlayerShapeshift += OnPlayerShapeshift;
			EventCoordinator.OnPlayerPhantom += OnPlayerPhantom;
		}

		protected override void OnDisable()
		{
			EventCoordinator.OnPlayerMurder -= OnPlayerMurder;
			EventCoordinator.OnPlayerShapeshift -= OnPlayerShapeshift;
			EventCoordinator.OnPlayerPhantom -= OnPlayerPhantom;
		}
	}
}