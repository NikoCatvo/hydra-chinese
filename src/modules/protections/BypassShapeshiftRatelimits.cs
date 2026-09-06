using AmongUs.GameOptions;

namespace HydraMenu.modules.protections
{
	internal class BypassShapeshiftRatelimits : Module
	{
		// 变身和撤销变身对房主有严格的速率限制，这会影响房主选项中的大规模变身功能
		// 我们可以通过发送游戏选项更新并将变身冷却设为零秒来绕过这些限速
		public BypassShapeshiftRatelimits() : base("绕过变身限速")
		{
			base.Enabled = true;
		}

		private void OnGameStart()
		{
			if(!AmongUsClient.Instance.AmHost) return;

			PlayerControl player = Utilities.GetRandomPlayer();
			if(player == null) return;

			IGameOptions options = GameOptions.CreateCloneOptions(GameManager.Instance.LogicOptions.currentGameOptions);
			options.SetFloat(FloatOptionNames.ShapeshifterCooldown, 0.0f);

			// 将设置更新发送给随机玩家，我们不想搞乱保存的大厅设置
			GameOptions.SendGameOptionsToClient(options, player.OwnerId);
		}

		protected override void OnEnable()
		{
			EventCoordinator.OnGameStart += OnGameStart;
		}

		protected override void OnDisable()
		{
			EventCoordinator.OnGameStart -= OnGameStart;
		}
	}
}