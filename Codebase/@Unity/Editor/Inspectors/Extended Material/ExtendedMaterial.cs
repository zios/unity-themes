using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
namespace Zios.Unity.Editor.Inspectors.ExtendedMaterial{
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	using Zios.File;
	using Zios.Reflection;
	using Zios.Unity.ProxyEditor;
	using Zios.Unity.Extensions.Convert;
	using Zios.Unity.Log;
	public static class MaterialBuffer{
		public static Material material;
		public static Shader shader;
		public static ExtendedMaterial active;
		public static string originalPath = "";
		public static bool loaded = false;
		public static bool unsaved = false;
		public static bool branchable = true;
		public static bool buildPreview = false;
		public static bool refresh = false;
		public static float buildDelay = 0;
		public static Dictionary<string,bool> options = new Dictionary<string,bool>();
	}
	public class Watcher : AssetPostprocessor{
		public static void OnPostprocessAllAssets(string[] imported,string[] deleted,string[] moved, string[] path){
			if(imported.Length > 0){
				if(MaterialBuffer.active != null && !MaterialBuffer.refresh){
					Log.Show("[ExtendedMaterial] Asset Refreshing -- " + MaterialBuffer.active.menuPath);
					ExtendedMaterial.DestroyImmediate(MaterialBuffer.active);
				}
			}
		}
	}
	//====================================
	// Enumerations
	//====================================
	public enum PassType{Normal,Use,Grab};
	public enum BindSource{Position,Normal,Tangent,Texcoord,Texcoord1,Color}
	public enum BindTarget{Position,Normal,Tangent,Texcoord,Texcoord0,Texcoord1,Texcoord2,Texcoord3,Texcoord4,Texcoord5,Texcoord6,Texcoord7,Color}
	public enum PropertyType{Range,Color,Texture,Rect,Cube,Float,Vector}
	public enum TexGen{Default,ObjectLinear,EyeLinear,SphereMap,CubeReflect,CubeNormal}
	public enum TextureDefault{Default,White,Grey,Black,Bump}
	public enum RenderQueue{Default,Background,Geometry,AlphaTest,Transparent,Overlay};
	public enum RenderType{Default,Opaque,Transparent,TransparentCutoff,Background,Overlay,TreeOpaque,TreeTransparentCutoff,TreeBillboard,Grass,GrassBillboard}
	public enum LightMode{Default,Always,ForwardBase,ForwardAdd,PrepassBase,PrepassFinal,Vertex,VertexLMRGBM,VertexLM,ShadowCaster,ShadowCollector}
	public enum Require{Default,SoftVegetation}
	public enum Cull{Default,Off,Back,Front}
	public enum Test{Default,Off,Less,Greater,LEqual,GEqual,Equal,NotEqual,Always}
	public enum Toggle{Default,On,Off}
	public enum BlendPreset{Default,Off,AlphaBlended,Additive,AdditiveSoft,Multiplicative,Multiplicative2x,Custom,CustomExtended}
	public enum Blend{One,Zero,SrcColor,SrcAlpha,DstColor,DstAlpha,OneMinusSrcColor,OneMinusSrcAlpha,OneMinusDstColor,OneMinusDstAlpha}
	public enum BlendOp{Default,Min,Max,Sub,RevSub}
	public enum FogMode{Default,Off,Linear,Exp,Exp2}
	public enum ColorMaterial{Default,AmbientAndDiffuse,Emission}
	//====================================
	// Categories
	//====================================
	[Serializable]
	public class Property{
		public PropertyType type = PropertyType.Float;
		public string name = "Example";
		public string variable = "_color";
		public float minimum = 0;
		public float maximum = 0;
		public object defaultValue = 0;
		public TexGen texgenMode;
		public void SetDefault(){
			string type = this.type.ToString();
			object value = 0.0f;
			if(type.ContainsAny("Texture","Rect","Cube")){value = TextureDefault.Default;}
			if(type == "Color"){value = Color.white;}
			if(type == "Vector"){value = Vector4.zero;}
			this.defaultValue = value;
		}
	}
	[Serializable]
	public class Builtin{
		public string colorMask = "RGBA";
		public Color color = Color.white;
		public Toggle lighting = Toggle.Off;
		public Toggle separateSpecular = Toggle.Off;
		public ColorMaterial vertexColors = ColorMaterial.Emission;
		public MaterialBlock materialBlock = new MaterialBlock();
	}
	[Serializable]
	public class MaterialBlock{
		public Color diffuseColor;
		public Color ambientColor;
		public Color specularColor;
		public Color shininessColor;
		public Color emissionColor;
		public string diffuse;
		public string ambient;
		public string specular;
		public string shininess;
		public string emission;
	}
	[Serializable]
	public class SubTags{
		public RenderQueue renderQueue = RenderQueue.Default;
		public RenderType renderType = RenderType.Default;
		public Require require = Require.Default;
		public LightMode lightMode = LightMode.Default;
		public int renderQueueOffset = 0;
		public bool ignoreProjector = false;
		public bool forceNoShadowCasting = false;
		public bool IsDefault(){
			bool result = this.renderQueue == RenderQueue.Default && this.renderType == RenderType.Default;
			result = result && renderQueueOffset == 0 && !this.ignoreProjector && !this.forceNoShadowCasting;
			return result && this.require == Require.Default && this.lightMode == LightMode.Default;
		}
	}
	[Serializable]
	public class PassTags{
		public Require require = Require.Default;
		public LightMode lightMode = LightMode.Default;
		public bool IsDefault(){
			return this.require == Require.Default && this.lightMode == LightMode.Default;
		}
	}
	[Serializable]
	public class Fog{
		public FogMode mode = FogMode.Default;
		public Color color = Color.white;
		public float density = 0;
		public Vector2 range = Vector2.zero;
		public bool IsDefault(){
			bool result = this.mode == FogMode.Default && this.color == Color.white;
			return result && this.density == 0 && this.range[0] == 0 && this.range[1] == 0;
		}
	}
	[Serializable]
	public class Bind{
		public BindSource source = BindSource.Position;
		public BindTarget target = BindTarget.Position;
		public void Validate(){}
	}
	[Serializable]
	public class Common{
		public Cull cull = Cull.Default;
		public Test zTest = Test.Default;
		public Toggle zWrite = Toggle.Default;
		public Test alphaTest = Test.Default;
		public BlendPreset blendPreset = BlendPreset.Default;
		public BlendOp blendOp = BlendOp.Default;
		public Blend[] blend = new Blend[2]{Blend.One,Blend.Zero};
		public Blend[] blendAlpha = new Blend[2]{Blend.One,Blend.Zero};
		public List<Bind> binds = new List<Bind>();
		public Fog fog = new Fog();
		public Builtin fixedFunction = new Builtin();
		public string alphaCutoff = "_cutoff";
		public Vector2 offset = Vector2.zero;
		public bool IsCommonDefault(){
			bool result = this.cull == Cull.Default && this.zTest == Test.Default;
			result = result && this.zWrite == Toggle.Default && this.alphaTest == Test.Default;
			result = result && this.blendPreset == BlendPreset.Default && this.blendOp == BlendOp.Default;
			result = result && this.blend[0] == Blend.One && this.blend[1] == Blend.Zero;
			result = result && this.blendAlpha[0] == Blend.One && this.blendAlpha[1] == Blend.Zero;
			return result && offset == Vector2.zero && this.fog.IsDefault();
		}
	}
	[Serializable]
	public class Pass : Common{
		public PassTags tags = new PassTags();
		public PassType type = PassType.Normal;
		public string name = "";
		public string grabPass = "";
		public string usePass = "";
		public string gpuShader = "";
		public Pass Copy(){
			Pass copy = this.Clone();
			copy.tags = this.tags.Clone();
			copy.binds = new List<Bind>();
			foreach(var item in this.binds){
				copy.binds.Add(item.Clone());
			}
			copy.fog = this.fog.Clone();
			copy.fixedFunction = this.fixedFunction.Clone();
			copy.blend = copy.blend.Copy();
			copy.blendAlpha = copy.blendAlpha.Copy();
			return copy;
		}
	}
	[Serializable]
	public class SubShader : Common{
		public SubTags tags = new SubTags();
		public Dictionary<string,Pass> passes = new Dictionary<string,Pass>();
		public SubShader Copy(){
			SubShader copy = this.Clone();
			copy.passes = new Dictionary<string,Pass>();
			copy.binds = new List<Bind>();
			copy.tags = this.tags.Clone();
			foreach(var item in this.passes){
				copy.passes[item.Key] = item.Value.Copy();
			}
			foreach(var item in this.binds){
				copy.binds.Add(item.Clone());
			}
			copy.fog = this.fog.Clone();
			copy.fixedFunction = this.fixedFunction.Clone();
			copy.blend = copy.blend.Copy();
			copy.blendAlpha = copy.blendAlpha.Copy();
			return copy;
		}
		public Pass AddPass(string name=""){
			Pass pass = new Pass();
			if(name == ""){name = "!"+pass.GetHashCode().ToString();}
			pass.name = name;
			this.passes[name] = pass;
			return pass;
		}
		public void RemovePass(Pass pass){
			if(this.passes.Count == 1){
				Log.Warning("[ExtendedMaterial] Cannot remove the only pass in a SubShader");
				return;
			}
			this.passes.RemoveValue(pass);
		}
	}
	//====================================
	// Main
	//====================================
	[Serializable]
	public class ExtendedMaterial : ScriptableObject{
		public Shader source;
		public string fileName;
		public string path;
		public string menuPath;
		public Dictionary<string,Property> properties = new Dictionary<string,Property>();
		public List<SubShader> subShaders = new List<SubShader>();
		public SubShader category = new SubShader();
		public string fallback = "";
		public string editor = "";
		public void Clear(){
			this.properties.Clear();
			this.subShaders.Clear();
		}
		public ExtendedMaterial Copy(){
			ExtendedMaterial copy = this.Clone();
			copy.properties = new Dictionary<string,Property>();
			copy.subShaders = new List<SubShader>();
			foreach(var item in this.properties){
				copy.properties[item.Key] = item.Value.Clone();
			}
			foreach(var item in this.subShaders){
				copy.subShaders.Add(item.Copy());
			}
			copy.category = this.category.Copy();
			return copy;
		}
		public Shader Save(){return this.Save(this.path,this.menuPath);}
		public Shader Save(string path){return this.Save(path,this.menuPath);}
		public Shader Save(string path,string menuPath){
			string output = this.Generate(menuPath);
			string folder =	path.Substring(0,path.LastIndexOf("/")) + "/";
			File.Create(folder);
			using(StreamWriter file = new StreamWriter(path,false)){
				file.Write(output);
			}
			ProxyEditor.RefreshAssets();
			return Shader.Find(menuPath);
		}
		public string Generate(string menuPath=""){
			StringBuilder output = new StringBuilder();
			output.AppendLine("Shader '" + menuPath + "'{");
			output.AppendLine("\tProperties{");
			foreach(var item in this.properties){
				Property property = item.Value;
				string type = property.type.ToString();
				string defaultValue = property.defaultValue.ToString().Remove("RGBA"," ").Remove(".000",".0");
				if(type.ContainsAny("Texture","Rect","Cube")){
					if(type == "Texture"){type = "2D";}
					defaultValue = "'"+defaultValue.ToLower()+"'{";
					if(property.texgenMode != TexGen.Default){defaultValue += "TexGen "+property.texgenMode;}
					defaultValue += "}";
				}
				output.Append("\t\t"+property.variable+"('"+property.name+"',"+type);
				if(type == "Range"){
					output.Append("("+property.minimum+","+property.maximum+")");
				}
				output.AppendLine(") = "+defaultValue);
			}
			output.AppendLine("\t}");
			foreach(SubShader subShader in this.subShaders){
				output.AppendLine("\tSubShader{");
				string tags = "";
				string common = this.GenerateCommon(subShader,"\t\t");
				if(!subShader.tags.IsDefault()){
					if(subShader.tags.lightMode != LightMode.Default){
						tags += "'LightMode'='"+subShader.tags.lightMode+"' ";
					}
					if(subShader.tags.renderQueue != RenderQueue.Default){
						int offset = subShader.tags.renderQueueOffset;
						string adjust = "";
						if(offset != 0){adjust = offset > 0 ? "+"+offset.ToString() : offset.ToString();}
						tags += "'Queue'='"+subShader.tags.renderQueue+adjust+"' ";
					}
					if(subShader.tags.renderType != RenderType.Default){
						tags += "'RenderType'='"+subShader.tags.renderType+"' ";
					}
					if(subShader.tags.require != Require.Default){
						tags += "'Require'='"+subShader.tags.require+"' ";
					}
					if(subShader.tags.ignoreProjector){tags += "'IgnoreProjector'='True' ";}
					if(subShader.tags.forceNoShadowCasting){tags += "'ForceNoShadowCasting'='True' ";}
					output.AppendLine("\t\tTags{"+tags.Trim()+"}");
				}
				output.Append(common);
				foreach(var item in subShader.passes){
					Pass pass = item.Value;
					if(pass.type == PassType.Normal){
						tags = "";
						output.AppendLine("\t\tPass{");
						common = this.GenerateCommon(pass,"\t\t\t");
						if(pass.name != "" && !pass.name.Contains("!")){
							output.AppendLine("\t\t\tName '"+pass.name+"'");
						}
						if(!pass.tags.IsDefault()){
							if(pass.tags.lightMode != LightMode.Default){
								tags += "'LightMode'='"+pass.tags.lightMode+"' ";
							}
							if(pass.tags.require != Require.Default){
								tags += "'Require'='"+pass.tags.require+"' ";
							}
							output.AppendLine("\t\t\tTags{"+tags.Trim()+"}");
						}
						output.Append(common);
						if(pass.gpuShader != ""){
							output.AppendLine("\t\t\t"+pass.gpuShader.Replace("\n","\n\t\t\t"));
						}
						output.AppendLine("\t\t}");
					}
					else if(pass.type == PassType.Use){
						if(pass.usePass == ""){pass.usePass = "Self-Illumin/VertexLit/BASE";}
						output.AppendLine("\t\tUsePass '"+pass.usePass+"'");
					}
					else if(pass.type == PassType.Grab){
						string grabPass = pass.grabPass != "" ? "'"+pass.grabPass+"'" : "";
						output.AppendLine("\t\tGrabPass{"+grabPass+"}");
					}
				}
				output.AppendLine("\t}");
			}
			if(this.fallback != ""){output.AppendLine("\tFallback '"+this.fallback+"'");}
			if(this.editor != ""){output.AppendLine("\tCustomEditor '"+this.editor+"'");}
			output.AppendLine("}");
			string result = output.ToString().Replace("'","\"");
			return result;
		}
		public string GenerateCommon(Common common,string pad){
			StringBuilder output = new StringBuilder();
			if(!common.IsCommonDefault()){
				if(common.cull != Cull.Default){output.AppendLine(pad+"Cull "+common.cull);}
				if(common.zTest != Test.Default){output.AppendLine(pad+"ZTest "+common.zTest);}
				if(common.zWrite != Toggle.Default){output.AppendLine(pad+"ZWrite "+common.zWrite);}
				if(common.alphaTest != Test.Default){
					string compare = " ["+common.alphaCutoff+"]";
					if(common.alphaCutoff.IsNumber()){compare = compare.Remove("[","]");}
					if(common.alphaTest == Test.Off || common.alphaTest == Test.Always){compare = "";}
					output.AppendLine(pad+"AlphaTest "+common.alphaTest+compare);
				}
				this.SortPreset(common);
				if(common.blendPreset != BlendPreset.Default){
					string extra = "";
					if(common.blendPreset == BlendPreset.CustomExtended){
						extra = ", "+common.blendAlpha[0]+" "+common.blendAlpha[1];
					}
					if(common.blendPreset == BlendPreset.Off){
						output.AppendLine(pad+"Blend Off");
					}
					else{
						output.AppendLine(pad+"Blend "+common.blend[0]+" "+common.blend[1]+extra);
						if(common.blendOp != BlendOp.Default){
							output.AppendLine(pad+"BlendOp "+common.blendOp);
						}
					}
				}
				if(common.offset != Vector2.zero){
					output.AppendLine(pad+"Offset "+common.offset.ToString().Pack());
				}
				if(!common.fog.IsDefault() && common.fog.mode != FogMode.Default){
					Fog fog = common.fog;
					string fogColor = fog.color.ToString().Remove("RGBA"," ");
					string fogRange = fog.range.ToString().Pack();
					output.AppendLine(pad+"Fog{");
					output.AppendLine(pad+"\tMode "+fog.mode);
					output.AppendLine(pad+"\tColor "+fogColor);
					if(fog.density != 0){output.AppendLine(pad+"\tDensity "+fog.density);}
					if(fog.range != Vector2.zero){output.AppendLine(pad+"\tRange "+fogRange);}
					output.AppendLine(pad+"}");
				}
			}
			return output.ToString().Remove(".000",".0");
		}
		public void Branch(){
			string hash = "#"+this.Generate().ToMD5();
			Log.Show(hash);
			//this.path = this.path.Replace(".shader",changeHash+".shader");
			//this.Save();
		}
		public string GetBlock(string contents,string keyword){
			int start = contents.IndexOf(keyword,true);
			if(start != -1){
				int index = 0;
				int level = 1;
				int open = contents.IndexOf('{',start);
				string area = contents.Substring(open+1);
				for(index=0;index<area.Length;++index){
					char current = area[index];
					if(current == '{'){level += 1;}
					if(current == '}'){level -= 1;}
					if(level == 0){break;}
				}
				return contents.Substring(start,(index-start+open)+2);
			}
			return "";
		}
		public void Load(Material material){
			string assetPath = File.GetPath(material.shader);
			this.Load(assetPath);
		}
		public void Load(Shader shader){
			string assetPath = File.GetPath(shader);
			this.Load(assetPath);
		}
		public void Load(string path){
			path = File.Exists(path) ? path : File.Find(path).path;
			string contents = "";
			int parsePass = 0;
			using(StreamReader file = new StreamReader(path)){
				contents = file.ReadToEnd();
			}
			if(contents != ""){
				string quote = "\"";
				contents = contents.Replace("Prepass","Prelight",true);
				contents = contents.Replace("UsePass","UseShader",true);
				contents = contents.Replace("GrabPass","GrabScreen",true);
				this.menuPath = contents.Cut(quote,quote).Remove(quote);
				this.editor = contents.Cut("CustomEditor","\n").Remove("CustomEditor").Pack();
				this.fallback = contents.Cut("Fallback","\n").Remove("Fallback").Pack();
				this.source = Shader.Find(this.menuPath);
				string propertyBlock = this.GetBlock(contents,"Properties");
				contents = contents.ReplaceFirst(propertyBlock,"",true);
				string [] lines = propertyBlock.Split(new string[]{"\r\n","\n"},StringSplitOptions.None);
				foreach(string current in lines){
					string line = current.Remove(" ","\t",quote).Condense();
					if(line.Contains("=")){
						Property property = new Property();
						property.variable = line.Substring(0,line.IndexOf("("));
						property.name = current.Cut("(",",").Remove("(",",",quote);
						string type = line.Cut(",",")").Remove(",",")");
						if(type.Contains("Range",true)){
							type = "Range";
							string data = line.Cut(",",")").Remove("Range","(",")").TrimLeft(",");
							property.minimum = Convert.ToSingle(data.TrySplit(',',0));
							property.maximum = Convert.ToSingle(data.TrySplit(',',1));
						}
						if(type == "2D"){type = "Texture";}
						string value = line.TrySplit('=',1);
						property.type = (PropertyType)property.type.Get(type,5);
						property.defaultValue = value;
						if(type == "Range" || type == "Float"){property.defaultValue = Convert.ToSingle(value);}
						if(type == "Color" || type == "Vector"){
							float[] values = value.Remove("(",")").Split(',').ConvertAll<float>();
							property.defaultValue = values;
							if(type == "Color"){property.defaultValue = values.ToColor();}
							if(type == "Vector"){property.defaultValue = values.ToVector4();}
						}
						if(type == "Texture" || type == "Rect" || type == "Cube"){
							string extra = current.Cut("{","}").Remove("{","}") + " ";
							if(extra.Contains("TexGen")){
								string texgen = extra.Cut("TexGen"," ",0,true,2).Remove("TexGen","\t"," ");
								property.texgenMode =  (TexGen)property.texgenMode.Get(texgen);
							}
							int bracket = value.IndexOf("{");
							property.defaultValue = new TextureDefault().Get(value.Substring(0,bracket));
						}
						this.properties[property.name] = property;
					}
				}
				while(true){
					if(parsePass > 128){
						Log.Error("[ExtendedMaterial] Error parsing shader -- " + this.menuPath);
						return;
					}
					string nextSubShader = this.GetBlock(contents,"SubShader");
					if(nextSubShader == ""){break;}
					contents = contents.ReplaceFirst(nextSubShader,"",true);
					SubShader subShader = new SubShader();
					string tagBlock;
					while(true){
						if(parsePass > 128){
							Log.Error("[ExtendedMaterial] Error parsing shader -- " + this.menuPath);
							return;
						}
						Pass pass = new Pass();
						string nextType = nextSubShader.FindFirst("UseShader","GrabScreen","Pass");
						string passName = "!"+pass.GetHashCode().ToString();
						string usePass = nextSubShader.Cut("UseShader","\n");
						if(nextType == ""){break;}
						if(nextType == "UseShader" && usePass != ""){
							nextSubShader = nextSubShader.ReplaceFirst(usePass,"",true);
							pass.type = PassType.Use;
							pass.usePass = usePass.Remove("UseShader").Pack();
						}
						string grabPass = this.GetBlock(nextSubShader,"GrabScreen");
						if(nextType == "GrabScreen" && grabPass != ""){
							nextSubShader = nextSubShader.ReplaceFirst(grabPass,"",true);
							pass.type = PassType.Grab;
							pass.grabPass = grabPass.Remove("GrabScreen").Pack();
						}
						string normalPass = this.GetBlock(nextSubShader,"Pass");
						if(nextType == "Pass" && normalPass != ""){
							nextSubShader = nextSubShader.ReplaceFirst(normalPass,"",true);
							pass.name = normalPass.Cut("Name \"","\n").Remove("Name").Pack();
							pass.gpuShader = normalPass.Cut("CGPROGRAM","ENDCG");
							normalPass = normalPass.ReplaceFirst(pass.gpuShader,"",true);
							if(pass.name != ""){passName = pass.name;}
							if(normalPass.Contains("GLSLPROGRAM")){pass.gpuShader = normalPass.Cut("GLSLPROGRAM","ENDGLSL");}
							pass.gpuShader = pass.gpuShader.Remove("\t\t\t");
							tagBlock = this.GetBlock(normalPass,"tags").Remove(" ","\t").Replace("Prelight","Prepass",true);
							if(tagBlock != ""){
								string lightMode = tagBlock.Cut("LightMode",quote,0,true,3).Remove(quote).TrySplit('=',1);
								string require = tagBlock.Cut("Require",quote,0,true,3).Remove(quote).TrySplit('=',1);
								pass.tags.lightMode = (LightMode)pass.tags.lightMode.Get(lightMode,0);
								pass.tags.require = (Require)pass.tags.require.Get(require,0);
							}
							this.SortCommon(pass,normalPass);
							normalPass = normalPass.ReplaceFirst(normalPass,"",true);
						}
						subShader.passes[passName] = pass;
						parsePass += 1;
					}
					tagBlock = this.GetBlock(nextSubShader,"tags");
					nextSubShader = nextSubShader.ReplaceFirst(tagBlock,"",true);
					if(tagBlock != ""){
						tagBlock = tagBlock.Remove(" ","\t").Replace("Prelight","Prepass",true);
						string renderQueue = tagBlock.Cut("Queue",quote,0,true,3).Remove(quote).TrySplit('=',1);
						string renderType = tagBlock.Cut("Rendertype",quote,0,true,3).Remove(quote).TrySplit('=',1);
						string ignoreProjector = tagBlock.Cut("IgnoreProjector",quote,0,true,3).Remove(quote).TrySplit('=',1);
						string forceNoShadowCasting = tagBlock.Cut("ForceNoShadowCasting",quote,0,true,3).Remove(quote).TrySplit('=',1);
						string lightMode = tagBlock.Cut("LightMode",quote,0,true,3).Remove(quote).TrySplit('=',1);
						string require = tagBlock.Cut("Require",quote,0,true,3).Remove(quote).TrySplit('=',1);
						if(renderQueue.Contains("+")){
							subShader.tags.renderQueueOffset = Convert.ToInt16(renderQueue.Split('+')[1]);
							renderQueue = renderQueue.Split('+')[0];
						}
						if(renderQueue.Contains("-")){
							subShader.tags.renderQueueOffset = Convert.ToInt16(renderQueue.Split('-')[1]) * -1;
							renderQueue = renderQueue.Split('-')[0];
						}
						subShader.tags.lightMode = (LightMode)subShader.tags.lightMode.Get(lightMode,0);
						subShader.tags.require = (Require)subShader.tags.require.Get(require,0);
						subShader.tags.renderQueue = (RenderQueue)subShader.tags.renderQueue.Get(renderQueue,0);
						subShader.tags.renderType = (RenderType)subShader.tags.renderType.Get(renderType,0);
						subShader.tags.ignoreProjector = ignoreProjector.Matches("true",true) ? true : false;
						subShader.tags.forceNoShadowCasting = forceNoShadowCasting.Matches("true",true) ? true : false;
					}
					this.SortCommon(subShader,nextSubShader);
					this.subShaders.Add(subShader);
					parsePass += 1;
				}
				if(this.subShaders.Count > 0){this.category = this.subShaders.First();}
			}
			this.path = path;
			this.fileName = path.Substring(path.LastIndexOf("/")+1);
		}
		public void SortCommon(Common common,string contents){
			string cull = contents.Cut("Cull","\n").Remove("Cull").Condense().Trim();
			string zTest = contents.Cut("ZTest","\n").Remove("ZTest").Condense().Trim();
			string zWrite = contents.Cut("ZWrite","\n").Remove("ZWrite").Condense().Trim();
			string alphaTest = contents.Cut("AlphaTest","\n").Remove("AlphaTest").Condense().Trim();
			string blend = contents.Cut("Blend","\n").Condense().Trim();
			string blendOp = contents.Cut("BlendOp","\n").Remove("BlendOp").Condense().Trim();
			string offset = contents.Cut("Offset","\n").Remove("Offset").Condense().Trim();
			common.cull = (Cull)common.cull.Get(cull,0);
			common.zTest = (Test)common.zTest.Get(zTest,0);
			common.zWrite = (Toggle)common.zWrite.Get(zWrite,0);
			if(alphaTest != ""){
				string test = alphaTest.Split(' ')[0];
				common.alphaTest = (Test)common.alphaTest.Get(test,0);
				common.alphaCutoff = test.Matches("Off",true) ? "0" : alphaTest.Split(' ')[1].Remove("[","]");
			}
			if(offset != ""){
				float x = Convert.ToSingle(offset.Split(',')[0].Trim());
				float y = Convert.ToSingle(offset.Split(',')[1].Trim());
				common.offset = new Vector2(x,y);
			}
			if(blend != ""){
				string[] part = blend.Replace(","," ").Replace("  "," ").Replace("\t"," ").Split(' ');
				if(blend.Contains("Off",true) || part.Length <= 2){common.blendPreset = BlendPreset.Off;}
				else{
					//bool loadPreset = !this.menuPath.Contains("Hidden/Preview");
					common.blend[0] = (Blend)common.blend[0].Get(part[1],0);
					common.blend[1] = (Blend)common.blend[1].Get(part[2],0);
					this.SortPreset(common,true);
					if(part.Length > 3){
						common.blendAlpha[0] = (Blend)common.blendAlpha[0].Get(part[3],0);
						common.blendAlpha[1] = (Blend)common.blendAlpha[1].Get(part[4],0);
						common.blendPreset = BlendPreset.CustomExtended;
					}
				}
			}
			common.blendOp = (BlendOp)common.blendOp.Get(blendOp,0);
			string fogBlock = this.GetBlock(contents,"Fog");
			if(fogBlock != ""){
				string fogMode = fogBlock.Cut("Mode","\n").Remove("Mode").Trim();
				string fogColor = fogBlock.Cut("Color","\n").Remove("Color","(",")").Trim();
				string fogRange = fogBlock.Cut("Range","\n").Remove("Range").Trim();
				string fogDensity = fogBlock.Cut("Density","\n").Remove("Density").Trim();
				if(fogRange != ""){
					float start = Convert.ToSingle(fogRange.TrySplit(',',0));
					float end = Convert.ToSingle(fogRange.TrySplit(',',1));
					common.fog.range = new Vector2(start,end);
				}
				if(fogColor != ""){
					common.fog.color = fogColor.Split(',').ConvertAll<float>().ToColor();
				}
				common.fog.mode = (FogMode)common.fog.mode.Get(fogMode,0);
				if(fogDensity !=""){common.fog.density = Convert.ToSingle(fogDensity);}
			}
		}
		public void SortPreset(Common common,bool identifyPreset=false){
			Blend[] blend = common.blend;
			BlendPreset preset = common.blendPreset;
			if(identifyPreset){
				if(blend[0] == Blend.One && blend[1] == Blend.Zero){preset = BlendPreset.Default;}
				if(blend[0] == Blend.SrcAlpha && blend[1] == Blend.OneMinusSrcAlpha){preset = BlendPreset.AlphaBlended;}
				if(blend[0] == Blend.One && blend[1] == Blend.One){preset = BlendPreset.Additive;}
				if(blend[0] == Blend.OneMinusDstColor && blend[1] == Blend.One){preset = BlendPreset.AdditiveSoft;}
				if(blend[0] == Blend.DstColor && blend[1] == Blend.Zero){preset = BlendPreset.Multiplicative;}
				if(blend[0] == Blend.DstColor && blend[1] == Blend.SrcColor){preset = BlendPreset.Multiplicative2x;}
				if(preset == common.blendPreset){preset = BlendPreset.Custom;}
				common.blendPreset = preset;
			}
			if(preset == BlendPreset.AlphaBlended){blend = new Blend[2]{Blend.SrcAlpha,Blend.OneMinusSrcAlpha};}
			if(preset == BlendPreset.Additive){blend = new Blend[2]{Blend.One,Blend.One};}
			if(preset == BlendPreset.AdditiveSoft){blend = new Blend[2]{Blend.OneMinusDstColor,Blend.One};}
			if(preset == BlendPreset.Multiplicative){blend = new Blend[2]{Blend.DstColor,Blend.Zero};}
			if(preset == BlendPreset.Multiplicative2x){blend = new Blend[2]{Blend.DstColor,Blend.SrcColor};}
			common.blend = blend;
		}
		public Property AddProperty(string name=""){
			Property property = new Property();
			if(name == ""){name = "!"+property.GetHashCode().ToString();}
			property.name += this.properties.Count;
			this.properties[name] = property;
			return property;
		}
		public SubShader AddSubShader(){
			SubShader subShader = new SubShader();
			subShader.AddPass();
			this.subShaders.Add(subShader);
			return subShader;
		}
		public void RemoveSubShader(SubShader subShader){
			if(this.subShaders.Count == 1){
				Log.Warning("[ExtendedMaterial] Cannot remove the only subShader in a material");
				return;
			}
			this.subShaders.Remove(subShader);
		}
	}
}