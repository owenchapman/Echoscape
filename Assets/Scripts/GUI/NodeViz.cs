using UnityEngine;
using System.Collections;

public class NodeViz : MonoBehaviour {

    private GameObject player;

	// Use this for initialization
	void Start () {

        player = GameObject.FindGameObjectWithTag("PlayerCam");
	
	}
	
	// Update is called once per frame
	void Update () {

        this.transform.localScale += 0.005f * Vector3.one;
        this.transform.LookAt(player.transform.position);
        this.renderer.material.color -= 0.01f * Color.white;
	
	}
}
