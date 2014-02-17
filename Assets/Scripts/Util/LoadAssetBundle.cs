using UnityEngine;
using System.Collections;

public class LoadAssetBundle : MonoBehaviour{

    public string url;
    public int version;
    
    public IEnumerator LoadBundle()
    {
			url = @"file:///Users/dan/Dropbox/MESH/Projects/2012/12MP002 Echoscapes/Workspace/Phase 3.1/Echoscape 3.1/Assets/DefaultScape.unity3d";
			Debug.Log(url);
			var www = new WWW(url);
			
			while(!www.isDone)
				yield return 0;
		    
		    AssetBundle assetBundle = www.assetBundle;
		    GameObject gameObject = assetBundle.mainAsset as GameObject;
		    Instantiate(gameObject );
		    assetBundle.Unload(false);
        
    }
    
    void Start(){
        //StartCoroutine(LoadBundle());
    }
}