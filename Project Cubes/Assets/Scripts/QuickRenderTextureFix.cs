using System;
using UnityEngine;
using System.Collections;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class QuickRenderTextureFix : MonoBehaviour {
    public ComputeShader TextureComputeShader0;  // this must be public so it can be set in the inspector!!!!!!!!!!!!!
    protected int TextureCSMainKernel;

    private RenderTexture outputLifeTexture1;  // a random write texture  for TextureCSMainKernel()
    private Texture2D inputLifeTexture1;  // a readable texture

    int texWidth = 1024;
    int texHeight = 1024;

    GameObject primitive;


    // Use this for initialization
    void Start()
    {
        if (TextureComputeShader0 != null)
        {
            TextureCSMainKernel = TextureComputeShader0.FindKernel("TextureCSMainKernel");

            inputLifeTexture1 = new Texture2D(texWidth, texHeight, TextureFormat.ARGB32, false, true);
            inputLifeTexture1.name = "inputLifeTexture1";
            TextureComputeShader0.SetTexture(TextureCSMainKernel, "inputTex1", inputLifeTexture1);

            Color[] pix = new Color[texWidth * texHeight]; // SetPixels takes Color[], rgba
            Random.seed = 12345;
            for (int p = 0; p < (texWidth * texHeight); ++p)
                pix[p] = new Color(Random.value, Random.value, Random.value, 1);  // rgba
            inputLifeTexture1.SetPixels(pix);
            inputLifeTexture1.Apply();

            outputLifeTexture1 = new RenderTexture(texWidth, texHeight, 0, RenderTextureFormat.ARGB32,
                            RenderTextureReadWrite.Linear);
            outputLifeTexture1.name = "outputLifeTexture1";
            outputLifeTexture1.enableRandomWrite = true;
            outputLifeTexture1.Create();  // otherwise not created until first time it is set to active
            TextureComputeShader0.SetTexture(TextureCSMainKernel, "outputTex1", outputLifeTexture1);
            RenderTexture.active = outputLifeTexture1;
            Graphics.Blit(inputLifeTexture1, outputLifeTexture1);
            RenderTexture.active = null;
        }


        primitive = GameObject.CreatePrimitive(PrimitiveType.Plane);
        primitive.GetComponent<Renderer>().castShadows = false;
        primitive.GetComponent<Renderer>().receiveShadows = false;
        primitive.transform.rotation = Quaternion.AngleAxis(90, Vector3.back);
        primitive.transform.Rotate(new Vector3(90, 0, 0), Space.World);
        primitive.transform.position = new Vector3(-20, 10, 0);
        primitive.transform.localScale = new Vector3(1, 1, 1);
        Material material = new Material(Shader.Find("Unlit/Transparent"));
        primitive.GetComponent<Renderer>().material = material;
        primitive.GetComponent<Renderer>().material.color = Color.white;
        primitive.GetComponent<Renderer>().material.renderQueue = 4000;  // force renderqueue to be after all other transparencies
        primitive.GetComponent<Renderer>().material.mainTexture = outputLifeTexture1;
        //  primitive.renderer.material.mainTexture=inputLifeTexture1;
    }


    void Update()
    {

        if (TextureComputeShader0 != null)
            TextureComputeShader0.Dispatch(TextureCSMainKernel, texWidth / 32, texHeight / 32, 1);
    }

    void OnPostRender()
    {   // we are still in the render frame at this point
        if (outputLifeTexture1 != null)
        {
            RenderTexture.active = outputLifeTexture1;  // copy RenderTexture to Texture2D
            inputLifeTexture1.ReadPixels(new Rect(0, 0, outputLifeTexture1.width, outputLifeTexture1.height), 0, 0);
            inputLifeTexture1.Apply();
            RenderTexture.active = null;
        }
    }
}
