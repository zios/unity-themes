using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR_WIN
using Microsoft.Win32;
#endif
namespace Zios.Unity.Editor.Themes{
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	using Zios.File;
	using Zios.Reflection;
	using Zios.Supports.Worker;
	using Zios.Unity.ProxyEditor;
	using Zios.Unity.Extensions;
	using Zios.Unity.Extensions.Convert;
	public enum AutoBalance{Off,Intensity,Luminance}
	[Serializable]
	public class RelativeColor{
		public static AutoBalance autoBalance = AutoBalance.Intensity;
		public static List<RelativeColor> lookupBuffer = new List<RelativeColor>();
		public static RelativeColor system = new RelativeColor();
		public string name;
		public Color value = Color.clear;
		public Color blend;
		public float offset = 1;
		public bool skipTexture;
		public string sourceName = "";
		public RelativeColor source;
		public ColorBlend blendMode = ColorBlend.Normal;
		public static implicit operator RelativeColor(string value){
			return value.IsNumber() ? new RelativeColor(value.ToFloat()) : new RelativeColor(value.ToColor());
		}
		public static implicit operator RelativeColor(float value){return new RelativeColor(value);}
		public static implicit operator RelativeColor(Color value){return new RelativeColor(value);}
		public static implicit operator Color(RelativeColor current){return current.value;}
		public static implicit operator string(RelativeColor current){return current.Serialize();}
		public RelativeColor(){}
		public RelativeColor(float offset) : this(Color.magenta,offset,null){}
		public RelativeColor(Color color) : this(color,1,null){}
		public RelativeColor(Color color,float offset,RelativeColor source){this.Assign(color,offset,source);}
		public static RelativeColor Create(string data){
			return new RelativeColor().Deserialize(data);
		}
		public static void UpdateSystem(){
			//if(Theme.active.IsNull() || !Theme.active.palette.usesSystem){return;}
			var system = RelativeColor.system;
			var current = system.value;
			system.name = "@System";
			#if UNITY_EDITOR_WIN
			object key = null;
			key = Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\DWM\\","AccentColor",null);
			key = key ?? Registry.GetValue("HKEY_CURRENT_USER\\Software\\Microsoft\\Windows\\CurrentVersion\\Explorer\\Accent","AccentColor",null);
			if(!key.IsNull() && key.As<int>() != -1){
				system.value = key.As<int>().ToHex().ToColor().Order("ABGR").SetAlpha(1);
			}
			else{
				key = Registry.GetValue("HKEY_LOCAL_MACHINE\\Software\\Policies\\Microsoft\\Windows\\Personalization","PersonalColor_Accent",null);
				key = key ?? Registry.GetValue("HKEY_CURRENT_USER\\Control Panel\\Colors","WindowFrame",null);
				if(!key.IsNull() && !key.As<string>().IsEmpty()){
					system.value = key.As<string>().ToColor(" ",false);
				}
			}
			#endif
			if(system.value != current){
				Theme.Rebuild();
			}
		}
		public RelativeColor Copy(){
			var copy = new RelativeColor(this.value,this.offset,this.source);
			copy.UseVariables(this);
			return copy;
		}
		public string Serialize(int nameLength=0,int sourceNameLength=0){
			var name = nameLength < 1 ? this.name : this.name.PadRight(nameLength,' ');
			var contents = name + " : " + this.value.Serialize().PadRight(9,' ');
			if(!this.sourceName.IsEmpty()){
				var sourceName = sourceNameLength < 1 ? this.sourceName : this.sourceName.PadRight(sourceNameLength,' ');
				contents += " : " + sourceName;
			}
			if(this.offset != 1){contents += " : " + this.offset;}
			if(this.blendMode != ColorBlend.Normal){
				contents += " : *" + this.blendMode.ToName()+"-"+this.blend.Serialize().Trim("#");
			}
			return contents;
		}
		public RelativeColor Deserialize(string data){
			var terms = data.Trim().Replace("\t"," ").Remove(":","=").Split(" ").Where(x=>!x.IsEmpty()).ToArray();
			var main = terms.Skip(1);
			this.name = terms[0];
			this.sourceName = main.Where(x=>!x.IsColorData() && !x.IsNumber()).FirstOrDefault() ?? "";
			var colorValue = main.Where(x=>x.IsColorData()).FirstOrDefault();
			var offsetValue = main.Where(x=>x.IsFloat()).FirstOrDefault();
			var blendValue = main.LastOrDefault();
			var color = !colorValue.IsEmpty() ? colorValue.ToColor() : Color.magenta;
			var offset = !offsetValue.IsEmpty() ? offsetValue.ToFloat() : 1;
			this.Assign(color,offset,null);
			if(blendValue.StartsWith("*")){
				var value = blendValue.Trim("*").Split("-");
				this.blendMode = ColorBlend.Normal.ParseEnum(value[0]);
				this.blend = value[1].ToColor();
			}
			return this;
		}
		public void Assign(Color color,float offset,RelativeColor source){
			this.source = source;
			this.value = color;
			this.offset = offset;
			this.Assign(source);
		}
		public void Assign(ThemePalette palette,string sourceName){
			var source = sourceName == "@System" ? RelativeColor.system : palette.colors["*"][sourceName];
			this.Assign(source);
		}
		public void Assign(RelativeColor source){
			if(!source.IsNull()){
				var alpha = this.value.a;
				this.value = source.value;
				this.value.a = alpha;
			}
			this.source = source;
		}
		public Color ApplyOffset(bool allowBalance=true){
			var processed = RelativeColor.lookupBuffer.Contains(this.source);
			if(!this.source.IsNull() && this.source != this && !processed){
				RelativeColor.lookupBuffer.Add(this.source);
				var offset = this.offset;
				var source = this.source.ApplyOffset();
				RelativeColor.lookupBuffer.Remove(this.source);
				if(this.blendMode != ColorBlend.Normal){
					this.value = this.blend.Blend(source,this.blendMode,offset);
					return this.value;
				}
				if(offset != 1 && allowBalance && RelativeColor.autoBalance != AutoBalance.Off){
					var sourceIntensity = RelativeColor.autoBalance.Matches("Intensity") ? source.GetIntensity() : source.GetLuminance();
					var result = offset * sourceIntensity;
					var tooDark = sourceIntensity < 0.3f && result.Distance(sourceIntensity) < 0.2f;
					var tooBright = result > 1.25f;
					if(tooDark){
						var natural = source.ToVector3().normalized.ToColor();
						source = natural.Lerp(source,sourceIntensity.InverseLerp(0,0.2f));
						if(sourceIntensity == 0){source = new Color(0.25f,0.25f,0.25f);}
					}
					var difference = source.Multiply(offset).Difference(source);
					if(tooBright || difference < 0.05f){offset = 1/offset;}
				}
				var alpha = this.value.a;
				this.value = source.Multiply(offset);
				this.value.a = this.source.value.a == 1 ? alpha : this.value.a;
			}
			return this.value;
		}
		public Texture2D UpdateTexture(){
			var color = this.value;
			var path = Theme.storagePath.GetAssetPath()+"Palettes/@Generated/";
			if(!File.Exists(path)){File.Create(path);}
			var imagePath = path+"Color"+this.name+".png";
			var image = default(Texture2D);
			Worker.MainThread(()=>{
				image = File.GetAsset<Texture2D>(imagePath);
				if(image.IsNull()){
					image = new Texture2D(1,1,TextureFormat.RGBA32,false);
					image.SaveAs(imagePath);
					ProxyEditor.ImportAsset(imagePath);
					return;
				}
				image.SetPixel(0,0,color);
				image.Apply();
			});
			return image;
		}
	}
}