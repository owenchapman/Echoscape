using UnityEngine;
using System.Collections;

public class AudioSourceOptions : MonoBehaviour
{

    private float range = 20f;
    private bool onOff = false;
    public bool dispGUI = false;
    private GameObject[] audioNodes;
    public GUISkin skin;
    private bool on = false;
    private string onOffMsg;

    // Use this for initialization
    void Start()
    {

        audioNodes = GameObject.FindGameObjectsWithTag("AudioNode") as GameObject[];
        onOffMsg = "";
    }

    // Update is called once per frame
    void Update()
    {


        //adjust range of audio source
        if (dispGUI && Input.GetMouseButton(0))
        {
            for (int i = 0; i < audioNodes.Length; i++)
            {
                //var recNode = audioNodes[i].GetComponent<RecordingNode>();
                //var tmpData = recNode.data;

                var source = audioNodes[i].GetComponent<AudioSource>();
                source.maxDistance = range;
            }
        }

        //turn on or off all audio nodes
        if (onOff)
        {
            for (int i = 0; i < audioNodes.Length; i++)
            {
                var source = audioNodes[i].GetComponent<AudioSource>();


                if (on)
                {
                    source.Stop();
                    audioNodes[i].renderer.material.color = Color.green;
                }

                if (!on)
                {
                    source.Play();
                    audioNodes[i].renderer.material.color = Color.yellow;
                }
            }

            if (on)
            {
                on = false;
                onOffMsg = "On";
            }

            else
            {
                on = true;
                onOffMsg = "Off";
            }
        }

    }

    void OnGUI()
    {

        GUI.skin = skin;

        var gRect = new Rect((Screen.width / 2f) - 150f, (Screen.height / 2f) - 75f, 300f, 150f);
        var tRect = new Rect((Screen.width / 2f) - 125f, (Screen.height / 2f) - 25f, 250f, 25f);
        var b1Rect = new Rect((Screen.width / 2f) - 125f, (Screen.height / 2f) + 5, 250f, 25f);
        var b2Rect = new Rect((Screen.width / 2f) - 125f, (Screen.height / 2f) + 35, 250f, 25f);



        if (dispGUI)
        {
            GUI.Box(gRect, "Audio Node Options:");
            GUI.Label(tRect, "Audio Node Area of Effect: " + range);
            range = GUI.HorizontalSlider(b1Rect, range, 2f, 50f);
            onOff = GUI.Button(b2Rect, "      Turn All Nodes " + onOffMsg);
        }
    }
}
