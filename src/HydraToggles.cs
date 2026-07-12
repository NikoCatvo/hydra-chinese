namespace HydraMenu
{
    internal static class HydraToggles
    {
        public static void RegisterAll()
        {
            // General
            Profile.Register("logChatMessages",
                () => HydraMenu.features.Chat.OnChat.LogChatMessages,
                v => HydraMenu.features.Chat.OnChat.LogChatMessages = v);

            // Self
            Profile.Register("updateStatsFreeplay",
                () => HydraMenu.features.Self.UpdateStatsFreeplay.Enabled,
                v => HydraMenu.features.Self.UpdateStatsFreeplay.Enabled = v);
            Profile.Register("immortal",
                () => HydraMenu.features.Immortality.Enabled,
                v => HydraMenu.features.Immortality.Enabled = v);
            Profile.Register("alwaysShowTaskAnimations",
                () => HydraMenu.features.Self.AlwaysShowTaskAnimations,
                v => HydraMenu.features.Self.AlwaysShowTaskAnimations = v);
            Profile.Register("noLadderCooldown",
                () => HydraMenu.features.Self.NoLadderCooldown.Enabled,
                v => HydraMenu.features.Self.NoLadderCooldown.Enabled = v);
            Profile.Register("unlimitedMeetings",
                () => HydraMenu.features.Self.UnlimitedMeetings.enabled,
                v => HydraMenu.features.Self.UnlimitedMeetings.enabled = v);

            // Troll
            Profile.Register("autoTriggerSpores",
                () => Hydra.routines.autoTriggerSpores.Enabled,
                v => Hydra.routines.autoTriggerSpores.Enabled = v);
            Profile.Register("blockSabotages",
                () => HydraMenu.features.Troll.BlockSabotages.Enabled,
                v => HydraMenu.features.Troll.BlockSabotages.Enabled = v);
            Profile.Register("blockVenting",
                () => HydraMenu.features.Troll.BlockVenting.Enabled,
                v => HydraMenu.features.Troll.BlockVenting.Enabled = v);
            Profile.Register("teleportSpammer",
                () => Hydra.routines.teleportSpammer.Enabled,
                v => Hydra.routines.teleportSpammer.Enabled = v);
            Profile.Register("doorTroller",
                () => Hydra.routines.doorTroller.Enabled,
                v => Hydra.routines.doorTroller.Enabled = v);

            // Sabotage
            Profile.Register("updateSystemsDirectly",
                () => HydraMenu.Sabotage.UpdateSystemsDirectly,
                v => HydraMenu.Sabotage.UpdateSystemsDirectly = v);

            // Host
            Profile.Register("banMidGame",
                () => HydraMenu.features.Host.BanMidGame.Enabled,
                v => HydraMenu.features.Host.BanMidGame.Enabled = v);
            Profile.Register("flippedSkeld",
                () => HydraMenu.features.Host.FlippedSkeld,
                v => HydraMenu.features.Host.FlippedSkeld = v);
            Profile.Register("disableSabotages",
                () => HydraMenu.features.Host.DisableSabotages.Enabled,
                v => HydraMenu.features.Host.DisableSabotages.Enabled = v);
            Profile.Register("disableCloseDoors",
                () => HydraMenu.features.Host.DisableCloseDoors.Enabled,
                v => HydraMenu.features.Host.DisableCloseDoors.Enabled = v);
            Profile.Register("disableCameras",
                () => HydraMenu.features.Host.DisableCameras.Enabled,
                v => HydraMenu.features.Host.DisableCameras.Enabled = v);
            Profile.Register("disableGameEnd",
                () => HydraMenu.features.Host.DisableGameEnd.Enabled,
                v => HydraMenu.features.Host.DisableGameEnd.Enabled = v);
            Profile.Register("noKillCooldown",
                () => HydraMenu.features.Host.NoKillCooldown.Enabled,
                v => HydraMenu.features.Host.NoKillCooldown.Enabled = v);
            Profile.Register("disableMeetings",
                () => HydraMenu.features.Host.DisableMeetings.Enabled,
                v => HydraMenu.features.Host.DisableMeetings.Enabled = v);
            Profile.Register("reportBodySpam",
                () => Hydra.routines.reportBodySpam.Enabled,
                v => Hydra.routines.reportBodySpam.Enabled = v);

            // Movement
            Profile.Register("useSnapToRPC",
                () => HydraMenu.Teleporter.UseSnapToRPC,
                v => HydraMenu.Teleporter.UseSnapToRPC = v);

            // Role
            Profile.Register("allowVentingForCrewmates",
                () => HydraMenu.features.Roles.AllowVentingForCrewmates,
                v => HydraMenu.features.Roles.AllowVentingForCrewmates = v);
            Profile.Register("moveInVents",
                () => HydraMenu.features.Roles.MoveModifier.MoveInVents,
                v => HydraMenu.features.Roles.MoveModifier.MoveInVents = v);
            Profile.Register("sabotageAsCrewmate",
                () => HydraMenu.features.Roles.SkipSabotageChecks.SabotageAsCrewmate,
                v => HydraMenu.features.Roles.SkipSabotageChecks.SabotageAsCrewmate = v);
            Profile.Register("sabotageInVents",
                () => HydraMenu.features.Roles.SkipSabotageChecks.SabotageInVents,
                v => HydraMenu.features.Roles.SkipSabotageChecks.SabotageInVents = v);
            Profile.Register("disableShapeshiftAnimation",
                () => HydraMenu.features.Roles.DisableShapeshiftAnimation,
                v => HydraMenu.features.Roles.DisableShapeshiftAnimation = v);
            Profile.Register("noKillChecks",
                () => HydraMenu.features.Roles.NoKillChecks,
                v => HydraMenu.features.Roles.NoKillChecks = v);

            // Protections
            Profile.Register("forceDTLS",
                () => HydraMenu.features.Protections.ForceDTLS.Enabled,
                v => HydraMenu.features.Protections.ForceDTLS.Enabled = v);
            Profile.Register("blockServerTeleports",
                () => HydraMenu.features.Protections.BlockServerTeleports.Enabled,
                v => HydraMenu.features.Protections.BlockServerTeleports.Enabled = v);
            Profile.Register("blockUnauthorizedSystemUpdates",
                () => HydraMenu.features.Protections.BlockUnauthorizedSystemUpdates,
                v => HydraMenu.features.Protections.BlockUnauthorizedSystemUpdates = v);
            Profile.Register("blockLargeGameMessages",
                () => HydraMenu.features.Protections.BlockLargeGameMessages,
                v => HydraMenu.features.Protections.BlockLargeGameMessages = v);
            Profile.Register("blockInvalidGameDataMessages",
                () => HydraMenu.features.Protections.BlockInvalidGameDataMessages,
                v => HydraMenu.features.Protections.BlockInvalidGameDataMessages = v);
            Profile.Register("hardenedReadPackedUInt",
                () => HydraMenu.features.Protections.HardenedReadPackedUInt.Enabled,
                v => HydraMenu.features.Protections.HardenedReadPackedUInt.Enabled = v);
            Profile.Register("memoryAllocationOverload",
                () => HydraMenu.features.Protections.MemoryAllocationOverload.Enabled,
                v => HydraMenu.features.Protections.MemoryAllocationOverload.Enabled = v);
            Profile.Register("bypassShapeshiftRatelimits",
                () => HydraMenu.features.Protections.BypassShapeshiftRatelimits.Enabled,
                v => HydraMenu.features.Protections.BypassShapeshiftRatelimits.Enabled = v);
            Profile.Register("votekicks",
                () => HydraMenu.features.Protections.Votekicks.Enabled,
                v => HydraMenu.features.Protections.Votekicks.Enabled = v);
            Profile.Register("protectAgainstNonHostKickExploit",
                () => HydraMenu.features.Protections.ProtectAgainstNonHostKickExploit,
                v => HydraMenu.features.Protections.ProtectAgainstNonHostKickExploit = v);

            // Visual
            Profile.Register("skipShhhAnimation",
                () => HydraMenu.features.Visuals.SkipShhhAnimation.Enabled,
                v => HydraMenu.features.Visuals.SkipShhhAnimation.Enabled = v);
            Profile.Register("accurateDisconnectReasons",
                () => HydraMenu.features.Visuals.AccurateDisconnectReasons.Enabled,
                v => HydraMenu.features.Visuals.AccurateDisconnectReasons.Enabled = v);
            Profile.Register("fullbright",
                () => HydraMenu.features.Visuals.Fullbright.Enabled,
                v => HydraMenu.features.Visuals.Fullbright.Enabled = v);
            Profile.Register("showProtections",
                () => HydraMenu.features.Visuals.ShowProtections.Enabled,
                v => HydraMenu.features.Visuals.ShowProtections.Enabled = v);
            Profile.Register("alwaysVisibleChat",
                () => HydraMenu.features.Chat.AlwaysVisibleChat.Enabled,
                v => HydraMenu.features.Chat.AlwaysVisibleChat.Enabled = v);
            Profile.Register("showGhosts",
                () => HydraMenu.features.Visuals.ShowGhosts.Enabled,
                v => HydraMenu.features.Visuals.ShowGhosts.Enabled = v);
            Profile.Register("showMessagesByGhosts",
                () => HydraMenu.features.Chat.OnChat.ShowMessagesByGhosts,
                v => HydraMenu.features.Chat.OnChat.ShowMessagesByGhosts = v);

            // Spoofer
            Profile.Register("shouldSpoofVersion",
                () => HydraMenu.features.Spoofer.shouldSpoofVersion,
                v => HydraMenu.features.Spoofer.shouldSpoofVersion = v);
            Profile.Register("useModdedProtocol",
                () => HydraMenu.features.Spoofer.useModdedProtocol,
                v => HydraMenu.features.Spoofer.useModdedProtocol = v);
            Profile.Register("spoofLevel",
                () => HydraMenu.features.Spoofer.SpoofLevel.Enabled,
                v => HydraMenu.features.Spoofer.SpoofLevel.Enabled = v);

            // Anticheat
            Profile.Register("anticheatEnabled",
                () => HydraMenu.anticheat.Anticheat.Enabled,
                v => HydraMenu.anticheat.Anticheat.Enabled = v);
            Profile.Register("checkSpoofedPlatforms",
                () => HydraMenu.anticheat.Anticheat.CheckSpoofedPlatforms,
                v => HydraMenu.anticheat.Anticheat.CheckSpoofedPlatforms = v);
            Profile.Register("acSendNotification",
                () => HydraMenu.anticheat.Anticheat.sendNotification,
                v => HydraMenu.anticheat.Anticheat.sendNotification = v);
            Profile.Register("acDiscardRpc",
                () => HydraMenu.anticheat.Anticheat.discardRpc,
                v => HydraMenu.anticheat.Anticheat.discardRpc = v);

            // === 滑块和枚举值 ===

            // 菜单不透明度
            Profile.RegisterValue<float>("menuOpacity",
                () => HydraMenu.ui.Styles.menuOpacity,
                v => HydraMenu.ui.Styles.menuOpacity = v);

            // 界面缩放
            Profile.RegisterValue<float>("uiScale",
                () => HydraMenu.ui.MainUI.scale,
                v => HydraMenu.ui.MainUI.scale = v);

            // 主色调（枚举转 int）
            Profile.RegisterValue<int>("primaryColor",
                () => (int)HydraMenu.ui.Styles.primaryColor,
                v => HydraMenu.ui.Styles.primaryColor = (HydraMenu.ui.Styles.UIColors)v);

            // 速度倍率
            Profile.RegisterValue<float>("playerSpeedMultiplier",
                () => HydraMenu.features.Self.PlayerSpeedModifier.Multiplier,
                v => HydraMenu.features.Self.PlayerSpeedModifier.Multiplier = v);

            // 反作弊惩罚方式（枚举转 int）
            Profile.RegisterValue<int>("acPunishment",
                () => (int)HydraMenu.anticheat.Anticheat.punishment,
                v => HydraMenu.anticheat.Anticheat.punishment = (HydraMenu.anticheat.Anticheat.Punishments)v);

            // 门整蛊延迟
            Profile.RegisterValue<float>("doorTrollerDelay",
                () => Hydra.routines.doorTroller.lockAndUnlockDelay,
                v => Hydra.routines.doorTroller.lockAndUnlockDelay = v);

            // 蹦迪派对颜色随机化延迟
            Profile.RegisterValue<float>("discoHostDelay",
                () => Hydra.routines.discoHost.randomizationDelay,
                v => Hydra.routines.discoHost.randomizationDelay = v);

            // 等级伪装数值
            Profile.RegisterValue<uint>("spoofLevelValue",
                () => HydraMenu.features.Spoofer.SpoofLevel.newLevel,
                v => HydraMenu.features.Spoofer.SpoofLevel.newLevel = v);

            // 伪装版本选择（索引）
            Profile.RegisterValue<int>("spoofedVersion",
                () => HydraMenu.features.Spoofer.spoofedVersion,
                v => HydraMenu.features.Spoofer.spoofedVersion = v);

            // 伪装平台（枚举转 int）
            Profile.RegisterValue<int>("spoofedPlatform",
                () => (int)HydraMenu.features.Spoofer.spoofedPlatform,
                v => HydraMenu.features.Spoofer.spoofedPlatform = (Platforms)v);
        }
    }
}
