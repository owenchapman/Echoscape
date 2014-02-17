using UnityEngine;
using System.Collections.Generic;

public class ListenerData : MonoBehaviour
{

    private float[] audioSamplesL;
    private float[] audioSamplesR;
    public Material lineMat;
    //private List<Vector3> bars;
    //public Texture barTexture;
    private int sampleCount = 512;
    //private int range = 32;
    public int rate = 0;
    public float[] samples;
    public Camera cam;

    private int vizCount = 8;

    // Use this for initialization
    void Start()
    {
        audioSamplesL = new float[sampleCount];
        audioSamplesR = new float[sampleCount];

        samples = new float[vizCount];

        for (int i = 0; i < vizCount; i++)
            samples[i] = 0f;

        //cam = this.transform.parent.GetComponent<Camera>();

    }

    // Update is called once per frame
    void FixedUpdate()
    {

        AudioListener.GetSpectrumData(audioSamplesL, 1, FFTWindow.Rectangular);
        AudioListener.GetSpectrumData(audioSamplesR, 0, FFTWindow.Rectangular);

        float scale = 1f;
        var w = Screen.width;
        //var h = Screen.height;
        var cellSize = (float)w / (float)(vizCount - 1);

        for (int i = 0; i < vizCount; i++)
        {

            Vector3 tmp = new Vector3();
            var diff = (audioSamplesL[(int)Mathf.Pow(2, i + 1)] + audioSamplesR[(int)Mathf.Pow(2, i)]) * scale;

            //soften the impact of the next sample - smoothes the visual representation
            diff -= samples[i];
            diff *= 0.2f;

            samples[i] = samples[i] + diff;
        }

    }

}
