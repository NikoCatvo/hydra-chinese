using AmongUs.InnerNet.GameDataMessages;
using Hazel;
using InnerNet;

namespace HydraMenu.anticheat.gamedata
{
	internal class SceneChange : GameDataCheck
	{
		public override bool Validate(MessageReader reader)
		{
			int clientId = reader.ReadPackedInt32();
			string scene = reader.ReadString();

			ClientData client = AmongUsClient.Instance.FindClientById(clientId);
			if(client == null)
			{
				Anticheat.Flag($"收到未知客户端的 SceneChange 消息: {clientId}。");
				return false;
			}

			// 如果房主收到 Tutorial 场景切换，它会生成一个 Skeld 地图实例
			if(scene == "Tutorial")
			{
				Anticheat.Flag(client.Character, $"{client.Character.Data.PlayerName} 发送了 Tutorial 场景切换。");
				return false;
			}

			return true;
		}

		public override GameDataTypes GetId()
		{
			return GameDataTypes.SceneChangeFlag;
		}
	}
}