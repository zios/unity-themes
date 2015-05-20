using UnityEngine;
namespace Zios{
	[AddComponentMenu("Zios/Singleton/Shader")][ExecuteInEditMode]
	public class ShaderSettings : MonoBehaviour{
		public float globalAlphaCutoff = 0.3f;
		public void Awake(){
			ShaderManager.settings = this;
		}
		public void Update(){
			Events.Add("On Editor Update",ShaderManager.Update);
			ShaderManager.Update();
		}
	}
	public static class ShaderManager{
		public static ShaderSettings settings;
		public static void Update(){
			if(ShaderManager.settings != null){
				Shader.SetGlobalFloat("alphaCutoffGlobal",ShaderManager.settings.globalAlphaCutoff);
			}
            Shader.SetGlobalFloat("timeConstant",(Time.realtimeSinceStartup));
			#if UNITY_EDITOR
			if(UnityEditor.EditorPrefs.GetBool("ShaderSettings-AlwaysUpdate")){
				Utility.RepaintSceneView();
			}
			#endif
		}
	}
}