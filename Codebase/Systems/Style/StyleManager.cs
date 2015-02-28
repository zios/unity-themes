using UnityEngine;
using System;
using System.Collections.Generic;
namespace Zios{
    [AddComponentMenu("Zios/Singleton/Style")]
    [ExecuteInEditMode]
    public class StyleManager : MonoBehaviour{
	    public Style[] styles = new Style[0];
	    public Dictionary<string,GUIStyle> instances = new Dictionary<string,GUIStyle>();
	    public GUIStyle Get(string name){return this.instances.ContainsKey(name) ? this.instances[name] : null;}
	    public void Awake(){
		    DontDestroyOnLoad(this.gameObject);
	    }
	    public void Reset(){
		    this.instances.Clear();
	    }
	    public void Update(){
		    /*if(Global.Styles == null || this.instances.Count < 1){
			    this.Start();
		    }*/
	    }
	    public void Start(){
		    foreach(Style item in this.styles){
			    this.instances[item.name] = item.style;
		    }
	    }
    }
    [Serializable]
    public class Style{
	    public string name;
	    public GUIStyle style;
    }
}