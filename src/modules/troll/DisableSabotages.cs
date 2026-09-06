using HarmonyLib;

namespace HydraMenu.modules.troll
{
	internal class DisableSabotages : Module
	{
		public DisableSabotages() : base("禁用破坏") { }

		public readonly float MINIMUM_TIMER_DURATION = 0.1f;
		// 可以是任何未分配给任何 SystemTypes 的值
		public readonly byte INVALID_SYSTEM_TYPE = 255;

		private static DisableSabotages Instance
		{
			get { return ModuleManager.disableSabotages; }
		}

		[HarmonyPatch(typeof(SabotageSystemType), nameof(SabotageSystemType.UpdateSystem))]
		class BlockSabotagesHost
		{
			static bool Prefix()
			{
				return !Instance.Enabled;
			}
		}

		// 当房主接收到破坏系统更新时，它首先确保没有进行中的会议，且破坏冷却已结束
		// 如果所有检查通过，房主将破坏冷却设为 30.0 秒，然后根据破坏类型处理要更新的系统
		// 唯一的问题是房主在未先确认尝试的破坏是否真正成功的情况下，就更新了破坏冷却
		// 这意味着如果我们破坏一个没有关联破坏的系统，房主只会重置破坏冷却
		// 我们可以利用这个缺陷，每次破坏冷却结束时破坏一个无效系统，来创建一个反破坏机制
		// 这会让内鬼几乎没有时间进行任何破坏
		[HarmonyPatch(typeof(SabotageSystemType), nameof(SabotageSystemType.Deserialize))]
		class BlockSabotagesNonHost
		{
			static void Postfix(SabotageSystemType __instance)
			{
				if(!Instance.Enabled || AmongUsClient.Instance.AmHost || __instance.Timer > Instance.MINIMUM_TIMER_DURATION || MeetingHud.Instance) return;

				Hydra.Log.LogMessage($"破坏冷却已减少至 {__instance.Timer}，发送破坏系统更新");
				ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Sabotage, Instance.INVALID_SYSTEM_TYPE);
			}
		}

		private void OnMeetingEnd()
		{
			Hydra.Log.LogMessage($"会议已结束，发送破坏系统更新");
			ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Sabotage, Instance.INVALID_SYSTEM_TYPE);
		}

		protected override void OnEnable()
		{
			EventCoordinator.OnMeetingEnd += OnMeetingEnd;

			if(ShipStatus.Instance != null)
			{
				ISystemType system = ShipStatus.Instance.Systems[SystemTypes.Sabotage];
				SabotageSystemType sabotageSystem = system.Cast<SabotageSystemType>();

				if(sabotageSystem.Timer > MINIMUM_TIMER_DURATION)
				{
					ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Sabotage, INVALID_SYSTEM_TYPE);
				}
			}
		}

		protected override void OnDisable()
		{
			EventCoordinator.OnMeetingEnd -= OnMeetingEnd;
		}
	}
}