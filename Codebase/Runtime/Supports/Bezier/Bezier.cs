using System;
using System.Collections.Generic;
namespace Zios.Supports.Bezier{
	using Zios.Supports.Vector;
	[Serializable]
	public class Bezier{
		public float[] data;
		public Bezier(params float[] values){
			this.data = values;
		}
		public float Curve(float step){
			List<float> points = new List<float>();
			List<float> lines = new List<float>();
			float start = 0;
			points.AddRange(this.data);
			while(points.Count > 1){
				lines.Clear();
				start = points[0];
				foreach(float point in points.GetRange(1,points.Count-1)){
					lines.Add(((point - start) * step) + start);
					start = point;
				}
				points.Clear();
				points.AddRange(lines);
			}
			return points[0];
		}
	}
	[Serializable]
	public class BezierVector{
		public Vector[] data;
		public BezierVector(params Vector[] values){
			this.data = values;
		}
		public Vector Curve(float step){
			var points = new List<Vector>();
			var lines = new List<Vector>();
			var start = new Vector(0,0,0);
			points.AddRange(this.data);
			while(points.Count > 1){
				lines.Clear();
				start = points[0];
				foreach(Vector point in points.GetRange(1,points.Count-1)){
					lines.Add(((point - start) * step) + start);
					start = point;
				}
				points.Clear();
				points.AddRange(lines);
			}
			return points[0];
		}
	}
}