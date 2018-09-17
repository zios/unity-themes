using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
namespace Zios.Extensions{
	public static class StringExtension{
		//============================
		// Standard
		//============================
		public static string ToLetterSequence(this string current){
			char lastDigit = current[current.Length-1];
			if(current.Length > 1 && current[current.Length-2] == ' ' && char.IsLetter(lastDigit)){
				char nextLetter = (char)(char.ToUpper(lastDigit)+1);
				return current.TrimEnd(lastDigit) + nextLetter;
			}
			return current + " B";
		}
		public static string ToCapitalCase(this string current){
			return current[0].ToString().ToUpper() + current.Substring(1);
		}
		public static string ToTitleCase(this string current){
			string text = Regex.Replace(current,"(\\B[A-Z])"," $1");
			text = CultureInfo.CurrentCulture.TextInfo.ToTitleCase(text);
			text = text.Replace("3 D","3D").Replace("2 D","2D");
			return text;
		}
		public static string ToPascalCase(this string current){
			return current.ToTitleCase().Remove(" ");
		}
		public static string ToCamelCase(this string current){
			return current[0].ToString().ToLower() + current.Substring(1).Remove(" ");
		}
		public static string Trim(this string current,params string[] values){
			foreach(string value in values){
				current = current.TrimLeft(value);
				current = current.TrimRight(value);
			}
			return current;
		}
		public static string TrimRight(this string current,params string[] values){
			foreach(string value in values){current = current.TrimRight(value,true);}
			return current;
		}
		public static string TrimLeft(this string current,params string[] values){
			foreach(string value in values){current = current.TrimLeft(value,true);}
			return current;
		}
		public static string TrimRight(this string current,string value,bool ignoreCase){
			if(value.IsEmpty()){return current;}
			while(current.EndsWith(value,ignoreCase)){
				current = current.Substring(0,current.Length - value.Length);
			}
			return current;
		}
		public static string TrimLeft(this string current,string value,bool ignoreCase){
			if(value.IsEmpty()){return current;}
			while(current.StartsWith(value,ignoreCase)){
				current = current.Substring(value.Length);
			}
			return current;
		}
		public static bool Matches(this string current,string value,bool ignoreCase=false){
			if(ignoreCase){return current.ToLower() == value.ToLower();}
			return current == value;
		}
		public static bool MatchesAny(this string current,params string[] values){
			foreach(string value in values){
				if(current.Matches(value,true)){return true;}
			}
			return false;
		}
		public static string Replace(this string current,string search,string replace,bool ignoreCase){
			if(ignoreCase){
				search = Regex.Escape(search);
				replace = Regex.Escape(replace);
				string output = Regex.Replace(current,search,replace,RegexOptions.IgnoreCase | RegexOptions.Multiline);
				return Regex.Unescape(output);
			}
			return current.Replace(search,replace);
		}
		public static string ReplaceFirst(this string current,string search,string replace,bool ignoreCase=false){
			int position = current.IndexOf(search,ignoreCase);
			if(position == -1){
				return current;
			}
			return current.Substring(0,position) + replace + current.Substring(position + search.Length);
		}
		public static string ReplaceLast(this string current,string search,string replace,bool ignoreCase=false){
			int position = current.LastIndexOf(search,ignoreCase);
			if(position == -1){
				return current;
			}
			return current.Substring(0,position) + replace + current.Substring(position + search.Length);
		}
		public static int IndexOf(this string current,string value,int start,bool ignoreCase){
			if(ignoreCase){
				return current.IndexOf(value,start,StringComparison.OrdinalIgnoreCase);
			}
			return current.IndexOf(value,start);
		}
		public static int IndexOf(this string current,string value,bool ignoreCase){
			return current.IndexOf(value,0,ignoreCase);
		}
		public static int IndexOf(this string current,string value,int start,int occurrence,bool ignoreCase){
			while(occurrence > 0){
				start = current.IndexOf(value,start+1,ignoreCase)+1;
				occurrence -= 1;
			}
			return Math.Max(start-1,-1);
		}
		public static int LastIndexOf(this string current,string value,int start,bool ignoreCase){
			if(ignoreCase){
				return current.LastIndexOf(value,start,StringComparison.OrdinalIgnoreCase);
			}
			return current.LastIndexOf(value,start);
		}
		public static int LastIndexOf(this string current,string value,bool ignoreCase){
			return current.LastIndexOf(value,current.Length-1,ignoreCase);
		}
		public static int LastIndexOf(this string current,string value,int start,int occurrence,bool ignoreCase){
			while(occurrence > 0){
				start = current.LastIndexOf(value,start+1,ignoreCase)+1;
				occurrence -= 1;
			}
			return Math.Max(start-1,-1);
		}
		public static bool StartsWith(this string current,string value,bool ignoreCase){
			if(ignoreCase){
				return current.StartsWith(value,StringComparison.OrdinalIgnoreCase);
			}
			return current.StartsWith(value);
		}
		public static bool EndsWith(this string current,string value,bool ignoreCase){
			if(ignoreCase){
				return current.EndsWith(value,StringComparison.OrdinalIgnoreCase);
			}
			return current.EndsWith(value);
		}
		public static bool Has(this string current,string value,bool ignoreCase){return current.Contains(value,ignoreCase);}
		public static bool HasAny(this string current,params string[] values){return current.ContainsAny(values);}
		public static bool HasAll(this string current,params string[] values){return current.ContainsAll(values);}
		public static bool Contains(this string current,string value,bool ignoreCase){
			return current.IndexOf(value,ignoreCase) >= 0;
		}
		public static bool ContainsAny(this string current,params string[] values){
			foreach(string name in values){
				if(current.Contains(name,true)){return true;}
			}
			return false;
		}
		public static bool ContainsAll(this string current,params string[] values){
			foreach(string name in values){
				if(!current.Contains(name,true)){return false;}
			}
			return true;
		}
		public static string Remove(this string current,params string[] values){
			string result = current;
			foreach(string value in values){
				result = result.Replace(value,"",true);
			}
			return result;
		}
		public static string[] Split(this string current,string value){
			if(value.Length == 0 || !current.Contains(value)){return new string[1]{current};}
			return current.Split(new string[]{value},StringSplitOptions.None);
		}
		//============================
		// Checks
		//============================
		public static bool IsEmpty(this string text){
			return string.IsNullOrEmpty(text);
		}
		public static bool IsInt(this string text){
			short number;
			return short.TryParse(text,out number);
		}
		public static bool IsFloat(this string text){
			float number;
			return float.TryParse(text,out number);
		}
		public static bool IsNumber(this string current){
			double result;
			return double.TryParse(current,out result);
		}
		public static bool IsColorData(this string current){
			return current.ContainsAny(",","#");
		}
		public static bool IsEnum<T>(this string current,bool ignoreCase=true){
			try{
				var result = (T)Enum.Parse(typeof(T),current,ignoreCase);
				return !result.IsNull();
			}
			catch{return false;}
		}
		//============================
		// Path
		//============================
		public static string FixPath(this string current){
			return current.Replace("\\","/");
		}
		public static string GetDirectory(this string current){
			int last = current.FixPath().LastIndexOf('/');
			if(last < 0){
				if(current.Contains(".")){return "";}
				return current;
			}
			return current.Substring(0,last);
		}
		public static string GetAssetPath(this string current){
			if(current.Contains("Packages")){return "Packages" + current.FixPath().Split("/Packages")[1];}
			if(current.Contains("Assets")){return "Assets" + current.FixPath().Split("/Assets")[1];}
			return current;
		}
		public static string GetPathTerm(this string current){
			return current.FixPath().Split("/").LastOrDefault() ?? "";
		}
		public static string GetFileName(this string current){
			var term = current.FixPath().Split("/").LastOrDefault();
			if(term.Contains(".")){return term.ReplaceLast("."+current.GetFileExtension(),"");}
			return "";
		}
		public static string GetFileExtension(this string current){
			var term = current.FixPath().Split("/").LastOrDefault();
			if(term.Contains(".")){return term.Split(".").Last();}
			return term;
		}
		//============================
		// Extension
		//============================
		public static string AddLine(this string current,string value){
			return current + value + "\r\n";
		}
		public static string[] GetLines(this string current){
			return current.Remove("\r").Split("\n");
		}
		public static string Implode(this string current,string separator=" "){
			StringBuilder builder = new StringBuilder(current.Length * 2);
			foreach(char letter in current){
				builder.Append(letter);
				builder.Append(separator);
			}
			return builder.ToString();
		}
		public static string Cut(this string current,int startIndex=0,int endIndex=-1){
			return current.Substring(startIndex,endIndex - startIndex + 1);
		}
		public static string Cut(this string current,string start="",string end="",int offset=0,bool ignoreCase=true,int endCount=1){
			int startIndex = start == "" ? 0 : current.IndexOf(start,offset,ignoreCase);
			if(startIndex != -1){
				if(end == ""){return current.Substring(startIndex);}
				int endIndex = current.IndexOf(end,startIndex + 1,ignoreCase);
				if(endIndex == -1){return current.Substring(startIndex);}
				while(endCount > 1){
					endIndex = current.IndexOf(end,endIndex + 1,ignoreCase);
					--endCount;
				}
				int distance = endIndex - startIndex + end.Length;
				return current.Substring(startIndex,distance);
			}
			return "";
		}
		public static string Parse(this string current,string start="",string end="",int offset=0,bool ignoreCase=true,int endCount=1){
			string value = current.Cut(start,end,offset,ignoreCase,endCount);
			if(value.IsEmpty()){return "";}
			return value.Substring(start.Length).TrimRight(end).Trim();
		}
		public static string FindFirst(this string current,params string[] values){
			int index = -1;
			string first = "";
			foreach(string value in values){
				int currentIndex = current.IndexOf(value,true);
				if(currentIndex != -1 && (index == -1 || currentIndex < index)){
					index = currentIndex;
					first = value;
				}
			}
			return first;
		}
		public static string StripMarkup(this string current){
			return Regex.Replace(current,"<.*?>",string.Empty);
		}
		public static string Pack(this string current){
			return current.Remove("\r","\n","'","\"","{","}","[","]","(",")","\t"," ");
		}
		public static string Condense(this string current){
			while(current.ContainsAny("\t\t","  ")){
				current = current.Replace("\t\t","\t").Replace("  "," ");
			}
			return current;
		}
		public static string Truncate(this string current,int maxLength){
			return current.Length <= maxLength ? current : current.Substring(0,maxLength);
		}
		public static string TrySplit(this string current,string value,int index=0){
			return current.TrySplit(value[0],index);
		}
		public static string TrySplit(this string current,char value,int index=0){
			if(current.Contains(value.ToString())){
				return current.Split(value)[index];
			}
			return current;
		}
		public static string SetDefault(this string current,string value){
			if(current.IsEmpty()){return value;}
			return current;
		}
	}
}