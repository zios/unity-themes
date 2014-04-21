using UnityEngine;
using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using Console = Zios.Console;
using ConsoleMethod = Zios.ConsoleMethod;
using ConsoleMethodFull = Zios.ConsoleMethodFull;
[AddComponentMenu("Zios/Component/General/Console Controller")]
[ExecuteInEditMode]
public class ConsoleController : MonoBehaviour{
	[Serializable]
	public class ConsoleData{
		public string key;
		public string replace;
		public string scopeName;
		public object scope;
		public bool validName;
		public string name;
		public string fullName;
		public string help;
		public string methodName;
		public Method simple;
		public ConsoleMethod basic;
		public ConsoleMethodFull full;
		public int minimumParameters = -1;
		public void ValidateScope(ConsoleController controller){
			Type staticType = this.LoadType(this.scopeName);
			Component component = controller.GetComponent(this.scopeName);
			if(staticType != null){
				this.scope = staticType;
			}
			else if(component != null){
				this.scope = component;
			}
			else{
				this.scope = null;
			}
		}
		public void ValidateAttribute(){
			this.validName = false;
			if(this.name != null && this.name.Trim() != ""){
				if(this.scope is Type){
					this.validName = this.scope.HasAttribute(this.name.Trim(),(Type)this.scope);
				}
				else{
					this.validName = this.scope.HasAttribute(this.name.Trim());
				}
			}
		}
		public void ValidateMethod(){
			if(this.methodName != null && this.methodName.Trim() != ""){
				MethodInfo methodInfo = null;
				if(this.scope is Type){
					if(this.scope.HasMethod(this.methodName,BindingFlags.Static|BindingFlags.Public,(Type)this.scope)){
						methodInfo = this.scope.GetMethod(this.methodName,BindingFlags.Static|BindingFlags.Public,(Type)this.scope);
					}
				}
				else{
					if(this.scope.HasMethod(this.methodName,BindingFlags.Static|BindingFlags.Public)){
						methodInfo = this.scope.GetMethod(this.methodName,BindingFlags.Static|BindingFlags.Public);
					}
				}
				if(methodInfo != null){
					ParameterInfo[] parameters = methodInfo.GetParameters();
					this.simple = null;
					this.basic = null;
					this.full = null;
					if(parameters.Length == 0){
						this.simple = (Method)Delegate.CreateDelegate(typeof(Method),methodInfo);
					}
					else if(parameters.Length == 1){
						this.basic = (ConsoleMethod)Delegate.CreateDelegate(typeof(ConsoleMethod),methodInfo);
					}
					else if(parameters.Length == 2){
						this.full = (ConsoleMethodFull)Delegate.CreateDelegate(typeof(ConsoleMethodFull),methodInfo);
					} 
				}
				else{
					this.simple = null;
					this.basic = null;
					this.full = null;
				}
			}
			else{
				this.methodName = "";
				this.simple = null;
				this.basic = null;
				this.full = null;
			}
		}
		public List<string> ListDelegates(){
			List<string> delegates = new List<string>();
			delegates.Add(" ");
			List<Type> types = new List<Type>();
			if(this.scope == null){
				return delegates;
			}
			if(this.scope is Type){
				delegates.AddRange(this.scope.ListMethods(types,BindingFlags.Static|BindingFlags.Public,(Type)this.scope));
				types.Add(typeof(string[]));
				delegates.AddRange(this.scope.ListMethods(types,BindingFlags.Static|BindingFlags.Public,(Type)this.scope));
				types.Add(typeof(bool));
				delegates.AddRange(this.scope.ListMethods(types,BindingFlags.Static|BindingFlags.Public,(Type)this.scope));
			}
			else{
				delegates.AddRange(this.scope.ListMethods(types,BindingFlags.Static|BindingFlags.Public));
				types.Add(typeof(string[]));
				delegates.AddRange(this.scope.ListMethods(types,BindingFlags.Static|BindingFlags.Public));
				types.Add(typeof(bool));
				delegates.AddRange(this.scope.ListMethods(types,BindingFlags.Static|BindingFlags.Public));
			}
			return delegates;
		}
	}
	public List<ConsoleData> shortcuts = new List<ConsoleData>();
	public List<ConsoleData> keywords = new List<ConsoleData>();
	public List<ConsoleData> cvars = new List<ConsoleData>();
	public void OnEnable(){
		if(Application.isPlaying){
			Debug.Log("Creating Console Commands for " + this.gameObject.name);
			foreach(ConsoleData data in this.cvars){
				string cvarKey = data.key;
				if(Console.cvars.ContainsKey(cvarKey)){
					Debug.LogWarning("Cvar " + cvarKey + " already exists. Won`t be added again.");
				}
				else{
					data.ValidateScope(this);
					if(data.scope == null){
						Debug.Log("Skipping Cvar " + cvarKey);
					}
					else if(data.simple == null && data.basic == null && data.full == null){
						Console.AddCvar(cvarKey,data.scope,data.name,data.fullName,data.help);
						Debug.Log("Added Cvar " + cvarKey);
					}
					else{
						if(data.simple != null){
							Console.AddCvarMethod(cvarKey,data.scope,data.name,data.fullName,data.help,data.simple);
						}
						else if(data.basic != null){
							Console.AddCvarMethod(cvarKey,data.scope,data.name,data.fullName,data.help,data.basic);
						}
						else{
							Console.AddCvarMethod(cvarKey,data.scope,data.name,data.fullName,data.help,data.full);
						}
						Debug.Log("Added Cvar Method" + cvarKey);
					}

				}
			}
			foreach(ConsoleData data in this.shortcuts){
				string shortcutKey = data.key;
				if(Console.shortcuts.ContainsKey(shortcutKey)){
					Debug.LogWarning("Shortcut " + shortcutKey + " already exists. Won`t be added again.");
				}
				else{
					string shortcutReplacement = data.replace;
					Console.AddShortcut(shortcutKey,shortcutReplacement);
					Debug.Log("Added Shortcut " + shortcutKey);
				}
			}
			foreach(ConsoleData data in this.keywords){
				string keywordKey = data.key;
				if(Console.keywords.ContainsKey(keywordKey)){
					Debug.LogWarning("Keyword " + keywordKey + " already exists. Won`t be added again.");
				}
				else{
					if(data.simple != null){
						Console.AddKeyword(keywordKey,data.simple,data.minimumParameters,data.help);
					}
					else if(data.basic != null){
						Console.AddKeyword(keywordKey,data.basic,data.minimumParameters,data.help);
					}
					else{
						Console.AddKeyword(keywordKey,data.full,data.minimumParameters,data.help);
					}
					Debug.Log("Added Keyword" + keywordKey);
				}
			}
		}
	}
}

