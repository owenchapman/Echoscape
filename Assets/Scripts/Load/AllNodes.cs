using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Xml;



public class AllNodes : MonoBehaviour {


    private string url;
    public RequestArea requestArea = null;
    private GameObject allNodes;
    private List<GameObject> markerNodes;
    private Map map;
    private bool loaded = false;
    // Use this for initialization
	void Start () {

        url = "http://audio-mobile.org/echoscapes/echobase_allNodeLocations_REALDB.php";
        //url = "http://audio-mobile.org/echoscapes/echobase_allNodeLocations.php";
        
        allNodes = new GameObject("All Audio Nodes");
        allNodes.transform.parent = this.transform;
        markerNodes = new List<GameObject>();
        //world = GameObject.FindGameObjectWithTag("World");
       
        

	}
	
	// Update is called once per frame
	void Update () {

        if(map == null)
        {
            map = GameObject.Find("[Map]").GetComponent<Map>();
        }

        if(map != null && !loaded)
        {
            Debug.Log("About to load map nodes");
            BeginRequestArea();
            loaded = true;
        }

        foreach (var node in markerNodes)
        {
            //node.transform.localScale = (20f/world.transform.localScale.x) * Vector3.one;
        }


        
	}

    void BeginRequestArea()
    {
 
        StartCoroutine(Util.GetUrlContents(url, CompleteRequestArea));

    }

    void CompleteRequestArea(string xmlData)
    {
        var doc = new XmlDocument();
        doc.LoadXml(xmlData);

        var nodes = doc.GetElementsByTagName("marker");
        var prefab = Resources.Load("Prefab/AudioMarker") as GameObject;
        Debug.Log("Total Map nodes: " + nodes.Count);
        foreach (XmlNode node in nodes)
        {
            var lat = float.Parse(node.Attributes["latitude"].Value);
            var lon = float.Parse(node.Attributes["longitude"].Value);

            var coords = new Vector3(lat, 0f, lon);
            var pos = Util.LonLat2Mercator2(coords, map);

            //jump through instantiation hoops
            var tmp = Instantiate(prefab, pos, Quaternion.identity) as GameObject;
            tmp.transform.forward = Vector3.up;
            tmp.transform.position += 0.1f * Vector3.up;
            tmp.transform.parent = allNodes.transform;
            tmp.transform.localScale *= 0.1f;
            tmp.tag = "LocationNode";
            tmp.layer = 12;

            map.CreateMarker<Marker>("AudioNode", new double[]{lon, lat}, tmp);

            markerNodes.Add(tmp);
            //tmp.transform.localScale = this.transform.localScale;

        }

    }
}
