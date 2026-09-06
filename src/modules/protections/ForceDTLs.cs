using HarmonyLib;
using InnerNet;

namespace HydraMenu.modules.protections
{
	internal class ForceDTLs : Module
	{
		public ForceDTLs() : base("强制DTLS") { }

		private static ForceDTLs Instance
		{
			get { return ModuleManager.forceDtls; }
		}

		[HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.SetEndpoint))]
		class EnableDTLS
		{
			static void Prefix(ref bool dtls)
			{
				if(Instance.Enabled) dtls = true;
			}
		}
	}
}