using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace UPTT.Tool.Generator
{
	[System.Serializable]
	public class RainfallErosion : ScriptableObject, Erosion.IErosion
	{
		[SerializeField]
		private int count;
		
		[SerializeField]
		private int offshoots = 3;

		[SerializeField]
		private float solubility = 0.025f;

		[SerializeField] private float strength;
		[SerializeField] private int blurIterations;

		public float Solubility
		{
			get => solubility;
			set => solubility = value;
		}

		public int Offshoots
		{
			get => offshoots;
			set => offshoots = value;
		}

		public int Count
		{
			get => count;
			set => count = value;
		}

		public void Erode(UPTerrain terrain)
		{
			Debug.Log("Rainfall Eroding terrain");
		}

		public float Strength { get =>strength; set => strength = value; }
		public int BlurIterations { get=>blurIterations; set => blurIterations = value; }
	}
}