using UnityEngine;
using UnityEditor;

public class TerrainSmoothnessEditor : EditorWindow
{
    private Terrain terrain;
    private float smoothness = 0.5f;
    private const int kernelSize = 3;

    [MenuItem("Tools/Terrain Smoothness Editor")]
    public static void ShowWindow()
    {
        GetWindow<TerrainSmoothnessEditor>("Terrain Smoothness");
    }

    private void OnGUI()
    {
        GUILayout.Label("Terrain Smoothness Tool", EditorStyles.boldLabel);
        
        terrain = (Terrain)EditorGUILayout.ObjectField("Terrain", terrain, typeof(Terrain), true);
        
        if (terrain == null)
        {
            EditorGUILayout.HelpBox("Please assign a terrain to modify.", MessageType.Info);
            return;
        }

        EditorGUILayout.Space();
        
        smoothness = EditorGUILayout.Slider("Smoothness", smoothness, 0f, 1f);
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Apply Smoothness", GUILayout.Height(30)))
        {
            ApplySmoothness();
        }
        
        EditorGUILayout.HelpBox(
            "Smoothness: 0 = No smoothing, 1 = Maximum smoothing\n" +
            "This will modify the terrain heightmap.", 
            MessageType.Info
        );
    }

    private void ApplySmoothness()
    {
        if (terrain == null || terrain.terrainData == null)
        {
            EditorUtility.DisplayDialog("Error", "No valid terrain selected.", "OK");
            return;
        }

        Undo.RegisterCompleteObjectUndo(terrain.terrainData, "Terrain Smoothness");

        TerrainData terrainData = terrain.terrainData;
        int width = terrainData.heightmapResolution;
        int height = terrainData.heightmapResolution;
        
        float[,] heights = terrainData.GetHeights(0, 0, width, height);
        float[,] smoothedHeights = new float[width, height];

        EditorUtility.DisplayProgressBar("Smoothing Terrain", "Processing heightmap...", 0f);

        // Apply smoothing based on the smoothness value
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (smoothness > 0f)
                {
                    smoothedHeights[x, y] = GetSmoothedHeight(heights, x, y, width, height);
                }
                else
                {
                    smoothedHeights[x, y] = heights[x, y];
                }
            }
            
            if (y % 10 == 0)
            {
                float progress = (float)y / height;
                EditorUtility.DisplayProgressBar("Smoothing Terrain", "Processing heightmap...", progress);
            }
        }

        // Blend between original and smoothed based on smoothness value
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                heights[x, y] = Mathf.Lerp(heights[x, y], smoothedHeights[x, y], smoothness);
            }
        }

        terrainData.SetHeights(0, 0, heights);
        
        EditorUtility.ClearProgressBar();
        EditorUtility.DisplayDialog("Success", "Terrain smoothness applied successfully!", "OK");
    }

    private float GetSmoothedHeight(float[,] heights, int x, int y, int width, int height)
    {
        float total = 0f;
        int count = 0;
        
        int halfKernel = kernelSize / 2;

        for (int ky = -halfKernel; ky <= halfKernel; ky++)
        {
            for (int kx = -halfKernel; kx <= halfKernel; kx++)
            {
                int sampleX = Mathf.Clamp(x + kx, 0, width - 1);
                int sampleY = Mathf.Clamp(y + ky, 0, height - 1);
                
                total += heights[sampleX, sampleY];
                count++;
            }
        }

        return total / count;
    }
}
