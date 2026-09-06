using HydraMenu.modules;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class ProtectionsSection : Section
	{
		public ProtectionsSection() : base("防护") { }

		public override void Render()
		{
			// 网络
			ModuleManager.forceDtls.Enabled = GUILayout.Toggle(ModuleManager.forceDtls.Enabled, "强制启用 DTLS 以加密网络数据");

			ModuleManager.bypassDisconnectPenalty.Enabled = GUILayout.Toggle(ModuleManager.bypassDisconnectPenalty.Enabled, "绕过断开连接惩罚");
			ModuleManager.blockServerTeleports.Enabled = GUILayout.Toggle(ModuleManager.blockServerTeleports.Enabled, "阻止来自服务器的位置更新");
			ModuleManager.blockUnauthorizedUpdates.Enabled = GUILayout.Toggle(ModuleManager.blockUnauthorizedUpdates.Enabled, "阻止未经授权的系统更新");
			ModuleManager.bypassShapeshiftRatelimits.Enabled = GUILayout.Toggle(ModuleManager.bypassShapeshiftRatelimits.Enabled, "绕过 Shapeshift RPC 的速率限制");

			ModuleManager.antiCrash.Enabled = GUILayout.Toggle(ModuleManager.antiCrash.Enabled, "抵御 ReportDeadBody 大厅崩溃漏洞");

			// 过载防护
			GUILayout.Space(5);
			GUILayout.Label("防过载:");
			ModuleManager.antiOverload.Enabled = GUILayout.Toggle(ModuleManager.antiOverload.Enabled, "启用");
			ModuleManager.antiOverload.BlockLargeGameMessages = GUILayout.Toggle(ModuleManager.antiOverload.BlockLargeGameMessages, "阻止大型游戏消息");
			ModuleManager.antiOverload.BlockInvalidGameDataMessages = GUILayout.Toggle(ModuleManager.antiOverload.BlockInvalidGameDataMessages, "阻止无效的游戏数据消息类型");
			ModuleManager.antiOverload.HardenedPacketUIntDeserializer = GUILayout.Toggle(ModuleManager.antiOverload.HardenedPacketUIntDeserializer, "使用加固的压缩整数反序列化器");
			ModuleManager.antiOverload.BlockVotingCompleteOverload = GUILayout.Toggle(ModuleManager.antiOverload.BlockVotingCompleteOverload, "抵御 VotingComplete 过载");

			// 踢人漏洞防护
			GUILayout.Space(5);
			GUILayout.Label("防踢出:");
			ModuleManager.antiKick.Enabled = GUILayout.Toggle(ModuleManager.antiKick.Enabled, "启用");
			ModuleManager.antiKick.BlockVotekicks = GUILayout.Toggle(ModuleManager.antiKick.BlockVotekicks, "作为房主抵御投票踢人");
			ModuleManager.antiKick.BlockHostOnlyRpcExploit = GUILayout.Toggle(ModuleManager.antiKick.BlockHostOnlyRpcExploit, "抵御仅房主 RPC 踢人漏洞");
		}
	}
}