using System.Collections.Generic;
using System.IO;
namespace Zios.Console{
	using Zios.File;
	public partial class Console{
		private static List<string> configOutput = new List<string>();
		public static void SaveConfig(){
				using(StreamWriter file = new StreamWriter(Console.Get().configFile,false)){
					foreach(string line in Console.configOutput){
						file.WriteLine(line);
				}
			}
		}
		public static void LoadConfig(string name){
			if(name != "" && File.Exists(name)){
				using(StreamReader file = new StreamReader(name)){
					string line = "";
					while((line = file.ReadLine()) != null){
						Console.AddCommand(line,true);
					}
				}
			}
		}
		public static void LoadConfig(string[] values){
			Console.LoadConfig(values[1]);
		}
		public static void DeleteConfig(string name){
			if(name != "" && File.Exists(name)){
				File.Delete(name);
			}
		}
	}
}