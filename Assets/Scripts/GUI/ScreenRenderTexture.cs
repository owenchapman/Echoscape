using UnityEngine;
using System.Collections;

public class ScreenRenderTexture : MonoBehaviour
{

    public RenderTexture rTexture;
    public GameObject textureObj;
    public ScreenRenderTexture parentTexture;
    public Material mat;
    public bool inheritTexture = false;
    public GameObject targetGUICam;
    public float hMult = 1f;
    public float wMult = 1f;

    private GameObject texViz;
    private Vector3[] cornerVerts;
    public Camera cam;
    private int lastWidth;
    private int lastHeight;

    // Use this for initialization
    void Start()
    {

        //rTexture = new RenderTexture((int) (wMult * Screen.width), (int) (hMult * Screen.height), 0);
        //rTexture = new RenderTexture(2000, 1600, 0);
        //rTexture.antiAliasing = 2;
        //cam = this.camera;
        //cam.targetTexture = rTexture;
        //lastWidth = Screen.width;
        //lastHeight = Screen.height;
    }

    Mesh BasicPlane(Vector3[] cornerVerts)
    {
        //assume 4 vertices, no duplicates, and clockwise ordering       
        var plane = new Mesh();

        plane.vertices = cornerVerts;

        var tris = new int[6];

        tris[0] = 0; tris[1] = 1; tris[2] = 2;
        tris[3] = 0; tris[4] = 2; tris[5] = 3;

        plane.triangles = tris;

        var uvs = new Vector2[4];

        uvs[0] = new Vector2(0f, 0f);
        uvs[1] = new Vector2(0f, 1f);
        uvs[2] = new Vector2(1f, 1f);
        uvs[3] = new Vector2(1f, 0f);

        plane.uv = uvs;
        plane.RecalculateNormals();

        return plane;
    }

    Vector3[] CornerPointsFromCamera(Camera cam)
    {
        var cPoints = new Vector3[4];

        cPoints[0] = cam.ScreenToWorldPoint(new Vector3(0f, 0f, cam.nearClipPlane));
        cPoints[1] = cam.ScreenToWorldPoint(new Vector3(0f, cam.pixelHeight, cam.nearClipPlane));
        cPoints[2] = cam.ScreenToWorldPoint(new Vector3(cam.pixelWidth, cam.pixelHeight, cam.nearClipPlane));
        cPoints[3] = cam.ScreenToWorldPoint(new Vector3(cam.pixelWidth, 0f, cam.nearClipPlane));

        //for (int i = 0; i < 4; i++)
        //{
        //    cPoints[i] = targetGUICam.transform.InverseTransformPoint(cPoints[i]);
        //}

        return cPoints;
    }


}
