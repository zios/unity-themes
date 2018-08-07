using System;
using System.Collections.Generic;
using UnityEngine;
namespace Zios.Supports.Variable{
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	using Zios.Reflection;
	using Zios.Supports.Hierarchy;
	using Zios.Unity.Extensions;
	using Zios.Unity.Locate;
	[Serializable]
	public class Variable{
		public static Hierarchy<string,Variable> all = new Hierarchy<string,Variable>();
		public string alias;
		public string serialized;
		protected string path;
		protected GameObject parent;
		public virtual void Build(){}
		static Variable(){
			foreach(var component in Locate.GetSceneComponents<Component>()){
				foreach(var variable in component.GetVariables<Variable>()){
					Variable.Add(component,variable.Key,variable.Value);
				}
			}
		}
		public static void Add(Component component,string name,Variable variable){
			var gamePath = component.gameObject.GetPath();
			var componentPath = component.GetPath();
			variable.parent = component.gameObject;
			variable.Build();
			if(!name.IsEmpty()){
				variable.path = gamePath+name;
				Variable.all[gamePath+name] = variable;
				Variable.all[componentPath+name] = variable;
			}
			if(!variable.alias.IsEmpty()){
				var alias = variable.alias;
				variable.path = gamePath+alias;
				Variable.all[gamePath+alias] = variable;
				Variable.all[componentPath+alias] = variable;
			}
		}
	}
	[Serializable]
	public class Variable<Type> : Variable{
		public Action<Type> Set;
		public Func<Type> Get;
		public override void Build(){
			if(this.Get == null){
				this.Get = this.Parse(this.serialized);
			}
			if(this.Set == null){
				this.Set = (value)=>{
					this.Get = ()=>{return value;};
				};
			}
		}
		public Func<Type> Parse(string data){
			var parts = new List<Variable>();
			var operations = new List<string>();
			var parent = this.parent;
			var operators = new string[]{"+","-","/","*","%","÷"};
			var functions = new string[]{"Flip","Abs","Sign","Floor","Ceil","Cos","Sin","Tan","ATan","Sqrt"};
			var comparers = new string[]{"Distance","Average","Max","Min"};
			foreach(var element in data.Split(" ")){
				if(element.ContainsAll("[","]")){
					var target = element.Remove("[","]");
					var path = "";
					foreach(var term in target.Split("/")){
						if(term == ".."){parent = parent.GetParent();}
						if(term == "@Next"){parent = parent.gameObject.GetNextSibling(true);}
						if(term == "@Previous"){parent = parent.gameObject.GetPreviousSibling(true);}
						else{path = parent.GetPath() + term;}
					}
					if(Variable.all.ContainsKey(path)){
						parts.Add(Variable.all[path]);
					}
					continue;
				}
				bool hasMethod = element.ContainsAll("(",")");
				bool hasOperator = element.ContainsAny(operators);
				bool hasFunction = hasMethod && element.ContainsAny(functions);
				bool hasComparer = hasMethod && element.ContainsAny(comparers);
				if(hasOperator){operations.Add(element);}
				else{
					var entry = new Variable<Type>();
					if(element.StartsWith("(") && element.EndsWith(")")){
						entry.Parse(element.Trim("(",")"));
					}
					else{
						var entryValue = element.Convert<Type>();
						entry.Get = ()=>{return entryValue;};
					}
					//entry.Convert(entry);
				}
			}
			if(parts.Count == 1){
				var value = parts[0].As<Variable<Type>>();
				return value.Get;
			}
			return ()=>{return this.Handle(parts,operations);};
		}
		public Type Handle(List<Variable> parts,List<string> operations){return default(Type);}
	}
	[Serializable] public class VariableInt : Variable<int>{}
	[Serializable] public class VariableFloat : Variable<float>{}
	[Serializable] public class VariableString : Variable<string>{}
	[Serializable] public class VariableBool : Variable<bool>{}
	[Serializable] public class VariableRect : Variable<Rect>{}
	[Serializable] public class VariableVector2 : Variable<Vector2>{}
	[Serializable] public class VariableVector3 : Variable<Vector3>{}
	[Serializable] public class VariableVector4 : Variable<Vector4>{}
	[Serializable] public class VariableQuaternion : Variable<Quaternion>{}
	[Serializable] public class VariableGameObject : Variable<GameObject>{}
}