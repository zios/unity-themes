using UnityEngine;
using System;
using System.Collections.Generic;
using System.Collections;
[Serializable]
public class EffectPrefab{
	public string name;
	public int maximum = 8;
	public bool flat = true;
	public bool loop = false;
	public Vector3 offset = new Vector3(0,0,0);
	public GameObject prefab;
}
public class Effect{
	public EffectPrefab prefab;
	public GameObject entity;
	public Vector3 scale;
	public Vector3 offset;
	public float initialSize;
	public bool free = true;
	public bool once = false;
	public void OnParticleEnd(){
		if(this.once){this.End();}
	}
	public void UpdatePosition(Vector3 position,float size=1.0f){
		position.y += 1.1f;
		this.entity.transform.position = position + (this.offset * size);
	}
	public void End(){
		if(this.entity != null){
			this.free = true;
			this.entity.SetActive(false);
		}
	}
}
[AddComponentMenu("Zios/Singleton/Effect")]
public class EffectManager : MonoBehaviour{
	public EffectPrefab[] effects = new EffectPrefab[64];
	private Dictionary<string,Effect[]> instances = new Dictionary<string,Effect[]>();
	public Effect FindAvailable(string name){
		if(this.instances.ContainsKey(name)){
			foreach(Effect effect in this.instances[name]){
				if(effect.free){return effect;}
			}
		}
		return null;
	}
	public void Awake(){
		Global.Effect = this;
	}
	public void Start(){
		Transform parent = Locate.GetScenePath("Effects").transform;
		for(int index=0;index<this.effects.Length;++index){
			EffectPrefab effect = this.effects[index];
			if(effect == null || effect.prefab == null){continue;}
			Effect[] slots = this.instances[effect.name] = new Effect[effect.maximum];
			Vector3 rotation = new Vector3(effect.flat ? 270 : 0,180,0);
			for(int current=0;current<effect.maximum;++current){
				Effect instance = slots[current] = new Effect();
				instance.prefab = effect;
				instance.entity = (GameObject)Instantiate(effect.prefab);
				instance.entity.SetActive(false);
				instance.entity.transform.parent = parent;
				Material material = instance.entity.renderer.sharedMaterial;
				instance.entity.renderer.sharedMaterial = (Material)Instantiate(material);
				instance.scale = instance.entity.transform.localScale;
				instance.offset = effect.offset;
				ParticleSystem particle = instance.entity.GetComponent<ParticleSystem>();
				if(particle != null){
					particle.Stop();
					instance.initialSize = particle.startSize;
				}
				else{
					instance.entity.transform.localEulerAngles = rotation;
				}
			}
		}
	}
	public Effect AddEffect(string name,Vector3 position,int speed=6,float size=1.0f,bool mirrorX=false,bool mirrorY=false,bool once=true,int duration=-1){
		Effect effect = this.FindAvailable(name);
		if(position == Vector3.zero){return null;}
		if(effect != null){
			Vector3 scale = effect.scale * size;
			ParticleController particle = effect.entity.GetComponent<ParticleController>();
			if(particle != null){
				particle.instance.Clear();
				particle.instance.Play();
				particle.instance.startSize = effect.initialSize * size;
				particle.onLast = effect.OnParticleEnd;
			}
			position.y += 1.1f;
			if(mirrorX){scale.x *= -1;}
			if(mirrorY){scale.y *= -1;}
			effect.UpdatePosition(position,size);
			effect.entity.renderer.castShadows = !effect.prefab.flat;
			effect.entity.transform.localScale = scale;
			effect.entity.SetActive(true);
			effect.once = once;
			effect.free = false;
		}
		return effect;
	}
}
