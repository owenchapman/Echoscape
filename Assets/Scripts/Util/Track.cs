using UnityEngine;
using System.Collections;

public class Track {

	public FMOD.Sound Clip;
	public FMOD.Channel Channel;
	public bool isPlaying;
	public FMOD.System System;
	public bool active;
	public FMOD.CHANNELINDEX ChannelIndex;

	public Track (FMOD.Sound clip, FMOD.Channel channel, FMOD.System system)
	{	
	
		this.Channel = channel;		
		this.ChannelIndex = FMOD.CHANNELINDEX.FREE;				
		this.Clip = clip;
		this.isPlaying = false;
		this.System = system;
		this.active = false;
	
	}
	
	public void Play (bool loop)
	{

		if(!active || this.ChannelIndex == null)
			return;
		
		if(loop)
			this.Clip.setMode(FMOD.MODE.LOOP_NORMAL);
		
		this.System.playSound(this.ChannelIndex, this.Clip, false, ref this.Channel);			
		this.isPlaying = true;
	
	}
	
	public void Pause ()
	{
		if(!active)
			return;
		
		var pause = false;
		
		this.Channel.getPaused (ref pause);
		this.Channel.setPaused (!pause);
		
		this.isPlaying = pause;

	}
}
