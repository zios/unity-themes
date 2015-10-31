using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityObject = UnityEngine.Object;
using MenuFunction = UnityEditor.GenericMenu.MenuFunction;
namespace Zios.UI{
    public class StaticInspector : EditorWindow{
		public static string currentAssembly;
		public static string currentNamespace;
		public static string currentClass;
		private Vector2 scrollPosition;
		private Rect viewArea;
	    private int selectedAssembly;
	    private int selectedNamespace;
	    private int selectedClass;
	    private object activeClass;
	    private bool setup;
	    private float nextRepaint;
		private Rect labelArea;
		private Rect valueArea;
	    private List<Assembly> assemblies = new List<Assembly>();
	    private int setupIndex;
		private Dictionary<int,bool> foldoutState = new Dictionary<int,bool>();
	    private Dictionary<string,Accessor> variables = new Dictionary<string,Accessor>();
	    private SortedDictionary<string,SortedDictionary<string,List<string>>> classNames = new SortedDictionary<string,SortedDictionary<string,List<string>>>();
	    private SortedDictionary<string,SortedDictionary<string,List<Type>>> classes = new SortedDictionary<string,SortedDictionary<string,List<Type>>>();
        [MenuItem ("Zios/Window/Static Inspector")]
	    static void Init(){
		    StaticInspector window = (StaticInspector)EditorWindow.GetWindow(typeof(StaticInspector));
		    window.position = new Rect(100,150,200,200);
		    window.Start();
        }
        public void OnGUI(){
			Events.Add("On Editor Update",this.Setup);
		    if(this.assemblies.Count < 1){this.setup = false;}
		    if(this.setup){
				this.DrawContext();
			    this.DrawSelectors();
			    this.DrawInspector();
		    }
			if(Application.isPlaying){
				this.Setup();
				this.Repaint();
			}
	    }
	    public void Start(){}
	    public void Setup(){
		    if(!this.setup){
			    if(this.assemblies.Count < 1){
				    this.ResetIndexes(3);
				    this.classNames.Clear();
				    this.classes.Clear();
				    this.setupIndex = 0;
				    this.assemblies = AppDomain.CurrentDomain.GetAssemblies().ToList();
			    }
			    if(this.setupIndex > this.assemblies.Count-1){
				    this.setup = true;
				    foreach(var assemblyItem in this.classNames){
					    foreach(var namespaceItem in assemblyItem.Value){
						    this.classes[assemblyItem.Key][namespaceItem.Key] = this.classes[assemblyItem.Key][namespaceItem.Key].OrderBy(x=>x.Name).ToList();
						    this.classNames[assemblyItem.Key][namespaceItem.Key].Sort();
					    }
				    }
				    EditorUtility.ClearProgressBar();
				    this.Repaint();
				    return;
			    }
			    Assembly assembly = this.assemblies[this.setupIndex];
			    string assemblyName = assembly.GetName().Name.TrySplit(".",0);
			    Type[] allTypes = assembly.GetTypes();
			    foreach(Type type in allTypes){
				    if(type == null || type.Name.Contains("_AnonStorey")){continue;}
				    string space = type.Namespace.IsEmpty() ? "[None]" : type.Namespace;
				    this.classes.AddNew(assemblyName).AddNew(space).Add(type);
				    this.classNames.AddNew(assemblyName).AddNew(space).Add(type.Name);
			    }
			    float percent = (float)this.setupIndex/(float)this.assemblies.Count;
			    EditorUtility.DisplayProgressBar("Loading","Sorting Data",percent);
			    this.setupIndex += 1;
		    }
		    if(Time.realtimeSinceStartup > this.nextRepaint){
			    this.nextRepaint = Time.realtimeSinceStartup + 0.25f;
			    this.Repaint();
		    }
	    }
	    public void Reset(){
		    this.setup = false;
		    this.assemblies.Clear();
			this.Setup();
	    }
	    public void DrawContext(){
		    if(this.position.SetX(0).SetY(0).Clicked(1)){
			    GenericMenu menu = new GenericMenu();
			    MenuFunction refresh = ()=>{this.Reset();};
			    menu.AddItem(new GUIContent("Refresh"),false,refresh);
			    menu.ShowAsContext();
		    }
	    }
	    public void DrawSelectors(){
            this.titleContent = new GUIContent("Static");
		    float fieldSize = this.position.width/3.0f - 6;
		    var assemblyArea = new Rect(5,5,fieldSize,15);
		    var namespaceArea = new Rect(5+(fieldSize)+2,5,fieldSize,15);
		    var classArea = new Rect(5+(fieldSize*2)+4,5,fieldSize,15);
		    GUI.changed = false;
		    //=================
		    // Assembly
		    //=================
		    string[] assemblyNames = this.classNames.Keys.ToArray();
		    int assemblyIndex = assemblyNames.IndexOf(StaticInspector.currentAssembly);
		    if(assemblyIndex == -1){assemblyIndex = 0;}
		    this.selectedAssembly = assemblyNames.Draw(assemblyArea,assemblyIndex);
		    if(GUI.changed){this.ResetIndexes(2);}
		    StaticInspector.currentAssembly = assemblyNames[this.selectedAssembly];
		    //=================
		    // Namespace
		    //=================
		    string[] namespaces = this.classNames[StaticInspector.currentAssembly].Keys.ToArray();
		    int namespaceIndex = namespaces.IndexOf(StaticInspector.currentNamespace);
		    if(namespaceIndex == -1){namespaceIndex = 0;}
		    this.selectedNamespace = namespaces.Draw(namespaceArea,namespaceIndex);
		    if(GUI.changed){this.ResetIndexes(1);}
		    StaticInspector.currentNamespace = namespaces[this.selectedNamespace];
		    //=================
		    // Class
		    //=================
		    string[] classes = this.classNames[StaticInspector.currentAssembly][StaticInspector.currentNamespace].ToArray();
		    int classIndex = classes.IndexOf(StaticInspector.currentClass);
		    if(classIndex == -1){classIndex = 0;}
		    this.selectedClass = classes.Draw(classArea,classIndex);
		    StaticInspector.currentClass = classes[this.selectedClass];
		    this.activeClass = this.classes[StaticInspector.currentAssembly][StaticInspector.currentNamespace][this.selectedClass];
		    if(GUI.changed){
			    this.variables.Clear();
		    }
	    }
	    public void DrawInspector(){
			this.scrollPosition = GUI.BeginScrollView(new Rect(0,25,Screen.width,Screen.height-45),this.scrollPosition,this.viewArea);
		    if(this.activeClass != null && this.variables.Count < 1){
			    List<string> names = this.activeClass.ListVariables(null,null,ObjectExtension.staticFlags);
			    foreach(string name in names){
				    try{
					    var accessor = new Accessor(this.activeClass,name);
					    this.variables[name] = accessor;
				    }
				    catch{}
			    }
		    }
		    if(this.variables.Count > 0){
			    this.labelArea = new Rect(13,10,this.position.width*0.415f,15);
			    this.valueArea = new Rect(-13+this.position.width*0.415f,10,this.position.width*0.585f,15);
			    foreach(var current in this.variables.Copy()){
					string name = current.Key;
					object value = current.Value.Get();
					var accessor = this.variables.ContainsKey(name) ? this.variables[name] : null;
					this.DrawValue(name,value,accessor);
			    }
		    }
			this.viewArea = this.viewArea.SetHeight(this.valueArea.y+22);
			GUI.EndScrollView();
	    }
		public void UpdateValue(Accessor accessor,object value){
			if(accessor != null && GUI.changed){accessor.Set(value);}
		}
		public void DrawValue(string labelText,object value,Accessor accessor=null,int depth=0){
			if(labelText.Contains("$cache")){return;}
			labelText = labelText.ToTitle();
			if(value is UnityObject){labelText = labelText + " (" + value.GetType().Name + ")";}
			bool labelDrawn = false;
			GUIContent label = new GUIContent(labelText);
			GUI.changed = false;
			bool common = (value is string || value is bool || value is float || value is int || value is UnityObject || value is Enum);
			if(common){
				label.DrawLabel(this.labelArea);
				labelDrawn = true;
			}
			int hash = 0;
			if(!value.IsNull()){
				hash = value.GetHashCode();
				this.foldoutState.AddNew(hash);
			}
			if(value is GameObject){
				GameObject newValue = value.As<GameObject>().Draw<GameObject>(this.valueArea);
				this.UpdateValue(accessor,newValue);
			}
			else if(value is Component){
				Component newValue = value.As<Component>().Draw<Component>(this.valueArea);
				this.UpdateValue(accessor,newValue);
			}
			else if(value.IsNull()){return;}
			else if(value is Enum){
				string name = accessor != null ? accessor.name : "";
				object scope = accessor != null ? accessor.scope : null;
				if(accessor != null && scope.HasAttribute(name,typeof(EnumMaskAttribute))){
					Enum newValue = ((Enum)value).DrawMask(this.valueArea);
					this.UpdateValue(accessor,newValue);
				}
				else{
					Enum newValue = ((Enum)value).Draw(this.valueArea);
					this.UpdateValue(accessor,newValue);
				}
			}
			else if(value is string){
				string newValue = ((string)value).Draw(this.valueArea);
				this.UpdateValue(accessor,newValue);
			}
			else if(value is bool){
				bool newValue = ((bool)value).Draw(this.valueArea);
				this.UpdateValue(accessor,newValue);
			}
			else if(value is float){
				float newValue =((float)value).Draw(this.valueArea);
				this.UpdateValue(accessor,newValue);
			}
			else if(value is int){
				int newValue = ((int)value).DrawInt(this.valueArea);
				this.UpdateValue(accessor,newValue);
			}
			else if(value is IList && depth < 9){
				IList items = (IList)value;
				/*if(items.Count == 1){
					this.DrawValue(label.text,items[0],null,depth+1);
					return;
				}*/
				label.text = label.text + " (" + items.Count + ")";
				this.foldoutState[hash] = EditorGUI.Foldout(this.labelArea,this.foldoutState[hash],label);
				this.labelArea = this.labelArea.AddY(18);
				this.valueArea = this.valueArea.AddY(18);
				if(this.foldoutState[hash]){
					if(items.Count < 1){return;}
					this.labelArea = this.labelArea.AddX(10);
					int index = 0;
					foreach(object item in items){
						this.DrawValue("Item " + index,item,null,depth+1);
						++index;
					}
					this.labelArea = this.labelArea.AddX(-10);
				}
				return;
			}
			else if(value is IDictionary && depth < 9){
				IDictionary items = (IDictionary)value;
				/*if(items.Count <= 1){
					foreach(DictionaryEntry item in items){
						this.DrawValue(label.text,item.Value,null,depth+1);
					}
					return;
				}*/
				label.text = label.text + " (" + items.Count + ")";
				this.foldoutState[hash] = EditorGUI.Foldout(this.labelArea,this.foldoutState[hash],label);
				this.labelArea = this.labelArea.AddY(18);
				this.valueArea = this.valueArea.AddY(18);
				if(this.foldoutState[hash]){
					this.labelArea = this.labelArea.AddX(10);
					int index = 0;
					foreach(DictionaryEntry item in items){
						if(item.Key is string){
							this.DrawValue((string)item.Key,item.Value,null,depth+1);
							--depth;
							continue;
						}
						int itemHash = item.GetHashCode();
						this.foldoutState.AddNew(itemHash);
						this.foldoutState[itemHash] = EditorGUI.Foldout(this.labelArea,this.foldoutState[itemHash],"Item " + index);
						this.labelArea = this.labelArea.AddY(18);
						this.valueArea = this.valueArea.AddY(18);
						if(this.foldoutState[itemHash]){
							this.labelArea = this.labelArea.AddX(10);
							this.DrawValue("Key",item.Key,null,depth+1);
							this.DrawValue("Value",item.Value,null,depth+1);
							this.labelArea = this.labelArea.AddX(-10);
						}
						++index;
					}
					this.labelArea = this.labelArea.AddX(-10);
				}
				return;
			}
			else if(value.GetType().IsSerializable && depth < 9){
				this.foldoutState[hash] = EditorGUI.Foldout(this.labelArea,this.foldoutState[hash],label);
				this.labelArea = this.labelArea.AddY(18);
				this.valueArea = this.valueArea.AddY(18);
				if(this.foldoutState[hash]){
					List<string> fieldNames = value.ListVariables(null,null,ObjectExtension.publicFlags);
					if(fieldNames.Count < 1){return;}
					this.labelArea = this.labelArea.AddX(10);
					foreach(string fieldName in fieldNames){
						try{
							object fieldValue = value.GetVariable(fieldName);
							this.DrawValue(fieldName,fieldValue,null,depth+1);
						}
						catch{}
					}
					this.labelArea = this.labelArea.AddX(-10);
				}
				return;
			}
			else if(!labelDrawn){return;}
			if(GUI.changed && this.activeClass.HasMethod("StaticValidate")){
				this.activeClass.CallMethod("StaticValidate");
			}
			this.labelArea = this.labelArea.AddY(18);
			this.valueArea = this.valueArea.AddY(18);
		}
	    public void ResetIndexes(int priority){
		    if(priority > 2){this.selectedAssembly = 0;}
		    if(priority > 1){this.selectedNamespace = 0;}
		    if(priority > 0){this.selectedClass = 0;}
	    }
    }
}