using UnityEngine;
using System.Collections;

public class TitleLoad2 : MonoBehaviour {

    public bool inGame = false;
    private Color guiColor;
    public Texture backColor;

	// Use this for initialization
	void Start () {

		guiColor = Color.clear;
		StartCoroutine(FadeIn());
	
	}
	
	// Update is called once per frame
	void Update () {

		if(Input.GetMouseButtonDown(0) && !inGame)
        {
            Application.LoadLevelAdditive(1);
            StartCoroutine(FadeAndDie());
    		
        }
	
	}

	void OnGUI()
	{
		GUI.depth = -10;
        var titleRect = new Rect(0, 100, Screen.width, Screen.height/2);
		var offset = new RectOffset(10,10,10,10);
		titleRect = offset.Remove(titleRect);

		GUI.color = guiColor;
        GUI.DrawTexture(new Rect(0,0,Screen.width, Screen.height), backColor, ScaleMode.StretchToFill);
		GUI.DrawTexture(titleRect, this.GetComponent<GUITexture>().texture, ScaleMode.ScaleToFit);
	}

    IEnumerator FadeAndDie()
    {
        int i = 0;
        inGame = true;
        //this.camera.clearFlags = CameraClearFlags.Depth;
        var targetCol = new Color(0f, 0f, 0f, 0f);
        var steps = 200;

        while(i < steps)
        {
            guiColor = Color.Lerp(Color.white, Color.clear, i * (1f/ steps));
            i++;
            yield return 0;
        }

        Destroy(this.gameObject, 0.1f);
        yield return 0;
    }

    IEnumerator FadeIn()
    {
        int i = 0;
        //this.camera.clearFlags = CameraClearFlags.Depth;

        while(i < 1000)
        {
            guiColor = Color.Lerp(guiColor, Color.white, 1f*Time.smoothDeltaTime);
            i++;
            yield return 0;
        }

        yield return 0;
    }
}
