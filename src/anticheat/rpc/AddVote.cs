using Hazel;
using InnerNet;
using System;

namespace HydraMenu.anticheat.rpc
{
	internal class AddVote : RpcCheck
	{
		public override bool Validate(PlayerControl player, MessageReader reader)
		{
			int source = reader.ReadInt32();
			int target = reader.ReadInt32();

			ClientData client = AmongUsClient.Instance.FindClientById(source);
			if(client == null || client.Character == null)
			{
				Hydra.Log.LogInfo($"未知客户端 ID ({source}) 尝试投票踢出 {target}");
				return false;
			}

			player = client.Character;

			if(player.Data.IsDead)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 死亡时尝试投票踢出玩家。");
				return false;
			}

			if(MeetingHud.Instance == null)
			{
				Anticheat.Flag(player, $"{player.Data.PlayerName} 在会议外尝试投票踢出玩家。");
				return false;
			}

			return true;
		}

		public override RpcCalls GetId()
		{
			return RpcCalls.AddVote;
		}

		public override Type GetExpectedNetObject()
		{
			return typeof(VoteBanSystem);
		}
	}
}