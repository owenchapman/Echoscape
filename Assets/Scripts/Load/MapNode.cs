using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
[Serializable]
public class MapNodeModel
{
    public int Id { get; set; }
    public float Latitude { get; set; }
    public float Longitude { get; set; }
    public Vector3 Position { get; set; }
}
[Serializable]
public class MapWayModel
{
    public enum WayType
    {
        Highway,
        Building,
        Other
    }

    public string Name { get; set; }
    public WayType Type { get; set; }
    public List<MapNodeModel> Nodes { get; set; }
}