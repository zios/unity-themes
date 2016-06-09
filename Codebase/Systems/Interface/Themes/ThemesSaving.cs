using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
namespace Zios{
	using Events;
	using Containers;
	#if UNITY_EDITOR
	using UnityEditor;
	public static partial class Themes{
		[NonSerialized] public static string createPath;
		[NonSerialized] public static bool includeBuiltin;
		[NonSerialized] public static Dictionary<string,object> styleGroupBuffer = new Dictionary<string,object>();
		[NonSerialized] public static Hierarchy<string,string,GUIContent> contentBuffer = new Hierarchy<string,string,GUIContent>();
		[MenuItem("Zios/Process/Theme/Development/Save All [GUISkin + GUIContent]")]
		public static void SaveGUIAll(){Themes.SaveGUI("",true);}
		[MenuItem("Zios/Process/Theme/Development/Save All [Assets]")]
		public static void SaveAssetsAll(){Themes.SaveAssets("",true);}
		[MenuItem("Zios/Process/Theme/Development/Save [GUISkin + GUIContent]")]
		public static void SaveGUI(){Themes.SaveGUI("");}
		public static void SaveGUI(string path,bool includeBuiltin=false){
			Themes.includeBuiltin = includeBuiltin;
			Themes.createPath = path.IsEmpty() ? EditorUtility.SaveFolderPanel("Save Theme [GUISkin/GUIContent]",Themes.storagePath,"@Default") : path;
			var allTypes = typeof(Editor).Assembly.GetTypes().Where(x=>!x.IsNull()).ToArray();
			Event.AddStepper("On Editor Update",Themes.SaveGUIStep,allTypes,50);
		}
		public static void SaveGUIStep(object collection,int itemIndex){
			var types = (Type[])collection;
			var type = types[itemIndex];
			if(!type.Name.ContainsAny("$","__Anon","<","AudioMixerDraw")){
				Event.stepperTitle = "Scanning " + types.Length + " Types";
				Event.stepperMessage = "Analyzing : " + type.Name;
				var terms = new string[]{"Styles","styles","s_GOStyles","s_Current","s_Styles","m_Styles","ms_Styles","constants"};
				foreach(var term in terms){
					if(!type.HasVariable(term,ObjectExtension.staticFlags)){continue;}
					try{
						var styleGroup = type.GetVariable(term,-1,ObjectExtension.staticFlags) ?? Activator.CreateInstance(type.GetVariableType(term));
						Themes.styleGroupBuffer[type.FullName+"."+term] = styleGroup;
					}
					catch{}
				}
				try{
					var styles = type.GetVariables<GUIStyle>(null,ObjectExtension.staticFlags);
					var content = type.GetVariables<GUIContent>(null,ObjectExtension.staticFlags);
					var contentGroups = type.GetVariables<GUIContent[]>(null,ObjectExtension.staticFlags);
					if(styles.Count > 0){Themes.styleGroupBuffer[type.FullName] = styles;}
					if(content.Count > 0){Themes.contentBuffer[type.FullName] = content;}
					foreach(var contentSet in contentGroups){
						if(contentSet.Value.IsNull() || contentSet.Value.Length < 1){continue;}
						var contents = Themes.contentBuffer[type.FullName+"."+contentSet.Key] = new Dictionary<string,GUIContent>();
						for(int index=0;index<contentSet.Value.Length;++index){
							contents[index.ToString()] = contentSet.Value[index];
						}
					}
				}
				catch{}
			}
			if(itemIndex >= types.Length-1){
				var savePath = Themes.createPath.GetAssetPath();
				var themeName = savePath.Split("/").Last();
				AssetDatabase.StartAssetEditing();
				EditorUtility.ClearProgressBar();
				foreach(var buffer in Themes.styleGroupBuffer){
					var skinPath = savePath+"/"+buffer.Key+".guiskin";
					var contentPath = savePath+"/"+buffer.Key+".guicontent";
					Themes.SaveGUISkin(skinPath,buffer);
					Themes.SaveGUIContent(contentPath,buffer.Value.GetVariables<GUIContent>());
				}
				foreach(var buffer in Themes.contentBuffer){
					var contentPath = savePath+"/"+buffer.Key+".guicontent";
					Themes.SaveGUIContent(contentPath,buffer.Value);
				}
				var skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
				skin = ScriptableObject.CreateInstance<GUISkin>().Use(skin);
				AssetDatabase.CreateAsset(skin,savePath+"/"+themeName+".guiskin");
				AssetDatabase.StopAssetEditing();
				Themes.styleGroupBuffer.Clear();
				Themes.contentBuffer.Clear();
			}
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
					if(Themes.includeBuiltin || !AssetDatabase.GetAssetPath(image).Contains("unity editor resources")){
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
		[MenuItem("Zios/Process/Theme/Development/Save [Palette]")]
		public static void SavePalette(){Themes.SavePalette("");}
		public static void SavePalette(string path){
			path = path.IsEmpty() ? EditorUtility.SaveFilePanel("Save Theme [Palette]",Themes.storagePath+"@Palettes","TheColorsDuke","unitypalette") : path;
			if(path.Length > 0){
				Themes.LoadColors();
				var palette = Themes.active.palette;
				var file = FileManager.Create(path);
				var contents = "";
				contents = contents.AddLine("Color "+palette.background.ToHex(false));
				contents = contents.AddLine("DarkColor "+palette.backgroundDark.value.Serialize());
				contents = contents.AddLine("LightColor "+palette.backgroundLight.value.Serialize());
				file.WriteText(contents);
				EditorPrefs.SetString("EditorPalette",path.GetFileName());
				FileManager.Refresh();
				Themes.setup = false;
			}
		}
		[MenuItem("Zios/Process/Theme/Development/Save [Assets]")]
		public static void SaveAssets(){Themes.SaveAssets("");}
		public static void SaveAssets(string path,bool includeBuiltin=false){
			path = path.IsEmpty() ? EditorUtility.SaveFolderPanel("Save Theme [Assets]",Themes.storagePath,"").GetAssetPath() : path;
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
	#endif
}