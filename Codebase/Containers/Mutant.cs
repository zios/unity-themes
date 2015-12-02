using System;
using UnityEngine;
namespace Zios{
	public class Mutant{
		public object original;
		public object current;
		public override string ToString(){return this.current.ToString();}
		public static implicit operator int(Mutant value){return (int)value.current;}
		public static implicit operator float(Mutant value){return (float)value.current;}
		public static implicit operator bool(Mutant value){return (bool)value.current;}
		public static implicit operator string(Mutant value){return (string)value.current;}
		public static implicit operator Mutant(float value){return new Mutant(value);}
		public static implicit operator Mutant(int value){return new Mutant(value);}
		public static implicit operator Mutant(bool value){return new Mutant(value);}
		public static implicit operator Mutant(string value){return new Mutant(value);}
		public Mutant(object value){this.original = this.current = value;}
		public void Scale(float value){this.Set((float)this.original * value);}
		public object Get(){return this.current;}
		public void Set(object value){this.current = value;}
		public void Revert(){this.current = this.original;}
		public void Morph(){this.original = this.current;}
		public bool HasChanged(){return this.current != this.original;}
	}
	[Serializable]
	public class MFloat{
		public float original;
		public float current;
		public float min;
		public float max;
		public override string ToString(){return this.current.ToString();}
		public static implicit operator int(MFloat value){return (int)value.current;}
		public static implicit operator float(MFloat value){return value.current;}
		public static implicit operator MFloat(float value){return new MFloat(value);}
		public MFloat(float value){this.original = this.current = value;}
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
		public float Lerp(float value){return Mathf.Lerp(this.min,this.max,value);}
		public void Add(float value){this.Set(this.current + value);}
		public void Scale(float value){this.Set(this.original * value);}
		public float Get(){return this.current;}
		public void Set(float value){
			if(!(this.min == 0 && this.max == 0)){
				current = Mathf.Clamp(value,this.min,this.max);
			}
			this.current = value;
		}
		public void Revert(){this.current = this.original;}
		public void Morph(){this.original = this.current;}
		public bool HasChanged(){return this.current != this.original;}
	}
	[Serializable]
	public class MInt{
		public int original;
		public int current;
		public int min;
		public int max;
		public override string ToString(){return this.current.ToString();}
		public static implicit operator int(MInt value){return value.current;}
		public static implicit operator MInt(int value){return new MInt(value);}
		public MInt(int value){this.original = this.current = value;}
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
		public int Lerp(float value){return (int)Mathf.Lerp(this.min,this.max,value);}
		public void Add(int value){this.Set(this.current + value);}
		public void Scale(int value){this.Set(this.original * value);}
		public int Get(){return this.current;}
		public void Set(int value){
			if(!(this.min == 0 && this.max == 0)){
				current = Mathf.Clamp(value,this.min,this.max);
			}
			this.current = value;
		}
		public void Revert(){this.current = this.original;}
		public void Morph(){this.original = this.current;}
		public bool HasChanged(){return this.current != this.original;}
	}
	[Serializable]
	public class MBool{
		public bool original;
		public bool current;
		public override string ToString(){return this.current.ToString();}
		public static implicit operator bool(MBool value){return value.current;}
		public static implicit operator MBool(bool value){return new MBool(value);}
		public MBool(bool value){this.original = this.current = value;}
		public bool Get(){return this.current;}
		public void Set(bool value){this.current = value;}
		public void Revert(){this.current = this.original;}
		public void Morph(){this.original = this.current;}
		public bool HasChanged(){return this.current != this.original;}
	}
	[Serializable]
	public class MString{
		public string original;
		public string current;
		public override string ToString(){return this.current.ToString();}
		public static implicit operator string(MString value){return value.current;}
		public static implicit operator MString(string value){return new MString(value);}
		public MString(string value){this.original = this.current = value;}
		public string Get(){return this.current;}
		public void Set(string value){this.current = value;}
		public void Revert(){this.current = this.original;}
		public void Morph(){this.original = this.current;}
		public bool HasChanged(){return this.current != this.original;}
	}
	[Serializable]
	public class MVector2{
		public Vector2 original;
		public Vector2 current;
		public override string ToString(){return this.current.ToString();}
		public static implicit operator Vector2(MVector2 value){return value.current;}
		public static implicit operator MVector2(Vector2 value){return new MVector2(value);}
		public MVector2(Vector2 value){this.original = this.current = value;}
		public Vector2 Get(){return this.current;}
		public void Set(Vector2 value){this.current = value;}
		public void Revert(){this.current = this.original;}
		public void Morph(){this.original = this.current;}
		public bool HasChanged(){return this.current != this.original;}
	}
	[Serializable]
	public class MVector3{
		public Vector3 original;
		public Vector3 current;
		public override string ToString(){return this.current.ToString();}
		public static implicit operator Vector3(MVector3 value){return value.current;}
		public static implicit operator MVector3(Vector3 value){return new MVector3(value);}
		public MVector3(Vector3 value){this.original = this.current = value;}
		public Vector3 Get(){return this.current;}
		public void Set(Vector3 value){this.current = value;}
		public void Revert(){this.current = this.original;}
		public void Morph(){this.original = this.current;}
		public bool HasChanged(){return this.current != this.original;}
	}
}