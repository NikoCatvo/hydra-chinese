using HydraMenu.features;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class ProtectionsSection : ISection
	{
		public ProtectionsSection() : base("防护") { }
		public override void Render()
		{
			// Network
			Protections.ForceDTLS.Enabled = GUILayout.Toggle(Protections.ForceDTLS.Enabled, "强制启用 DTLS 加密网络数据");

			Protections.BlockServerTeleports.Enabled = GUILayout.Toggle(Protections.BlockServerTeleports.Enabled, "阻止来自服务器的位置更新");
			Protections.BlockUnauthorizedSystemUpdates = GUILayout.Toggle(Protections.BlockUnauthorizedSystemUpdates, "阻止未授权的系统更新");

			// Overloads
			Protections.BlockLargeGameMessages = GUILayout.Toggle(Protections.BlockLargeGameMessages, "阻止超大游戏消息");
			Protections.BlockInvalidGameDataMessages = GUILayout.Toggle(Protections.BlockInvalidGameDataMessages, "阻止无效的游戏数据消息类型");
			Protections.HardenedReadPackedUInt.Enabled = GUILayout.Toggle(Protections.HardenedReadPackedUInt.Enabled, "使用加固的整数反序列化器");
			Protections.MemoryAllocationOverload.Enabled = GUILayout.Toggle(Protections.MemoryAllocationOverload.Enabled, "防护 VotingComplete 过载攻击");

			Protections.BypassShapeshiftRatelimits.Enabled = GUILayout.Toggle(Protections.BypassShapeshiftRatelimits.Enabled, "绕过变形 RPC 的速率限制");
			Protections.Votekicks.Enabled = GUILayout.Toggle(Protections.Votekicks.Enabled, "防止作为房主被投票踢出");
			Protections.ProtectAgainstNonHostKickExploit = GUILayout.Toggle(Protections.ProtectAgainstNonHostKickExploit, "防护非房主踢人漏洞");
		}
	}
}
