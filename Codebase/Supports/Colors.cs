using UnityEngine;
using System.Collections.Generic;
public static class Colors{
	public static Color[] numbers;
	public static Dictionary<string,Color> names;
	public static Color Get(int index){return Colors.numbers[index];}
	public static Color Get(string name){return Colors.names[name.ToLower()];}
	static Colors(){
		numbers = new Color[42];
		names = new Dictionary<string,Color>();
		//-------------
		// Base
		//-------------
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
		names["darkgray"] = numbers[10] = new Color(0.35f,0.35f,0.35f,1.00f);
		names["silver"]   = numbers[11] = new Color(0.80f,0.80f,0.80f,1.00f);
		names["violet"]   = numbers[12] = new Color(0.50f,0.00f,1.00f,1.00f);
		//-------------
		// Pastel
		//-------------
		names["pastelred"]    = names["chestnut"]   = numbers[13] = new Color(0.72f,0.32f,0.32f,1.00f);
		names["pastelgreen"]  = names["meadow"]     = numbers[14] = new Color(0.32f,0.72f,0.42f,1.00f);
		names["pastelblue"]   = names["steel"]      = numbers[15] = new Color(0.32f,0.42f,0.72f,1.00f);
		names["pastelyellow"] = names["celery"]     = numbers[16] = new Color(0.72f,0.72f,0.32f,1.00f);
		names["pastelorange"] = names["twine"]      = numbers[17] = new Color(0.72f,0.56f,0.32f,1.00f);
		names["pastelpurple"] = names["amethyst"]   = numbers[18] = new Color(0.72f,0.32f,0.72f,1.00f);
		names["pastelcyan"]   = names["fountain"]   = numbers[19] = new Color(0.32f,0.72f,0.56f,1.00f);
		//-------------
		// Dark
		//-------------
		names["darkred"]     = names["maroon"]      = numbers[20] = new Color(0.50f,0.00f,0.00f,1.00f);
		names["darkgreen"]   = names["laurel"]      = numbers[21] = new Color(0.00f,0.50f,0.00f,1.00f);
		names["darkblue"]    = names["navy"]        = numbers[22] = new Color(0.00f,0.00f,0.50f,1.00f);
		names["darkyellow"]  = names["olive"]       = numbers[23] = new Color(0.50f,0.50f,0.00f,1.00f);
		names["darkorange"]  = names["cinnamon"]    = numbers[24] = new Color(0.50f,0.25f,0.00f,1.00f);
		names["darkpurple"]  = names["indigo"]      = numbers[25] = new Color(0.25f,0.00f,0.50f,1.00f);
		names["darkcyan"]    = names["deapsea"]     = numbers[26] = new Color(0.00f,0.50f,0.50f,1.00f);
		//-------------
		// Light
		//-------------
		names["lightred"]    = names["tangerine"]  = numbers[27] = new Color(1.00f,0.50f,0.50f,1.00f);
		names["lightgreen"]  = names["mint"]       = numbers[28] = new Color(0.50f,1.00f,0.50f,1.00f);
		names["lightblue"]   = names["malibu"]     = numbers[29] = new Color(0.50f,0.50f,1.00f,1.00f);
		names["lightyellow"] = names["dolly"]      = numbers[30] = new Color(1.00f,1.00f,0.50f,1.00f);
		names["lightorange"] = names["macncheese"] = numbers[31] = new Color(1.00f,0.75f,0.50f,1.00f);
		names["lightpurple"] = names["blush"]      = numbers[32] = new Color(1.00f,0.50f,1.00f,1.00f);
		names["lightcyan"]   = names["anakiwa"]    = numbers[33] = new Color(0.50f,1.00f,1.00f,1.00f);
		names["pink"]        = names["lightpurple"];
		//-------------
		// Bold
		//-------------
		names["boldred"]    = names["coral"]       = numbers[34] = new Color(1.00f,0.25f,0.25f,1.00f);
		names["boldgreen"]  = names["earthbound"]  = numbers[35] = new Color(0.25f,1.00f,0.50f,1.00f);
		names["boldblue"]   = names["dodger"]      = numbers[36] = new Color(0.25f,0.50f,1.00f,1.00f);
		names["boldyellow"] = names["golden"]      = numbers[37] = new Color(1.00f,1.00f,0.25f,1.00f);
		names["boldorange"] = names["crusta"]      = numbers[38] = new Color(1.00f,0.50f,0.25f,1.00f);
		names["boldpurple"] = names["velectric"]   = numbers[39] = new Color(0.50f,0.25f,1.00f,1.00f);
		names["boldcyan"]   = names["aqua"]        = numbers[40] = new Color(0.25f,1.00f,1.00f,1.00f);
		names["boldpink"]   = names["strawberry"]  = numbers[41] = new Color(1.00f,0.25f,0.50f,1.00f);
	}
}