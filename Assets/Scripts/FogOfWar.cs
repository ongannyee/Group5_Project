using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogOfWar : MonoBehaviour
{
    public int textureSize = 512;       //Size of the fog in pixels
    public float worldSize = 50f;       //size of the fog area in Unity world units

    private Texture2D fogTexture;       //actual texture storing fog visibility, modified at runtime to reveal/hide areas.
    private Color32[] colors;           //Stores pixel color data for the fogTexture
    private SpriteRenderer sr;          //component that renders the fog texture
    private float pixelPerUnit;         //World-to-pixel conversion ratio

    private void Start()
    {
        // Component & Texture Initialization
        sr = GetComponent<SpriteRenderer>();
        fogTexture = new Texture2D(textureSize, textureSize, TextureFormat.ARGB32, false);
        fogTexture.filterMode = FilterMode.Bilinear;

        // Set Texture to Fully Black
        colors = new Color32[textureSize * textureSize];
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = new Color32(0, 0, 0, 255); // fully black
        }
        fogTexture.SetPixels32(colors);
        fogTexture.Apply();

        // Assign to SpriteRenderer. Show it visually in the game
        Sprite fogSprite = Sprite.Create(fogTexture, new Rect(0, 0, textureSize, textureSize), new Vector2(0.5f, 0.5f), textureSize / worldSize);
        sr.sprite = fogSprite;

        // Track pixel/world conversion. Needed for revealing based on player/world position
        pixelPerUnit = textureSize / worldSize;
    }

/*     //reveals a circular area, not block by obstacles
    public void Reveal(Vector3 worldPos, float radius)
    {
        Vector2Int center = WorldToTextureCoord(worldPos);              //Converts the world position (e.g., player position) into texture coordinates (pixels).

        int radiusInPixels = Mathf.RoundToInt(radius * pixelPerUnit);   //Converts the world radius into texture pixels.
        int sqrRadius = radiusInPixels * radiusInPixels;                //For checking x² + y² <= r², used to avoid square root calculations (for performance)

        for (int y = -radiusInPixels; y <= radiusInPixels; y++)         // Loops over a square around the center point
        {
            for (int x = -radiusInPixels; x <= radiusInPixels; x++)     // Loops over a square around the center point
            {
                int px = center.x + x;
                int py = center.y + y;

                if (px < 0 || py < 0 || px >= textureSize || py >= textureSize) //Bounds Checking
                    continue;

                if (x * x + y * y <= sqrRadius)                                 //Checks if the current pixel is inside the circle and set to alpha 0
                {
                    int index = py * textureSize + px;
                    colors[index].a = 0;                                        // reveal fully
                }
            }
        }
    } */

    // Reveal ciruclar vision
    public void RevealCircularBlocked(Vector3 center, float radius, LayerMask obstacleMask, int rayCount = 360)
    {

        int radiusInPixels = Mathf.RoundToInt(radius * pixelPerUnit);
        float angleStep = 360f / rayCount;      //Divides the full circle into equal segments.

        for (int i = 0; i < rayCount; i++)
        {
            float angle = angleStep * i;
            float rad = angle * Mathf.Deg2Rad;                               // Converts angle to radians.
            Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad));       // Calculates the direction vector from angle

            RaycastHit2D hit = Physics2D.Raycast(center, dir, radius, obstacleMask);    // Casts a ray from the center outward in that direction
                                                                                        // If it hits something in the obstacleMask, it returns hit.distance.
            
            float distance = hit ? hit.distance : radius;   // Use full radius dis or hit radius dis
            Vector3 endPoint = center + dir * distance;     // Compute the ray end: either at the obstacle or at full vision radius.
            RevealLine(center, endPoint, radiusInPixels);                 // Reveal pixels along the ray line
        }

        fogTexture.SetPixels32(colors);
        fogTexture.Apply();                                 // Applying the changes
    }

    // line-drawing algorithm: Bresenham's Line Algorithm
    void RevealLine(Vector3 startWorld, Vector3 endWorld, int radiusInPixels)
    {
        // Converts world space coordinates (e.g., player position, ray hit point) to pixel coordinates on the fog texture.
        Vector2Int start = WorldToTextureCoord(startWorld);     
        Vector2Int end = WorldToTextureCoord(endWorld);

        //Differences and Directions
        int dx = Mathf.Abs(end.x - start.x);
        int dy = Mathf.Abs(end.y - start.y);

        //Set step direction for x and y (left/right, up/down)
        int sx = start.x < end.x ? 1 : -1;
        int sy = start.y < end.y ? 1 : -1;

        // Start point to draw, err for error correction(which pixel to draw next)
        int err = dx - dy;
        int x = start.x;
        int y = start.y;

        while (true)
        {
            //Calculate 1D index from 2D position. Makes that pixel transparent.
            int index = y * textureSize + x;
            if (index >= 0 && index < colors.Length)
            {
                Vector2Int center = WorldToTextureCoord(startWorld);
                float dx_ = x - center.x;
                float dy_ = y - center.y;
                float distanceRatio = Mathf.Sqrt(dx * dx + dy * dy) / (radiusInPixels); // normalize distance
                // distanceRatio = Mathf.Clamp01(distanceRatio); // prevent overexposure

                // Invert so it's 1 near center, 0 at edges
                float falloff = 1f - Mathf.SmoothStep(0f, 0.5f, distanceRatio);
                byte softAlpha = (byte)(152 * Mathf.Clamp(falloff, 0f, 1f)); // keep at least 10% fog if desired
                colors[index].a = (byte)Mathf.Min(colors[index].a, softAlpha); // take minimum to preserve fading
            }

            // Break if reach end
            if (x == end.x && y == end.y)
                break;

            // Decides whether to step horizontally, vertically, or both, based on accumulated error. Ensures the line looks natural and continuous.
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x += sx; }
            if (e2 < dx) { err += dx; y += sy; }
        }
    }

    // map world positions to pixel positions for revealing or modify the fog
    Vector2Int WorldToTextureCoord(Vector3 worldPos)
    {
        Vector3 localPos = transform.InverseTransformPoint(worldPos);                       // Converts world position to local space relative to the fog object.
        float halfSize = worldSize / 2f;                                                    // Shifts coordinates so the fog's center is treated as (0,0)
        float normX = (localPos.x + halfSize) / worldSize;                                  // Normalizes x and y into values between 0 and 1.
        float normY = (localPos.y + halfSize) / worldSize;

        int x = Mathf.Clamp(Mathf.RoundToInt(normX * textureSize), 0, textureSize - 1);     // Converts normalized (0-1) values to actual pixel coordinates on the texture
        int y = Mathf.Clamp(Mathf.RoundToInt(normY * textureSize), 0, textureSize - 1);     

        return new Vector2Int(x, y);
    }

    // Reveal cone vision
    public void RevealConeMesh(List<Vector3> worldVertices)
    {
        // Reset Fog of war to black
        ResetFogToBlack();

        // reveal each triangle (3 vertices form a triangle)
        for (int i = 1; i < worldVertices.Count - 1; i++)
        {
            RevealTriangle(worldVertices[0], worldVertices[i], worldVertices[i + 1]);
        }

        // Update pixel data
        fogTexture.SetPixels32(colors);
        fogTexture.Apply();
    }

    Vector3 TextureCoordToWorld(int x, int y)
    {
        float halfSize = worldSize / 2f;
        float wx = ((float)x / textureSize) * worldSize - halfSize;
        float wy = ((float)y / textureSize) * worldSize - halfSize;
        return transform.TransformPoint(new Vector3(wx, wy, 0));
    }

    // Reveal triangle vision so that they form into a cone
    void RevealTriangle(Vector3 a, Vector3 b, Vector3 c)
    {
        Bounds bounds = new Bounds(a, Vector3.zero);
        bounds.Encapsulate(b);
        bounds.Encapsulate(c);

        Vector2Int min = WorldToTextureCoord(bounds.min);
        Vector2Int max = WorldToTextureCoord(bounds.max);

        Vector3 center = (a + b + c) / 3f; // approximate center for distance calculation
        Vector2Int centerTex = WorldToTextureCoord(center);

        float maxDist = Vector3.Distance(center, a); // approximate max distance for normalization
        float maxDistTex = maxDist * pixelPerUnit;

        for (int y = min.y; y <= max.y; y++)
        {
            for (int x = min.x; x <= max.x; x++)
            {
                Vector3 worldPoint = TextureCoordToWorld(x, y);
                if (PointInTriangle(worldPoint, a, b, c))
                {
                    int index = y * textureSize + x;
                    if (index >= 0 && index < colors.Length)
                    {
                        float dx = x - centerTex.x;
                        float dy = y - centerTex.y;
                        float distance = Mathf.Sqrt(dx * dx + dy * dy);

                        float distanceRatio = distance / maxDistTex;
                        float falloff = 1f - Mathf.SmoothStep(0f, 0.5f, distanceRatio);
                        byte softAlpha = (byte)(152 * Mathf.Clamp(falloff, 0f, 1f));

                        colors[index].a = (byte)Mathf.Min(colors[index].a, softAlpha);
                    }
                }
            }
        }
    }

    // checks whether a point p lies inside the triangle defined by the points a, b, and c, using the barycentric coordinate technique.
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

        return (u >= 0) && (v >= 0) && (w >= 0);    // Ensures the point lies inside or on the edge of the triangle.
    }

    // Reset for to black
    private void ResetFogToBlack()
    {
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i].a = 255; // fully black
        }
    }

/*     //Handle Kieran and DASH vision
    public void RevealFromMultipleSources(List<List<Vector3>> coneMeshes, List<Vector3> circularCenters, float radius, LayerMask obstacleMask)
    {
        foreach (var cone in coneMeshes)
        {
            RevealConeMesh(cone);
        }

        foreach (var center in circularCenters)
        {
            RevealCircularBlocked(center, radius, obstacleMask);
        }
    } */
}