using Hazel;
using System;

namespace HydraMenu.anticheat
{
	internal abstract class RpcCheck : ICheck
	{
		public virtual bool Enabled { get; set; } = true;

		public virtual bool Validate(PlayerControl player, MessageReader reader)
		{
			return true;
		}

		public abstract RpcCalls GetId();

		public virtual bool IsHostOnly()
		{
			return false;
		}

		public virtual Type GetExpectedNetObject()
		{
// PlayerControl 网络对象的 RPC 比任何其他网络对象都多
		// 为方便起见，除非另有说明，每个 RpcCheck 实例都针对 PlayerControl 网络对象
			return typeof(PlayerControl);
		}
	}
}