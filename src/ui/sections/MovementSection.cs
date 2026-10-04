using HydraMenu.modules;
using System.Collections.Generic;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class MovementSection : Section
	{
		public MovementSection() : base("移动") { }

		public override void Render()
		{
			if(PlayerControl.LocalPlayer == null)
			{
				GUILayout.Label("你当前不在对局中，这些选项将无法使用。");

				// Having all possible options shown at once (even if the player isn't in a game) is nice user experience
				GUILayout.Toggle(false, "穿墙");
			}
			else
			{
				// We don't want the position that includes the player's collider from PlayerControl::GetTruePosition()
				Vector2 position = PlayerControl.LocalPlayer.transform.position;

				GUILayout.Label($"当前地图：{Lang.Tr(Utilities.GetCurrentMap())}\n当前位置：\nX: {position.x:F2}\nY: {position.y:F2}");

				PlayerControl.LocalPlayer.Collider.enabled = !GUILayout.Toggle(!PlayerControl.LocalPlayer.Collider.enabled, "穿墙");
			}

			GUILayout.Label($"速度倍率：{ModuleManager.speedModifier.Multiplier:F2}x");
			ModuleManager.speedModifier.Multiplier = GUILayout.HorizontalSlider(ModuleManager.speedModifier.Multiplier, 0f, 5f);

			Teleporter.UseSnapToRPC = GUILayout.Toggle(Teleporter.UseSnapToRPC, "传送时使用 SnapTo RPC");
			GUILayout.Label("传送到位置：");

			Dictionary<string, Vector2> teleportLocations = Teleporter.GetTeleportLocations();
			Controls.DrawButtonCell(teleportLocations, HandleTeleport, 2);
		}

		private void HandleTeleport(Vector2 location)
		{
			if(PlayerControl.LocalPlayer == null)
			{
				Hydra.notifications.Send("传送", "该选项只能在游戏中使用。");
				return;
			}

			Teleporter.TeleportTo(location);
		}
	}
}