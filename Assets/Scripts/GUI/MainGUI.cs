using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

public enum RecordState
{
    Recording,
    Stopped,
    Initialize,
    Finalize
}

public static class MainGUI
{
    public static AudioReverbFilter reverb;
    public static AudioEchoFilter echo;
    public static AudioHighPassFilter high;
    public static AudioLowPassFilter low;
    public static AudioChorusFilter chorus;
    public static AudioListener listener;
    public static Recorder audioRecorder;

    public static Rect clientRect;
    public static Rect[] fiveByOneGrid;
    public static Rect[] threeByOneGrid;
    public static float winHeight;
    public static GameObject audioListener;
    public static ScapeSelector scapeSelector;
    public static NodeFilter nodeFilter;
    public static MovePlayerToNode movePlayer;
    public static LoadAudioFile audioFileLoader;

    public static List<EffectController> controllers;
    public static GUISkin skin;

    public static bool recTrigger;
    public static bool loopTrigger;

    private static Event currEvent;
    private static Vector2 scrollPos;
    private static string searchString;
    private static AudioMobileModel currNodeOnDisplay;
    private static GameObject currNodeObj;
    private static float audioRange;
    private static GameObject nodeParentObj;
    private static bool autoLoop = false;

    private static string message = "";
    private static float alpha = 1.0f;
    private static char pathChar = '/';

    public static void Init()
    {
        //lots of things to initialize...      
        audioListener = GameObject.FindWithTag("AudioListener");
        reverb = audioListener.GetComponent<AudioReverbFilter>();
        echo = audioListener.GetComponent<AudioEchoFilter>();
        high = audioListener.GetComponent<AudioHighPassFilter>();
        low = audioListener.GetComponent<AudioLowPassFilter>();
        chorus = audioListener.GetComponent<AudioChorusFilter>();
        audioFileLoader = GameObject.FindGameObjectWithTag("UserSounds").GetComponent<LoadAudioFile>();
        audioRecorder = audioListener.GetComponent<Recorder>();

        scrollPos = Vector2.zero;
        loopTrigger = false;

        listener = (AudioListener)audioListener.GetComponent<AudioListener>();
        scapeSelector = GameObject.Find("SlippyMap").GetComponent<ScapeSelector>();
        nodeFilter = GameObject.FindGameObjectWithTag("NodeManager").GetComponent<NodeFilter>();
        clientRect = new Rect(0, 0, Screen.width, winHeight);
        movePlayer = GameObject.FindGameObjectWithTag("Player").GetComponent<MovePlayerToNode>();
        searchString = "";

        fiveByOneGrid = Util.SelectionGrid(clientRect, 5, 5, clientRect.height - 2, 2f);
        threeByOneGrid = Util.SelectionGrid(clientRect, 3, 3, clientRect.height - 2, 2f);

        controllers = new List<EffectController>();

        //init Reverb Controller
        var reverbParams = new AudioParameter[4];
        reverbParams[0] = new AudioParameter(-1000f, 0f, -1000, "dryLevel", "dry");
        reverbParams[1] = new AudioParameter(0f, 20f, 0f, "decayTime", "decay");
        reverbParams[2] = new AudioParameter(-10000f, 0f, 0f, "reverbLevel", "wet");
        reverbParams[3] = new AudioParameter(0f, 0.1f, 0f, "reverbDelay", "delay");
        controllers.Add(new EffectController(reverbParams, fiveByOneGrid[0], "Reverb Effect", reverb));

        //init Echo Controller
        var echoParams = new AudioParameter[4];
        echoParams[0] = new AudioParameter(0f, 1000f, 500, "delay", "delay");
        echoParams[1] = new AudioParameter(0f, 1f, 0.5f, "decayRatio", "decay");
        echoParams[2] = new AudioParameter(0f, 1f, 1f, "wetMix", "wet");
        echoParams[3] = new AudioParameter(0f, 1f, 1f, "dryMix", "dry");
        controllers.Add(new EffectController(echoParams, fiveByOneGrid[1], "Echo Effect", echo));

        //init HighPass Controller
        var highParams = new AudioParameter[2];
        highParams[0] = new AudioParameter(10f, 22000f, 10f, "cutoffFrequency", "cut off");
        highParams[1] = new AudioParameter(0f, 20f, 4f, "highpassResonaceQ", "reso");
        controllers.Add(new EffectController(highParams, fiveByOneGrid[2], "High Pass Filter", high));

        //init LowPass Controller
        var lowParams = new AudioParameter[2];
        lowParams[0] = new AudioParameter(10f, 22000f, 22000, "cutoffFrequency", "cut off");
        lowParams[1] = new AudioParameter(0f, 20f, 4f, "lowpassResonaceQ", "reso");
        controllers.Add(new EffectController(lowParams, fiveByOneGrid[3], "Low Pass Filter", low));

        //init Chorus Controller
        var chorusParams = new AudioParameter[4];
        chorusParams[0] = new AudioParameter(0f, 1f, 1f, "dryMix", "dry");
        chorusParams[1] = new AudioParameter(0.1f, 100f, 40f, "delay", "delay");
        chorusParams[2] = new AudioParameter(0f, 201f, 0.8f, "rate", "rate");
        chorusParams[3] = new AudioParameter(0f, 1f, 0.03f, "depth", "depth");
        controllers.Add(new EffectController(chorusParams, fiveByOneGrid[4], "Chorus", chorus));

    }


    //specific window drawing function.
    public static void SelectLayout(int ID)
    {
        switch (ID)
        {
            case 0:
                MapGUI();
                break;

            case 1:
                foreach (var c in controllers)
                    c.Display();
                break;

            case 2:
                Options();
                break;
        }
    }

    private static void MapGUI()
    {
        var loadRect = new Rect(clientRect.width - 105, 0, 100, 55);

        var infoRect = new Rect(0, 0, 400, 100);
        var offset = new RectOffset(5, 5, 5, 5);
        var contentRect = offset.Remove(loadRect);

        infoRect = offset.Remove(infoRect);

        GUILayout.BeginArea(infoRect);
        GUILayout.Label("To select an Echoscape, right mouse click on the map to place the marker."
                        + "\nThen, click the button at the bottom of the screen", skin.customStyles[7]);
        GUILayout.EndArea();

        GUILayout.BeginArea(contentRect);
        if (GUILayout.Button("Load Echoscape"))
            scapeSelector.LoadTile();
        GUILayout.EndArea();
    }

    public static void RecordGUI()
    {
        var recordRect = new Rect(0, 0, 300, 125);
        var offset = new RectOffset(5, 5, 5, 5);
        var contentRect = offset.Remove(recordRect);


        GUILayout.BeginArea(contentRect);
        GUILayout.BeginHorizontal();


        var timeCounter = "record what you hear";

        //        audioRecorder.loop = GUILayout.Toggle(audioRecorder.loop, "Loop");
        //        audioRecorder.autoName = GUILayout.Toggle(audioRecorder.autoName, "Auto Name");
        //        audioRecorder.writeToFile = GUILayout.Toggle(audioRecorder.writeToFile, "Write To File");

        //record behaviour
        recTrigger = GUILayout.Toggle(recTrigger, "", skin.customStyles[10]);

        if (recTrigger)
            timeCounter = (Time.timeSinceLevelLoad - audioRecorder.timeStamp).ToString();

        audioRecorder.ToggleRecord(recTrigger);
        GUILayout.Space(5);

        GUILayout.Label(timeCounter, skin.customStyles[6]);
        GUILayout.EndHorizontal();

        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        //upload a user sound
        if (GUILayout.Button("", skin.customStyles[11]))
            audioFileLoader.LoadFile();
        
        GUILayout.Space(5);
        GUILayout.Label("Load a sound or scene", skin.customStyles[6]);
        GUILayout.EndHorizontal();

		GUILayout.Space(10);		
		GUILayout.BeginHorizontal();
		//upload a user sound
		if (GUILayout.Button("", skin.customStyles[12]))		
			audioFileLoader.SaveFile();
		GUILayout.Space(5);		
		GUILayout.Label("Save the current scene", skin.customStyles[6]);		
		GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }


    private static void Options()
    {
        foreach (var r in threeByOneGrid)
            GUI.Box(r, "");

        AudioNodeFilterGUI();
        AudioNodeDetails(threeByOneGrid[1]);

        GlobalSettings();

    }

    public static void AudioNodeFilterGUI()
    {
        var currColRect = threeByOneGrid[0];

        var allNodes = nodeFilter.filteredNodes;
        var scrollOffset = new RectOffset(5, 5, 40, 5);
        var scrollRect = scrollOffset.Remove(currColRect);

        var viewRect = new Rect(scrollRect.x, scrollRect.y, scrollRect.width - 25, Mathf.Max(scrollRect.height, allNodes.Count * 35f));

        var searchRect = new Rect(scrollRect.x, currColRect.y + 5, scrollRect.width - 60, 27);
        var goSearhRect = new Rect(currColRect.width - 60, currColRect.y + 5, 28, 28);
        var clearRect = new Rect(currColRect.width - 30, currColRect.y + 5, 28, 28);

        searchString = GUI.TextField(searchRect, searchString, 60);

        if (GUI.Button(goSearhRect, "Go"))
        {
            nodeFilter.FilterNodes(searchString);
        }

        if (GUI.Button(clearRect, "X"))
        {
            nodeFilter.ResetToCompleteList();
        }

        //Scrollable List of all audio nodes in scene
        scrollPos = GUI.BeginScrollView(scrollRect, scrollPos, viewRect);

        for (int i = 0; i < allNodes.Count; i++)
        {
            var currRect = new Rect(scrollRect.x, scrollRect.y + i * 35f, scrollRect.width, 35f);

            var msg = "No info...";

            if (allNodes[i] != null)
            {
                var tmpNode = allNodes[i].GetComponent<RecordingNode>();
                if (tmpNode != null)
                    msg = tmpNode.data.Author + ": " + tmpNode.data.Latitude + ", " + tmpNode.data.Longitude;

            }


            if (GUI.Button(currRect, msg))
            {
                var recNode = allNodes[i].GetComponent<RecordingNode>();
                var tmpData = recNode.data;
                currNodeOnDisplay = tmpData;
                currNodeObj = allNodes[i];
            }
        }

        GUI.EndScrollView();
    }

    public static void AudioNodeDetails(Rect dispRect)
    {

        try
        {
            var offset = new RectOffset(15, 5, 40, 37);
            var contentRect = offset.Remove(dispRect);
            var viewNodeRect = new Rect(contentRect.x, contentRect.y + contentRect.height + 5, contentRect.width - 5, 28);

            var data = currNodeOnDisplay;

            var texRect = new RectOffset(5, 5, 5, 5).Remove(dispRect);

            GUI.color = 0.6f * Color.white;
            GUI.DrawTexture(texRect, currNodeObj.GetComponent<RecordingNode>().currPic, ScaleMode.ScaleAndCrop);
            GUI.color = Color.white;

            GUILayout.BeginArea(contentRect);

            GUILayout.Label("Node Details:  ");
            GUILayout.Label("");

            GUILayout.Label(data.Author);
            GUILayout.Label(data.Date);
            GUILayout.Label("");
            GUILayout.Label(data.SoundURL);
            GUILayout.Label(data.Latitude + ", " + data.Longitude);
            GUILayout.EndArea();

            if (GUI.Button(viewNodeRect, "View Node in scene"))
            {
                movePlayer.MovePlayer(currNodeObj);
                Debug.Log(currNodeObj.transform.position);
            }


        }

        catch { }
    }

    public static void GlobalSettings()
    {
        var offset = new RectOffset(5, 5, 40, 5);
        var contentRect = offset.Remove(threeByOneGrid[2]);


        GUILayout.BeginArea(contentRect);
        GUILayout.Label("Default folder for recordings: ");
        audioRecorder.defaultDir = GUILayout.TextArea(audioRecorder.defaultDir, skin.customStyles[5]);
        audioFileLoader.defaultDir = audioRecorder.defaultDir;

		GUILayout.Space(10);
		GUILayout.Label("User name: ");
		Global.userName = GUILayout.TextArea(Global.userName, skin.customStyles[5]);

		GUILayout.Space(10);
		GUILayout.Label("Location: ");
		Global.location = GUILayout.TextArea(Global.location, skin.customStyles[5]);
		
		
		GUILayout.Space(10);
        GUILayout.Label("Audio Node Effect Range: ");
        audioRange = GUILayout.HorizontalSlider(audioRange, 0.1f, 10f);



        GUILayout.EndArea();
    }

}
