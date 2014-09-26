using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Progress
{
    None,
    Loading,
    Complete
}

public class RecordingNode : MonoBehaviour
{
    public AudioMobileModel data;
    public string localWWW;
    public GameObject vizObject;
    public float[] samples;
    public float max;
    public Color errorCol;
    public Color loadingCol;
    public Color playingCol;
    public Color readyCol;
    public GUISkin nodeSkin;
    public float interactDist = 2f;
    public float currDist;
    public bool inRange = false;
    public bool dispGUI = false;
    public bool isPlaying;
    public float loopValLeft;
    public float loopValRight;
    public bool tempInfo = false;
	public string fileName;
    public Progress progress;

    //temp behaviour to load a random user image
    public Texture2D[] userPics;
    public Texture gradient;
    public Texture2D currPic;

    private int leftClip;
    private int rightClip;
    private WWW www;
    private GameObject player;
    private Vector3 orgScale;
    private List<GameObject> allVizObjects;
    private WindowManager winManager;
    private int vizCount = 8;
    private float adjust = 1f;
    private Color guiColor;
    private FXGui fxGUI;


    // Use this for initialization
    void Start()
    {

		progress = Progress.None;
        samples = new float[vizCount];
        orgScale = this.transform.localScale;
        var guiManager = GameObject.FindGameObjectWithTag("GUIManager");
        if (guiManager != null)
            winManager = guiManager.GetComponent<WindowManager>();
        currPic = userPics[UnityEngine.Random.Range(0, userPics.Length)];

        //vizObject = this.transform.GetChild(0).gameObject;
        loopValLeft = 0f;
        loopValRight = (float)Screen.width;

        StartCoroutine(GrabSamples());
        StartCoroutine(VizSpawn());

        if (tempInfo)
        {
            data.Author = "Gandalf Greyheim";
            data.Date = "2013/09/24";
            data.Location = "75.62356, 45.56456";
            //data.SoundURL = "http://www.audio-mobile.org";
        }

        this.name = data.Author + " :" + data.Date;
        guiColor = Color.clear;
        fxGUI = this.GetComponent<FXGui>();
		
		Debug.Log ("Node: " + data.Author+ ": " + this.transform.position + ": " + data.Position);
    }


    // Update is called once per frame
    void Update()
    {

		if (data.SoundURL != null)
            this.light.color = 0.75f * this.renderer.material.color;

        if (player == null)
            player = GameObject.FindGameObjectWithTag("Player");

        this.transform.localScale = Vector3.Lerp(this.transform.localScale, adjust * orgScale, 0.4f);

        if (player != null)
        {
            currDist = Vector3.Distance(player.transform.position, this.transform.position);

            if (currDist <= interactDist)
            {
                inRange = true;
            }
            else
                inRange = false;
        }

        if (isPlaying && audio.clip != null && !audio.isPlaying)
        {
            audio.Play();
        }

        //loop behaviour
        if (audio.clip != null)
        {
            leftClip = Mathf.RoundToInt((loopValLeft / Screen.width) * audio.clip.samples);
            rightClip = Mathf.RoundToInt((loopValRight / Screen.width) * audio.clip.samples);
        }

        if (audio.isPlaying)
        {
            if (audio.timeSamples > rightClip)
                audio.timeSamples = leftClip;
            else if (audio.timeSamples < leftClip)
                audio.timeSamples = leftClip;
        }

    }

	public string ReturnAudioData()
	{
		return this.fileName;
	}

    void OnGUI()
    {
        GUI.skin = nodeSkin;
        GUI.color = guiColor;

        //if(!dispGUI)
        //	return;

        if (GUI.color == Color.clear)
            return;

        var contentRect = new Rect(0, 0, Screen.width, 250);
        var contentRect1 = new Rect(0, 0, Screen.width, winManager.yLevel);
        var offset = new RectOffset(0, 0, 0, 0);
        contentRect = offset.Remove(contentRect);
        contentRect1 = offset.Remove(contentRect1);
        var closeRect = new Rect(contentRect.x + contentRect.width - 24, contentRect.y, 24, 24);

        //GUI.color = 0.85f * Color.white;

        GUI.DrawTexture(contentRect1, currPic, ScaleMode.ScaleAndCrop);
        GUI.DrawTexture(contentRect1, gradient, ScaleMode.StretchToFill);

        GUILayout.BeginArea(contentRect);

        GUILayout.Label(data.Author);
        GUILayout.Label(data.Location);
        GUILayout.Label(data.SoundURL, nodeSkin.customStyles[0]);
        GUILayout.Space(20);
        GUILayout.BeginHorizontal();

        if (GUILayout.Button("", nodeSkin.customStyles[5]))
        {
            RayHit();
        }

        GUILayout.Space(15);

        if (GUILayout.Button("", nodeSkin.customStyles[7]))
        {
        }
        GUILayout.Space(15);

        if (this.tag == "UserSounds")
        {
            if (GUILayout.Button("", nodeSkin.customStyles[6]))
            {
                winManager.forceFreeze = false;
                Destroy(this.gameObject);
                return;
            }
        }


        GUILayout.Space(20);
        GUILayout.BeginVertical();

        GUILayout.BeginHorizontal();
        audio.volume = GUILayout.HorizontalSlider(audio.volume, 1f, 0f);
        GUILayout.Label("Vol: " + audio.volume, nodeSkin.customStyles[9]);
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        audio.pitch = GUILayout.HorizontalSlider(audio.pitch, 0f, 3f);
        GUILayout.Label("Pitch: " + audio.pitch, nodeSkin.customStyles[9]);
        GUILayout.EndHorizontal();

        GUILayout.Space(10);

        GUILayout.BeginHorizontal();
        audio.maxDistance = GUILayout.HorizontalSlider(audio.maxDistance, 1f, 100f);
        GUILayout.Label("Effect Radius: " + audio.maxDistance, nodeSkin.customStyles[9]);
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();

        GUILayout.EndArea();

        if (GUI.Button(closeRect, "", nodeSkin.customStyles[1]))
        {
            //dispGUI = !dispGUI;
            StartCoroutine(LerpGUIAlpha(Color.clear));
            winManager.forceFreeze = !winManager.forceFreeze;
        }

        //Loop Bar
        var w = 50;
        var barRect = new Rect(contentRect.x + loopValLeft, contentRect.y + contentRect.height + 20, loopValRight - loopValLeft, 100);
        var trackRect = new Rect(contentRect.x, barRect.y, contentRect.width, 100);
        var lHandle = new Rect(barRect.x - w, barRect.y + barRect.height - w / 3, 2 * w, w / 2);
        var rHandle = new Rect(barRect.x + barRect.width - w, lHandle.y, 2 * w, w / 2);

        if (GUI.RepeatButton(lHandle, "", nodeSkin.customStyles[2]))
        {
            loopValLeft = Input.mousePosition.x;
            loopValLeft = Mathf.Max(loopValLeft, 0f);
            loopValLeft = Mathf.Min(loopValRight - 10f, loopValLeft);
        }
        if (GUI.RepeatButton(rHandle, "", nodeSkin.customStyles[2]))
        {
            loopValRight = Input.mousePosition.x;
            loopValRight = Mathf.Min(loopValRight, Screen.width);
            loopValRight = Mathf.Max(loopValLeft + 10, loopValRight);
        }

        if (audio.clip != null)
        {
            var prog = ((float)(audio.timeSamples - leftClip) / audio.clip.samples) * Screen.width;
            var progRect = new Rect(loopValLeft, trackRect.y, prog, trackRect.height);

            GUI.Box(trackRect, "", nodeSkin.customStyles[4]);


            if (audio.isPlaying)
                GUI.Box(progRect, "", nodeSkin.customStyles[8]);
        }

        GUI.Box(barRect, "", nodeSkin.customStyles[3]);

        //FXGUI
        if(winManager.yLevel/Screen.height > 0.8f)
            fxGUI.Display();

    }

    void SetLoop()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {

        }

        //set end of loop and play
        if (Input.GetKeyUp(KeyCode.L))
        {

        }
    }
	

    IEnumerator LerpGUIAlpha(Color target)
    {

        int i = 0;
        var steps = 20;
        var orgColor = guiColor;

        while (i <= steps)
        {
            guiColor = Color.Lerp(orgColor, target, i * (1f / steps));
            i++;
            yield return 0;
        }

        yield return 0;
    }

    IEnumerator GrabSamples()
    {
        while (true)
        {
            if (this.audio.isPlaying)
            {
                float[] audioSamplesL = audio.GetSpectrumData(1024, 0, FFTWindow.BlackmanHarris);
                float[] audioSamplesR = audio.GetSpectrumData(1024, 1, FFTWindow.BlackmanHarris);
                int i = 1;
                max = 0f;
                while (i < vizCount)
                {
                    samples[i] = audioSamplesL[(int)Mathf.Pow(2, i + 1)] + audioSamplesR[(int)Mathf.Pow(2, i)];

                    if (samples[i] > max)
                        max = samples[i];

                    i++;
                }

                adjust = 0.5f + 10f * max;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator VizSpawn()
    {
        while (true)
        {

            if (this.audio.isPlaying)
            {

                var tmpObj = Instantiate(vizObject, this.transform.position, Quaternion.identity) as GameObject;

                //tmpObj.transform.parent = this.transform;

                Destroy(tmpObj, 2f);
            }

            yield return new WaitForSeconds(0.5f);
        }
    }

    IEnumerator BeginLoading()
    {
        progress = Progress.Loading;

        www = new WWW(data.SoundURL);
        
        Debug.Log("Downloading sound: " + data.SoundURL);
	
        while (!www.isDone)
        {

            //temp download visual
            this.renderer.material.color = loadingCol;
            yield return null;
        }

        Debug.Log("Download complete: " + data.SoundURL);

        //this.AddComponent<AudioSource>();
        if (www.error == null)
        {
            this.audio.clip = www.audioClip;
            this.audio.loop = true;
            this.audio.rolloffMode = AudioRolloffMode.Linear;
            this.audio.minDistance = 0.001f;
            //scale dependant
            this.audio.maxDistance = 5f;

            //temp playing visual
            this.renderer.material.color = readyCol;
        }
        else
        {
            Debug.Log("error in stream: " + www.url);

            //temp error visual
            this.renderer.material.color = errorCol;
        }

        // update transform
        this.transform.position = data.Position;

        //autoplay
        this.audio.Play();
        this.renderer.material.color = playingCol;

        progress = Progress.Complete;

        if (data.Image != null)
            StartCoroutine(BeginLoadingImage());

    }

    public void Load()
    {
        StartCoroutine(BeginLoading());
    }

    IEnumerator BeginLoadingImage()
    {
        progress = Progress.Loading;
        www = new WWW(data.Image);

        Debug.Log("Downloading Image: " + data.Image);

        while (!www.isDone)
        {

            //temp download visual
            this.renderer.material.color = loadingCol;
            yield return null;
        }

        Debug.Log("Download complete: " + data.Image);

        //this.AddComponent<AudioSource>();
        if (www.error == null)
        {
            www.LoadImageIntoTexture(this.currPic);

            //temp playing visual
            this.renderer.material.color = readyCol;
        }
        else
        {
            Debug.Log("error in image stream");
        }

        progress = Progress.Complete;

        //initialize audio effect paramters
        //var audioLoader = GameObject.FindGameObjectWithTag("UserSounds").GetComponent<LoadAudioFile>();
        //audioLoader.LoadEffectParamters(null);

    }

    void RayHit()
    {

        if (audio.isPlaying)
        {
            this.audio.Stop();
            this.renderer.material.color = readyCol;
            adjust = 1f;
            isPlaying = false;
        }

        else
        {
            this.audio.Play();
            isPlaying = true;
            if (this.audio.isPlaying)
                this.renderer.material.color = playingCol;
        }
    }

    void OnMouseDown()
    {
        
        if (progress == Progress.None && Vector3.Distance(player.transform.position, this.data.Position) < 10f)
        {
            StartCoroutine(BeginLoading());
            return;
        }


        if (!inRange)
            RayHit();
        else
        {
            if (winManager.userState != UserState.freeze)
            {
                //dispGUI = !dispGUI;
                StartCoroutine(LerpGUIAlpha(0.8f * Color.white));
                winManager.forceFreeze = !winManager.forceFreeze;
            }
        }

    }


}
