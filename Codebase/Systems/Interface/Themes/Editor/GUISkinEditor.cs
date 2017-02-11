using UnityEditor;
using UnityEngine;
using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Zios.Interface;
using UnityEvent = UnityEngine.Event;
namespace Zios.Editors{
	[CustomEditor(typeof(GUISkin))]
	public partial class GUISkinEditor : Editor{
		public static GUIStyle focus;
		public static string action;
		public bool advanced;
		public int viewMode;
		public int inputMode;
		public string hash;
		public string[] inputTerms = new string[0];
		public GUIStyle[] searchResults = new GUIStyle[0];
		public GUISkin[] fragments = new GUISkin[0];
		public GUISkin skin;
		//=================================
		// Main
		//=================================
		public override void OnInspectorGUI(){
			if(UnityEvent.current.type.MatchesAny("mouseMove")){return;}
			//this.optimize = Utility.GetPref<bool>("GUISkinEditor-Optimize",false);
			this.hash = this.hash ?? Utility.GetInspector(this).GetInstanceID().ToString();
			this.skin = this.skin ?? this.target.As<GUISkin>();
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
			this.fragments = FileManager.GetAssets<GUISkin>(this.skin.name+"#*.guiSkin",false);
			this.ProcessMenu();
			this.DrawSearch();
			this.DrawSplit();
			if(this.inputMode == 0 && this.inputTerms[0] == "Search"){
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
			this.viewMode = Utility.GetPref<int>("GUISkin-Mode-"+this.hash,0);
			if(this.viewMode == 1){term = "Replica";}
			if(this.viewMode == 2){term = "Compact";}
			if(term.ToLabel().DrawButton(GUI.skin.button.FixedHeight(30))){
				this.viewMode = (this.viewMode + 1) % 3;
				Utility.SetPref<int>("GUISkin-Mode-"+this.hash,this.viewMode);
				EditorUI.foldoutChanged = true;
			}
		}
		public bool DrawInputMode(string fallback="Search",string splitBy=" ",string endSymbol="+"){
			var search = Utility.GetPref("GUISkin-"+fallback+"-"+this.hash,fallback).Split(splitBy);
			var inputChanged = false;
			if(!this.inputTerms.SequenceEqual(search)){
				this.inputTerms = search;
				inputChanged = true;
			}
			EditorGUILayout.BeginHorizontal();
			var color = EditorStyles.textField.normal.textColor.SetAlpha(0.5f);
			var baseStyle = EditorStyles.textField.Alignment("MiddleCenter").FixedHeight(30);
			var symbolStyle = baseStyle.FixedWidth(24).FontSize(16).TextColor(color);
			var inputStyle = baseStyle.FontStyle("BoldAndItalic");
			if("∗".ToLabel().DrawButton(symbolStyle.ContentOffset(1,0).Overflow(0,3,0,0),false)){
				var menu = new EditorMenu();
				Action clear = ()=>this.inputTerms.Clear();
				menu.Add("Search",this.inputMode==0,(()=>{this.inputMode=0;})+clear);
				menu.Add("Split",this.inputMode==1,(()=>{this.inputMode=1;})+clear);
				menu.Draw();
			}
			var filter = this.inputTerms.Join(splitBy).Replace("~"," ").Draw(null,inputStyle);
			inputChanged = inputChanged || EditorUI.lastChanged;
			EditorUI.lastChanged = false;
			endSymbol.ToLabel().DrawButton(symbolStyle.ContentOffset(-2,0).Overflow(3,0,0,0),false);
			EditorGUILayout.EndHorizontal();
			bool open = false;
			var output = new StringBuilder();
			foreach(var symbol in filter){
				if(symbol=='['){open=true;}
				if(symbol==']'){open=false;}
				output.Append(symbol==' ' && open ? '~' : symbol);
			}
			this.inputTerms = output.ToString().Split(splitBy);
			if(this.inputTerms.Join(splitBy).IsEmpty()){
				this.inputTerms = new string[1]{fallback};
				EditorUI.foldoutChanged = true;
			}
			Utility.SetPref<string>("GUISkin-"+fallback+"-"+this.hash,this.inputTerms.Join(splitBy));
			return inputChanged;
		}
		public void DrawSearch(){
			if(this.inputMode != 0){return;}
			var inputChanged = this.DrawInputMode();
			this.advanced = EditorUI.lastChanged ? !this.advanced : this.advanced;
			if(this.advanced){
				"Advanced form searching goes here.".DrawHelp();
				GUILayout.Space(10);
			}
			if(this.inputTerms[0] != "Search"){
				if(inputChanged){this.searchResults = this.PerformSearch();}
				if(this.searchResults.Length < 1){
					EditorGUI.indentLevel += 1;
					"No results found.".ToLabel().DrawLabel(EditorStyles.label.Alignment("MiddleCenter"),false);
					EditorGUI.indentLevel -= 1;
				}
				this.DrawStyles(this.hash,this.searchResults);
			}
		}
		public void DrawSplit(){
			if(this.inputMode != 1){return;}
			this.DrawInputMode("Split | By | Terms"," | ","•");
			if(EditorUI.lastChanged){
				var original = this.inputTerms.Copy();
				GUIStyle[] styles = null;
				foreach(var term in original){
					this.inputTerms = term.AsArray();
					styles = this.PerformSearch(styles,false,false);
					Debug.Log(this.inputTerms[0] + " -- " + this.searchResults.Length);
				}
			}
		}
		public void DrawStyles(string key,GUIStyle[] styles,bool editable=true){
			var compact = this.viewMode == 2;
			EditorGUI.indentLevel += 1;
			for(int index=0;index<styles.Length;++index){
				var style = styles[index];
				style = styles[index] = style ?? EditorStyles.label.Copy();
				if(this.CheckBounds(style)){
					style.Draw(key,compact);
					if(this.rebuild && Utility.IsRepainting()){
						this.allRect.Add(GUILayoutUtility.GetLastRect());
					}
					this.DrawMenu(style,editable);
				}
				else if(this.drawn){break;}
				this.count += 1;
			}
			EditorGUI.indentLevel -= 1;
		}
		public void DrawStandard(){
			EditorGUI.indentLevel += 1;
			if("Standard".ToLabel().DrawFoldout("GUISkin-Standard-"+this.hash)){
				this.DrawStyles(this.hash,this.skin.GetStyles(true,false),false);
			}
			EditorGUI.indentLevel -= 1;
		}
		public void DrawCustom(){
			EditorGUI.indentLevel += 1;
			if("Custom".ToLabel().DrawFoldout("GUISkin-Custom-"+this.hash)){
				EditorGUI.indentLevel += 1;
				var size = this.skin.customStyles.Length.DrawIntDelayed("Size");
				EditorGUI.indentLevel -= 1;
				if(this.skin.customStyles.Length != size){
					this.skin.customStyles = this.skin.customStyles.Resize(size);
					EditorUI.foldoutChanged = true;
				}
				this.DrawStyles(this.hash,this.skin.customStyles);
			}
			EditorGUI.indentLevel -= 1;
		}
		public void DrawFragmented(){
			if(this.fragments.Length > 0){
				GUILayout.Space(10);
				if("Fragments".ToLabel().DrawHeader("GUISkin-Fragments-"+this.hash,GUI.skin.GetStyle("LargeButton").FixedHeight(30))){
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
			if(styles.IsNull()){
				styles = this.skin.GetStyles(includeStandard,true);
				if(includeFragments){
					var fragments = this.fragments.SelectMany(x=>x.customStyles).ToArray();
					styles = styles.Concat(fragments);
				}
			}
			EditorUI.foldoutChanged = true;
			return styles.Where(x=>this.ContainsAttributes(x) && x.name.ContainsAll(this.FilterSearch())).ToArray();
		}
		public bool ContainsAttributes(GUIStyle style){
			foreach(var term in this.inputTerms){
				if(!term.ContainsAll("[","]","=")){continue;}
				var data = term.Remove("[","]").Replace("~"," ").Split("=").Trim(" ");
				var name = data[0];
				var value = data[1];
				if(value.IsEmpty()){continue;}
				if(name.ContainsAny("textColor","background")){
					var found = false;
					foreach(var state in style.GetStates()){
						var current = state.GetVariable(name);
						var type = state.GetVariableType(name);
						if(current.IsNull()){continue;}
						if(type.Is<Color>()){
							bool hexMatch = current.As<Color>().ToHex().StartsWith(value,true);
							bool decimalMatch = current.As<Color>().ToDecimal().StartsWith(value,true);
							if(hexMatch || decimalMatch){found = true;}
						}
						if(type.Is<Texture>() && current.As<Texture>().name.Contains(value,true)){found=true;}
					}
					if(found){continue;}
					return false;
				}
				if(style.HasVariable(name)){
					var current = style.GetVariable(name);
					var type = style.GetVariableType(name);
					if(current.IsNull()){return false;}
					if(type.Is<RectOffset>()){
						if(!current.As<RectOffset>().Serialize().Contains(value)){
							return false;
						}
					}
					else if(type.Is<Vector2>()){
						if(!current.As<Vector2>().ToString().Remove(".0",",").Contains(value)){
							return false;
						}
					}
					else if(type.Is<bool>()){
						if(!current.As<bool>().ToString().Contains(value)){
							return false;
						}
					}
					else if(type.IsAny<float,int>()){
						if(current.ToString()!=value){
							return false;
						}
					}
					else if(!current.ToString().Contains(value,true)){
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
			if(GUI.changed){
				Utility.SetAssetDirty(this.target.As<GUISkin>());
				foreach(var fragment in fragments){
					Utility.SetAssetDirty(fragment);
				}
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
				EditorGUI.indentLevel += 1;
				EditorGUIUtility.labelWidth = compact ? 130 : 0;
				current.name = current.name.Draw("Name".ToLabel());
				Utility.SetPref<bool>(key+"."+current.name,true);
				if(compact){
					EditorGUILayout.BeginHorizontal();
					GUILayout.Space(130);
					if(headers){
						"Text".ToLabel().DrawLabel(EditorStyles.boldLabel.FixedWidth(120),false);
						"Background".ToLabel().DrawLabel(EditorStyles.boldLabel,false);
					}
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
					EditorGUIUtility.labelWidth = compact ? 130 : 0;
					current.imagePosition = current.imagePosition.Draw("Image Position").As<ImagePosition>();
					current.contentOffset = current.contentOffset.DrawVector2("Content Offset");
					current.fixedWidth = current.fixedWidth.Draw("Fixed Width");
					current.fixedHeight = current.fixedHeight.Draw("Fixed Height");
					current.stretchWidth = current.stretchWidth.Draw("Stretch Width");
					current.stretchHeight = current.stretchHeight.Draw("Stretch Height");
					EditorGUIUtility.labelWidth = 0;
				}
				EditorGUI.indentLevel -= 1;
			}
			EditorGUILayout.EndVertical();
		}
		public static void Draw(this RectOffset current,string name="RectOffset",bool compact=false,string key=null){
			if(compact){
				EditorGUILayout.BeginHorizontal();
				EditorGUIUtility.labelWidth = 130;
				EditorGUIUtility.fieldWidth = 26;
				var style = EditorStyles.numberField.FixedWidth(26);
				current.left = current.left.DrawInt(name);
				current.right = current.right.DrawInt(null,style,false);
				current.top = current.top.DrawInt(null,style,false);
				current.bottom = current.bottom.DrawInt(null,style,false);
				EditorGUIUtility.labelWidth = 0;
				EditorGUIUtility.fieldWidth = 0;
				GUILayout.FlexibleSpace();
				EditorGUILayout.EndHorizontal();
				return;
			}
			if(name.ToTitleCase().ToLabel().DrawFoldout(key)){
				EditorGUI.indentLevel += 1;
				current.left = current.left.DrawInt("Left");
				current.right = current.right.DrawInt("Right");
				current.top = current.top.DrawInt("Top");
				current.bottom = current.bottom.DrawInt("Bottom");
				EditorGUI.indentLevel -= 1;
			}
		}
		public static void DrawTextSettings(this GUIStyle current,bool compact=false){
			if(compact){
				EditorGUILayout.BeginHorizontal();
				EditorGUIUtility.labelWidth = 130;
				EditorGUIUtility.fieldWidth = 30;
				EditorUI.autoLayout = false;
				current.fontSize = current.fontSize.DrawInt("Font",EditorStyles.textField.FixedWidth(40).FixedHeight(EditorStyles.popup.fixedHeight));
				EditorGUIUtility.labelWidth = 0.00000000001f;
				EditorGUIUtility.fieldWidth = 75;
				GUILayout.Space(4);
				var original = new GUIStyle(EditorStyles.popup);
				EditorStyles.popup.Use(EditorStyles.popup.Margin(0));
				current.fontStyle = current.fontStyle.Draw(null,EditorStyles.popup.Margin(0,0,2,0).FixedWidth(80),false).As<FontStyle>();
				EditorGUIUtility.fieldWidth = 0;
				current.font = current.font.Draw<Font>(null,false,false);
				EditorStyles.popup.Use(original);
				EditorGUILayout.EndHorizontal();
				EditorUI.autoLayout = true;
				EditorGUIUtility.labelWidth = 130;
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
			EditorGUIUtility.labelWidth = 0;
		}
		public static void Draw(this GUIStyleState current,string name="GUIStyleState",bool compact=false,string key=null){
			EditorUI.height = 15;
			if(compact){
				EditorGUILayout.BeginHorizontal();
				EditorGUIUtility.labelWidth = 130;
				EditorUI.width = 200;
				current.textColor = current.textColor.Draw(name.ToTitleCase());
				EditorUI.width = 0;
				EditorGUIUtility.labelWidth = 0.00001f;
				current.background = current.background.Draw<Texture2D>(null,true,false).As<Texture2D>();
				EditorGUIUtility.labelWidth = 0;
				EditorGUILayout.EndHorizontal();
				EditorUI.height = 0;
				return;
			}
			if(name.ToTitleCase().ToLabel().DrawFoldout(key)){
				EditorGUI.indentLevel += 1;
				current.background = current.background.Draw<Texture2D>("Background").As<Texture2D>();
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
						current.scaledBackgrounds[index] = background.Draw<Texture2D>("Background").As<Texture2D>();
					}
					EditorGUI.indentLevel -= 1;
				}
				#endif
				EditorGUI.indentLevel -= 1;
			}
			EditorUI.height = 0;
		}
	}
}