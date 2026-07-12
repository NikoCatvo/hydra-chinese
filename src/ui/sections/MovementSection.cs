using HydraMenu.features;
using System.Collections.Generic;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class MovementSection : ISection
	{
		public MovementSection() : base("移动") { }

		public override void Render()
		{
			if(PlayerControl.LocalPlayer == null)
			{
				GUILayout.Label("你当前不在游戏中，这些选项将无法生效。");

				GUILayout.Toggle(false, "穿墙");
			}
			else
			{
				Vector2 position = PlayerControl.LocalPlayer.transform.position;

				GUILayout.Label($"当前地图: {Utilities.GetCurrentMap()}\n当前坐标:\nX: {position.x:F2}\nY: {position.y:F2}");

				PlayerControl.LocalPlayer.Collider.enabled = !GUILayout.Toggle(!PlayerControl.LocalPlayer.Collider.enabled, "穿墙");
			}

			GUILayout.Label($"速度倍率: {Self.PlayerSpeedModifier.Multiplier:F2}x");
			Self.PlayerSpeedModifier.Multiplier = GUILayout.HorizontalSlider(Self.PlayerSpeedModifier.Multiplier, 0f, 5f);

			HydraMenu.Teleporter.UseSnapToRPC = GUILayout.Toggle(HydraMenu.Teleporter.UseSnapToRPC, "传送时使用 SnapTo RPC");
			GUILayout.Label("传送到指定位置:");

			Dictionary<string, Vector2> teleportLocations = HydraMenu.Teleporter.GetTeleportLocations();
			Controls.DrawButtonCell(teleportLocations, HandleTeleport, 2);
		}

		private void HandleTeleport(Vector2 location)
		{
			HydraMenu.Teleporter.TeleportTo(location);
		}
	}
}
