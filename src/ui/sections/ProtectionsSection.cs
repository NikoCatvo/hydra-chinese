using HydraMenu.modules;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class ProtectionsSection : Section
	{
		public ProtectionsSection() : base("防护") { }

		public override void Render()
		{
			// Network
			ModuleManager.forceDtls.Enabled = GUILayout.Toggle(ModuleManager.forceDtls.Enabled, "强制启用 DTLS 加密网络数据");

			ModuleManager.bypassDisconnectPenalty.Enabled = GUILayout.Toggle(ModuleManager.bypassDisconnectPenalty.Enabled, "绕过断线惩罚");
			ModuleManager.blockServerTeleports.Enabled = GUILayout.Toggle(ModuleManager.blockServerTeleports.Enabled, "拦截来自服务器的位置更新");
			ModuleManager.blockUnauthorizedUpdates.Enabled = GUILayout.Toggle(ModuleManager.blockUnauthorizedUpdates.Enabled, "拦截未授权的系统更新");
			ModuleManager.bypassShapeshiftRatelimits.Enabled = GUILayout.Toggle(ModuleManager.bypassShapeshiftRatelimits.Enabled, "绕过变形 RPC 的频率限制");

			ModuleManager.antiCrash.Enabled = GUILayout.Toggle(ModuleManager.antiCrash.Enabled, "防护 ReportDeadBody 房间崩溃漏洞");

			// Overloads
			GUILayout.Space(5);
			GUILayout.Label("防过载：");
			ModuleManager.antiOverload.Enabled = GUILayout.Toggle(ModuleManager.antiOverload.Enabled, "启用");
			ModuleManager.antiOverload.BlockLargeGameMessages = GUILayout.Toggle(ModuleManager.antiOverload.BlockLargeGameMessages, "拦截超大游戏消息");
			ModuleManager.antiOverload.BlockInvalidGameDataMessages = GUILayout.Toggle(ModuleManager.antiOverload.BlockInvalidGameDataMessages, "拦截无效的游戏数据消息类型");
			ModuleManager.antiOverload.HardenedPacketUIntDeserializer = GUILayout.Toggle(ModuleManager.antiOverload.HardenedPacketUIntDeserializer, "使用加固的打包整数反序列化器");
			ModuleManager.antiOverload.BlockVotingCompleteOverload = GUILayout.Toggle(ModuleManager.antiOverload.BlockVotingCompleteOverload, "防护 VotingComplete 过载攻击");

			// Kick Exploits
			GUILayout.Space(5);
			GUILayout.Label("防踢：");
			ModuleManager.antiKick.Enabled = GUILayout.Toggle(ModuleManager.antiKick.Enabled, "启用");
			ModuleManager.antiKick.BlockVotekicks = GUILayout.Toggle(ModuleManager.antiKick.BlockVotekicks, "防护针对房主的投票踢人");
			ModuleManager.antiKick.BlockHostOnlyRpcExploit = GUILayout.Toggle(ModuleManager.antiKick.BlockHostOnlyRpcExploit, "防护仅房主 RPC 踢人漏洞");
		}
	}
}