using System;
using EditorGUITable;
using UnityEditor;
using UnityEditor.Experimental.TerrainAPI;
using UnityEngine;


[CustomEditor((typeof(UPTerrain)))]
[CanEditMultipleObjects]
public class UPTerrainEditor : Editor
{
    // UPTerrain properties
    private SerializedProperty _unityRandomRange;
    
    // Height Map settings
    private SerializedProperty _heightMapTexture;
    private SerializedProperty _heightMapScale;
    private SerializedProperty _replaceHeightMap;
    
    // Perlin/ fBM settings
    private SerializedProperty _seed;
    private SerializedProperty _perlinScaleFactor;
    private SerializedProperty _perlinHeightScale;
    private SerializedProperty _octaves;
    private SerializedProperty _persistence;

    private GUITableState _perlinSettingsTable;
    private SerializedProperty _perlinSettingsList;
    
    
    // Editor Foldouts
    private bool _showRandom, _showHeightMapProperties, _showPerlin, _showfBM, _showVoronoi = false;
    
    
    
    /**
     *  [SerializeField] private int seed;
     *  [SerializeField] private Vector2 perlinScaleFactor;
     *  [SerializeField] private bool replaceHeightmap = false;
     */
    
    private void OnEnable()
    {
        // Get our properties from the UPTerrain script
        _unityRandomRange = serializedObject.FindProperty("unityRandomRange");
        _heightMapTexture = serializedObject.FindProperty("heightMapTexture");
        _heightMapScale   = serializedObject.FindProperty("heightMapScale");
        _replaceHeightMap   = serializedObject.FindProperty("replaceHeightmap");
        
        _seed               = serializedObject.FindProperty("seed");
        _perlinScaleFactor  = serializedObject.FindProperty("perlinScaleFactor");
        _perlinHeightScale  = serializedObject.FindProperty("perlinHeightScale");
        _octaves            = serializedObject.FindProperty("octaves");
        _persistence        = serializedObject.FindProperty("persistence");

        _perlinSettingsTable = new GUITableState("perlinSettingsTable");
        _perlinSettingsList = serializedObject.FindProperty("perlinNoiseSettingsList");

    }

    public override void OnInspectorGUI()
    {
        // Update all of the serialised values between the editor and the terrain object
        serializedObject.Update();

        UPTerrain terrain = (UPTerrain) target;
        
        
        // Show ResetButton
        GUILayout.Label("Reset Terrain", EditorStyles.boldLabel);

        if (GUILayout.Button("Reset Terrain"))
        {
            terrain.ResetTerrainHeightMap();
        }

        _showHeightMapProperties = EditorGUILayout.Foldout(_showHeightMapProperties, "HeightMap Settings");
        if (_showHeightMapProperties)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("HeightMap Texture");
            EditorGUILayout.PropertyField(_heightMapTexture);
            EditorGUILayout.PropertyField(_heightMapScale);
            if (GUILayout.Button("Load HeightMap"))
            {
                terrain.LoadHeightMapFromTexture();
            }
        }
        
        // Random Height Foldout UI
        _showRandom = EditorGUILayout.Foldout(_showRandom, "Random HeightMap Generator");
        if (_showRandom)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            GUILayout.Label("Set heights between given range", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_unityRandomRange);
            EditorGUILayout.PropertyField(_replaceHeightMap);
            if(GUILayout.Button("Generate Random Heights"))
            {
                terrain.UnityRandomHeightMap();
            }
        }
        
        // Perlin Foldout UI
        _showPerlin = EditorGUILayout.Foldout(_showPerlin, "Perlin Noise HeightMap Generator");
        if (_showPerlin)
        {
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
            if(GUILayout.Button("Generate Perlin Heights"))
            {
                terrain.PerlinHeightmap();
            }
        }
        
        // fBM Foldout UI
        _showfBM = EditorGUILayout.Foldout(_showfBM, "fBM perlin noise");
        if (_showfBM)
        {
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

            GUILayout.Label("fBM Perlin Noise");
            _perlinSettingsTable = GUITableLayout.DrawTable(_perlinSettingsTable, serializedObject.FindProperty("perlinNoiseSettingsList"));
            
            GUILayout.Space(24);
            EditorGUILayout.BeginHorizontal();
                if(GUILayout.Button("ADD")) terrain.CreateNewSettings();
                if(GUILayout.Button("REMOVE")) terrain.DeleteSettings();
            EditorGUILayout.EndHorizontal();
            
            GUILayout.Label("Replace heightmap", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_replaceHeightMap);
            if(GUILayout.Button("Apply")) terrain.PerlinSuperposition();
        }


        _showVoronoi = EditorGUILayout.Foldout(_showVoronoi, "Voronoi Tessellation");
        if (_showVoronoi)
        {
            if(GUILayout.Button("Voronoi Tessellation")) terrain.VoronoiTessellation();
        }
        
        // Apply the changes made to the object
        serializedObject.ApplyModifiedProperties();
    }

    private void Start()
    {
        
    }

    private void Update()
    {
        
    }
}
