using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
namespace Zios.Interface{
	public class ThemeIconset{
		public static List<ThemeIconset> all = new List<ThemeIconset>();
		public string name;
		public string path;
		public List<ThemeContent> contents = new List<ThemeContent>();
		public static List<ThemeIconset> Import(){
			var imported = new List<ThemeIconset>();
			foreach(var path in Directory.GetDirectories(Theme.storagePath+"Iconsets")){
				imported.Add(ThemeIconset.Import(path));
			}
			return imported;
		}
		public static ThemeIconset Import(string path){
			path = path.Replace("\\","/");
			var iconset = new ThemeIconset();
			iconset.name = path.GetPathTerm();
			iconset.path = path;
			iconset.contents = ThemeContent.ImportDefaults(path);
			iconset.contents.AddRange(ThemeContent.Import(path));
			return iconset;
		}
		public void Apply(){this.Apply(true);}
		public void Apply(bool includeBuiltin){
			foreach(var content in this.contents){
				if(!includeBuiltin && content.builtin){continue;}
				content.Sync();
				content.target.text = content.value.text;
				content.target.tooltip = content.value.tooltip;
				content.target.image = content.value.image;
			}
		}
		public void Export(string savePath=null,bool split=true){
			savePath = savePath ?? this.path.GetDirectory();
			if(savePath.Length > 0){
				var contents = this.Serialize();
				if(split){
					foreach(var data in contents.Split("(")){
						var group = data.Parse("",")");
						var file = FileManager.Create(savePath+"/"+group+".guiContent");
						file.WriteText("("+group);
					}
				}
				else{
					var file = FileManager.Create(savePath+"/"+savePath.GetPathTerm()+".guiContent");
					file.WriteText(contents);
				}
				Theme.setup = false;
				Theme.loaded = false;
			}
		}
		public string Serialize(){
			var contents = "";
			foreach(var group in this.contents.Select(x=>x.targetPath).Distinct()){
				var targets = this.contents.Where(x=>x.targetPath==group);
				contents = contents.AddLine("("+group+")");
				foreach(var content in targets){
					contents = contents.AddLine(content.Serialize());
				}
				contents = contents.AddLine("");
			}
			return contents;
		}
	}
	public class ThemeContent{
		public string name;
		public string imageName;
		public object targetScope;
		public string targetPath;
		public ThemeIconset iconset;
		public bool builtin;
		public GUIContent target = new GUIContent();
		public GUIContent value = new GUIContent();
		public static List<ThemeContent> Import(string path){
			var imported = new List<ThemeContent>();
			foreach(var file in FileManager.FindAll(path+"/*.guiContent",false)){
				var contents = ThemeContent.DeserializeGroup(file.GetText());
				foreach(var content in contents){content.Setup(path);}
				imported.AddRange(contents);
			}
			return imported;
		}
		public static List<ThemeContent> ImportDefaults(string path){
			var imported = new List<ThemeContent>();
			var contents = typeof(EditorGUIUtility).GetVariable<Hashtable>("s_IconGUIContents");
			foreach(DictionaryEntry item in contents){
				var fileName = path+"/"+item.Value.As<GUIContent>().image.name+".png";
				if(FileManager.Exists(fileName)){
					var content = imported.AddNew();
					content.name = item.Key.As<string>();
					content.targetPath = "EditorGUIUtility.s_IconGUIContents";
					content.value = new GUIContent(item.Value.As<GUIContent>());
					content.value.image = FileManager.GetAsset<Texture2D>(fileName);
					content.builtin = true;
				}
			}
			return imported;
		}
		public static List<ThemeContent> DeserializeGroup(string data){
			var contents = new List<ThemeContent>();
			var content = new ThemeContent();
			var targetPath = "";
			foreach(var line in data.GetLines()){
				if(line.Trim().IsEmpty()){continue;}
				if(line.Contains("(")){targetPath = data.Parse("(",")");}
				if(line.ContainsAll("[","]")){
					content = contents.AddNew();
					content.targetPath = targetPath;
					content.name = line.Parse("[","]");
				}
				else{
					var term = line.Parse("","=").Trim();
					var value = line.Parse("=").Trim();
					if(term == "image"){content.imageName = value;}
					else if(term == "text"){content.value.text = value;}
					else if(term == "tooltip"){content.value.tooltip = value;}
				}
			}
			return contents;
		}
		public string Serialize(bool original=false){
			var contents = "".AddLine("["+this.name+"]");
			var target = original ? this.target : this.value;
			if(!this.value.text.IsEmpty()){contents = contents.AddLine("text = "+target.text);}
			if(!value.image.IsNull()){contents = contents.AddLine("image = "+target.image.name);}
			if(!value.tooltip.IsEmpty()){contents = contents.AddLine("tooltip = "+target.tooltip);}
			return contents;
		}

		public void Setup(string path){
			if(this.imageName.IsEmpty()){return;}
			this.value.image = FileManager.GetAsset<Texture2D>(path+"/"+this.imageName+".png");
			if(this.value.image.IsNull()){
				foreach(var texture in Locate.GetAssets<Texture2D>()){
					if(texture.name == this.imageName && AssetDatabase.GetAssetPath(texture).Contains("Library/unity")){
						this.value.image = texture;
						return;
					}
				}
			}
		}
		public void Sync(){
			string field = this.targetPath.Split(".").Last();
			string parent =  this.targetPath.Replace("."+field,"");
			var typeDirect = Utility.GetUnityType(this.targetPath);
			var typeParent = Utility.GetUnityType(parent);
			if(typeDirect.IsNull() && (typeParent.IsNull() || !typeParent.HasVariable(field))){
				if(Theme.debug){Debug.LogWarning("[Themes] No matching class/field found for GUIContent -- " + this.targetPath);}
				return;
			}
			this.targetScope = typeDirect ?? typeParent.InstanceVariable(field);
			if(this.targetScope.IsNull()){return;}
			if(this.targetScope.Is<GUIContent[]>() || this.targetScope.Is<Hashtable>() || this.targetScope.HasVariable(this.name)){
				this.target = this.targetScope.InstanceVariable(this.name).As<GUIContent>();
			}
		}
		[MenuItem("Zios/Theme/Development/Dump [GUIContent]")]
		public static void Dump(){ThemeContent.Dump(Theme.active);}
		public static void Dump(Theme theme){
			var path = theme.path.GetDirectory()+"/Dump/";
			FileManager.Create(path);
			foreach(var targetName in new string[2]{"s_IconGUIContents","s_TextGUIContents"}){
				var target = "UnityEditor.EditorGUIUtility."+targetName;
				var iconContents = typeof(EditorGUIUtility).GetVariable<Hashtable>(targetName);
				var contents = "".AddLine("("+target+")");
				foreach(DictionaryEntry item in iconContents){
					var content = new ThemeContent();
					content.name = item.Key.As<string>();
					content.value = item.Value.As<GUIContent>();
					if(!content.value.image.IsNull()){
						content.value.image.SaveAs(path+content.value.image.name+".png",true);
					}
					contents = contents.AddLine(content.Serialize());
				}
				FileManager.Create(path+target+".guiContent").WriteText(contents);
			}
		}
	}
}
