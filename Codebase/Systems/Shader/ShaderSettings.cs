using UnityEngine;
namespace Zios{
	[AddComponentMenu("Zios/Singleton/Shader")]
	public class ShaderSettings : MonoBehaviour{
		public float globalAlphaCutoff = 0.3f;
		public void Awake(){
			Shader.settings = this;
		}
		public void Update(){
			Shader.Update();
		}
	}
	public static class Shader{
		public static ShaderSettings settings;
		public static void Update(){
			UnityEngine.Shader.SetGlobalFloat("alphaCutoffGlobal",Shader.settings.globalAlphaCutoff);
            UnityEngine.Shader.SetGlobalFloat("timeConstant", (Time.time * 0.1f));
		}
	}
}