using AmongUs.GameOptions;
using Hazel;
using HydraMenu.modules;
using HydraMenu.network;

namespace HydraMenu
{
	internal class GameOptions
	{
		// 如果我们想要在不应用到自己身上的情况下自由修改 IGameOptions，我们需要克隆它
		// 可能有更好的方法，但我只是将游戏选项序列化为字节数组，再反序列化回 IGameOptions
		// 这样就能基于我们预先存在的选项得到一个新的 IGameOptions 实例
		public static IGameOptions CreateCloneOptions(IGameOptions options)
		{
			LogicOptions logicOptions = GameManager.Instance.LogicOptions;

			byte[] byteArray = logicOptions.gameOptionsFactory.ToBytes(options, AprilFoolsMode.IsAprilFoolsModeToggledOn);
			return logicOptions.gameOptionsFactory.FromBytes(byteArray);
		}

		// 仅向一个特定玩家发送游戏选项更新
		public static void SendGameOptionsToClient(IGameOptions options, int targetClientId)
		{
			// 在自由模式中，我们必须手动应用游戏选项，因为没有网络层
			// 自由模式有一些无法更改的设置，例如玩家视野，因此盲人玩家功能在那里无法工作
			if(AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay && targetClientId == PlayerControl.LocalPlayer.OwnerId)
			{
				GameManager.Instance.LogicOptions.SetGameOptions(options);
				return;
			}

			// 非常黑客的做法，但确保我们仍然拥有 Shapeshift 速率限制绕过
			// 如果我们尝试修改自己的游戏选项，这会产生影响我们大厅设置的意外行为
			if(ModuleManager.bypassShapeshiftRatelimits.Enabled) options.SetFloat(FloatOptionNames.ShapeshifterCooldown, 0.0f);

			MessageWriter writer = MessageWriter.Get(SendOption.Reliable);
			writer.StartMessage((byte)FindLogicOptionsIndex());
			writer.WriteBytesAndSize(GameManager.Instance.LogicOptions.gameOptionsFactory.ToBytes(options, AprilFoolsMode.IsAprilFoolsModeToggledOn));
			writer.EndMessage();

			BatchedMessage batch = new BatchedMessage(targetClientId);
			batch.QueueDataFlag(GameManager.Instance.NetId, writer);
			batch.FinishBatch();
		}

		private static int FindLogicOptionsIndex()
		{
			int logicIndex = -1;
			for(int i = 0; i < GameManager.Instance.LogicComponents.Count; i++)
			{
				GameLogicComponent component = GameManager.Instance.LogicComponents[i];

				Hydra.Log.LogMessage($"在索引 {i} 处找到组件 {component.GetType()}");
				if(component.GetType() != typeof(LogicOptions)) continue;

				logicIndex = i;
				break;
			}

			Hydra.Log.LogMessage($"在索引 {logicIndex} 处找到 LogicOptions");
			return logicIndex;
		}
	}
}