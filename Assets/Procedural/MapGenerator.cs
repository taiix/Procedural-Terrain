using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    #region Variables
    [SerializeField]
    private TerrainResolution resolution;

    private int width;
    private int height;

    [SerializeField] private float scale;

    #region Perlin Noise Variables
    [Header("Perlin Noise")]
    [SerializeField] private int octaves;
    [SerializeField] private float persistence;
    [SerializeField] private float lacunarity;
    [SerializeField] private float seed;
    [SerializeField] private float terrainHighMultiplier;

    [SerializeField] private Vector2 offset;

    private float[,] noiseHeights;
    #endregion

    #region Terrain Variables
    [Space]
    [Header("Terrain")]

    private Terrain terrain;
    private TerrainData terrainData;

    [SerializeField] private float terrainHeight;
    [SerializeField] private AnimationCurve terrainCurve;

    [Space]
    [Header("Terrain Texture")]
    [SerializeField] private List<SplatHeights> splatHeights = new();


    #endregion

    #region DELETE BEFORE PRODUCTION, ONLY FOR TESTING
    public TerrainRegion[] regions;
    [SerializeField] private Renderer rend;
    #endregion

    #endregion

    TerrainShapeMap shapeMap;
    FalloffTest falloffMap;

    private void Start()
    {
        terrain = GetComponent<Terrain>();
        shapeMap = GetComponent<TerrainShapeMap>();
        falloffMap = GetComponent<FalloffTest>();
        terrainData = terrain.terrainData;

        CalculatePerlin();
    }

    private void OnValidate()
    {
        terrain = GetComponent<Terrain>();
        terrainData = terrain.terrainData;
    }

    #region Generation
    public void CalculatePerlin()
    {

        int resolutionValue = (int)resolution;

        terrainData.alphamapResolution = resolutionValue;

        width = resolutionValue;
        height = resolutionValue;

        noiseHeights = new float[width, height];



        if (scale <= 0) scale = 0.0001f;

        float minValue = float.MaxValue;
        float maxValue = float.MinValue;

        foreach (Vector2 pos in shapeMap.GetPixelInfo())
        {
            float amplitude = 1;        //aka height
            float frequency = 1;        //aka lenght
            float noiseHeight = 0;

            for (int i = 0; i < octaves; i++)
            {
                float xCoord = ((float)pos.x / (width / 2) * scale * frequency) + offset.x;
                float yCoord = ((float)pos.y / (height / 2) * scale * frequency) + offset.y;

                float perlin = Mathf.PerlinNoise(xCoord, yCoord);
                noiseHeight += perlin * amplitude;

                amplitude *= persistence;
                frequency *= lacunarity;
            }

            if (noiseHeight < minValue) minValue = noiseHeight;
            if (noiseHeight > maxValue) maxValue = noiseHeight;

            noiseHeights[(int)pos.x, (int)pos.y] = noiseHeight;
        }

        float[,] falloffMapArray = falloffMap.GetFalloffValue(width, height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float falloffValue = falloffMapArray[x, y];
                noiseHeights[x, y] = terrainCurve.Evaluate(
                    (noiseHeights[x, y] - falloffValue)) * terrainHighMultiplier;

                // noiseHeights[x, y] = terrainCurve.Evaluate(
                //        (noiseHeights[x, y] - minValue) / (maxValue - minValue)) * terrainHighMultiplier;
            }
        }

        SplatMaps();

        GenerateRegions();      //DELETE LATER

        ApplyTerrainSettings();
    }

    private void ApplyTerrainSettings()
    {
        terrainData.heightmapResolution = (int)resolution;

        terrainData.size = new Vector3(width, terrainHeight, height);
        terrainData.SetHeights(0, 0, noiseHeights);
    }
    #endregion

    #region Splatmap Textures

    private void CreateLayers()
    {
        TerrainLayer[] newSplatPrototypes = new TerrainLayer[splatHeights.Count];

        for (int spIndex = 0; spIndex < splatHeights.Count; spIndex++)
        {
            SplatHeights sh = splatHeights[spIndex];

            TerrainLayer newLayer = new TerrainLayer
            {
                diffuseTexture = sh.texture
            };

            newLayer.diffuseTexture.Apply(true);
            string path = $"Assets/Terrain Layers/New Terrain Layer {spIndex}.terrainlayer";
            AssetDatabase.CreateAsset(newLayer, path);

            newSplatPrototypes[spIndex] = newLayer;
        }

        terrainData.terrainLayers = newSplatPrototypes;
    }

    private void SplatMaps()
    {
        CreateLayers();

        int mapResolution = terrainData.heightmapResolution;

        int alphaWidth = terrainData.alphamapWidth;
        int alphaHeight = terrainData.alphamapHeight;
        int alphaLayers = terrainData.alphamapLayers;

        float[,,] splatmapData = new float[alphaWidth, alphaHeight, alphaLayers];
        float[,] terrainHeights = terrainData.GetHeights(0, 0, mapResolution, mapResolution);

        for (int y = 0; y < alphaHeight; y++)
        {
            for (int x = 0; x < alphaWidth; x++)
            {
                float[] splatWeights = new float[alphaLayers];
                float currentPointHeight = terrainData.GetHeight(y, x) / terrainData.size.y;

                for (int j = 0; j < splatHeights.Count; j++)
                {
                    float min = splatHeights[j].minHeight;
                    float max = splatHeights[j].maxHeight;

                    if (currentPointHeight >= min && currentPointHeight <= max)
                    {
                        splatWeights[j] = 1;
                    }

                    for (int i = 0; i < splatWeights.Length; i++)
                        splatmapData[x, y, i] = splatWeights[i];
                    //float totalWeight = splatWeights.Sum();
                    //for (int i = 0; i < splatWeights.Length; i++)
                    //    splatmapData[x, y, i] = splatWeights[i] / totalWeight;
                }
            }
        }
        terrainData.SetAlphamaps(0, 0, splatmapData);
    }

    #endregion

    #region Serializable
    public enum TerrainResolution
    {
        Resolution_33x33 = 33,
        Resolution_65x65 = 65,
        Resolution_129x129 = 129,
        Resolution_257x257 = 257,
        Resolution_513x513 = 513,
        Resolution_1025x1025 = 1025,
        Resolution_2049x2049 = 2049,
        Resolution_4097x4097 = 4097
    }

    public enum FalloffType
    {
        Square,
        Circle
    }

    [System.Serializable]
    public class SplatHeights
    {
        public Texture2D texture = null;
        public float minHeight = 0.1f;
        public float maxHeight = 0.2f;
    }
    #endregion

    #region DELETE BEFORE PRODUCTION, ONLY FOR TESTING
    [System.Serializable]
    public struct TerrainRegion
    {
        public string name;
        public float height;
        public Color color;
    }

    public void GenerateRegions()
    {
        float[,] map = noiseHeights;

        Texture2D texture = new Texture2D(width, height);
        Color[] colorMap = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float currentH = map[x, y];
                for (int i = 0; i < regions.Length; i++)
                {
                    if (currentH <= regions[i].height)
                    {
                        colorMap[y * width + x] = regions[i].color;
                        break;
                    }
                }
            }
        }
        texture.SetPixels(colorMap);

        texture.Apply();

        //rend.sharedMaterial.mainTexture = texture;
        //texture.filterMode = FilterMode.Point;
    }

    Texture2D CreateTexture(float[,] map)
    {
        Texture2D texture = new Texture2D(width, height);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color color = CalculateColor(x, y, map);
                texture.SetPixel(x, y, color);
            }
        }
        texture.Apply();
        return texture;
    }

    Color CalculateColor(int x, int y, float[,] map)
    {
        return new Color(map[x, y], map[x, y], map[x, y]);
    }

    //void OnDrawGizmos()
    //{
    //    if (terrainData == null) return;

    //    int stepSize = 10;  // Reduce the number of points displayed
    //    Vector3 terrainPosition = terrain.transform.position;

    //    for (int y = 0; y < terrainData.heightmapResolution; y += stepSize)
    //    {
    //        for (int x = 0; x < terrainData.heightmapResolution; x += stepSize)
    //        {
    //            // Get the current height from the terrain
    //            float heightValue = terrainData.GetHeight(x, y) / terrainData.size.y;
    //            Vector3 position = new Vector3(x, heightValue, y);  // Add terrain's world position
    //            Handles.Label(position, heightValue.ToString("F2"));
    //        }
    //    }
    //}
    #endregion
}