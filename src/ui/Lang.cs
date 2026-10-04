using System.Collections.Generic;

namespace HydraMenu.ui
{
	// 汉化 by 微生猫
	// 用于把枚举等运行时对象显示成中文（角色、地图、系统、颜色、平台、RPC、惩罚等）。
	// 找不到对应翻译时回退为原始英文，保证不会出现空字符串。
	internal static class Lang
	{
		private static readonly Dictionary<string, string> Map = new Dictionary<string, string>()
		{
			// RoleTypes
			{ "Crewmate", "船员" },
			{ "Scientist", "科学家" },
			{ "Engineer", "工程师" },
			{ "Noisemaker", "噪音制造者" },
			{ "Tracker", "追踪者" },
			{ "Detective", "侦探" },
			{ "Judge", "法官" },
			{ "Impostor", "内鬼" },
			{ "Shapeshifter", "变形者" },
			{ "Phantom", "幻影" },
			{ "Viper", "毒蛇" },
			{ "CrewmateGhost", "船员幽灵" },
			{ "GuardianAngel", "守护天使" },
			{ "SpiritGuide", "灵魂向导" },
			{ "ImpostorGhost", "内鬼幽灵" },

			// MapNames
			{ "Skeld", "太空飞船" },
			{ "Dleks", "迪莱克斯" },
			{ "MiraHQ", "米拉总部" },
			{ "Polus", "波拉斯" },
			{ "Airship", "飞艇" },
			{ "Fungle", "真菌丛林" },

			// SystemTypes
			{ "Reactor", "反应堆" },
			{ "Electrical", "电力" },
			{ "LifeSupp", "氧气" },
			{ "Comms", "通讯" },
			{ "Doors", "门" },
			{ "Sabotage", "破坏" },
			{ "MedBay", "医疗室" },
			{ "Security", "安保室" },
			{ "UpperEngine", "上引擎室" },
			{ "LowerEngine", "下引擎室" },
			{ "Cafeteria", "自助餐厅" },
			{ "Storage", "储藏室" },
			{ "Weapons", "武器库" },
			{ "Office", "办公室" },
			{ "Decontamination", "净化室" },
			{ "Laboratory", "实验室" },
			{ "HeliSabotage", "直升机" },
			{ "MushroomMixupSabotage", "蘑菇混战" },
			{ "HudOverride", "通讯干扰" },
			{ "AutoDoors", "自动门" },
			{ "Ventilation", "通风系统" },
			{ "Medical", "医务室" },
			{ "Brig", "禁闭室" },
			{ "Records", "档案室" },
			{ "MainHall", "主厅" },
			{ "Kitchen", "厨房" },
			{ "Lounge", "休息室" },
			{ "Lookout", "瞭望台" },
			{ "MiningPit", "矿坑" },

			// Player colors
			{ "Red", "红色" },
			{ "Blue", "蓝色" },
			{ "Green", "绿色" },
			{ "Pink", "粉色" },
			{ "Orange", "橙色" },
			{ "Yellow", "黄色" },
			{ "Black", "黑色" },
			{ "White", "白色" },
			{ "Purple", "紫色" },
			{ "Brown", "棕色" },
			{ "Cyan", "青色" },
			{ "Lime", "青柠色" },
			{ "Maroon", "栗色" },
			{ "Rose", "玫瑰色" },
			{ "Banana", "香蕉色" },
			{ "Gray", "灰色" },
			{ "Tan", "茶色" },
			{ "Coral", "珊瑚色" },
			{ "Fortegreen", "深绿色" },

			// Styles.UIColors
			{ "Azure", "天蓝色" },
			{ "Carbon", "碳黑色" },
			{ "Cardinal", "绯红色" },
			{ "Pesto", "青酱绿" },
			{ "Pumpkin", "南瓜橙" },
			{ "Violet", "紫罗兰" },

			// Anticheat.Punishments
			{ "None", "无" },
			{ "Kick", "踢出" },
			{ "ErrorKick", "错误踢出" },
			{ "Ban", "封禁" },

			// RpcCalls
			{ "PlayAnimation", "播放动画" },
			{ "CompleteTask", "完成任务" },
			{ "Exiled", "放逐" },
			{ "CheckName", "检查名称" },
			{ "SetName", "设置名称" },
			{ "SetColor", "设置颜色" },
			{ "ReportDeadBody", "报告尸体" },
			{ "SetScanner", "设置扫描仪" },
			{ "SetStartCounter", "设置开始倒计时" },
			{ "EnterVent", "进入管道" },
			{ "ExitVent", "离开管道" },
			{ "SnapTo", "瞬移" },
			{ "AddVote", "添加投票" },
			{ "CloseDoorsOfType", "关闭某类门" },
			{ "ClimbLadder", "爬梯子" },
			{ "UsePlatform", "使用平台" },
			{ "UpdateSystem", "更新系统" },
			{ "SetLevel", "设置等级" },

			// Platforms
			{ "Unknown", "未知" },
			{ "StandaloneEpicPC", "Epic PC" },
			{ "StandaloneSteamPC", "Steam PC" },
			{ "StandaloneMac", "Mac" },
			{ "StandaloneWin10", "Microsoft Store" },
			{ "StandaloneItch", "Itch.io" },
			{ "Android", "安卓" },
			{ "IOS", "iOS" },
			{ "NintendoSwitch", "Nintendo Switch" },
			{ "Xbox", "Xbox" },
			{ "Playstation", "PlayStation" },

			// TaskTypes（任务动画）
			{ "ClearAsteroids", "击碎陨石" },
			{ "EmptyGarbage", "倒垃圾" },
			{ "PrimeShields", "激活护盾" },
			{ "SubmitScan", "医疗扫描" },
			{ "EmptyGarbage2", "倒垃圾" },
			{ "FuelEngines", "加燃料" },
			{ "AlignEngineOutput", "校准引擎" },
			{ "StartReactor", "启动反应堆" },
			{ "CalibrateDistributor", "校准配电" },
			{ "FixWiring", "修理线路" },
			{ "DivertPower", "转移电力" },
			{ "AcceptPower", "接收电力" },
			{ "UnlockManifolds", "解锁面板" },
			{ "ResetReactor", "重启反应堆" },
			{ "FixLights", "修灯" },
			{ "CleanO2Filter", "清理氧气过滤器" },
			{ "InspectSample", "检验样本" },
			{ "ReplaceWaterJug", "换水壶" },
			{ "RebootWifi", "重启 WiFi" },
			{ "OpenWaterways", "打开水道" },
			{ "ScanBoardingPass", "扫描登机牌" },
			{ "SwipeCard", "刷卡" },
			{ "InsertKeys", "插钥匙" },
			{ "ChartCourse", "规划航线" },
			{ "StabilizeSteering", "稳定转向" },
			{ "UnlockSafe", "打开保险箱" },
			{ "PutAwayPistols", "收起手枪" },
			{ "PutAwayRifles", "收起步枪" },
			{ "DownloadData", "下载数据" },
			{ "StoreArtifacts", "存放文物" },
			{ "MeasureWeather", "测量天气" },
			{ "EnterIdCode", "输入 ID 码" },
			{ "ProcessData", "处理数据" },
			{ "RunDiagnostics", "运行诊断" },
			{ "MonitorOxygen", "监控氧气" },
			{ "AssembleArtifact", "拼装文物" },
			{ "SortSamples", "整理样本" },
			{ "ScanBoardingPass2", "扫描登机牌" }
		};

		public static string Tr(object value)
		{
			if(value == null) return string.Empty;

			string key = value.ToString();
			return Map.TryGetValue(key, out string translated) ? translated : key;
		}
	}
}
