using UnityEngine;

public class FalloffTest : MonoBehaviour
{
    public Texture2D inputImage;  // Assign the uploaded image in Unity Inspector
    public int size = 64;
    public AnimationCurve curve;
    private Texture2D falloffMapTexture;  // Generated falloff map
    private float[,] falloffMap1;  // Generated falloff map

    void Start()
    {
        

        GenerateFalloffMap();
        ///GenerateFalloffMapArray();
    }

    public float[,] GetFalloffValue(int a, int b)
    {
        return GenerateFalloffMapArray(a, b);
    }

#if UNITY_EDITOR
    void GenerateFalloffMap()
    {
        falloffMapTexture = new Texture2D(size, size);

        Vector2 islandCenter = FindIslandCenter();

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float pixelValue = inputImage.GetPixel(x, y).grayscale;
                if (Mathf.Approximately(pixelValue, 0f))
                {
                    float falloffValue = CalculateFalloffValue(x, y, islandCenter);

                    falloffMapTexture.SetPixel(x, y, new Color(falloffValue, falloffValue, falloffValue));
                }
                else falloffMapTexture.SetPixel(x, y, Color.white);

            }
        }

        falloffMapTexture.Apply();
    }
#endif

    float[,] GenerateFalloffMapArray(int a, int b)
    {
        falloffMap1 = new float[a,b];
        Vector2 islandCenter = FindIslandCenter();

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float pixelValue = inputImage.GetPixel(x, y).grayscale;
                if (Mathf.Approximately(pixelValue, 0f))
                {
                    float falloffValue = CalculateFalloffValue(x, y, islandCenter);
                    falloffMap1[x, y] = falloffValue;

                }
                else falloffMap1[x, y] = 1f;
            }
        }

        return falloffMap1;
    }

    Vector2 FindIslandCenter()
    {
        Vector2 centerOfIsland = Vector2.zero;
        int numIslandPositions = 0;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float pixelValue = inputImage.GetPixel(x, y).grayscale;

                if (Mathf.Approximately(pixelValue, 0f))
                {
                    centerOfIsland += new Vector2(x, y);
                    numIslandPositions++;
                }
            }
        }

        if (numIslandPositions > 0)
            return centerOfIsland / numIslandPositions;
        else return new Vector2(size / 2, size / 2);
    }

    float CalculateFalloffValue(int x, int y, Vector2 islandCenter)
    {
        float mapCenter = size / 2;

        float xDist = (x - islandCenter.x) / mapCenter;
        float yDist = (y - islandCenter.y) / mapCenter;

        return curve.Evaluate(Mathf.Sqrt(xDist * xDist + yDist * yDist));
    }

    void OnDrawGizmos()
    {
        // Debug draw falloff map
        if (falloffMapTexture != null)
        {
            Gizmos.DrawGUITexture(new Rect(10, 10, 256, 256), falloffMapTexture);
        }
    }
}
