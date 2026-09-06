using HydraMenu.network;
using Il2CppInterop.Runtime;
using System.Collections.Generic;

namespace HydraMenu
{
	internal class Sabotage
	{
		/*
		当你作为内鬼进行破坏时，你的游戏会使用破坏的系统 ID 作为 amount 字段来更新 SystemTypes.Sabotage，并将其发送给房主
		房主接收到系统更新后，会检查游戏当前是否处于会议中，或破坏是否处于冷却中，如果满足任一条件则阻止破坏
		如果所有检查都通过，房主会直接更新系统（例如如果你破坏了反应堆，房主会更新 SystemTypes.Reactor）并向所有在线客户端广播更新
		为了绕过会议和冷却检查，我们可以直接自己更新系统，房主会将其中继给所有在线客户端
		*/
		public static bool UpdateSystemsDirectly { get; set; } = true;

		public static Dictionary<string, SystemTypes> skeldSabotages = new Dictionary<string, SystemTypes>()
		{
			{ "反应堆", SystemTypes.Reactor },
			{ "氧气", SystemTypes.LifeSupp },
			{ "灯光", SystemTypes.Electrical },
			{ "通讯", SystemTypes.Comms }
		};

		public static Dictionary<string, SystemTypes> skeldDoors = new Dictionary<string, SystemTypes>()
		{
			{ "餐厅", SystemTypes.Cafeteria },
			{ "储物室", SystemTypes.Storage },
			{ "医务室", SystemTypes.MedBay },
			{ "安保室", SystemTypes.Security },
			{ "上层引擎", SystemTypes.UpperEngine },
			{ "下层引擎", SystemTypes.LowerEngine },
			{ "电力室", SystemTypes.Electrical }
		};

		public static Dictionary<string, SystemTypes> miraSabotages = new Dictionary<string, SystemTypes>()
		{
			{ "反应堆", SystemTypes.Reactor },
			{ "氧气", SystemTypes.LifeSupp },
			{ "灯光", SystemTypes.Electrical },
			{ "通讯", SystemTypes.Comms }
		};

		public static Dictionary<string, SystemTypes> polusSabotages = new Dictionary<string, SystemTypes>()
		{
			{ "反应堆", SystemTypes.Laboratory },
			{ "灯光", SystemTypes.Electrical },
			{ "通讯", SystemTypes.Comms }
		};

		public static Dictionary<string, SystemTypes> polusDoors = new Dictionary<string, SystemTypes>()
		{
			{ "办公室", SystemTypes.Office },
			{ "通讯", SystemTypes.Comms },
			{ "实验室", SystemTypes.Laboratory },
			{ "消毒室", SystemTypes.Decontamination },
			{ "电力室", SystemTypes.Electrical },
			{ "氧气", SystemTypes.LifeSupp },
			{ "武器室", SystemTypes.Weapons },
			{ "储物室", SystemTypes.Storage }
		};

		public static Dictionary<string, SystemTypes> airshipSabotages = new Dictionary<string, SystemTypes>()
		{
			{ "反应堆", SystemTypes.HeliSabotage },
			{ "灯光", SystemTypes.Electrical },
			{ "通讯", SystemTypes.Comms }
		};

		public static Dictionary<string, SystemTypes> airshipDoors = new Dictionary<string, SystemTypes>()
		{
			{ "禁闭室", SystemTypes.Brig },
			{ "档案室", SystemTypes.Records },
			{ "通讯", SystemTypes.Comms },
			{ "主厅", SystemTypes.MainHall },
			{ "厨房", SystemTypes.Kitchen },
			{ "医疗室", SystemTypes.Medical },
			{ "休息室", SystemTypes.Lounge }
		};

		public static Dictionary<string, SystemTypes> fungleSabotages = new Dictionary<string, SystemTypes>()
		{
			{ "反应堆", SystemTypes.Reactor },
			{ "通讯", SystemTypes.Comms },
			{ "蘑菇混乱", SystemTypes.MushroomMixupSabotage }
		};

		public static Dictionary<string, SystemTypes> fungleDoors = new Dictionary<string, SystemTypes>()
		{
			{ "储物室", SystemTypes.Storage },
			{ "厨房", SystemTypes.Kitchen },
			{ "实验室", SystemTypes.Laboratory },
			{ "瞭望台", SystemTypes.Lookout },
			{ "矿坑", SystemTypes.MiningPit },
			{ "通讯", SystemTypes.Comms },
			{ "反应堆", SystemTypes.Reactor }
		};

		public static Dictionary<string, SystemTypes> GetSabotages()
		{
			MapNames map = Utilities.GetCurrentMap();
			return map switch
			{
				MapNames.Skeld or MapNames.Dleks => skeldSabotages,
				MapNames.MiraHQ => miraSabotages,
				MapNames.Polus => polusSabotages,
				MapNames.Airship => airshipSabotages,
				MapNames.Fungle => fungleSabotages,
				// 如果当前地图没有任何破坏，则默认使用 Skeld 的
				_ => skeldSabotages,
			};
		}

		public static Dictionary<string, SystemTypes> GetDoors()
		{
			MapNames map = Utilities.GetCurrentMap();
			return map switch
			{
				MapNames.Skeld or MapNames.Dleks => skeldDoors,
				// Mira has no closable doors
				MapNames.MiraHQ => [],
				MapNames.Polus => polusDoors,
				MapNames.Airship => airshipDoors,
				MapNames.Fungle => fungleDoors,
				// If we don't have any doors for the current map then just default to the Skeld ones
				_ => skeldDoors,
			};
		}

		// 我以为也许可以检查 ShipStatus::Systems 是否包含门系统类型的条目
		// 结果发现 Skeld 即使没有可解锁的门也有门系统类型
		public static bool CanUnlockDoors()
		{
			MapNames map = Utilities.GetCurrentMap();
			return AmongUsClient.Instance.AmHost || map == MapNames.Polus || map == MapNames.Airship || map == MapNames.Fungle;
		}

		public static void SabotageSystem(SystemTypes system, int targetClientId = -2)
		{
			if(!UpdateSystemsDirectly)
			{
				ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Sabotage, (byte)system);
				return;
			}

			BatchedMessage batch = new BatchedMessage(targetClientId == -2 ? AmongUsClient.Instance.HostId : targetClientId);

			switch(system)
			{
				case SystemTypes.Reactor:
				case SystemTypes.Laboratory:
				case SystemTypes.HeliSabotage:
				case SystemTypes.LifeSupp:
				case SystemTypes.Comms:
					batch.QueueUpdateSystem(PlayerControl.LocalPlayer, system, 128);
					break;

				// Electrical sabotage requires us to update each individual light switch
				// The following code comes from SabotageSystemType::UpdateSystem
				case SystemTypes.Electrical:
					byte amount = 4;

					for(byte i = 0; i < 5; i++)
					{
						if(BoolRange.Next(0.5f))
						{
							amount |= (byte)(1 << i);
						}
					}

					batch.QueueUpdateSystem(PlayerControl.LocalPlayer, SystemTypes.Electrical, (byte)(amount | 128));
					break;

				case SystemTypes.MushroomMixupSabotage:
					batch.QueueUpdateSystem(PlayerControl.LocalPlayer, system, (byte)MushroomMixupSabotageSystem.Operation.TriggerSabotage);
					break;

				default:
					Hydra.Log.LogError($"尝试破坏未知系统 {system}");
					break;
			}

			batch.FinishBatch();
		}

		public static void FixSabotage(SystemTypes system, int targetClientId = -2)
		{
			BatchedMessage batch = new BatchedMessage(targetClientId == -2 ? AmongUsClient.Instance.HostId : targetClientId);

			switch(system)
			{
				// ShipStatus::RepairCriticalSabotages uses amount value of 16 to instantly fix sabotages
				// This amount value should only be sent by the host, so this can be detected by anticheats
				case SystemTypes.Reactor:
				case SystemTypes.Laboratory:
				case SystemTypes.LifeSupp:
					batch.QueueUpdateSystem(PlayerControl.LocalPlayer, system, 16);
					break;

				// Comms in Mira HQ and HeliSabotage require two different updates in order to complete
				case SystemTypes.Comms:
				case SystemTypes.HeliSabotage:
					batch.QueueUpdateSystem(PlayerControl.LocalPlayer, system, 16 | 0);
					batch.QueueUpdateSystem(PlayerControl.LocalPlayer, system, 16 | 1);
					break;

				case SystemTypes.Electrical:
					SwitchSystem switches = ShipStatus.Instance.Systems[SystemTypes.Electrical].Cast<SwitchSystem>();

					// Hydra.Log.LogMessage($"Actual: {switches.ActualSwitches}, expected: {switches.ExpectedSwitches}");
					int amount = switches.ActualSwitches ^ switches.ExpectedSwitches;

					if(amount == 0)
					{
						Hydra.Log.LogInfo($"尝试修复灯光，异或操作为 0，无需修复");
						break;
					}

					// If the 8th bit is off, then the amount value is the index of the light switch (so 0, 1, 2, 3, or 4, and potentially 5 or 6 if those were to ever get added) that should get toggled
					// If it is on, then the amount value is a binary representation of what switches should be toggled
					// So if we had an amount value of 172 (which in binary is 1000 1101), that would mean light switches 0, 2, and 3 would be toggled
					// I don't think the 8th bit is actually ever used outside SabotageSystemType, so anticheats can detect this
					batch.QueueUpdateSystem(PlayerControl.LocalPlayer, SystemTypes.Electrical, (byte)(amount | 128));
					break;

				case SystemTypes.MushroomMixupSabotage:
					if(!AmongUsClient.Instance.AmHost)
					{
						Hydra.Log.LogInfo("尝试修复蘑菇混乱，但我们不是房主，无法处理");
						break;
					}

					MushroomMixupSabotageSystem mixupSystem = ShipStatus.Instance.Systems[SystemTypes.MushroomMixupSabotage].Cast<MushroomMixupSabotageSystem>();

					if(!mixupSystem.IsActive)
					{
						Hydra.Log.LogInfo("尝试修复蘑菇混乱，破坏未启用，无需修复");
						break;
					}

					Hydra.Log.LogInfo("尝试修复蘑菇混乱，我们是房主，可以修复");

					mixupSystem.currentSecondsUntilHeal = 0.1f;
					mixupSystem.IsDirty = true;
					break;

				default:
					Hydra.Log.LogError($"尝试修复未知破坏系统 {system}");
					break;
			}

			batch.FinishBatch();
		}

		public static bool IsSabotageActive(SystemTypes system)
		{
			ShipStatus.Instance.Systems.TryGetValue(system, out ISystemType systemType);
			if(systemType == null) return false;

			IActivatable activableSystem = systemType.TryCast<IActivatable>();
			if(activableSystem == null)
			{
				Hydra.Log.LogError($"所有破坏类型应继承自 IActivatable，但 {system} 并非如此");
				return false;
			}

			return activableSystem.IsActive;
		}

		public static void LockDoor(SystemTypes door)
		{
			ShipStatus.Instance.RpcCloseDoorsOfType(door);
		}

		public static void UnlockDoor(SystemTypes system)
		{
			for(byte i = 0; i < ShipStatus.Instance.AllDoors.Count; i++)
			{
				OpenableDoor door = ShipStatus.Instance.AllDoors[i];
				if(door.Room != system) continue;

				UnlockDoor(door);
			}
		}

		public static void UnlockDoor(int id)
		{
			MapNames currentMap = Utilities.GetCurrentMap();
			if(currentMap != MapNames.Skeld)
			{
				UnlockDoor(ShipStatus.Instance.AllDoors[id]);
				return;
			}

			// On Skeld, all doors have an id of 0, so unfortunately getting a door by its ID by using ShipStatus.Instance.AllDoors[id] won't work
			for(byte i = 0; i < ShipStatus.Instance.AllDoors.Count; i++)
			{
				OpenableDoor door = ShipStatus.Instance.AllDoors[i];
				if(door.Id == id) UnlockDoor(door, i);
			}
		}

		public static void UnlockDoor(OpenableDoor door, int index = 0)
		{
			if(!AmongUsClient.Instance.AmHost)
			{
				ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Doors, (byte)(door.Id | 64));
				return;
			}

			door.SetDoorway(true);

			Il2CppSystem.Type doorType = door.GetIl2CppType();
			if(doorType == Il2CppType.From(typeof(AutoOpenDoor)))
			{
				AutoDoorsSystemType doorSystem = ShipStatus.Instance.Systems[SystemTypes.Doors].Cast<AutoDoorsSystemType>();
				doorSystem.dirtyBits |= 1U << index;
			}
			else if(doorType == Il2CppType.From(typeof(PlainDoor)))
			{
				DoorsSystemType doorSystem = ShipStatus.Instance.Systems[SystemTypes.Doors].Cast<DoorsSystemType>();
				doorSystem.IsDirty = true;
			}
			else
			{
				Hydra.Log.LogError($"门类型 {doorType.FullName} 未知，无法标记为脏");
			}
		}

		public static void SabotageAll()
		{
			Dictionary<string, SystemTypes> sabotages = GetSabotages();
			foreach(SystemTypes system in sabotages.Values)
			{
				SabotageSystem(system);
			}
		}

		public static void FixAllSabotages()
		{
			Dictionary<string, SystemTypes> sabotages = GetSabotages();
			foreach(SystemTypes system in sabotages.Values)
			{
				FixSabotage(system);
			}
		}

		public static void LockAll()
		{
			Dictionary<string, SystemTypes> doors = GetDoors();

			BatchedMessage batch = new BatchedMessage();

			foreach(SystemTypes door in doors.Values)
			{
				batch.QueueCloseDoors(door);
			}

			batch.FinishBatch();
		}

		public static void UnlockAll()
		{
			for(byte i = 0; i < ShipStatus.Instance.AllDoors.Count; i++)
			{
				UnlockDoor(ShipStatus.Instance.AllDoors[i], i);
			}
		}
	}
}