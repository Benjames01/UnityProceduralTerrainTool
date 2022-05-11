using UnityEngine;

namespace UPTT.Tool.Generator
{
	[System.Serializable]
	public class ThermalErosion : ScriptableObject, Erosion.IErosion
	{
		[SerializeField]
		private int count;
		
		[SerializeField]
		private int offshoots = 3;

		[SerializeField]
		private float solubility = 0.025f;

		[SerializeField] private float strength;
		[SerializeField] private int blurIterations;
		
		public void Erode(UPTerrain terrain)
		{
			Debug.Log("Thermal Eroding terrain");
		}

		public float Strength { get =>strength; set => strength = value; }
		public int BlurIterations { get=>blurIterations; set => blurIterations = value; }
	}
}