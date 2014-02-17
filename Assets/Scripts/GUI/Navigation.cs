using UnityEngine;
using System.Collections.Generic;

public class Navigation : MonoBehaviour
{
    bool loaded;
    List<GameObject> textObjs;
    //Vector3 lastCamFor;
    //Vector3 lastCamPos;
    private GameObject textObjTemp;
    private GameObject player;

    public List<MapWayModel> Ways { get; set; }
	public Material signMat;
    private GameObject streetCam;
    public bool offline = false;

    // Use this for initialization
    void Start()
    {
        loaded = false;
        textObjs = new List<GameObject>();
        textObjTemp = Resources.Load("Prefab/TextMesh") as GameObject;
        player = GameObject.FindGameObjectWithTag("Player");
        streetCam = GameObject.Find("StreetCam");

        //lastCamFor = new Vector3();
        //lastCamPos = new Vector3();
		signMat = (Material)Resources.Load("Materials/Signs");
		if(offline)
			textObjs = new List<GameObject>(GameObject.FindGameObjectsWithTag("Label"));
    }

    // Update is called once per frame
    void Update()
    {
        if (Ways == null && !offline)
            return;
            
		if (!loaded && !offline)
            InitWays();

        // return if camera has not altered direction
        //if (lastCamFor == Camera.main.transform.forward &&
        //    lastCamPos == Camera.main.transform.position)
        //    return;

        foreach(var t in textObjs)
        {
            var d = Vector3.Distance(t.transform.position, streetCam.transform.position);

            d /= 10f;
            d = Mathf.Pow(1f/d, 2f);
            d = Mathf.Clamp(d, 0f, 0.8f);

            t.renderer.material.color = new Color(d, d, d, d);
            t.transform.LookAt(streetCam.transform.position);
            t.transform.forward *= -1f;
        }



    }

    void InitWays()
    {
        Debug.Log("Initiating ways");

        foreach (var way in Ways)
        {
            if (way.Name == "" || way.Type != MapWayModel.WayType.Highway)
                continue;

            //Debug.Log(way.Name);

            var textObj = Instantiate(textObjTemp) as GameObject;
            textObj.transform.localScale *= 1f;
            textObj.layer = 10;
            textObj.transform.parent = this.gameObject.transform;
            var text = textObj.GetComponent<TextMesh>();
            var line = textObj.GetComponent<LineRenderer>();

            //set up line renderer - pretty heavy computation

            if (line.enabled)
            {
                line.SetVertexCount(way.Nodes.Count);

                for (int i = 0; i < way.Nodes.Count; i++)
                    line.SetPosition(i, way.Nodes[i].Position);

            }


            var rend = textObj.renderer;
            rend.castShadows = false;
            rend.receiveShadows = false;

            text.text = way.Name;

            if (way.Nodes.Count == 1)
                text.transform.position = way.Nodes[0].Position;

            else if (way.Nodes.Count > 0)
            {
                var wayVec = way.Nodes[1].Position - way.Nodes[0].Position;
                
                text.transform.position = (way.Nodes[0].Position + way.Nodes[1].Position) / 2f;
                //text.transform.rotation = Quaternion.FromToRotation(text.transform.right, wayVec);
                text.transform.position += 0.1f*Vector3.up;
                

                //var cross = Vector3.Cross(player.transform.forward, text.transform.right);

                //if (cross.y < 0f)
                    //text.transform.Rotate(Vector3.up, 180f, Space.World);

                //text.transform.RotateAroundLocal(text.transform.right, 60f * Mathf.Deg2Rad);
                //text.transform.forward = Vector3.down;
            }
            else
                continue;
			
			textObj.transform.position += new Vector3(0f, 0.01f, 0f);
            

            if (way.Type == MapWayModel.WayType.Highway)
            {
                text.transform.position += new Vector3(0f, 0f, 0f);
            }

            //settings for other types of labels
            else
            {
                text.transform.position += new Vector3(0f, 0.01f, 0f);
                rend.material.color = new Color(0f,0f,0f,0f);
            }

            textObjs.Add(textObj);
        }

        loaded = true;
    }
}
