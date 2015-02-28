using UnityEngine;
using System;
using System.Collections.Generic;
namespace Zios{
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
	    public Vector3[] data;
	    public BezierVector(params Vector3[] values){
		    this.data = values;
	    }
	    public Vector3 Curve(float step){
		    List<Vector3> points = new List<Vector3>();
		    List<Vector3> lines = new List<Vector3>();
		    Vector3 start = Vector3.zero;
		    points.AddRange(this.data);
		    while(points.Count > 1){
			    lines.Clear();
			    start = points[0];
			    foreach(Vector3 point in points.GetRange(1,points.Count-1)){
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