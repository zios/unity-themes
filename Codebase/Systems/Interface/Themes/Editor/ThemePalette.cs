using System.Collections.Generic;
using System;
using System.Linq;
namespace Zios.Interface{
	using Containers;
	using UnityEngine;
	using UnityEditor;
	[Serializable]
	public class ThemePalette{
		public static List<ThemePalette> all = new List<ThemePalette>();
		public string name;
		public string path;
		public Dictionary<Color,RelativeColor> swap = new Dictionary<Color,RelativeColor>();
		public Hierarchy<string,string,RelativeColor> colors = new Hierarchy<string,string,RelativeColor>(){{"*",new Dictionary<string,RelativeColor>(){{"Window","#C0C0C0"}}}};
		//=================================
		// Files
		//=================================
		public static List<ThemePalette> Import(string path=null){
			path = path ?? "*.unitypalette";
			var imported = new List<ThemePalette>();
			foreach(var file in FileManager.FindAll(path,false)){
				var active = imported.AddNew();
				active.name = file.name;
				active.path = file.path;
				active.Deserialize(file.GetText());
			}
			return imported;
		}
		public void Export(string path=null){
			var theme = Theme.active;
			var savePath = path ?? Theme.storagePath+"Palettes";
			var saveName = theme.palette.name+"-Variant";
			path = path.IsEmpty() ? EditorUtility.SaveFilePanel("Save Theme [Palette]",savePath,saveName,"unitypalette") : path;
			if(path.Length > 0){
				var file = FileManager.Create(path);
				file.WriteText(this.Serialize());
				EditorPrefs.SetString("EditorPalette"+Theme.suffix,path.GetFileName());
				Theme.setup = false;
				Theme.loaded = false;
			}
		}
		//=================================
		// Data
		//=================================
		public void Deserialize(string data){
			if(data.IsEmpty()){return;}
			bool skipTexture = false;
			var group = "Default";
			var sourceMap = new Dictionary<string,string>();
			this.colors.Clear();
			foreach(var line in data.GetLines()){
				if(line.Trim().IsEmpty()){continue;}
				if(line.Contains("(")){
					group = line.Parse("(",")");
					continue;
				}
				if(line.Contains("[")){
					group = "Default";
					skipTexture = line.Contains("[No",true) || line.Contains("[Skip",true);
					continue;
				}
				var color = new RelativeColor().Deserialize(line);
				color.skipTexture = skipTexture;
				this.colors.AddNew(group)[color.name] = color;
				this.colors.AddNew("*")[color.name] = color;
				sourceMap[color.name] = color.sourceName;
			}
			foreach(var item in sourceMap){
				if(item.Value.IsEmpty()){continue;}
				var source = this.colors["*"].Get(item.Value);
				this.colors["*"][item.Key].Assign(source);
			}
		}
		public string Serialize(){
			var contents = "";
			contents = contents.AddLine("[Textured]");
			var nameLength = this.colors["*"].Select(x=>x.Value).OrderByDescending(x=>x.name.Length).First().name.Length;
			var sourceLength = this.colors["*"].Select(x=>x.Value).OrderByDescending(x=>x.sourceName.Length).First().sourceName.Length;
			foreach(var item in this.colors.Where(x=>x.Key!="*")){
				var values = item.Value.Where(x=>!x.Value.skipTexture);
				if(values.Count() > 0){
					if(item.Key != "Default"){contents = contents.AddLine("("+item.Key+")");}
					foreach(var textured in values){
						contents = contents.AddLine("\t"+textured.Value.Serialize(nameLength,sourceLength));
					}
				}
			}
			contents = contents.AddLine("");
			contents = contents.AddLine("[NonTextured]");
			foreach(var item in this.colors.Where(x=>x.Key!="*")){
				var values = item.Value.Where(x=>x.Value.skipTexture);
				if(values.Count() > 0){
					if(item.Key != "Default"){contents = contents.AddLine("("+item.Key+")");}
					foreach(var untextured in values){
						contents = contents.AddLine("\t"+untextured.Value.Serialize(nameLength,sourceLength));
					}
				}
			}
			return contents;
		}
		//=================================
		// Utility
		//=================================
		public bool Has(string name){return this.colors["*"].ContainsKey(name);}
		public Color Get(string name){
			if(this.Has(name)){return this.colors["*"][name].value;}
			return Color.magenta;
		}
		public ThemePalette Use(ThemePalette other){
			this.name = other.name;
			this.path = other.path;
			this.colors.Clear();
			foreach(var group in other.colors){
				foreach(var color in group.Value){
					this.colors.AddNew(group.Key)[color.Key] = other.colors[group.Key][color.Key].Copy();
				}
			}
			return this;
		}
		public bool Matches(ThemePalette other){
			foreach(var item in this.colors["*"]){
				var name = item.Key;
				bool mismatchedName = !other.colors["*"].ContainsKey(name);
				bool mismatchedOriginal = mismatchedName || this.colors["*"][name].original != other.colors["*"][name].original;
				bool mismatchedOffset = mismatchedName || this.colors["*"][name].offset != other.colors["*"][name].offset;
				bool mismatchedSource = mismatchedName || this.colors["*"][name].sourceName != other.colors["*"][name].sourceName;
				if(mismatchedName || mismatchedOriginal || mismatchedSource || mismatchedOffset){
					return false;
				}
			}
			return true;
		}
		public void Build(){
			var active = new Color32(0,255,255,0);
			foreach(var color in this.colors["*"]){
				active.r += 1;
				this.swap[active] = color.Value.value;
			}
			active = new Color32(255,0,255,0);
			foreach(var color in this.colors.Values.ElementAt(2)){
				active.g += 1;
				this.swap[active] = color.Value.value;
			}
			active = new Color32(255,255,0,0);
			foreach(var color in this.colors.Values.ElementAt(3)){
				active.b += 1;
				this.swap[active] = color.Value.value;
			}
		}
		//=================================
		// Dynamics
		//=================================
		public void Apply(GUISkin skin){
			if(this.swap.Count < 1){this.Build();}
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
		public void ApplyTexture(string path,Texture2D texture){
			var name = path.GetPathTerm().TrimLeft("#");
			var isSplat = name.StartsWith("!");
			var parts = name.TrimLeft("!").Split("-");
			var offsetX = isSplat && parts[0].IsNumber() ? parts[0].ToInt() : -1;
			var offsetY = isSplat && parts[1].IsNumber() ? parts[1].ToInt() : -1;
			var offsetZ = isSplat && parts[2].IsNumber() ? parts[2].ToInt() : -1;
			var swapA = this.swap.ElementAtOrDefault(offsetX-1);
			var swapB = this.swap.ElementAtOrDefault(offsetY-1);
			var swapC = this.swap.ElementAtOrDefault(offsetZ-1);
			var colorA = !isSplat || swapA.IsNull() ? Color.clear : swapA.Value.value;
			var colorB = !isSplat ||swapB.IsNull() ? Color.clear : swapB.Value.value;
			var colorC = !isSplat ||swapC.IsNull() ? Color.clear : swapC.Value.value;
			if(isSplat && (swapA.IsNull() || swapB.IsNull() || swapC.IsNull())){
				Debug.Log("[ThemePallete] : Invalid splat texture offset -- " + path + " -- " + offsetX + " -- " + offsetY + " -- " + offsetZ);
				return;
			}
			name = isSplat ? parts.Skip(3).Join("-") : parts.Join("-");
			var original = FileManager.Find(path.GetDirectory().GetDirectory()+"/"+name);
			if(original.IsNull()){return;}
			int index = 0;
			var pixels = texture.GetPixels();
			foreach(var pixel in pixels.Copy()){
				if(isSplat){
					var splatA = Color.clear.Lerp(colorA,pixel.r);
					var splatB = Color.clear.Lerp(colorB,pixel.g);
					var splatC = Color.clear.Lerp(colorC,pixel.b);
					pixels[index] = splatA + splatB + splatC;
					pixels[index].a *= pixel.a;
					index += 1;
					continue;
				}
				foreach(var swap in this.swap){
					if(pixel.Matches(swap.Key,false)){
						var color = swap.Value.value;
						color.a *= pixel.a;
						pixels[index] = color;
					}
				}
				index += 1;
			}
			var originalImage = original.GetAsset<Texture2D>();
			if(pixels.Length != originalImage.GetPixels().Length){
				Debug.LogWarning("[ThemePalette] : Dynamic texture and source are different sizes -- " + name);
				return;
			}
			originalImage.SetPixels(pixels);
			originalImage.Apply();
		}
	}
}
