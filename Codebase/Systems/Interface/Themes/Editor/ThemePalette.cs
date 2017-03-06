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
		public bool usesSystem;
		public Dictionary<Color,RelativeColor> swap = new Dictionary<Color,RelativeColor>();
		public Hierarchy<string,string,RelativeColor> colors = new Hierarchy<string,string,RelativeColor>(){{"*",new Dictionary<string,RelativeColor>(){{"Window","#C0C0C0"}}}};
		//=================================
		// Files
		//=================================
		public static List<ThemePalette> Import(string path=null){
			path = path ?? "*.unitypalette";
			var imported = new List<ThemePalette>();
			foreach(var file in FileManager.FindAll(path,Theme.debug)){
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
			path = path.IsEmpty() ? EditorUtility.SaveFilePanel("Save Theme [Palette]",savePath.GetAssetPath(),saveName,"unitypalette") : path;
			if(path.Length > 0){
				var file = FileManager.Create(path);
				file.WriteText(this.Serialize());
				AssetDatabase.ImportAsset(path.GetAssetPath());
				Utility.SetPref<string>("EditorPalette"+Theme.suffix,path.GetFileName());
				Theme.Reset(true);
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
			RelativeColor.UpdateSystem();
			foreach(var item in sourceMap){
				if(item.Value.IsEmpty()){continue;}
				this.colors["*"][item.Key].Assign(this,item.Value);
				if(this.colors["*"][item.Key].source == RelativeColor.system){
					this.usesSystem = true;
				}
			}
			foreach(var color in this.colors["*"]){
				color.Value.ApplyOffset();
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
				if(!other.colors["*"].ContainsKey(name)){return false;}
				var colorA = this.colors["*"][name];
				var colorB = other.colors["*"][name];
				var isBlended = colorA.blendMode != ColorBlend.Normal;
				var isSystem = colorA.source == RelativeColor.system;
				bool mismatchedValue = !isSystem && !isBlended && (colorA.value != colorB.value);
				bool mismatchedBlend = isBlended && (colorA.blendMode.ToInt() != colorB.blendMode.ToInt());
				bool mismatchedOffset = colorA.offset != colorB.offset;
				bool mismatchedSource = colorA.sourceName != colorB.sourceName;
				if(mismatchedBlend || mismatchedValue || mismatchedSource || mismatchedOffset){
					return false;
				}
			}
			return true;
		}
		public void Build(){
			if(this.colors.Values.Count < 3){
				Debug.LogWarning("[ThemePalette] Colors attempted build before initialized.");
				return;
			}
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
		public Color ParseColor(string term){
			var index = term.Remove("S","O","I").Split("A").First();
			var offset = index.IsNumber() ? index.ToInt() : -1;
			var swap = this.swap.ElementAtOrDefault(offset-1);
			var current = swap.IsNull() || offset == -1 ? Color.clear : swap.Value.value;
			if(term.StartsWith("C")){current = Color.clear;}
			if(term.StartsWith("W")){current = Color.white;}
			if(term.StartsWith("B")){current = Color.black;}
			if(current != Color.clear){
				if(term.StartsWith("S")){current = current.GetIntensity() < 0.33f ? Color.black : Color.white;}
				if(term.StartsWith("O")){current = current.GetIntensity() < 0.33f ? Color.white : Color.black;}
				if(term.StartsWith("I")){current = current.Invert();}
				if(term.Contains("A")){
					if(term.Split("A")[1].IsEmpty()){current.a = 1;}
					else{current.a *= term.Split("A")[1].ToFloat() / 10.0f;}
				}
			}
			return current;
		}
		public void ApplyTexture(string path,Texture2D texture,bool writeToDisk=false){
			if(texture.IsNull()){return;}
			var name = path.GetPathTerm().TrimLeft("#");
			var ignoreAlpha = name.StartsWith("A-");
			var isSplat = name.StartsWith("!");
			var parts = name.TrimLeft("!","A-").Split("-");
			if(isSplat && parts.Length < 2){
				Debug.LogWarning("[ThemePalette] : Improperly formed splat texture -- " + path.GetPathTerm());
				return;
			}
			var colorA = isSplat ? this.ParseColor(parts[0]) : Color.clear;
			var colorB = isSplat ? this.ParseColor(parts[1]) : Color.clear;
			var colorC = isSplat ? this.ParseColor(parts[2]) : Color.clear;
			if(isSplat){
				parts = parts.Skip(3).ToArray();
			}
			name = parts.Join("-");
			int index = 0;
			bool changes = false;
			var originalPath = path.GetDirectory().GetDirectory()+"/"+name;
			var originalImage = FileManager.GetAsset<Texture2D>(originalPath,false) ?? FileManager.GetAsset<Texture2D>(name,false);
			var pixels = texture.GetPixels();
			if(originalImage.IsNull() || pixels.Length != originalImage.GetPixels().Length){
				Debug.Log("[TexturePalette] : Generating source for index/splat -- " + originalPath.GetPathTerm());
				texture.SaveAs(originalPath);
				AssetDatabase.ImportAsset(originalPath.GetAssetPath());
				originalImage = FileManager.GetAsset<Texture2D>(originalPath,false);
			}
			originalPath = originalImage.GetAssetPath();
			if(Theme.debug && originalImage.format != TextureFormat.RGBA32){
				Debug.Log("[ThemePalette] Original image is not an RGBA32 texture -- " + originalPath);
			}
			var originalPixels = pixels.Copy();
			foreach(var pixel in pixels){
				if(isSplat){
					var emptyRed = pixel.r == 0 || colorA.a == 0;
					var emptyGreen = pixel.g == 0 || colorB.a == 0;
					var emptyBlue = pixel.b == 0 || colorC.a == 0;
					var colorAStart = emptyGreen && emptyBlue ? colorA.SetAlpha(0) : Color.clear;
					var colorBStart = emptyRed && emptyBlue ? colorB.SetAlpha(0) : Color.clear;
					var colorCStart = emptyRed && emptyGreen ? colorC.SetAlpha(0) : Color.clear;
					var splatA = colorAStart.Lerp(colorA,pixel.r);
					var splatB = colorBStart.Lerp(colorB,pixel.g);
					var splatC = colorCStart.Lerp(colorC,pixel.b);
					var pixelColor = splatA + splatB + splatC;
					pixelColor.a *= pixel.a;
					if(originalPixels[index] != pixelColor){
						originalPixels[index] = pixelColor;
						changes = true;
					}
					index += 1;
					continue;
				}
				foreach(var swap in this.swap){
					if(pixel.Matches(swap.Key,false)){
						var color = swap.Value.value;
						color.a = ignoreAlpha ? pixel.a : color.a * pixel.a;
						if(originalPixels[index] != color){
							originalPixels[index] = color;
							changes = true;
						}
					}
				}
				index += 1;
			}
			if(changes){
				originalImage.SetPixels(originalPixels);
				originalImage.Apply();
				if(writeToDisk){
					Utility.DelayCall(originalImage,()=>originalImage.SaveAs(originalPath),0.5f);
				}
			}
		}
	}
}