using UnityEngine;
using System.Collections;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

public class LoadTest : MonoBehaviour
{

    private GameObject go;
	private string defaultDir;
	private string path;

    // Use this for initialization
    void Start()
    {
		defaultDir = Path.Combine(Application.dataPath, "UserRecordings");
		
		if (Application.platform == RuntimePlatform.OSXEditor)
			defaultDir = "/Users/dan/Documents/UserRecordings";
		
		if (Application.platform == RuntimePlatform.WindowsEditor)
			defaultDir = @"C:\Users\Home\AppData\Roaming\Echoscape\";
		
		if (!Directory.Exists(defaultDir))
			Directory.CreateDirectory(defaultDir);
		
		path = Path.Combine(defaultDir, "testScene.data");
    }

    // Update is called once per frame
    void Update()
    {

    }

	void OnGUI()
	{
		GUILayout.Space(150);
		if(GUILayout.Button("Save"))
			Serialize();
		if(GUILayout.Button("Load"))
			DeSerialize();
	}

	void Serialize()
	{
		DownloadedData obj = GameObject.FindGameObjectWithTag("EchoscapeTile").GetComponent<EchoscapesLoader>().dlData;
		IFormatter formatter = new BinaryFormatter();
		Stream stream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
		formatter.Serialize(stream, obj);
		stream.Close();

		
		var debug = Path.Combine (defaultDir, "debug.xml");
		File.WriteAllText(debug, obj.audio.ToString());

	}

	void DeSerialize()
	{
		Destroy(GameObject.FindGameObjectWithTag("EchoscapeTile"));

		IFormatter formatter = new BinaryFormatter();
		Stream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
		DownloadedData obj = (DownloadedData) formatter.Deserialize(stream);
		stream.Close();


		// create new loader parented to EchoscapesManager
		var loader = new GameObject("user tile");
		loader.tag = "EchoscapeTile";
		
		// set up loading parameters
		var request = loader.AddComponent<EchoscapesLoader>();
		request.map = GameObject.Find("[Map]").GetComponent<Map>();
		
		request.BuildFromFile(obj);


	}

    void AddComponentRecursive(Transform parent, string typeName)
    {
        var success = false;
        if (typeName == "StoreMesh" && parent.gameObject.GetComponent<MeshFilter>() != null)
        {
            parent.gameObject.AddComponent(typeName);
            success = true;
        }
        if (typeName == "StoreMaterials" && parent.gameObject.GetComponent<MeshRenderer>() != null)
        {
            parent.gameObject.AddComponent(typeName);
            success = true;
        }
        if (!success)
            parent.gameObject.AddComponent(typeName);
        foreach (Transform child in parent)
        {
            AddComponentRecursive(child, typeName);
        }
    }
}
