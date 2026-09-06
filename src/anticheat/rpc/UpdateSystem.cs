using Hazel;
using Il2CppInterop.Runtime;
using InnerNet;
using System;
using System.Collections.Generic;
using System.Linq;
using static HydraMenu.network.Constants;

namespace HydraMenu.anticheat.rpc
{
	internal class UpdateSystem : RpcCheck
	{
		// TODO: 也许把变量名改短一点 lol？
		private static readonly SystemTypes[] SystemsThatCanBeUpdatedWhenDead = {
			SystemTypes.MedBay,
			SystemTypes.Sabotage,
			// 幽灵关闭摄像头时更新 Security 系统，但打开时不更新
			SystemTypes.Security,
			SystemTypes.Ventilation
		};

		private static readonly Dictionary<Il2CppSystem.Type, Func<PlayerControl, MessageReader, bool>> systemHandlers = new Dictionary<Il2CppSystem.Type, Func<PlayerControl, MessageReader, bool>>()
		{
			{ Il2CppType.From(typeof(HudOverrideSystemType)), ValidateHudOverrideSystem },
			{ Il2CppType.From(typeof(LifeSuppSystemType)), ValidateLifeSuppSystem },
			{ Il2CppType.From(typeof(MushroomMixupSabotageSystem)), ValidateMushroomMixupSystem },
			{ Il2CppType.From(typeof(ReactorSystemType)), ValidateReactorSystem},
			{ Il2CppType.From(typeof(SabotageSystemType)), ValidateSabotageSystem },
			{ Il2CppType.From(typeof(SwitchSystem)), ValidateSwitchSystem }
		};

		public override bool Validate(PlayerControl player, MessageReader reader)
		{
			SystemTypes system = (SystemTypes)reader.ReadByte();
			player = reader.ReadNetObject<PlayerControl>();

			ShipStatus.Instance.Systems.TryGetValue(system, out ISystemType systemInterface);
			if(systemInterface == null)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 在当前地图无此系统时尝试更新系统 {system}。");
				return false;
			}

			if(player.Data.IsDead && !SystemsThatCanBeUpdatedWhenDead.Contains(system))
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 死亡时尝试更新系统 {system}。");
				return false;
			}

			// thanks https://discord.com/channels/623153565053222947/754333645199900723/1292612730418757785
			Il2CppSystem.Type systemType = Il2CppType.TypeFromPointer(systemInterface.ObjectClass);

			systemHandlers.TryGetValue(systemType, out Func<PlayerControl, MessageReader, bool> handler);
			if(handler == null) return true;

			return handler(player, reader);
		}

		private static bool ValidateHudOverrideSystem(PlayerControl player, MessageReader reader)
		{
			HudOverrideSystemOperation operation = (HudOverrideSystemOperation)reader.ReadByte();

			if(operation.HasFlag(HudOverrideSystemOperation.Sabotage))
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 尝试强制触发反应堆破坏");
				return false;
			}

			return true;
		}

		private static bool ValidateLifeSuppSystem(PlayerControl player, MessageReader reader)
		{
			LifeSuppSystemOperation operation = (LifeSuppSystemOperation)reader.ReadByte();

			if(operation.HasFlag(LifeSuppSystemOperation.Fix))
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 尝试强制修复氧气破坏");
				return false;
			}

			if(operation.HasFlag(LifeSuppSystemOperation.Sabotage))
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 尝试强制触发氧气破坏");
				return false;
			}

			return true;
		}

		// 蘑菇混乱系统仅由房主在 SabotageSystemType::Update 函数中更新。玩家永远不应发送它
		private static bool ValidateMushroomMixupSystem(PlayerControl player, MessageReader reader)
		{
			MushroomMixupSabotageSystem.Operation operation = (MushroomMixupSabotageSystem.Operation)reader.ReadByte();

			Anticheat.Flag(player, $"{player.Data.PlayerName} 尝试以操作 {operation} 更新蘑菇混乱系统。");
			return false;
		}

		private static bool ValidateReactorSystem(PlayerControl player, MessageReader reader)
		{
			ReactorSystemOperation operation = (ReactorSystemOperation)reader.ReadByte();

			if(operation.HasFlag(ReactorSystemOperation.Fix))
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 尝试强制修复反应堆破坏");
				return false;
			}

			if(operation.HasFlag(ReactorSystemOperation.Sabotage))
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 尝试强制触发反应堆破坏");
				return false;
			}

			return true;
		}

		private static bool ValidateSabotageSystem(PlayerControl player, MessageReader reader)
		{
			SystemTypes system = (SystemTypes)reader.ReadByte();

			Dictionary<string, SystemTypes> validSabotages = Sabotage.GetSabotages();
			if(!validSabotages.ContainsValue(system))
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 尝试破坏无效系统: {system}。");
				return false;
			}

			if(!RoleManager.IsImpostorRole(player.Data.RoleType))
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 在非内鬼时尝试破坏 {system}。");
				return false;
			}

			if(GameManager.Instance.IsHideAndSeek())
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 在躲猫猫模式中尝试破坏 {system}。");
				return false;
			}

			if(player.inVent)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 在通风管道中尝试破坏 {system}。");
				return false;
			}

			return true;
		}

		private static bool ValidateSwitchSystem(PlayerControl player, MessageReader reader)
		{
			byte switches = reader.ReadByte();

			if(switches.HasBit(128))
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 尝试批量更新开关: {switches}。");
				return false;
			}

			if(switches > 5)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 尝试切换无效开关: {switches}。");
				return false;
			}

			// 当灯光当前未被破坏时阻止开关更新
			// 如果玩家尝试修复灯光但未收到破坏已修复的消息，此检查可能会误报
			// 这也是为什么你可能会遇到灯光刚修好就又坏了的 bug
			// 为避免误封玩家，我们只是静默标记并阻止 RPC，以防黑客强制破坏灯光
			SwitchSystem system = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();
			if(system.ExpectedSwitches == system.ActualSwitches)
			{
				Hydra.Log.LogInfo($"已阻止来自 {player.Data.PlayerName} 的开关更新，因为灯光当前未被破坏");
				return false;
			}

			// 如果玩家在其客户端接收到 StartMeeting RPC 前切换灯光开关，可能会误报，所以我们静默标记
			// 也许我们可以检查会议处于什么状态，如果在会议动画播放后则标记玩家？
			if(MeetingHud.Instance)
			{
				Hydra.Log.LogInfo($"已阻止来自 {player.Data.PlayerName} 的开关更新，因为当前有进行中的会议");
				return false;
			}

			return true;
		}

		public override RpcCalls GetId()
		{
			return RpcCalls.UpdateSystem;
		}

		public override Type GetExpectedNetObject()
		{
			return typeof(ShipStatus);
		}
	}
}