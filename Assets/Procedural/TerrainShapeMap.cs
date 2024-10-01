using System.Collections.Generic;
using UnityEngine;

public class TerrainShapeMap : MonoBehaviour
{
    public Texture2D texture;

    private const int scaleFactor = 8; // 512 / 64 = 8

    private List<Vector2> list = new List<Vector2>();

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
                float pixelColor = texture.GetPixel(x, y).grayscale;

                int scaledX = x * scaleFactor;
                int scaledY = y * scaleFactor;

                for (int scX = scaledX; scX < scaledX + scaleFactor; scX++)
                {
                    for (int scY = scaledY; scY < scaledY + scaleFactor; scY++)
                    {

                        if (!Mathf.Approximately(pixelColor, 1f))
                            list.Add(new Vector2(scX, scY));
                    }
                }
            }
        }

    }

    public List<Vector2> GetPixelInfo()
    {
        return list;
    }
}
