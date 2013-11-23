using System;
using System.Collections.Generic;
public static class Store{
	public static float PackFloats(params float[] values){
		int packed = 0;
		int amount = values.Length;
		int bitPrecision = 24 / amount;
		int intPrecision = (1<<bitPrecision) - 1;
		int slot = 0;
		for(int index=amount;index>0;--index){
			int shift = bitPrecision * (index - 1);
			packed |= ((int)Math.Floor(values[slot] * (float)intPrecision))<<shift;
			++slot;
		}
		return (float)packed * 0.0000001f;
	}
	public static float[] UnpackFloats(int amount,float value){
		value = value * 10000000;
		int bitPrecision = 24 / amount;
		int intPrecision = 1<<bitPrecision;
		float floatPrecision = (float)intPrecision;
		List<float> unpacked = new List<float>();
		for(int index=amount;index>0;--index){
			float slot = (float)((value / Math.Pow(intPrecision,index - 1)) % floatPrecision) / floatPrecision;
			unpacked.Add(slot);
		}
		return unpacked.ToArray();
	}
	public static float PackFloat4(float a,float b,float c,float d){
		int x = ((int)Math.Floor(a * 63))<<18;
		int y = ((int)Math.Floor(b * 63))<<12;
		int z = ((int)Math.Floor(c * 63))<<6;
		int w = ((int)Math.Floor(d * 63));
		return (x | y | z | w) * 0.0000001f;
	}
}