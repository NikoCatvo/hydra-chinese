using HydraMenu.modules;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class VisualSection : Section
	{
		public VisualSection() : base("视觉") { }

		public override void Render()
		{
			ModuleManager.skipShhhAnimation.Enabled = GUILayout.Toggle(ModuleManager.skipShhhAnimation.Enabled, "跳过嘘声动画");
			ModuleManager.noSeekerAnimation.Enabled = GUILayout.Toggle(ModuleManager.noSeekerAnimation.Enabled, "跳过寻找者动画");
			ModuleManager.accurateDisconnectReason.Enabled = GUILayout.Toggle(ModuleManager.accurateDisconnectReason.Enabled, "使用更准确的断开连接原因");

			ModuleManager.fullbright.Enabled = GUILayout.Toggle(ModuleManager.fullbright.Enabled, "全亮");
			ModuleManager.showProtections.Enabled = GUILayout.Toggle(ModuleManager.showProtections.Enabled, "显示守护天使保护");

			ModuleManager.alwaysVisibleChat.Enabled = GUILayout.Toggle(ModuleManager.alwaysVisibleChat.Enabled, "聊天框始终可见");

			ModuleManager.showGhosts.Enabled = GUILayout.Toggle(ModuleManager.showGhosts.Enabled, "显示幽灵");
			ModuleManager.showGhostMessages.Enabled = GUILayout.Toggle(ModuleManager.showGhostMessages.Enabled, "显示幽灵的消息");
		}
	}
}