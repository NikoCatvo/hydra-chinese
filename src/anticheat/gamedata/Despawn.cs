using AmongUs.InnerNet.GameDataMessages;
using Hazel;
using HydraMenu.network;
using InnerNet;

namespace HydraMenu.anticheat.gamedata
{
	internal class Despawn : GameDataCheck
	{
		public override bool Validate(MessageReader reader)
		{
			uint netId = reader.ReadPackedUInt32();
			InnerNetObject netObject = AmongUsClient.Instance.FindObjectByNetId<InnerNetObject>(netId);
			if(netObject == null || netObject.SpawnId == (uint)SpawnType.NetworkedPlayerInfo) return true;

			// Only the host should be despawning net objects
			if(AmongUsClient.Instance.AmHost)
			{
				Anticheat.Flag($"作为房主时收到了网络对象 {(SpawnType)netObject.SpawnId}（网络 ID：{netObject.NetId}）的销毁请求。");
				return false;
			}

			// Nobody should be despawning net objects we own (our PlayerControl, PlayerPhysics, or CustomNetworkTransform)
			bool isVanilla = Utilities.IsAnticheatPresent();
			if(isVanilla && netObject.AmOwner)
			{
				Anticheat.Flag($"收到了属于我们自己的网络对象 {(SpawnType)netObject.SpawnId}（网络 ID：{netObject.NetId}）的销毁请求。");
				return false;
			}

			// The only time a net object is despawned in vanilla lobbies is when the host despawns the LobbyBehavior when the game starts
			if(isVanilla && netObject.SpawnId != (uint)SpawnType.LobbyBehavior)
			{
				Anticheat.Flag($"收到了意料之外的网络对象 {(SpawnType)netObject.SpawnId}（网络 ID：{netObject.NetId}）的销毁请求。");
				return false;
			}

			return true;
		}

		public override GameDataTypes GetId()
		{
			return GameDataTypes.DespawnFlag;
		}
	}
}