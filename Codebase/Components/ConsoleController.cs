using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using Console = Zios.Console;
using Cvar = Zios.Cvar;
using Callback = Zios.ConsoleCallback;
[AddComponentMenu("Zios/Component/General/Console Controller")]
[ExecuteInEditMode]
public class ConsoleController : MonoBehaviour{
	public Dictionary<string,string> shortcuts = new Dictionary<string,string>();
	public Dictionary<string,Callback> keywords = new Dictionary<string,Callback>();
	public Dictionary<string,Cvar> cvars = new Dictionary<string,Cvar>();
	public void OnAwake(){
		if(shortcuts == null){
			this.shortcuts = new Dictionary<string,string>();
		}
		if(keywords == null){
			keywords = new Dictionary<string,Callback>();
		}
		if(cvars == null){
			cvars = new Dictionary<string,Cvar>();
		}
	}
	public void OnEnable(){
		if(Application.isPlaying){
			Debug.Log("Creating Console Commands for " + this.gameObject.name);
			foreach(string cvarKey in this.cvars.Keys){
				if(Console.cvars.ContainsKey(cvarKey)){
					Debug.LogWarning("Cvar " + cvarKey + " already exists. Won`t be added again.");
				}
				else{
					Cvar cvar = this.cvars[cvarKey];
					object scope = cvar.scope;
					if(scope is string){
						Debug.Log("Skipping Cvar " + cvarKey);
					}
					else if(cvar.method == null || (cvar.method.simple == null && cvar.method.basic == null && cvar.method.full == null)){
						Console.AddCvar(cvarKey,scope,cvar.name,cvar.fullName,cvar.help);
						Debug.Log("Added Cvar " + cvarKey);
					}
					else{
						if(cvar.method.simple != null){
							Console.AddCvarMethod(cvarKey,scope,cvar.name,cvar.fullName,cvar.help,cvar.method.simple);
						}
						else if(cvar.method.basic != null){
							Console.AddCvarMethod(cvarKey,scope,cvar.name,cvar.fullName,cvar.help,cvar.method.basic);
						}
						else{
							Console.AddCvarMethod(cvarKey,scope,cvar.name,cvar.fullName,cvar.help,cvar.method.full);
						}
						Debug.Log("Added Cvar Method" + cvarKey);
					}

				}
			}
			foreach(string shortcutKey in this.shortcuts.Keys){
				if(Console.shortcuts.ContainsKey(shortcutKey)){
					Debug.LogWarning("Shortcut " + shortcutKey + " already exists. Won`t be added again.");
				}
				else{
					string shortcutReplacement = this.shortcuts[shortcutKey];
					Console.AddShortcut(shortcutKey,shortcutReplacement);
					Debug.Log("Added Shortcut " + shortcutKey);
				}
			}
			foreach(string keywordKey in this.keywords.Keys){
				if(Console.keywords.ContainsKey(keywordKey)){
					Debug.LogWarning("Keyword " + keywordKey + " already exists. Won`t be added again.");
				}
				else{
					Callback callback = this.keywords[keywordKey];
					if(callback.simple != null){
						Console.AddKeyword(keywordKey,callback.simple,callback.minimumParameters,callback.help);
					}
					else if(callback.basic != null){
						Console.AddKeyword(keywordKey,callback.basic,callback.minimumParameters,callback.help);
					}
					else{
						Console.AddKeyword(keywordKey,callback.full,callback.minimumParameters,callback.help);
					}
					Debug.Log("Added Keyword" + keywordKey);
				}
			}
		}
	}
	public void populateTest(){
		if(this.shortcuts.Count == 0){
			this.shortcuts.Add("test","TestTheComponent");
			Cvar cvar = new Cvar();
			cvar.scope = typeof(Application);
			cvar.name = "targetFrameRate";
			cvar.help = "Any help to test only";
			cvar.fullName = "Maximum FPS";
			cvar.method = new Callback();
			this.cvars.Add("maxfps2",cvar);
			Callback keyword = new Callback();
			keyword.full = Console.HandleCvar;
			keywords.Add("maxfps2",keyword);
		}
	}
}

