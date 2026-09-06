using HarmonyLib;
using Hazel;
using HydraMenu.network;

namespace HydraMenu.modules.troll
{
	internal class DisableCloseDoors : Module
	{
		public DisableCloseDoors() : base("禁用关门") { }

		private static DisableCloseDoors Instance
		{
			get { return ModuleManager.disableCloseDoors; }
		}

		[HarmonyPatch(typeof(ShipStatus), nameof(ShipStatus.CloseDoorsOfType))]
		class OnCloseDoor
		{
			static bool Prefix()
			{
				return !Instance.Enabled;
			}
		}

		// 虽然我们作为非房主无法阻止门真正被关闭，但我们可以在门被锁定后立即修复它们
		[HarmonyPatch(typeof(DoorsSystemType), nameof(DoorsSystemType.Deserialize))]
		class DoorsDeserialize
		{
			static void Prefix(MessageReader reader)
			{
				if(!Instance.Enabled || AmongUsClient.Instance.AmHost) return;

				int oldReadPosition = reader.Position;

				int systems = (int)reader.ReadByte();
				if(systems > ShipStatus.Instance.Systems.Count || systems > reader.BytesRemaining) goto end;

				// 一个字节的系统类型标识符，一个浮点数（四字节）用于系统冷却时间
				reader.Position += systems * (1 + 4);

				int packingLimit = AmongUsClient.Instance.GetMaxMessagePackingLimit();
				BatchedMessage batch = new BatchedMessage(AmongUsClient.Instance.HostId);

				for(int i = 0; i < ShipStatus.Instance.AllDoors.Count; i++)
				{
					bool isOpen = reader.ReadBoolean();
					if(isOpen) continue;

					if(batch.msgCount >= packingLimit)
					{
						batch.FinishBatch();
						batch = new BatchedMessage(AmongUsClient.Instance.HostId);
					}

					batch.QueueUpdateSystem(PlayerControl.LocalPlayer, SystemTypes.Doors, (byte)(i | 64));
				}

				batch.FinishBatch();

				end:
				reader.Position = oldReadPosition;
			}
		}

		protected override void OnEnable()
		{
			if(!Sabotage.CanUnlockDoors())
			{
				Hydra.notifications.Send("禁用关门", "禁用关门仅在大厅房主，或处于 Polus、Airship、Fungle 地图时有效。");
			}
		}
	}
}