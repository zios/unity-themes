using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityObject = UnityEngine.Object;
#if UNITY_EDITOR 
using UnityEditor;
public class FileManagerWatcher : AssetPostprocessor{
	public static void OnPostprocessAllAssets(string[] imported,string[] deleted,string[] moved, string[] path){
		FileManager.Refresh();
	}
}
#endif
public static class FileManager{
	public static Dictionary<string,List<FileData>> files = new Dictionary<string,List<FileData>>();
	static FileManager(){Refresh();}
	public static void Refresh(){
		files.Clear();
		FileManager.Scan(Application.dataPath);
	}
	public static void Scan(string directory){
		string[] fileEntries = Directory.GetFiles(directory);
		string[] folderEntries = Directory.GetDirectories(directory);
		foreach(string filePath in fileEntries){
			string path = filePath.Replace("\\","/");
			string type = path.Substring(filePath.LastIndexOf(".")+1).ToLower();
			if(!files.ContainsKey(type)){
				files[type] = new List<FileData>();
			}
			files[type].Add(new FileData(path));
		}
		foreach(string folderPath in folderEntries){
			if(folderPath.Contains(".svn")){continue;}
			FileManager.Scan(folderPath);
		}
	}
	public static FileData[] FindAll(string name,bool ignoreCase=true,bool showWarnings=true){
		if(name == "" && showWarnings){
			Debug.LogWarning("[FileManager] No path given for search.");
			return null;
		}
		string fileName = Path.GetFileName(name);
		string type = Path.GetExtension(name).Trim(".").ToLower();
		string path = Path.GetDirectoryName(name);
		bool wildcard = fileName[0] == '*';
		List<FileData> results = new List<FileData>();
		if(files.ContainsKey(type)){
			foreach(FileData file in files[type]){
				bool correctPath = path != "" ? file.path.Contains(path,ignoreCase) : true;
				if(correctPath && (file.fullName.Matches(fileName,ignoreCase) || wildcard)){
					results.Add(file);
				}
			}
		}
		if(results.Count == 0 && showWarnings){Debug.LogWarning("[FileManager] Path [" + name + "] could not be found.");}
		return results.ToArray();
	}
	public static FileData Find(string name,bool ignoreCase=true,bool showWarnings=true){
		FileData[] results = FileManager.FindAll(name,ignoreCase,showWarnings);
		if(results.Length > 0){return results[0];}
		return null;
	}
	public static string GetPath(UnityObject item){
		#if UNITY_EDITOR 
		if(Application.isEditor){
			return AssetDatabase.GetAssetPath(item);
		}
		#endif
		return "";
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
	public static void WriteFile(string path,byte[] bytes){
		FileStream stream = new FileStream(path,FileMode.Create);
		BinaryWriter file = new BinaryWriter(stream);
		file.Write(bytes);
		file.Close();
		stream.Close();
	}
	public static void DeleteFile(string path){
		File.Delete(path);
	}
}
public class FileData{
	public string path;
	public string name;
	public string fullName;
	public string extension;
	public FileData(string path){
		this.path = path;
		this.fullName = path.Substring(path.LastIndexOf("/")+1);
		this.extension = path.Substring(path.LastIndexOf(".")+1);
		this.name = this.fullName.Replace("."+this.extension,"");
	}
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