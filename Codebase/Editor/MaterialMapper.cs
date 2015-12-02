using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using MenuFunction = UnityEditor.GenericMenu.MenuFunction;
namespace Zios.UI{
	[Serializable]
	public class ShaderInfo{
		public bool found;
		public string name;
		public string type;
		public string mapTo;
	}
	public class MaterialMapper : EditorWindow{
		public int sourceCount = 4;
		public Shader[] shaders = new Shader[4];
		public Shader goal;
		public GUILayoutOption labelWidth = GUILayout.Width(1);
		public List<ShaderInfo> keywordMap = new List<ShaderInfo>();
		public Dictionary<string,List<string>> goalProperties = new Dictionary<string,List<string>>();
		private Vector2 scrollPosition;
		[MenuItem("Zios/Window/Material Mapper")]
		private static void Init(){
			MaterialMapper window = (MaterialMapper)EditorWindow.GetWindow(typeof(MaterialMapper));
			window.position = new Rect(100,150,200,200);
			window.Start();
		}
		public void OnGUI(){
			this.scrollPosition = EditorGUILayout.BeginScrollView(this.scrollPosition);
			EditorGUIUtility.labelWidth = 100;
			this.titleContent = new GUIContent("MaterialMapper");
			this.shaders = this.shaders.Remove(this.goal).Resize(this.sourceCount);
			this.labelWidth = GUILayout.Width(Screen.width/2-20);
			this.sourceCount = Mathf.Max(1,this.sourceCount.DrawInt("Source Count"));
			if(EditorGUILayoutExtensionSpecial.DrawFoldout("Shaders")){this.DrawShaders();}
			this.BuildKeywords();
			if(EditorGUILayoutExtensionSpecial.DrawFoldout("Keywords")){this.DrawKeywords();}
			if("Process".DrawButton()){this.Process();}
			EditorGUILayout.EndScrollView();
		}
		public void DrawShaders(){
			EditorGUI.indentLevel += 1;
			GUILayout.BeginHorizontal();
			GUILayout.BeginVertical(this.labelWidth);
			for(int index=0;index<this.shaders.Length;++index){
				this.shaders[index] = this.shaders[index].Draw<Shader>((index+1).ToString(),false,true);
			}
			GUILayout.EndVertical();
			GUILayout.BeginVertical(this.labelWidth);
			this.goal = this.goal.Draw<Shader>("Goal");
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
			EditorGUI.indentLevel -= 1;
			if(GUILayoutUtility.GetLastRect().Clicked(1)){
				GenericMenu menu = new GenericMenu();
				MenuFunction loadUsed = ()=>this.LoadUsed();
				menu.AddItem(new GUIContent("Load Used Shaders"),false,loadUsed);
				menu.ShowAsContext();
				Event.current.Use();
			}
		}
		public void DrawKeywords(){
			EditorGUI.indentLevel += 1;
			GUILayout.BeginHorizontal();
			GUILayout.BeginVertical(this.labelWidth);
			GUI.enabled = false;
			foreach(var info in this.keywordMap){
				GUILayout.BeginHorizontal();
				info.name.Draw(info.type,GUI.skin.GetStyle("TextField").Alignment("MiddleRight"),true);
				GUILayout.EndHorizontal();
			}
			GUI.enabled = true;
			GUILayout.EndVertical();
			GUILayout.BeginVertical(this.labelWidth);
			foreach(var info in this.keywordMap){
				List<string> types = this.goalProperties.ContainsKey(info.type) ? this.goalProperties[info.type] : new List<string>{"[No Matching]"};
				int selected = types.IndexOf(info.mapTo);
				if(selected == -1){selected = 0;}
				selected = types.Draw(selected);
				info.mapTo = types[selected];
			}
			GUILayout.EndVertical();
			GUILayout.EndHorizontal();
			EditorGUI.indentLevel -= 1;
		}
		public void BuildKeywords(){
			this.keywordMap.ForEach(x=>x.found=false);
			for(int index=0;index<this.shaders.Length;++index){
				var shader = this.shaders[index];
				if(shader == null){continue;}
				int propertyCount = ShaderUtil.GetPropertyCount(shader);
				for(int propertyIndex=0;propertyIndex<propertyCount;++propertyIndex){
					string name = ShaderUtil.GetPropertyName(shader,propertyIndex);
					var info = this.keywordMap.FirstOrDefault(x=>x.name==name);
					if(info == null){
						info = new ShaderInfo();
						info.name = name;
						info.type = ShaderUtil.GetPropertyType(shader,propertyIndex).ToName();
						if(info.type == "Range"){info.type = "Float";}
						this.keywordMap.Add(info);
					}
					info.found = true;
				}
			}
			this.keywordMap.RemoveAll(x=>!x.found);
			this.goalProperties.Clear();
			if(this.goal != null){
				int propertyCount = ShaderUtil.GetPropertyCount(this.goal);
				for(int propertyIndex=0;propertyIndex<propertyCount;++propertyIndex){
					string name = ShaderUtil.GetPropertyName(this.goal,propertyIndex);
					string type = ShaderUtil.GetPropertyType(this.goal,propertyIndex).ToName();
					if(type == "Range"){type = "Float";}
					this.goalProperties.AddNew(type).AddNew("[Ignore]");
					this.goalProperties.AddNew(type).AddNew(name);
				}
			}
		}
		public void LoadUsed(){
			FileData[] files = FileManager.FindAll("*.mat");
			List<Shader> shaders = new List<Shader>();
			foreach(var file in files){
				var material = file.GetAsset<Material>();
				if(this.goal != material.shader){shaders.AddNew(material.shader);}
			}
			this.sourceCount = shaders.Count;
			this.shaders = shaders.ToArray();
		}
		public void Process(){
			if(this.goal == null){
				Debug.LogWarning("[MaterialMapper] : No goal shader selected.");
				return;
			}
			AssetDatabase.CreateAsset(new Material(this.goal),"Assets/Temporary.mat");
			string goalHeader = FileManager.Find("Temporary.mat").GetText().Cut("%YAML","m_SavedProperties");
			string goalName = goalHeader.Parse("m_Name:","\n");
			AssetDatabase.DeleteAsset("Assets/Temporary.mat");
			List<string> guids = new List<string>();
			List<FileData> matching = new List<FileData>();
			FileData[] allMaterials = FileManager.FindAll("*.mat");
			foreach(var shader in this.shaders){
				if(shader == null){continue;}
				var path = AssetDatabase.GetAssetPath(shader);
				guids.AddNew(AssetDatabase.AssetPathToGUID(path));
			}
			AssetDatabase.StartAssetEditing();
			foreach(FileData materialFile in allMaterials){
				Material material = materialFile.GetAsset<Material>();
				string text = materialFile.GetText();
				foreach(string guid in guids){
					string idLine = "guid: "+guid;
					bool repair = material.shader.name.Contains("Hidden/InternalErrorShader");
					if(repair || text.Contains(idLine)){
						string header = text.Cut("%YAML","m_SavedProperties");
						string name = header.Parse("m_Name:","\n");
						foreach(ShaderInfo info in this.keywordMap){
							if(!info.mapTo.ContainsAny("[Ignore]","[No Matching]")){
								text = text.Replace(" "+info.name," "+info.mapTo);
							}
						}
						text = text.Replace(header,goalHeader);
						text = text.Replace(goalName,name);
						materialFile.WriteText(text);
						matching.AddNew(materialFile);
						break;
					}
				}
			}
			AssetDatabase.StopAssetEditing();
			MaterialCleaner.Clean(matching.ToArray());
			Debug.Log("[MaterialMapper] : " + matching.Count + " materials modified.");
		}
		public void Start(){}
	}
}