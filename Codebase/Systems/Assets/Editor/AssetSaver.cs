using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml;
using MaterialExtended;
using UnityEditor;
using UnityEngine;
namespace Zios{
	public class AssetExport : MonoBehaviour{
		private static string foldersPath;
		private static Dictionary<Type,List<string>> skipProperties;
		private static Dictionary<string,NeutralEntity> mappedObjects;
		private static List<NeutralEntity> exportObjects;
		private static List<Type> exportTypes = new List<Type>(){
			typeof(GameObject),
			typeof(Animation),
			typeof(Mesh),
			typeof(Material),
			typeof(Texture),
			typeof(Particle),
			typeof(AudioSource),
			typeof(GUISkin)
		};
		private static readonly List<Type> accessTypes = new List<Type>(){
			typeof(long),
			typeof(string),
			typeof(int),
			typeof(bool),
			typeof(double),
			typeof(float)
		};
		[MenuItem("Zios/Process/Export Asset/Scene")]
		private static void SaveScene(){
			SaveObjects((GameObject[])FindObjectsOfType(typeof(GameObject)));
		}
		[MenuItem("Zios/Process/Export Asset/Selection")]
		private static void SaveSelectedObjects(){
			SaveObjects(Selection.gameObjects);
		}
		private static void SaveObjects(GameObject[] gameObjects){
			foldersPath = EditorUtility.SaveFolderPanel("Save objects to directory","","");
			System.Diagnostics.Stopwatch stopWatch = new System.Diagnostics.Stopwatch();
			stopWatch.Start();
			mappedObjects = new Dictionary<string,NeutralEntity>();
			exportObjects = new List<NeutralEntity>();
			skipProperties = new Dictionary<Type,List<string>>();
			List<string> skipInTransform = new List<string>();
			skipInTransform.Add("gameObject");
			List<string> skipInMatrix = new List<string>();
			skipInMatrix.Add("inverse");
			skipInMatrix.Add("isIdentity");
			skipInMatrix.Add("transpose");
			List<string> skipInQuaternion = new List<string>();
			skipInQuaternion.Add("eulerAngles");
			skipProperties.Add(typeof(Transform),skipInTransform);
			skipProperties.Add(typeof(Matrix4x4),skipInMatrix);
			skipProperties.Add(typeof(Quaternion),skipInQuaternion);
			foreach(GameObject gameObject in gameObjects){
				try{
					if(gameObject.transform.parent == null){
						MapUnityObject(gameObject);
					}
				}
				catch(Exception ex){
					Log(ex.ToString(),true);
				}
			}
			foreach(NeutralEntity neutralEntity in exportObjects){
				SaveXml(foldersPath,neutralEntity);
			}
			stopWatch.Stop();
			TimeSpan timeSpan = stopWatch.Elapsed;
			string elapsedTime = String.Format("{0:00}:{1:00}:{2:00}.{3:00}",timeSpan.Hours,timeSpan.Minutes,timeSpan.Seconds,timeSpan.Milliseconds / 10);
			Log("Process finished in " + elapsedTime,false);
		}
		private static void Log(string text,bool isError){
			if(isError){
				Debug.LogError(text);
			}
			else{
				Debug.Log(text);
			}
		}
		private static NeutralEntity MapUnityObject(UnityEngine.Object gameObject){
			if(gameObject == null){
				return null;
			}
			NeutralEntity neutralEntity = new NeutralEntity();
			neutralEntity.entityType = gameObject.GetType().Name;
			neutralEntity.entityHashCode = gameObject.GetHashCode();
			neutralEntity.name = gameObject.name.Split(' ')[0];
			if(mappedObjects.ContainsKey(gameObject.GetInstanceID().ToString())){
				return mappedObjects[gameObject.GetInstanceID().ToString()];
			}
			mappedObjects.Add(gameObject.GetInstanceID().ToString(),neutralEntity);
			foreach(Type exportType in exportTypes){
				if(exportType.IsAssignableFrom(gameObject.GetType())){
					exportObjects.Add(neutralEntity);
					break;
				}
			}
			MapObject(gameObject,neutralEntity);
			if(gameObject is GameObject){
				UnityEngine.Component[] components = ((GameObject)gameObject).GetComponents<UnityEngine.Component>();
				foreach(UnityEngine.Component component in components){
					NeutralEntity neutralComponent = MapUnityObject(component);
					if(neutralComponent != null){
						neutralEntity.PopulateArray("components",neutralComponent);
					}
				}
			}
			return neutralEntity;
		}
		private static NeutralEntity MapReference(UnityEngine.Object gameObject){
			if(gameObject == null){
				return null;
			}
			NeutralEntity neutralEntity = MapUnityObject(gameObject);
			foreach(Type exportType in exportTypes){
				if(exportType.IsAssignableFrom(gameObject.GetType())){
					NeutralEntity referenceEntity = new NeutralEntity();
					referenceEntity.entityType = gameObject.GetType().Name;
					referenceEntity.entityHashCode = gameObject.GetHashCode();
					referenceEntity.name = gameObject.name.Split(' ')[0];
					referenceEntity.AddAttribute("references",gameObject.name);
					return referenceEntity;
				}
			}
			return neutralEntity;
		}
		private static void MapObject(object obj,NeutralEntity neutralEntity){
			if(obj == null){
				return;
			}
			if(neutralEntity == null){
				neutralEntity = new NeutralEntity();
			}
			neutralEntity.entityType = obj.GetType().Name;
			neutralEntity.entityHashCode = obj.GetHashCode();
			Dictionary<string,object> attributes = new Dictionary<string, object>();
			if(obj is Material){
				try{
					ExtendedMaterial data = ScriptableObject.CreateInstance<ExtendedMaterial>();
					data.Load((Material)obj);
					foreach(KeyValuePair<string,Property> item in data.properties){
						Property property = item.Value;
						string variable = property.variable;
						PropertyType shaderType = property.type;
						if(shaderType == PropertyType.Color){
							attributes.Add(variable,((Material)obj).GetColor(variable));
						}
						else if(shaderType == PropertyType.Cube || shaderType == PropertyType.Rect || shaderType == PropertyType.Texture){
							attributes.Add(variable,((Material)obj).GetTexture(variable));
						}
						else if(shaderType == PropertyType.Float || shaderType == PropertyType.Range){
							attributes.Add(variable,((Material)obj).GetFloat(variable));
						}
					}
				}
				catch(Exception ex){
					Log("Material '" + ((Material)obj).name + "' Exception: " + ex.ToString(),true);
				}
			}
			else{
				MapFields(obj,attributes);
				MapProperties(obj,attributes);
				if(obj.GetType().Equals(typeof(Transform))){
					List<Transform> children = new List<Transform>();
					foreach(Transform child in (Transform)obj){
						if(child != null){
							children.Add(child);
						}
					}
					if(children.Count > 0){
						attributes.Add("children",children);
					}
				}
			}
			foreach(KeyValuePair<string,object> attribute in attributes){
				object attributeValue = attribute.Value;
				Type attributeType = attributeValue.GetType();
				if(attributeType.IsArray || attributeValue is ICollection){
					foreach(object innerAttribute in (ICollection)attributeValue){
						MapAttribute(neutralEntity,obj,attribute.Key,innerAttribute,true);
					}
				}
				else{
					MapAttribute(neutralEntity,obj,attribute.Key,attributeValue,false);
				}
			}
		}
		private static void MapFields(object obj,Dictionary<string,object> attributes){
			Type objectType = obj.GetType();
			foreach(FieldInfo f in obj.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public)){
				if(MustSkip(f.Name,f.FieldType,objectType) || HasEquivalent(obj,f.Name)){
					continue;
				}
				object fieldValue = f.GetValue(obj);
				if(fieldValue != null && !fieldValue.Equals(null)){
					if(!IsDefault(fieldValue,f.Name)){
						attributes.Add(f.Name,fieldValue);
					}
				}
			}
		}
		private static void MapProperties(object obj,Dictionary<string,object> attributes){
			Type objectType = obj.GetType();
			foreach(PropertyInfo p in obj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public)){
				if(p.GetIndexParameters().Length > 0 || MustSkip(p.Name,p.PropertyType,objectType) || HasEquivalent(obj,p.Name)){
					continue;
				}
				object propertyValue = null;
				try{
					if(!p.IsDefined(typeof(ObsoleteAttribute),true)){
						propertyValue = p.GetValue(obj,null);
					}
				}
				catch(MissingComponentException ex){
					Log("\t\t\t\t[ERROR][" + p.PropertyType.Name + " " + p.Name + "] MISSING COMPONENT: " + ex.ToString(),true);
				}
				catch(Exception ex){
					Log("\t\t\t\t[ERROR][" + p.PropertyType.Name + " " + p.Name + "] EXCEPTION: " + ex.ToString(),true);
				}
				if(propertyValue != null && !propertyValue.Equals(null)){
					if(!IsDefault(propertyValue,p.Name)){
						attributes.Add(p.Name,propertyValue);
					}
				}
			}
		}
		private static bool HasEquivalent(object obj,String elementName){
			String capitalizedName = elementName.Substring(0,1).ToUpper() + elementName.Substring(1);
			string[] equivalencePrefixes = new string[]{"shared", "Shared"};
			foreach(string prefix in equivalencePrefixes){
				string equivalentName = prefix + capitalizedName;
				if(obj.GetType().GetField(equivalentName) != null || obj.GetType().GetProperty(equivalentName) != null){
					return true;
				}
			}
			return false;
		}
		private static bool MustSkip(String elementName,Type elementType,Type objectType){
			if(elementType == objectType){
				return true;
			}
			foreach(Type typeToSkip in skipProperties.Keys){
				if(typeToSkip == objectType && skipProperties[objectType].Contains(elementName)){
					return true;
				}
				else if(typeToSkip.IsAssignableFrom(objectType) && skipProperties[typeToSkip].Contains(elementName)){
					return true;
				}
			}
			return false;
		}
		private static bool IsDefault(object elementValue,string elementName){
			Type elementType = elementValue.GetType();
			if(elementType == typeof(Vector3) && !elementName.ToLower().Contains("scale") && elementValue.Equals(Vector3.zero)){
				return true;
			}
			if(elementType == typeof(Vector3) && elementName.ToLower().Contains("scale") && elementValue.Equals(new Vector3(1,1,1))){
				return true;
			}
			if(elementType == typeof(Vector2) && elementValue.Equals(Vector2.zero)){
				return true;
			}
			if(elementType == typeof(Vector4) && elementValue.Equals(Vector4.zero)){
				return true;
			}
			if(elementType == typeof(Quaternion) && elementValue.Equals(Quaternion.identity)){
				return true;
			}
			return false;
		}
		private static void MapAttribute(NeutralEntity neutralEntity,object parentObject,String attributeName,object attributeValue,bool isCollectionMember){
			Type attributeType = attributeValue.GetType();
			Type parentType = parentObject.GetType();
			if(!attributeType.IsPublic || (!isCollectionMember && attributeType == parentType) || attributeValue == null || attributeValue.Equals(null)){
				return;
			}
			if(attributeType.IsPrimitive || accessTypes.Contains(attributeType) || attributeType.Equals(typeof(System.Single)) || attributeType.IsEnum){
				if(isCollectionMember){
					neutralEntity.PopulateArray(attributeName,attributeValue);
				}
				else{
					neutralEntity.AddAttribute(attributeName,attributeValue);
				}
			}
			else{
				NeutralEntity neutralAttribute = null;
				if(attributeValue is UnityEngine.Object){
					neutralAttribute = MapReference((UnityEngine.Object)attributeValue);
				}
				else{
					neutralAttribute = new NeutralEntity();
					MapObject(attributeValue,neutralAttribute);
				}
				if(isCollectionMember){
					neutralEntity.PopulateArray(attributeName,neutralAttribute);
				}
				else{
					neutralEntity.AddAttribute(attributeName,neutralAttribute);
				}
			}
		}
		private static void SaveXml(string foldersPath,NeutralEntity neutralEntity){
			String path = foldersPath + "\\" + neutralEntity.entityType;
			List<string> serializedCodes = new List<string>();
			System.IO.Directory.CreateDirectory(path);
			XmlDocument xmlDoc = new XmlDocument();
			XmlElement xmlElement = BuildXml(xmlDoc,neutralEntity.entityType,neutralEntity,serializedCodes);
			xmlDoc.AppendChild(xmlElement);
			using(XmlTextWriter writer = new XmlTextWriter(path + "\\" + neutralEntity.name + ".u" +neutralEntity.entityType+ "Xml" ,null)){
				writer.Formatting = Formatting.Indented;
				writer.Indentation = 1;
				writer.IndentChar = '\t';
				xmlDoc.Save(writer);
			}
		}
		private static XmlElement BuildXml(XmlDocument xmlDoc,String nodeName,NeutralEntity neutralEntity,List<string> serializedCodes){
			string code = nodeName + "_" + neutralEntity.entityHashCode;
			XmlElement xmlElement = xmlDoc.CreateElement(nodeName);
			xmlElement.SetAttribute("code",code);
			xmlElement.SetAttribute("type",neutralEntity.entityType);
			if(!serializedCodes.Contains(code)){
				if(neutralEntity.entityHashCode != 0){
					serializedCodes.Add(code);
				}
				foreach(KeyValuePair<string,object> attribute in neutralEntity.objectAttributes){
					if(attribute.Value is NeutralEntity){
						xmlElement.AppendChild(BuildXml(xmlDoc,attribute.Key,(NeutralEntity)attribute.Value,serializedCodes));
					}
					else{
						xmlElement.SetAttribute(attribute.Key,attribute.Value.ToString());
					}
				}
				foreach(KeyValuePair<string,List<object>> attribute in neutralEntity.collectionAttributes){
					List<object> collection = attribute.Value;
					object firstElement = collection[0];
					XmlElement xmlCollection = xmlDoc.CreateElement(attribute.Key);
					if(firstElement is NeutralEntity){
						foreach(object element in collection){
							xmlCollection.AppendChild(BuildXml(xmlDoc,attribute.Key,(NeutralEntity)element,serializedCodes));
						}
					}
					else{
						foreach(object element in collection){
							XmlElement xmlCollectionElement = xmlDoc.CreateElement(attribute.Key);
							xmlCollectionElement.SetAttribute("value",element.ToString());
							xmlCollection.AppendChild(xmlCollectionElement);
						}
						xmlElement.SetAttribute(attribute.Key,attribute.Value.ToString());
					}
					xmlElement.AppendChild(xmlCollection);
				}
			}
			return xmlElement;
		}
	}
}