using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityObject = UnityEngine.Object;
namespace Zios{
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
		public static Dictionary<string,FileData> folders = new Dictionary<string,FileData>();
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
				string fixedPath = folderPath.Replace("\\","/");
				folders[fixedPath] = new FileData(fixedPath);
			    FileManager.Scan(fixedPath);
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
			foreach(var item in FileManager.folders){
				FileData folder = item.Value;
				string folderPath = item.Key;
				if(folderPath.Matches(name,ignoreCase)){
					results.Add(folder);
				}
			}
		    if(results.Count == 0){
				if(FileManager.files.ContainsKey(type)){
					foreach(FileData file in FileManager.files[type]){
						bool correctPath = path != "" ? file.path.Contains(path,ignoreCase) : true;
						if(correctPath && (file.fullName.Matches(fileName,ignoreCase) || wildcard)){
							results.Add(file);
						}
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
	    public static FileData Get(UnityObject item,bool showWarnings=true){
			string path = FileManager.GetPath(item,false);
			return FileManager.Find(path);
	    }
	    public static string GetPath(UnityObject item,bool relative=true){
		    #if UNITY_EDITOR 
		    if(Application.isEditor){
				string assetPath = AssetDatabase.GetAssetPath(item);
				if(!relative){assetPath = Application.dataPath.Replace("Assets","") + assetPath;}
			    return assetPath;
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
		public bool isFolder;
	    public FileData(string path){
		    this.path = path;
		    this.fullName = Path.GetFileName(path);
			this.extension = Path.GetExtension(path).Trim(".");
			this.name = Path.GetFileNameWithoutExtension(path);
			this.isFolder = Directory.Exists(path);
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
}