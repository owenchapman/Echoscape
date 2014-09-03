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

	}

	void PrintCoords()
	{
		var map = GameObject.Find ("[Map]").GetComponent<Map>();

		var coords = Util.Mercator2LonLat2(this.transform.position, map, Global.mapCenter);

		Debug.Log(coords);
	}

	void Update()
	{
		if(Input.GetKeyDown (KeyCode.C))
			PrintCoords();
	}
}
