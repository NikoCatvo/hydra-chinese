using AmongUs.GameOptions;
using Hazel;
using HydraMenu.network;
using InnerNet;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static HydraMenu.network.Constants;

namespace HydraMenu
{
	internal class Utilities
	{
		private static readonly Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<SkinData> allSkins = HatManager.Instance.allSkins;
		private static readonly Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<HatData> allHats = HatManager.Instance.allHats;
		private static readonly Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<VisorData> allVisors = HatManager.Instance.allVisors;
		private static readonly Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<PetData> allPets = HatManager.Instance.allPets;
		private static readonly Il2CppInterop.Runtime.InteropTypes.Arrays.Il2CppReferenceArray<NamePlateData> allNameplates = HatManager.Instance.allNamePlates;

		public static int GetRandomUnusedColor()
		{
			List<int> colors = Enumerable.Range(0, 18).ToList();

			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				colors.Remove(player.Data.DefaultOutfit.ColorId);
			}

			System.Random rnd = new System.Random();

			// 一些模组大厅可能有超过18名玩家，这意味着没有足够的独特颜色供每个人使用
			// 因此我们应该考虑到这种边缘情况
			return colors.Count != 0 ? colors[rnd.Next(0, colors.Count)] : rnd.Next(0, 18);
		}

		public static void RandomizePlayer(bool ingame = false)
		{
			System.Random rnd = new System.Random();

			if(ingame)
			{
				PlayerControl.LocalPlayer.CmdCheckColor((byte)GetRandomUnusedColor());

				PlayerControl.LocalPlayer.RpcSetHat(allHats[rnd.Next(0, allHats.Length)].ProductId);
				PlayerControl.LocalPlayer.RpcSetVisor(allVisors[rnd.Next(0, allVisors.Length)].ProductId);
				PlayerControl.LocalPlayer.RpcSetSkin(allSkins[rnd.Next(0, allSkins.Length)].ProductId);
				PlayerControl.LocalPlayer.RpcSetPet(allPets[rnd.Next(0, allPets.Length)].ProductId);
			}
			else
			{
				AccountManager.Instance.RandomizeName();

				PlayerCustomization.EquipHat(allHats[rnd.Next(0, allHats.Length)]);
				PlayerCustomization.EquipVisor(allVisors[rnd.Next(0, allVisors.Length)]);
				PlayerCustomization.EquipSkin(allSkins[rnd.Next(0, allSkins.Length)]);
				PlayerCustomization.EquipPet(allPets[rnd.Next(0, allPets.Length)]);
				PlayerCustomization.EquipNameplate(allNameplates[rnd.Next(0, allNameplates.Length)]);
			}
		}

		public static PlayerControl GetRandomPlayer(bool excludeHost = false, bool excludeDead = false, bool excludeImposters = false, bool excludeSelf = true)
		{
			Il2CppSystem.Collections.Generic.List<PlayerControl> allPlayers = PlayerControl.AllPlayerControls;
			List<PlayerControl> validPlayers = new List<PlayerControl>();

			foreach(PlayerControl player in allPlayers)
			{
				if(
					(excludeSelf && AmongUsClient.Instance.ClientId == player.OwnerId) ||
					(excludeHost && AmongUsClient.Instance.HostId == player.OwnerId) ||
					(excludeDead && player.Data.IsDead) ||
					(excludeImposters && player.Data.Role.CanUseKillButton)
				) continue;

				validPlayers.Add(player);
			}

			if(validPlayers.Count == 0) return null;

			System.Random rnd = new System.Random();
			return validPlayers[rnd.Next(validPlayers.Count)];
		}

		public static void CopyPlayer(PlayerControl player)
		{
			NetworkedPlayerInfo.PlayerOutfit outfit = player.CurrentOutfit;

			bool hasAnticheat = IsAnticheatPresent();

			BatchedMessage batch = new BatchedMessage();

			// 在服务器权威的大厅中，我们无法更改自己玩家的名字，即使作为房主也不行
			if(!hasAnticheat)
			{
				batch.QueueSetName(PlayerControl.LocalPlayer, outfit.PlayerName);
			}

			if(!hasAnticheat || AmongUsClient.Instance.AmHost)
			{
				batch.QueueSetColor(PlayerControl.LocalPlayer, (byte)outfit.ColorId);
			}

			batch.QueueSetNameplateStr(PlayerControl.LocalPlayer, outfit.NamePlateId, ++outfit.NamePlateSequenceId);
			batch.QueueSetHatStr(PlayerControl.LocalPlayer, outfit.HatId, ++outfit.HatSequenceId);
			batch.QueueSetVisorStr(PlayerControl.LocalPlayer, outfit.VisorId, ++outfit.VisorSequenceId);
			batch.QueueSetSkinStr(PlayerControl.LocalPlayer, outfit.SkinId, ++outfit.SkinSequenceId);
			batch.QueueSetPetStr(PlayerControl.LocalPlayer, outfit.PetId, ++outfit.PetSequenceId);

			batch.FinishBatch();
		}

		public static void AttemptStartMeeting(PlayerControl reporter, NetworkedPlayerInfo target)
		{
			Hydra.Log.LogInfo($"尝试为 {reporter.Data.PlayerName} 召开会议");

			bool hasAnticheat = IsAnticheatPresent();

			if(hasAnticheat && AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
			{
				Hydra.notifications.Send("召开会议", "游戏必须已开始，此功能才能生效。");
				return;
			}

			if(AmongUsClient.Instance.AmHost)
			{
				Hydra.Log.LogInfo($"我们是房主，可以直接使用 StartMeeting RPC");

				if(ShipStatus.Instance == null)
				{
					Hydra.notifications.Send("召开会议", "必须存在有效的 ShipStatus 实例才能使用此功能。");
				}
				else
				{
					OpenMeeting(reporter, target);
				}

				return;
			}

			Hydra.Log.LogInfo("我们不是房主，必须使用 ReportDeadBody RPC");

			if(hasAnticheat && reporter != PlayerControl.LocalPlayer)
			{
				Hydra.notifications.Send("召开会议", "你必须是大厅房主才能让另一名玩家召开会议。");
				return;
			}

			if(reporter.Data.IsDead)
			{
				Hydra.notifications.Send("召开会议", "只有存活时才能召集会议或报告尸体。");
				return;
			}

			if(hasAnticheat && target != null)
			{
				if(!target.IsDead)
				{
					Hydra.notifications.Send("召开会议", "只能报告本轮游戏中已死亡玩家的尸体。");
					return;
				}

				if(!DoesDeadBodyExist(target.PlayerId))
				{
					Hydra.notifications.Send("召开会议", "找不到该玩家的尸体，只能报告本轮已死亡且尸体未消失的玩家。");
					return;
				}
			}

			reporter.CmdReportDeadBody(target);
		}

		public static void OpenMeeting(PlayerControl reporter, NetworkedPlayerInfo target)
		{
			MeetingRoomManager.Instance.AssignSelf(reporter, target);
			reporter.RpcStartMeeting(target);
			HudManager.Instance.OpenMeetingRoom(reporter);
		}

		public static bool DoesDeadBodyExist(byte playerId)
		{
			foreach(Collider2D collider in Physics2D.OverlapCircleAll(new Vector2(0, 0), 99999f, Constants.PlayersOnlyMask))
			{
				if(collider.tag != "DeadBody") continue;

				DeadBody bodyComponent = collider.GetComponent<DeadBody>();
				if(bodyComponent && bodyComponent.ParentId == playerId)
				{
					return true;
				}
			}

			return false;
		}

		public static void ShapeshiftPlayer(PlayerControl victim, PlayerControl target, bool shouldAnimate = true)
		{
			bool hasAnticheat = IsAnticheatPresent();

			if(hasAnticheat && !AmongUsClient.Instance.AmHost)
			{
				Hydra.notifications.Send("变身玩家", "你必须是大厅房主才能使用此功能。");
				return;
			}

			if(hasAnticheat && AmongUsClient.Instance.GameState != InnerNetClient.GameStates.Started)
			{
				Hydra.notifications.Send("变身玩家", "游戏必须已开始才能使用此选项。");
				return;
			}

			BatchedMessage batch = new BatchedMessage();

			// 原版反作弊会在房主尝试为非 Shapeshifter 角色的玩家发送 Shapeshift RPC 时封禁房主
			// 为了绕过这一点，我们临时将玩家的角色更改为 Shapeshifter，使其变形，然后将其恢复为之前的角色
			if(hasAnticheat && victim.Data.RoleType != RoleTypes.Shapeshifter)
			{
				RoleTypes currentRole = victim.Data.RoleType;

				// 我们试图陷害的客户端不应注意到任何变化，因为在角色选择期间 SetRole RPC 是以 canOverrideRole 选项设为 false 发送的
				// 这意味着任何未来的 SetRole RPC 都将被忽略，除非新角色是幽灵角色
				// 以防将来此机制被更改，我们可以将 SetRole RPC 广播给一个垃圾客户端 ID 而不是所有人，以避免客户端知道他们成为了 Shapeshifter
				batch.QueueSetRole(victim, RoleTypes.Shapeshifter, true);
				batch.QueueShapeshift(victim, target, shouldAnimate);
				batch.QueueSetRole(victim, currentRole, true);
			}
			else
			{
				batch.QueueShapeshift(victim, target, shouldAnimate);
			}

			batch.FinishBatch();
		}

		public static MapNames GetCurrentMap()
		{
			// 如果 Among Us 尚未完全加载，则默认返回 The Skeld
			if(AmongUsClient.Instance == null) return MapNames.Skeld;

			// 如果 ShipStatus 不存在，则根据游戏选项回退到当前地图
			if(ShipStatus.Instance == null)
			{
				if(AmongUsClient.Instance.NetworkMode == NetworkModes.FreePlay)
				{
					return (MapNames)AmongUsClient.Instance.TutorialMapId;
				}
				else
				{
					return (MapNames)GameOptionsManager.Instance.CurrentGameOptions.MapId;
				}
			}

			return (SpawnType)ShipStatus.Instance.SpawnId switch
			{
				SpawnType.SkeldShipStatus => MapNames.Skeld,
				SpawnType.DleksShipStatus => MapNames.Dleks,
				SpawnType.MiraShipStatus => MapNames.MiraHQ,
				SpawnType.PolusShipStatus => MapNames.Polus,
				SpawnType.AirshipShipStatus => MapNames.Airship,
				SpawnType.FungleShipStatus => MapNames.Fungle,
				_ => MapNames.Skeld
			};
		}

		public static bool IsAnticheatPresent()
		{
			if(Constants.IsVersionModded() || PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null) return false;

			// 在 Freeplay、本地和模组大厅中，NetworkedPlayerInfo 网络对象由房主拥有 (-2)
			// 在原版大厅中，NetworkedPlayerInfo 网络对象由 Among Us 后端服务器拥有 (-4)
			// 如果我们的 NetworkedPlayerInfo 网络对象由房主拥有，我们可以假设该大厅具有宽松的反作弊机制，没有服务器权威
			// 因此不需要我们使用任何绕过手段
			return PlayerControl.LocalPlayer.Data.OwnerId != (int)OwnerIds.Host;
		}

		public static string GetPlayerColor(NetworkedPlayerInfo player)
		{
			int colorId = player.DefaultOutfit.ColorId;

			if(colorId < 0 || colorId >= Palette.ColorNames.Length)
			{
				return "Fortegreen";
			}

			return player.GetPlayerColorString();
		}

		// 这种踢出方法允许非大厅房主的玩家通过让对方触发 Among Us 反作弊来将其踢出大厅
		// 有多种 RPC 只能由大厅房主发送，例如 MurderPlayer、Shapeshift、ProtectPlayer 等
		// 这些 RPC 由房主作为对其客户端权威等价物的响应发送，例如 CheckMurder、CheckShapeshift、CheckProtect 等
		// 如果我们能让某个玩家在不作为大厅房主的情况下发送仅限房主的 RPC，我们就能让反作弊将其踢出大厅
		// 大多数 RPC 处理程序都有检查以确保客户端是大厅房主，正是为了避免这种利用
		// 然而通风系统的 UpdateSystem RPC 是一个例外
		// 发送操作为 StartCleaning 或 BootFromVent 的通风系统更新会导致房主发送 BootFromVent RPC，这正是这些仅限房主的 RPC 之一
		// 然而在从 ShipStatus::UpdateSystem 到 PlayerPhysics::RpcBootFromVent 的调用栈中，没有任何地方检查当前客户端是否是大厅房主
		// 如果你将此系统更新发送给房主以外的人，他们将发送 BootFromVent RPC 并被 Among Us 反作弊踢出
		// 根据我的经验，即使我不是大厅房主，这也非常有用，可以踢出那些公然作弊、召开无用会议或造成其他破坏的玩家
		public static void KickPlayer(PlayerControl player, bool skipFirstStage = false)
		{
			if(AmongUsClient.Instance.AmHost)
			{
				AmongUsClient.Instance.KickPlayer(player.OwnerId, true);
				Hydra.notifications.Send("踢出玩家", $"{player.Data.PlayerName} 已被踢出游戏。", 5);
				return;
			}

			if(player.OwnerId == AmongUsClient.Instance.HostId)
			{
				Hydra.notifications.Send("踢出玩家", "你不能踢出大厅的房主。");
				return;
			}

			if(ShipStatus.Instance == null)
			{
				Hydra.notifications.Send("踢出玩家", "游戏必须已开始才能使用此功能。");
				return;
			}

			if(!IsAnticheatPresent())
			{
				Hydra.notifications.Send("踢出玩家", "此功能仅在服务器权威的大厅中有效。");
				return;
			}

			BatchedMessage batch = new BatchedMessage(player.OwnerId);

			if(!skipFirstStage)
			{
				Hydra.Log.LogInfo($"向 {player.OwnerId} 发送进入通风系统更新");

				MessageWriter writer = MessageWriter.Get(SendOption.Reliable);
				writer.Write((ushort)0);
				writer.Write((byte)VentilationSystem.Operation.Enter);
				writer.Write((byte)0);

				batch.QueueUpdateSystem(PlayerControl.LocalPlayer, SystemTypes.Ventilation, writer);
				writer.Recycle();
			}

			Hydra.Log.LogInfo($"向 {player.OwnerId} 发送踢出内鬼通风系统更新");

			MessageWriter writer2 = MessageWriter.Get(SendOption.Reliable);
			writer2.Write((ushort)1);
			writer2.Write((byte)VentilationSystem.Operation.BootImpostors);
			writer2.Write((byte)0);

			batch.QueueUpdateSystem(PlayerControl.LocalPlayer, SystemTypes.Ventilation, writer2);
			writer2.Recycle();

			batch.FinishBatch();

			Hydra.notifications.Send("踢出玩家", $"{player.Data.PlayerName} 已被踢出游戏。", 5);
		}
	}
}