using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpdateExploredMap : MonoBehaviour
{
    public RenderTexture visionTexture;
    public RenderTexture exploredTexture;
    public Material blendMaterial;

    void LateUpdate()
    {
        Graphics.Blit(visionTexture, exploredTexture, blendMaterial);
    }
}
