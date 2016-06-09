using System;
using System.Linq;
using UnityEngine;
namespace Zios{
	#if UNITY_EDITOR
	using UnityEditor;
	public static partial class Themes{
		public static GUIStyle[] ReflectStyles(string path,bool showWarnings=true){
			var empty = new GUIStyle[0];
			var fieldName = path.Split(".").Last();
			var fieldFlags = fieldName.Contains("s_Current") ? ObjectExtension.privateFlags : ObjectExtension.staticFlags;
			var typeStatic = Utility.GetUnityType(path);
			var typeInstance = Utility.GetUnityType(path.Replace("."+fieldName,""));
			if(!typeStatic.IsNull()){
				return typeStatic.GetValues<GUIStyle>(null,fieldFlags);
			}
			if(!typeInstance.IsNull()){
				var target = typeInstance.GetVariable(fieldName);
				if(target.IsNull()){
					try{target = Activator.CreateInstance(typeInstance.GetVariableType(fieldName));}
					catch{return empty;}
				}
				return target.GetValues<GUIStyle>();
			}
			if(showWarnings){Debug.LogWarning("[Themes] No matching class/field found for GUISkin -- " + path + ". Possible version conflict.");}
			return empty;
		}
		[MenuItem("Zios/Process/Theme/Development/Sync Names [GUISkin]")]
		public static void SyncSkins(){Themes.SyncSkins("");}
		public static void SyncSkins(string path=""){
			path = path.IsEmpty() ? EditorUtility.SaveFolderPanel("Sync Names [GUISkin]",Themes.storagePath,"").GetAssetPath() : path;
			var files = FileManager.FindAll(path+"/*.guiSkin");
			foreach(var file in files){
				var stylesSkin = file.GetAsset<GUISkin>().customStyles;
				var stylesInternal = file.name.Contains(".") ? Themes.ReflectStyles(file.name) : EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector).customStyles;
				if(stylesSkin.Length == stylesInternal.Length){
					for(int index=0;index<stylesSkin.Length;++index){
						if(stylesSkin[index].name != stylesInternal[index].name){
							Debug.Log("[Themes] Fixed style name in " + file.name + ". Was " + stylesSkin[index].name + ". Now " + stylesInternal[index].name);
							stylesSkin[index].name = stylesInternal[index].name;
						}
					}
					Utility.SetAssetDirty(file.GetAsset<GUISkin>());
					continue;
				}
				Debug.LogWarning("[Themes] Mismatched number of styles -- " + file.name + ". Found " + stylesSkin.Length + ", but expected " + stylesInternal.Length + ". Possible version conflict.");
			}
		}
		[MenuItem("Zios/Process/Theme/Development/Localize [Assets]")]
		public static void LocalizeAssets(){Themes.LocalizeAssets("");}
		public static void LocalizeAssets(string path="",bool includeBuiltin=false){
			path = path.IsEmpty() ? EditorUtility.SaveFolderPanel("Localize Theme [Assets]",Themes.storagePath,"").GetAssetPath() : path;
			var files = FileManager.FindAll(path+"/*.guiSkin");
			foreach(var file in files){
				string assetPath = "";
				var skin = file.GetAsset<GUISkin>();
				foreach(var style in skin.GetStyles()){
					if(!style.font.IsNull()){
						assetPath = path+"/Font/"+style.font.name;
						if(!includeBuiltin && AssetDatabase.GetAssetPath(style.font).Contains("unity editor resources")){continue;}
						var font = FileManager.GetAsset<Font>(assetPath+".ttf",false);
						font = font ?? FileManager.GetAsset<Font>(assetPath+".otf",false);
						style.font = font ?? style.font;
					}
					foreach(var state in style.GetStates()){
						if(state.background.IsNull()){continue;}
						if(!includeBuiltin && AssetDatabase.GetAssetPath(state.background).Contains("unity editor resources")){continue;}
						assetPath = path+"/Background/"+state.background.name+".png";
						state.background = FileManager.GetAsset<Texture2D>(assetPath) ?? state.background;
					}
				}
				Utility.SetDirty(skin,false,true);
			}
			AssetDatabase.SaveAssets();
		}
	}
	#endif
}