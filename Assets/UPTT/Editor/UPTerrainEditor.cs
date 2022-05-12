using System;
using EditorGUITable;
using UnityEditor;
using UnityEngine;
using UPTT.Tool.Generator;


[CustomEditor((typeof(UPTerrain)))]
[CanEditMultipleObjects]
public class UPTerrainEditor : Editor
{
    // UPTerrain properties
    private SerializedProperty _unityRandomRange;
    
    // Height Map settings
    private SerializedProperty _heightMapTexture, _heightMapScale, _replaceHeightMap;
    
    // Perlin/ fBM settings
    private SerializedProperty _seed, _perlinScaleFactor, _perlinHeightScale, _octaves, _persistence;

    // Blurring
    private SerializedProperty _blurAmount;
    
    // Lists
    private GUITableState _perlinSettingsTable, _weightMapSettingsTable;

    // Editor Foldouts
    private bool _displayBlur, _displayRandom, _displayHeightMapProperties,
        _displayPerlin, _displayfBM, _displayVoronoi, _displayDSA, _displayErode,
        _displayWeightMap, _displaySave;

    private SerializedProperty _erosionMethod;
    private Erosion.IErosion _erosion;


    private RainfallErosion _rainfallErosion;
    private SerializedObject _serialisedRainfall;
    
    private ThermalErosion _thermalErosion;
    private SerializedObject _serialisedThermal;
    
    
    private Texture2D _texture;
    
    private SerializedProperty GetProperty(string propName)
    {
        return serializedObject.FindProperty(propName);
    }

    private void OnEnable()
    {
        // Get our properties from the UPTerrain script
        _unityRandomRange = GetProperty("unityRandomRange");
        _heightMapTexture = GetProperty("heightMapTexture");
        _heightMapScale   = GetProperty("heightMapScale");
        _replaceHeightMap   = GetProperty("replaceHeightmap");
        
        // Perlin fBM
        _seed               = GetProperty("seed");
        _perlinScaleFactor  = GetProperty("perlinScaleFactor");
        _perlinHeightScale  = GetProperty("perlinHeightScale");
        _octaves            = GetProperty("octaves");
        _persistence        = GetProperty("persistence");

        _perlinSettingsTable = new GUITableState("perlinSettingsTable");
        _weightMapSettingsTable = new GUITableState("weightMapSettingsTable");

        _blurAmount = GetProperty("blurIterations");

        _erosionMethod = GetProperty("erosionMethod");

        _rainfallErosion = ScriptableObject.CreateInstance<RainfallErosion>();
        _serialisedRainfall = new SerializedObject(_rainfallErosion);
        
        _thermalErosion = ScriptableObject.CreateInstance<ThermalErosion>();
        _serialisedThermal = new SerializedObject(_thermalErosion);
    }
    
    public override void OnInspectorGUI()
    {
        // Update all of the serialised values between the editor and the terrain object
        serializedObject.Update();

        var terrain = (UPTerrain) target;

        DisplayResetButton(terrain);
        
        EditorGUILayout.LabelField("Load", GUI.skin.horizontalSlider);
        DisplayHeightMapFoldout(terrain);
        
        EditorGUILayout.LabelField("Noise Algorithms", GUI.skin.horizontalSlider);
        DisplayRandomNoiseFoldout(terrain);
        DisplayPerlinFoldout(terrain);
        DisplayfBMFoldout(terrain);

        EditorGUILayout.LabelField("Other Algorithms", GUI.skin.horizontalSlider);
        DisplayVoronoiFoldout(terrain);
        DisplayDSAFoldout(terrain);

        EditorGUILayout.LabelField("Erosion", GUI.skin.horizontalSlider);
        DisplayErodeFoldout(terrain);
        
        EditorGUILayout.LabelField("Misc", GUI.skin.horizontalSlider);
        DisplayBlurFoldout(terrain);
        DisplayWeightMapFoldout(terrain);

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        DisplaySaveFoldout(terrain);
        
        // Apply the changes made to the object
        serializedObject.ApplyModifiedProperties();
    }
    
    private bool isTextureLoaded = false;
    private void GenerateTextureFromTerrain(UPTerrain terrain)
    {
        // Change button text if a heightmap texture has been loaded
        var text = isTextureLoaded ? "Refresh HeightMap" : "Get HeightMap";
        
        if (GUILayout.Button(text) != true) return;
        var size = terrain.GetTerrainData().heightmapResolution;
        
        var hMap = terrain.GetHeightMap();
        // Iterate through each point in the heightmap
        for (var i = 0; i < size; i++)
        {
            for (var j = 0; j < size; j++)
            {
                // Set the pixel in the texture to it's new colour
                var value = hMap[j, i];
                _texture.SetPixel(j,i,new Color(value, value, value, 1));
            }
        }
        // Apply changes to the texture
        _texture.Apply();
        
        // Texture has now been loaded
        isTextureLoaded = true;
    }
    
    // Sets up UI for Saving Texture to file
    private void DisplaySaveFoldout(UPTerrain terrain)
    {
        _displaySave = EditorGUILayout.Foldout(_displaySave, "Save Data");
        if (_displaySave != true) return;
        
        const int Padding = 100;
        var editorWindowWidth = EditorGUIUtility.currentViewWidth - Padding;

        // Generate new texture if one doesn't already exist
        if (_texture == null)
            _texture = new Texture2D(terrain.GetTerrainAlphaMapSize(),
                terrain.GetTerrainAlphaMapSize(),
                TextureFormat.ARGB32, false);
        
        
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();  
            GenerateTextureFromTerrain(terrain);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
            // Apply the texture to a label
            GUILayout.Label(_texture, GUILayout.Width(editorWindowWidth), GUILayout.Height(editorWindowWidth));
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();


        // Don't show save button if a texture's not been loaded
        if (!isTextureLoaded) return;
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        
        // Setup for saving texture to file using Editor Utility
        if (GUILayout.Button("Save To File") == true)
        {
            var savePath = EditorUtility.SaveFilePanel("Save Asset",
                "",
                "UPTT Heightmap.png",
                "png");

            if (!string.IsNullOrEmpty(savePath)) 
            {
                var data = _texture.EncodeToPNG();

                if (data != null)
                {
                    System.IO.File.WriteAllBytes(savePath, data);
                }
            }
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }
    
    // Sets up the reset button UI
    private static void DisplayResetButton(UPTerrain terrain)
    {
        // Show ResetButton
        //GUILayout.Label("Reset Terrain", EditorStyles.boldLabel);

        if (GUILayout.Button("Reset Terrain") == true)
        {
            terrain.ResetTerrainHeightMap();
        }
    }

    // Sets up UI for Terrain Blurring
    private void DisplayBlurFoldout(UPTerrain terrain)
    {
        _displayBlur = EditorGUILayout.Foldout(_displayBlur, "Terrain Smoothing");
        if (_displayBlur != true) return;
        EditorGUILayout.IntSlider(_blurAmount, 1, 20, new GUIContent("Iterations"));
        if (GUILayout.Button("Blur Terrain") == true)
        {
            terrain.BlurTerrain();
        }
    }

    // Sets up UI for HeightMap
    private void DisplayHeightMapFoldout(UPTerrain terrain)
    {
        _displayHeightMapProperties = EditorGUILayout.Foldout(_displayHeightMapProperties, "HeightMap Settings");
        if (_displayHeightMapProperties != true) return;
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("HeightMap Texture");
        EditorGUILayout.PropertyField(_heightMapTexture);
        EditorGUILayout.PropertyField(_heightMapScale);
        if (GUILayout.Button("Load HeightMap") == true)
        {
            terrain.LoadHeightMapFromTexture();
        }
    }

    // Sets up UI for random noise algorithm
    private void DisplayRandomNoiseFoldout(UPTerrain terrain)
    {
        // Random Height Foldout UI
        _displayRandom = EditorGUILayout.Foldout(_displayRandom, "Random HeightMap Generator");
        if (_displayRandom != true) return;
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("Set heights between given range", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_unityRandomRange);
        EditorGUILayout.PropertyField(_replaceHeightMap);
        if (GUILayout.Button("Generate Random Heights") == true)
        {
            terrain.UnityRandomHeightMap();
        }
    }

    // Sets up UI for perlin noise algorithm
    private void DisplayPerlinFoldout(UPTerrain terrain)
    {
        // Perlin Foldout UI
        _displayPerlin = EditorGUILayout.Foldout(_displayPerlin, "Perlin Noise HeightMap Generator");
        if (_displayPerlin != true) return;
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        GUILayout.Label("Set seed", EditorStyles.boldLabel);
        EditorGUILayout.IntSlider(_seed, 0, 64000, new GUIContent("Seed"));

        GUILayout.Label("Set Scale (Recommended value between 0.001 & 1)", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_perlinScaleFactor);
        EditorGUILayout.Slider(_perlinHeightScale, 0.01f, 10, new GUIContent("Height Scale"));

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("fBM Settings", EditorStyles.boldLabel);
        EditorGUILayout.IntSlider(_octaves, 1, 16, new GUIContent("Octaves"));
        EditorGUILayout.Slider(_persistence, 0.01f, 5, new GUIContent("Persistence"));
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        GUILayout.Label("Replace heightmap", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_replaceHeightMap);
        if (GUILayout.Button("Generate Perlin Heights") == true)
        {
            terrain.PerlinHeightmap();
        }
    }

    // Sets up UI for fBM
    private void DisplayfBMFoldout(UPTerrain terrain)
    {
        // fBM Foldout UI
        _displayfBM = EditorGUILayout.Foldout(_displayfBM, "fBM perlin noise");
        if (_displayfBM != true) return;
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        GUILayout.Label("fBM Perlin Noise");
        _perlinSettingsTable = GUITableLayout.DrawTable(_perlinSettingsTable,
            GetProperty("perlinNoiseSettingsList"));

        GUILayout.Space(24);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("ADD") == true) terrain.CreateNewSettings();
        if (GUILayout.Button("REMOVE") == true) terrain.DeleteSettings();
        EditorGUILayout.EndHorizontal();

        GUILayout.Label("Replace heightmap", EditorStyles.boldLabel);
        EditorGUILayout.PropertyField(_replaceHeightMap);
        if (GUILayout.Button("Apply") == true) terrain.PerlinSuperposition();
    }

    // Sets up UI for Voronoi Tessellation
    private void DisplayVoronoiFoldout(UPTerrain terrain)
    {
        // Voronoi settings foldout UI
        _displayVoronoi = EditorGUILayout.Foldout(_displayVoronoi, "Voronoi Tessellation");
        if (_displayVoronoi != true) return;
        var numMountains = serializedObject.FindProperty("voronoiSettings.numMountains");
        var falloffAmount = serializedObject.FindProperty("voronoiSettings.falloffAmount");
        var dropoffAmount = serializedObject.FindProperty("voronoiSettings.dropoffAmount");
        var falloffType = serializedObject.FindProperty("voronoiSettings.falloffType");
        var lowHeight = serializedObject.FindProperty("voronoiSettings.lowHeight");
        var highHeight = serializedObject.FindProperty("voronoiSettings.highHeight");

        EditorGUILayout.IntSlider(numMountains, 1, 15, new GUIContent("Mountain Peaks."));
        EditorGUILayout.Slider(lowHeight, 0, 1, new GUIContent("Lowest Peak Height"));
        EditorGUILayout.Slider(highHeight, 0, 1, new GUIContent("Highest Peak Height"));

        EditorGUILayout.Slider(dropoffAmount, 0, 15, new GUIContent("Drop off"));
        EditorGUILayout.Slider(falloffAmount, 0, 15, new GUIContent("Fall off"));
        EditorGUILayout.PropertyField(falloffType);

        EditorGUILayout.PropertyField(_replaceHeightMap);
        if (GUILayout.Button("Voronoi Tessellation") == true) terrain.VoronoiTessellation();
    }

    // Sets up UI for Diamond Square Algorithm
    private void DisplayDSAFoldout(UPTerrain terrain)
    {
        // Diamond-Square Algorithm foldout UI
        _displayDSA = EditorGUILayout.Foldout(_displayDSA, "Diamond-Square Algorithm");
        if (_displayDSA != true) return;
        var minHeight = serializedObject.FindProperty("diamondSquareSettings.MinHeight");
        var maxHeight = serializedObject.FindProperty("diamondSquareSettings.MaxHeight");

        var dampenerAmount = serializedObject.FindProperty("diamondSquareSettings.DampenerAmount");
        var roughnessExponent = serializedObject.FindProperty("diamondSquareSettings.RoughnessExponent");

        EditorGUILayout.Slider(minHeight, -10, 0, new GUIContent("Min Height"));
        EditorGUILayout.Slider(maxHeight, 0, 10, new GUIContent("Max Height"));
        EditorGUILayout.Slider(dampenerAmount, 0, 10, new GUIContent("Dampener Amount"));
        EditorGUILayout.Slider(roughnessExponent, 0, 5, new GUIContent("Roughness Exponent"));

        if (GUILayout.Button("Diamond Square") == true) terrain.DiamondSquareAlgorithm();
    }

    // Sets up UI for WeightMap texturing
    private void DisplayWeightMapFoldout(UPTerrain terrain)
    {
        // WeightMap Foldout UI
        _displayWeightMap = EditorGUILayout.Foldout(_displayWeightMap, "Texturing - WeightMaps");
        if (_displayWeightMap != true) return;
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        GUILayout.Label("WeightMaps");
        _weightMapSettingsTable = GUITableLayout.DrawTable(_weightMapSettingsTable,
            GetProperty("weightMaps"));

        GUILayout.Space(24);
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("ADD") == true) terrain.CreateNewWeightMap();
        if (GUILayout.Button("REMOVE") == true) terrain.DeleteWeightMap();
        EditorGUILayout.EndHorizontal();

        if (GUILayout.Button("Apply") == true) terrain.ApplyWeightMaps();
    }
    
    
    private Erosion.IErosion lastSelectedErosion;
    private void DisplayErodeFoldout(UPTerrain terrain)
    {
        _displayErode = EditorGUILayout.Foldout(_displayErode, "Apply Erosion");
        if (_displayErode != true) return;

        EditorGUILayout.PropertyField(_erosionMethod);


        var buttonText = "Erode";
        
        var method = _erosionMethod.enumValueIndex;
        switch (method)
        {
            case (int) Erosion.ErosionMethod.Rainfall:
                _serialisedRainfall.Update();
                buttonText = "Rainfall Erosion";
                EditorGUILayout.IntSlider(_serialisedRainfall.FindProperty("blurIterations"), 0, 10, new GUIContent("Blur Iterations"));
                EditorGUILayout.Slider(_serialisedRainfall.FindProperty("strength"), 0, 10, new GUIContent("Strength"));
                EditorGUILayout.IntSlider(_serialisedRainfall.FindProperty("count"), 0, 10, new GUIContent("Count"));
                EditorGUILayout.IntSlider(_serialisedRainfall.FindProperty("offshoots"), 0, 10, new GUIContent("Offshoots"));
                EditorGUILayout.Slider(_serialisedRainfall.FindProperty("solubility"), 0, 10, new GUIContent("Solubility"));
                _serialisedRainfall.Update();
                lastSelectedErosion = _rainfallErosion;
                break;
            case (int) Erosion.ErosionMethod.Thermal:
                _serialisedThermal.Update();
                buttonText = "Thermal Erosion";
                EditorGUILayout.IntSlider(_serialisedThermal.FindProperty("blurIterations"), 0, 10, new GUIContent("Blur Iterations"));
                EditorGUILayout.Slider(_serialisedThermal.FindProperty("strength"), 0, 10, new GUIContent("Strength"));
                
                _serialisedThermal.Update();
                lastSelectedErosion = _thermalErosion;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (GUILayout.Button(buttonText) == true)
            lastSelectedErosion.Erode((UPTerrain) target);
    }
}
