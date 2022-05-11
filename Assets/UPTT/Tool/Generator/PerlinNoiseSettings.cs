using UnityEngine;

namespace UPTT.Tool.Generator
{
	[System.Serializable]
	public class PerlinNoiseSettings : IDeletable
	{
		[SerializeField] private float scaleX = 0.01f, scaleY = 0.01f, scaleHeight = 0.07f;
		[SerializeField] private int fBMOctaves = 4;
		[SerializeField] private float fBMPersistence = 5;
		[SerializeField] private int seed;
		[SerializeField] private bool canDelete = false;
		public bool ToRemove
		{
			get => canDelete;
			set => canDelete = value;
		}

		public float ScaleX
		{
			get => scaleX;
			set => scaleX = value;
		}

		public float ScaleY
		{
			get => scaleY;
			set => scaleY = value;
		}

		public int Seed
		{
			get => seed;
			set => seed = value;
		}

		public int FBmOctaves
		{
			get => fBMOctaves;
			set => fBMOctaves = value;
		}

		public float FBmPersistence
		{
			get => fBMPersistence;
			set => fBMPersistence = value;
		}

		public float ScaleHeight
		{
			get => scaleHeight;
			set => scaleHeight = value;
		}
	}

}
