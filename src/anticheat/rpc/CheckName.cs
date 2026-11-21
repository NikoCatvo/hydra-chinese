using Hazel;

namespace HydraMenu.anticheat.rpc
{
	internal class CheckName : RpcCheck
	{
		public readonly int MAX_NAME_LENGTH = 10;

		public override bool Validate(PlayerControl player, MessageReader reader)
		{
			string requestedName = reader.ReadString();

			/*
			ClientData clientData = AmongUsClient.Instance.GetClient(player.OwnerId);
			if(AmongUsClient.Instance.NetworkMode != NetworkModes.LocalGame && requestedName != clientData.PlayerName)
			{
				Anticheat.Flag(player, $"{clientData.PlayerName} 请求的名称与登录握手时的名称不一致。");
				player.SetName(clientData.PlayerName);
				return false;
			}
			*/

			if(requestedName.Length > MAX_NAME_LENGTH)
			{
				Anticheat.Flag(player, $"{requestedName} 尝试设置过长的名称（{requestedName.Length} 个字符）。");
				return false;
			}

			if(requestedName.Contains('<'))
			{
				Anticheat.Flag(player, $"{requestedName} 请求设置包含无效字符的名称。");
				return false;
			}

			return true;
		}

		public override RpcCalls GetId()
		{
			return RpcCalls.CheckName;
		}
	}
}