using HydraMenu.modules.host;
using HydraMenu.modules.protections;
using HydraMenu.modules.roles;
using HydraMenu.modules.self;
using HydraMenu.modules.spoofer;
using HydraMenu.modules.troll;
using HydraMenu.modules.visuals;
using System;
using System.Collections.Generic;
using System.Text.Json;
using UnityEngine;

namespace HydraMenu.modules
{
	internal class ModuleManager : MonoBehaviour
	{
		// Host
		public static AssignRoles assignRoles = new AssignRoles();
		public static BanMidGame banMidGame = new BanMidGame();
		public static BlockLowLevels blockLowLevels = new BlockLowLevels();
		public static DisableGameEnd disableGameEnd = new DisableGameEnd();
		public static DisableMeetings disableMeetings = new DisableMeetings();
		public static DisableVentClean disableVentClean = new DisableVentClean();
		public static FakeShapeshiftBubble fakeShapeshiftBubble = new FakeShapeshiftBubble();
		public static FlipSkeld flipSkeld = new FlipSkeld();
		public static VoteImmune voteImmune = new VoteImmune();

		// Protections
		public static AntiCrash antiCrash = new AntiCrash();
		public static AntiKick antiKick = new AntiKick();
		public static AntiOverload antiOverload = new AntiOverload();
		public static BlockServerTeleports blockServerTeleports = new BlockServerTeleports();
		public static BlockUnauthorizedUpdates blockUnauthorizedUpdates = new BlockUnauthorizedUpdates();
		public static BypassDisconnectPenalty bypassDisconnectPenalty = new BypassDisconnectPenalty();
		public static BypassShapeshiftRatelimits bypassShapeshiftRatelimits = new BypassShapeshiftRatelimits();
		public static ForceDTLs forceDtls = new ForceDTLs();

		// Roles
		public static MoveInVents moveInVents = new MoveInVents();
		public static NoKillChecks noKillChecks = new NoKillChecks();
		public static NoSabotageCooldown noSabotageCooldown = new NoSabotageCooldown();
		public static NoShapeshiftAnimation noShapeshiftAnimation = new NoShapeshiftAnimation();
		public static UnlockSabotageButton unlockSabotageButton = new UnlockSabotageButton();
		public static VentAsCrewmate ventAsCrewmate = new VentAsCrewmate();

		// Self
		public static AlwaysShowTaskAnimations alwaysShowTaskAnimations = new AlwaysShowTaskAnimations();
		public static Immortality immortality = new Immortality();
		public static NoLadderCooldown noLadderCooldown = new NoLadderCooldown();
		public static NoZiplineCooldown noZiplineCooldown = new NoZiplineCooldown();
		public static SpeedModifier speedModifier = new SpeedModifier();
		public static UnlimitedMeetings unlimitedMeetings = new UnlimitedMeetings();
		public static UpdateStatsFreeplay updateStatsFreeplay = new UpdateStatsFreeplay();

		// Spoofer
		public static SpoofDevice spoofDevice = new SpoofDevice();
		public static SpoofLevel spoofLevel = new SpoofLevel();
		public static SpoofVersion spoofVersion = new SpoofVersion();

		// Troll
		public static AutoExposeImpostors autoExposeImpostors = new AutoExposeImpostors();
		public static AutoReportBodies autoReportBodies = new AutoReportBodies();
		public static CrashLobby crashLobby = new CrashLobby();
		public static DisableCameras disableCameras = new DisableCameras();
		public static DisableCloseDoors disableCloseDoors = new DisableCloseDoors();
		public static DisableSabotages disableSabotages = new DisableSabotages();
		public static DisableVents disableVents = new DisableVents();

		// Visual
		public static AccurateDisconnectReason accurateDisconnectReason = new AccurateDisconnectReason();
		public static AlwaysVisibleChat alwaysVisibleChat = new AlwaysVisibleChat();
		public static Fullbright fullbright = new Fullbright();
		public static NoSeekerAnimation noSeekerAnimation = new NoSeekerAnimation();
		public static ShowGhostMessages showGhostMessages = new ShowGhostMessages();
		public static ShowGhosts showGhosts = new ShowGhosts();
		public static ShowProtections showProtections = new ShowProtections();
		public static SkipShhhAnimation skipShhhAnimation = new SkipShhhAnimation();
		public static SpectatePlayer spectatePlayer = new SpectatePlayer();

		public static readonly Module[] moduleList;

		static ModuleManager()
		{
			moduleList = [
				assignRoles,
				banMidGame,
				blockLowLevels,
				disableGameEnd,
				disableMeetings,
				disableVentClean,
				fakeShapeshiftBubble,
				flipSkeld,
				voteImmune,

				antiCrash,
				antiKick,
				antiOverload,
				blockServerTeleports,
				blockUnauthorizedUpdates,
				bypassDisconnectPenalty,
				bypassShapeshiftRatelimits,
				forceDtls,

				moveInVents,
				noKillChecks,
				noSabotageCooldown,
				noShapeshiftAnimation,
				unlockSabotageButton,
				ventAsCrewmate,

				alwaysShowTaskAnimations,
				immortality,
				noLadderCooldown,
				noZiplineCooldown,
				speedModifier,
				unlimitedMeetings,
				updateStatsFreeplay,

				spoofDevice,
				spoofLevel,
				spoofVersion,

				autoExposeImpostors,
				autoReportBodies,
				crashLobby,
				disableCameras,
				disableCloseDoors,
				disableSabotages,
				disableVents,

				accurateDisconnectReason,
				alwaysVisibleChat,
				fullbright,
				noSeekerAnimation,
				showGhostMessages,
				showGhosts,
				showProtections,
				skipShhhAnimation,
				spectatePlayer
			];
		}

		// 返回每个模块及其名称的字典，以及每个属性名称和值的字典
		public Dictionary<string, Dictionary<string, JsonElement>> GetConfigData()
		{
			Dictionary<string, Dictionary<string, JsonElement>> moduleConfig = new Dictionary<string, Dictionary<string, JsonElement>>();

			foreach(Module module in moduleList)
			{
				moduleConfig.Add(module.name, module.GetConfigData());
			}

			return moduleConfig;
		}

		public void LoadConfigData(Dictionary<string, Dictionary<string, JsonElement>> moduleConfig)
		{
			foreach((string moduleName, Dictionary<string, JsonElement> configData) in moduleConfig)
			{
				int moduleIndex = Array.FindIndex(moduleList, r => r.name == moduleName);
				if(moduleIndex == -1)
				{
					Hydra.Log.LogWarning($"配置中存在模块 {moduleName} 的条目，但不存在此模块");
					continue;
				}

				Module module = moduleList[moduleIndex];
				module.LoadConfigData(configData);
			}
		}

		// 不知道还能把这放哪
		public void Update()
		{
			// 如果 PlayerControl::Data 不为 null，则玩家已完全加载进游戏
			if(PlayerControl.LocalPlayer == null || PlayerControl.LocalPlayer.Data == null) return;

			if(unlockSabotageButton.SabotageAsCrewmate) HudManager.Instance.SabotageButton.gameObject.SetActive(true);
			if(ventAsCrewmate.Enabled) HudManager.Instance.ImpostorVentButton.gameObject.SetActive(true);

			// 聊天按钮和比赛信息按钮在游戏中同时激活时会重叠（但在会议中不会）
			// 我尝试修改 `MatchInfoButton.transform.position` 等来尝试将按钮向左移动
			// 但那只移动了按钮的碰撞体，而非图标
			// 所以我们只用这个变通方法，在不会与聊天按钮重叠的情况下隐藏比赛信息按钮
			if(alwaysVisibleChat.Enabled)
			{
				HudManager.Instance.MatchInfoButton.gameObject.SetActive(MeetingHud.Instance != null);
			}
		}
	}
}