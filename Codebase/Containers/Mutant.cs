using UnityEngine;
using System;
public class Mutant{
	public object original;
	public object current;
	public override string ToString(){
		return this.current.ToString();
	}
	public static implicit operator int(Mutant value){
		return (int)value.current;
	}
	public static implicit operator float(Mutant value){
		return (float)value.current;
	}
	public static implicit operator bool(Mutant value){
		return (bool)value.current;
	}
	public static implicit operator string(Mutant value){
		return (string)value.current;
	}
	public static implicit operator Mutant(float value){
		return new Mutant(value);
	}
	public static implicit operator Mutant(int value){
		return new Mutant(value);
	}
	public static implicit operator Mutant(bool value){
		return new Mutant(value);
	}
	public static implicit operator Mutant(string value){
		return new Mutant(value);
	}
	public Mutant(object value){
		this.original = this.current = value;
	}
	public void Scale(float value){
		this.Set((float)this.original * value);
	}
	public void Set(object value){
		this.current = value;
	}
	public void Revert(){
		this.current = this.original;
	}
}
[Serializable]
public class MFloat{
	public float original;
	public float current;
	public float min;
	public float max;
	public override string ToString(){
		return this.current.ToString();
	}
	public static implicit operator int(MFloat value){
		return (int)value.current;
	}
	public static implicit operator float(MFloat value){
		return value.current;
	}
	public static implicit operator MFloat(float value){
		return new MFloat(value);
	}
	public MFloat(float value){
		this.original = this.current = value;
	}
	public MFloat(float min,float max){
		this.original = this.current = min;
		this.min = min;
		this.max = max;
	}
	public MFloat(float value,float min,float max){
		this.original = this.current = value;
		this.min = min;
		this.max = max;
	}
	public float Lerp(float value){
		return Mathf.Lerp(this.min,this.max,value);
	}
	public void Add(float value){
		this.Set(this.current + value);
	}
	public void Scale(float value){
		this.Set(this.original * value);
	}
	public void Set(float value){
		if(!(this.min == 0 && this.max == 0)){
			current = Mathf.Clamp(value,this.min,this.max);
		}
		this.current = value;
	}
	public void Revert(){
		this.current = this.original;
	}
}
[Serializable]
public class MInt{
	public int original;
	public int current;
	public int min;
	public int max;
	public override string ToString(){
		return this.current.ToString();
	}
	public static implicit operator int(MInt value){
		return value.current;
	}
	public static implicit operator MInt(int value){
		return new MInt(value);
	}
	public MInt(int value){
		this.original = this.current = value;
	}
	public MInt(int min,int max){
		this.original = this.current = min;
		this.min = min;
		this.max = max;
	}
	public MInt(int value,int min,int max){
		this.original = this.current = value;
		this.min = min;
		this.max = max;
	}
	public int Lerp(float value){
		return (int)Mathf.Lerp(this.min,this.max,value);
	}
	public void Add(int value){
		this.Set(this.current + value);
	}
	public void Scale(int value){
		this.Set(this.original * value);
	}
	public void Set(int value){
		if(!(this.min == 0 && this.max == 0)){
			current = Mathf.Clamp(value,this.min,this.max);
		}
		this.current = value;
	}
	public void Revert(){
		this.current = this.original;
	}
}
[Serializable]
public class MBool{
	public bool original;
	public bool current;
	public override string ToString(){
		return this.current.ToString();
	}
	public static implicit operator bool(MBool value){
		return value.current;
	}
	public static implicit operator MBool(bool value){
		return new MBool(value);
	}
	public MBool(bool value){
		this.original = this.current = value;
	}
	public void Set(bool value){
		this.current = value;
	}
	public void Revert(){
		this.current = this.original;
	}
}
[Serializable]
public class MString{
	public string original;
	public string current;
	public override string ToString(){
		return this.current.ToString();
	}
	public static implicit operator string(MString value){
		return value.current;
	}
	public static implicit operator MString(string value){
		return new MString(value);
	}
	public MString(string value){
		this.original = this.current = value;
	}
	public void Set(string value){
		this.current = value;
	}
	public void Revert(){
		this.current = this.original;
	}
}