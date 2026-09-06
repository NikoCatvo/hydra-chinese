using HarmonyLib;
using Hazel;

namespace HydraMenu.modules.protections
{
	internal class AntiOverload : Module
	{
		public AntiOverload() : base("防过载")
		{
			base.Enabled = true;
		}

		private static AntiOverload Instance
		{
			get { return ModuleManager.antiOverload; }
		}

		public bool BlockLargeGameMessages { get; set; } = true;
		public bool BlockInvalidGameDataMessages { get; set; } = true;
		public bool HardenedPacketUIntDeserializer { get; set; } = true;
		public bool BlockVotingCompleteOverload { get; set; } = true;

		[HarmonyPatch(typeof(MessageReader), nameof(MessageReader.ReadPackedUInt32))]
		class HardenedReadPackedUInt
		{
			static bool Prefix(MessageReader __instance, ref uint __result)
			{
				if(!Instance.Enabled || !Instance.HardenedPacketUIntDeserializer) return true;

				bool readMore = true;
				int shift = 0;
				uint output = 0;

				while(readMore)
				{
					if(__instance.BytesRemaining < 1) break;

					byte b = __instance.ReadByte();
					if(b >= 0x80)
					{
						readMore = true;
						b ^= 0x80;
					}
					else
					{
						readMore = false;
					}

					output |= (uint)(b << shift);
					shift += 7;
				}

				__result = output;
				return false;
			}
		}

		[HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.HandleRpc))]
		class MemoryAllocationOverload
		{
			static bool Prefix(byte callId, MessageReader reader)
			{
				if(!Instance.Enabled || !Instance.BlockVotingCompleteOverload || callId != (byte)RpcCalls.VotingComplete) return true;

				int oldReadPosition = reader.Position;

				// 游戏创建一个具有以下值大小的数组
				// 如果此值非常大，则客户端将尝试分配数 GB 的内存
				int arrayLength = reader.ReadPackedInt32();

				if(arrayLength > 1024 || arrayLength > reader.BytesRemaining)
				{
					return false;
				}

				reader.Position = oldReadPosition;
				return true;
			}
		}
	}
}