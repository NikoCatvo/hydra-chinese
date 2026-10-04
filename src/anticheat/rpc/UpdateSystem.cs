using Hazel;
using HydraMenu.network;
using Il2CppInterop.Runtime;
using InnerNet;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HydraMenu.anticheat.rpc
{
	internal class UpdateSystem : RpcCheck
	{
		// TODO: Maybe change the variable name to something shorter lol?
		private static readonly SystemTypes[] SystemsThatCanBeUpdatedWhenDead = {
			SystemTypes.MedBay,
			SystemTypes.Sabotage,
			// Ghosts update the Security system when closing cameras, but not opening them
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
				Anticheat.Flag(player, $"{player.Data.PlayerName} 尝试更新系统 {HydraMenu.ui.Lang.Tr(system)}，但当前地图不存在该系统。");
				return false;
			}

			if(player.Data.IsDead && !SystemsThatCanBeUpdatedWhenDead.Contains(system))
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 在死亡状态下尝试更新系统 {HydraMenu.ui.Lang.Tr(system)}。");
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

		// The Mushroom Mixup system is only updated in the SabotageSystemType::Update function by the host. It should never be sent by a player
		private static bool ValidateMushroomMixupSystem(PlayerControl player, MessageReader reader)
		{
			MushroomMixupSabotageSystem.Operation operation = (MushroomMixupSabotageSystem.Operation)reader.ReadByte();

			Anticheat.Flag(player, $"{player.Data.PlayerName} 尝试以操作 {operation} 更新蘑菇混战系统。");
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
				Anticheat.Flag(player, $"{player.Data.PlayerName} 尝试破坏无效的系统：{HydraMenu.ui.Lang.Tr(system)}。");
				return false;
			}

			if(!RoleManager.IsImpostorRole(player.Data.RoleType))
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 并非内鬼，却尝试破坏 {HydraMenu.ui.Lang.Tr(system)}。");
				return false;
			}

			if(GameManager.Instance.IsHideAndSeek())
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 在躲猫猫模式中尝试破坏 {HydraMenu.ui.Lang.Tr(system)}。");
				return false;
			}

			if(player.inVent)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 在管道内尝试破坏 {HydraMenu.ui.Lang.Tr(system)}。");
				return false;
			}

			return true;
		}

		private static bool ValidateSwitchSystem(PlayerControl player, MessageReader reader)
		{
			byte switches = reader.ReadByte();

			if(switches.HasBit(128))
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 尝试批量更新电灯开关：{switches}。");
				return false;
			}

			if(switches > 5)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 尝试切换无效的电灯开关：{switches}。");
				return false;
			}

			// Block light switch updates when lights are currently not sabotaged
			// It is possible for this check to false flag if a player is attempting to fix lights when they have not received the message about the sabotage being fixed
			// This is also why you may experience the bug where lights get unfixed right after they get fixed
			// So to avoid wrongly banning players, we just silent flag and block the RPC to prevent hackers from being able to force sabotage lights
			if(!Sabotage.IsSabotageActive(SystemTypes.Electrical))
			{
				Hydra.Log.LogInfo($"Blocked switch update from {player.Data.PlayerName} as lights are not currently sabotaged");
				return false;
			}

			// False positives may be possible if a player is toggling light switches before their client receives the StartMeeting RPC so we silent flag
			// Maybe we can check to see what state the meeting is in, and if it is after the meeting was animated then flag the player?
			if(MeetingHud.Instance)
			{
				Hydra.Log.LogInfo($"Blocked switch update from {player.Data.PlayerName} as there is a currently active meeting");
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