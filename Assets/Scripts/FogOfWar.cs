using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogOfWar : MonoBehaviour
{
    public int textureSize = 512;
    public float worldSize = 50f;
    public float revealRadius = 5f;

    private Texture2D fogTexture;
    private Color32[] colors;
    //private byte[] revealMask;
    private SpriteRenderer sr;
    private float pixelPerUnit;

    //private bool[] hasBeenSeen;


    private void Start()
    {
        //hasBeenSeen = new bool[textureSize * textureSize];
        sr = GetComponent<SpriteRenderer>();
        fogTexture = new Texture2D(textureSize, textureSize, TextureFormat.ARGB32, false);
        fogTexture.filterMode = FilterMode.Bilinear;

        colors = new Color32[textureSize * textureSize];
        //revealMask = new byte[textureSize * textureSize];

        
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = new Color32(0, 0, 0, 255); // fully black
            //revealMask[i] = 0; // 0 = never seen
        }

        fogTexture.SetPixels32(colors);
        fogTexture.Apply();

        Sprite fogSprite = Sprite.Create(fogTexture, new Rect(0, 0, textureSize, textureSize), new Vector2(0.5f, 0.5f), textureSize / worldSize);
        sr.sprite = fogSprite;
        pixelPerUnit = textureSize / worldSize;
    }

/*     public void Reveal(Vector3 worldPos)
    {
        Vector2Int center = WorldToTextureCoord(worldPos);

        int radiusInPixels = Mathf.RoundToInt(revealRadius * pixelPerUnit);
        int sqrRadius = radiusInPixels * radiusInPixels;

        bool changed = false;

        for (int y = -radiusInPixels; y <= radiusInPixels; y++)
        {
            for (int x = -radiusInPixels; x <= radiusInPixels; x++)
            {
                int px = center.x + x;
                int py = center.y + y;

                if (px < 0 || py < 0 || px >= textureSize || py >= textureSize)
                    continue;

                if (x * x + y * y <= sqrRadius)
                {
                    int index = py * textureSize + px;

                    if (colors[index].a != 0)
                    {
                        colors[index].a = 0; // make fully transparent (visible)
                        changed = true;
                    }
                }
            }
        }

        if (changed)
        {
            fogTexture.SetPixels32(colors);
            fogTexture.Apply();
            Debug.Log("Reveal applied at: " + worldPos);
        }
    } */

    public void Reveal(Vector3 worldPos, float radius)
    {
        Vector2Int center = WorldToTextureCoord(worldPos);

        int radiusInPixels = Mathf.RoundToInt(radius * pixelPerUnit);
        int sqrRadius = radiusInPixels * radiusInPixels;

        for (int y = -radiusInPixels; y <= radiusInPixels; y++)
        {
            for (int x = -radiusInPixels; x <= radiusInPixels; x++)
            {
                int px = center.x + x;
                int py = center.y + y;

                if (px < 0 || py < 0 || px >= textureSize || py >= textureSize)
                    continue;

                if (x * x + y * y <= sqrRadius)
                {
                    int index = py * textureSize + px;
                    colors[index].a = 0; // reveal fully
                    //hasBeenSeen[index] = true;
                }
            }
        }
    }

    public void RevealCircularBlocked(Vector3 center, float radius, LayerMask obstacleMask, int rayCount = 100)
    {
        float angleStep = 360f / rayCount;

        for (int i = 0; i < rayCount; i++)
        {
            float angle = angleStep * i;
            float rad = angle * Mathf.Deg2Rad;
            Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad));
            RaycastHit2D hit = Physics2D.Raycast(center, dir, radius, obstacleMask);

            float distance = hit ? hit.distance : radius;
            Vector3 endPoint = center + dir * distance;

            RevealLine(center, endPoint);
        }

        fogTexture.SetPixels32(colors);
        fogTexture.Apply();
    }

    void RevealLine(Vector3 startWorld, Vector3 endWorld)
    {
        Vector2Int start = WorldToTextureCoord(startWorld);
        Vector2Int end = WorldToTextureCoord(endWorld);

        int dx = Mathf.Abs(end.x - start.x);
        int dy = Mathf.Abs(end.y - start.y);

        int sx = start.x < end.x ? 1 : -1;
        int sy = start.y < end.y ? 1 : -1;

        int err = dx - dy;

        int x = start.x;
        int y = start.y;

        while (true)
        {
            int index = y * textureSize + x;
            if (index >= 0 && index < colors.Length)
            {
                colors[index].a = 0;
            }

            if (x == end.x && y == end.y)
                break;

            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x += sx; }
            if (e2 < dx) { err += dx; y += sy; }
        }
    }

    /*
    private void LateUpdate()
    {
        // Dim everything that has been seen but is not currently visible
        for (int i = 0; i < colors.Length; i++)
        {
            if (revealMask[i] == 1 && colors[i].a > 0)
            {
                colors[i].a = 50; // dim
            }
        }

        fogTexture.SetPixels32(colors);
        fogTexture.Apply();
    }*/

    Vector2Int WorldToTextureCoord(Vector3 worldPos)
    {
        Vector3 localPos = transform.InverseTransformPoint(worldPos);
        float halfSize = worldSize / 2f;
        float normX = (localPos.x + halfSize) / worldSize;
        float normY = (localPos.y + halfSize) / worldSize;

        int x = Mathf.Clamp(Mathf.RoundToInt(normX * textureSize), 0, textureSize - 1);
        int y = Mathf.Clamp(Mathf.RoundToInt(normY * textureSize), 0, textureSize - 1);

        return new Vector2Int(x, y);
    }

    public void RevealConeMesh(List<Vector3> worldVertices)
    {
        //DimPreviouslySeen(); // reset before applying new vision
        ResetFogToBlack();

        for (int i = 1; i < worldVertices.Count - 1; i++)
        {
            RevealTriangle(worldVertices[0], worldVertices[i], worldVertices[i + 1]);
        }

        fogTexture.SetPixels32(colors);
        fogTexture.Apply();
    }

    void RevealTriangle(Vector3 a, Vector3 b, Vector3 c)
    {
        Bounds bounds = new Bounds(a, Vector3.zero);
        bounds.Encapsulate(b);
        bounds.Encapsulate(c);

        Vector2Int min = WorldToTextureCoord(bounds.min);
        Vector2Int max = WorldToTextureCoord(bounds.max);

        for (int y = min.y; y <= max.y; y++)
        {
            for (int x = min.x; x <= max.x; x++)
            {
                Vector3 worldPoint = TextureCoordToWorld(x, y);
                if (PointInTriangle(worldPoint, a, b, c))
                {
                    int index = y * textureSize + x;
                    colors[index].a = 0;
                    //hasBeenSeen[index] = true;
                }
            }
        }
    }

    Vector3 TextureCoordToWorld(int x, int y)
    {
        float halfSize = worldSize / 2f;
        float wx = ((float)x / textureSize) * worldSize - halfSize;
        float wy = ((float)y / textureSize) * worldSize - halfSize;
        return transform.TransformPoint(new Vector3(wx, wy));
    }

    bool PointInTriangle(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
    {
        // Barycentric technique
        Vector3 v0 = c - a;
        Vector3 v1 = b - a;
        Vector3 v2 = p - a;

        float d00 = Vector3.Dot(v0, v0);
        float d01 = Vector3.Dot(v0, v1);
        float d11 = Vector3.Dot(v1, v1);
        float d20 = Vector3.Dot(v2, v0);
        float d21 = Vector3.Dot(v2, v1);

        float denom = d00 * d11 - d01 * d01;
        if (denom == 0) return false;

        float v = (d11 * d20 - d01 * d21) / denom;
        float w = (d00 * d21 - d01 * d20) / denom;
        float u = 1.0f - v - w;

        return (u >= 0) && (v >= 0) && (w >= 0);
    }

    private void ResetFogToBlack()
    {
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i].a = 255; // fully black
        }
    }


    /*
    private void DimPreviouslySeen()
    {
        for (int i = 0; i < colors.Length; i++)
        {
            if (hasBeenSeen[i])
            {
                colors[i].a = 255; // dim alpha
            }
            else
            {
                colors[i].a = 255; // fully black
            }
        }
    }*/

}