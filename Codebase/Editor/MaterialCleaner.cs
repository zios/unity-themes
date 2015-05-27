using UnityEngine;
using UnityEditor;
using System.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
namespace Zios{
    public static class MaterialCleaner{
        [MenuItem ("Zios/Process/Material Clean (All)")]
        public static void Clean(){MaterialCleaner.Clean(null);}
        public static void Clean(FileData[] materials){
			FileData[] files = materials ?? FileManager.FindAll("*.mat");
			AssetDatabase.StartAssetEditing();
			foreach(var file in files){
				string text = file.GetText();
				string copy = text;
				int index = 0;
				bool removePrevious = false;
				string guid = text.Cut("guid: ",",").Strip(",").Substring(6);
				string shaderPath = AssetDatabase.GUIDToAssetPath(guid);
				Material material = file.GetAsset<Material>();
				Shader shader = FileManager.GetAsset<Shader>(shaderPath,false);
				if(shader == null){continue;}
				int propertyCount = ShaderUtil.GetPropertyCount(shader);
				Dictionary<string,string> properties = new Dictionary<string,string>();
				for(int propertyIndex=0;propertyIndex<propertyCount;++propertyIndex){
					string name = ShaderUtil.GetPropertyName(shader,propertyIndex);
					properties[name] = ShaderUtil.GetPropertyType(shader,propertyIndex).ToName();
				}
				Debug.Log("[MaterialCleaner] : Cleaning unused serialized data -- " + file.fullName);
				while(true){
					int nextIndex = text.IndexOf("data:",index+5);
					if(removePrevious){
						int nextGroup = text.IndexOf("\n    m",index);
						int count = nextGroup != -1 && nextGroup < nextIndex ? nextGroup-index : nextIndex-index;
						string section = nextIndex < 0 ? text.Substring(index) : text.Substring(index,count);
						copy = copy.Replace(section,"");
						removePrevious = false;
					}
					if(nextIndex == -1){break;}
					index = nextIndex;
					int keywordStart = text.IndexOf("name: ",index)+6;
					int keywordEnd = text.IndexOf("\n",keywordStart);
					string name = text.Substring(keywordStart,keywordEnd-keywordStart);
					if(name.IsEmpty()){continue;}
					bool emptyTexture = properties.ContainsKey(name) && properties[name] == "Texture" && material.GetTexture(name) == null;
					removePrevious = !properties.ContainsKey(name) || emptyTexture;
					//if(removePrevious){Debug.Log("[MaterialCleaner] : Removing " + name + " from " + file.fullName);}
				}
				if(text != copy){
					file.WriteText(copy);
				}
			}
			AssetDatabase.StopAssetEditing();
			AssetDatabase.Refresh();
	    }
    }
}