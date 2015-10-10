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
	public static bool force;
	public static Action writes = ()=>{};
	public static Action updates = ()=>{};
	public static bool IsBroken(Material material){
		if(material.shader.name.Contains("Hidden/InternalErrorShader")){
			foreach(string keyword in material.shaderKeywords){
				if(keyword.Contains("VARIABLE_MATERIAL_")){
					return true;
				}
			}
		}
		return false;
	}
	public static List<Material> GetAll(){
		var materialFiles = FileManager.FindAll("*.mat");
		var materials = new List<Material>();
		foreach(var file in materialFiles){
			var material = file.GetAsset<Material>();
			if(material.IsNull()){continue;}
			string editorName = material.shader.GetVariable<string>("customEditor");
			if(editorName == "VariableMaterialEditor" || VariableMaterial.IsBroken(material)){
				materials.Add(material);
			}
		}
		materials = materials.Distinct().ToList();
		return materials;
	}
	public static void Refresh(params UnityObject[] targets){
		VariableMaterial.Refresh(false,targets);
	}
	public static void Refresh(bool delay,params UnityObject[] targets){
		VariableMaterial.delay = delay;
		foreach(var target in targets){
			var material = (Material)target;
			bool isFlat = material.shader != null && material.shader.name.Contains("#");
			if(isFlat || VariableMaterial.IsBroken(material)){
				VariableMaterial.Flatten(target);
			}
		}
	}
	public static FileData GetParentShader(UnityObject target){
		var material = (Material)target;
		FileData file;
		if(material.shader.name.Contains("Hidden/InternalErrorShader")){
			foreach(string keyword in material.shaderKeywords){
				if(keyword.Contains("VARIABLE_MATERIAL_")){
					string shaderName = keyword.Split("_").Skip(2).Join("_").ToLower();
					file = FileManager.Find(shaderName+".shader",true,false);
					if(file.IsNull()){file = FileManager.Find(shaderName+".zshader",true,false);}
					if(file.IsNull()){Debug.LogWarning("[VariableMaterial] : Parent recovery shader missing : " + shaderName);}
					return file;
				}
			}
			Debug.LogWarning("[VariableMaterial] : Parent shader missing : " + material.name);
		}
		file = FileManager.Get(material.shader);
		if(!file.IsNull() && file.name.Contains("#")){
			string shaderName = file.name.Split("#")[0];
			file = FileManager.Find(shaderName+".shader",true,false);
			if(file.IsNull()){file = FileManager.Find(shaderName+".zshader",true,false);}
			if(file.IsNull()){Debug.LogWarning("[VariableMaterial] : Parent shader/zshader not found : " + shaderName);}
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
	public static void Flatten(bool force,params UnityObject[] targets){
		VariableMaterial.force = force;
		VariableMaterial.Flatten(targets);
	}
	public static void Flatten(params UnityObject[] targets){
		#if UNITY_EDITOR 
		string originalName = "";
		foreach(var target in targets){
			Material material = (Material)target;
			FileData shaderFile = VariableMaterial.GetParentShader(target);
			if(shaderFile.IsNull()){continue;}
			string timestamp = shaderFile.GetModifiedDate("MdyyHmmff");
			string hash = timestamp + "-" + material.shaderKeywords.Join(" ").ToMD5();
			string folderPath = shaderFile.folder+"/"+shaderFile.name+"/";
			string outputPath = folderPath+shaderFile.name+"#"+hash+".shader";
			Action update = ()=>{
				material.EnableKeyword("VARIABLE_MATERIAL_"+shaderFile.name.ToUpper());
				material.shader = FileManager.GetAsset<Shader>(outputPath);
				Utility.SetAssetDirty(material);
				if(VariableMaterial.debug){Debug.Log("[VariableMaterial] Shader set " + outputPath);}
			};
			if(!VariableMaterial.force && File.Exists(outputPath)){
				VariableMaterial.updates += update;
				continue;
			}
			originalName = shaderFile.fullName;
			string text = shaderFile.GetText();
			string shaderName = text.Parse("Shader ","{").Trim(' ','"');
			if(shaderName.Contains("#")){continue;}
			string output = "Shader " + '"' + "Hidden/"+shaderName+"#"+hash+'"'+"{\r\n";
			var allowed = new Stack<bool?>();
			int tabs = -1;
			text = text.Replace("\\\r\n","");
			foreach(string current in text.Split("\r\n").Skip(1)){
				if(current.IsEmpty()){continue;}
				string line = current;
				bool hideBlock = allowed.Count > 0 && allowed.Peek() != true;
				bool allowedBranch = line.ContainsAny("#else","#elif") && allowed.Peek() != null;
				bool ignoredBranch = line.Contains("@#");
				//if(line.ContainsAny("[KeywordEnum","[Toggle")){continue;}
				if(!ignoredBranch && line.Contains("#endif")){
					allowed.Pop();
					if(allowed.Count == 0){tabs = -1;}
					continue;
				}
				if(hideBlock && !allowedBranch){
					if(!ignoredBranch && line.ContainsAny("#if")){allowed.Push(null);}
					continue;
				}
				if(ignoredBranch){
					bool end = line.Contains("#end");
					bool include = line.Contains("#include");
					if(tabs < 0){tabs = line.Length - line.TrimStart().Length;}
					if(end){tabs -= 1;}
					line = new String('\t',tabs) + line.TrimStart();
					output += line.Replace("@#","#") + "\r\n";
					if(!end && !include){tabs += 1;}
					continue;
				}
				if(line.Contains("#include")){line = line.Replace("#include \"","#include \"../");}
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
					if(useBlock && allowed.Count == 1 && tabs <= 0){
						tabs = line.Length - line.TrimStart().Length;
					}
					continue;
				}
				if(tabs >= 1){
					if(line.Contains("}") && !line.Contains("{")){tabs -= 1;}
					line = new String('\t',tabs) + line.TrimStart();
					if(line.Contains("{") && !line.Contains("}")){tabs += 1;}
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
				Action write = ()=>{
					Directory.CreateDirectory(folderPath);
					File.WriteAllText(outputPath,output);
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
		VariableMaterial.force = false;
		#endif
	}
}
