using UnityEngine;
using System.Collections;
using System.IO;
using System.Xml;
using System;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

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
        if (Application.platform == RuntimePlatform.WindowsEditor || Application.platform == RuntimePlatform.WindowsPlayer)
        {
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

        winManager.forceFreeze = true;
        winManager.userState = UserState.freeze;
        UniFileBrowser.use.SetPath(recorder.defaultDir);
        UniFileBrowser.use.OpenFileWindow(OpenFile);
        UniFileBrowser.use.SendWindowCloseMessage(CloseMessage);

    }

	public void SaveFile()
	{
		winManager.forceFreeze = true;
		winManager.userState = UserState.freeze;
		UniFileBrowser.use.SetPath(recorder.defaultDir);
		UniFileBrowser.use.SaveFileWindow(SaveFileAction);
		UniFileBrowser.use.SendWindowCloseMessage(CloseMessage);
	}

	void SaveFileAction(string pathToFile)
	{
		Serialize (pathToFile);
		winManager.forceFreeze = false;
	}

	void Serialize(string path)
	{
		DownloadedData obj = GameObject.FindGameObjectWithTag("EchoscapeTile").GetComponent<EchoscapesLoader>().dlData;
		IFormatter formatter = new BinaryFormatter();
		Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
		formatter.Serialize(stream, obj);
		stream.Close();
		
		
		var debug = Path.Combine(defaultDir, "debug.xml");
		File.WriteAllText(debug, obj.audio.ToString());
		
	}

	void DeSerialize(string path)
	{
		Destroy(GameObject.FindGameObjectWithTag("EchoscapeTile"));
		
		IFormatter formatter = new BinaryFormatter();
		Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
		DownloadedData obj = (DownloadedData)formatter.Deserialize(stream);
		stream.Close();
		
		// create new loader parented to EchoscapesManager
		var loader = new GameObject(Path.GetFileNameWithoutExtension(path));
		loader.tag = "EchoscapeTile";
		
		// set up loading parameters
		var request = loader.AddComponent<EchoscapesLoader>();
		request.map = GameObject.Find("[Map]").GetComponent<Map>();
		
		request.BuildFromFile(obj);
	}

    IEnumerator LoadWWW(WWW url, string path)
    {
        if(!File.Exists(path))
			yield return 0;

		while (!url.isDone)
        {
            yield return 0;
        }

		var extension = Path.GetExtension(path);
		
		if(extension == ".scape")
		{
			DeSerialize(path);
		}
		else if(extension ==  ".wav" || extension == ".ogg")
		{
        	CreateNewAudioSource(url);
		}

        yield return 0;
    }

    void CloseMessage()
    {
        winManager.forceFreeze = false;
    }

    private void CreateNewAudioSource(WWW url)
    {
        var tmp = Instantiate(nodePrefab, this.transform.position, Quaternion.identity) as GameObject;
        tmp.tag = "UserSounds";
        tmp.transform.position = player.transform.position;
        tmp.transform.parent = GameObject.FindWithTag("NodeParentObj").transform;

        //var source = tmp.GetComponent<AudioSource>();
        var rNode = tmp.GetComponent<RecordingNode>();
        rNode.data = new AudioMobileModel();
        rNode.data.SoundURL = url.url;
		var map = GameObject.Find("[Map]").GetComponent<Map>();
		var coords = Util.Mercator2LonLat2(tmp.transform.position, map, Global.mapCenter);
		rNode.data.Latitude = coords.z;
		rNode.data.Longitude = coords.x;
        rNode.data.Position = tmp.transform.position;
        //Debug.Log ("The audio url is: " + data.SoundURL);


        //append node to Node XML
		var loader = GameObject.FindGameObjectWithTag("EchoscapeTile").GetComponent<EchoscapesLoader>();
        if (loader != null)
        {
            var xmlData = loader.dlData.audio;

            if (xmlData != null)
            {
                var doc = new XmlDocument();
                doc.LoadXml(xmlData);

                var audioNode = doc.GetElementsByTagName("audio");
                
                if (audioNode != null)
                {
                    var newNode = doc.CreateElement("marker");
                    newNode.SetAttribute("user_id", "1001");
					newNode.SetAttribute("author", Global.userName);
                    newNode.SetAttribute("latitude", coords.z.ToString());
                    newNode.SetAttribute("longitude", coords.x.ToString());
                    newNode.SetAttribute("location", Global.location);
                    newNode.SetAttribute("date", DateTime.Today.ToString());
                    newNode.SetAttribute("time", DateTime.Now.TimeOfDay.ToString());
                    newNode.SetAttribute("weather", "");
                    newNode.SetAttribute("info1", "");
                    newNode.SetAttribute("info2", "");
                    newNode.SetAttribute("soundfile", url.url);
                    newNode.SetAttribute("imagefile", "");

                    audioNode[0].AppendChild(newNode);
                }

                using (var stringWriter = new StringWriter())
                using (var xmlTextWriter = XmlWriter.Create(stringWriter))
                {
                    doc.WriteTo(xmlTextWriter);
                    xmlTextWriter.Flush();
                    loader.dlData.audio = stringWriter.GetStringBuilder().ToString();
                }

            }
        }

    }

    void OnGUI()
    {

        //GUI.Label (new Rect(100, 275, 500, 1000), message);

    }

    void OpenFile(string pathToFile)
    {

        winManager.forceFreeze = false;

        var fileIndex = pathToFile.LastIndexOf(pathChar);
        message = "You selected file: " + pathToFile.Substring(fileIndex + 1, pathToFile.Length - fileIndex - 1);

        Debug.Log(pathToFile);
        var clipWWW = new WWW("file://" + pathToFile);

        StartCoroutine(LoadWWW(clipWWW, pathToFile));

    }

    void OpenFiles(string[] pathsToFiles)
    {
        message = "You selected these files:\n";
        for (var i = 0; i < pathsToFiles.Length; i++)
        {
            var fileIndex = pathsToFiles[i].LastIndexOf(pathChar);
            message += pathsToFiles[i].Substring(fileIndex + 1, pathsToFiles[i].Length - fileIndex - 1) + "\n";
        }

    }

}
