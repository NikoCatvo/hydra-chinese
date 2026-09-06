using Hazel;
using System;

namespace HydraMenu.anticheat.rpc
{
	internal class ExitVent : RpcCheck
	{
		// 发送 ExitVent RPC 可用于让玩家传送到区域，而无需发送 SnapTo RPC
		public override bool Validate(PlayerControl player, MessageReader reader)
		{
			if(ShipStatus.Instance == null)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 在不存在 ShipStatus 实例时尝试退出通风管道。");
				return false;
			}

			if(!player.Data.IsDead && !player.Data.Role.CanVent)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 在其角色 ({player.Data.RoleType}) 不支持通风管道时尝试退出通风管道。");
				return false;
			}

			if(GameManager.Instance.IsHideAndSeek() && RoleManager.IsImpostorRole(player.Data.RoleType))
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 作为搜寻者时尝试退出通风管道。");
				return false;
			}

			return true;
		}

		public override RpcCalls GetId()
		{
			return RpcCalls.ExitVent;
		}

		public override Type GetExpectedNetObject()
		{
			return typeof(PlayerPhysics);
		}
	}
}