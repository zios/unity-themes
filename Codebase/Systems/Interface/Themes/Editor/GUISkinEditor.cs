using UnityEditor;
using UnityEngine;
using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Zios.Interface;
using UnityEvent = UnityEngine.Event;
namespace Zios.Editors{
	using Event;
	[CustomEditor(typeof(GUISkin))]
	public partial class GUISkinEditor : Editor{
		public static GUIStyle focus;
		public static string action;
		public bool refocus;
		public bool advanced;
		public bool changes;
		public bool isFragment;
		public int viewMode;
		public int inputMode;
		public string queue;
		public string queueSplit;
		public Action queueMethod = ()=>{};
		public string hash;
		public List<string> inputTerms = new List<string>();
		public List<SearchAttribute> searchAttributes = new List<SearchAttribute>();
		public GUIStyle[] searchResults = new GUIStyle[0];
		public GUISkin[] fragments = new GUISkin[0];
		public GUISkin skin;
		//=================================
		// Main
		//=================================
		public override void OnInspectorGUI(){
			if(UnityEvent.current.type.MatchesAny("mouseMove") || Utility.IsBusy()){return;}
			EditorUI.Reset();
			this.hash = this.hash ?? Utility.GetInspector(this).GetInstanceID().ToString();
			this.skin = this.skin ?? this.target.As<GUISkin>();
			this.isFragment = this.skin.name.Contains("#");
			//this.optimize = Utility.GetPref<bool>("GUISkinEditor-Optimize",false);
			EditorUI.foldoutChanged = false;
			this.DrawViewMode();
			if(this.viewMode == 0){
				this.DrawDefaultInspector();
				return;
			}
			if(!UnityEvent.current.type.MatchesAny("Repaint","Layout","scrollWheel","used")){
				Utility.RecordObject(this.skin,"GUI Skin Changes");
				foreach(var fragment in this.fragments){
					Utility.RecordObject(fragment,"GUI Skin Changes");
				}
			}
			GUI.changed = false;
			this.drawn = false;
			this.count = 0;
			this.fragments = FileManager.GetAssets<GUISkin>(this.skin.name+"#*.guiskin",false);
			this.ProcessMenu();
			this.DrawSearch();
			//this.DrawSplit();
			if(this.inputMode == 0 && this.inputTerms.Count == 0 || this.inputTerms[0] == "Search"){
				this.DrawStandard();
				this.DrawCustom();
				this.DrawFragmented();
			}
			this.CheckChanges();
			this.CheckReset();
			GUILayout.Space(this.lowerBounds);
		}
		public void DrawViewMode(){
			var term = "Default";
			this.viewMode = Utility.GetPref<int>("GUISkin-Mode",0);
			if(this.viewMode == 1){term = "Replica";}
			if(this.viewMode == 2){term = "Compact";}
			if(term.ToLabel().Layout(0,30).DrawButton()){
				this.viewMode = (this.viewMode + 1) % 3;
				Utility.SetPref<int>("GUISkin-Mode",this.viewMode);
				EditorUI.foldoutChanged = true;
			}
		}
		public bool DrawInputMode(string fallback="Search",string splitBy=" ",string endSymbol="+"){
			EditorGUILayout.BeginHorizontal();
			var color = EditorStyles.textField.normal.textColor.SetAlpha(0.5f);
			var baseStyle = EditorStyles.textField.Alignment("MiddleCenter");
			var symbolStyle = baseStyle.FixedWidth(24).FontSize(16).TextColor(color);
			var inputStyle = baseStyle.FontStyle("BoldAndItalic");
			EditorUI.SetLayout(-1,30);
			if("∗".ToLabel().DrawButton(symbolStyle.ContentOffset(1,0).Overflow(0,3,0,0),false)){
				var menu = new EditorMenu();
				Action clear = ()=>this.queue="";
				menu.Add("Search",this.inputMode==0,(()=>{this.inputMode=0;})+clear);
				//menu.Add("Split",this.inputMode==1,(()=>{this.inputMode=1;})+clear);
				menu.Draw();
			}
			var filter = this.inputTerms.Join(splitBy).Replace("~"," ").Draw(null,inputStyle);
			var inputChanged = EditorUI.lastChanged;
			EditorUI.lastChanged = false;
			endSymbol.ToLabel().DrawButton(symbolStyle.ContentOffset(-2,0).Overflow(3,0,0,0),false);
			EditorUI.SetLayout(-1,0);
			EditorGUILayout.EndHorizontal();
			bool open = false;
			var output = new StringBuilder();
			foreach(var symbol in filter){
				if(symbol=='['){open=true;}
				if(symbol==']'){open=false;}
				output.Append(symbol==' ' && open ? '~' : symbol);
			}
			this.queue = this.queue ?? (output.ToString().Trim().IsEmpty() ? fallback : output.ToString());
			this.queueSplit = splitBy;
			Utility.SetPref<string>("GUISkin-"+fallback+"-"+this.hash,this.inputTerms.Join(splitBy));
			GUI.changed = false;
			return inputChanged;
		}
		public void DrawSearch(){
			if(this.inputMode != 0){return;}
			var inputChanged = this.DrawInputMode();
			this.advanced = EditorUI.lastChanged ? !this.advanced : this.advanced;
			if(inputChanged || (this.advanced && this.DrawAdvancedSearch())){
				this.queueMethod = ()=>{this.searchResults = this.PerformSearch(null,!this.isFragment);};
			}
			if(this.inputTerms.Count > 0 && this.inputTerms[0] != "Search"){
				if(this.searchResults.Length < 1){
					EditorGUI.indentLevel += 1;
					"No results found.".ToLabel().DrawLabel(EditorStyles.label.Alignment("MiddleCenter"),false);
					EditorGUI.indentLevel -= 1;
				}
				this.DrawStyles(this.hash,this.searchResults);
			}
		}
		public bool DrawAdvancedSearch(){
			EditorGUILayout.BeginVertical(GUI.skin.box.Margin(20,15,4,4).Padding(0,0,0,0));
			EditorUI.anyChanged = false;
			var queue = new List<string>();
			var refocus = false;
			foreach(var term in this.inputTerms){
				GUI.changed = false;
				var attribute = term.Contains("=") ? term.Split("=")[0].Trim("[") : "name";
				var value = term.Contains("=") ? term.Split("=")[1].Trim("]").Replace("~"," ") : term;
				attribute = attribute.Replace("textClipping","clipping").Replace("color","textColor");
				if(attribute=="name" && value=="Search"){continue;}
				if(attribute.IsEnum<GUIStyleField>()){
					EditorGUILayout.BeginHorizontal();
					if("x".ToLabel().Layout(25,16).DrawButton(GUI.skin.button.Margin(4,2,2,2).Padding(0,0,-2,0))){
						EditorUI.ResetLayout();
						EditorGUILayout.EndHorizontal();
						continue;
					}
					var field = attribute.ToEnum<GUIStyleField>();
					attribute = field.As<GUIStyleField>().Layout(150).Draw().ToName();
					if(GUI.changed || value.IsEmpty()){
						field = attribute.ToEnum<GUIStyleField>();
						value = "0";
					}
					var lookup = field.ToInt();
					EditorUI.SetLayoutOnce((Screen.width-255),16);
					if(lookup.MatchesAny(1,21)){
						value = value == "0" ? "#FFF" : value;
						if(value.IsColor(" ")){
							value = value.Contains(" ") ? value.ToColor(" ").Draw().ToDecimal(false) : value.ToColor(" ").Draw().ToHex(false);
						}
						else{value = value.Draw();}
					}
					if(lookup == 2){
						value = value == "0" ? "UnityWhite" : value;
						var image = value == "UnityWhite" ? Texture2D.whiteTexture : FileManager.GetAsset<Texture2D>(value+".*",false);
						if(!image.IsNull() && !value.Contains("*")){
							image = image.Draw<Texture2D>();
							value = image.IsNull() ? "" : image.name;
						}
						else{value = value.Draw();}
					}
					if(lookup == 7){
						value = value == "0" ? "Arial" : value;
						var font = FileManager.GetAsset<Font>(value+".ttf",false) ?? FileManager.GetAsset<Font>(value+".otf",false);
						if(!font.IsNull() && !value.Contains("*")){
							font = font.Draw<Font>();
							value = font.IsNull() ? "" : font.name;
						}
						else{value = value.Draw();}
					}
					try{
						if(lookup == 0){
							value = value == "0" ? "button" : value;
							GUI.SetNextControlName("name");
							value = value.Draw();
							if(this.refocus){GUI.FocusControl("name");}
							if(value.Contains(" ")){refocus = true;}
						}
						if(lookup.Between(3,6)){
							value = value == "0" ? "0 0 0 0" : value;
							value = value.ToRectOffset().Layout(26).Draw("",true).Serialize();
						}
						if(lookup.MatchesAny(8,16,17)){value = value.ToFloat().Draw().ToString();}
						if(lookup == 9){value = value.ToEnum<FontStyle>().Draw().ToName();}
						if(lookup == 10){value = value.ToEnum<TextAnchor>().Draw().ToName();}
						if(lookup.MatchesAny(11,12,18,19)){value = value.ToBool().Draw().Serialize();}
						if(lookup.MatchesAny(13,20)){value = value.ToEnum<TextClipping>().Draw().ToName();}
						if(lookup == 14){value = value.ToEnum<ImagePosition>().Draw().ToName();}
						if(lookup == 15){
							value = value == "0" ? "0 0" : value;
							value = value.Replace(" ",",").ToVector2().DrawVector2().ToString().Remove("(",")",",",".0");
						}
					}
					catch{
						EditorUI.SetLayoutOnce((Screen.width-255),16);
						value = value.Draw();
					}
					EditorGUILayout.EndHorizontal();
					var command = lookup == 0 ? value : "["+attribute+"="+value.Replace(" ","~")+"]";
					queue.Add(command);
					continue;
				}
				queue.Add(term);
			}
			this.refocus = refocus;
            if("Add".ToLabel().DrawButton()){
				GUI.FocusControl("");
				queue.Add("[textColor=#FFF]");
            }
			this.queue = queue.Count > 0 ? queue.Join(" ") : "Search";
			EditorGUILayout.EndVertical();
			return EditorUI.anyChanged;
		}
		public void DrawSplit(){
			if(this.inputMode != 1){return;}
			this.DrawInputMode("Split | By | Terms"," | ","•");
			if(EditorUI.lastChanged){
				var original = this.inputTerms.Copy();
				GUIStyle[] styles = null;
				foreach(var term in original){
					this.inputTerms = term.AsList();
					styles = this.PerformSearch(styles,false,false);
					Debug.Log(this.inputTerms[0] + " -- " + this.searchResults.Length);
				}
			}
		}
		public void DrawStyles(string key,GUIStyle[] styles,bool editable=true){
			var compact = this.viewMode == 2;
			var changes = false;
			EditorGUI.indentLevel += 1;
			for(int index=0;index<styles.Length;++index){
				var style = styles[index];
				style = styles[index] = style ?? EditorStyles.label.Copy();
				if(this.CheckBounds(style)){
					GUI.changed = false;
					style.Draw(key,compact);
					changes = changes || GUI.changed;
					if(this.rebuild && Utility.IsRepainting()){
						this.allRect.Add(GUILayoutUtility.GetLastRect());
					}
					this.DrawMenu(style,editable);
				}
				else if(this.drawn){break;}
				this.count += 1;
			}
			this.changes = this.changes || changes;
			EditorGUI.indentLevel -= 1;
		}
		public void DrawStandard(){
			if(this.skin.name.Contains("#")){return;}
			EditorGUI.indentLevel += 1;
			if("Standard".ToLabel().DrawFoldout("GUISkin-Standard-"+this.hash)){
				this.DrawStyles(this.hash,this.skin.GetStyles(true,false),false);
			}
			EditorGUI.indentLevel -= 1;
		}
		public void DrawCustom(){
			if(!this.isFragment){EditorGUI.indentLevel += 1;}
			if(this.isFragment || "Custom".ToLabel().DrawFoldout("GUISkin-Custom-"+this.hash)){
				EditorGUI.indentLevel += 1;
				var size = this.skin.customStyles.Length.DrawIntDelayed("Size");
				EditorGUI.indentLevel -= 1;
				if(this.skin.customStyles.Length != size){
					this.skin.customStyles = this.skin.customStyles.Resize(size);
					EditorUI.foldoutChanged = true;
				}
				this.DrawStyles(this.hash,this.skin.customStyles);
			}
			if(!this.isFragment){EditorGUI.indentLevel -= 1;}
		}
		public void DrawFragmented(){
			if(this.fragments.Length > 0){
				GUILayout.Space(10);
				if("Fragments".ToLabel().Layout(0,30).DrawHeader("GUISkin-Fragments-"+this.hash,GUI.skin.GetStyle("LargeButton"))){
					GUILayout.Space(5);
					foreach(var fragment in fragments){
						EditorGUI.indentLevel += 1;
						var fragmentName = fragment.name.Split("#")[1];
						if(fragmentName.ToLabel().DrawFoldout("GUISkin-"+this.hash+"-"+fragment.name)){
							this.DrawStyles(fragmentName+"-"+this.hash,fragment.customStyles);
						}
						EditorGUI.indentLevel -= 1;
					}
				}
			}
		}
        public void DrawMenu(GUIStyle style,bool editable=false){
            if(GUILayoutUtility.GetLastRect().Clicked(1)){
				var menu = new EditorMenu();
				Action select = ()=>GUISkinEditor.focus=style;
				menu["Copy Style"] = (()=>GUISkinEditor.action="copy")+select;
                if(editable){
					menu["Duplicate Style"] = (()=>GUISkinEditor.action="duplicate")+select;
					menu["Delete Style"] = (()=>GUISkinEditor.action="delete")+select;
                }
                if(!GUISkinEditor.focus.IsNull()){
					Action paste = ()=>style.Use(GUISkinEditor.focus);
                    menu["/"] = null;
					menu["Paste Style"] = (()=>GUISkinEditor.action="paste")+paste;
                }
				menu.Draw();
			}
		}
		public GUIStyle[] PerformSearch(GUIStyle[] styles=null,bool includeStandard=true,bool includeFragments=true){
			this.BuildAttributes();
			if(styles.IsNull()){
				styles = this.skin.GetStyles(includeStandard,true);
				if(includeFragments){
					var fragments = this.fragments.SelectMany(x=>x.customStyles).ToArray();
					styles = styles.Concat(fragments);
				}
			}
			EditorUI.foldoutChanged = true;
			return styles.Where(x=>x.name.ContainsAll(this.FilterSearch()) && this.ContainsAttributes(x)).ToArray();
		}
		public void BuildAttributes(){
			this.searchAttributes.Clear();
			foreach(var input in this.inputTerms){
				if(!input.ContainsAll("[","]","=")){continue;}
				var term = input.Replace("~"," ").Replace(","," ");
				var state = term.Contains("/") ? term.Parse("[","/") : "";
				var name = term.Parse("[","=").Split("/").Last();
				var value = term.Parse("=","]");
				var search = value.Remove("*","#");
				var wild = value.Contains("*");
				var wildStart = wild && value.StartsWith("*");
				var wildEnd = wild && value.EndsWith("*");
				var wildSplit = wild ? value.Split("*") : new string[1]{""};
				Func<string,bool> method = (x)=>{return false;};
				if(!wild){method = (data)=>data.Matches(search,true);}
				else if(wildStart && wildEnd && wildSplit.Length > 2){method = (data)=>data.Contains(search,true);}
				else if(wildEnd){method = (data)=>data.StartsWith(search,true);}
				else if(wildStart){method = (data)=>data.EndsWith(search,true);}
				else{
					method = (data)=>{
						var startCheck = data.StartsWith(wildSplit.First(),true);
						var endCheck = data.EndsWith(wildSplit.Last(),true);
						return startCheck && endCheck;
					};
				};
				var searchAttribute = new SearchAttribute();
				searchAttribute.name = name;
				searchAttribute.value = value;
				searchAttribute.method = method;
				searchAttribute.state = state;
				this.searchAttributes.Add(searchAttribute);
			}
		}
		public bool ContainsAttributes(GUIStyle style){
			foreach(var input in this.searchAttributes){
				var name = input.name;
				var rawValue = input.value;
				var value = rawValue.Remove("*","#");
				var state = input.state;
				var CheckMatch = input.method;
				if(value.IsEmpty()){continue;}
				if(name.ContainsAny("textColor","background")){
					var found = false;
					var states = style.GetNamedStates();
					if(!state.IsEmpty() && states.ContainsKey(state)){
						states = states[state].AsArray().ToDictionary(x=>state,x=>x);
					}
					foreach(var item in states){
						var styleState = item.Value;
						if(name.Matches("textColor",true)){
							bool hasAlpha = !rawValue.Contains(" ") ? value.Length > 6 : value.Split(" ").Length == 4;
							bool hexMatch = !rawValue.Contains(" ") && CheckMatch(styleState.textColor.ToHex(hasAlpha));
							bool decimalMatch = rawValue.Contains(" ") && CheckMatch(styleState.textColor.ToDecimal(hasAlpha));
							if(hexMatch || decimalMatch){found = true;}
						}
						if(name.Matches("background",true) && !styleState.background.IsNull() && CheckMatch(styleState.background.name)){
							found = true;
						}
					}
					if(found){continue;}
					return false;
				}
				name = name.Replace("textClipping","clipping").Replace("color","textColor");
				if(name.IsEnum<GUIStyleField>()){
					var current = style.GetVariable(name);
					var type = style.GetVariableType(name);
					if(current.IsNull()){return false;}
					if(type.Is<RectOffset>()){
						if(!CheckMatch(current.As<RectOffset>().Serialize())){
							return false;
						}
					}
					else if(type.Is<Vector2>()){
						if(!CheckMatch(current.As<Vector2>().ToString().Remove(".0",",","(",")"))){
							return false;
						}
					}
					else if(type.Is<bool>()){
						if(value.ToBool()!=current.As<bool>()){
							return false;
						}
					}
					else if(type.IsAny<float,int>()){
						if(current.ToString()!=value){
							return false;
						}
					}
					else if(type.Is<Font>()){
						if(!CheckMatch(current.As<Font>().name)){
							return false;
						}
					}
					else if(!CheckMatch(current.ToString()) && !CheckMatch(current.ToString().ToTitleCase())){
						return false;
					}
				}
				else{
					return false;
				}
			}
			return true;
		}
		public string[] FilterSearch(){
			var terms = new List<string>();
			foreach(var term in this.inputTerms){
				if(term.ContainsAll("[","]","=")){continue;}
				terms.Add(term);
			}
			return terms.ToArray();
		}
		public void ProcessMenu(){
			if(Utility.IsRepainting() && !GUISkinEditor.focus.IsNull() && !GUISkinEditor.action.ContainsAny("copy","paste")){
				var target = GUISkinEditor.focus;
				var targetSkin = this.skin.customStyles.Contains(target) ? this.skin : this.fragments.Find(x=>x.customStyles.Contains(target));
				if(GUISkinEditor.action=="duplicate"){
					var index = targetSkin.customStyles.IndexOf(target);
					var styles = targetSkin.customStyles.ToList();
					styles.Insert(index,new GUIStyle(target));
					targetSkin.customStyles = styles.ToArray();
				}
				if(GUISkinEditor.action=="delete"){targetSkin.customStyles = targetSkin.customStyles.Remove(GUISkinEditor.focus);}
				GUISkinEditor.action = "";
				GUISkinEditor.focus = null;
				EditorUI.foldoutChanged = true;
				GUI.changed = true;
			}
		}
	}
	public struct SearchAttribute{
		public string name;
		public string value;
		public string state;
		public Func<string,bool> method;
	}
	//=================================
	// Optimize GUI Clipping
	//=================================
	public partial class GUISkinEditor{
		public bool optimize = true;
		public bool rebuild = true;
		public bool drawn;
		public int count;
		public int visibleStart;
		public int visibleEnd;
		public float upperBounds;
		public float lowerBounds;
		private Vector2 scroll;
		public List<Rect> allRect = new List<Rect>();
		public bool Rebuild(bool force=false){
			if(!this.optimize){return false;}
			if(force || EditorUI.foldoutChanged){
				this.allRect.Clear();
				this.upperBounds = 0;
				this.lowerBounds = 0;
				EditorUI.foldoutChanged = false;
				this.rebuild = true;
				this.Repaint();
				return true;
			}
			return false;
		}
		public void ResetVisible(){
			if(!this.optimize || this.rebuild){return;}
			this.scroll = Utility.GetInspectorScroll(this);
			var screen = new Rect(0,this.scroll.y,Screen.width,Screen.height);
			this.visibleStart = this.allRect.FindIndex(x=>screen.Overlaps(x));
			this.visibleEnd = this.allRect.Skip(this.visibleStart).ToList().FindIndex(x=>!screen.Overlaps(x));
			this.visibleEnd = this.visibleEnd == -1 ? this.allRect.Count-1 : this.visibleEnd+(this.visibleStart);
			if(this.visibleStart == -1){return;}
			this.upperBounds = this.allRect.Take(this.visibleStart).Select(x=>x.height+2).Sum();
			this.lowerBounds = this.allRect.Skip(this.visibleEnd+1).Select(x=>x.height+2).Sum();
			Utility.DelayCall(this.Repaint,0.25f);
		}
		public void CheckChanges(){
			if(this.changes){
				Utility.SetAssetDirty(this.target.As<GUISkin>());
				foreach(var fragment in fragments){
					Utility.SetAssetDirty(fragment);
				}
				Events.Call("On GUISkin Changed");
				this.changes = false;
			}
			if(!this.queue.IsNull()){
				var newTerms = this.queue.Split(this.queueSplit).ToList();
				if(!this.inputTerms.SequenceEqual(newTerms)){
					EditorUI.foldoutChanged = true;
					this.inputTerms = newTerms;
					this.queueMethod();
				}
				this.queue = null;
				this.queueMethod = ()=>{};
			}
		}
		public void CheckReset(){
			if(this.optimize){
				if(this.rebuild && this.allRect.Count == this.count){
					this.rebuild = false;
					this.ResetVisible();
				}
				if(!this.rebuild){
					if(Utility.IsRepainting()){
						if(GUI.changed || this.scroll.y != Utility.GetInspectorScroll(this).y){
							this.ResetVisible();
						}
					}
				}
				this.Rebuild();
			}
		}
		public bool CheckBounds(GUIStyle style){
			if(!this.optimize || this.rebuild){return true;}
			if((this.count < this.visibleStart) || (this.count > this.visibleEnd)){
				return false;
			}
			if(!this.drawn){
				GUILayout.Space(this.upperBounds);
				this.drawn = true;
			}
			return true;
		}
	}
}
namespace Zios.Interface{
	public static partial class GUISkinEditorExtensions{
		public static void Draw(this GUIStyle current,string key,bool compact=false,bool grouped=false,bool headers=false){
			EditorGUILayout.BeginVertical();
			var styleKey = key + "." + current.name;
			var styleFoldout = current.name.ToLabel().DrawFoldout(styleKey);
			if(styleFoldout){
				var labelWidth = compact ? 140 : 0;
				EditorGUI.indentLevel += 1;
				EditorUI.SetFieldSize(-1,labelWidth,false);
				current.name = current.name.Draw("Name".ToLabel());
				Utility.SetPref<bool>(key+"."+current.name,true);
				if(compact && headers){
					EditorGUILayout.BeginHorizontal();
					GUILayout.Space(140);
					EditorUI.SetLayout(120);
					"Text".ToLabel().DrawLabel(EditorStyles.boldLabel,false);
					"Background".ToLabel().DrawLabel(EditorStyles.boldLabel,false);
					EditorGUILayout.EndHorizontal();
				}
				foreach(var state in current.GetNamedStates()){
					state.Value.Draw(state.Key,compact,styleKey+"."+state.Key);
				}
				current.border.Draw("Border",compact,key+".Border");
				current.margin.Draw("Margin",compact,key+".Margin");
				current.padding.Draw("Padding",compact,key+".Padding");
				current.overflow.Draw("Overflow",compact,key+".Overflow");
				if(!grouped || "Text".ToTitleCase().ToLabel().DrawFoldout(key+"Text")){
					current.DrawTextSettings(compact);
				}
				if(!grouped || "Position & Size".ToTitleCase().ToLabel().DrawFoldout(key+"Area")){
					current.imagePosition = current.imagePosition.Draw("Image Position").As<ImagePosition>();
					current.contentOffset = current.contentOffset.DrawVector2("Content Offset");
					current.fixedWidth = current.fixedWidth.Draw("Fixed Width");
					current.fixedHeight = current.fixedHeight.Draw("Fixed Height");
					current.stretchWidth = current.stretchWidth.Draw("Stretch Width");
					current.stretchHeight = current.stretchHeight.Draw("Stretch Height");
				}
				EditorUI.ResetFieldSize();
				EditorGUI.indentLevel -= 1;
			}
			EditorGUILayout.EndVertical();
		}
		public static RectOffset Draw(this RectOffset current,string name="RectOffset",bool compact=false,string key=null){
			if(compact){
				EditorGUILayout.BeginHorizontal();
				EditorUI.SetFieldSize(26);
				current.left = current.left.DrawInt(name);
				EditorUI.SetLayout(26);
				current.right = current.right.DrawInt(null,null,false);
				current.top = current.top.DrawInt(null,null,false);
				current.bottom = current.bottom.DrawInt(null,null,false);
				EditorUI.SetLayout(0);
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
				return current;
			}
			if(name.ToTitleCase().ToLabel().DrawFoldout(key)){
				EditorGUI.indentLevel += 1;
				current.left = current.left.DrawInt("Left");
				current.right = current.right.DrawInt("Right");
				current.top = current.top.DrawInt("Top");
				current.bottom = current.bottom.DrawInt("Bottom");
				EditorGUI.indentLevel -= 1;
			}
			return current;
		}
		public static void DrawTextSettings(this GUIStyle current,bool compact=false){
			if(compact){
				EditorGUILayout.BeginHorizontal();
				current.fontSize = current.fontSize.Layout(171).DrawInt("Font",EditorStyles.textField);
				current.fontStyle = current.fontStyle.Layout(80).Draw(null,null,false).As<FontStyle>();
				EditorUI.SetLayoutOnce(Screen.width-305+EditorStyles.inspectorDefaultMargins.padding.left);
				current.font = current.font.Draw<Font>(null,false,false);
				EditorGUILayout.EndHorizontal();
			}
			else{
				current.font = current.font.Draw<Font>("Font");
				current.fontSize = current.fontSize.DrawInt("Font Size");
				current.fontStyle = current.fontStyle.Draw("Font Style").As<FontStyle>();
			}
			current.alignment = current.alignment.Draw("Alignment").As<TextAnchor>();
			current.wordWrap = current.wordWrap.Draw("Word Wrap");
			current.richText = current.richText.Draw("Rich Text");
			current.clipping = current.clipping.Draw("Text Clipping").As<TextClipping>();
		}
		public static void Draw(this GUIStyleState current,string name="GUIStyleState",bool compact=false,string key=null){
			if(compact){
				EditorGUILayout.BeginHorizontal();
				current.textColor = current.textColor.Layout(200).Draw(name.ToTitleCase());
				EditorUI.SetLayoutOnce(Screen.width-240,16);
				current.background = current.background.Draw<Texture2D>(null,true,false).As<Texture2D>();
				EditorGUILayout.EndHorizontal();
				return;
			}
			if(name.ToTitleCase().ToLabel().DrawFoldout(key)){
				EditorGUI.indentLevel += 1;
				current.background = current.background.Layout(0,15).Draw<Texture2D>("Background").As<Texture2D>();
				current.textColor = current.textColor.Draw("Text Color");
				#if UNITY_5_4_OR_NEWER
				if("Scaled Backgrounds".ToLabel().DrawFoldout(key+".Scaled")){
					EditorGUI.indentLevel += 1;
					var size = current.scaledBackgrounds.Length;
					var newSize = current.scaledBackgrounds.Length.DrawIntDelayed("Size");
					if(size != newSize){
						current.scaledBackgrounds = current.scaledBackgrounds.Resize(newSize);
						EditorUI.foldoutChanged = true;
					}
					for(int index=0;index<current.scaledBackgrounds.Length;++index){
						var background = current.scaledBackgrounds[index];
						current.scaledBackgrounds[index] = background.Layout(0,15).Draw<Texture2D>("Background").As<Texture2D>();
					}
					EditorGUI.indentLevel -= 1;
				}
				#endif
				EditorGUI.indentLevel -= 1;
			}
		}
	}
}