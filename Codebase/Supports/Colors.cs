using UnityEngine;
using System.Collections.Generic;
public static class Colors{
	public static Color[] numbers;
	public static Dictionary<string,Color> names;
	static Colors(){
		numbers = new Color[19];
		names = new Dictionary<string,Color>();
		names["black"]    = numbers[0]  = new Color(0.00f,0.00f,0.00f,1.00f);
		names["red"]      = numbers[1]  = new Color(1.00f,0.00f,0.00f,1.00f);
		names["green"]    = numbers[2]  = new Color(0.00f,1.00f,0.00f,1.00f);
		names["yellow"]   = numbers[3]  = new Color(1.00f,1.00f,0.00f,1.00f);
		names["blue"]     = numbers[4]  = new Color(0.00f,0.00f,1.00f,1.00f);
		names["cyan"]     = numbers[5]  = new Color(0.00f,1.00f,1.00f,1.00f);
		names["purple"]   = numbers[6]  = new Color(1.00f,0.00f,1.00f,1.00f);
		names["white"]    = numbers[7]  = new Color(1.00f,1.00f,1.00f,1.00f);
		names["orange"]   = numbers[8]  = new Color(1.00f,0.50f,0.00f,1.00f);
		names["gray"]     = numbers[9]  = new Color(0.50f,0.50f,0.50f,1.00f);
		names["silver"]   = numbers[10] = new Color(0.80f,0.80f,0.80f,1.00f);
		names["chestnut"] = numbers[11] = new Color(0.72f,0.32f,0.32f,1.00f);
		names["meadow"]   = numbers[12] = new Color(0.32f,0.72f,0.42f,1.00f);
		names["steel"]    = numbers[13] = new Color(0.32f,0.42f,0.72f,1.00f);
		names["cinnamon"] = numbers[14] = new Color(0.50f,0.25f,0.00f,1.00f);
		names["indigo"]   = numbers[15] = new Color(0.25f,0.50f,0.00f,1.00f);
		names["deapsea"]  = numbers[16] = new Color(0.00f,0.50f,0.50f,1.00f);
		names["violet"]   = numbers[17] = new Color(0.50f,0.00f,1.00f,1.00f);
		names["tundora"]  = numbers[18] = new Color(0.35f,0.35f,0.35f,1.00f);
	}
}