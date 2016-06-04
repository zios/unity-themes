using System;
using System.IO;
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
		[NonSerialized] public static Dictionary<string,object> styleGroupBuffer = new Dictionary<string,object>();
		[NonSerialized] public static Hierarchy<string,string,GUIContent> contentBuffer = new Hierarchy<string,string,GUIContent>();
		[MenuItem("Zios/Process/Theme/Save Current Theme")]
		public static void SaveCurrent(){
			Themes.createPath = EditorUtility.SaveFolderPanel("Export Theme",Themes.storagePath,"@Default");
			Themes.styleGroupBuffer.Clear();
			Themes.contentBuffer.Clear();
			var allTypes = typeof(UnityEditor.Editor).Assembly.GetTypes().Where(x=>!x.IsNull()).ToArray();
			Event.AddStepper("On Editor Update",Themes.SaveStep,allTypes,50);
		}
		public static void SaveStep(object collection,int itemIndex){
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
				FileManager.Create(savePath+"/GUIContent");
				FileManager.Create(savePath+"/Background");
				foreach(var buffer in Themes.styleGroupBuffer){
					var customStyles = new List<GUIStyle>();
					var skinPath = savePath+"/"+buffer.Key+".guiskin";
					var contentPath = savePath+"/"+buffer.Key+".guicontent";
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
						AssetDatabase.CreateAsset(newSkin,skinPath);
						newSkin.SaveFonts(savePath+"/Font");
						newSkin.SaveBackgrounds(savePath+"/Background");
						Themes.LocalizeSkin(newSkin,savePath);
					}
					Themes.SaveGUIContent(contentPath,buffer.Value.GetVariables<GUIContent>());
				}
				foreach(var buffer in Themes.contentBuffer){
					var contentPath = savePath+"/"+buffer.Key+".guicontent";
					Themes.SaveGUIContent(contentPath,buffer.Value);
				}
				var skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
				skin = ScriptableObject.CreateInstance<GUISkin>().Use(skin);
				skin.SaveFonts(savePath+"/Font");
				skin.SaveBackgrounds(savePath+"/Background");
				AssetDatabase.CreateAsset(skin,savePath+"/"+themeName+".guiskin");
				AssetDatabase.StopAssetEditing();
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
					if(!FileManager.Exists(imagePath)){
						image.SaveAs(imagePath,true);
					}
				}
				if(!value.tooltip.IsEmpty()){contents = contents.AddLine("tooltip = "+value.tooltip);}
				contents = contents.AddLine("");
			}
			FileManager.Create(path).WriteText(contents.Trim());
		}
		public static void SavePalette(){
			var path = EditorUtility.SaveFilePanel("Save Palette",Themes.storagePath+"@Palettes","TheColorsDuke","unitypalette");
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
		[MenuItem("Zios/Process/Theme/Localize Theme Assets")]
		public static void LocalizeAssets(){
			var path = EditorUtility.OpenFolderPanel("Localize Theme",Themes.storagePath,"").GetAssetPath();
			var files = FileManager.FindAll(path+"/*.guiSkin");
			FileManager.Create(path+"/Background");
			FileManager.Create(path+"/Font");
			AssetDatabase.StartAssetEditing();
			foreach(var file in files){
				var guiSkin = file.GetAsset<GUISkin>();
				guiSkin.SaveFonts(path+"/Font");
				guiSkin.SaveBackgrounds(path+"/Background",false);
			}
			AssetDatabase.StopAssetEditing();
			Utility.DelayCall(()=>{
				foreach(var file in files){
					var guiSkin = file.GetAsset<GUISkin>();
					Themes.LocalizeSkin(guiSkin,path);
					Utility.SetDirty(guiSkin,false,true);
				}
				AssetDatabase.SaveAssets();
			},1);
		}
		public static void LocalizeSkin(GUISkin skin,string path){
			string assetPath = "";
			foreach(var style in skin.GetStyles()){
				if(!style.font.IsNull()){
					assetPath = path+"/Font/"+style.font.name;
					var font = FileManager.GetAsset<Font>(assetPath+".ttf");
					font = font ?? FileManager.GetAsset<Font>(assetPath+".otf");
					style.font = font ?? style.font;
				}
				foreach(var state in style.GetStates()){
					if(state.background.IsNull()){continue;}
					if(AssetDatabase.GetAssetPath(state.background).Contains("unity editor resources")){continue;}
					assetPath = path+"/Background/"+state.background.name+".png";
					state.background = FileManager.GetAsset<Texture2D>(assetPath) ?? state.background;
				}
			}
		}
	}
	#endif
}