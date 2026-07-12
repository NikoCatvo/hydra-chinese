using HydraMenu.features;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class GeneralSection : ISection
	{
		public GeneralSection() : base("综合") { }

		public override void Render() {
			GUILayout.Label("欢迎使用 Hydra！Hydra 是一个用于提升 Among Us 游戏体验的实用、管理与整蛊菜单。我们提供各种便利功能、可以和一小群好友一起搞怪同乐的功能，以及帮助你守护房间、抵御恶意玩家的功能。其中大部分功能属于整蛊类，因为作者本人主要是在和朋友的私人房间里使用它。希望你（以及和你一起玩的人）也能用 Hydra 玩得开心，或者用它保护自己不受作弊者的侵扰。\n\n由于 Hydra 的部分功能可能被用于作弊，必须明确声明：请勿使用 Hydra 破坏其他玩家的游戏体验。有些人也许刚结束一天的工作或学习，只想安安静静玩一局 Among Us。用 Hydra 去毁掉房间，是在破坏别人的心情、剥夺他们的乐趣。如果这还不足以说服你，那你也该知道：滥用模组进行恶意行为，可能会导致你的账号受到封禁处罚。");

			Chat.OnChat.LogChatMessages = GUILayout.Toggle(Chat.OnChat.LogChatMessages, "在控制台记录聊天消息");

			if(GUILayout.Button("清除通知"))
			{
				Hydra.notifications.ClearNotifications();
				Hydra.notifications.Send("通知", "所有通知已清除。", 5);
			}
		}
	}
}
