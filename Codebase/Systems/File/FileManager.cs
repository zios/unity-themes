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
		private static Dictionary<string,List<FileData>> files = new Dictionary<string,List<FileData>>();
		private static Dictionary<string,FileData> folders = new Dictionary<string,FileData>();
		private static Dictionary<string,FileData[]> cache = new Dictionary<string,FileData[]>();
		private static Dictionary<UnityObject,object> assets = new Dictionary<UnityObject,object>();
		//===============
		// Storage
		//===============
		public static void Load(){
			if(File.Exists("FileManager.data")){
				int mode = 0;
				string line = "";
				string extension = "";
				using(var input = new StreamReader("FileManager.data")){
					while(true){
						line = input.ReadLine();
						if(line.IsNull()){break;}
						if(line.Contains("[Files]")){mode = 1;}
						else if(line.Contains("[Folders]")){mode = 2;}
						else if(line.StartsWith("(")){
							extension = line.Parse("(",")");
							FileManager.files[extension] = new List<FileData>();
						}
						else{
							var data = line.Split('|');
							var fileData = new FileData();
							fileData.path = data[0];
							fileData.folder = data[1];
							if(mode == 1){
								fileData.name = data[2];
								fileData.fullName = data[3];
								fileData.extension = extension;
								FileManager.files[extension].Add(fileData);
							}
							else if(mode == 2){
								fileData.isFolder = true;
								FileManager.folders[fileData.path] = fileData;
							}
						}
					}
				}
			}
		}
		public static void Save(){
			using(var output = new StreamWriter("FileManager.data",false)){
				output.WriteLine("[Files]");
				foreach(var item in FileManager.files){
					var extension = item.Key;
					var files = item.Value;
					output.WriteLine("("+extension+")");
					foreach(var file in files){
						output.WriteLine(file.path+"|"+file.folder+"|"+file.name+"|"+file.fullName);
					}
				}
				output.WriteLine("[Folders]");
				foreach(var folder in FileManager.folders){
					output.WriteLine(folder.Key+"|"+folder.Value.folder);
				}
			}
		}
		//===============
		// Setup
		//===============
		public static void Refresh(){
			FileManager.assets.Clear();
			FileManager.files.Clear();
			FileManager.folders.Clear();
			FileManager.cache.Clear();
			FileManager.Scan(Application.dataPath.GetDirectory(),false);
			if(!Application.isPlaying){
				Event.Add("On Asset Changed",FileManager.Refresh).SetPermanent();
				FileManager.Scan(Application.dataPath);
				FileManager.Save();
			}
			else{
				FileManager.Load();
			}
			FileManager.setup = true;
		}
		public static void Scan(string directory,bool deep=true){
			string[] fileEntries = Directory.GetFiles(directory);
			string[] folderEntries = Directory.GetDirectories(directory);
			foreach(string filePath in fileEntries){
				var data = new FileData(filePath.Replace("\\","/"));
				FileManager.files.AddNew(data.extension).Add(data);
			}
			foreach(string folderPath in folderEntries){
				if(folderPath.Contains(".svn")){continue;}
				string fixedPath = folderPath.Replace("\\","/");
				FileManager.folders[fixedPath] = new FileData(fixedPath,true);
				if(deep){FileManager.Scan(fixedPath);}
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
			string type = name.GetExtension().ToLower();
			string path = name.GetDirectory();
			bool wildcard = fileName.Length > 0 && fileName.Contains("*");
			List<FileData> results = new List<FileData>();
			foreach(var item in FileManager.folders){
				FileData folder = item.Value;
				string folderPath = item.Key;
				if(folderPath.Matches(name,ignoreCase)){
					results.Add(folder);
				}
			}
			if(results.Count == 0){
				if(type.IsEmpty()){
					foreach(var fileGroup in FileManager.files){
						type = fileGroup.Key;
						foreach(FileData file in FileManager.files[type]){
							bool correctPath = path != "" ? file.path.Contains(path,ignoreCase) : true;
							bool wildMatch = wildcard && (fileName.IsEmpty() || file.name.Contains(fileName.Remove("*"),ignoreCase));
							if(correctPath && (wildMatch || file.fullName.Matches(fileName,ignoreCase))){
								results.Add(file);
							}
						}
					}
				}
				else if(FileManager.files.ContainsKey(type)){
					foreach(FileData file in FileManager.files[type]){
						bool correctPath = path != "" ? file.path.Contains(path,ignoreCase) : true;
						bool wildMatch = wildcard && (fileName.IsEmpty() || file.name.Contains(fileName.Remove("*"),ignoreCase));
						if(correctPath && (wildMatch || file.name.Matches(fileName,ignoreCase))){
							results.Add(file);
						}
					}
				}
			}
			if(results.Count == 0 && showWarnings){Debug.LogWarning("[FileManager] Path [" + name + "] could not be found.");}
			FileManager.cache[searchKey] = results.ToArray();
			return results.ToArray();
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
			var data = new FileData(path.Replace("\\","/"));
			FileManager.files.AddNew(data.extension).Add(data);
			return data;
		}
		public static void DeleteFile(string path){
			if(!File.Exists(path)){
				if(!FileManager.setup){FileManager.Refresh();}
				var file = FileManager.Find(path);
				path = !file.IsNull() ? file.path : path;
			}
			File.Delete(path);
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
	public class FileData{
		public string path;
		public string folder;
		public string name;
		public string fullName;
		public string extension;
		public bool isFolder;
		public FileData(){}
		public FileData(string path,bool isFolder=false){
			this.path = path;
			this.folder = path.GetDirectory();
			this.extension = path.GetExtension();
			this.name = path.GetFileName();
			this.fullName = this.name + "." + this.extension;
			this.isFolder = false;
		}
		public string GetText(){
			return File.ReadAllText(this.path);
		}
		public void WriteText(string contents){
			File.WriteAllText(this.path,contents);
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