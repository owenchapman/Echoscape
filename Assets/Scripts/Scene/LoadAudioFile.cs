using UnityEngine;
using System.Collections;
using System.IO;

public class LoadAudioFile : MonoBehaviour
{

    public string fileName;
    public string defaultDir;
	public GameObject nodePrefab;
	public GameObject player;
	public Recorder recorder;

	public WindowManager winManager;

    private string path;
    public AudioClip clip;

	private bool loadFile;

	string message = "";
	float alpha = 1.0f;
	char pathChar = '/';
    

    // Use this for initialization
    void Start()
    {
		if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer) {
			pathChar = '\\';
		}

		loadFile = false;
		winManager = GameObject.FindWithTag("GUIManager").GetComponent<WindowManager>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LoadFile()
    {
//        path = Path.Combine(defaultDir, fileName);
//
//        if (!File.Exists(path))
//            return;
//
//        var clipWWW = new WWW(path);
//
//        StartCoroutine(LoadWWW(clipWWW));
		winManager.forceFreeze = true;
		winManager.userState = UserState.freeze;
		UniFileBrowser.use.SetPath(recorder.defaultDir);
		UniFileBrowser.use.OpenFileWindow (OpenFile);
		UniFileBrowser.use.SendWindowCloseMessage(CloseMessage);

    }

    IEnumerator LoadWWW(WWW url)
    {
        while (!url.isDone)
        {
            yield return 0;
        }

        CreateNewAudioSource(url.GetAudioClip(true, false, AudioType.WAV));

		yield return 0;
    }

	void CloseMessage()
	{
		winManager.forceFreeze = false;
	}

    private void CreateNewAudioSource(AudioClip clip)
    {
		var tmp = Instantiate(nodePrefab, this.transform.position, Quaternion.identity) as GameObject;
		tmp.tag = "UserSounds";
		tmp.transform.position = player.transform.position;
        tmp.transform.parent = this.transform;
        var source = tmp.GetComponent<AudioSource>();

        source.clip = clip;

        source.Play();
        source.loop = true;
    }

	void OnGUI () {

		//GUI.Label (new Rect(100, 275, 500, 1000), message);

	}

	void OpenFile (string pathToFile) {

		winManager.forceFreeze = false;

		var fileIndex = pathToFile.LastIndexOf (pathChar);
		message = "You selected file: " + pathToFile.Substring (fileIndex+1, pathToFile.Length-fileIndex-1);

		Debug.Log(pathToFile);
		var clipWWW = new WWW("file://" + pathToFile);

        StartCoroutine(LoadWWW(clipWWW));

	}

	void OpenFiles (string[] pathsToFiles) {
		message = "You selected these files:\n";
		for (var i = 0; i < pathsToFiles.Length; i++) {
			var fileIndex = pathsToFiles[i].LastIndexOf (pathChar);
			message += pathsToFiles[i].Substring (fileIndex+1, pathsToFiles[i].Length-fileIndex-1) + "\n";
		}

	}

}
