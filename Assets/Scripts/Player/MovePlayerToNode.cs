using UnityEngine;
using System.Collections;

public class MovePlayerToNode : MonoBehaviour {

    public MonoBehaviour[] scriptsToDisable;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void MovePlayer(GameObject targetObj)
    {
        foreach (var m in scriptsToDisable)
            m.enabled = false;
        
        StartCoroutine(LerpTransform(targetObj));
    }

    IEnumerator LerpTransform(GameObject targetObj)
    {
        int i = 0;
        int steps = 100;

        var targetPos = targetObj.transform.position - 2f * Vector3.forward;

        while (i < steps)
        {

            this.transform.position = Vector3.Lerp(this.transform.position, targetPos, ((float)i / steps));
            this.transform.LookAt(targetObj.transform);
            
            i++;
            yield return 0;
        }

        foreach (var m in scriptsToDisable)
            m.enabled = true;

        yield return 0;
    }
}
