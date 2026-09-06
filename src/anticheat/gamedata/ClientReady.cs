using AmongUs.InnerNet.GameDataMessages;
using Hazel;
using InnerNet;

namespace HydraMenu.anticheat.gamedata
{
	internal class ClientReady : GameDataCheck
	{
		public override bool Validate(MessageReader reader)
		{
			int clientId = reader.ReadPackedInt32();

			ClientData client = AmongUsClient.Instance.FindClientById(clientId);
			if(client == null)
			{
				Anticheat.Flag($"收到未知客户端的 ClientReady 消息: {clientId}。");
				return false;
			}

			if(client.IsReady)
			{
				Anticheat.Flag(client.Character, $"{client.Character.Data.PlayerName} 在已准备就绪的情况下发送了 ClientReady 消息。");
				return false;
			}

			return true;
		}

		public override GameDataTypes GetId()
		{
			return GameDataTypes.ReadyFlag;
		}
	}
}