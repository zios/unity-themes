using System;
namespace Zios.Supports.Mutant{
	using Zios.Extensions;
	[Serializable]
	public class Mutant<Type>{
		public Type original;
		public Type current;
		public Action GetCallback = ()=>{};
		public Action SetCallback = ()=>{};
		public override string ToString(){return this.current.ToString();}
		public static implicit operator Type(Mutant<Type> value){return value.current;}
		public static implicit operator Mutant<Type>(Type value){return new Mutant<Type>(value);}
		public Mutant(Type value){this.original = this.current = value;}
		public Type Get(){
			this.GetCallback();
			return this.current;
		}
		public void Set(Type value){
			this.current = value;
			this.SetCallback();
		}
		public void Revert(){this.current = this.original;}
		public void Morph(){this.original = this.current;}
		public bool HasChanged(){return !this.current.Equals(this.original);}
	}
	[Serializable]
	public class MutantRanged : Mutant<float>{
		public float min;
		public float max;
		public MutantRanged(float value,float min=-1,float max=-1) : base(value){
			this.min = min;
			this.max = max;
			this.SetCallback += this.Clamp;
		}
		public void Clamp(){
			if(this.min != -1 || this.max != -1){
				this.current = this.current.Clamp(this.min,this.max);
			}
		}
		public void Add(float value){this.Set(this.current+value);}
		public void Scale(float value){this.Set(this.current*value);}
		public float Lerp(float value){return value.Lerp(this.min,this.max);}
	}
}