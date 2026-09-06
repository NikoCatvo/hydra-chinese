using Hazel;
using HydraMenu.modules;
using InnerNet;
using UnityEngine;

namespace HydraMenu.routines
{

	public class PetPlayerRoutine : Routine
	{
		public PetPlayerRoutine() : base("宠物玩家") { }

		// 此值不能太低，否则抚摸动画会卡顿且看起来不自然
		public readonly float PET_DELAY = 0.60f;

		public PlayerControl target;
		private float timeElapsed = 0.0f;

		public override void Run()
		{
			if(PlayerControl.LocalPlayer == null || target == null) return;

			timeElapsed += Time.deltaTime;
			if(timeElapsed < PET_DELAY) return;
			timeElapsed = 0.0f;

			Vector2 petPosition = target.transform.position;
			petPosition.y -= PlayerControl.LocalPlayer.cosmetics.currentPet.yOffset * 2;

			// PlayerPhysics::CoPet 函数调用 PlayerPhysics::CancelPet 函数
			// 这会将 PlayerControl::moveable 设为 true，允许玩家再次移动
			// 所以我们只需重新实现必要部分以显示抚摸手并发送 Pet RPC
			PlayerControl.LocalPlayer.cosmetics.CurrentPet.SetGettingPet(true, petPosition);
			PlayerControl.LocalPlayer.cosmetics.PettingHand.StartPet(PlayerControl.LocalPlayer.cosmetics.currentPet);

			MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
				PlayerControl.LocalPlayer.MyPhysics.NetId,
				(byte)RpcCalls.Pet,
				SendOption.Reliable,
				-1
			);

			NetHelpers.WriteVector2(PlayerControl.LocalPlayer.GetTruePosition(), writer);
			NetHelpers.WriteVector2(petPosition, writer);

			AmongUsClient.Instance.FinishRpcImmediately(writer);
		}

		private void OnDisconnect()
		{
			Hydra.notifications.Send("宠物玩家", "你离开了游戏，宠物玩家已禁用。", 10);
			Enabled = false;
		}

		private void OnPlayerDisconnect(ClientData client, DisconnectReasons reason)
		{
			if(client.Character != target) return;

			Hydra.notifications.Send("宠物玩家", "被你抚摸的玩家离开了游戏，宠物玩家已禁用。");
			Enabled = false;
		}

		protected override void OnEnable()
		{
			if(PlayerControl.LocalPlayer == null)
			{
				_enabled = false;
				return;
			}

			// 尝试移动会导致我们的抚摸手跟随移动
			// 为避免意外行为，我们阻止玩家移动
			PlayerControl.LocalPlayer.moveable = false;
			PlayerControl.LocalPlayer.NetTransform.body.velocity = Vector2.zero;

			EventCoordinator.OnDisconnect += OnDisconnect;
			EventCoordinator.OnPlayerDisconnect += OnPlayerDisconnect;
		}

		protected override void OnDisable()
		{
			target = null;

			if(PlayerControl.LocalPlayer != null)
			{
				PlayerControl.LocalPlayer.moveable = true;
				PlayerControl.LocalPlayer.MyPhysics.RpcCancelPet();
			}

			EventCoordinator.OnDisconnect -= OnDisconnect;
			EventCoordinator.OnPlayerDisconnect -= OnPlayerDisconnect;
		}
	}
}