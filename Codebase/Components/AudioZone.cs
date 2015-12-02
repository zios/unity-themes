using UnityEngine;
namespace Zios{
	[AddComponentMenu("Zios/Component/General/Audio Zone")]
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
			this.nextPlay = Time.time + Random.Range(this.minDelay,this.maxDelay);
			this.index = this.random ? Random.Range(0,this.sounds.Length-1) : ++index;
			if(this.index >= this.sounds.Length){this.index = 0;}
			this.currentSound = this.sounds[this.index];
		}
		public void Update(){
			if(Time.time > this.nextPlay){
				this.Play();
				this.Queue();
			}
		}
		public void Play(){
			Debug.Log(this.currentSound);
			//float volume = Random.Range(0.2f,1);
			//Audio.Play(this.currentSound,this.transform.position,volume);
		}
	}
}