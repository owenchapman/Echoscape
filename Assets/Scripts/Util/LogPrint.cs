using UnityEngine;
using System.Collections;

public class LogPrint : MonoBehaviour
{	
	private static string myLog;
	private string output = "";
	private string stack = "";
	private Vector2 scrollPosition;
	private Rect scrollRect;
    public GUISkin skin;

	// Use this for initialization
	void OnEnable ()
	{
		
		Application.RegisterLogCallback (HandleLog);
		scrollRect = new Rect(2, 2, Screen.width, 100);
		Debug.Log("Starting Log");
		//scrollPosition = new Vector2(0f, 0f);
	
	}
	
	void OnDisable ()
	{
		// Remove callback when object goes out of scope
		Application.RegisterLogCallback (null);
		
	}
	
	void HandleLog (string logString, string stackTrace,LogType type )
	{
		output = logString;
		stack = stackTrace;
		myLog += " : " + output;
        //myLog = output;
	}
	
	void OnGUI ()
	{
        GUI.skin = skin;

        GUI.TextArea(scrollRect, myLog);
		//myLog = GUI.TextField (new Rect (10, 50, Screen.width - 10, 25), myLog);
	}
}
