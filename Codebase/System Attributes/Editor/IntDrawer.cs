using UnityEditor;
using UnityEngine;
namespace Zios.Editors{
	using Interface;
	public class IntDrawer : MaterialPropertyDrawer{
		public override void OnGUI(Rect position,MaterialProperty property,string label,MaterialEditor editor){
			Vector2 limits = property.rangeLimits;
			float value = property.floatValue;
			float labelSize = Screen.width * 0.345f;
			property.displayName.DrawLabel(position.SetWidth(labelSize));
			position = position.AddX(labelSize).AddWidth(-labelSize-69);
			if(limits != Vector2.zero){
				value = value.DrawSlider(position,limits.x,limits.y);
			}
			value = value.ToInt();
			value = value.Draw(position.AddX(position.width+5).SetWidth(64));
			property.floatValue = (float)value;
		}
	}
}