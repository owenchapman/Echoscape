using UnityEngine;
using System;
using UnitySlippyMap;
using ProjNet.CoordinateSystems;
using ProjNet.CoordinateSystems.Transformations;
using ProjNet.Converters.WellKnownText;
using System.IO;
using System.Collections;
using System.Collections.Generic;

public class SlippyMap : MonoBehaviour {
	
	private Map		map;
	
	public Texture	LocationTexture;
	public Texture	MarkerTexture;
	
	private float	guiXScale;
	private float	guiYScale;
	private Rect	guiRect;
	
	private bool 	isPerspectiveView = false;
	private float	perspectiveAngle = 30.0f;
	private float	destinationAngle = 0.0f;
	private float	currentAngle = 0.0f;
	private float	animationDuration = 0.5f;
	private float	animationStartTime = 0.0f;
	
    private List<Layer> layers;
    private int     currentLayerIndex = 0;
	
	public double scale;

    public double mapCenterLat;
    public double mapCenterLon;
	
	private GameObject lastCollider;

	public bool freeze = false;

	private GameObject mapObj;

	public float mScale = 1f;
	public Texture crosshair;
	private WindowManager winManager;


	// Use this for initialization
	void Start () {
		

		Debug.Log("About to Initialize SlippyMap");
		// create the map singleton
		map = Map.Instance;
		map.InputDelegate += UnitySlippyMap.Input.MapInput.BasicTouchAndKeyboard;
		winManager = GameObject.FindWithTag("GUIManager").GetComponent<WindowManager>();

		//var title = GameObject.Find("Title");
		//var coords = title.GetComponent<TitleLoad>().coords;
		// start point - Montreal
		map.CenterWGS84 = new double[2] { -73.575044, 45.521430 };
        //Buenos Aires
        //map.CenterWGS84 = new double[2] { -58.372082, -34.608469 };
        //map.CenterWGS84 = new double[2] { 0.000, 0.000 };
		//map.CenterWGS84 = coords;
		map.UseLocation = true;
		map.InputsEnabled = true;
		map.ShowGUIControls = false;
		map.MinZoom = 2.9f;
		map.CurrentZoom = 10.5f;
		map.MaxZoom = 16f;


		// create an OSM tile layer
        OSMTileLayer osmLayer = map.CreateLayer<OSMTileLayer>("OSM");
        Debug.Log("Created the layer");
        osmLayer.BaseURL = "http://a.tile.openstreetmap.org/";

        mapObj = GameObject.Find("[Map]");
        mapObj.layer = 14;
        //mapObj.transform.parent = this.transform;
        //mapObj.transform.localScale *= 100f;


		Debug.Log("Initialized SlippyMap");
		
	}
	
	// Update is called once per frame
	void Update () {

		mScale = (float)map.CurrentZoom;


        if (winManager.userState == UserState.worldNavigation || winManager.userState == UserState.freeze)
        {
            map.InputsEnabled = false;
            return;
        }
        else
        	map.InputsEnabled = true;
		
		if(Input.GetAxis("Mouse ScrollWheel") > 0 && map.CurrentZoom <13.5f)
			map.Zoom(5f);
		
		if(Input.GetAxis("Mouse ScrollWheel") < 0)
			map.Zoom(-5f);


        var allMapObjs = mapObj.GetComponentsInChildren<Transform>();
        
        if(allMapObjs.Length > 5)
        {
	        foreach(var t in allMapObjs)
	        {
	        	if(t.gameObject.layer != 23 && t.gameObject.tag != "LocationNode" )
	        	{
	        		t.gameObject.layer = 23;
	        	}
	        }
    	}
		
	}

	void OnGUI()
	{
		//var rect = new Rect(Screen.width/2f - 50f, Screen.height/2f - 50f, 100f, 100f);
		//GUI.DrawTexture(rect, crosshair, ScaleMode.ScaleAndCrop);
	}

    //functions that tell this user interaction what to turn on and off when in focus or not.
    //They are called by the GUI Manager
    void OutOfFocus()
    {
        map.InputsEnabled = false;
    }

    void InFocus()
    {
        map.InputsEnabled = true;
    }
	
}
