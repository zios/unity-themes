using System;
using UnityEngine;
using UnityEditor;
namespace Zios.UI{
    public class IntDrawer : MaterialPropertyDrawer{
	    public override void OnGUI(Rect position,MaterialProperty property,string label,MaterialEditor editor){
			Vector2 limits = property.rangeLimits;
			int value = (int)property.floatValue;
			float labelSize = Screen.width * 0.345f;
			property.displayName.DrawLabel(position.SetWidth(labelSize));
			position = position.AddX(labelSize).AddWidth(-labelSize);
			if(limits != Vector2.zero){
				value = value.DrawSlider(position,(int)limits.x,(int)limits.y);
			}
			else{
				value = value.DrawInt(position);
			}
			property.floatValue = (float)value;
		}
	}
}
