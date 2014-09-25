//CONFIDENTIAL and COPYRIGHT 2014
//DO NOT DISTRIBUTE
//Daniel Hambleton 2014

//#define OFFLINE

using System;
using System.Xml;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

[Serializable]
public struct LatLonAlt
{
    public float Lat { get; set; }
    public float Lon { get; set; }
    public float Alt { get; set; }
}
[Serializable]
public class DataUrls
{
    public string coreUrl;
    public string dataFolder;
    public string terrainData;
    public string wayData;
}
[Serializable]
public class DownloadedData
{
    public string audio;
    public string terrain;
    public byte[] ways; // binary data
}
[Serializable]
public class ConstructedData
{
    public GameObject recNodes;
    public Vector3 averagePoint;
    public LatLonAlt[,] heightMap;
    public List<MapWayModel> ways;
    public GameObject terrain;
}

public class RequestArea
{
    public float lat;
    public float lon;
    public float tile;
}

public enum CompletetionState
{
    Starting,
    Downloading,
    Downloaded,
    Creating,
    Waiting, // audio and ways must wait for terrain for snapping-down, terrain must wait for averagePoint (audio)
    Done
}

public class EchoscapesLoader : MonoBehaviour
{
    public bool init = false;

    public CompletetionState audioState;
    public CompletetionState terrainState;
	public CompletetionState wayState;

    public DataUrls urls = new DataUrls();
    public DownloadedData dlData = new DownloadedData();
    public ConstructedData conData = new ConstructedData();
    public RequestArea requestArea = null;

    private float pScale;
    private Vector3 translate;
    public Map map;

    void Awake()
    {

        //var world = GameObject.FindGameObjectWithTag("World");
        //var focus = world.GetComponent<WorldFocus>();

        //translate = world.transform.position;
        //pScale = focus.pScale;

        translate = Vector3.zero;
        pScale = 50000f;

    }

    void Start()
    {
		Global.mapCenter = new double[2]{map.CenterWGS84[0], map.CenterWGS84[1]};

		audioState = CompletetionState.Starting;
        terrainState = CompletetionState.Starting;
        wayState = CompletetionState.Starting;

#if OFFLINE
        urls.coreUrl = "file://" + Application.dataPath + "/DefaultData/request.xml";
        urls.dataFolder = "file://" + Application.dataPath + "/DefaultData/";
#else
        //urls.coreUrl = "http://audio-mobile.org/echoscapes/echobase_v1.php?lat={0}&lon={1}";
        //single tile php call
        //urls.coreUrl = "http://audio-mobile.org/echoscapes/echobase_singletile.php?lat={0}&lon={1}";
        urls.coreUrl = "http://audio-mobile.org/echoscapes/echobase_singletile_REALDB.php?lat={0}&lon={1}";
        //urls.coreUrl = "http://audio-mobile.org/echoscapes/echobase_singletile_random.php?lat={0}&lon={1}";
        urls.dataFolder = "http://audio-mobile.org/echoscapes/data/";
#endif
    }

	IEnumerator BuildFromFileLoop()
	{
		yield return new WaitForSeconds(1f);
		this.terrainState = CompletetionState.Downloaded;
		this.audioState = CompletetionState.Downloaded;
		this.wayState = CompletetionState.Downloaded;
		bool success = false;
		while(!success)
		{
			// if we've done everything exit
			if (init && (audioState == CompletetionState.Done && terrainState == CompletetionState.Done &&
			             wayState == CompletetionState.Done))
			{
				success = true;
				yield return 0;
			}
			
			
			if (terrainState == CompletetionState.Downloaded)
			{
				Debug.Log("begin terrain coroutine");
				ConstructTerrain();
			}
			
			if (terrainState == CompletetionState.Done && audioState == CompletetionState.Downloaded)
			{
				Debug.Log("begin audio coroutine");
				// construct recording nodes (require to be snapped to terrain later)
				ConstructAudio(); // audio will be 'Waiting' when done				
			}
			
			
			if (terrainState == CompletetionState.Done && wayState == CompletetionState.Downloaded)
			{
				Debug.Log("begin ways coroutine");
				// ways may be constructed as soon as terrain is finished
				ConstructWays();
				
				var newPlayer = GameObject.FindGameObjectWithTag("Player");
				
				//construct a player object if one does not exist in the scene
				if (newPlayer == null)
				{
					newPlayer = Instantiate(Resources.Load("Prefab/Player") as GameObject) as GameObject;
					newPlayer.transform.localScale *= 1f;
				}
				
				var pos = conData.terrain.collider.bounds.center;
				pos.y = Util.GetTerrainHeightAt(pos) + 0.01f;
				newPlayer.transform.position = pos;	

			}
			
			yield return 0;
		}
	}

	IEnumerator BuildFromServerLoop()
	{
		yield return new WaitForSeconds(1f);

		bool success = false;
		while(!success)
		{
			// if we've done everything exit
			if (init && (audioState == CompletetionState.Done && terrainState == CompletetionState.Done &&
			             wayState == CompletetionState.Done))
			{
				success = true;
				yield return 0;
			}
			
			// otherwise check if urls/request area are set so we may begin
			if (!init && requestArea != null &&
			    (audioState == CompletetionState.Starting &&
			 terrainState == CompletetionState.Starting &&
			 wayState == CompletetionState.Starting))
			{
				// begin downloads
				BeginRequestArea();
				init = true;
			}
			
			if (!init)
			{
				yield return 0;
			}
			
			
			if (terrainState == CompletetionState.Downloaded)
			{
				Debug.Log("begin terrain coroutine");
				ConstructTerrain();
			}
			
			if (terrainState == CompletetionState.Done && audioState == CompletetionState.Downloaded)
			{
				Debug.Log("begin audio coroutine");
				// construct recording nodes (require to be snapped to terrain later)
				ConstructAudio(); // audio will be 'Waiting' when done
				
			}
			
			
			if (terrainState == CompletetionState.Done && wayState == CompletetionState.Downloaded)
			{
				Debug.Log("begin ways coroutine");
				// ways may be constructed as soon as terrain is finished
				ConstructWays();
				
				var newPlayer = GameObject.FindGameObjectWithTag("Player");
				
				//construct a player object if one does not exist in the scene
				if (newPlayer == null)
				{
					newPlayer = Instantiate(Resources.Load("Prefab/Player") as GameObject) as GameObject;
					newPlayer.transform.localScale *= 1f;
				}
				
				var pos = conData.terrain.collider.bounds.center;
				pos.y = Util.GetTerrainHeightAt(pos) + 0.01f;
				newPlayer.transform.position = pos;
				
			}

			yield return 0;
		}
	}

	public void BuildFromServer()
	{
        try
        {
            GameObject.FindGameObjectWithTag("GUIManager").GetComponent<WindowManager>().GUIAlphaLerp(0.8f * Color.white);
        }
        catch { }
        StartCoroutine(BuildFromServerLoop());
	}

	public void BuildFromFile(DownloadedData data)
	{
		this.dlData = data;
        try
        {
            GameObject.FindGameObjectWithTag("GUIManager").GetComponent<WindowManager>().GUIAlphaLerp(0.8f * Color.white);
        }
        catch { }
        StartCoroutine(BuildFromFileLoop());
	}


    void BeginRequestArea()
    {
        var lat = requestArea.lat;
        var lon = requestArea.lon;

        var request = string.Format(urls.coreUrl, lat, lon);




        Debug.Log("Request area: " + request);
        StartCoroutine(Util.GetUrlContents(request, CompleteRequestArea));

        audioState = CompletetionState.Downloading;
    }

    void CompleteRequestArea(string xmlData)
    {
        var doc = new XmlDocument();
        doc.LoadXml(xmlData);
        Debug.Log("xml: " + xmlData);

        var streetTiles = doc.GetElementsByTagName("street");
        var terrainTiles = doc.GetElementsByTagName("terrain");

        foreach (XmlNode node in streetTiles[0])
        {
            var id = int.Parse(node.Attributes["id"].Value);
            if (id == requestArea.tile)
                urls.wayData = urls.dataFolder + "o5m/" + node.Attributes["name"].Value;
        }

        foreach (XmlNode node in terrainTiles[0])
        {
            var id = int.Parse(node.Attributes["id"].Value);
            if (id == requestArea.tile)
                urls.terrainData = urls.dataFolder + "asc90m/" + node.Attributes["name"].Value;
        }

        // audio data is included in the original lat/lon request
        dlData.audio = xmlData;
        audioState = CompletetionState.Downloaded;

        BeginDownloads();
    }

    void BeginDownloads()
    {
        Debug.Log("beginning downloads...");

        StartCoroutine(Util.GetUrlContents(urls.terrainData, TerrainDownloadComplete));
        StartCoroutine(Util.GetUrlBytes(urls.wayData, WaysDownloadComplete));

        terrainState = CompletetionState.Downloading;
        wayState = CompletetionState.Downloading;
    }

    #region Downloading finished callbacks

    void ConstructAudio()
    {
        audioState = CompletetionState.Creating;
        Debug.Log("constructing audio nodes");
        var builder = new AudioBuilder();
        //MUST set scale before proceeding
        //builder.SetScale(pScale, translate);
        builder.ConstructAudio(this.gameObject, map, urls, dlData, conData, AudioComplete);
    }

    void ConstructTerrain()
    {
        terrainState = CompletetionState.Creating;
        Debug.Log("constructing terrain");
        var builder = new TerrainBuilder();
        //MUST set scale before proceeding
        //builder.SetScale(pScale, translate);
        builder.ConstructTerrain(this.gameObject, map, dlData, conData, TerrainComplete);
    }

    void ConstructWays()
    {
        wayState = CompletetionState.Creating;
        Debug.Log("constructing ways");
        var builder = new WaysBuilder();
        //MUST set scale before proceeding
        //builder.SetScale(pScale, translate);
        Debug.Log("Way data size: " + dlData.ways.Length);


        builder.ConstructWays(this.gameObject, map, dlData, conData, WaysComplete);
    }

    #endregion

    #region Construction done callbacks

    void AudioDownloadComplete(string xmlData)
    {
        Debug.Log("downloaded audio data");
        dlData.audio = xmlData;
        audioState = CompletetionState.Downloaded;
    }

    void TerrainDownloadComplete(string xmlData)
    {
        Debug.Log("downloaded terrain data package");
        dlData.terrain = xmlData;
        terrainState = CompletetionState.Downloaded;
    }

    void WaysDownloadComplete(byte[] o5mData)
    {
        Debug.Log("downloaded way data package");
        dlData.ways = o5mData;

        wayState = CompletetionState.Downloaded;
    }

    void AudioComplete()
    {
        // audio goes into waiting after initial construction
        // it needs to be constructed before terrain but there are post-steps needed
        // (rec nodes get snapped to terrain once it's built)
        Debug.Log("audio data complete, waiting for terrain");
        audioState = CompletetionState.Waiting;
    }

    void TerrainComplete()
    {
        Debug.Log("terrain constructed, snapping audio nodes");
        terrainState = CompletetionState.Done;

        // FIXME: re-enable audio snapping when recording nodes are re-enabled
        //AudioBuilder.SnapAudioNodesToTerrain(conData, pScale); // finally, snap audio to terrain (async?)

    }

    void WaysComplete()
    {
        Debug.Log("ways created");
        wayState = CompletetionState.Done;

        //hack
        GameObject.FindGameObjectWithTag("NodeManager").GetComponent<NodeFilter>().ResetToCompleteList();
        GameObject.FindGameObjectWithTag("GUIManager").GetComponent<WindowManager>().GUIAlphaLerp(Color.clear);
        //play the title track
        try
        {
            GameObject.FindGameObjectWithTag("SFX").audio.Play();
        }
        catch { }

        //        var go = GameObject.Instantiate(Resources.Load("Prefabs/HexTerrain")) as GameObject;
        //        var hex = go.GetComponent<Hexes2>();
        //        hex.terrain = conData.terrain;
    }

    #endregion

}
