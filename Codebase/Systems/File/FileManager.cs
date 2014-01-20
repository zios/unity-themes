using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
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
		Scan(Application.dataPath);
	}
	public static void Scan(string directory){
		string[] fileEntries = Directory.GetFiles(directory);
		string[] folderEntries = Directory.GetDirectories(directory);
		foreach(string filePath in fileEntries){
			string path = filePath.Replace("\\","/");
			string type = path.Substring(filePath.LastIndexOf(".")+1);
			if(!files.ContainsKey(type)){
				files[type] = new List<FileData>();
			}
			files[type].Add(new FileData(path));
		}
		foreach(string folderPath in folderEntries){
			if(folderPath.Contains(".svn")){continue;}
			Scan(folderPath);
		}
	}
	public static FileData Find(string name,bool showWarnings=true){
		if(name == ""){
			Debug.LogWarning("FileManager : No path given for search.");
			return null;
		}
		int period = name.LastIndexOf(".");
		int slash = name.LastIndexOf("/") + 1;
		string fileName = slash == -1 ? name : name.Substring(slash,period-slash);
		string type = name.Substring(period+1);
		string path = name.Substring(0,period).TrimRight(fileName);
		if(files.ContainsKey(type)){
			foreach(FileData file in files[type]){
				bool correctPath = slash != -1 ? file.path.Contains(path) : true;
				if(correctPath && file.name.Contains(fileName)){
					return file;
				}
			}
		}
		if(showWarnings){Debug.LogWarning("FileManager : Path [" + name + "] could not be found.");}
		return null;
	}
	public static string GetPath(object item){
		#if UNITY_EDITOR 
		if(Application.isEditor){
			return AssetDatabase.GetAssetPath((UnityEngine.Object)item);
		}
		#endif
		return "";
	}
	public static T GetAsset<T>(string name,bool showWarnings=true){
		FileData file = FileManager.Find(name,showWarnings);
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
}
public class FileData{
	public string path;
	public string name;
	public FileData(string path){
		this.path = path;
		this.name = path.Substring(path.LastIndexOf("/")+1);
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
	public string GetAssetPath(){
		return this.path.Substring(this.path.IndexOf("Assets"));
	}
	public string GetFolderPath(){
		return this.path.Substring(0,this.path.LastIndexOf("/")) + "/";
	}
}