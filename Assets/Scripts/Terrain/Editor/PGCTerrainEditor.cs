using UnityEditor;
using UnityEngine;

namespace PGC_Terrain
{
    [CustomEditor(typeof(PGCTerrain))]
    [CanEditMultipleObjects]
    public class PGCTerrainEditor : Editor
    {
        private PGCTerrain m_target;
        private Terrain m_terrain;
        void OnEnable()
        {
            m_target = (PGCTerrain)target;
            m_terrain = m_target.GetComponent<Terrain>();
        }

        private bool showPerlin = true;
        private bool showMultiplePerlin = false;
        private bool showVoronoi = false;
        private bool showMPD = false;
        private bool showTextures = false;

        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("PGC Terrain Editor", EditorStyles.largeLabel);
            EditorGUILayout.Space();
            showPerlin = EditorGUILayout.Foldout(showPerlin, "Perlin");
            if (showPerlin)
            {
                DisplayPerlinGUIElements();
            }

            showMultiplePerlin = EditorGUILayout.Foldout(showMultiplePerlin, "Multiple Perlin");
            if (showMultiplePerlin)
            {
                DisplayMultiplePerlinGUIElements();
            }

            showVoronoi = EditorGUILayout.Foldout(showVoronoi, "Voronoi");
            if (showVoronoi)
            {
                DisplayVoronoiUIElements();
            }

            showMPD = EditorGUILayout.Foldout(showMPD, "Midpoint Displacement");
            if(showMPD)
            {
                DisplayMPDUIElements();
            }

            showTextures = EditorGUILayout.Foldout(showTextures, "Textures");
            if (showTextures)
            {
                DisplayTexturesElements();
            }

        }

        private void DisplayPerlinGUIElements()
        {
            EditorGUILayout.LabelField("Perlin Scale");
            m_target.perlinScale = EditorGUILayout.Slider(m_target.perlinScale, 0.0001f, 100f);
            EditorGUILayout.LabelField("Perlin Frequency");
            m_target.perlinFrequency = EditorGUILayout.Slider(m_target.perlinFrequency, 1f, 32f);
            EditorGUILayout.LabelField("Perlin Amplitude");
            m_target.perlinAmplitude = EditorGUILayout.Slider(m_target.perlinAmplitude, 0.0001f, 1f);
            if (GUILayout.Button("Apply Perlin"))
            {
                m_target.ApplyPerlinNoise(m_terrain, true);
            }
            if (GUILayout.Button("Reset to Perlin"))
            {
                m_target.ApplyPerlinNoise(m_terrain, false);
            }
        }

        private void DisplayMultiplePerlinGUIElements()
        {
            EditorGUILayout.LabelField("Perlin Scale");
            m_target.perlinScale = EditorGUILayout.Slider(m_target.perlinScale, 0.0001f, 100f);
            EditorGUILayout.LabelField("Perlin Number of Octaves");
            m_target.perlinNumberOfOctaves = EditorGUILayout.IntSlider(m_target.perlinNumberOfOctaves, 1, 8);
            EditorGUILayout.LabelField("Perlin Persistence");
            m_target.perlinPersistence = EditorGUILayout.Slider(m_target.perlinPersistence, 0.0001f, 1f);
            EditorGUILayout.LabelField("Perlin Lacunarity");
            m_target.perlinLacunarity = EditorGUILayout.Slider(m_target.perlinLacunarity, 1f, 10f);
            if (GUILayout.Button("Apply Multiple Perlin"))
            {
                m_target.ApplyMultiplePerlinNoise(m_terrain, true);
            }
            if (GUILayout.Button("Reset to Multiple Perlin"))
            {
                m_target.ApplyMultiplePerlinNoise(m_terrain, false);
            }
        }

        private void DisplayVoronoiUIElements()
        {
            EditorGUILayout.LabelField("Voronoi Radius");
            m_target.voronoiMountainRadius = EditorGUILayout.Slider(m_target.voronoiMountainRadius, 1f, 400f);
            EditorGUILayout.LabelField("Voronoi Height");
            m_target.voronoiMountainHeight = EditorGUILayout.Slider(m_target.voronoiMountainHeight, 0.0001f, 1f);
            EditorGUILayout.LabelField("Voronoi Curvature");
            m_target.voronoiMountainCurvature = EditorGUILayout.Slider(m_target.voronoiMountainCurvature, 1f, 20f);
            if (GUILayout.Button("Add Voronoi Mountain"))
            {
                m_target.ApplyVornoiMountain(m_terrain, true);
            }
            if (GUILayout.Button("Reset with Voronoi Mountain"))
            {
                m_target.ApplyVornoiMountain(m_terrain, false);
            }
        }

        private void DisplayMPDUIElements()
        {
            EditorGUILayout.LabelField("Starting Spread");
            m_target.mpdStartingSpread = EditorGUILayout.Slider(m_target.mpdStartingSpread, 0.0001f, 1f);
            EditorGUILayout.LabelField("Spread Reduction Constant");
            m_target.mpdSpreadReductionConstant = EditorGUILayout.Slider(m_target.mpdSpreadReductionConstant, 0.0001f, 1f);
            if (GUILayout.Button("Smooth with Midpoint Displacement"))
            {
                m_target.ApplyMPDNoise(m_terrain, true);
            }
            if (GUILayout.Button("Reset with Midpoint Displacement"))
            {
                m_target.ApplyMPDNoise(m_terrain, false);
            }
        }

        private void DisplayTexturesElements()
        {
            EditorGUILayout.LabelField("Dirt Height Range");
            EditorGUILayout.MinMaxSlider(ref m_target.minHeightDirt, ref m_target.maxHeightDirt, 0, 1);
            EditorGUILayout.LabelField("Dirt Slope Range");
            EditorGUILayout.MinMaxSlider(ref m_target.minSlopeDirt, ref m_target.maxSlopeDirt, 0, 1);

            EditorGUILayout.LabelField("Grass Height Range");
            EditorGUILayout.MinMaxSlider(ref m_target.minHeightGrass, ref m_target.maxHeightGrass, 0, 1);
            EditorGUILayout.LabelField("Grass Slope Range");
            EditorGUILayout.MinMaxSlider(ref m_target.minSlopeGrass, ref m_target.maxSlopeGrass, 0, 1);

            EditorGUILayout.LabelField("Rock Height Range");
            EditorGUILayout.MinMaxSlider(ref m_target.minHeightRock, ref m_target.maxHeightRock, 0, 1);
            EditorGUILayout.LabelField("Rock Slope Range");
            EditorGUILayout.MinMaxSlider(ref m_target.minSlopeRock, ref m_target.maxSlopeRock, 0, 1);

            EditorGUILayout.LabelField("Snow Height Range");
            EditorGUILayout.MinMaxSlider(ref m_target.minHeightSnow, ref m_target.maxHeightSnow, 0, 1);
            EditorGUILayout.LabelField("Snow Slope Range");
            EditorGUILayout.MinMaxSlider(ref m_target.minSlopeSnow, ref m_target.maxSlopeSnow, 0, 1);

            if (GUILayout.Button("Apply Textures"))
            {
                m_target.ApplyTerrainTextures(m_terrain);
            }
        }
    }
}
