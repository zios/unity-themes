using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEvent = UnityEngine.Event;
#pragma warning disable 618
namespace Zios.Editors.MaterialEditors{
	using Interface;
	using Undo = UnityEditor.Undo;
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
			bool targetMismatch = MaterialBuffer.active == null || MaterialBuffer.material == null || (material.shader != MaterialBuffer.shader);
			bool newMaterial = MaterialBuffer.material != material;
			if(targetReady && (MaterialBuffer.refresh || targetMismatch)){
				if(newMaterial){MaterialBuffer.originalPath = "";}
				MaterialBuffer.material = material;
				MaterialBuffer.shader = material.shader;
				if(!MaterialBuffer.refresh || MaterialBuffer.active == null){
					if(MaterialBuffer.active != null){DestroyImmediate(MaterialBuffer.active);}
					MaterialBuffer.active = ScriptableObject.CreateInstance<ExtendedMaterial>();
				}
				MaterialBuffer.active.Clear();
				MaterialBuffer.active.Load(material.shader);
				MaterialBuffer.refresh = false;
				MaterialBuffer.unsaved = false;
				this.LoadSettings();
				Debug.Log("[ExtendedMaterial] Material loaded -- " + material.name + " [" + material.shader.name + "]");
			}
		}
		public void LoadSettings(){
			string name = MaterialBuffer.material.name;
			bool firstOpen = !Utility.HasPref(name+"-Material");
			MaterialBuffer.options["ShowDefault"] = firstOpen ? true : Utility.GetPref<int>("ExtendedMaterial-ShowDefault").ToBool();
			MaterialBuffer.options["ShowPreview"] = firstOpen ? true : Utility.GetPref<int>("ExtendedMaterial-ShowPreview").ToBool();
			MaterialBuffer.options["ShaderUnity"] = Utility.GetPref<int>(name+"-ShaderUnity").ToBool();
			MaterialBuffer.options["ShaderGPU"] = Utility.GetPref<int>(name+"-ShaderGPU").ToBool();
			MaterialBuffer.options["Properties"] = Utility.GetPref<int>(name+"-Properties").ToBool();
			MaterialBuffer.options["Material"] = firstOpen ? true : Utility.GetPref<int>(name+"-Material").ToBool();
			int shaderIndex = 0;
			int passIndex = 0;
			foreach(SubShader subShader in MaterialBuffer.active.subShaders){
				shaderIndex += 1;
				string shaderHash = "Sub"+shaderIndex;
				MaterialBuffer.options[shaderHash] = firstOpen ? true : Utility.GetPref<int>(name+"-"+shaderHash).ToBool();
				MaterialBuffer.options[shaderHash+"Tags"] = Utility.GetPref<int>(name+"-"+shaderHash+"Tags").ToBool();
				MaterialBuffer.options[shaderHash+"Fog"] = Utility.GetPref<int>(name+"-"+shaderHash+"Fog").ToBool();
				foreach(var item in subShader.passes){
					passIndex += 1;
					item.Value.name = item.Value.name;
					string passHash = "Pass"+passIndex;
					MaterialBuffer.options[passHash] = firstOpen ? true : Utility.GetPref<int>(name+"-"+passHash).ToBool();
					MaterialBuffer.options[passHash+"Tags"] = Utility.GetPref<int>(name+"-"+passHash+"Tags").ToBool();
					MaterialBuffer.options[passHash+"Fog"] = Utility.GetPref<int>(name+"-"+passHash+"Fog").ToBool();
				}
			}
		}
		public object Draw(string label,object field,object ignore=default(object)){
			EditorGUI.BeginChangeCheck();
			GUILayoutOption[] size = new GUILayoutOption[]{GUILayout.Width(165),GUILayout.Height(18)};
			GUILayoutOption[] half = new GUILayoutOption[]{GUILayout.Width(80),GUILayout.Height(18)};
			bool hideDefault = (field is Enum && (int)field == 0);
			hideDefault = hideDefault || (ignore != default(object) && ignore.Equals(field));
			if(hideDefault && !MaterialBuffer.options["ShowDefault"]){return field;}
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
				Undo.RegisterUndo(MaterialBuffer.active,"Shader Edit - " + label);
			}
			return field;
		}
		public bool DrawTitle(string title,bool current){
			EditorGUI.BeginChangeCheck();
			string skinSuffix = EditorGUIUtility.isProSkin || Utility.GetPref<bool>("EditorTheme-Dark",false) ? "Dark" : "Light";
			skinSuffix += current ? "Open" : "";
			GUIStyle styleTitle = this.UI.GetStyle("Title"+skinSuffix);
			string arrow = "<color=#008ffeFF>▼</color>";
			string state = current ? arrow : "▶";
			bool pressed = GUILayout.Button(state+" "+title,styleTitle) ? !current : current;
			this.titleChanged = EditorGUI.EndChangeCheck() ? title : this.titleChanged;
			return pressed;
		}
		public bool DrawToggleButton(string label,bool state){
			string skinSuffix = EditorGUIUtility.isProSkin || Utility.GetPref<bool>("EditorTheme-Dark",false) ? "Dark" : "Light";
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
			if(Utility.IsRepainting()){
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
			bool showBlend = MaterialBuffer.options["ShowDefault"] || (custom || extended);
			if(preset != common.blendPreset && custom){
				common.blend = new Blend[2]{Blend.One,Blend.Zero};
			}
			if(common.blendPreset != BlendPreset.Off && showBlend){
				++EditorGUI.indentLevel;
				EditorGUI.BeginDisabledGroup(!custom && !extended);
				string blendName = extended ? "Color" : "Color & Alpha";
				common.blend = this.Draw(blendName,common.blend).As<List<object>>().ConvertAll<object,Blend>();
				if(extended){common.blendAlpha = this.Draw("Alpha",common.blendAlpha).As<List<object>>().ConvertAll<object,Blend>();}
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
			bool hideFog = !MaterialBuffer.options["ShowDefault"] && (fog.IsDefault() || fogModeDefault);
			if(!hideFog){
				MaterialBuffer.options[hash+"Fog"] = EditorGUILayout.Foldout(MaterialBuffer.options[hash+"Fog"],"Fog");
				if(MaterialBuffer.options[hash+"Fog"]){
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
				Action flagDirty = ()=> MaterialBuffer.buildPreview = true;
				if(hover is Property){
					Property property = (Property)hover;
					typeName = "Property ["+property.name+"]";
					removeMethod = ()=>MaterialBuffer.active.properties.RemoveValue(property);
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
					removeMethod = ()=>MaterialBuffer.active.RemoveSubShader(subShader);
				}
				GUIContent field = new GUIContent("Remove "+typeName);
				menu.AddItem(field,false,new GenericMenu.MenuFunction(removeMethod+flagDirty));
				menu.ShowAsContext();
				MaterialBuffer.unsaved = true;
				this.hoverObject = null;
				UnityEvent.current.Use();
			}
		}
		public override void OnInspectorGUI(){
			if(!UnityEvent.current.IsUseful()){return;}
			EditorUI.Reset();
			this.Setup(true);
			if(this.isVisible && MaterialBuffer.active != null){
				Material material = MaterialBuffer.material;
				this.stateChanged = "";
				this.titleChanged = "";
				string unsaved = MaterialBuffer.unsaved ? "*" : "";
				MaterialBuffer.options["ShaderUnity"] = this.DrawTitle("Unity Shader"+unsaved,MaterialBuffer.options["ShaderUnity"]);
				if(MaterialBuffer.options["ShaderUnity"]){
					this.drawn = -2;
					bool isPreview = MaterialBuffer.active.menuPath.Contains("Hidden/Preview/");
					bool isBranch = MaterialBuffer.active.fileName.Contains("#");
					bool exists = Shader.Find(MaterialBuffer.active.menuPath) != null;
					EditorGUIUtility.labelWidth = Screen.width - 207;
					EditorGUILayout.BeginHorizontal();
					if(!isBranch && GUILayout.Button("Save")){
						string warning = "Saving will overwrite the original shader and effect all materials that use it.";
						warning += "  It is advised to instead use the Save As or Branch option.";
						bool confirm = EditorUI.DrawDialog("Are you sure?",warning,"Save","Cancel");
						if(confirm){
							this.EndPreview();
							MaterialBuffer.unsaved = false;
							MaterialBuffer.material.shader = MaterialBuffer.active.Save();
							Debug.Log("[ExtendedMaterial] Shader saved -- " + MaterialBuffer.active.path);
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
							MaterialBuffer.unsaved = false;
							string path = EditorUtility.SaveFilePanel("Save Shader",MaterialBuffer.active.path,MaterialBuffer.active.fileName,"shader");
							MaterialBuffer.material.shader = MaterialBuffer.active.Save(path);
							Debug.Log("[ExtendedMaterial] Shader saved -- " + path);
							return;
						}
					}
					if((MaterialBuffer.unsaved || isPreview) && GUILayout.Button("Revert")){
						string warning = "Changes will be lost.";
						bool confirm = EditorUI.DrawDialog("Revert shader to defaults?",warning,"Revert","Cancel");
						if(confirm){
							Undo.RegisterUndo(MaterialBuffer.material,"Shader Edit - Revert");
							this.EndPreview();
							this.FixPreviewShader(true);
							return;
						}
					}
					if(GUILayout.Button("Branch")){
						this.EndPreview();
						MaterialBuffer.unsaved = false;
						MaterialBuffer.active.Branch();
						Debug.Log("[ExtendedMaterial] Shader branched -- " + MaterialBuffer.active.path);
						return;
					}
					EditorGUILayout.EndHorizontal();
					EditorGUILayout.BeginHorizontal();
					MaterialBuffer.options["ShowDefault"] = this.DrawToggleButton("Show Default",MaterialBuffer.options["ShowDefault"]);
					MaterialBuffer.options["ShowPreview"] = this.DrawToggleButton("Show Preview",MaterialBuffer.options["ShowPreview"]);
					EditorGUILayout.EndHorizontal();
					if(this.warning != ""){EditorGUILayout.HelpBox(this.warning,MessageType.Warning);}
					EditorGUI.BeginDisabledGroup(true);
					MaterialBuffer.active.fileName = (string)this.Draw("File",MaterialBuffer.active.fileName.Remove("-Preview"));
					EditorGUI.EndDisabledGroup();
					string pathPrefix = isPreview ? "Hidden/Preview/" : "";
					MaterialBuffer.active.menuPath = pathPrefix+(string)this.Draw("Menu",MaterialBuffer.active.menuPath.Remove("\\","Hidden/Preview/"));
					MaterialBuffer.active.fallback = (string)this.Draw("Fallback",MaterialBuffer.active.fallback,"");
					MaterialBuffer.active.editor = (string)this.Draw("Editor",MaterialBuffer.active.editor,"");
					MaterialBuffer.options["Properties"] = this.DrawFold("Properties",MaterialBuffer.options["Properties"]);
					if(MaterialBuffer.options["Properties"]){
						EditorGUI.BeginChangeCheck();
						EditorGUILayout.BeginHorizontal();
						EditorGUILayout.LabelField("\t",GUILayout.Width(10));
						EditorGUILayout.LabelField("Variable",GUILayout.Width(100));
						EditorGUILayout.LabelField("Type",GUILayout.Width(60));
						EditorGUILayout.LabelField("Name",GUILayout.Width(100));
						EditorGUILayout.LabelField("Default",GUILayout.Width(100));
						EditorGUILayout.EndHorizontal();
						foreach(var item in MaterialBuffer.active.properties){
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
							MaterialBuffer.active.AddProperty();
							this.stateChanged = "Event-AddProperty";
						}
						this.stateChanged = changes ? "Property" : this.stateChanged;
						MaterialBuffer.branchable = !changes;
					}
					int shaderIndex = 0;
					int passIndex = 0;
					foreach(SubShader subShader in MaterialBuffer.active.subShaders){
						shaderIndex += 1;
						string hash = "Sub"+shaderIndex;
						bool showSubShaders = MaterialBuffer.active.subShaders.Count > 0;
						if(!MaterialBuffer.options.ContainsKey(hash)){continue;}
						int passAmount = subShader.passes.Count;
						string passInfo = passAmount > 1 ? " ["+passAmount+" passes]" : " [1 pass]";
						if(showSubShaders){MaterialBuffer.options[hash] = this.DrawFold("SubShader"+passInfo,MaterialBuffer.options[hash]);}
						this.hoverObject = this.CheckHover() ? subShader : this.hoverObject;
						if(showSubShaders && !MaterialBuffer.options[hash]){continue;}
						if(showSubShaders){++EditorGUI.indentLevel;}
						this.DrawCommon(subShader);
						bool hideTags = !MaterialBuffer.options["ShowDefault"] && subShader.tags.IsDefault();
						if(!hideTags){
							MaterialBuffer.options[hash+"Tags"] = this.DrawFold("Tags",MaterialBuffer.options[hash+"Tags"]);
							if(MaterialBuffer.options[hash+"Tags"]){
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
							MaterialBuffer.options[passHash] = this.DrawFold(passName,MaterialBuffer.options[passHash]);
							this.hoverObject = this.CheckHover() ? new object[]{subShader,pass} : this.hoverObject;
							if(!MaterialBuffer.options[passHash]){continue;}
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
								bool hidePassTags = !MaterialBuffer.options["ShowDefault"] && pass.tags.IsDefault();
								if(!hidePassTags){
									MaterialBuffer.options[passHash+"Tags"] = this.DrawFold("Tags",MaterialBuffer.options[passHash+"Tags"]);
									if(MaterialBuffer.options[passHash+"Tags"]){
										++EditorGUI.indentLevel;
										pass.tags.lightMode = (LightMode)this.Draw("Light Mode",pass.tags.lightMode);
										pass.tags.require = (Require)this.Draw("Require",pass.tags.require);
										--EditorGUI.indentLevel;
									}
								}
								this.DrawFog(pass.fog,passHash);
								bool hideGPUShader = !MaterialBuffer.options["ShowDefault"] && pass.gpuShader == "";
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
						MaterialBuffer.active.AddSubShader();
						this.LoadSettings();
						this.stateChanged = "Event-AddSubShader";
					}
				}
				//Buffer.options["ShaderGPU"] = this.DrawTitle("GPU Shader",Buffer.options["ShaderGPU"]);
				MaterialBuffer.options["Material"] = this.DrawTitle("Material",MaterialBuffer.options["Material"]);
				if(MaterialBuffer.options["Material"]){
					EditorGUIUtility.labelWidth = Screen.width - 84;
					base.OnInspectorGUI();
				}
				if(this.stateChanged != "" || this.titleChanged != ""){
					foreach(var item in MaterialBuffer.options){
						string key = item.Key;
						int value = item.Value.ToInt();
						string settingPrefix = key.ContainsAny("ShowDefault","ShowPreview") ? "ExtendedMaterial-" : MaterialBuffer.material.name+"-";
						Utility.SetPref<int>(settingPrefix+key,value);
					}
					this.warning = "";
					Utility.SetDirty(material);
				}
				if(this.titleChanged != "" || this.stateChanged.Contains("ToggleButton")){
					GUI.FocusControl("Menu");
				}
				if(this.stateChanged != ""){
					MaterialBuffer.unsaved = true;
					if(MaterialBuffer.options["ShowPreview"]){
						MaterialBuffer.buildDelay = Time.realtimeSinceStartup;
						MaterialBuffer.buildPreview = true;
					}
					if(this.stateChanged.Contains("TextArea")){MaterialBuffer.buildDelay += 2.0f;}
					if(this.stateChanged.Contains("Field")){MaterialBuffer.buildDelay += 1.5f;}
					if(this.stateChanged.Contains("Color")){MaterialBuffer.buildDelay += 0.5f;}
					//Debug.Log("[ExtendedMaterial] Value changed -- " + this.stateChanged);
				}
			}
			this.CheckContext();
		}
		public static void CheckPreview(){
			if(MaterialBuffer.buildPreview && MaterialBuffer.material != null && Time.realtimeSinceStartup > MaterialBuffer.buildDelay){
				string path = MaterialBuffer.material.shader.name;
				if(!path.Contains("Hidden/Preview/")){MaterialBuffer.originalPath = path;}
				MaterialBuffer.active.path = MaterialBuffer.active.path.Remove("-Preview").Replace(".shader","-Preview.shader");
				MaterialBuffer.active.menuPath = "Hidden/Preview/"+MaterialBuffer.active.menuPath.Remove("Hidden/Preview/");
				MaterialBuffer.refresh = true;
				MaterialBuffer.material.shader = MaterialBuffer.active.Save();
				MaterialBuffer.shader = MaterialBuffer.material.shader;
				MaterialBuffer.buildPreview = false;
			}
		}
		public void EndPreview(){
			MaterialBuffer.active.path = MaterialBuffer.active.path.Remove("-Preview");
			MaterialBuffer.active.menuPath = MaterialBuffer.active.menuPath.Remove("Hidden/Preview/");
		}
		public void FixPreviewShader(bool force=false){
			Material material = (Material)this.target;
			if(force || (MaterialBuffer.material != material && MaterialBuffer.material != null)){
				string name = material.shader.name;
				if(name.Contains("Hidden/Preview/")){
					Shader shader = Shader.Find(MaterialBuffer.originalPath);
					Func<Shader,bool> Validate = item => item == null || item.name.Contains("Hidden/Preview");
					if(Validate(shader) && MaterialBuffer.active != null){shader = Shader.Find(MaterialBuffer.active.menuPath);}
					if(Validate(shader) && MaterialBuffer.material != null){shader = Shader.Find(MaterialBuffer.material.shader.name.Remove("Hidden/Preview/"));}
					if(Validate(shader)){shader = Shader.Find(material.shader.name.Remove("Hidden/Preview/"));}
					if(shader == null){
						Debug.LogWarning("Shader for material is a 'preview' shader, but original path could not be found to revert.  Please fix manually.");
						return;
					}
					if(MaterialBuffer.material != null){MaterialBuffer.material.shader = shader;}
					Debug.Log("[ExtendedMaterial] Shader reverted -- " + shader.name);
					material.shader = shader;
					MaterialBuffer.unsaved = false;
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