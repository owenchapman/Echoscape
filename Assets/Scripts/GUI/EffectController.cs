using UnityEngine;
using System.Collections;
using System;

[Serializable]
public struct AudioParameter
{
    public float from;
    public float to;
    public float value;
    public float initialValue;
    public string ParameterName;
    public string nickName;

    /*<summary>
    Parameter Name MUST match the name of the property you want to set (Reflection is used).
   </summary> */
    public AudioParameter(float from, float to, float value, string parameterName, string shortName)
    {
        this.from = from;
        this.to = to;
        this.value = value;
        this.ParameterName = parameterName;
        this.initialValue = value;
        this.nickName = shortName;
    }

	public void SetCurrentValue(float val)
	{
		this.value = val;
	}
}

[Serializable]
public class EffectController
{
    public AudioParameter[] parameters;
    public Rect clientRect;
    public string name;
    public UnityEngine.Object AudioFilter;

    public bool mute;

    public EffectController(AudioParameter[] parameters, Rect clientRect, string name, UnityEngine.Object audioFilter)
    {
        this.parameters = parameters;
        this.clientRect = clientRect;
        this.name = name;
        this.AudioFilter = audioFilter;

        this.mute = false;
    }

    /*<summary>
    Display is meant to be called from a Monobehaviour's OnGUI function
     </summary> */
    public void Display()
    {
        var offset = new RectOffset(10, 10, 30, 35);
        var paramRect = offset.Remove(this.clientRect);

        GUI.Box(this.clientRect, this.name);

        GUILayout.BeginArea(paramRect);
        GUILayout.BeginHorizontal();

        for (int i = 0; i < this.parameters.Length; i++)
        {
            GUILayout.BeginVertical();
            parameters[i].value = GUILayout.VerticalSlider(parameters[i].value, parameters[i].to, parameters[i].from);
            GUILayout.Label(parameters[i].nickName);
            GUILayout.EndVertical();

            var param = this.AudioFilter.GetType().GetProperty(parameters[i].ParameterName);
            param.SetValue(this.AudioFilter, parameters[i].value, null);
        }

        GUILayout.EndHorizontal();
        GUILayout.EndArea();

        var optionsRect = new Rect(paramRect.x, paramRect.y + paramRect.height + 5, paramRect.width - 25, 27);

        GUILayout.BeginArea(optionsRect);
        GUILayout.BeginHorizontal();

        mute = GUILayout.Toggle(mute, "On/Off");
        //would like to not set this every frame...
        var enabled = this.AudioFilter.GetType().GetProperty("enabled");
        enabled.SetValue(this.AudioFilter, mute, null);


        if (GUILayout.Button("Reset"))
        {
            for (int i = 0; i < this.parameters.Length; i++)
            {
                parameters[i].value = parameters[i].initialValue;
            }
        }


        GUILayout.EndHorizontal();
        GUILayout.EndArea();
		
    }

	public void SetParamValue(int paramIdx, float value)
	{
		this.parameters[paramIdx].value = value;
		var param = this.AudioFilter.GetType().GetProperty(parameters[paramIdx].ParameterName);
		param.SetValue(this.AudioFilter, parameters[paramIdx].value, null);
	}
}
