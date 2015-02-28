using UnityEngine;
using System;
using Zios;
namespace Zios{
    using Random = UnityEngine.Random;
    public enum AudioOrder{First,Sequence,Random}
    [AddComponentMenu("Zios/Component/Action/Play Audio")]
    public class PlayAudio : ActionLink{
	    public AudioOrder order;
	    public AudioSource[] sources = new AudioSource[1];
	    public AttributeFloat pitch = -1;
	    public AttributeFloat volume = -1;
	    public AttributeBool stopWhenNotUsable = false;
	    private AudioSource currentSource;
	    public override void Awake(){
		    base.Awake();
            this.pitch.Setup("Pitch", this);
            this.volume.Setup("Volume", this);
		    this.stopWhenNotUsable.Setup("Stop When Not Usable",this);
		    this.currentSource = this.sources[0];
	    }
	    public override void Use(){
		    base.Use();
		    this.currentSource.pitch = this.pitch.Get() == -1 ? this.currentSource.pitch : this.pitch.Get();
		    this.currentSource.volume = this.volume.Get() == -1 ? this.currentSource.volume : this.volume.Get();
		    this.currentSource.Play();
		    if(this.order == AudioOrder.Sequence){
			    int index = this.sources.IndexOf(this.currentSource) + 1;
			    if(index > this.sources.Length-1){index = 0;}
			    if(index < 0){index = this.sources.Length-1;}
			    this.currentSource = this.sources[index];
		    }
		    if(this.order == AudioOrder.Random){
			    int index = (int)Random.Range(0,this.sources.Length-1);
			    this.currentSource = this.sources[index];
		    }
	    }
	    public override void End(){
		    base.End();
		    if(this.stopWhenNotUsable){
			    foreach(AudioSource source in this.sources){
				    source.Stop();
			    }
		    }
	    }
    }
}
