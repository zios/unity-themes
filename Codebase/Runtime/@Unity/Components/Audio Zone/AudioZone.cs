using UnityEngine;
namespace Zios.Unity.Components.AudioZone{
	using Zios.Unity.Log;
	using Zios.Unity.Time;
	[AddComponentMenu("Zios/Component/Audio/Audio Zone")]
	public class AudioZone : MonoBehaviour{
		public float minDelay = 2;
		public float maxDelay = 8;
		public bool random = true;
		private AudioSource[] sounds;
		private AudioSource currentSound;
		private float nextPlay;
		private int index = 0;
		public void Start(){
			this.sounds = this.GetComponentsInChildren<AudioSource>();
			this.Queue();
		}
		public void Queue(){
			this.nextPlay = Time.Get() + Random.Range(this.minDelay,this.maxDelay);
			this.index = this.random ? Random.Range(0,this.sounds.Length-1) : ++index;
			if(this.index >= this.sounds.Length){this.index = 0;}
			this.currentSound = this.sounds[this.index];
		}
		public void Update(){
			if(Time.Get() > this.nextPlay){
				this.Play();
				this.Queue();
			}
		}
		public void Play(){
			Log.Show(this.currentSound.ToString());
			//float volume = Random.Range(0.2f,1);
			//Audio.Play(this.currentSound,this.transform.position,volume);
		}
	}
}