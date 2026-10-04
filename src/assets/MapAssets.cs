using System.Collections.Generic;

namespace HydraMenu.assets
{
	internal class MapAssets
	{
		public static readonly Dictionary<string, TaskTypes> skeldAnimations = new Dictionary<string, TaskTypes>()
		{
			{ "击碎陨石", TaskTypes.ClearAsteroids },
			{ "倒垃圾", TaskTypes.EmptyGarbage },
			{ "激活护盾", TaskTypes.PrimeShields }
		};

		public static readonly Dictionary<string, TaskTypes> polusAnimations = new Dictionary<string, TaskTypes>()
		{
			{ "击碎陨石", TaskTypes.ClearAsteroids }
		};

		public static readonly SortedDictionary<int, string> skeldVents = new SortedDictionary<int, string>()
		{
			{ 0, "管理室" },
			{ 1, "走廊" },
			{ 2, "自助餐厅" },
			{ 3, "电力室" },
			{ 4, "上引擎室" },
			{ 5, "安保室" },
			{ 6, "医疗室" },
			{ 7, "武器库" },
			{ 8, "下反应堆" },
			{ 9, "下引擎室" },
			{ 10, "护盾室" },
			{ 11, "上反应堆" },
			{ 12, "上导航室"},
			{ 13, "下导航室" }
		};

		public static readonly SortedDictionary<int, string> miraVents = new SortedDictionary<int, string>()
		{
			// Mira HQ has no vent with the ID of 0
			// Fortebass' explanation on why this is the case can be found at https://github.com/roobscoob/among-us-protocol/blob/master/images/mirahq_vents_quote.png
			// Very informative as you can see... At least we know why the game uses 255 as a default value frequently in the code
			{ 1, "阳台" },
			{ 2, "自助餐厅上方" },
			{ 3, "反应堆" },
			{ 4, "实验室" },
			{ 5, "办公室" },
			{ 6, "管理室" },
			{ 7, "温室" },
			{ 8, "医疗室" },
			{ 9, "净化室" },
			{ 10, "更衣室" },
			{ 11, "发射台" }
		};

		public static readonly SortedDictionary<int, string> polusVents = new SortedDictionary<int, string>()
		{
			{ 0, "电力室" },
			{ 1, "电力室外" },
			{ 2, "氧气室" },
			{ 3, "通讯室外" },
			{ 4, "办公室" },
			{ 5, "通讯室" },
			{ 6, "实验室" },
			{ 7, "实验室外" },
			{ 8, "储藏室" },
			{ 9, "火箭外" },
			{ 10, "电力室上方" },
			{ 11, "办公室外" }
		};

		public static readonly SortedDictionary<int, string> airshipVents = new SortedDictionary<int, string>()
		{
			{ 0, "金库" },
			{ 1, "驾驶舱" },
			{ 2, "观景台" },
			{ 3, "引擎室" },
			{ 4, "厨房" },
			{ 5, "卧室 1" },
			{ 6, "清洁工房间" },
			{ 7, "右侧空隙房" },
			{ 8, "左侧空隙房" },
			{ 9, "淋浴室" },
			{ 10, "档案室" },
			{ 11, "货舱" }
		};

		public static readonly SortedDictionary<int, string> fungleVents = new SortedDictionary<int, string>()
		{
			{ 0, "通讯室" },
			{ 1, "厨房" },
			{ 2, "瞭望台" },
			{ 3, "会议室上方" },
			{ 4, "实验室" },
			{ 5, "反应堆" },
			{ 6, "丛林" },
			{ 7, "丛林底部" },
			{ 8, "水花区" },
			{ 9, "运输飞船外" }
		};

		public static Dictionary<string, TaskTypes> GetAnimations()
		{
			MapNames currentMap = Utilities.GetCurrentMap();

			return currentMap switch
			{
				MapNames.Skeld or MapNames.Dleks => skeldAnimations,
				MapNames.Polus => polusAnimations,
				// These maps do not have any task animations, other than medbay scan
				MapNames.MiraHQ or MapNames.Airship or MapNames.Fungle => [],
				// If we do not any known animations for the current map then just default to the Skeld ones
				_ => skeldAnimations
			};
		}

		public static SortedDictionary<int, string> GetVents()
		{
			MapNames currentMap = Utilities.GetCurrentMap();

			return currentMap switch
			{
				MapNames.Skeld or MapNames.Dleks => skeldVents,
				MapNames.MiraHQ => miraVents,
				MapNames.Polus => polusVents,
				MapNames.Airship => airshipVents,
				MapNames.Fungle => fungleVents,
				// If we do not any known vents for the current map then just default to the Skeld ones
				_ => skeldVents
			};
		}
	}
}