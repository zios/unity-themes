using UnityEngine;
using System;
namespace Zios{
	public static class TextureExtensions{
		public static void SaveAs(this Texture current,string path,bool useBlit=false){
			var texture = current is Texture2D ? (Texture2D)current : new Texture2D(1,1);
			if(useBlit){
				RenderTexture.active = new RenderTexture(current.width,current.height,0);
				Graphics.Blit(current,RenderTexture.active);
				texture = new Texture2D(current.width,current.height);
				texture.ReadPixels(new Rect(0,0,current.width,current.height),0,0);
			}
			FileManager.WriteFile(path,texture.EncodeToPNG());
		}
	}
}