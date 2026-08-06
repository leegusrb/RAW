using System;

namespace RAW.Network
{
	[Serializable]
	public sealed class NetworkConnectionPayload
	{
		public int protocolVersion;
		public string userId;
	}
}