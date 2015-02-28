using UnityEngine;
namespace Zios{
    using System.Collections.Generic;
    public static class ColorExtension{
	    public static Color Random(this Color current,float intensity=1.0f){
		    int[] order = (new List<int>(){0,1,2}).Shuffle().ToArray();
		    float[] color = new float[3];
		    color[order[0]] = UnityEngine.Random.Range(intensity,1.0f);
		    color[order[1]] = UnityEngine.Random.Range(0,1.0f - intensity);
		    color[order[2]] = UnityEngine.Random.Range(0,1.0f);
		    return new Color(color[0],color[1],color[2]);
	    }
    }
}