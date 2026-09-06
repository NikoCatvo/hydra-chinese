using HarmonyLib;
using Hazel;
using Il2CppInterop.Runtime.InteropTypes.Arrays;
using InnerNet;

namespace HydraMenu.modules.protections
{
	internal class AntiKick : Module
	{
		public AntiKick() : base("防踢出")
		{
			base.Enabled = true;
		}

		private static AntiKick Instance
		{
			get { return ModuleManager.antiKick; }
		}

		public bool BlockVotekicks { get; set; } = true;
		public bool BlockHostOnlyRpcExploit { get; set; } = true;

		[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.HandleRpc))]
		class OnShipStatusRPC
		{
			static bool Prefix(byte callId, MessageReader reader)
			{
				if(!Instance.Enabled || !Instance.BlockHostOnlyRpcExploit || callId != (byte)RpcCalls.UpdateSystem) return true;
				int oldReadPosition = reader.Position;

				SystemTypes system = (SystemTypes)reader.ReadByte();
				PlayerControl player = reader.ReadNetObject<PlayerControl>();

				bool shouldBlock = false;
				if(system == SystemTypes.Ventilation && !AmongUsClient.Instance.AmHost)
				{
					Hydra.notifications.Send("防护警报", $"{player.Data.PlayerName} 试图对你使用通气系统踢人漏洞!");
					shouldBlock = true;
				}

				reader.Position = oldReadPosition;

				return !shouldBlock;
			}
		}

		private void OnPlayerVotekick(ClientData source, ClientData target)
		{
			if(!BlockVotekicks || !AmongUsClient.Instance.AmHost || source.Id != AmongUsClient.Instance.ClientId) return;

			// 移除我们的投票，就像从未发生过一样
			VoteBanSystem.Instance.Votes[AmongUsClient.Instance.ClientId] = new Il2CppStructArray<int>(0);
		}

		protected override void OnEnable()
		{
			EventCoordinator.OnPlayerVotekick += OnPlayerVotekick;
		}

		protected override void OnDisable()
		{
			EventCoordinator.OnPlayerVotekick -= OnPlayerVotekick;
		}
	}
}