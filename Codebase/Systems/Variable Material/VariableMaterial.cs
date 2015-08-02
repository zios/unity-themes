using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Zios;
using UnityObject = UnityEngine.Object;
public class VariableMaterial{
	public static bool debug;
	public static bool dirty;
	public static bool delay;
	public static Action writes;
	public static Action updates;
	public static void Refresh(params UnityObject[] targets){
		VariableMaterial.Refresh(false,targets);
	}
	public static void Refresh(bool delay,params UnityObject[] targets){
		VariableMaterial.delay = delay;
		foreach(var target in targets){
			var material = (Material)target;
			bool isFlat = material.shader != null && material.shader.name.Contains("#");
			if(isFlat){
				VariableMaterial.Flatten(target);
			}
		}
	}
	public static FileData GetParentShader(UnityObject target){
		var material = (Material)target;
		FileData file = FileManager.Get(material.shader);
		if(!file.IsNull() && file.name.Contains("#")){
			string realName = file.name.Split("#")[0];
			file = FileManager.Find(realName+".shader",true,false);
			if(file.IsNull()){file = FileManager.Find(realName+".zshader",true,false);}
			if(file.IsNull()){Debug.LogWarning("[VariableMaterial] : Parent shader/zshader not found : " + realName);}
		}
		return file;
	}
	public static void RefreshEditor(){
		if(VariableMaterial.updates.GetInvocationList().Length > 1){
			Utility.StartAssetEditing();
			VariableMaterial.writes();
			Utility.StopAssetEditing();
			//Utility.SaveAssets();
			Utility.RefreshAssets();
			FileManager.Refresh();
			VariableMaterial.updates();
		}
		VariableMaterial.updates = ()=>{};
		VariableMaterial.writes = ()=>{};
		VariableMaterial.dirty = true;
		VariableMaterial.delay = false;
		Utility.RebuildInspectors();
	}
	public static void Unflatten(params UnityObject[] targets){
		string shaderName = "";
		foreach(var target in targets){
			var material = (Material)target;
			FileData shaderFile = VariableMaterial.GetParentShader(target);
			if(shaderFile.IsNull()){continue;}
			shaderName = shaderFile.fullName;
			material.shader = shaderFile.GetAsset<Shader>();
		}
		VariableMaterial.dirty = true;
		if(VariableMaterial.debug){
			Debug.Log("[VariableMaterial] " + shaderName + " -- " + targets.Length + " Unflattened.");
		}
		Utility.RebuildInspectors();
	}
	public static void Flatten(params UnityObject[] targets){
		string originalName = "";
		foreach(var target in targets){
			Material material = (Material)target;
			FileData shaderFile = VariableMaterial.GetParentShader(target);
			if(shaderFile.IsNull()){continue;}
			originalName = shaderFile.fullName;
			string hash = shaderFile.GetModifiedDate("MdyyHmmff") + "-" + material.shaderKeywords.Join(" ").ToMD5();
			string text = shaderFile.GetText();
			string shaderName = text.Parse("Shader ","{").Trim(' ','"');
			if(shaderName.Contains("#")){continue;}
			string outputPath = shaderFile.folder+"/"+shaderFile.name+"#"+hash+".shader";
			string output = "Shader " + '"' + "Hidden/"+shaderName+"#"+hash+'"'+"{\r\n";
			var allowed = new Stack<bool?>();
			int tabs = -1;
			foreach(string current in text.Split("\r\n").Skip(1)){
				if(current.IsEmpty()){continue;}
				string line = current;
				bool hideBlock = allowed.Count > 0 && allowed.Peek() != true;
				bool allowedBranch = line.ContainsAny("#else","#elif") && allowed.Peek() != null;
				//if(line.ContainsAny("[KeywordEnum","[Toggle")){continue;}
				if(line.Contains("#endif")){
					allowed.Pop();
					if(allowed.Count == 0){tabs = -1;}
					continue;
				}
				if(hideBlock && !allowedBranch){
					if(line.ContainsAny("#if")){allowed.Push(null);}
					continue;
				}
				if(line.Contains("#pragma shader_feature")){continue;}
				if(line.Contains("#pragma multi_compile ")){continue;}
				if(line.ContainsAny("#if","#elif","#else")){
					bool useBlock = false;
					if(line.ContainsAny("#else","#elif")){
						bool lastAllowed = allowed.Pop() == true;
						if(lastAllowed){
							allowed.Push(null);
							continue;
						}
						useBlock = line.Contains("#else");
					}
					if(line.ContainsAny("#ifdef","#ifndef")){
						bool hasTerm = material.shaderKeywords.Contains(line.Trim().Split(" ").Last());
						useBlock = line.Contains("#ifndef") ? !hasTerm : hasTerm;
					}
					else if(line.Contains("defined")){
						string[] orBlocks = line.Trim().Trim("#if ").Trim("#elif ").Split("||");
						foreach(string orBlock in orBlocks){
							string[] andBlocks = orBlock.Split("&&");
							foreach(string andBlock in andBlocks){
								string term = andBlock.Parse("defined(",")");
								bool hasTerm = material.shaderKeywords.Contains(term);
								useBlock = andBlock.Contains("!") ? !hasTerm : hasTerm;
								if(!useBlock){break;}
							}
							if(useBlock){break;}
						}
					}
					allowed.Push(useBlock);
					if(useBlock && allowed.Count == 1){
						tabs = line.Length - line.TrimStart().Length;
					}
					continue;
				}
				if(tabs != -1){
					if(line.Contains("}")){tabs -= 1;}
					line = new String('\t',tabs) + line.TrimStart();
					if(line.Contains("{")){tabs += 1;}
				}
				output += line+"\r\n";
			}
			output = output.Replace("{\r\n\t\t\t}","{}");
			string pattern = output.Cut("{\r\n\t\t\t\treturn ",";\r\n\t\t\t");
			while(!pattern.IsEmpty()){
				string replace = pattern.Replace("\r\n","").Replace("\t","");
				output = output.ReplaceFirst(pattern,replace);
				pattern = output.Cut("{\r\n\t\t\t\treturn ",";\r\n\t\t\t");
			}
			if(output != text){
				Action write = ()=>File.WriteAllText(outputPath,output);
				Action update = ()=>{
					material.shader = FileManager.GetAsset<Shader>(outputPath);
					Utility.SetAssetDirty(material);
					if(VariableMaterial.debug){Debug.Log("[VariableMaterial] Shader set " + outputPath);}
				};
				VariableMaterial.writes += write;
				VariableMaterial.updates += update;
			}
		}
		if(VariableMaterial.debug){
			Debug.Log("[VariableMaterial] " + originalName + " -- " + targets.Length + " flattened.");
		}
		if(VariableMaterial.delay){
			Utility.EditorDelayCall(VariableMaterial.RefreshEditor,0.5f);
			return;
		}
		VariableMaterial.RefreshEditor();
	}
}
