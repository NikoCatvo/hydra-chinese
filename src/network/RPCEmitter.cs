using Hazel;

namespace HydraMenu.network
{
	// 无需通过 BatchedMessage 创建新批次即可发送 RPC 的简便方法
	internal class RPCEmitter
	{
		// PlayerControl::RpcSetScanner 函数在可视化任务关闭时不发送 RPC
		// 如果我们想在可视化任务开启时也显示扫描动画，则需要重新实现它
		public static void SendSetScanner(bool scanning)
		{
			byte scanCount = ++PlayerControl.LocalPlayer.scannerCount;

			// 为我们自己渲染医疗扫描动画
			PlayerControl.LocalPlayer.SetScanner(scanning, scanCount);

			MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
				PlayerControl.LocalPlayer.NetId,
				(byte)RpcCalls.SetScanner,
				SendOption.Reliable,
				-1
			);

			writer.Write(scanning);
			writer.Write(scanCount);

			AmongUsClient.Instance.FinishRpcImmediately(writer);
		}

		// PlayerControl::RpcPlayAnimation 函数在可视化任务关闭时不发送 RPC
		// 如果我们想在可视化任务开启时也显示任务动画，则需要重新实现它
		public static void SendPlayAnimation(byte animation)
		{
			// 为我们自己渲染任务动画
			PlayerControl.LocalPlayer.PlayAnimation(animation);

			MessageWriter writer = AmongUsClient.Instance.StartRpcImmediately(
				PlayerControl.LocalPlayer.NetId,
				(byte)RpcCalls.PlayAnimation,
				SendOption.None,
				-1
			);

			writer.Write(animation);

			AmongUsClient.Instance.FinishRpcImmediately(writer);
		}
	}
}