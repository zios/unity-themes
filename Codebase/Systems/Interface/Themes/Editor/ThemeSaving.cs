using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
namespace Zios.Interface{
	using Events;
	using Containers;
	using UnityEditor;
	public partial class Theme{
		[NonSerialized] public static string createPath;
		[NonSerialized] public static bool includeBuiltin;
		[NonSerialized] public static Dictionary<string,object> styleGroupBuffer = new Dictionary<string,object>();
		[NonSerialized] public static Hierarchy<string,string,GUIContent> contentBuffer = new Hierarchy<string,string,GUIContent>();
		[MenuItem("Zios/Theme/Development/Save All [GUISkin + GUIContent]")]
		public static void SaveGUIAll(){Theme.SaveGUI("",true);}
		[MenuItem("Zios/Theme/Development/Save All [Assets]")]
		public static void SaveAssetsAll(){Theme.SaveAssets("",true);}
		[MenuItem("Zios/Theme/Development/Save [GUISkin + GUIContent]")]
		public static void SaveGUI(){Theme.SaveGUI("");}
		public static void SaveGUI(string path,bool includeBuiltin=false){
			Theme.includeBuiltin = includeBuiltin;
			Theme.createPath = path.IsEmpty() ? EditorUtility.SaveFolderPanel("Save Theme [GUISkin/GUIContent]",Theme.storagePath,"@Default") : path;
			var allTypes = typeof(Editor).Assembly.GetTypes().Where(x=>!x.IsNull()).ToArray();
			var stepper = new EventStepper(Theme.SaveGUIStep,Theme.SaveGUIComplete,allTypes,50);
			EditorApplication.update += stepper.Step;
		}
		public static void SaveGUIStep(object collection,int itemIndex){
			var types = (Type[])collection;
			var type = types[itemIndex];
			if(!type.Name.ContainsAny("$","__Anon","<","AudioMixerDraw")){
				EventStepper.title = "Scanning " + types.Length + " Types";
				EventStepper.message = "Analyzing : " + type.Name;
				var terms = new string[]{"Styles","styles","s_GOStyles","s_Current","s_Styles","m_Styles","ms_Styles","constants","s_Defaults"};
				foreach(var term in terms){
					if(!type.HasVariable(term,ObjectExtension.staticFlags)){continue;}
					try{
						var styleGroup = type.GetVariable(term,-1,ObjectExtension.staticFlags) ?? Activator.CreateInstance(type.GetVariableType(term));
						type.SetVariable(term,styleGroup);
						Theme.styleGroupBuffer[type.FullName+"."+term] = styleGroup;
					}
					catch{}
				}
				try{
					var styles = type.GetVariables<GUIStyle>(null,ObjectExtension.staticFlags);
					var content = type.GetVariables<GUIContent>(null,ObjectExtension.staticFlags);
					var contentGroups = type.GetVariables<GUIContent[]>(null,ObjectExtension.staticFlags);
					if(styles.Count > 0){Theme.styleGroupBuffer[type.FullName] = styles;}
					if(content.Count > 0){Theme.contentBuffer[type.FullName] = content;}
					foreach(var contentSet in contentGroups){
						if(contentSet.Value.IsNull() || contentSet.Value.Length < 1){continue;}
						var contents = Theme.contentBuffer[type.FullName+"."+contentSet.Key] = new Dictionary<string,GUIContent>();
						for(int index=0;index<contentSet.Value.Length;++index){
							contents[index.ToString()] = contentSet.Value[index];
						}
					}
				}
				catch{}
			}
		}
		public static void SaveGUIComplete(){
			var savePath = Theme.createPath.GetAssetPath();
			var themeName = savePath.Split("/").Last();
			AssetDatabase.StartAssetEditing();
			EditorUtility.ClearProgressBar();
			EditorApplication.update -= EventStepper.active.Step;
			foreach(var buffer in Theme.styleGroupBuffer){
				var skinPath = savePath+"/"+buffer.Key+".guiskin";
				var contentPath = savePath+"/"+buffer.Key+".guicontent";
				Theme.SaveGUISkin(skinPath,buffer);
				Theme.SaveGUIContent(contentPath,buffer.Value.GetVariables<GUIContent>());
			}
			foreach(var buffer in Theme.contentBuffer){
				var contentPath = savePath+"/"+buffer.Key+".guicontent";
				Theme.SaveGUIContent(contentPath,buffer.Value);
			}
			var skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
			skin = ScriptableObject.CreateInstance<GUISkin>().Use(skin);
			AssetDatabase.CreateAsset(skin,savePath+"/"+themeName+".guiskin");
			AssetDatabase.StopAssetEditing();
			Theme.styleGroupBuffer.Clear();
			Theme.contentBuffer.Clear();
		}
		public static void SaveGUISkin(string path,KeyValuePair<string,object> buffer){
			var customStyles = new List<GUIStyle>();
			var styles = buffer.Value is Dictionary<string,GUIStyle> ? (Dictionary<string,GUIStyle>)buffer.Value : buffer.Value.GetVariables<GUIStyle>().Distinct();
			foreach(var styleData in styles){
				var style = new GUIStyle(styleData.Value);
				if(!buffer.Key.Contains("s_Current")){style.Rename(styleData.Key);}
				customStyles.Add(style);
			}
			if(customStyles.Count > 0){
				GUISkin newSkin = ScriptableObject.CreateInstance<GUISkin>();
				newSkin.name = buffer.Key;
				newSkin.customStyles = customStyles.ToArray();
				AssetDatabase.CreateAsset(newSkin,path);
			}
		}
		public static void SaveGUIContent(string path,Dictionary<string,GUIContent> data){
			if(data.Count < 1){return;}
			var contents = "";
			var keys = data.Keys.ToList();
			keys.Sort();
			foreach(var key in keys){
				if(key.ContainsAny("<",">")){continue;}
				GUIContent value = data[key];
				contents = contents.AddLine("["+key+"]");
				if(value.IsNull()){continue;}
				if(!value.text.IsEmpty()){contents = contents.AddLine("text = "+value.text);}
				if(!value.image.IsNull()){
					var image = value.image;
					var imagePath = path.GetDirectory()+"/GUIContent/"+image.name+".png";
					contents = contents.AddLine("image = "+image.name);
					if(Theme.includeBuiltin || !FileManager.GetPath(image).Contains("unity editor resources")){
						if(!FileManager.Exists(imagePath)){
							image.SaveAs(imagePath,true);
						}
					}
				}
				if(!value.tooltip.IsEmpty()){contents = contents.AddLine("tooltip = "+value.tooltip);}
				contents = contents.AddLine("");
			}
			FileManager.Create(path).WriteText(contents.Trim());
		}
		[MenuItem("Zios/Theme/Development/Save [Palette]")]
		public static void SavePalette(){Theme.active.palette.Export("");}
		[MenuItem("Zios/Theme/Development/Save [Assets]")]
		public static void SaveAssets(){Theme.SaveAssets("");}
		public static void SaveAssets(string path,bool includeBuiltin=false){
			path = path.IsEmpty() ? EditorUtility.SaveFolderPanel("Save Theme [Assets]",Theme.storagePath,"").GetAssetPath() : path;
			var files = FileManager.FindAll(path+"/*.guiSkin");
			FileManager.Create(path+"/Background");
			FileManager.Create(path+"/Font");
			AssetDatabase.StartAssetEditing();
			foreach(var file in files){
				var guiSkin = file.GetAsset<GUISkin>();
				guiSkin.SaveFonts(path+"/Font",includeBuiltin);
				guiSkin.SaveBackgrounds(path+"/Background",includeBuiltin);
			}
			AssetDatabase.StopAssetEditing();
		}
	}
}