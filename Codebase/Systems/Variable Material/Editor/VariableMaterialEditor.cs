using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using Zios;
[CanEditMultipleObjects]
public class VariableMaterialEditor : MaterialEditor{
	public Material material;
	public Shader shader;
	public string hash;
	public FileData parent;
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
			if(!isFlat && "Flatten".DrawButton()){VariableMaterial.Flatten(this.targets);}
			GUI.enabled = Event.current.shift || !isUpdated;
			if("Update".DrawButton()){VariableMaterial.Refresh(this.targets);}
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
		//VariableMaterial.shader = this.material.shader;
		this.Repaint();
	}
}
