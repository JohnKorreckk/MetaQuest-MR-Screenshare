using UnityEngine;
using System.Collections;
using PassthroughCameraSamples;

[RequireComponent(typeof(Renderer))]

public class WebcamTextureAssigner : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    IEnumerator Start()
    {
        WebCamTextureManager webCamTextureManager = null;
        WebCamTexture webCamTexture = null;

        do
        {
            yield return null;

            if (webCamTextureManager == null)
            {
                webCamTextureManager = FindFirstObjectByType<WebCamTextureManager>();
            }
            else
            {
                webCamTexture = webCamTextureManager.WebCamTexture;
            }
        } while (webCamTexture == null);

        GetComponent<Renderer>().material.mainTexture = webCamTexture;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
