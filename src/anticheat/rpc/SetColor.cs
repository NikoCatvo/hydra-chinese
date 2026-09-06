using Hazel;

namespace HydraMenu.anticheat.rpc
{
	internal class SetColor : RpcCheck
	{
		public override bool Validate(PlayerControl player, MessageReader reader)
		{
			uint netId = reader.ReadUInt32();
			byte color = reader.ReadByte();

			// RPC 中写入的这个 net id 字段看似毫无用处，因为客户端 RPC 处理程序不使用此值
			if(netId != player.Data.NetId)
			{
				Anticheat.Flag(player, $"为 {player.Data.PlayerName} 发送的 SetColor RPC 包含无效网络 ID，预期 {player.Data.NetId}，收到 {netId}", false);
				player.SetColor((byte)CrewmateColor.Red);
				return false;
			}

			if(color >= Palette.ColorNames.Length)
			{
				Anticheat.Flag(player, $"为 {player.Data.PlayerName} 发送的 SetColor RPC 包含无效颜色: {color}", false);
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