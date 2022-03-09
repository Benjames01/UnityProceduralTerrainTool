

namespace UPTT.Tool
{
	[System.Serializable]
	public class PerlinNoiseSettings
	{
		public float scaleX = 0.01f, scaleY = 0.01f, scaleHeight = 0.07f;
		public int fBMOctaves = 4;
		public float fBMPersistence = 5;
		public int seed;
		public bool canDelete = false;
	}
}
