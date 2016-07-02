using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEvent = UnityEngine.Event;
#pragma warning disable 618
namespace Zios.Editors.MaterialEditors{
	public class ExtendedMaterialEditor : MaterialEditor{
		public GUISkin UI;
		public int drawn;
		public string stateChanged = "";
		public string titleChanged = "";
		public string warning = "";
		public object hoverObject;
		public override void Awake(){
			this.Setup();
			base.Awake();
		}
		public void Setup(bool full=false){
			if(full){
				this.UI = FileManager.GetAsset<GUISkin>("ExtendedMaterialEditor.guiskin");
				this.UI.button = new GUIStyle(GUI.skin.button);
				this.UI.button.margin.left = 12;
			}
			Material material = (Material)this.target;
			bool targetReady = material != null && material.shader != null;
			bool targetMismatch = Buffer.active == null || Buffer.material == null || (material.shader != Buffer.shader);
			bool newMaterial = Buffer.material != material;
			if(targetReady && (Buffer.refresh || targetMismatch)){
				if(newMaterial){Buffer.originalPath = "";}
				Buffer.material = material;
				Buffer.shader = material.shader;
				if(!Buffer.refresh || Buffer.active == null){
					if(Buffer.active != null){DestroyImmediate(Buffer.active);}
					Buffer.active = ScriptableObject.CreateInstance<ExtendedMaterial>();
				}
				Buffer.active.Clear();
				Buffer.active.Load(material.shader);
				Buffer.refresh = false;
				Buffer.unsaved = false;
				this.LoadSettings();
				Debug.Log("[ExtendedMaterial] Material loaded -- " + material.name + " [" + material.shader.name + "]");
			}
		}
		public void LoadSettings(){
			string name = Buffer.material.name;
			bool firstOpen = !PlayerPrefs.HasKey(name+"-Material");
			Buffer.options["ShowDefault"] = firstOpen ? true : PlayerPrefs.GetInt("ExtendedMaterial-ShowDefault").ToBool();
			Buffer.options["ShowPreview"] = firstOpen ? true : PlayerPrefs.GetInt("ExtendedMaterial-ShowPreview").ToBool();
			Buffer.options["ShaderUnity"] = PlayerPrefs.GetInt(name+"-ShaderUnity").ToBool();
			Buffer.options["ShaderGPU"] = PlayerPrefs.GetInt(name+"-ShaderGPU").ToBool();
			Buffer.options["Properties"] = PlayerPrefs.GetInt(name+"-Properties").ToBool();
			Buffer.options["Material"] = firstOpen ? true : PlayerPrefs.GetInt(name+"-Material").ToBool();
			int shaderIndex = 0;
			int passIndex = 0;
			foreach(SubShader subShader in Buffer.active.subShaders){
				shaderIndex += 1;
				string shaderHash = "Sub"+shaderIndex;
				Buffer.options[shaderHash] = firstOpen ? true : PlayerPrefs.GetInt(name+"-"+shaderHash).ToBool();
				Buffer.options[shaderHash+"Tags"] = PlayerPrefs.GetInt(name+"-"+shaderHash+"Tags").ToBool();
				Buffer.options[shaderHash+"Fog"] = PlayerPrefs.GetInt(name+"-"+shaderHash+"Fog").ToBool();
				foreach(var item in subShader.passes){
					passIndex += 1;
					item.Value.name = item.Value.name;
					string passHash = "Pass"+passIndex;
					Buffer.options[passHash] = firstOpen ? true : PlayerPrefs.GetInt(name+"-"+passHash).ToBool();
					Buffer.options[passHash+"Tags"] = PlayerPrefs.GetInt(name+"-"+passHash+"Tags").ToBool();
					Buffer.options[passHash+"Fog"] = PlayerPrefs.GetInt(name+"-"+passHash+"Fog").ToBool();
				}
			}
		}
		public object Draw(string label,object field,object ignore=default(object)){
			EditorGUI.BeginChangeCheck();
			GUILayoutOption[] size = new GUILayoutOption[]{GUILayout.Width(165),GUILayout.Height(18)};
			GUILayoutOption[] half = new GUILayoutOption[]{GUILayout.Width(80),GUILayout.Height(18)};
			bool hideDefault = (field is Enum && (int)field == 0);
			hideDefault = hideDefault || (ignore != default(object) && ignore.Equals(field));
			if(hideDefault && !Buffer.options["ShowDefault"]){return field;}
			int indent = EditorGUI.indentLevel;
			EditorGUILayout.BeginHorizontal();
			Type type = field.GetType();
			if(label != ""){EditorGUILayout.PrefixLabel(label);}
			EditorGUI.indentLevel = 0;
			GUI.SetNextControlName(label);
			if(field is Enum){field = EditorGUILayout.EnumPopup((Enum)field,size);}
			if(field is int){field = EditorGUILayout.IntField((int)field,size);}
			if(field is float){field = EditorGUILayout.FloatField((float)field,size);}
			if(field is string){field = EditorGUILayout.TextField((string)field,size);}
			if(field is Color){field = EditorGUILayout.ColorField((Color)field,size);}
			if(field is Vector2){
				Vector2 value = (Vector2)field;
				float fieldA = EditorGUILayout.FloatField((float)value.x,half);
				float fieldB = EditorGUILayout.FloatField((float)value.y,half);
				field = new Vector2(fieldA,fieldB);
			}
			if(type.IsArray && typeof(Enum).IsAssignableFrom(type.GetElementType())){
				IEnumerable items = (IEnumerable)field;
				List<Enum> result = new List<Enum>();
				foreach(object value in items){
					result.Add(EditorGUILayout.EnumPopup((Enum)value,half));
				}
				field = result.ToArray();
			}
			if(field is bool){field = EditorGUILayout.Toggle((bool)field);}
			string typeName = field is Enum ? "Enum" : "Field";
			if(field is Color){typeName = "Color";}
			if(field is bool){typeName = "Toggle";}
			this.drawn += 1;
			EditorGUILayout.EndHorizontal();
			EditorGUI.indentLevel = indent;
			bool currentChanged = EditorGUI.EndChangeCheck();
			this.stateChanged = currentChanged ? typeName+"-"+label : this.stateChanged;
			if(currentChanged){
				Undo.RegisterUndo(Buffer.active,"Shader Edit - " + label);
			}
			return field;
		}
		public bool DrawTitle(string title,bool current){
			EditorGUI.BeginChangeCheck();
			string skinSuffix = EditorGUIUtility.isProSkin || EditorPrefs.GetBool("EditorTheme-Dark",false) ? "Dark" : "Light";
			skinSuffix += current ? "Open" : "";
			GUIStyle styleTitle = this.UI.GetStyle("Title"+skinSuffix);
			string arrow = "<color=#008ffeFF>▼</color>";
			string state = current ? arrow : "▶";
			bool pressed = GUILayout.Button(state+" "+title,styleTitle) ? !current : current;
			this.titleChanged = EditorGUI.EndChangeCheck() ? title : this.titleChanged;
			return pressed;
		}
		public bool DrawToggleButton(string label,bool state){
			string skinSuffix = EditorGUIUtility.isProSkin || EditorPrefs.GetBool("EditorTheme-Dark",false) ? "Dark" : "Light";
			string prefix = state ? "☗ " : "☖ ";
			skinSuffix += state ? "Active" : "";
			GUIStyle buttonStyle = this.UI.GetStyle("Button"+skinSuffix);
			if(GUILayout.Button(prefix+label,buttonStyle)){
				this.stateChanged = "ToggleButton-"+label;
				return !state;
			}
			return state;
		}
		public bool DrawFold(string label,bool state){
			EditorGUI.BeginChangeCheck();
			bool result = EditorGUILayout.Foldout(state,label);
			this.titleChanged = EditorGUI.EndChangeCheck() ? label : this.titleChanged;
			return result;
		}
		public bool CheckHover(){
			if(UnityEvent.current.type == EventType.Repaint){
				Rect last = GUILayoutUtility.GetLastRect();
				Vector2 mousePosition = UnityEvent.current.mousePosition;
				return last.Contains(mousePosition);
			}
			return false;
		}
		public void DrawCommon(Common common){
			common.cull = (Cull)this.Draw("Cull",common.cull);
			common.zTest = (Test)this.Draw("Z-Test",common.zTest);
			common.zWrite = (Toggle)this.Draw("Z-Write",common.zWrite);
			if(common.zWrite != Toggle.Off){
				common.alphaTest = (Test)this.Draw("Alpha Test",common.alphaTest);
				if(common.alphaTest != Test.Always && common.alphaTest != Test.Default && common.alphaTest != Test.Off){
					common.alphaCutoff = (string)this.Draw("Cutoff",common.alphaCutoff);
				}
			}
			BlendPreset preset = common.blendPreset;
			common.blendPreset = (BlendPreset)this.Draw("Blend Preset",common.blendPreset);
			bool custom = common.blendPreset == BlendPreset.Custom;
			bool extended = common.blendPreset == BlendPreset.CustomExtended;
			bool showBlend = Buffer.options["ShowDefault"] || (custom || extended);
			if(preset != common.blendPreset && custom){
				common.blend = new Blend[2]{Blend.One,Blend.Zero};
			}
			if(common.blendPreset != BlendPreset.Off && showBlend){
				++EditorGUI.indentLevel;
				EditorGUI.BeginDisabledGroup(!custom && !extended);
				string blendName = extended ? "Color" : "Color & Alpha";
				common.blend = this.Draw(blendName,common.blend).As<Array>().Convert<Blend>();
				if(extended){common.blendAlpha = this.Draw("Alpha",common.blendAlpha).As<Array>().Convert<Blend>();}
				EditorGUI.EndDisabledGroup();
				--EditorGUI.indentLevel;
			}
			if(common.blendPreset != BlendPreset.Default && common.blendPreset != BlendPreset.Off){
				common.blendOp = (BlendOp)this.Draw("Blend Operation",common.blendOp);
			}
			common.offset = (Vector2)this.Draw("Offset",common.offset,Vector2.zero);
		}
		public void DrawFog(Fog fog,string hash){
			bool fogModeDefault = fog.mode ==  FogMode.Default;
			bool hideFog = !Buffer.options["ShowDefault"] && (fog.IsDefault() || fogModeDefault);
			if(!hideFog){
				Buffer.options[hash+"Fog"] = EditorGUILayout.Foldout(Buffer.options[hash+"Fog"],"Fog");
				if(Buffer.options[hash+"Fog"]){
					++EditorGUI.indentLevel;
					fog.mode = (FogMode)this.Draw("Mode",fog.mode);
					if(!fogModeDefault){
						fog.color = (Color)this.Draw("Color",fog.color,Color.white);
						fog.density = (float)this.Draw("Density",fog.density,0.0f);
						fog.range = (Vector2)this.Draw("Range",fog.range,Vector2.zero);
					}
					--EditorGUI.indentLevel;
				}
			}
		}
		public void CheckContext(){
			if(UnityEvent.current.type == EventType.ContextClick && this.hoverObject != null){
				object hover = this.hoverObject;
				string typeName = "";
				GenericMenu menu = new GenericMenu();
				Action removeMethod = ()=>Debug.Log("No Context Found.");
				Action flagDirty = ()=> Buffer.buildPreview = true;
				if(hover is Property){
					Property property = (Property)hover;
					typeName = "Property ["+property.name+"]";
					removeMethod = ()=>Buffer.active.properties.RemoveValue(property);
				}
				else if(hover is object[]){
					object[] items = (object[])hover;
					Pass pass = (Pass)items[1];
					string name = pass.name != "" ? " ["+pass.name+"]" : "";
					typeName = "Pass"+name;
					removeMethod = ()=>((SubShader)items[0]).RemovePass(pass);
				}
				else if(hover is SubShader){
					SubShader subShader = (SubShader)hover;
					typeName = "SubShader";
					removeMethod = ()=>Buffer.active.RemoveSubShader(subShader);
				}
				GUIContent field = new GUIContent("Remove "+typeName);
				menu.AddItem(field,false,new GenericMenu.MenuFunction(removeMethod+flagDirty));
				menu.ShowAsContext();
				Buffer.unsaved = true;
				this.hoverObject = null;
				UnityEvent.current.Use();
			}
		}
		public override void OnInspectorGUI(){
			if(!UnityEvent.current.IsUseful()){return;}
			this.Setup(true);
			if(this.isVisible && Buffer.active != null){
				Material material = Buffer.material;
				this.stateChanged = "";
				this.titleChanged = "";
				string unsaved = Buffer.unsaved ? "*" : "";
				Buffer.options["ShaderUnity"] = this.DrawTitle("Unity Shader"+unsaved,Buffer.options["ShaderUnity"]);
				if(Buffer.options["ShaderUnity"]){
					this.drawn = -2;
					bool isPreview = Buffer.active.menuPath.Contains("Hidden/Preview/");
					bool isBranch = Buffer.active.fileName.Contains("#");
					bool exists = Shader.Find(Buffer.active.menuPath) != null;
					EditorGUIUtility.labelWidth = Screen.width - 207;
					EditorGUILayout.BeginHorizontal();
					if(!isBranch && GUILayout.Button("Save")){
						string warning = "Saving will overwrite the original shader and effect all materials that use it.";
						warning += "  It is advised to instead use the Save As or Branch option.";
						bool confirm = EditorUtility.DisplayDialog("Are you sure?",warning,"Save","Cancel");
						if(confirm){
							this.EndPreview();
							Buffer.unsaved = false;
							Buffer.material.shader = Buffer.active.Save();
							Debug.Log("[ExtendedMaterial] Shader saved -- " + Buffer.active.path);
							return;
						}
					}
					if(GUILayout.Button("Save As")){
						if(exists && !isPreview){
							GUI.FocusControl("Menu");
							this.warning = "Menu path must be unique for shader.";
						}
						else{
							this.EndPreview();
							Buffer.unsaved = false;
							string path = EditorUtility.SaveFilePanel("Save Shader",Buffer.active.path,Buffer.active.fileName,"shader");
							Buffer.material.shader = Buffer.active.Save(path);
							Debug.Log("[ExtendedMaterial] Shader saved -- " + path);
							return;
						}
					}
					if((Buffer.unsaved || isPreview) && GUILayout.Button("Revert")){
						string warning = "Changes will be lost.";
						bool confirm = EditorUtility.DisplayDialog("Revert shader to defaults?",warning,"Revert","Cancel");
						if(confirm){
							Undo.RegisterUndo(Buffer.material,"Shader Edit - Revert");
							this.EndPreview();
							this.FixPreviewShader(true);
							return;
						}
					}
					if(GUILayout.Button("Branch")){
						this.EndPreview();
						Buffer.unsaved = false;
						Buffer.active.Branch();
						Debug.Log("[ExtendedMaterial] Shader branched -- " + Buffer.active.path);
						return;
					}
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.BeginHorizontal();
					Buffer.options["ShowDefault"] = this.DrawToggleButton("Show Default",Buffer.options["ShowDefault"]);
					Buffer.options["ShowPreview"] = this.DrawToggleButton("Show Preview",Buffer.options["ShowPreview"]);
					EditorGUILayout.EndHorizontal();
					if(this.warning != ""){EditorGUILayout.HelpBox(this.warning,MessageType.Warning);}
					EditorGUI.BeginDisabledGroup(true);
					Buffer.active.fileName = (string)this.Draw("File",Buffer.active.fileName.Remove("-Preview"));
					EditorGUI.EndDisabledGroup();
					string pathPrefix = isPreview ? "Hidden/Preview/" : "";
					Buffer.active.menuPath = pathPrefix+(string)this.Draw("Menu",Buffer.active.menuPath.Remove("\\","Hidden/Preview/"));
					Buffer.active.fallback = (string)this.Draw("Fallback",Buffer.active.fallback,"");
					Buffer.active.editor = (string)this.Draw("Editor",Buffer.active.editor,"");
					Buffer.options["Properties"] = this.DrawFold("Properties",Buffer.options["Properties"]);
					if(Buffer.options["Properties"]){
						EditorGUI.BeginChangeCheck();
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.LabelField("\t",GUILayout.Width(10));
						EditorGUILayout.LabelField("Variable",GUILayout.Width(100));
						EditorGUILayout.LabelField("Type",GUILayout.Width(60));
						EditorGUILayout.LabelField("Name",GUILayout.Width(100));
						EditorGUILayout.LabelField("Default",GUILayout.Width(100));
						EditorGUILayout.EndHorizontal();
						foreach(var item in Buffer.active.properties){
							EditorGUILayout.BeginHorizontal();
							Property property = item.Value;
							string type = property.type.ToString();
							EditorGUILayout.LabelField("",GUILayout.Width(10));
							property.variable = EditorGUILayout.TextField(property.variable,GUILayout.Width(100));
							property.type = (PropertyType)EditorGUILayout.EnumPopup(property.type,GUILayout.Width(60));
							property.name = EditorGUILayout.TextField(property.name,GUILayout.Width(100));
							if(type != property.type.ToString()){
								property.SetDefault();
								continue;
							}
							if(type == "Range"){
								property.minimum = EditorGUILayout.FloatField(property.minimum,GUILayout.Width(30));
								property.maximum = EditorGUILayout.FloatField(property.maximum,GUILayout.Width(30));
								property.defaultValue = EditorGUILayout.FloatField((float)property.defaultValue,GUILayout.Width(64));
							}
							else if(type == "Color"){
								property.defaultValue = EditorGUILayout.ColorField((Color)property.defaultValue,GUILayout.Width(128));
							}
							else if(type == "Texture" || type == "Rect" || type == "Cube"){
								property.texgenMode = (TexGen)EditorGUILayout.EnumPopup(property.texgenMode,GUILayout.Width(64));
								property.defaultValue = (TextureDefault)EditorGUILayout.EnumPopup((Enum)property.defaultValue,GUILayout.Width(64));
							}
							else{property.defaultValue = EditorGUILayout.TextField(property.defaultValue.ToString(),GUILayout.Width(132));}
							EditorGUILayout.EndHorizontal();
							this.hoverObject = this.CheckHover() ? property : this.hoverObject;
						}
						bool changes = EditorGUI.EndChangeCheck();
						if(GUILayout.Button("Add Property",this.UI.button,GUILayout.Width(115))){
							//Undo.RegisterUndo(Buffer.active,"Shader - Add Property");
							Buffer.active.AddProperty();
							this.stateChanged = "Event-AddProperty";
						}
						this.stateChanged = changes ? "Property" : this.stateChanged;
						Buffer.branchable = !changes;
					}
					int shaderIndex = 0;
					int passIndex = 0;
					foreach(SubShader subShader in Buffer.active.subShaders){
						shaderIndex += 1;
						string hash = "Sub"+shaderIndex;
						bool showSubShaders = Buffer.active.subShaders.Count > 0;
						if(!Buffer.options.ContainsKey(hash)){continue;}
						int passAmount = subShader.passes.Count;
						string passInfo = passAmount > 1 ? " ["+passAmount+" passes]" : " [1 pass]";
						if(showSubShaders){Buffer.options[hash] = this.DrawFold("SubShader"+passInfo,Buffer.options[hash]);}
						this.hoverObject = this.CheckHover() ? subShader : this.hoverObject;
						if(showSubShaders && !Buffer.options[hash]){continue;}
						if(showSubShaders){++EditorGUI.indentLevel;}
						this.DrawCommon(subShader);
						bool hideTags = !Buffer.options["ShowDefault"] && subShader.tags.IsDefault();
						if(!hideTags){
							Buffer.options[hash+"Tags"] = this.DrawFold("Tags",Buffer.options[hash+"Tags"]);
							if(Buffer.options[hash+"Tags"]){
								++EditorGUI.indentLevel;
								subShader.tags.lightMode = (LightMode)this.Draw("Light Mode",subShader.tags.lightMode);
								subShader.tags.require = (Require)this.Draw("Require",subShader.tags.require);
								subShader.tags.renderQueue = (RenderQueue)this.Draw("Render Queue",subShader.tags.renderQueue);
								if(subShader.tags.renderQueue != RenderQueue.Default){
									++EditorGUI.indentLevel;
									subShader.tags.renderQueueOffset = (int)this.Draw("Offset",subShader.tags.renderQueueOffset);
									--EditorGUI.indentLevel;
								}
								subShader.tags.renderType = (RenderType)this.Draw("Render Type",subShader.tags.renderType);
								subShader.tags.ignoreProjector = (bool)this.Draw("Ignore Projector",subShader.tags.ignoreProjector,false);
								subShader.tags.forceNoShadowCasting = (bool)this.Draw("No Shadow Casting",subShader.tags.forceNoShadowCasting,false);
								--EditorGUI.indentLevel;
							}
						}
						this.DrawFog(subShader.fog,hash);
						string resetPass = "";
						PassType resetType = PassType.Normal;
						foreach(var item in subShader.passes){
							passIndex += 1;
							Pass pass = item.Value;
							string passHash = "Pass"+passIndex;
							string passName = pass.name != "" && !pass.name.Contains("!") ? "Pass ["+pass.name+"]" : "Pass";
							if(pass.type == PassType.Use){passName = "UsePass";}
							if(pass.type == PassType.Grab){passName = "GrabPass";}
							Buffer.options[passHash] = this.DrawFold(passName,Buffer.options[passHash]);
							this.hoverObject = this.CheckHover() ? new object[]{subShader,pass} : this.hoverObject;
							if(!Buffer.options[passHash]){continue;}
							++EditorGUI.indentLevel;
							PassType passType = pass.type;
							pass.type = (PassType)this.Draw("Type",pass.type);
							if(pass.type != passType){
								resetPass = item.Key;
								resetType = pass.type;
								continue;
							}
							if(pass.type == PassType.Normal){
								pass.name = (string)this.Draw("Name",pass.name,"");
								this.DrawCommon(pass);
								bool hidePassTags = !Buffer.options["ShowDefault"] && pass.tags.IsDefault();
								if(!hidePassTags){
									Buffer.options[passHash+"Tags"] = this.DrawFold("Tags",Buffer.options[passHash+"Tags"]);
									if(Buffer.options[passHash+"Tags"]){
										++EditorGUI.indentLevel;
										pass.tags.lightMode = (LightMode)this.Draw("Light Mode",pass.tags.lightMode);
										pass.tags.require = (Require)this.Draw("Require",pass.tags.require);
										--EditorGUI.indentLevel;
									}
								}
								this.DrawFog(pass.fog,passHash);
								bool hideGPUShader = !Buffer.options["ShowDefault"] && pass.gpuShader == "";
								if(!hideGPUShader){
									EditorGUI.BeginChangeCheck();
									pass.gpuShader = EditorGUILayout.TextArea(pass.gpuShader,GUILayout.Width(Screen.width-45));
									this.stateChanged = EditorGUI.EndChangeCheck() ? "TextArea" : this.stateChanged;
								}
							}
							else if(pass.type == PassType.Use){
								pass.usePass = (string)this.Draw("Shader",pass.usePass);
							}
							else if(pass.type == PassType.Grab){
								pass.grabPass = (string)this.Draw("Texture",pass.grabPass,"");
							}
							--EditorGUI.indentLevel;
						}
						if(resetPass != ""){
							subShader.passes[resetPass] = new Pass();
							subShader.passes[resetPass].type = resetType;
						}
						if(GUILayout.Button("Add Pass",this.UI.button,GUILayout.Width(115))){
							//Undo.RegisterUndo(Buffer.active,"Shader - Add Pass");
							subShader.AddPass();
							this.LoadSettings();
							this.stateChanged = "Event-AddPass";
						}
						if(showSubShaders){--EditorGUI.indentLevel;}
					}
					if(GUILayout.Button("Add SubShader",GUILayout.Width(115))){
						//Undo.RegisterUndo(Buffer.active,"Shader - Add SubShader");
						Buffer.active.AddSubShader();
						this.LoadSettings();
						this.stateChanged = "Event-AddSubShader";
					}
				}
				//Buffer.options["ShaderGPU"] = this.DrawTitle("GPU Shader",Buffer.options["ShaderGPU"]);
				Buffer.options["Material"] = this.DrawTitle("Material",Buffer.options["Material"]);
				if(Buffer.options["Material"]){
					EditorGUIUtility.labelWidth = Screen.width - 84;
					base.OnInspectorGUI();
				}
				if(this.stateChanged != "" || this.titleChanged != ""){
					foreach(var item in Buffer.options){
						string key = item.Key;
						int value = item.Value.ToInt();
						string settingPrefix = key.ContainsAny("ShowDefault","ShowPreview") ? "ExtendedMaterial-" : Buffer.material.name+"-";
						PlayerPrefs.SetInt(settingPrefix+key,value);
					}
					this.warning = "";
					EditorUtility.SetDirty(material);
				}
				if(this.titleChanged != "" || this.stateChanged.Contains("ToggleButton")){
					GUI.FocusControl("Menu");
				}
				if(this.stateChanged != ""){
					Buffer.unsaved = true;
					if(Buffer.options["ShowPreview"]){
						Buffer.buildDelay = Time.realtimeSinceStartup;
						Buffer.buildPreview = true;
					}
					if(this.stateChanged.Contains("TextArea")){Buffer.buildDelay += 2.0f;}
					if(this.stateChanged.Contains("Field")){Buffer.buildDelay += 1.5f;}
					if(this.stateChanged.Contains("Color")){Buffer.buildDelay += 0.5f;}
					//Debug.Log("[ExtendedMaterial] Value changed -- " + this.stateChanged);
				}
			}
			this.CheckContext();
		}
		public static void CheckPreview(){
			if(Buffer.buildPreview && Buffer.material != null && Time.realtimeSinceStartup > Buffer.buildDelay){
				string path = Buffer.material.shader.name;
				if(!path.Contains("Hidden/Preview/")){Buffer.originalPath = path;}
				Buffer.active.path = Buffer.active.path.Remove("-Preview").Replace(".shader","-Preview.shader");
				Buffer.active.menuPath = "Hidden/Preview/"+Buffer.active.menuPath.Remove("Hidden/Preview/");
				Buffer.refresh = true;
				Buffer.material.shader = Buffer.active.Save();
				Buffer.shader = Buffer.material.shader;
				Buffer.buildPreview = false;
			}
		}
		public void EndPreview(){
			Buffer.active.path = Buffer.active.path.Remove("-Preview");
			Buffer.active.menuPath = Buffer.active.menuPath.Remove("Hidden/Preview/");
		}
		public void FixPreviewShader(bool force=false){
			Material material = (Material)this.target;
			if(force || (Buffer.material != material && Buffer.material != null)){
				string name = material.shader.name;
				if(name.Contains("Hidden/Preview/")){
					Shader shader = Shader.Find(Buffer.originalPath);
					Func<Shader,bool> Validate = item => item == null || item.name.Contains("Hidden/Preview");
					if(Validate(shader) && Buffer.active != null){shader = Shader.Find(Buffer.active.menuPath);}
					if(Validate(shader) && Buffer.material != null){shader = Shader.Find(Buffer.material.shader.name.Remove("Hidden/Preview/"));}
					if(Validate(shader)){shader = Shader.Find(material.shader.name.Remove("Hidden/Preview/"));}
					if(shader == null){
						Debug.LogWarning("Shader for material is a 'preview' shader, but original path could not be found to revert.  Please fix manually.");
						return;
					}
					if(Buffer.material != null){Buffer.material.shader = shader;}
					Debug.Log("[ExtendedMaterial] Shader reverted -- " + shader.name);
					material.shader = shader;
					Buffer.unsaved = false;
				}
			}
		}
		public override void OnEnable(){
			if(EditorApplication.update != CheckPreview){
				EditorApplication.update += CheckPreview;
			}
			this.FixPreviewShader();
			base.OnEnable();
		}
		public override void OnDisable(){
			this.FixPreviewShader();
			base.OnDisable();
		}
	}
}