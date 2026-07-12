using HydraMenu.features;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class VisualSection : ISection
	{
		public VisualSection() : base("视觉") { }

		public override void Render()
		{
			Visuals.SkipShhhAnimation.Enabled = GUILayout.Toggle(Visuals.SkipShhhAnimation.Enabled, "跳过嘘声动画");
			Visuals.AccurateDisconnectReasons.Enabled = GUILayout.Toggle(Visuals.AccurateDisconnectReasons.Enabled, "使用更精确的断线原因");

			Visuals.Fullbright.Enabled = GUILayout.Toggle(Visuals.Fullbright.Enabled, "全亮");
			Visuals.ShowProtections.Enabled = GUILayout.Toggle(Visuals.ShowProtections.Enabled, "显示守护天使的保护");

			Chat.AlwaysVisibleChat.Enabled = GUILayout.Toggle(Chat.AlwaysVisibleChat.Enabled, "始终显示聊天框");

			Visuals.ShowGhosts.Enabled = GUILayout.Toggle(Visuals.ShowGhosts.Enabled, "显示幽灵");
			Chat.OnChat.ShowMessagesByGhosts = GUILayout.Toggle(Chat.OnChat.ShowMessagesByGhosts, "显示幽灵发送的消息");
		}
	}
}
