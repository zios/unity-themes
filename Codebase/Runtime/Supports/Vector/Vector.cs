using System.Linq;
namespace Zios.Supports.Vector{
	public struct Vector{
		public static Vector zero = new Vector(0,0,0);
		public float[] values;
		public Vector(params float[] values){
			this.values = values;
		}
		public static Vector operator +(Vector current,Vector other){
			var output = new Vector(current.values);
			for(int index=0;index<output.values.Length;++index){
				output.values[index] += other.values[index];
			}
			return output;
		}
		public static Vector operator -(Vector current,Vector other){
			var output = new Vector(current.values);
			for(int index=0;index<output.values.Length;++index){
				output.values[index] -= other.values[index];
			}
			return output;
		}
		public static Vector operator *(Vector current,Vector other){
			var output = new Vector(current.values);
			for(int index=0;index<output.values.Length;++index){
				output.values[index] *= other.values[index];
			}
			return output;
		}
		public static Vector operator *(Vector current,float value){
			return current*new Vector(Enumerable.Repeat(value,current.values.Length).ToArray());
		}
	}
}