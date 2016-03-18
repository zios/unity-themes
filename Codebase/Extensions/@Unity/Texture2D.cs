using UnityEngine;
using System;
namespace Zios{
	public static class Texture2DExtensions{
		public static void SaveAs(this Texture2D current,string path,bool useBlit=false){
			if(useBlit){
				RenderTexture.active = new RenderTexture(current.width,current.height,0);
				Graphics.Blit(current,RenderTexture.active);
				current = new Texture2D(current.width,current.height);
				current.ReadPixels(new Rect(0,0,current.width,current.height),0,0);
			}
			FileManager.WriteFile(path,current.EncodeToPNG());
		}
	}
}