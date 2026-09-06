using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class GeneralSection : Section
	{
		public GeneralSection() : base("常规") { }

		public override void Render() {
			GUILayout.Label("欢迎使用 Hydra！Hydra 是一个实用、管理和恶搞菜单，旨在增强 Among Us 的游戏体验。我们提供生活质量功能、供你和一群朋友一起玩乐的功能，以及帮助保护你的大厅免受恶意玩家侵害的功能。大部分功能属于恶搞类别，因为我个人制作这个主要是为了和朋友在私人大厅里玩。希望你（以及你一起玩的人）也能享受使用 Hydra 游戏或保护自己免受黑客侵害的乐趣。\n\n既然 Hydra 的某些功能可用于作弊，我必须明确说明：Hydra 不应被用于损害其他玩家的体验。有些人可能刚下完班或放学，只想轻松玩一局 Among Us。如果你用 Hydra 破坏大厅，就是在毁掉别人的一天，剥夺他们的乐趣。如果这还不够说服你，请知悉：滥用模组进行恶意行为可能导致你的账号受到制裁。");

			if(GUILayout.Button("清除通知"))
			{
				Hydra.notifications.ClearNotifications();
				Hydra.notifications.Send("通知", "所有通知已清除。", 5);
			}
		}
	}
}