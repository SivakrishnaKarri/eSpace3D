using UnityEngine;
using SFB;

public class SnapShot : MonoBehaviour
{

    [SerializeField]
    Camera roomCamera;

    public void TakeSnapShot()
    {
        var filePath = StandaloneFileBrowser.SaveFilePanel("Select a File", "", "snapshot", "jpg");
        if(!string.IsNullOrEmpty(filePath))
            System.IO.File.WriteAllBytes(filePath, RTImage(roomCamera).EncodeToPNG());
    }
    
    // Take a "screenshot" of a camera's Render Texture.
    Texture2D RTImage(Camera camera)
    {
        RenderTexture rt = new RenderTexture(camera.pixelWidth, camera.pixelHeight, 24);
        camera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(camera.pixelWidth, camera.pixelHeight, TextureFormat.RGB24, false);
        camera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, camera.pixelWidth, camera.pixelHeight), 0, 0);
        camera.targetTexture = null;
        RenderTexture.active = null; // JC: added to avoid errors
        Destroy(rt);
        return screenShot;
    }
}