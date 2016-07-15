using System;
using System.Linq;
using UnityEngine;
namespace Zios.Interface{
	using UnityEditor;
	[Serializable]
	public class RelativeColor{
		public string name;
		public Color value;
		public Color original;
		public float offset = 1;
		public bool skipTexture;
		public string sourceName;
		public RelativeColor source;
		public Texture2D texture;
		public ColorBlend blend = ColorBlend.Multiply;
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
		public RelativeColor Copy(){
			var copy = new RelativeColor(this.value,this.offset,this.source);
			copy.UseVariables(this);
			return copy;
		}
		public string Serialize(){
			var contents = this.name + " : " + this.value.Serialize();
			if(!this.sourceName.IsEmpty()){contents += " : " + this.sourceName;}
			if(this.offset != 1){contents += " : " + this.offset;}
			return contents;
		}
		public RelativeColor Deserialize(string data){
			var terms = data.Remove(":","=").Split(" ").Where(x=>!x.IsEmpty()).ToArray();
			var main = terms.Skip(1);
			this.name = terms[0];
			this.sourceName = main.Where(x=>!x.IsColor() && !x.IsNumber()).FirstOrDefault();
			var colorValue = main.Where(x=>x.IsColor()).FirstOrDefault();
			var offsetValue = main.Where(x=>x.IsNumber()).FirstOrDefault();
			var color = !colorValue.IsEmpty() ? colorValue.ToColor() : Color.magenta;
			var offset = !offsetValue.IsEmpty() ? offsetValue.ToFloat() : 1;
			this.Assign(color,offset,null);
			return this;
		}
		public void Assign(Color color,float offset,RelativeColor source){
			this.source = source;
			this.original = color;
			this.value = color;
			this.offset = offset;
			this.Assign(source);
		}
		public void Assign(RelativeColor source){
			if(!source.IsNull()){
				this.value = source.value;
				this.original = source.value;
			}
			this.source = source;
		}
		public void ApplyOffset(){
			if(!this.source.IsNull()){
				this.value = this.source.original.Multiply(this.offset);
			}
		}
		public void ApplyOffset(Color target){this.value = target.Multiply(this.offset);}
		public void ApplyColor(){this.value = this.source ?? this.value;}
		public void ApplyColor(Color target){this.value = target;}
		public Texture2D UpdateTexture(string path){
			var color = this.value;
			path = path.GetAssetPath();
			FileManager.Create(path+"@Palettes/Generated/");
			var imagePath = path+"@Palettes/Generated/Color"+this.name+".png";
			var borderPath = path+"@Palettes/Generated/Color"+this.name+"Outline.png";
			var image = this.texture = (Texture2D)AssetDatabase.LoadAssetAtPath(imagePath,typeof(Texture2D));
			var border = (Texture2D)AssetDatabase.LoadAssetAtPath(borderPath,typeof(Texture2D));
			if(image.IsNull()){
				image = this.texture = new Texture2D(1,1,TextureFormat.RGBA32,false);
				image.SaveAs(imagePath);
				AssetDatabase.ImportAsset(imagePath);
			}
			if(border.IsNull()){
				border = new Texture2D(3,3,TextureFormat.RGBA32,false);
				border.SaveAs(borderPath);
				AssetDatabase.ImportAsset(borderPath);
			}
			image.SetPixel(0,0,color);
			image.Apply();
			border.SetPixels(new Color[]{color,color,color,color,Color.clear,color,color,color,color});
			border.Apply();
			image.SaveAs(imagePath);
			border.SaveAs(borderPath);
			return image;
		}
	}
}
