using System;

namespace UPTT.Tool
{
	[System.Serializable]
	public class VoronoiSettings
	{
		public uint numMountains;
		public float falloffAmount;
		public float dropoffAmount;
		public float lowHeight;
		public float highHeight;
		public FalloffType falloffType = FalloffType.Linear;
		
		public enum FalloffType
		{
			Pow,
			Combine,
			Linear,
			PowSin,
			Plateau,
		}
	}
	

}