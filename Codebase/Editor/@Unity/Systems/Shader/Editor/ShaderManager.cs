using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Zios.Unity.Editor.ShaderManager{
	using Zios.Events;
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	using Zios.File;
	using Zios.Supports.Stepper;
	using Zios.Unity.Editor.MonoBehaviourEditor;
	using Zios.Unity.Editor.VariableMaterial;
	using Zios.Unity.ShaderManager;
	//asm Zios.Shortcuts;
	//asm Zios.Unity.Editor.Inspectors;
	//asm Zios.Unity.Shortcuts;
	//asm Zios.Unity.Supports.Singleton;
	[CustomEditor(typeof(ShaderManager))]
	public class ShaderManagerEditor : MonoBehaviourEditor{
		public static List<Material> materials = new List<Material>();
		public static List<Material> materialsChanged = new List<Material>();
		public static bool keywordsChanged;
		public override void OnInspectorGUI(){
			this.title = "Shader";
			this.header = this.header ?? File.GetAsset<Texture2D>("ShaderIcon.png");
			base.OnInspectorGUI();
			var target = this.target.As<ShaderManager>();
			if(this.changed){target.Setup();}
		}
		[MenuItem("Zios/Settings/Shader")]
		public static void Select(){
			Selection.activeObject = ShaderManager.Get();
		}
		public void OnAwake(){
			var instance = ShaderManager.Get();
			ShaderManagerEditor.materials = VariableMaterial.GetAll();
			ShaderManagerEditor.keywordsChanged = false;
			ShaderManagerEditor.SetKeyword(instance.shadingBlend);
			ShaderManagerEditor.SetKeyword(instance.shadowType);
			ShaderManagerEditor.SetKeyword(instance.shadowMode);
			ShaderManagerEditor.SetKeyword(instance.shadowBlend);
			ShaderManagerEditor.SetKeyword(instance.lightmapType);
			ShaderManagerEditor.SetKeyword(instance.lightmapMode);
			ShaderManagerEditor.SetKeyword(instance.lightmapBlend);
			ShaderManagerEditor.SetKeyword(instance.fadeType);
			ShaderManagerEditor.SetKeyword(instance.fadeBlend);
			ShaderManagerEditor.SetKeyword(instance.fadeGrayscale);
			if(ShaderManagerEditor.keywordsChanged){
				Events.AddStepper("On Editor Update",ShaderManagerEditor.RefreshStep,ShaderManagerEditor.materialsChanged,50);
			}
		}
		public static void SetKeyword(Enum target){
			string typeName = target.GetType().Name.ToUpper()+"_";
			string targetKeyword = typeName+target.ToString().ToUpper();
			foreach(var material in ShaderManagerEditor.materials){
				foreach(var name in target.GetNames()){
					string keyword = typeName+name.ToUpper();
					if(keyword != targetKeyword && material.IsKeywordEnabled(keyword)){
						material.DisableKeyword(keyword);
					}
				}
				if(!material.IsKeywordEnabled(targetKeyword)){
					if(!ShaderManagerEditor.keywordsChanged){
						ShaderManagerEditor.materialsChanged.Clear();
						ShaderManagerEditor.keywordsChanged = true;
					}
					ShaderManagerEditor.materialsChanged.Add(material);
					material.EnableKeyword(targetKeyword);
				}
			}
		}
		public static void RefreshStep(object collection,int index){
			var materials = (List<Material>)collection;
			Stepper.title = "Updating " + materials.Count + " Materials";
			Stepper.message = "Updating material : " + materials[index].name;
			VariableMaterial.Refresh(true,materials[index]);
		}
	}
}