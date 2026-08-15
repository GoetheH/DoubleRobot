using UnityEngine;

public class PixelSpriteGenerator : MonoBehaviour
{
    public static Texture2D CreatePixelTexture(int width, int height, Color[] colors)
    {
        Texture2D texture = new Texture2D(width, height);
        texture.filterMode = FilterMode.Point;
        texture.SetPixels(colors);
        texture.Apply();
        return texture;
    }

    public static Sprite CreateSpriteFromTexture(Texture2D texture, int pixelsPerUnit = 16)
    {
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), 
            new Vector2(0.5f, 0.5f), pixelsPerUnit);
    }

    public static Texture2D GenerateMechTexture(Color bodyColor, Color detailColor, bool facingRight = true)
    {
        int width = 16;
        int height = 16;
        Color[] pixels = new Color[width * height];

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.clear;
        }

        // Head
        SetPixelBlock(pixels, width, 5, 10, 6, 2, bodyColor);
        // Eyes
        SetPixel(pixels, width, 6, 11, Color.red);
        SetPixel(pixels, width, 9, 11, Color.red);

        // Body
        SetPixelBlock(pixels, width, 4, 5, 8, 4, bodyColor);
        // Chest detail
        SetPixelBlock(pixels, width, 6, 6, 4, 2, detailColor);

        // Arms
        SetPixelBlock(pixels, width, 2, 5, 2, 4, bodyColor);
        SetPixelBlock(pixels, width, 12, 5, 2, 4, bodyColor);

        // Weapon (right arm)
        SetPixelBlock(pixels, width, 14, 6, 1, 3, Color.gray);

        // Legs
        SetPixelBlock(pixels, width, 5, 1, 2, 4, bodyColor);
        SetPixelBlock(pixels, width, 9, 1, 2, 4, bodyColor);

        if (!facingRight)
        {
            pixels = FlipHorizontal(pixels, width, height);
        }

        return CreatePixelTexture(width, height, pixels);
    }

    public static Texture2D GenerateBackgroundTexture()
    {
        int width = 64;
        int height = 64;
        Color[] pixels = new Color[width * height];

        Color bgColor = new Color(0.15f, 0.1f, 0.2f, 1f);
        Color gridColor = new Color(0.2f, 0.15f, 0.25f, 1f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (x % 8 == 0 || y % 8 == 0)
                {
                    pixels[y * width + x] = gridColor;
                }
                else
                {
                    pixels[y * width + x] = bgColor;
                }
            }
        }

        return CreatePixelTexture(width, height, pixels);
    }

    private static void SetPixel(Color[] pixels, int width, int x, int y, Color color)
    {
        if (x >= 0 && x < width && y >= 0 && y < width)
        {
            pixels[y * width + x] = color;
        }
    }

    private static void SetPixelBlock(Color[] pixels, int width, int startX, int startY, int blockWidth, int blockHeight, Color color)
    {
        for (int y = 0; y < blockHeight; y++)
        {
            for (int x = 0; x < blockWidth; x++)
            {
                SetPixel(pixels, width, startX + x, startY + y, color);
            }
        }
    }

    private static Color[] FlipHorizontal(Color[] pixels, int width, int height)
    {
        Color[] flipped = new Color[pixels.Length];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                flipped[y * width + (width - 1 - x)] = pixels[y * width + x];
            }
        }
        return flipped;
    }
}
