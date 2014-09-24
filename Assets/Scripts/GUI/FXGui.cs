using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(AudioReverbFilter))]
[RequireComponent(typeof(AudioEchoFilter))]
[RequireComponent(typeof(AudioHighPassFilter))]
[RequireComponent(typeof(AudioLowPassFilter))]
[RequireComponent(typeof(AudioChorusFilter))]

public class FXGui : MonoBehaviour
{

    public List<EffectController> controllers;
    private Rect clientRect;
    private Rect[] fiveByOneGrid;
    private GUISkin skin;

    // Use this for initialization
    void Start()
    {
        skin = Resources.Load("Skins/MainWindows") as GUISkin;
        
        controllers = new List<EffectController>();
        clientRect = new Rect(0, 0.61f * Screen.height, Screen.width, 0.4f * Screen.height - 25f);
        fiveByOneGrid = Util.SelectionGrid(clientRect, 5, 5, clientRect.height - 2, 2f);

        AudioReverbFilter reverb = this.GetComponent<AudioReverbFilter>();
        AudioEchoFilter echo = this.GetComponent<AudioEchoFilter>();
        AudioHighPassFilter high = this.GetComponent<AudioHighPassFilter>();
        AudioLowPassFilter low = this.GetComponent<AudioLowPassFilter>();
        AudioChorusFilter chorus = this.GetComponent<AudioChorusFilter>();

        //init Reverb Controller
        var reverbParams = new AudioParameter[4];
        reverbParams[0] = new AudioParameter(-1000f, 0f, -1000, "dryLevel", "dry");
        reverbParams[1] = new AudioParameter(0f, 20f, 0f, "decayTime", "decay");
        reverbParams[2] = new AudioParameter(-10000f, 0f, 0f, "reverbLevel", "wet");
        reverbParams[3] = new AudioParameter(0f, 0.1f, 0f, "reverbDelay", "delay");
        controllers.Add(new EffectController(reverbParams, fiveByOneGrid[0], "Reverb", reverb));

        //init Echo Controller
        var echoParams = new AudioParameter[4];
        echoParams[0] = new AudioParameter(0f, 1000f, 500, "delay", "delay");
        echoParams[1] = new AudioParameter(0f, 1f, 0.5f, "decayRatio", "decay");
        echoParams[2] = new AudioParameter(0f, 1f, 1f, "wetMix", "wet");
        echoParams[3] = new AudioParameter(0f, 1f, 1f, "dryMix", "dry");
        controllers.Add(new EffectController(echoParams, fiveByOneGrid[1], "Echo", echo));

        //init HighPass Controller
        var highParams = new AudioParameter[2];
        highParams[0] = new AudioParameter(10f, 22000f, 10f, "cutoffFrequency", "cut off");
        highParams[1] = new AudioParameter(0f, 20f, 4f, "highpassResonaceQ", "reso");
        controllers.Add(new EffectController(highParams, fiveByOneGrid[2], "HighPassFilter", high));

        //init LowPass Controller
        var lowParams = new AudioParameter[2];
        lowParams[0] = new AudioParameter(10f, 22000f, 22000, "cutoffFrequency", "cut off");
        lowParams[1] = new AudioParameter(0f, 20f, 4f, "lowpassResonaceQ", "reso");
        controllers.Add(new EffectController(lowParams, fiveByOneGrid[3], "LowPassFilter", low));

        //init Chorus Controller
        var chorusParams = new AudioParameter[4];
        chorusParams[0] = new AudioParameter(0f, 1f, 1f, "dryMix", "dry");
        chorusParams[1] = new AudioParameter(0.1f, 100f, 40f, "delay", "delay");
        chorusParams[2] = new AudioParameter(0f, 201f, 0.8f, "rate", "rate");
        chorusParams[3] = new AudioParameter(0f, 1f, 0.03f, "depth", "depth");
        controllers.Add(new EffectController(chorusParams, fiveByOneGrid[4], "Chorus", chorus));

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Display()
    {

        GUI.skin = skin;

        foreach (var c in controllers)
            c.Display();
    }
}
