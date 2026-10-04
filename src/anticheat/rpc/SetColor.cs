using Hazel;

namespace HydraMenu.anticheat.rpc
{
	internal class SetColor : RpcCheck
	{
		public override bool Validate(PlayerControl player, MessageReader reader)
		{
			uint netId = reader.ReadUInt32();
			byte color = reader.ReadByte();

			// This net id field written in the RPC is seemingly useless as the client RPC handler does not do anything with this value
			if(netId != player.Data.NetId)
			{
				Anticheat.Flag(player, $"针对 {player.Data.PlayerName} 发送的 SetColor RPC 包含无效的网络 ID：预期 {player.Data.NetId}，收到 {netId}", false);
				player.SetColor((byte)CrewmateColor.Red);
				return false;
			}

			if(color >= Palette.ColorNames.Length)
			{
				Anticheat.Flag(player, $"针对 {player.Data.PlayerName} 发送的 SetColor RPC 包含无效的颜色值：{color}", false);
				player.SetColor((byte)CrewmateColor.Red);
				return false;
			}

			return true;
		}

		public override RpcCalls GetId()
		{
			return RpcCalls.SetColor;
		}

		public override bool IsHostOnly()
		{
			return true;
		}
	}
}