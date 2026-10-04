using HydraMenu.modules;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class GeneralSection : Section
	{
		public GeneralSection() : base("通用") { }

		public override void Render() {
			GUILayout.Label("欢迎使用 Hydra！Hydra 是一款用于提升《Among Us》游戏体验的功能、管理与恶搞菜单。我们提供便利性功能、可以和熟人朋友私下整活找乐子的功能，以及帮助你保护房间免受恶意玩家破坏的功能。其中大部分功能属于恶搞类，因为我本人就是把它做来和朋友在私人房间使用的。希望你（以及和你一起玩的人）也能在用 Hydra 游玩、或保护自己免受作弊者侵害时玩得开心。\n\n由于 Hydra 的部分功能可用于作弊，我必须明确说明：Hydra 绝不应被用来破坏其他玩家的游戏体验。有些人可能辛苦工作或学习了一天，只想轻松地玩一局《Among Us》。用 Hydra 去摧毁房间，是在毁掉别人的一天、剥夺他们的乐趣。如果这还不足以说服你，那么你应该清楚：出于恶意滥用模组可能会导致你的账号受到封禁处罚。");

			ModuleManager.chatLogger.Enabled = GUILayout.Toggle(ModuleManager.chatLogger.Enabled, "将聊天消息记录到控制台");

			if(GUILayout.Button("清除通知"))
			{
				Hydra.notifications.ClearNotifications();
				Hydra.notifications.Send("通知", "所有通知已清除。", 5);
			}
		}
	}
}