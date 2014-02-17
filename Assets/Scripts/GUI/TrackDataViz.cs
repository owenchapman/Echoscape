using UnityEngine;
using System.Collections;
using System;

public class TrackDataViz : MonoBehaviour
{

    private ListenerData aData;
    public Material mat;
    private float[] lineVals;
    private WindowManager winManager;


    // Use this for initialization
    void Start()
    {

        aData = GameObject.Find("Player").GetComponentInChildren<ListenerData>();
        lineVals = new float[Screen.width];

        StartCoroutine(GrabSamples());

        for (int i = 0; i < lineVals.Length; i++)
        {
            lineVals[i] = 1f;
        }

        winManager = GameObject.FindWithTag("GUIManager").GetComponent<WindowManager>();

    }

    // Update is called once per frame
    void Update()
    {


    }

    void OnPostRender()
    {
        this.camera.DoClear();
        TrackViz();
    }

    void TrackViz()
    {
        var yPos = (Screen.height - winManager.yLevel) + 22f;

        for (int i = 0; i < lineVals.Length; i++)
        {

            GL.PushMatrix();
            mat.SetPass(0);
            GL.LoadPixelMatrix();
            GL.Begin(GL.LINES);
            GL.Vertex3(i, yPos, 0);
            GL.Vertex3(i, yPos + lineVals[i], 0);
            GL.End();
            GL.PopMatrix();
        }
    }

    IEnumerator GrabSamples()
    {
        while (true)
        {
            //get latest audio val
            var y = 0.005f;

            foreach (var v in aData.samples)
            {
                if (v > y)
                    y = v;
            }

            y *= 200f;

            Array.Copy(lineVals, 0, lineVals, 1, lineVals.Length - 1);
            lineVals[0] = Mathf.Max(y, 1);


            yield return new WaitForSeconds(0.01f);
        }
    }
}
