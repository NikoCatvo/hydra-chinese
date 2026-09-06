using System.Collections.Generic;

namespace HydraMenu.assets
{
	internal class MapAssets
	{
		public static readonly Dictionary<string, TaskTypes> skeldAnimations = new Dictionary<string, TaskTypes>()
		{
			{ "清理小行星", TaskTypes.ClearAsteroids },
			{ "清空垃圾", TaskTypes.EmptyGarbage },
			{ "启动护盾", TaskTypes.PrimeShields }
		};

		public static readonly Dictionary<string, TaskTypes> polusAnimations = new Dictionary<string, TaskTypes>()
		{
			{ "清理小行星", TaskTypes.ClearAsteroids }
		};

		public static readonly SortedDictionary<int, string> skeldVents = new SortedDictionary<int, string>()
		{
			{ 0, "管理室" },
			{ 1, "走廊" },
			{ 2, "餐厅" },
			{ 3, "电力室" },
			{ 4, "上层引擎" },
			{ 5, "安保室" },
			{ 6, "医务室" },
			{ 7, "武器室" },
			{ 8, "下层反应堆" },
			{ 9, "下层引擎" },
			{ 10, "护盾室" },
			{ 11, "上层反应堆" },
			{ 12, "上层导航"},
			{ 13, "下层导航" }
		};

		public static readonly SortedDictionary<int, string> miraVents = new SortedDictionary<int, string>()
		{
			// Mira HQ 没有 ID 为 0 的通风管道
			// Fortebass 关于原因的解释见 https://github.com/roobscoob/among-us-protocol/blob/master/images/mirahq_vents_quote.png
			// 如你所见很有信息量... 至少我们知道为什么游戏代码中频繁使用 255 作为默认值
			{ 1, "阳台" },
			{ 2, "餐厅上方" },
			{ 3, "反应堆" },
			{ 4, "实验室" },
			{ 5, "办公室" },
			{ 6, "管理室" },
			{ 7, "温室" },
			{ 8, "医务室" },
			{ 9, "消毒室" },
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
			{ 8, "储物室" },
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
			{ 5, "卧室1" },
			{ 6, "保洁室" },
			{ 7, "缺口房右" },
			{ 8, "缺口房左" },
			{ 9, "淋浴间" },
			{ 10, "档案室" },
			{ 11, "货舱" }
		};

		public static readonly SortedDictionary<int, string> fungleVents = new SortedDictionary<int, string>()
		{
			{ 0, "通讯室" },
			{ 1, "厨房" },
			{ 2, "瞭望台" },
			{ 3, "会议厅上方" },
			{ 4, "实验室" },
			{ 5, "反应堆" },
			{ 6, "丛林" },
			{ 7, "丛林底部" },
			{ 8, "戏水区" },
			{ 9, "空降艇外" }
		};

		public static Dictionary<string, TaskTypes> GetAnimations()
		{
			MapNames currentMap = Utilities.GetCurrentMap();

			return currentMap switch
			{
				MapNames.Skeld or MapNames.Dleks => skeldAnimations,
				MapNames.Polus => polusAnimations,
				// 这些地图除了医疗扫描外没有任务动画
				MapNames.MiraHQ or MapNames.Airship or MapNames.Fungle => [],
				// 如果当前地图没有已知动画，则默认使用 Skeld 的
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
				// 如果当前地图没有已知通风管道，则默认使用 Skeld 的
				_ => skeldVents
			};
		}
	}
}