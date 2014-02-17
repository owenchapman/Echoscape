using UnityEngine;
using System.Collections.Generic;


public class AudioLoop : MonoBehaviour
{
    private AudioSource audioComp;
    private int[] clip;
    private AudioClip loop;
    private bool initState;

    private GameObject playerObj;
    private float maxD;
    private bool toggleLoop = false;
    private bool reset = false;

    public RecordState recState;

    // Use this for initialization
    void Start()
    {
        initState = false;

        //intialize loop length to max length
        clip = new int[2];
        clip[0] = 0;
        clip[1] = int.MaxValue;

        playerObj = GameObject.FindWithTag("Player");
        recState = RecordState.Stopped;

    }

    // Update is called once per frame
    void Update()
    {
        if (!initState)
        {
            Init();
            return;
        }

        //set start of loop
        if (Vector3.Distance(this.transform.position, playerObj.transform.position) < 5 && this.audio.isPlaying)
        {
            if (Input.GetKeyDown(KeyCode.L) || recState == RecordState.Initialize)
            {
                Debug.Log("setting");
                clip[0] = audioComp.timeSamples;
                recState = RecordState.Recording;
            }

            //set end of loop and play
            if (Input.GetKeyUp(KeyCode.L) || recState == RecordState.Finalize)
            {
                Debug.Log("Finalizing");

                clip[1] = audioComp.timeSamples;
                audioComp.timeSamples = clip[1];

                audioComp.maxDistance = 1000f;

                recState = RecordState.Stopped;
            }
        }

        //check if audio is within loop, if not, restart
        if (audioComp.timeSamples > clip[1])
            audioComp.timeSamples = clip[0];
    }

    void Init()
    {
        audioComp = GetComponent<AudioSource>();

        if (audioComp != null)
        {
            initState = true;
            maxD = audioComp.maxDistance;
        }
    }

    void ToggleLoop(RecordState sentState)
    {

        if (recState == RecordState.Stopped && sentState == RecordState.Initialize)
            recState = RecordState.Initialize;

        if (recState == RecordState.Recording && sentState == RecordState.Finalize)
            recState = RecordState.Finalize;

    }

    void Reset()
    {
        clip[0] = 0;
        clip[1] = int.MaxValue;
        audioComp.maxDistance = 5f;
    }
}
