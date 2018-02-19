using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Zios.Unity.Editor.Menus{
	using Zios.Events;
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	using Zios.File;
	using Zios.Supports.Stepper;
	using Zios.Unity.Call;
	using Zios.Unity.ProxyEditor;
	using Zios.Unity.Log;
	//asm Zios.Shortcuts;
	//asm Zios.Unity.Shortcuts;
	public static class MaterialCleaner{
		public static bool changes;
		[MenuItem ("Zios/Material/Remove Unused Data (All)")]
		public static void Clean(){MaterialCleaner.Clean(null);}
		public static void Clean(FileData[] materials){
			MaterialCleaner.changes = false;
			FileData[] files = materials ?? File.FindAll("*.mat");
			Events.AddStepper("On Editor Update",MaterialCleaner.Step,files,50);
		}
		public static void Step(object collection,int itemIndex){
			var materials = (FileData[])collection;
			var file = materials[itemIndex];
			bool last = itemIndex == materials.Length-1;
			Stepper.title = "Updating " + materials.Length + " Materials";
			Stepper.message = "Updating material : " + file.name;
			string text = file.GetText();
			string copy = text;
			int index = 0;
			bool changed = false;
			bool removePrevious = false;
			string guid = text.Parse("guid: ",",");
			string shaderPath = ProxyEditor.GetAssetPath(guid);
			if(!shaderPath.IsEmpty()){
				Material material = file.GetAsset<Material>();
				Shader shader = File.GetAsset<Shader>(shaderPath,false);
				if(shader != null){
					int propertyCount = ProxyEditor.GetPropertyCount(shader);
					Dictionary<string,string> properties = new Dictionary<string,string>();
					for(int propertyIndex=0;propertyIndex<propertyCount;++propertyIndex){
						string name = ProxyEditor.GetPropertyName(shader,propertyIndex);
						properties[name] = ProxyEditor.GetPropertyType(shader,propertyIndex).ToName();
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
						//if(removePrevious){Log.Show("[MaterialCleaner] : Removing " + name + " from " + file.fullName);}
					}
					if(changed){
						MaterialCleaner.changes = true;
						Log.Show("[MaterialCleaner] : Cleaned unused serialized data " + file.fullName);
						file.WriteText(copy);
					}
				}
			}
			if(last){
				if(!MaterialCleaner.changes){Log.Show("[MaterialCleaner] : All files already clean.");}
				else{Log.Show("[MaterialCleaner] : Cleaned all materials.");}
				Call.Delay(()=>ProxyEditor.RefreshAssets(),1);
			}
		}
	}
}