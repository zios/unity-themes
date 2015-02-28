using UnityEngine;
namespace Zios{
	[AddComponentMenu("Zios/Singleton/Shader")]
	public class ShaderSettings : MonoBehaviour{
		public float globalAlphaCutoff = 0.3f;
		public void Awake(){
			ShaderManager.settings = this;
		}
		public void Update(){
			ShaderManager.Update();
		}
	}
	public static class ShaderManager{
		public static ShaderSettings settings;
		public static void Update(){
			Shader.SetGlobalFloat("alphaCutoffGlobal",ShaderManager.settings.globalAlphaCutoff);
            Shader.SetGlobalFloat("timeConstant", (Time.time * 0.1f));
		}
	}
}