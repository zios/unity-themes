using System.Collections.Generic;
namespace Zios.Console{
	using Zios.Shortcuts;
	using Zios.Unity.Log;
	using Zios.Unity.Proxy;
	public partial class Console{
		public static Dictionary<string,ConsoleCallback> keywords = new Dictionary<string,ConsoleCallback>();
		public static void AddKeyword(string name,ConsoleCallback call){
			if(!Proxy.IsPlaying()){return;}
			if(Console.keywords.ContainsKey(name)){
				Log.Warning("[Console] Already has registered Keyword for -- " + name);
				return;
			}
			Console.keywords.Add(name,call);
		}
		public static void AddKeyword(string name,Method method=null,int minimumParameters=-1,string help=""){
			if(!Proxy.IsPlaying()){return;}
			ConsoleCallback call = new ConsoleCallback();
			call.simple = method;
			call.help = help;
			call.minimumParameters = minimumParameters;
			Console.AddKeyword(name,call);
		}
		public static void AddKeyword(string name,ConsoleMethod method=null,int minimumParameters=-1,string help=""){
			if(!Proxy.IsPlaying()){return;}
			ConsoleCallback call = new ConsoleCallback();
			call.basic = method;
			call.help = help;
			call.minimumParameters = minimumParameters;
			Console.AddKeyword(name,call);
		}
		public static void AddKeyword(string name,ConsoleMethodFull method=null,int minimumParameters=-1,string help=""){
			if(!Proxy.IsPlaying()){return;}
			ConsoleCallback call = new ConsoleCallback();
			call.full = method;
			call.help = help;
			call.minimumParameters = minimumParameters;
			Console.AddKeyword(name,call);
		}
		public static void AddShortcut(string term,params string[] shortcuts){
			if(!Proxy.IsPlaying()){return;}
			foreach(string name in shortcuts){
				Console.AddShortcut(term,name);
			}
		}
	}
	public delegate void ConsoleMethod(string[] values);
	public delegate void ConsoleMethodFull(string[] values,bool help);
	public class ConsoleCallback{
		public Method simple;
		public ConsoleMethod basic;
		public ConsoleMethodFull full;
		public int minimumParameters = -1;
		public string help;
	}
}