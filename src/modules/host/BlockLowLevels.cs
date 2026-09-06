using HarmonyLib;

namespace HydraMenu.modules.host
{
	internal class BlockLowLevels : Module
	{
		public BlockLowLevels() : base("阻止低等级") { }

		public uint MinLevel { get; set; } = 20;

		private static BlockLowLevels Instance
		{
			get { return ModuleManager.blockLowLevels; }
		}

		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.SetLevel))]
		class OnSetLevel
		{
			static void Prefix(PlayerControl __instance, uint level)
			{
				if(!Instance.Enabled || !AmongUsClient.Instance.AmHost || __instance == PlayerControl.LocalPlayer || level >= Instance.MinLevel) return;

				KickPlayer(__instance, level);
			}
		}

		private static void KickPlayer(PlayerControl player, uint level)
		{
			Hydra.notifications.Send("拦截低等级", $"{player.Data.PlayerName} 等级为 {level}，低于等级阈值。他们将被踢出游戏。");
			AmongUsClient.Instance.KickPlayer(player.OwnerId, false);
		}

		protected override void OnEnable()
		{
			if(AmongUsClient.Instance == null || !AmongUsClient.Instance.AmHost) return;

			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				if(player == PlayerControl.LocalPlayer || player.Data == null || player.Data.PlayerLevel >= MinLevel) return;

				KickPlayer(player, player.Data.PlayerLevel);
			}
		}
	}
}