using System.Collections.Generic;
using UnityEngine;

public class TerrainShapeMap : MonoBehaviour
{
    public Texture2D texture;

    private const int scaleFactor = 8; // 512 / 64 = 8
    [Range(0,1f)] public float distanceFactor; // 512 / 64 = 8

    private List<Vector3> islandPoints = new List<Vector3>();
    public List<Vector3> borders = new List<Vector3>();

    private void Start()
    {
        if (texture.width != 64 || texture.height != 64)
        {
            Debug.Log($"Border texture's resolution is {texture.width} and {texture.height}, make sure it's 64x64");
            return;
        }
        Generate();
    }

    private void Generate()
    {
        //1 = white, 0 = black
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                float pixelColorHeight = texture.GetPixel(x, y).grayscale;
                Color pixelColorBorder = texture.GetPixel(x, y);

                int scaledX = x * scaleFactor;
                int scaledY = y * scaleFactor;

                float tolerance = .9f;

                if (Mathf.Abs(pixelColorBorder.r - 1.0f) < tolerance && pixelColorBorder.g < tolerance && pixelColorBorder.b < tolerance)
                {
                    islandPoints.Add(new Vector2(scaledX, scaledY));
                }

                for (int scX = scaledX; scX < scaledX + scaleFactor; scX++)
                {
                    for (int scY = scaledY; scY < scaledY + scaleFactor; scY++)
                    {
                        if (!Mathf.Approximately(pixelColorHeight, 1f))
                            islandPoints.Add(new Vector2(scX, scY));
                    }
                }
            }
        }
    }

    public List<Vector3> GetIslandInfo()
    {
        return islandPoints;
    }

    public List<Vector3> GetBordersInfo()
    {
        return borders;
    }
    }
