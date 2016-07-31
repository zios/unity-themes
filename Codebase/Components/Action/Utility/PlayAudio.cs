using UnityEngine;
namespace Zios.Actions.AudioComponents{
	using Attributes;
	using Random = UnityEngine.Random;
	public enum AudioOrder{PlayOnlyFirst,PlaySequence,PlayRandom}
	[AddComponentMenu("Zios/Component/Action/General/Play Audio")]
	public class PlayAudio : StateMonoBehaviour{
		public AudioOrder order;
		public AudioSource[] sources = new AudioSource[1];
		[Advanced] public AttributeFloat pitch = -1;
		[Advanced] public AttributeFloat volume = -1;
		[Advanced] public AttributeBool stopWhenNotUsable = false;
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
			if(this.order == AudioOrder.PlaySequence){
				int index = this.sources.IndexOf(this.currentSource) + 1;
				if(index > this.sources.Length-1){index = 0;}
				if(index < 0){index = this.sources.Length-1;}
				this.currentSource = this.sources[index];
			}
			if(this.order == AudioOrder.PlayRandom){
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