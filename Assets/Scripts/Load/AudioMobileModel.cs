using UnityEngine;
using System;
using System.Collections;

public struct AudioMobileModel
{
    public string UserId { get; set; }
    public string Author { get; set; }
    public string Location { get; set; }
	public string Image {get; set;}
    public float Latitude { get; set; }
    public float Longitude { get; set; }
    public string Date { get; set; }
    public string Time { get; set; }
    public string Weather { get; set; }
    public string Info1 { get; set; }
    public string Info2 { get; set; }
    public string SoundURL { get; set; }
    public Vector3 Position { get; set; }
}
