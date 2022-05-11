namespace UPTT.Tool.Generator
{
	public abstract class Erosion 
	{
		public enum ErosionMethod
		{
			Rainfall,
			Wind,
			Thermal
		}


		public interface IErosion
		{
			public void Erode(UPTerrain terrain);
			public float Strength { get; set; }
			public int BlurIterations { get; set; }
		}
		
	}
}