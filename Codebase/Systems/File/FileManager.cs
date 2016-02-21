using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityObject = UnityEngine.Object;
namespace Zios{
	using Events;
	#if UNITY_EDITOR
	using UnityEditor;
	#endif
	public static class FileManager{
		private static bool setup;
		private static string path;
		private static DateTime pathModifyTime;
		public static Dictionary<string,List<FileData>> files = new Dictionary<string,List<FileData>>();
		public static Dictionary<string,FileData> folders = new Dictionary<string,FileData>();
		public static Dictionary<string,FileData[]> cache = new Dictionary<string,FileData[]>();
		public static Dictionary<UnityObject,object> assets = new Dictionary<UnityObject,object>();
		//===============
		// Storage
		//===============
		public static void Load(){
			if(File.Exists("FileManager.data")){
				int mode = 0;
				string extension = "";
				string lastPath = "";
				var lines = File.ReadAllLines("FileManager.data");
				for(int index=0;index<lines.Length;++index){
					var line = lines[index];
					if(line.Contains("[Files]")){mode = 1;}
					else if(line.Contains("[Folders]")){mode = 2;}
					else if(line.StartsWith("(")){
						extension = line.Parse("(",")");
						FileManager.files[extension] = new List<FileData>();
					}
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
							FileManager.files[extension].Add(fileData);
						}
						else if(mode == 2){
							fileData.path = fileData.directory+"/"+fileData.name;
							fileData.isFolder = true;
							FileManager.folders[fileData.path] = fileData;
						}
					}
				}
			}
		}
		public static void Save(){
			string lastPath = ")@#(*$";
			using(var output = new StreamWriter("FileManager.data",false)){
				output.WriteLine("[Files]");
				foreach(var item in FileManager.files){
					if(item.Key.Contains("meta")){continue;}
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
			var modifyTime = Directory.GetLastWriteTime(FileManager.path);
			if(FileManager.pathModifyTime != modifyTime){
				FileManager.pathModifyTime = modifyTime;
				if(Application.isPlaying){
					var rootFiles = FileManager.files.SelectMany(x=>x.Value).Where(x=>x.directory==FileManager.path).ToArray();
					foreach(var file in rootFiles){FileManager.files[file.extension].Remove(file);}
					FileManager.cache.Clear();
					FileManager.Scan(FileManager.path,false);
					return;
				}
				FileManager.Refresh();
			}
		}
		public static void Refresh(){
			FileManager.assets.Clear();
			FileManager.files.Clear();
			FileManager.folders.Clear();
			FileManager.cache.Clear();
			FileManager.path = Application.dataPath.GetDirectory();
			FileManager.pathModifyTime = Directory.GetLastWriteTime(FileManager.path);
			FileManager.Scan(FileManager.path,false);
			if(!Application.isPlaying){
				FileManager.Scan(Application.dataPath);
				FileManager.Save();
			}
			else{
				FileManager.Load();
			}
			FileManager.setup = true;
			Event.Add("On Editor Update",FileManager.Monitor).SetPermanent();
			Event.Add("On Asset Changed",FileManager.Refresh).SetPermanent();
		}
		public static void Scan(string directory,bool deep=true){
			string[] fileEntries = Directory.GetFiles(directory);
			string[] folderEntries = Directory.GetDirectories(directory);
			foreach(string filePath in fileEntries){
				var path = filePath.Replace("\\","/");
				var data = new FileData(path);
				FileManager.files.AddNew(data.extension).Add(data);
			}
			foreach(string folderPath in folderEntries){
				if(folderPath.Contains(".svn")){continue;}
				var path = folderPath.Replace("\\","/");
				var data = new FileData(path,true);
				FileManager.folders[path] = data;
				if(deep){FileManager.Scan(path);}
			}
		}
		//===============
		// Primary
		//===============
		public static FileData[] FindAll(string name,bool ignoreCase=true,bool showWarnings=true){
			if(!FileManager.setup){FileManager.Refresh();}
			if(name == "" && showWarnings){
				Debug.LogWarning("[FileManager] No path given for search.");
				return null;
			}
			string searchKey = name+"-"+ignoreCase.ToString();
			if(FileManager.cache.ContainsKey(searchKey)){
				return FileManager.cache[searchKey];
			}
			if(name.ContainsAny("<",">","?",":","|")){
				if(name[1] != ':') {
					Debug.LogWarning("[FileManager] Path has invalid characters -- " + name);
					return new FileData[0];
				}
			}
			string fileName = name.GetFileName();
			string path = name.GetDirectory();
			string type = name.GetExtension().ToLower();
			bool wildcard = fileName.Length > 0 && fileName.Contains("*");
			var results = new List<FileData>();
			foreach(var item in FileManager.folders){
				FileData folder = item.Value;
				string folderPath = item.Key;
				if(folderPath.Matches(name,ignoreCase)){
					results.Add(folder);
				}
			}
			if(results.Count == 0){
				if(type.IsEmpty()){
					foreach(var fileType in FileManager.files){
						FileManager.SearchType(fileName,fileType.Key,path,ignoreCase,wildcard,ref results);
					}
				}
				else if(FileManager.files.ContainsKey(type)){
					FileManager.SearchType(fileName,type,path,ignoreCase,wildcard,ref results);
				}
			}
			if(results.Count == 0 && showWarnings){Debug.LogWarning("[FileManager] Path [" + name + "] could not be found.");}
			FileManager.cache[searchKey] = results.ToArray();
			return results.ToArray();
		}
		public static void SearchType(string name,string type,string path,bool ignoreCase,bool wildcard,ref List<FileData> results){
			foreach(FileData file in FileManager.files[type]){
				bool correctPath = path != "" ? file.path.Contains(path,ignoreCase) : true;
				bool wildMatch = wildcard && (name.IsEmpty() || file.name.Contains(name.Remove("*"),ignoreCase));
				if(correctPath && (wildMatch || file.name.Matches(name,ignoreCase))){

					results.Add(file);
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
					string assetPath = AssetDatabase.GetAssetPath(target);
					object asset = AssetDatabase.LoadAssetAtPath(assetPath,typeof(T));
					if(asset == null){return default(T);}
					FileManager.assets[target] = Convert.ChangeType(asset,typeof(T));
				}
				return (T)FileManager.assets[target];
			}
			#endif
			return default(T);
		}
		public static FileData CreateFile(string path){
			File.Create(path).Dispose();
			path = Path.GetFullPath(path).Replace("\\","/");
			var data = new FileData(path);
			FileManager.files.AddNew(data.extension).Add(data);
			FileManager.cache.Clear();
			return data;
		}
		public static void DeleteFile(string path){
			var file = FileManager.Find(path);
			if(!file.IsNull()){
				file.Delete();
				FileManager.cache.Clear();
				FileManager.assets.Clear();
				FileManager.files[file.extension].Remove(file);
			}
		}
		public static void WriteFile(string path,byte[] bytes){
			FileStream stream = new FileStream(path,FileMode.Create);
			BinaryWriter file = new BinaryWriter(stream);
			file.Write(bytes);
			file.Close();
			stream.Close();
		}
		//===============
		// Shorthand
		//===============
		public static FileData Find(string name,bool ignoreCase=true,bool showWarnings=true){
			FileData[] results = FileManager.FindAll(name,ignoreCase,showWarnings);
			if(results.Length > 0){return results[0];}
			return null;
		}
		public static FileData Get(UnityObject target,bool showWarnings=false){
			string path = FileManager.GetPath(target,false);
			return FileManager.Find(path,true,showWarnings);
		}
		public static string GetGUID(string name,bool showWarnings=true){
			FileData file = FileManager.Find(name,showWarnings);
			if(file != null){return file.GetGUID();}
			return "";
		}
		public static T GetAsset<T>(string name,bool showWarnings=true){
			FileData file = FileManager.Find(name,true,showWarnings);
			if(file != null){return file.GetAsset<T>();}
			return default(T);
		}
		public static T[] GetAssets<T>(string name,bool showWarnings=true){
			var files = FileManager.FindAll(name,true,showWarnings);
			if(files.Length < 1){return new T[0];}
			return files.Select(x=>x.GetAsset<T>()).Where(x=>!x.IsNull()).ToArray();
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
			this.extension = path.GetExtension();
			this.name = path.GetFileName();
			this.fullName = this.name + "." + this.extension;
			this.isFolder = isFolder;
		}
		public string GetText(){
			return File.ReadAllText(this.path);
		}
		public void WriteText(string contents){
			File.WriteAllText(this.path,contents);
		}
		public void Delete(){
			File.Delete(this.path);
		}
		public string GetModifiedDate(string format="M-d-yy"){return File.GetLastWriteTime(this.path).ToString(format);}
		public string GetAccessedDate(string format="M-d-yy"){return File.GetLastAccessTime(this.path).ToString(format);}
		public string GetCreatedDate(string format="M-d-yy"){return File.GetCreationTime(this.path).ToString(format);}
		public string GetChecksum(){return this.GetText().ToMD5();}
		public T GetAsset<T>(){
			#if UNITY_EDITOR
			if(Application.isEditor){
				object asset = AssetDatabase.LoadAssetAtPath(this.GetAssetPath(),typeof(T));
				return (T)Convert.ChangeType(asset,typeof(T));
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