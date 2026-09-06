using HarmonyLib;

namespace HydraMenu.modules.self
{
	internal class Immortality : Module
	{
		// PlayerControl::CheckMurder 函数是 CheckMurder RPC 的处理程序。当大厅房主接收到此 RPC 时，它首先检查
		// 尝试击杀的玩家是否为内鬼且存活，然后检查被击杀的玩家是否存活、不在通风管道中、不在梯子或平台上
		// 如果一切顺利，带有 Succeeded 标志的 MurderPlayer RPC 会发送给所有在线玩家，玩家被击杀
		// 如果上述检查任一失败，则发送带有 FailedError 标志的 MurderPlayer RPC，玩家不会被击杀
		// 我们可以通过让房主以为我们在通风管道中，来将其作为不死之身漏洞利用
		// 理论上，我们可以向房主发送一个包含 EnterVent RPC 的 GameDataTo 消息，让房主以为我们在通风管道中
		// 但对游戏中的其他所有玩家来说，我们仍然会显示为正在移动
		// 最大的问题是 CheckMurder RPC 是服务器权威的，而非房主权威的，这意味着我们在 PlayerControl::CheckMurder 函数中看到的检查，在后端 Among Us 服务器处理 CheckMurder RPC 时可能并不适用
		// 后端 Among Us 服务器确实会检查被击杀玩家是否在通风管道中，但不是通过 EnterVent 或 ExitVent RPC！
		// 相反，它们通过 ShipStatus 网络对象中的 VentilationSystem 系统来检查玩家是否在通风管道中
		// 当你的 Among Us 客户端进入通风管道时，你首先发送一个 EnterVent RPC，让你的玩家走向通风管道并进入，然后你的客户端发送一个 UpdateSystem RPC
		// 针对通风系统，操作为 Enter，这会告诉玩家你在通风管道中
		// 此功能用于通风管道清理功能，以确定哪些玩家应该被踢出通风管道
		// 但后端 Among Us 服务器在处理 CheckMurder RPC 时也用它来判断玩家是否在通风管道中
		// 所以当后端 Among Us 服务器收到 CheckMurder RPC 时，它会遍历给定大厅存在的所有网络对象列表，找到 ShipStatus，获取 VentilationSystem 的数据，并通过它判断玩家是否在通风管道中
		// 这里的服务器权威实际上对我们有帮助，因为我们之前的不死之身理论只会在房主眼中让我们不死，而这会让我们对所有在线玩家可见
		public Immortality() : base("不死之身") { }

		private static readonly int CUSTOM_VENT_ID = 50;

		private static Immortality Instance
		{
			get { return ModuleManager.immortality; }
		}

		[HarmonyPatch(typeof(VentilationSystem), nameof(VentilationSystem.Update))]
		class BlockSendingUpdates
		{
			static bool Prefix(VentilationSystem.Operation op, int ventId)
			{
				if(Instance.Enabled && ventId != CUSTOM_VENT_ID && (op == VentilationSystem.Operation.Enter || op == VentilationSystem.Operation.Exit || op == VentilationSystem.Operation.Move))
				{
					// Hydra.Log.LogInfo($"我们的客户端对通风管道 {ventId} 发送了通风系统操作 {op}，正在重新发送不死之身 RPC");
					// VentilationSystem.Update(VentilationSystem.Operation.Enter, CUSTOM_VENT_ID);

					Hydra.Log.LogInfo($"我们的客户端对通风管道 {ventId} 发送了通风系统操作 {op}，正在取消..");
					return false;
				}

				return true;
			}
		}

		private void OnGameLoad()
		{
			Hydra.Log.LogMessage($"新的 ShipStatus 实例已生成，发送不死之身 RPC");
			VentilationSystem.Update(VentilationSystem.Operation.Enter, CUSTOM_VENT_ID);
		}

		private void OnPlayerMurder(PlayerControl murderer, PlayerControl victim, MurderResultFlags flags)
		{
			if(victim != PlayerControl.LocalPlayer) return;

			Hydra.notifications.Send("不死之身", $"{murderer.Data.PlayerName} 试图击杀你!", 5);
		}

		private void OnMeetingEnd()
		{
			if(PlayerControl.LocalPlayer.Data.IsDead) return;

			Hydra.Log.LogInfo("会议已结束，重新发送不死之身 RPC 以保持不死状态");
			VentilationSystem.Update(VentilationSystem.Operation.Enter, CUSTOM_VENT_ID);
		}

		protected override void OnEnable()
		{
			EventCoordinator.OnGameLoad += OnGameLoad;
			EventCoordinator.OnPlayerMurder += OnPlayerMurder;
			EventCoordinator.OnMeetingEnd += OnMeetingEnd;

			if(PlayerControl.LocalPlayer != null)
			{
				Hydra.Log.LogInfo("不死之身已启用，发送进入操作的通风系统更新");
				VentilationSystem.Update(VentilationSystem.Operation.Enter, CUSTOM_VENT_ID);
			}
		}

		protected override void OnDisable()
		{
			EventCoordinator.OnGameLoad -= OnGameLoad;
			EventCoordinator.OnPlayerMurder -= OnPlayerMurder;
			EventCoordinator.OnMeetingEnd -= OnMeetingEnd;

			if(PlayerControl.LocalPlayer != null)
			{
				Hydra.Log.LogInfo("不死之身已禁用，发送退出操作的通风系统更新");
				VentilationSystem.Update(VentilationSystem.Operation.Exit, CUSTOM_VENT_ID);
			}
		}
	}
}