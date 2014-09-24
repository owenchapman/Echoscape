using UnityEngine;
using System.Collections;
using System.IO;
using System;

public class LogPrint : MonoBehaviour
{	
		private static string myLog;
		private string output = "";
		private string stack = "";
		private Vector2 scrollPosition;
		private Rect fullRect;
		private Rect scrollRect;
		private Rect contentRect;
		public GUISkin skin;
		private string defaultDir;
		private string path;

		// Use this for initialization
		void OnEnable ()
		{
		
				Application.RegisterLogCallback (HandleLog);
				scrollRect = new Rect (300, 25, Screen.width - 305, 120);
				fullRect = new Rect (scrollRect);
				fullRect.height = 2 * scrollRect.height;
				fullRect.width = scrollRect.width - 20;
				contentRect = new Rect (scrollRect);
				Debug.Log ("Starting Log");
				//scrollPosition.y = scrollRect.height;
				//scrollPosition = new Vector2(0f, 0f);


				defaultDir = Path.Combine (Application.dataPath, "UserRecordings");
		
				if (Application.platform == RuntimePlatform.OSXEditor)
						defaultDir = "/Users/dan/Documents/UserRecordings";
		
				if (Application.platform == RuntimePlatform.WindowsEditor)
						defaultDir = @"C:\Users\Home\AppData\Roaming\Echoscape\";
		
				if (!Directory.Exists (defaultDir))
						Directory.CreateDirectory (defaultDir);
		
				path = Path.Combine (defaultDir, "DebugLog.txt");

				if (!File.Exists (path))
						File.Create (path);
				using (StreamWriter w = File.CreateText (path)) {
						w.WriteLine ("Echoscape Log File: " + DateTime.Now.ToString ());
						w.WriteLine ("________________________________________________________________");
				}

		
	
		}
	
		void OnDisable ()
		{
				// Remove callback when object goes out of scope
				Application.RegisterLogCallback (null);
		
		}
	
		void HandleLog (string logString, string stackTrace, LogType type)
		{
				output = logString;
				stack = stackTrace;
				myLog += output + "\n";
				//myLog = output;
				var h = skin.textArea.fontSize;
				fullRect.height += 2f * h;
				scrollRect.height += 2f * h;
				scrollPosition.y += 1.1f * h;

				using (StreamWriter w = File.AppendText(path))		
						w.WriteLine (output);

		}
	
		void OnGUI ()
		{
				GUI.skin = skin;

				scrollPosition = GUI.BeginScrollView (contentRect, scrollPosition, fullRect);
				GUI.TextArea (scrollRect, myLog);
				GUI.EndScrollView ();
				//myLog = GUI.TextField (new Rect (10, 50, Screen.width - 10, 25), myLog);
		}
}
