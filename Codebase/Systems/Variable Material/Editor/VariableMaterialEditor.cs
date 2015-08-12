using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Collections.Generic;
using Zios;
[CanEditMultipleObjects]
public class VariableMaterialEditor : MaterialEditor{
	public Material material;
	public Shader shader;
	public string hash;
	public FileData parent;
	public static List<Material> allMaterials = new List<Material>();
	public override void OnInspectorGUI(){
		this.material = (Material)this.target;
		bool matching = this.shader == this.material.shader;
		if(!matching || VariableMaterial.dirty){this.Reload();}
		if(this.shader != null){
			EditorGUILayout.BeginHorizontal();
			string[] keywords = this.material.shaderKeywords;
			bool isHook = this.shader.name.EndsWith("#");
			bool isFlat = this.shader.name.Contains("#") && !isHook;
			bool isUpdated = !isFlat || this.shader.name.Split("#")[1].Split(".")[0] == this.hash;
			GUI.enabled = !this.parent.IsNull() && (isHook || this.parent.extension != "zshader");
			if(isFlat && "Unflatten".DrawButton()){VariableMaterial.Unflatten(this.targets);}
			if(!isFlat && "Flatten".DrawButton()){VariableMaterial.Flatten(true,this.targets);}
			GUI.enabled = Event.current.shift || !isUpdated;
			if("Update".DrawButton()){
				VariableMaterial.force = true;
				var materials = this.targets.Cast<Material>().ToList();
				Events.AddStepper("On Editor Update",VariableMaterialEditor.RefreshStep,materials,50);
			}
			GUI.enabled = true;
			EditorGUILayout.EndHorizontal();
			base.OnInspectorGUI();
			if(isFlat && !keywords.SequenceEqual(this.material.shaderKeywords)){
				VariableMaterial.Refresh(this.target);
			}
		}
	}
	public void Reload(){
		this.parent = VariableMaterial.GetParentShader(this.target);
		if(!this.parent.IsNull()){
			this.hash = this.parent.GetModifiedDate("MdyyHmmff") + "-" + this.material.shaderKeywords.Join(" ").ToMD5();
		}
		VariableMaterial.dirty = false;
		this.shader = this.material.shader;
		this.Repaint();
	}
	[MenuItem("Zios/Process/Material/Refresh Variable Materials (Scene)")]
	public static void RefreshScene(){
		List<Material> materials = new List<Material>();
		var renderers = Locate.GetSceneComponents<Renderer>();
		foreach(var renderer in renderers){materials.AddRange(renderer.sharedMaterials);}
		Events.AddStepper("On Editor Update",VariableMaterialEditor.RefreshStep,materials,50);
	}
	[MenuItem("Zios/Process/Material/Refresh Variable Materials (All)")]
	public static void RefreshAll(){
		var materials = VariableMaterial.GetAll();
		Events.AddStepper("On Editor Update",VariableMaterialEditor.RefreshStep,materials,50);
	}
	public static void RefreshStep(object collection,int index){
		var materials = (List<Material>)collection;
		Events.stepperTitle = "Updating " + materials.Count + " Materials";
		Events.stepperMessage = "Updating material : " + materials[index].name;
		VariableMaterial.Refresh(true,materials[index]);
	}
}
