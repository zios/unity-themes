using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using SystemFile = System.IO.File;
using UnityObject = UnityEngine.Object;
namespace Zios.File{
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	using Zios.Supports.Hierarchy;
	using Zios.Unity.ProxyEditor;
	using Zios.Unity.Log;
	using Zios.Unity.Proxy;
	using Zios.Unity.SystemAttributes;
	using Zios.Unity.Time;
	[InitializeOnLoad]
	public static partial class File{
		private static string root;
		public static string dataPath;
		public static bool monitor = true;
		private static bool debug = false;
		private static bool clock = false;
		private static bool needsSave = false;
		private static bool fullScan = true;
		private static float lastMonitor;
		public static Action refreshHooks = ()=>{};
		private static Dictionary<string,FileMonitor> monitors = new Dictionary<string,FileMonitor>();
		public static Dictionary<string,List<FileData>> filesByPath = new Dictionary<string,List<FileData>>(StringComparer.InvariantCultureIgnoreCase);
		public static Dictionary<string,List<FileData>> filesByType = new Dictionary<string,List<FileData>>(StringComparer.InvariantCultureIgnoreCase);
		public static Dictionary<string,FileData> folders = new Dictionary<string,FileData>(StringComparer.InvariantCultureIgnoreCase);
		public static Dictionary<string,FileData[]> cache = new Dictionary<string,FileData[]>();
		public static Dictionary<UnityObject,object> assets = new Dictionary<UnityObject,object>();
		public static Dictionary<UnityObject,string> assetPaths = new Dictionary<UnityObject,string>();
		public static Hierarchy<Type,string,string,UnityObject> namedAssets = new Hierarchy<Type,string,string,UnityObject>();
		static File(){
			var needsPersistent = !Proxy.IsEditor() && Application.platform.MatchesAny("IPhonePlayer","MetroPlayerX86","MetroPlayerX64","MetroPlayerARM");
			File.dataPath = needsPersistent ? Application.persistentDataPath : Application.dataPath;
			File.Refresh();
		}
		public static void Monitor(){
			if(!File.monitor){return;}
			var time = Time.Get();
			if(time>File.lastMonitor){
				foreach(var item in File.monitors){
					if(item.Value.WasChanged()){
						File.Refresh();
						break;
					}
				}
				File.lastMonitor = time + 0.3f;
			}
		}
		//===============
		// Storage
		//===============
		public static void Load(){
			Time.Start();
			var cachePath = Proxy.IsEditor() ? "Temp/File.data" : File.root+"/File.data";
			if(File.Exists(cachePath)){
				string extension = "";
				string lastPath = "";
				var lines = SystemFile.ReadAllLines(cachePath);
				for(int index=0;index<lines.Length;++index){
					var line = lines[index];
					if(line.StartsWith("(")){extension = line.Parse("(",")");}
					else if(line.StartsWith("=") || line.StartsWith("+")){
						lastPath = line.StartsWith("=") ? line.TrimLeft("=").Replace("$",File.root) : lastPath + line.TrimLeft("+");
						var folderData = new FileData();
						folderData.name = lastPath.GetPathTerm();
						folderData.directory = lastPath.GetDirectory();
						folderData.path = lastPath;
						folderData.isFolder = true;
						File.BuildCache(folderData);
					}
					else{
						var fileData = new FileData();
						fileData.directory = lastPath;
						fileData.name = line;
						fileData.fullName = fileData.name+"."+extension;
						fileData.path = fileData.directory+"/"+fileData.fullName;
						fileData.extension = extension;
						File.BuildCache(fileData);
					}
				}
			}
			if(File.clock){Log.Show("[File] : Load cache complete -- " + Time.Passed() + ".");}
		}
		public static void CheckSave(){
			if(File.needsSave){
				File.needsSave = false;
				File.Save();
			}
		}
		public static void Save(){
			string lastPath = ")@#(*$";
			Time.Start();
			var cachePath = Proxy.IsEditor() ? "Temp/File.data" : File.root+"/File.data";
			if(Proxy.IsEditor()){File.Create("Temp");}
			using(var output = new StreamWriter(cachePath,false)){
				foreach(var item in File.filesByType){
					var extension = item.Key;
					var files = item.Value;
					output.WriteLine("("+extension+")");
					foreach(var file in files){
						File.SaveData(file,output,ref lastPath);
					}
				}
			}
			if(File.clock){Log.Show("[File] : Save cache complete -- " + Time.Passed() + ".");}
		}
		public static void SaveData(FileData data,StreamWriter output,ref string lastPath){
			var directory = data.directory.Replace(File.root,"$");
			if(directory == lastPath){}
			else if(directory.Contains(lastPath)){
				var addition = directory.Replace(lastPath,"");
				output.WriteLine("+"+addition);
				lastPath += addition;
			}
			else{
				output.WriteLine("="+directory);
				lastPath = directory;
			}
			output.WriteLine(data.name);
		}
		//===============
		// Setup
		//===============
		public static void Refresh(){
			Time.Start();
			File.refreshHooks();
			File.assets.Clear();
			File.assetPaths.Clear();
			File.filesByPath.Clear();
			File.filesByType.Clear();
			File.folders.Clear();
			File.cache.Clear();
			File.root = Proxy.IsEditor() || Application.platform.MatchesAny("WindowsPlayer","OSXPlayer","LinuxPlayer") ? File.dataPath.GetDirectory() : File.dataPath;
			var needsScan = !Proxy.IsEditor() || (Proxy.IsEditor() && !Proxy.IsPlaying());
			if(needsScan){
				File.Scan(File.root);
				if(Proxy.IsEditor() || Application.platform.MatchesAny("WindowsPlayer","OSXPlayer","LinuxPlayer")){File.Scan(File.root+"/Temp",true);}
				if(File.fullScan){File.Scan(File.dataPath,true);}
				if(File.clock){Log.Show("[File] : Scan complete -- " + Time.Passed() + ".");}
				File.Save();
			}
			else{
				File.Load();
			}
			if(File.clock){Log.Show("[File] : Refresh complete -- " + Time.Passed() + ".");}
		}
		public static void Scan(string directory,bool deep=false){
			if(!Directory.Exists(directory)){return;}
			string[] fileEntries = Directory.GetFiles(directory);
			string[] folderEntries = Directory.GetDirectories(directory);
			if(!File.monitors.ContainsKey(directory)){
				File.monitors[directory] = new FileMonitor(directory);
			}
			File.filesByPath.AddNew(directory);
			foreach(string filePath in fileEntries){
				if(filePath.ContainsAny(".meta","unitytemp","unitylock")){continue;}
				var path = filePath.Replace("\\","/");
				File.BuildCache(new FileData(path));
			}
			foreach(string folderPath in folderEntries){
				if(folderPath.ContainsAny(".svn","~",".git")){continue;}
				var path = folderPath.Replace("\\","/");
				File.BuildCache(new FileData(path,true));
				if(deep){File.Scan(path,true);}
			}
		}
		public static void BuildCache(FileData file){
			File.cache["!"+file.path.ToLower()] = file.AsArray();
			if(!file.isFolder){
				File.cache["!"+file.fullName.ToLower()] = file.AsArray();
				File.filesByType.AddNew(file.extension).Add(file);
				File.filesByPath.AddNew(file.directory).Add(file);
				return;
			}
			File.folders[file.path] = file;
		}
		//===============
		// Primary
		//===============
		public static FileData[] FindAll(string name,bool showWarnings=true,bool returnFirstMatch=false){
			name = name.Replace("\\","/");
			Time.Start();
			if(name == "" && showWarnings){
				Log.Warning("[File] No path given for search.");
				return null;
			}
			string searchKey = name.ToLower();
			if(File.cache.ContainsKey(searchKey)){
				if(File.clock){
					Log.Show("[File] : Find [" + name + "] complete (cached:" + File.cache[searchKey].Count() + ") -- " + Time.Passed() + ".");
				}
				return File.cache[searchKey];
			}
			if(name.StartsWith("!")){name = name.ReplaceFirst("!","");}
			if(name.ContainsAny("<",">","?",":","|")){
				if(name[1] != ':'){
					if(File.debug){Log.Warning("[File] Path has invalid characters -- " + name);}
					return new FileData[0];
				}
			}
			string fileName = name.GetFileName();
			string path = name.GetDirectory();
			string type = name.GetFileExtension().ToLower();
			var results = new List<FileData>();
			var types = new List<string>();
			var allTypes = File.filesByType.Keys;
			if(type.IsEmpty() || type == "*"){types = allTypes.ToList();}
			else if(type.StartsWith("*")){types.AddRange(allTypes.Where(x=>x.EndsWith(type.Remove("*"),true)));}
			else if(type.EndsWith("*")){types.AddRange(allTypes.Where(x=>x.StartsWith(type.Remove("*"),true)));}
			else if(File.filesByType.ContainsKey(type)){types.Add(type);}
			foreach(var typeName in types){
				File.SearchType(fileName,typeName,path,returnFirstMatch,ref results);
			}
			if(results.Count == 0){
				foreach(var item in File.folders){
					FileData folder = item.Value;
					string folderPath = item.Key;
					if(folderPath.Matches(name,true) || folderPath.EndsWith(name,true)){
						results.Add(folder);
					}
				}
			}
			if(results.Count == 0 && !name.Contains(".")){return File.FindAll(name+".*",showWarnings,returnFirstMatch);}
			if(results.Count == 0 && showWarnings){Log.Warning("[File] Path [" + name + "] could not be found.");}
			File.cache[searchKey] = results.ToArray();
			if(File.clock){Log.Show("[File] : Find [" + name + "] complete (" + results.Count + ") -- " + Time.Passed() + ".");}
			return results.ToArray();
		}
		public static void SearchType(string name,string type,string path,bool firstOnly,ref List<FileData> results){
			bool pathSearch = !path.IsEmpty() && File.filesByPath.ContainsKey(path);
			var files = pathSearch ? File.filesByPath[path] : File.filesByType[type];
			if(File.debug){Log.Show("[File] Search -- " + name + " -- " + type + " -- " + path);}
			foreach(FileData file in files){
				bool correctPath = pathSearch ? true : file.path.Contains(path+"/",true);
				bool correctType = !pathSearch ? true : file.extension.Matches(type,true);
				bool wildcard = name.IsEmpty() || name == "*";
				wildcard = wildcard || name.StartsWith("*") && file.name.EndsWith(name.Remove("*"),true);
				wildcard = wildcard || (name.EndsWith("*") && file.name.StartsWith(name.Remove("*"),true));
				if(correctPath && correctType && (wildcard || file.name.Matches(name,true))){
					results.Add(file);
					if(firstOnly){return;}
				}
			}
		}
		public static FileData AddNew(string path){
			return File.Find(path,false) ?? File.Create(path);
		}
		public static string GetAssetPath(UnityObject target){
			if(!File.assetPaths.ContainsKey(target)){
				File.assetPaths[target] = ProxyEditor.GetAssetPath(target);
			}
			return File.assetPaths[target];
		}
		public static T GetAsset<T>(UnityObject target){
			#if UNITY_EDITOR
			if(Proxy.IsEditor()){
				if(!File.assets.ContainsKey(target)){
					string assetPath = File.GetAssetPath(target);
					object asset = ProxyEditor.LoadAsset(assetPath,typeof(T));
					if(asset == null){return default(T);}
					File.assets[target] = Convert.ChangeType(asset,typeof(T));
				}
				return (T)File.assets[target];
			}
			#endif
			return default(T);
		}
		public static FileData Create(string path){
			path = Path.GetFullPath(path).Replace("\\","/");
			var data = new FileData(path);
			if(!data.name.IsEmpty()){
				var folder = path.GetDirectory();
				if(!File.Exists(folder)){File.Create(folder);}
				SystemFile.Create(path).Dispose();
			}
			else{
				data.isFolder = true;
				Directory.CreateDirectory(path);
			}
			File.BuildCache(data);
			File.needsSave = true;
			return data;
		}
		public static void Copy(string path,string destination){
			SystemFile.Copy(path,destination,true);
		}
		public static void Delete(string path){
			var file = File.Find(path);
			if(!file.IsNull()){
				file.Delete();
			}
		}
		public static void Write(string path,byte[] bytes){File.AddNew(path).Write(bytes);}
		public static void Write(string path,string content){File.AddNew(path).Write(content);}
		public static void Write(string path,string[] lines){File.AddNew(path).Write(lines);}
		public static byte[] ReadBytes(string path){return File.AddNew(path).ReadBytes();}
		public static string ReadText(string path){return File.AddNew(path).ReadText();}
		public static IEnumerable<string> ReadLines(string path){return File.AddNew(path).ReadLines();}
		//===============
		// Shorthand
		//===============
		public static bool Exists(string path){return !path.IsEmpty() && (SystemFile.Exists(path) || Directory.Exists(path));}
		public static FileData Find(string name,bool showWarnings=true){
			name = !name.ContainsAny("*") ? "!"+name : name;
			var results = File.FindAll(name,showWarnings,!name.Contains("*"));
			if(results.Length > 0){return results[0];}
			return null;
		}
		public static FileData Get(UnityObject target,bool showWarnings=false){
			string path = File.dataPath.Replace("Assets","") + File.GetAssetPath(target);
			return File.Find(path,showWarnings);
		}
		public static string GetGUID(string name,bool showWarnings=true){
			FileData file = File.Find(name,showWarnings);
			if(file != null){return file.GetGUID();}
			return "";
		}
		public static string GetGUID(UnityObject target){
			return ProxyEditor.GetGUID(File.GetAssetPath(target));
		}
		public static T GetAsset<T>(string name,bool showWarnings=true) where T : UnityObject{
			FileData file = File.Find(name,showWarnings);
			if(file != null){return file.GetAsset<T>();}
			return default(T);
		}
		public static T[] GetAssets<T>(string name="*",bool showWarnings=true) where T : UnityObject{
			var files = File.FindAll(name,showWarnings);
			if(files.Length < 1){return new T[0];}
			return files.Select(x=>x.GetAsset<T>()).Where(x=>!x.IsNull()).ToArray();
		}
		public static Dictionary<string,T> GetNamedAssets<T>(string name="*",bool showWarnings=true) where T : UnityObject{
			if(!File.namedAssets.AddNew(typeof(T)).ContainsKey(name)){
				var files = File.GetAssets<T>(name,showWarnings).GroupBy(x=>x.name).Select(x=>x.First());
				File.namedAssets[typeof(T)][name] = files.ToDictionary(x=>x.name,x=>(UnityObject)x);
			}
			return File.namedAssets[typeof(T)][name].ToDictionary(x=>x.Key,x=>(T)x.Value);
		}
	}
	public class FileMonitor{
		public string path;
		public DateTime lastModify;
		public FileMonitor(string path){
			this.path = path;
			this.lastModify = Directory.GetLastWriteTime(this.path);
		}
		public bool WasChanged(){
			var modifyTime = Directory.GetLastWriteTime(this.path);
			if(this.lastModify != modifyTime){
				this.lastModify = modifyTime;
				return true;
			}
			return false;
		}
	}
	[Serializable]
	public class FileData{
		public string path;
		public string directory;
		public string name;
		public string fullName;
		public string extension;
		public bool isFolder;
		public object asset;
		public FileData(){}
		public FileData(string path,bool isFolder=false){
			this.path = path;
			this.directory = path.GetDirectory();
			this.name = path.GetFileName();
			this.extension = isFolder ? "" : path.GetFileExtension();
			this.fullName = isFolder ? this.name : this.name + "." + this.extension;
			this.isFolder = isFolder;
		}
		public override string ToString(){return this.path;}
		public IEnumerable<string> ReadLines(){return SystemFile.ReadAllLines(this.path);}
		public byte[] ReadBytes(){return SystemFile.ReadAllBytes(this.path);}
		public string ReadText(){return SystemFile.ReadAllText(this.path);}
		public void Write(byte[] bytes){SystemFile.WriteAllBytes(this.path,bytes);}
		public void Write(string contents){SystemFile.WriteAllText(this.path,contents);}
		public void Write(string[] lines){SystemFile.WriteAllLines(this.path,lines);}
		public void Delete(bool cacheOnly=false){
			foreach(var item in File.cache.Copy()){
				if(item.Value.Contains(this)){
					File.cache[item.Key] = item.Value.Remove(this);
					if(File.cache[item.Key].Length < 1){
						File.cache.Remove(item.Key);
					}
				}
			}
			if(!this.isFolder){
				if(!cacheOnly){SystemFile.Delete(this.path);}
				File.filesByType[this.extension].Remove(this);
				File.filesByPath[this.directory].Remove(this);
				return;
			}
			if(!cacheOnly){Directory.Delete(this.path);}
			File.folders.Remove(this.path);
		}
		public void MarkDirty(){SystemFile.SetLastWriteTime(this.path,DateTime.Now);}
		public string GetModifiedDate(string format="M-d-yy"){return SystemFile.GetLastWriteTime(this.path).ToString(format);}
		public string GetAccessedDate(string format="M-d-yy"){return SystemFile.GetLastAccessTime(this.path).ToString(format);}
		public string GetCreatedDate(string format="M-d-yy"){return SystemFile.GetCreationTime(this.path).ToString(format);}
		public string GetChecksum(){return this.ReadText().ToMD5();}
		public long GetSize(){return new FileInfo(this.path).Length;}
		public T GetAsset<T>() where T : UnityObject{
			if(Proxy.IsEditor()){
				if(this.asset.IsNull()){
					this.asset = ProxyEditor.LoadAsset(this.GetAssetPath(),typeof(T)).As<T>();
				}
				return this.asset.As<T>();
			}
			return default(T);
		}
		public string GetGUID(){
			return ProxyEditor.GetGUID(this.GetAssetPath());
		}
		public string GetAssetPath(){return this.path.GetAssetPath();}
		public string GetFolderPath(){
			return this.path.Substring(0,this.path.LastIndexOf("/")) + "/";
		}
	}
}
//===============
// Exetensions
//===============
namespace Zios.File{
	using Zios.Extensions;
	using Zios.Unity.ProxyEditor;
	using Zios.Unity.Extensions;
	public static partial class File{
		#if !UNITY_THEMES
		//[MenuItem("Zios/Format Code")]
		#endif
		public static void FormatCode(){
			var output = new StringBuilder();
			var current = "";
			foreach(var file in File.FindAll("*.cs")){
				var contents = file.ReadText();
				output.Clear();
				foreach(var line in contents.GetLines()){
					var leading = line.Substring(0,line.TakeWhile(char.IsWhiteSpace).Count()).Replace("    ","\t");
					current = leading+line.Trim().Replace("//","////");
					if(line.Trim().IsEmpty()){continue;}
					output.AppendLine(current);
				}
				file.Write(output.ToString().TrimEnd(null));
			}
		}
	}
	public static class TextureExtensions{
		public static Texture2D SaveAs(this Texture current,string path,bool useBlit=false){
			var texture = current is Texture2D ? (Texture2D)current : new Texture2D(1,1);
			if(useBlit){
				RenderTexture.active = new RenderTexture(current.width,current.height,0);
				Graphics.Blit(current,RenderTexture.active);
				texture = new Texture2D(current.width,current.height);
				texture.ReadPixels(new Rect(0,0,current.width,current.height),0,0);
				RenderTexture.active = null;
				RenderTexture.DestroyImmediate(RenderTexture.active);
			}
			File.Write(path,texture.EncodeToPNG());
			return texture;
		}
	}
	public static class UnityObjectExtensions{
		public static string GetAssetPath(this UnityObject current){
			return File.GetAssetPath(current);
		}
		public static bool IsAsset(this UnityObject current){
			return !File.GetAssetPath(current).IsEmpty();
		}
	}
	public static class GUIStyleExtensions{
		public static GUIStyle Font(this GUIStyle current,string value,bool asCopy=true){
			Font font = File.GetAsset<Font>(value);
			if(font != null){return current.Font(font,asCopy);}
			return current;
		}
		public static GUIStyle Background(this GUIStyle current,string value,bool asCopy=true){
			if(value.IsEmpty()){return current.Background(new Texture2D(0,0),asCopy);}
			Texture2D texture = File.GetAsset<Texture2D>(value);
			if(texture != null){return current.Background(texture,asCopy);}
			return current;
		}
	}
	public static class MonoBehaviourExtensions{
		public static string GetGUID(this MonoBehaviour current){
			var scriptFile = ProxyEditor.GetMonoScript(current);
			string path = File.GetAssetPath(scriptFile);
			return ProxyEditor.GetGUID(path);
		}
	}
}