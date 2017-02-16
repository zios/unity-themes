using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;
namespace Zios{
	public static class AnimationCurveExtension{
		public static string Serialize(this AnimationCurve current){
			var output = new StringBuilder();
			foreach(var key in current.keys){
				output.Append(key.time);
				output.Append("-");
				output.Append(key.value);
				output.Append("-");
				output.Append(key.inTangent);
				output.Append("-");
				output.Append(key.outTangent);
				output.Append("|");
			}
			return output.ToString().TrimRight("|");
		}
		public static AnimationCurve Deserialize(this AnimationCurve current,string value){
			var keys = new List<Keyframe>();
			foreach(var keyData in value.Split("|")){
				var data = keyData.Split("-").ConvertAll<float>();
				keys.Add(new Keyframe(data[0],data[1],data[2],data[3]));
			}
			current.keys = keys.ToArray();
			return current;
		}
	}
}