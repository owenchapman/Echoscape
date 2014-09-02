using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NodeFilter : MonoBehaviour
{
    private string filter;
    private bool sort = false;
    public bool dispGUI = false;
    public GameObject[] audioNodes;
    public List<GameObject> filteredNodes;
    private bool clear = false;
    public GUISkin skin;

    private string searchString;

    private int lastLength = 10;

    // Use this for initialization
    void Start()
    {
        filter = "";
        audioNodes = GameObject.FindGameObjectsWithTag("AudioNode") as GameObject[];
        filteredNodes = new List<GameObject>();

        ResetToCompleteList();
    }

    // Update is called once per frame
    void Update()
    {
        audioNodes = GameObject.FindGameObjectsWithTag("AudioNode") as GameObject[];
    }

    public void FilterNodes(string searchString)
    {
        filteredNodes = new List<GameObject>();

        this.searchString = searchString;

        for (int i = 0; i < audioNodes.Length; i++)
        {
            var recNode = audioNodes[i].GetComponent<RecordingNode>();
            var tmpData = recNode.data;

            if (!tmpData.Author.Contains(searchString) && !tmpData.UserId.Contains(searchString) && !tmpData.Location.Contains(searchString))
            {
                audioNodes[i].renderer.material.color = new Color(1f, 1f, 1f, 0.1f);
                audioNodes[i].audio.Stop();
            }

            else
                filteredNodes.Add(audioNodes[i]);

        }
    }

    public void ResetToCompleteList()
    {
        filteredNodes = new List<GameObject>();

        for (int i = 0; i < audioNodes.Length; i++)
        {
            audioNodes[i].renderer.material.color = Color.green;
            filteredNodes.Add(audioNodes[i]);
        }

        Debug.Log("In Filter: " + filteredNodes.Count);
    }

}
