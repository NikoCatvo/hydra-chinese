using AmongUs.InnerNet.GameDataMessages;
using HarmonyLib;
using Hazel;
using HydraMenu.anticheat;
using HydraMenu.modules;
using InnerNet;

namespace HydraMenu.network
{
	internal class DataHandler
	{
		// 在原版服务器上，消息最大限制为 1201 字节
		// 在模组版 Nikocat 服务器上，消息最大限制为 1400 字节
		// 消息无论如何不应大于 1500 字节，因为此时可能因超过 MTU 大小而导致某些客户端无法接收
		// 1400 字节似乎是最佳平衡点，既能支持模组服务器，也能防止大消息攻击
		public static readonly int MAX_MESSAGE_LENGTH = 1400;

		[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.HandleGameData))]
		class HandleGameData
		{
			static bool Prefix(InnerNetClient __instance, MessageReader parentReader)
			{
				if(ModuleManager.antiOverload.Enabled && ModuleManager.antiOverload.BlockLargeGameMessages && parentReader.Length > MAX_MESSAGE_LENGTH)
				{
					parentReader.Recycle();
					return false;
				}

				try
				{
					while(parentReader.BytesRemaining > 0)
					{
						MessageReader reader = parentReader.ReadMessageAsNewBuffer();
						HandleGameDataInner(__instance, reader, ++__instance.msgNum);
					}
				}
				finally
				{
					parentReader.Recycle();
				}

				return false;
			}
		}

		public static void HandleGameDataInner(InnerNetClient innerNetClient, MessageReader reader, int msgNum)
		{
			GameDataTypes type = (GameDataTypes)reader.Tag;

			if(ModuleManager.antiOverload.Enabled && ModuleManager.antiOverload.BlockInvalidGameDataMessages && (type == GameDataTypes.Invalid || type == (GameDataTypes)3 || type > GameDataTypes.ReadyFlag))
			{
				reader.Recycle();
				return;
			}

			bool isValid = Anticheat.HandleGameData(type, reader);
			if(!isValid)
			{
				reader.Recycle();
				return;
			}

			innerNetClient.StartCoroutine(innerNetClient.HandleGameDataInner(reader, msgNum));
		}
	}
}