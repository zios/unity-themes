using UnityEngine;
namespace Zios{
	[ExecuteInEditMode][AddComponentMenu("Zios/Singleton/Overlay")]
	public class OverlaySettings : MonoBehaviour{
		public Vector2 defaultResolution = new Vector2(1280,720);
		public void Awake(){
			Overlay.defaultResolution = this.defaultResolution;
			Overlay.CalculateGUIMatrix();
		}
	}
}