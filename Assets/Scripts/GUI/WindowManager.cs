using UnityEngine;
using System.Collections;

public enum UserState
{
    guiInteract,
    worldNavigation,
    freeze
}

public class WindowManager : MonoBehaviour
{

    public ScreenRenderTexture[] rTextures;
    public int currLayoutSel;
    public float counter;
    public GUISkin skin;
    public Camera sceneCamera;
    public Rect[] guiRects;
    public UserState userState;

    private bool guiMoving = false;
    private int currGUISel;
    private int guiSel = 0;
    private int screenLayout = 0;
    private Vector2 mPos;
    private string[] toolMenu;
    private bool ready = false;
    private Color guiColor;
    public bool forceFreeze;
    private Rect fullRect;

    public float yLevel;
    public float xLevel;
    public Rect camRect;

    //important variable - GUILayout class depends on it. Must set manually.
    public float winHeight = 0.4f * 800f;//Screen.height;

    // Use this for initialization
    void Start()
    {

        toolMenu = new string[3] { "", "", "" };
        yLevel = Screen.height;
        xLevel = 0f;
        guiRects = new Rect[rTextures.Length];
        guiColor = Color.clear;



        fullRect = new Rect(0, 0, Screen.width, Screen.height);
        //Initialize controllable components
        MainGUI.winHeight = winHeight;
        MainGUI.skin = skin;
        MainGUI.Init();
        ready = true;
        forceFreeze = false;

        userState = UserState.guiInteract;

        var tmp = (++screenLayout) % 2;
        StartCoroutine(LerpY(Screen.height - (tmp * 0.4f * Screen.height)));

    }

    // Update is called once per frame
    void Update ()
	{

		//update main scene camera
		sceneCamera.pixelRect = new Rect (0f, Screen.height - yLevel, Screen.width, Screen.height);
		camRect = sceneCamera.pixelRect;

		//track mouse position;
		mPos = Input.mousePosition;

		if (!forceFreeze) 
		{
			if (Screen.height - mPos.y > yLevel || Time.timeSinceLevelLoad < 2)
				userState = UserState.guiInteract;
			else
				userState = UserState.worldNavigation;
		}
		else
			userState = UserState.freeze;

        //Track current gui menu selection
        GUISelection();

    }

    void OnGUI()
    {
        if (!ready)
            return;

        GUI.skin = skin;
        //GUI.color = new Color(1,1,1,1);

        if (guiColor != Color.clear)
        {
            GUI.backgroundColor = guiColor;
            fullRect = GUI.Window(99, fullRect, DrawFullWindow, "", skin.customStyles[8]);
        }

        //Draw windows and fill with render textures
        for (int i = 0; i < rTextures.Length; i++)
        {
            var currRect = new Rect(xLevel + (i * Screen.width), yLevel, Screen.width, winHeight);

            //Since each window has a lot of GUI controls, we call out to the separate class MainGUI.
            guiRects[i] = GUI.Window(i, currRect, MainGUI.SelectLayout, rTextures[i].rTexture);
        }
	

        //Need to keep toolbar window on top.
        var toolRect = new Rect(0, yLevel - 20, Screen.width, 35);
        toolRect = GUI.Window(100, toolRect, DrawToolBar, "", skin.customStyles[0]);

        if (guiColor == Color.clear)
            GUI.BringWindowToFront(100);

		GUI.depth = 5;
		MainGUI.RecordGUI();

    }

    void DrawFullWindow(int windowID)
    {
        var offset = new RectOffset(100, 100, 300, 300);

        var labelRect = offset.Remove(fullRect);

        GUI.Label(labelRect, "Loading Echoscape...", skin.customStyles[9]);
    
    }



    void DrawToolBar(int ID)
    {
        var sizeRect = new Rect(5, 5, 20, 20);
        if (GUI.Button(sizeRect, "", skin.customStyles[2]))
        {
            var tmp = (++screenLayout) % 2;
            StartCoroutine(LerpY(Screen.height - (tmp * 0.4f * Screen.height)));
        }

        var selRect = new Rect(Screen.width - 50, 5, 45, 10);
        currGUISel = GUI.Toolbar(selRect, currGUISel, toolMenu, skin.customStyles[1]);
    }

    IEnumerator GUIAlpha(Color target)
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

    public void GUIAlphaLerp(Color target)
    {
        StartCoroutine(GUIAlpha(target));
    }

    IEnumerator LerpY(float yVal)
    {
        guiMoving = true;
        while (Mathf.Abs(yLevel - yVal) > 0.005f)
        {
            yLevel = Mathf.Lerp(yLevel, yVal, 10f * Time.deltaTime);
            yield return 0;
        }
        guiMoving = false;
        yLevel = yVal;

        yield return 0;
    }

    void GUISelection()
    {
        if (currGUISel != guiSel && !guiMoving)
        {
            StartCoroutine(LerpGUIValue(-1 * currGUISel * Screen.width));
            guiSel = currGUISel;
        }
    }

    IEnumerator LerpGUIValue(int target)
    {
        guiMoving = true;

        while (Mathf.Abs(xLevel - target) > 0.005f)
        {
            xLevel = Mathf.Lerp(xLevel, target, 10f * Time.deltaTime);
            yield return 0;
        }

        xLevel = target;
        guiMoving = false;
        yield return 0;
    }

    public Ray RayThroughWindow(Vector3 mousePosition)
    {
        var tmpRay = new Ray();

        return tmpRay;
    }
}
