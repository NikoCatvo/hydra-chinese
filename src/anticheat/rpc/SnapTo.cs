using Hazel;
using System;

namespace HydraMenu.anticheat.rpc
{
	internal class SnapTo : RpcCheck
	{
		public override bool Validate(PlayerControl player, MessageReader reader)
		{
			// Vector2 position = NetHelpers.ReadVector2(reader);
			// ushort seqId = reader.ReadUInt16();

			if(LobbyBehaviour.Instance != null)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 在大厅内发送了 SnapTo RPC。");

				// 我们无法在原版服务器上使用其他玩家的 NetTransform net id 发送 SnapTo RPC
				if(AmongUsClient.Instance.AmHost && !Utilities.IsAnticheatPresent())
				{
					player.NetTransform.RpcSnapTo(player.transform.position);
				}

				return false;
			}

			return true;
		}

		public override RpcCalls GetId()
		{
			return RpcCalls.SnapTo;
		}

		public override Type GetExpectedNetObject()
		{
			return typeof(CustomNetworkTransform);
		}
	}
}