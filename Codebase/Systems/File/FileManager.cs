using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityObject = UnityEngine.Object;
namespace Zios{
	using Events;
	using Containers;
	#if UNITY_EDITOR
	using UnityEditor;
	#endif
	public static class FileManager{
		private static string path;
		private static bool setup;
		private static bool debug = false;
		private static bool clock = false;
		private static bool fullScan = true;
		private static DateTime pathModifyTime;
		public static Dictionary<string,List<FileData>> filesByPath = new Dictionary<string,List<FileData>>(StringComparer.InvariantCultureIgnoreCase);
		public static Dictionary<string,List<FileData>> filesByType = new Dictionary<string,List<FileData>>(StringComparer.InvariantCultureIgnoreCase);
		public static Dictionary<string,FileData> folders = new Dictionary<string,FileData>(StringComparer.InvariantCultureIgnoreCase);
		public static Dictionary<string,FileData[]> cache = new Dictionary<string,FileData[]>();
		public static Dictionary<UnityObject,object> assets = new Dictionary<UnityObject,object>();
		public static Hierarchy<Type,string,string,UnityObject> namedAssets = new Hierarchy<Type,string,string,UnityObject>();
		//===============
		// Storage
		//===============
		public static void Load(){
			var time = Time.realtimeSinceStartup;
			if(FileManager.Exists("Temp/FileManager.data")){
				int mode = 0;
				string extension = "";
				string lastPath = "";
				var lines = File.ReadAllLines("Temp/FileManager.data");
				for(int index=0;index<lines.Length;++index){
					var line = lines[index];
					if(line.Contains("[Files]")){mode = 1;}
					else if(line.Contains("[Folders]")){mode = 2;}
					else if(line.StartsWith("(")){extension = line.Parse("(",")");}
					else if(line.StartsWith("=")){lastPath = line.Remove("=").Replace("$",FileManager.path);}
					else if(line.StartsWith("+")){lastPath += line.Remove("+");}
					else{
						var fileData = new FileData();
						fileData.directory = lastPath;
						fileData.name = line;
						if(mode == 1){
							fileData.fullName = fileData.name+"."+extension;
							fileData.path = fileData.directory+"/"+fileData.fullName;
							fileData.extension = extension;
						}
						else if(mode == 2){
							fileData.path = fileData.directory+"/"+fileData.name;
							fileData.isFolder = true;
						}
						fileData.BuildCache();
					}
				}
			}
			if(FileManager.clock){Debug.Log("[FileManager] : Load cache complete -- " + (Time.realtimeSinceStartup-time) + " seconds.");}
		}
		public static void Save(){
			string lastPath = ")@#(*$";
			var time = Time.realtimeSinceStartup;
			FileManager.Create("Temp");
			using(var output = new StreamWriter("Temp/FileManager.data",false)){
				output.WriteLine("[Files]");
				foreach(var item in FileManager.filesByType){
					var extension = item.Key;
					var files = item.Value;
					output.WriteLine("("+extension+")");
					foreach(var file in files){
						FileManager.SaveData(file,output,ref lastPath);
					}
				}
				output.WriteLine("[Folders]");
				foreach(var item in FileManager.folders){
					FileManager.SaveData(item.Value,output,ref lastPath);
				}
			}
			if(FileManager.clock){Debug.Log("[FileManager] : Save cache complete -- " + (Time.realtimeSinceStartup-time) + " seconds.");}
		}
		public static void SaveData(FileData data,StreamWriter output,ref string lastPath){
			var directory = data.directory.Replace(FileManager.path,"$");
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
		public static void Monitor(){
			var time = Time.realtimeSinceStartup;
			var modifyTime = Directory.GetLastWriteTime(FileManager.path);
			if(FileManager.pathModifyTime != modifyTime){
				FileManager.pathModifyTime = modifyTime;
				FileManager.Refresh();
				if(FileManager.clock){Debug.Log("[FileManager] : Monitor sync complete -- " + (Time.realtimeSinceStartup-time) + " seconds.");}
			}
		}
		public static void Refresh(){
			var time = Time.realtimeSinceStartup;
			FileManager.assets.Clear();
			FileManager.filesByPath.Clear();
			FileManager.filesByType.Clear();
			FileManager.folders.Clear();
			FileManager.cache.Clear();
			FileManager.path = Application.dataPath.GetDirectory();
			FileManager.pathModifyTime = Directory.GetLastWriteTime(FileManager.path);
			FileManager.Scan(FileManager.path);
			if(!Application.isPlaying){
				if(FileManager.fullScan){FileManager.Scan(Application.dataPath,true);}
				FileManager.Save();
			}
			else{
				FileManager.Load();
			}
			FileManager.setup = true;
			Event.Add("On Editor Update",FileManager.Monitor).SetPermanent();
			Event.Add("On Asset Changed",FileManager.Refresh).SetPermanent();
			if(FileManager.clock){Debug.Log("[FileManager] : Refresh complete -- " + (Time.realtimeSinceStartup-time) + " seconds.");}
		}
		public static void Scan(string directory,bool deep=false){
			string[] fileEntries = Directory.GetFiles(directory);
			string[] folderEntries = Directory.GetDirectories(directory);
			FileManager.filesByPath.AddNew(directory);
			foreach(string filePath in fileEntries){
				if(filePath.Contains(".meta")){continue;}
				var path = filePath.Replace("\\","/");
				new FileData(path).BuildCache();
			}
			foreach(string folderPath in folderEntries){
				if(folderPath.ContainsAny(".svn","~","Temp")){continue;}
				var path = folderPath.Replace("\\","/");
				new FileData(path,true).BuildCache();
				if(deep){FileManager.Scan(path,true);}
			}
		}
		//===============
		// Primary
		//===============
		public static FileData[] FindAll(string name,bool showWarnings=true,bool firstOnly=false){
			if(!FileManager.setup){FileManager.Refresh();}
			var time = Time.realtimeSinceStartup;
			if(name == "" && showWarnings){
				Debug.LogWarning("[FileManager] No path given for search.");
				return null;
			}
			string searchKey = name.ToLower();
			if(FileManager.cache.ContainsKey(searchKey)){
				return FileManager.cache[searchKey];
			}
			if(name.StartsWith("!")){name = name.ReplaceFirst("!","");}
			if(name.ContainsAny("<",">","?",":","|")){
				if(name[1] != ':') {
					if(FileManager.debug){Debug.LogWarning("[FileManager] Path has invalid characters -- " + name);}
					return new FileData[0];
				}
			}
			if(!name.Contains(".") && name.EndsWith("*")){name = name + ".*";}
			if(name.Contains("*")){firstOnly = false;}
			else if(name.Contains(":")){firstOnly = true;}
			string fileName = name.GetFileName();
			string path = name.GetDirectory();
			string type = name.GetFileExtension().ToLower();
			var results = new List<FileData>();
			var types = new List<string>();
			var allTypes = FileManager.filesByType.Keys;
			if(type.IsEmpty() || type == "*"){types = allTypes.ToList();}
			else if(type.StartsWith("*")){types.AddRange(allTypes.Where(x=>x.EndsWith(type.Remove("*"),true)));}
			else if(type.EndsWith("*")){types.AddRange(allTypes.Where(x=>x.StartsWith(type.Remove("*"),true)));}
			else if(FileManager.filesByType.ContainsKey(type)){types.Add(type);}
			foreach(var typeName in types){
				FileManager.SearchType(fileName,typeName,path,firstOnly,ref results);
			}
			if(results.Count == 0){
				foreach(var item in FileManager.folders){
					FileData folder = item.Value;
					string folderPath = item.Key;
					if(folderPath.Matches(name,true)){
						results.Add(folder);
					}
				}
			}
			if(results.Count == 0 && showWarnings){Debug.LogWarning("[FileManager] Path [" + name + "] could not be found.");}
			FileManager.cache[searchKey] = results.ToArray();
			if(FileManager.clock){Debug.Log("[FileManager] : Find [" + name + "] complete (" + results.Count + ") -- " + (Time.realtimeSinceStartup-time) + " seconds.");}
			return results.ToArray();
		}
		public static void SearchType(string name,string type,string path,bool firstOnly,ref List<FileData> results){
			bool pathSearch = !path.IsEmpty() && FileManager.filesByPath.ContainsKey(path);
			var files = pathSearch ? FileManager.filesByPath[path] : FileManager.filesByType[type];
			if(FileManager.debug){Debug.Log("[FileManager] Search -- " + name + " -- " + type + " -- " + path);}
			foreach(FileData file in files){
				bool correctPath = pathSearch ? true : file.path.Contains(path,true);
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
		public static string GetPath(UnityObject target,bool relative=true){
			if(!FileManager.setup){FileManager.Refresh();}
			#if UNITY_EDITOR
			if(Application.isEditor){
				string assetPath = AssetDatabase.GetAssetPath(target);
				if(!relative){assetPath = Application.dataPath.Replace("Assets","") + assetPath;}
				return assetPath;
			}
			#endif
			return "";
		}
		public static T GetAsset<T>(UnityObject target){
			#if UNITY_EDITOR
			if(Application.isEditor){
				if(!FileManager.setup){FileManager.Refresh();}
				if(!FileManager.assets.ContainsKey(target)){
					string assetPath = FileManager.GetPath(target);
					object asset = AssetDatabase.LoadAssetAtPath(assetPath,typeof(T));
					if(asset == null){return default(T);}
					FileManager.assets[target] = Convert.ChangeType(asset,typeof(T));
				}
				return (T)FileManager.assets[target];
			}
			#endif
			return default(T);
		}
		public static FileData Create(string path){
			path = Path.GetFullPath(path).Replace("\\","/");
			var data = new FileData(path);
			if(!data.name.IsEmpty()){
				File.Create(path).Dispose();
			}
			else{
				data.isFolder = true;
				Directory.CreateDirectory(path);
			}
			data.BuildCache();
			return data;
		}
		public static void Copy(string path,string destination){
			File.Copy(path,destination,true);
		}
		public static void Delete(string path){
			var file = FileManager.Find(path);
			if(!file.IsNull()){
				file.Delete();
			}
		}
		public static void WriteFile(string path,byte[] bytes){
			var folder = path.GetDirectory();
			if(!FileManager.Exists(folder)){FileManager.Create(folder);}
			FileStream stream = new FileStream(path,FileMode.Create);
			BinaryWriter file = new BinaryWriter(stream);
			file.Write(bytes);
			file.Close();
			stream.Close();
		}
		//===============
		// Shorthand
		//===============
		public static bool Exists(string path){return File.Exists(path) || Directory.Exists(path);}
		public static FileData Find(string name,bool showWarnings=true){
			name = !name.ContainsAny("*") ? "!"+name : name;
			var results = FileManager.FindAll(name,showWarnings,true);
			if(results.Length > 0){return results[0];}
			return null;
		}
		public static FileData Get(UnityObject target,bool showWarnings=false){
			string path = FileManager.GetPath(target,false);
			return FileManager.Find(path,showWarnings);
		}
		public static string GetGUID(string name,bool showWarnings=true){
			FileData file = FileManager.Find(name,showWarnings);
			if(file != null){return file.GetGUID();}
			return "";
		}
		public static string GetGUID(UnityObject target){
			#if UNITY_EDITOR
			return AssetDatabase.AssetPathToGUID(FileManager.GetPath(target));
			#else
			return "";
			#endif
		}
		public static T GetAsset<T>(string name,bool showWarnings=true){
			FileData file = FileManager.Find(name,showWarnings);
			if(file != null){return file.GetAsset<T>();}
			return default(T);
		}
		public static T[] GetAssets<T>(string name="*",bool showWarnings=true){
			var files = FileManager.FindAll(name,true,showWarnings);
			if(files.Length < 1){return new T[0];}
			return files.Select(x=>x.GetAsset<T>()).Where(x=>!x.IsNull()).ToArray();
		}
		public static Dictionary<string,T> GetNamedAssets<T>(string name="*",bool showWarnings=true) where T : UnityObject{
			if(!FileManager.namedAssets.AddNew(typeof(T)).ContainsKey(name)){
				var files = FileManager.GetAssets<T>(name,showWarnings).GroupBy(x=>x.name).Select(x=>x.First());
				FileManager.namedAssets[typeof(T)][name] = files.ToDictionary(x=>x.name,x=>(UnityObject)x);
			}
			return FileManager.namedAssets[typeof(T)][name].ToDictionary(x=>x.Key,x=>(T)x.Value);
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
		public FileData(){}
		public FileData(string path,bool isFolder=false){
			this.path = path;
			this.directory = path.GetDirectory();
			this.extension = path.GetFileExtension();
			this.name = path.GetFileName();
			this.fullName = this.name + "." + this.extension;
			this.isFolder = isFolder;
		}
		public void BuildCache(){
			FileManager.cache["!"+this.path.ToLower()] = this.AsArray();
			if(!this.isFolder){
				FileManager.cache["!"+this.fullName.ToLower()] = this.AsArray();
				FileManager.filesByType.AddNew(this.extension).Add(this);
				FileManager.filesByPath.AddNew(this.directory).Add(this);
				return;
			}
			FileManager.folders[this.path] = this;
		}
		public string GetText(){
			return File.ReadAllText(this.path);
		}
		public void WriteText(string contents){
			File.WriteAllText(this.path,contents);
		}
		public void Delete(){
			foreach(var item in FileManager.cache.Copy()){
				if(item.Value.Contains(this)){
					FileManager.cache[item.Key] = item.Value.Remove(this);
					if(FileManager.cache[item.Key].Length < 1){
						FileManager.cache.Remove(item.Key);
					}
				}
			}
			if(!this.isFolder){
				File.Delete(this.path);
				FileManager.filesByType[this.extension].Remove(this);
				FileManager.filesByPath[this.path].Remove(this);
				return;
			}
			Directory.Delete(this.path);
			FileManager.folders.Remove(this.path);
		}
		public void MarkDirty(){File.SetLastWriteTime(this.path,DateTime.Now);}
		public string GetModifiedDate(string format="M-d-yy"){return File.GetLastWriteTime(this.path).ToString(format);}
		public string GetAccessedDate(string format="M-d-yy"){return File.GetLastAccessTime(this.path).ToString(format);}
		public string GetCreatedDate(string format="M-d-yy"){return File.GetCreationTime(this.path).ToString(format);}
		public string GetChecksum(){return this.GetText().ToMD5();}
		public long GetSize(){return new FileInfo(this.path).Length;}
		public T GetAsset<T>(){
			#if UNITY_EDITOR
			if(Application.isEditor && this.path.IndexOf("Assets") != -1){
				return (T)AssetDatabase.LoadAssetAtPath(this.GetAssetPath(),typeof(T)).Box();
			}
			#endif
			return default(T);
		}
		public string GetGUID(){
			#if UNITY_EDITOR
			if(Application.isEditor){
				return AssetDatabase.AssetPathToGUID(this.GetAssetPath());
			}
			#endif
			return "";
		}
		public string GetAssetPath(bool full=true){
			string path = this.path.Substring(this.path.IndexOf("Assets"));
			if(full){return path;}
			return path.Cut(0,path.LastIndexOf("/"));
		}
		public string GetFolderPath(){
			return this.path.Substring(0,this.path.LastIndexOf("/")) + "/";
		}
	}
}