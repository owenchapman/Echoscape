using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class ScapeSelector : MonoBehaviour
{

    public float lat;
    public float lon;
    public bool guiOff = true;
    public GUISkin skin;
    public Material mat;
    public Vector3 mapPos;
    public GameObject marker;

    public Map map;
    private WindowManager winManager;
    private Vector3 mPos;
    private GameObject scapeTile;
    private Vector3 lastPos;
    public Vector3 loadCoords;
    private bool freeze = false;
    private Rect loadRect;
    private Vector3 posVec;

    private Marker locationMarker;

    // Use this for initialization
    void Start()
    {

    
        mapPos = new Vector3();
        lastPos = new Vector3();
        loadCoords = new Vector3();
        //guiManager = GameObject.FindWithTag("GUIManager").GetComponent<GUIManager>();

        freeze = false;

        loadCoords = new Vector3(-73.575044f, 0f, 45.521430f);

        loadRect = new Rect(Screen.width - 125f, 20, 125f, 30f);

        Debug.Log("Initialized ScapeSelector");
        winManager = GameObject.FindWithTag("GUIManager").GetComponent<WindowManager>();

        posVec = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        if (map == null)
        {
            map = GameObject.Find("[Map]").GetComponent<Map>();

            if (map != null)
            {
                Debug.Log("Scape selector has the map");
                //initial scene load
                //Util.BeginLoadingEchoscape(loadCoords.z, loadCoords.x, 4, map);
            }
			else
				Debug.Log("Can't find map");

            return;
        }


        if(winManager.userState == UserState.worldNavigation)
            return;

        mPos = Input.mousePosition;
        mPos = this.camera.ScreenToWorldPoint(new Vector3(mPos.x, mPos.y + winManager.yLevel, this.transform.position.y));

        loadCoords = Util.Mercator2LonLat2(mPos, map);

        if(Input.GetMouseButtonDown(1))
        {
            var coords = new double[2]{loadCoords.x, loadCoords.z};
            
            if(locationMarker == null)
                locationMarker = map.CreateMarker<Marker>("ScapeSelector", coords, (GameObject) Instantiate(marker));

            else
                locationMarker.CoordinatesWGS84 = coords;
        }

    }

    public void Reset()
    {
        var tiles = GameObject.FindGameObjectsWithTag("EchoscapeTile") as GameObject[];

        foreach (var t in tiles)
            Destroy(t);

    }

    IEnumerator CenterLerp()
    {

        for (int i = 0; i <= 25; i++)
        {

            var p = Vector3.Lerp(lastPos, mapPos, 0.04f);
            var newPos = Util.Mercator2LonLat2(p, map);
            map.CenterWGS84 = new double[2] { newPos.x, newPos.z };
            map.HasMoved = true;

            yield return null;

        }

    }


    public void LoadTile()
    {
        if(locationMarker == null)
            return;

        var coords = locationMarker.CoordinatesWGS84;

        Reset();
        Util.BeginLoadingEchoscape((float)coords[1], (float)coords[0], 4, map);;
    }

    //functions that tell this user interaction what to turn on and off we focus or not.
    //They are called by the GUI Manager
    void OutOfFocus()
    {
        guiOff = true;

    }

    void InFocus()
    {
        guiOff = false;
    }
}


