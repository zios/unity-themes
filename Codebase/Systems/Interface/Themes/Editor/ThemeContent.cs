using System;
using System.Linq;
using UnityEngine;
namespace Zios.Interface{
	using UnityEditor;
	public class ThemeContent{
		public string name;
		public string path;
		public object scope;
		public GUIContent target = new GUIContent();
		public GUIContent value = new GUIContent();
		public static void Parse(Theme theme,string path){
			if(theme.name == "@Default"){return;}
			var contents = FileManager.FindAll(path+"/*.guiContent",true,false);
			foreach(var contentFile in contents){
				var content = new ThemeContent();
				var contentName = "";
				foreach(var line in contentFile.GetText().GetLines()){
					if(line.Trim().IsEmpty()){continue;}
					if(line.ContainsAll("[","]")){
						if(!contentName.IsEmpty()){
							content.Setup(contentFile.name,contentName);
							theme.contents.Add(content);
						}
						content = new ThemeContent();
						contentName = line.Parse("[","]");
					}
					else{
						var term = line.Parse("","=").Trim();
						var value = line.Parse("=").Trim();
						if(term == "image"){content.value.image = FileManager.GetAsset<Texture2D>(path+"/GUIContent/"+value+".png");}
						else if(term == "text"){content.value.text = value;}
						else if(term == "tooltip"){content.value.tooltip = value;}
					}
				}
				if(!contentName.IsEmpty()){
					content.Setup(contentFile.name,contentName);
					theme.contents.Add(content);
				}
			}
		}
		public void Setup(string path,string contentName){
			this.path = path;
			this.name = contentName;
		}
		public void SyncScope(){
			string field = this.path.Split(".").Last();
			string parent =  this.path.Replace("."+field,"");
			var typeDirect = Utility.GetUnityType(this.path);
			var typeParent = Utility.GetUnityType(parent);
			if(typeDirect.IsNull() && (typeParent.IsNull() || !typeParent.HasVariable(field))){
				if(Theme.debug){
					Debug.LogWarning("[Themes] No matching class/field found for GUIContent -- " + this.path);}
				return;
			}
			this.scope = typeDirect ?? typeParent.GetVariable(field);
			if(this.scope.IsNull()){
				try{
					this.scope = Activator.CreateInstance(typeParent.GetVariableType(field));
					typeParent.SetVariable(field,this.scope);
				}
				catch{}
			}
		}
		public void SyncTarget(){
			if(this.scope.IsNull()){return;}
			bool isArray = this.scope.GetType() == typeof(GUIContent[]);
			if(isArray || this.scope.HasVariable(this.name)){
				this.target = this.scope.GetVariable<GUIContent>(this.name);
				if(this.target.IsNull()){
					this.target = new GUIContent();
					this.scope.SetVariable(this.name,this.target);
				}
			}
		}
		public static void Apply(){
			if(Theme.active.IsNull()){return;}
			ThemeContent.Revert();
			Utility.DelayCall(()=>{
				foreach(var content in Theme.active.contents){
					content.SyncScope();
					content.SyncTarget();
					content.target.text = content.value.text;
					content.target.tooltip = content.value.tooltip;
					content.target.image = content.value.image;
				}
			},0.5f);
		}
		public static void Revert(){}
	}
}
