using UnityEngine;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using MenuFunction = UnityEditor.GenericMenu.MenuFunction;
namespace Zios{
    public class StaticInspector : EditorWindow{
	    private string currentAssembly;
	    private string currentNamespace;
	    private string currentClass;
	    private int selectedAssembly;
	    private int selectedNamespace;
	    private int selectedClass;
	    private object activeClass;
	    private bool setup;
	    private float nextRepaint;
	    private List<Assembly> assemblies = new List<Assembly>();
	    private int setupIndex;
	    private Dictionary<string,Accessor> variables = new Dictionary<string,Accessor>();
	    private SortedDictionary<string,SortedDictionary<string,List<string>>> classNames = new SortedDictionary<string,SortedDictionary<string,List<string>>>();
	    private SortedDictionary<string,SortedDictionary<string,List<Type>>> classes = new SortedDictionary<string,SortedDictionary<string,List<Type>>>();
        [MenuItem ("Zios/Window/Static Inspector")]
	    static void Init(){
		    StaticInspector window = (StaticInspector)EditorWindow.GetWindow(typeof(StaticInspector));
		    window.position = new Rect(100,150,200,200);
		    window.Start();
        }
	    public void OnDestroy(){
		    Utility.RemoveEditorUpdate(this.Setup);
	    }
        public void OnGUI(){
		    Utility.AddEditorUpdate(this.Setup,true);
		    if(this.assemblies.Count < 1){this.setup = false;}
		    if(this.setup){
			    this.DrawContext();
			    this.DrawSelectors();
			    this.DrawInspector();
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
				    this.currentAssembly = EditorPrefs.GetString("StaticInspector-Assembly");
				    this.currentNamespace = EditorPrefs.GetString("StaticInspector-Namespace");
				    this.currentClass = EditorPrefs.GetString("StaticInspector-Class");
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
			    Utility.SetDirty(this);
			    this.Repaint();
		    }
	    }
	    public void Reset(){
		    this.setup = false;
		    this.assemblies.Clear();
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
		    this.title = "Static";
		    float fieldSize = this.position.width/3.0f - 6;
		    Rect assemblyArea = new Rect(5,5,fieldSize,15);
		    Rect namespaceArea = new Rect(5+(fieldSize)+2,5,fieldSize,15);
		    Rect classArea = new Rect(5+(fieldSize*2)+4,5,fieldSize,15);
		    GUI.changed = false;
		    //=================
		    // Assembly
		    //=================
		    string[] assemblyNames = this.classNames.Keys.ToArray();
		    int assemblyIndex = assemblyNames.IndexOf(this.currentAssembly);
		    if(assemblyIndex == -1){assemblyIndex = 0;}
		    this.selectedAssembly = assemblyNames.Draw(assemblyArea,assemblyIndex);
		    if(GUI.changed){this.ResetIndexes(2);}
		    this.currentAssembly = assemblyNames[this.selectedAssembly];
		    //=================
		    // Namespace
		    //=================
		    string[] namespaces = this.classNames[this.currentAssembly].Keys.ToArray();
		    int namespaceIndex = namespaces.IndexOf(this.currentNamespace);
		    if(namespaceIndex == -1){namespaceIndex = 0;}
		    this.selectedNamespace = namespaces.Draw(namespaceArea,namespaceIndex);
		    if(GUI.changed){this.ResetIndexes(1);}
		    this.currentNamespace = namespaces[this.selectedNamespace];
		    //=================
		    // Class
		    //=================
		    string[] classes = this.classNames[this.currentAssembly][this.currentNamespace].ToArray();
		    int classIndex = classes.IndexOf(this.currentClass);
		    if(classIndex == -1){classIndex = 0;}
		    this.selectedClass = classes.Draw(classArea,classIndex);
		    this.currentClass = classes[this.selectedClass];
		    this.activeClass = this.classes[this.currentAssembly][this.currentNamespace][this.selectedClass];
		    if(GUI.changed){
			    this.variables.Clear();
			    EditorPrefs.SetString("StaticInspector-Assembly",this.currentAssembly);
			    EditorPrefs.SetString("StaticInspector-Namespace",this.currentNamespace);
			    EditorPrefs.SetString("StaticInspector-Class",this.currentClass);
		    }
	    }
	    public void DrawInspector(){
		    if(this.activeClass != null && this.variables.Count < 1){
			    List<string> names = this.activeClass.ListVariables(null,ObjectExtension.staticFlags);
			    foreach(string name in names){
				    try{
					    var accessor = new Accessor(this.activeClass,name);
					    this.variables[name] = accessor;
				    }
				    catch{}
			    }
		    }
		    if(this.variables.Count > 0){
			    Rect labelArea = new Rect(13,25,this.position.width*0.415f,15);
			    Rect valueArea = new Rect(-13+this.position.width*0.415f,25,this.position.width*0.585f,15);
			    foreach(var current in this.variables.Copy()){
				    string name = current.Key;
				    object value = current.Value.Get();
				    if(!(value is string || value is bool || value is float || value is int)){
					    continue;
				    }
				    GUIContent label = new GUIContent(name.ToTitle());
				    GUI.changed = false;
				    label.DrawLabel(labelArea);
				    if(value is string){
					    string newValue = ((string)value).Draw(valueArea);
					    if(GUI.changed){current.Value.Set(newValue);}
				    }
				    if(value is bool){
					    bool newValue = ((bool)value).Draw(valueArea);
					    if(GUI.changed){current.Value.Set(newValue);}
				    }
				    if(value is float){
					    float newValue =((float)value).Draw(valueArea);
					    if(GUI.changed){current.Value.Set(newValue);}
				    }
				    if(value is int){
					    int newValue = ((int)value).DrawInt(valueArea);
					    if(GUI.changed){current.Value.Set(newValue);}
				    }
				    labelArea = labelArea.AddY(18);
				    valueArea = valueArea.AddY(18);	
			    }
		    }
	    }
	    public void ResetIndexes(int priority){
		    if(priority > 2){this.selectedAssembly = 0;}
		    if(priority > 1){this.selectedNamespace = 0;}
		    if(priority > 0){this.selectedClass = 0;}
	    }
    }
}