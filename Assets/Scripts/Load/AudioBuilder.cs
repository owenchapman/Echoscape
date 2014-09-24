using System;
using System.Xml;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class AudioBuilder
{
    List<AudioMobileModel> allNodeData;
    private float pScale;
    private Vector3 moveVec;
    public Map map;

    public void SetScale(float scale, Vector3 translate)
    {
        pScale = scale;
        moveVec = translate;
        
    }

    public void ConstructAudio(GameObject parent, Map worldMap, DataUrls urls, DownloadedData dlData, ConstructedData conData,
                               Action completedCallback)
    {
        map = worldMap;
        var doc = new XmlDocument();
        doc.LoadXml(dlData.audio);

        var nodes = doc.GetElementsByTagName("marker");
        var avg = new Vector3();
        allNodeData = new List<AudioMobileModel>();

        foreach (XmlNode node in nodes)
        {
            var data = new AudioMobileModel();

            data.UserId = node.Attributes["user_id"].Value;
            data.Author = node.Attributes["author"].Value;
            data.Location = node.Attributes["location"].Value;
            data.Latitude = float.Parse(node.Attributes["latitude"].Value);
            data.Longitude = float.Parse(node.Attributes["longitude"].Value);
			data.Image = node.Attributes["imagefile"].Value;

            DateTime date;
            if (DateTime.TryParse(node.Attributes["date"].Value, out date))
                data.Date = date.Date.ToShortDateString();
            else
                data.Date = "";

            TimeSpan time;
            if (TimeSpan.TryParse(node.Attributes["time"].Value, out time))
                data.Time = time.ToString();
            else
                data.Time = "";

            data.Weather = node.Attributes["weather"].Value;
            data.Info1 = node.Attributes["info1"].Value;
            data.Info2 = node.Attributes["info2"].Value;
            
            //data.SoundURL = urls.dataFolder + "ogg/" + node.Attributes["soundfile"].Value;
            data.SoundURL = node.Attributes["soundfile"].Value;

            //data.Position = Util.LonLat2Mercator(new Vector3(data.Longitude, 0f, data.Latitude), pScale);
            //data.Position += moveVec;

			data.Position = Util.LonLat2Mercator2(new Vector3(data.Latitude, 0f, data.Longitude), map);
			data.Position += new Vector3(0f, Util.GetTerrainHeightAt(data.Position) + 0.5f, 0f);

			if(data.UserId == "1001")
			{
				Debug.Log ("Lat: " + data.Latitude + ": " + "Lon: " + data.Longitude);
				Debug.Log (data.Position);
			}


            //avg += data.Position;

            allNodeData.Add(data);
        }
        
        //conData.averagePoint = avg / nodes.Count;

        conData.recNodes = new GameObject("Recording Nodes");
        conData.recNodes.tag = "NodeParentObj";
        conData.recNodes.transform.parent = parent.transform;

        foreach (var data in allNodeData)
        {
            //var recordingNode = conData.recNodes.AddComponent<RecordingNode>();
            var node = UnityEngine.MonoBehaviour.Instantiate(Resources.Load("Prefab/AudioVisualNode")) as GameObject;
            var recordingNode = node.GetComponent<RecordingNode>();

            node.transform.position = data.Position;
            node.transform.localScale *= 100f*map.RoundedScaleMultiplier;
            node.transform.parent = conData.recNodes.transform;
            recordingNode.data = data;
			recordingNode.fileName = data.SoundURL;
        }

        completedCallback();

    }

}
