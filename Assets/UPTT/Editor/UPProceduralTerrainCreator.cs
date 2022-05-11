using System;
using System.IO;
using Codice.Client.BaseCommands;
using UnityEngine;
using UnityEditor;
using UPTT.Tool.Utils;

public class UPProceduralTerrainCreator : EditorWindow
{
	private static string dataPath;
	private static string fileExtension;
	
	private const int Padding = 100;

	private struct TextureSettings
	{
		public string name;
		
		public bool seamless;
		public bool alpha;
		public bool rescale;
		
		public Vector3 scale;
		public Vector2 offset;
		public Vector2 colourRange;
		
		public float persistence;
		public float lacuranity;
		public int octaves;
		public int size;
	}

	private Texture2D _proceduralTexture;
	
	private TextureSettings _settings = new TextureSettings()
	{
		name = "UPTT Texture",
		size = 512,
		scale = new Vector3(0.001f, 0.9f, 0.001f),
		offset = Vector2.zero,
		octaves = 5,
		persistence = 2,
		lacuranity = 2,

		seamless = false,
		alpha = false,
		rescale = false,
		
		colourRange = new Vector2(1, 0)
	};
	
	
	// Create a menu item to display the editor window
	[MenuItem("Window/UPTT/Procedural Texture Creator #p")]
	public static void EnableWindow()
	{
		EditorWindow.GetWindow(typeof(UPProceduralTerrainCreator));
	}


	private void OnGUI()
	{
		DisplayTextureSettings();
		DisplayTextureLabel();
	}


	
	// Create the UI elements for changing the TextureSettings
	private void DisplayTextureSettings()
	{
		GUILayout.Label("Procedural Texture", EditorStyles.boldLabel);
		
		_settings.name = EditorGUILayout.TextField("File Name", _settings.name);
		_settings.size = EditorGUILayout.IntField("Texture Size", _settings.size);
		
		_settings.scale = EditorGUILayout.Vector3Field("Perlin Scale Factor", _settings.scale);
		
		Maths.Clamp(ref _settings.scale.x, 0, 0.1f);
		Maths.Clamp(ref _settings.scale.y, 0, 1);
		Maths.Clamp(ref _settings.scale.z, 0, 0.1f);
		
		_settings.offset = EditorGUILayout.Vector2Field("Offset", _settings.offset);
		_settings.offset = Maths.ClampVector2(ref _settings.offset, 0, 5000);
			
		_settings.persistence = EditorGUILayout.Slider("Persistance", _settings.persistence, 1, 10);
		_settings.octaves = EditorGUILayout.IntSlider("Octaves", _settings.octaves, 1, 10);
		_settings.lacuranity = EditorGUILayout.Slider("Lacuranity", _settings.lacuranity, 0, 10);
		
		
		_settings.alpha = EditorGUILayout.Toggle("Enable Alpha", _settings.alpha);
		_settings.seamless = EditorGUILayout.Toggle("Enable Seamless", _settings.seamless);
		_settings.rescale = EditorGUILayout.Toggle("Enable Colour Rescaling", _settings.rescale);

	}


	// Create UI elements for displaying the 2DTexture
	private void DisplayTextureLabel()
	{
		var editorWindowWidth = EditorGUIUtility.currentViewWidth - Padding;

		
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		// Apply the texture to a label
		GUILayout.Label(_proceduralTexture, GUILayout.Width(editorWindowWidth), GUILayout.Height(editorWindowWidth));
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();

		
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();

		if (GUILayout.Button("Create Texture", GUILayout.Width(editorWindowWidth)) == true)
		{
			GenerateTexture();
		}
		
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		if (GUILayout.Button("Save to File",GUILayout.Width(editorWindowWidth)) == true)
		{
			SaveAssetToFile();
		}
		
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
	}

	private int GetTexSize()
	{
		return _settings.size + 1;
	}
	
	private void GenerateTexture()
	{
		// Iterate through all pixels in the texture
		for (var i = 0; i < GetTexSize(); i++)
		{
			for (var j = 0; j < GetTexSize(); j++)
			{
				float perlin;
				// If generating seamless run the seamless algo
				if (_settings.seamless == true)
				{
					perlin = GenerateSeamlessTexture(j, i);
				}
				else // Otherwise calculate using fBM with given settings
				{
					perlin = TerrainAlgorithms.FractalBrownianMotion(
						(j + _settings.offset.x) * _settings.scale.x,
						(i + _settings.offset.y) * _settings.scale.z,
						_settings.octaves,
						_settings.persistence,
						_settings.lacuranity) * _settings.scale.y;
				}
				
				// Change the alpha value if necessary 
				var alpha = 1f;
				if (_settings.alpha == true)
				{
					alpha = perlin;
				}
				
				if (_settings.colourRange.x > perlin)
				{
					_settings.colourRange.x = perlin;
				}
				if (_settings.colourRange.y < perlin)
				{
					_settings.colourRange.y = perlin;
				}
				
				// Set the pixel at j,i using our calculated perlin and alpha values
				_proceduralTexture.SetPixel(j, i, new Color(perlin, perlin, perlin, alpha));
			}
		}

		if (_settings.rescale == true)
		{
			// iterate through each pixel
			for (var i = 0; i < GetTexSize(); i++)
			{
				for (var j = 0; j < GetTexSize(); j++)
				{
					var colour = _proceduralTexture.GetPixel(j, i); // Get the current pixel colour
					var newValue = Maths.Map01(colour.b, _settings.colourRange.x, _settings.colourRange.y); // Map it from the colourRange to 0,1
					_proceduralTexture.SetPixel(j, i, new Color(newValue, newValue, newValue, colour.a)); // Set the current pixel colour to the new value
				}
			}
		}
		
		// Apply the changes made to the texture, if not done won't update in the editor
		_proceduralTexture.Apply(false, false);
	}
	
	private float GenerateSeamlessTexture(int j, int i)
	{
				var x = j / (float) GetTexSize();
				var y = i / (float) GetTexSize();

				var perlins = new float[4]
				{
					TerrainAlgorithms.FractalBrownianMotion(
						(j + _settings.offset.x) * _settings.scale.x,
						(i + _settings.offset.y) * _settings.scale.z,
						_settings.octaves,
						_settings.persistence,
						_settings.lacuranity) * _settings.scale.y,
					
					TerrainAlgorithms.FractalBrownianMotion(
						(j + _settings.offset.x) * _settings.scale.x,
						(i + _settings.offset.y + GetTexSize()) * _settings.scale.z,
						_settings.octaves,
						_settings.persistence,
						_settings.lacuranity) * _settings.scale.y,
					
					TerrainAlgorithms.FractalBrownianMotion(
						(j + _settings.offset.x + GetTexSize()) * _settings.scale.x,
						(i + _settings.offset.y) * _settings.scale.z,
						_settings.octaves,
						_settings.persistence,
						_settings.lacuranity)* _settings.scale.y,
					
						TerrainAlgorithms.FractalBrownianMotion(
							(j + _settings.offset.x + GetTexSize()) * _settings.scale.x,
							(i + _settings.offset.y + GetTexSize()) * _settings.scale.z,
							_settings.octaves,
							_settings.persistence,
							_settings.lacuranity)* _settings.scale.y
				};

				var total =
					x * y * perlins[0] +
					x * (1 - y) * perlins[1] +
					(1 - x) * y * perlins[2] +
					(1 - x) * (1 - y) * perlins[3];

				var value = (int) (256 * total) + 27;
				
				// Calculate the pixel colour adding a small offset
				var rgb = new Vector3(perlins[0], value, value + 63);
				Maths.ClampVector3(ref rgb, 0, 255);
				
				// Calculate the percent value, of the rgb values
				return (rgb.x + rgb.y + rgb.z) /  (765f);
	}

	private void SaveAssetToFile()
	{
		var fileName = _settings.name +"."+ fileExtension;
		
		var savePath = EditorUtility.SaveFilePanel("Save Asset",
			"",
			fileName,
			fileExtension);

		if (string.IsNullOrEmpty(savePath)) return;
		var data = _proceduralTexture.EncodeToPNG();

		if (data == null) return;
		System.IO.File.WriteAllBytes(savePath, data);
		Debug.Log($"Saved Asset at: {savePath}/{fileName}");
	}

	#region Unity

	private void OnEnable()
	{
	 dataPath = Application.dataPath + "/UPTT/GeneratedTextures/";
	 fileExtension = "png";
	}

	private void Reset()
	{
		// Generate New Texture
		_proceduralTexture = new Texture2D(GetTexSize(), GetTexSize(), TextureFormat.ARGB32, false);
	}

	#endregion
}
