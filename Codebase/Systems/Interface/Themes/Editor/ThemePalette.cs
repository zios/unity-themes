using System.Collections.Generic;
using System;
using System.Linq;
namespace Zios.Interface{
	using UnityEngine;
	using UnityEditor;
	[Serializable]
	public class ThemePalette{
		public static List<ThemePalette> all = new List<ThemePalette>();
		public string name;
		public Dictionary<Color,RelativeColor> swap = new Dictionary<Color,RelativeColor>();
		public Dictionary<string,RelativeColor> colors = new Dictionary<string,RelativeColor>(){{"Window","#B1B1B1"}};
		public static void Import(string path=null){
			path = path ?? "*.unitypalette";
			foreach(var file in FileManager.FindAll(path)){
				bool skipTexture = false;
				var palette = ThemePalette.all.AddNew();
				var sourceMap = new Dictionary<string,string>();
				palette.colors.Clear();
				palette.name = file.name;
				foreach(var line in file.GetText().GetLines()){
					if(line.Trim().IsEmpty()){continue;}
					if(line.Contains("[")){
						skipTexture = line.Contains("[No",true) || line.Contains("[Skip",true);
						continue;
					}
					var color = new RelativeColor().Deserialize(line);
					color.skipTexture = skipTexture;
					palette.colors[color.name] = color;
					sourceMap[color.name] = color.sourceName;
				}
				foreach(var item in sourceMap){
					if(item.Value.IsEmpty()){continue;}
					var source = palette.colors.Get(item.Value);
					palette.colors[item.Key].Assign(source);
				}
			}
		}
		public void Export(string path=""){
			path = path.IsEmpty() ? EditorUtility.SaveFilePanel("Save Theme [Palette]",Theme.storagePath+"@Palettes","TheColorsDuke","unitypalette") : path;
			if(path.Length > 0){
				var file = FileManager.Create(path);
				var contents = "";
				var textured = this.colors.Where(x=>!x.Value.skipTexture);
				var nonTextured = this.colors.Where(x=>x.Value.skipTexture);
				contents = contents.AddLine("[Textured]");
				foreach(var color in textured){
					contents = contents.AddLine(color.Value.Serialize());
				}
				contents = contents.AddLine("");
				contents = contents.AddLine("[NonTextured]");
				foreach(var color in nonTextured){
					contents = contents.AddLine(color.Value.Serialize());
				}
				file.WriteText(contents);
				EditorPrefs.SetString("EditorPalette",path.GetFileName());
				Theme.setup = false;
			}
		}
		public bool Has(string name){return this.colors.ContainsKey(name);}
		public Color Get(string name){
			if(this.Has(name)){return this.colors[name].value;}
			return Color.magenta;
		}
		public ThemePalette Use(ThemePalette other){
			this.name = other.name;
			this.colors.Clear();
			foreach(var item in other.colors){
				this.colors[item.Key] = other.colors[item.Key].Copy();
			}
			return this;
		}
		public bool Matches(ThemePalette other){
			foreach(var item in this.colors){
				var name = item.Key;
				bool mismatchedName = !other.colors.ContainsKey(name);
				bool mismatchedOriginal = mismatchedName || this.colors[name].original != other.colors[name].original;
				bool mismatchedOffset = mismatchedName || this.colors[name].offset != other.colors[name].offset;
				bool mismatchedSource = mismatchedName || this.colors[name].sourceName != other.colors[name].sourceName;
				if(mismatchedName || mismatchedOriginal || mismatchedSource || mismatchedOffset){
					return false;
				}
			}
			return true;
		}
		public void Build(){
			var active = new Color32(0,255,255,0);
			foreach(var color in this.colors){
				active.r += 1;
				this.swap[active] = color.Value.value;
			}
			active = new Color32(255,0,255,0);
			foreach(var color in this.colors.Where(x=>x.Value.skipTexture)){
				active.g += 1;
				this.swap[active] = color.Value.value;
			}
		}
		public void Apply(GUISkin skin){
			if(this.swap.Count < 1 && this.colors.Count > 0){this.Build();}
			var styles = skin.GetStyles();
			foreach(var style in styles){
				foreach(var state in style.GetStates()){
					foreach(var swap in this.swap){
						var color = swap.Value.value;
						if(state.textColor.Matches(swap.Key,false)){
							state.textColor = state.textColor.a == 0 ? color : new Color(color.r,color.g,color.b,state.textColor.a);
						}
					}
				}
			}
			foreach(var swap in this.swap){
				var color = swap.Value.value;
				var settings = skin.settings;
				if(settings.selectionColor.Matches(swap.Key,false)){
					settings.selectionColor = settings.selectionColor.a == 0 ? color : new Color(color.r,color.g,color.b,settings.selectionColor.a);
				}
				if(settings.cursorColor.Matches(swap.Key,false)){
					settings.cursorColor = settings.cursorColor.a == 0 ? color : new Color(color.r,color.g,color.b,settings.cursorColor.a);
				}
			}
		}
	}
}
