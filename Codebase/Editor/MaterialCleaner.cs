using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Zios.Editors{
	using Event;
	public static class MaterialCleaner{
		public static bool changes;
		[MenuItem ("Zios/Material/Remove Unused Data (All)")]
		public static void Clean(){MaterialCleaner.Clean(null);}
		public static void Clean(FileData[] materials){
			MaterialCleaner.changes = false;
			FileData[] files = materials ?? FileManager.FindAll("*.mat");
			Events.AddStepper("On Editor Update",MaterialCleaner.Step,files,50);
		}
		public static void Step(object collection,int itemIndex){
			var materials = (FileData[])collection;
			var file = materials[itemIndex];
			bool last = itemIndex == materials.Length-1;
			EventStepper.title = "Updating " + materials.Length + " Materials";
			EventStepper.message = "Updating material : " + file.name;
			string text = file.GetText();
			string copy = text;
			int index = 0;
			bool changed = false;
			bool removePrevious = false;
			string guid = text.Parse("guid: ",",");
			string shaderPath = AssetDatabase.GUIDToAssetPath(guid);
			if(!shaderPath.IsEmpty()){
				Material material = file.GetAsset<Material>();
				Shader shader = FileManager.GetAsset<Shader>(shaderPath,false);
				if(shader != null){
					int propertyCount = ShaderUtil.GetPropertyCount(shader);
					Dictionary<string,string> properties = new Dictionary<string,string>();
					for(int propertyIndex=0;propertyIndex<propertyCount;++propertyIndex){
						string name = ShaderUtil.GetPropertyName(shader,propertyIndex);
						properties[name] = ShaderUtil.GetPropertyType(shader,propertyIndex).ToName();
					}
					string keywords = text.Parse("m_ShaderKeywords:","m_").Trim("[]");
					if(!keywords.IsEmpty()){
						string keywordsCleaned = keywords;
						foreach(string keyword in keywords.Replace("\n   ","").Split(" ")){
							if(!properties.ContainsKey(keyword.Split("_")[0],true)){
								keywordsCleaned = keywordsCleaned.Replace(" "+keyword,"");
								changed = true;
							}
						}
						copy = copy.Replace(keywords,keywordsCleaned);
					}
					while(true){
						int nextIndex = text.IndexOf("data:",index+5);
						if(removePrevious){
							int nextGroup = text.IndexOf("\n    m",index);
							int count = nextGroup != -1 && nextGroup < nextIndex ? nextGroup-index : nextIndex-index;
							string section = nextIndex < 0 ? text.Substring(index) : text.Substring(index,count);
							copy = copy.Replace(section,"");
							removePrevious = false;
							changed = true;
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
					if(changed){
						MaterialCleaner.changes = true;
						Debug.Log("[MaterialCleaner] : Cleaned unused serialized data " + file.fullName);
						file.WriteText(copy);
					}
				}
			}
			if(last){
				if(!MaterialCleaner.changes){Debug.Log("[MaterialCleaner] : All files already clean.");}
				else{Debug.Log("[MaterialCleaner] : Cleaned all materials.");}
				Utility.DelayCall(()=>AssetDatabase.Refresh(),1);
			}
		}
	}
}