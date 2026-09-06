using UnityEngine.AddressableAssets;

namespace HydraMenu.modules.host
{
	internal class FlipSkeld : Module
	{
		public FlipSkeld() : base("翻转Skeld") { }

		private void SwapMapAssets()
		{
			if(AmongUsClient.Instance == null)
			{
				_enabled = false;
				return;
			}

			// ShipPrefabs 是一个将地图 ID 对应到其地图的列表
			// ID 0 是 Skeld，1 是 Mira，2 是 Polus，3 是 Dleks
			// 如果我们想能在 Dleks 生成（因为这通常不可访问），我们可以交换这两个元素
			// 使 0 变为 Dleks 而 3 变为 Skeld，从而在 Dleks 生成而不是 Skeld
			AssetReference temp = AmongUsClient.Instance.ShipPrefabs[3];
			AmongUsClient.Instance.ShipPrefabs[3] = AmongUsClient.Instance.ShipPrefabs[0];
			AmongUsClient.Instance.ShipPrefabs[0] = temp;
		}

		protected override void OnEnable()
		{
			SwapMapAssets();
		}

		protected override void OnDisable()
		{
			SwapMapAssets();
		}
	}
}