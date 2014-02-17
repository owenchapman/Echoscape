using UnityEngine;
using System.Collections;

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
    */
    public AudioParameter(float from, float to, float value, string parameterName, string shortName)
    {
        this.from = from;
        this.to = to;
        this.value = value;
        this.ParameterName = parameterName;
        this.initialValue = value;
		this.nickName = shortName;
    }
}

public class EffectController
{
    public AudioParameter[] parameters;
    public Rect clientRect;
    public string name;
    public Object AudioFilter;

    private bool mute;

    public EffectController(AudioParameter[] parameters, Rect clientRect, string name, Object audioFilter)
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
}
