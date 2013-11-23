using UnityEngine;
using System;
public class Mutant{
	public object original;
	public object value;
	public override string ToString(){
		return this.value.ToString();
	}
	public static implicit operator int(Mutant current){
		return (int)current.value;
	}
	public static implicit operator float(Mutant current){
		return (float)current.value;
	}
	public static implicit operator bool(Mutant current){
		return (bool)current.value;
	}
	public static implicit operator string(Mutant current){
		return (string)current.value;
	}
	public static implicit operator Mutant(float current){
		return new Mutant(current);
	}
	public static implicit operator Mutant(int current){
		return new Mutant(current);
	}
	public static implicit operator Mutant(bool current){
		return new Mutant(current);
	}
	public static implicit operator Mutant(string current){
		return new Mutant(current);
	}
	public Mutant(object value){
		this.original = this.value = value;
	}
	public void Scale(float value){
		this.Set((float)this.original * value);
	}
	public void Set(object value){
		this.value = value;
	}
	public void Revert(){
		this.value = this.original;
	}
}
[Serializable]
public class MFloat{
	public float original;
	public float value;
	public float min;
	public float max;
	public override string ToString(){
		return this.value.ToString();
	}
	public static implicit operator int(MFloat current){
		return (int)current.value;
	}
	public static implicit operator float(MFloat current){
		return current.value;
	}
	public static implicit operator MFloat(float current){
		return new MFloat(current);
	}
	public MFloat(float value){
		this.original = this.value = value;
	}
	public void Add(float value){
		this.Set(this.value + value);
	}
	public void Scale(float value){
		this.Set(this.original * value);
	}
	public void Set(float value){
		if(!(this.min == 0 && this.max == 0)){
			value = Mathf.Clamp(value,this.min,this.max);
		}
		this.value = value;
	}
	public void Revert(){
		this.value = this.original;
	}
}
[Serializable]
public class MInt{
	public int original;
	public int value;
	public int min;
	public int max;
	public override string ToString(){
		return this.value.ToString();
	}
	public static implicit operator int(MInt current){
		return current.value;
	}
	public static implicit operator MInt(int current){
		return new MInt(current);
	}
	public MInt(int value){
		this.original = this.value = value;
	}
	public void Add(int value){
		this.Set(this.value + value);
	}
	public void Scale(int value){
		this.Set(this.original * value);
	}
	public void Set(int value){
		if(!(this.min == 0 && this.max == 0)){
			value = Mathf.Clamp(value,this.min,this.max);
		}
		this.value = value;
	}
	public void Revert(){
		this.value = this.original;
	}
}
[Serializable]
public class MBool{
	public bool original;
	public bool value;
	public override string ToString(){
		return this.value.ToString();
	}
	public static implicit operator bool(MBool current){
		return current.value;
	}
	public static implicit operator MBool(bool current){
		return new MBool(current);
	}
	public MBool(bool value){
		this.original = this.value = value;
	}
	public void Set(bool value){
		this.value = value;
	}
	public void Revert(){
		this.value = this.original;
	}
}
[Serializable]
public class MString{
	public string original;
	public string value;
	public override string ToString(){
		return this.value.ToString();
	}
	public static implicit operator string(MString current){
		return current.value;
	}
	public static implicit operator MString(string current){
		return new MString(current);
	}
	public MString(string value){
		this.original = this.value = value;
	}
	public void Set(string value){
		this.value = value;
	}
	public void Revert(){
		this.value = this.original;
	}
}