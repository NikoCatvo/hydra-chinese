using Hazel;
using System;

namespace HydraMenu.anticheat.rpc
{
	internal class CloseDoorsOfType : RpcCheck
	{
		// 此处 player 为 null，因为我们无法确定是谁发送了此 RPC
		public override bool Validate(PlayerControl player, MessageReader reader)
		{
			// 如果我们能添加额外检查会很不错，例如非内鬼关门
			// 但我们无法确定是谁发送了 CloseDoorsOfType RPC
			if(GameManager.Instance.IsHideAndSeek())
			{
				Anticheat.Flag("有人在躲猫猫模式中尝试关门。");
				return false;
			}

			return true;
		}

		public override RpcCalls GetId()
		{
			return RpcCalls.CloseDoorsOfType;
		}

		public override Type GetExpectedNetObject()
		{
			return typeof(ShipStatus);
		}
	}
}