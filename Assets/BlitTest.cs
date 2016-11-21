using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class BlitTest : MonoBehaviour
{
    public Texture aTexture;
    public RenderTexture rTex;
    public Material rMat;

    void Start()
    {
        rTex = new RenderTexture();
    }
    void Update()
    {

        Graphics.Blit(aTexture, (RenderTexture)null);
        //tex2dsample(uv)
    }
}

