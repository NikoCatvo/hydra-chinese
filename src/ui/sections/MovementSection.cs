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
				GUILayout.Label("你当前不在游戏中，这些选项将无法使用。");

				// 即使玩家不在游戏中，也把所有可能选项一次显示出来是良好的用户体验
				GUILayout.Toggle(false, "穿墙");
			}
			else
			{
				// 我们不想要 PlayerControl::GetTruePosition() 包含玩家碰撞体的位置
				Vector2 position = PlayerControl.LocalPlayer.transform.position;

				GUILayout.Label($"当前地图: {Utilities.GetCurrentMap()}\n当前位置:\nX: {position.x:F2}\nY: {position.y:F2}");

				PlayerControl.LocalPlayer.Collider.enabled = !GUILayout.Toggle(!PlayerControl.LocalPlayer.Collider.enabled, "穿墙");
			}

			GUILayout.Label($"速度倍率: {ModuleManager.speedModifier.Multiplier:F2}倍");
			ModuleManager.speedModifier.Multiplier = GUILayout.HorizontalSlider(ModuleManager.speedModifier.Multiplier, 0f, 5f);

			Teleporter.UseSnapToRPC = GUILayout.Toggle(Teleporter.UseSnapToRPC, "对传送使用 SnapTo RPC");
			GUILayout.Label("传送到位置:");

			Dictionary<string, Vector2> teleportLocations = Teleporter.GetTeleportLocations();
			Controls.DrawButtonCell(teleportLocations, HandleTeleport, 2);
		}

		private void HandleTeleport(Vector2 location)
		{
			Teleporter.TeleportTo(location);
		}
	}
}