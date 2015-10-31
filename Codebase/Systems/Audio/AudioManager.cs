using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
namespace Zios{
	[AddComponentMenu("Zios/Singleton/Audio")]
	public class AudioManager : MonoBehaviour{
		public AudioListener listener;
		public Dictionary<string,AudioSource> sounds = new Dictionary<string,AudioSource>();
		public float soundVolume = 1.0f;
		public float musicVolume = 0.5f;
		public string musicTrack = "";
		public string fallbackTrack = "";
		private AudioSource music;
		private AudioSource[] playlist;
		private string currentTrack;
		private float startDelay;
		private string[] help = new string[]{
			"^3soundVolume ^9<^7number^9> :^10 Controls the volume for all sound and environment audio.",
			"^3musicVolume ^9<^7number^9> :^10 Controls the volume for all music and jingles.",
			"^3mute ^9 :^10 Toggles all sound/music as being off or on.",
		};
		public void Awake(){
			this.startDelay = Time.time + 0.1f;
		}
		public void Start(){
			Zios.Console.AddCvarMethod("soundVolume",this,"soundVolume","Sound Volume",this.help[0],this.SetSoundVolume);
			Zios.Console.AddCvarMethod("musicVolume",this,"musicVolume","Music Volume",this.help[1],this.SetMusicVolume);
			Zios.Console.AddKeyword("mute",this.ToggleMute);
			foreach(AudioSource sound in FindObjectsOfType(typeof(AudioSource))){
				if(sound.clip == null){continue;}
				this.sounds[sound.clip.name] = sound;
			}
			Transform musicGroup = GameObject.Find("Music").transform;
			this.listener = (AudioListener)FindObjectOfType(typeof(AudioListener));
			this.playlist = musicGroup.GetComponentsInChildren<AudioSource>(true);
		}
		public void LateUpdate(){
			if(this.musicTrack == "" || (this.music != null && !this.music.isPlaying)){
				this.musicTrack = this.fallbackTrack;
			}
			if(this.musicTrack != this.currentTrack){
				this.SetMusic(this.musicTrack);
			}
			this.transform.position = this.listener.transform.position;
		}
		public void SetMusic(string name){
			if(name == ""){return;}
			if(this.music != null){this.music.Stop();}
			foreach(AudioSource track in this.playlist){
				if(track.clip.name == this.musicTrack){
					this.music = track;
					this.music.volume = this.musicVolume;
					this.music.Play();
					this.currentTrack = name;
					break;
				}
			}
		}
		public void SetSoundVolume(){
			Transform musicGroup = GameObject.Find("Music").transform;
			foreach(AudioSource sound in FindObjectsOfType(typeof(AudioSource))){
				if(sound.transform != musicGroup){
					sound.volume = this.soundVolume;
				}
			}
		}
		public void SetMusicVolume(){
			foreach(AudioSource music in this.playlist){
				music.volume = this.musicVolume;
			}
		}
		public void ToggleMute(string[] values,bool help){
			if(help){
				Debug.Log(this.help[2]);
				return;
			}
			foreach(AudioSource sound in FindObjectsOfType(typeof(AudioSource))){
				sound.mute = !sound.mute;
			}
		}
		public AudioSource Play(AudioSource sound,float volumeScale=1.0f){
			if(this.listener == null){
				Debug.LogError("[AudioManager] No audio listener exists in the scene to play sound from.");
				return null;
			}
			return this.Play(sound,this.listener.transform.position,volumeScale);
		}
		public AudioSource Play(AudioSource sound,Vector3 position,float volumeScale=1.0f){
			if(Time.time < this.startDelay){return null;}
			Vector3 listenPosition = this.listener.transform.position;
			float distance = Vector3.Distance(listenPosition,position);
			if(distance <= sound.maxDistance && !(sound.loop && sound.isPlaying)){
				sound.volume = (1-distance/sound.maxDistance) * volumeScale * this.soundVolume;
				sound.Play();
				//AudioSource.PlayClipAtPoint(sound.clip,listenPosition,volume);
				//AudioSource.PlayOneShot(sound.clip,volume);
			}
			return sound;
		}
		public AudioSource Play(string name,Vector3 position,float volumeScale=1.0f){
			if(this.sounds.ContainsKey(name)){
				return this.Play(this.sounds[name],position,volumeScale);
			}
			return null;
		}
		public AudioSource Play(string name,float volumeScale=1.0f){
			if(this.sounds.ContainsKey(name)){
				return this.Play(this.sounds[name],volumeScale);
			}
			return null;
		}
		public void Stop(string name){
			if(this.sounds.ContainsKey(name)){
				this.sounds[name].Stop();
			}
		}
		public void ShiftPitch(string name,float pitchShift=0.0f,float pitchMin=0.5f,float pitchMax=2.0f){
			if(this.sounds.ContainsKey(name)){
				this.sounds[name].pitch += pitchShift;
				this.sounds[name].pitch = Mathf.Clamp(this.sounds[name].pitch,pitchMin,pitchMax);
			}
		}
		public void SetPitch(string name,float pitch=1.0f){
			if(this.sounds.ContainsKey(name)){
				this.sounds[name].pitch = pitch;
			}
		}
	}
}