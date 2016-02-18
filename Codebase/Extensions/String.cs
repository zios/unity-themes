using System;
using System.Linq;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
namespace Zios{
	public static class StringExtension{
		//============================
		// Conversion
		//============================
		public static GUIContent ToContent(this string current){return new GUIContent(current);}
		public static string ToLetterSequence(this string current){
			char lastDigit = current[current.Length-1];
			if(current.Length > 1 && current[current.Length-2] == ' ' && char.IsLetter(lastDigit)){
				char nextLetter = (char)(char.ToUpper(lastDigit)+1);
				return current.TrimEnd(lastDigit) + nextLetter;
			}
			return current + " B";
		}
		public static string ToMD5(this string current){
			byte[] bytes = Encoding.UTF8.GetBytes(current);
			byte[] hash = MD5.Create().ComputeHash(bytes);
			return BitConverter.ToString(hash).Replace("-","");
		}
		public static Vector3 ToVector3(this string current){
			if(!current.Contains(",")){return Vector3.zero;}
			string[] split = current.Remove("(",")"," ").Split(",");
			float x = split[0].ToFloat();
			float y = split[1].ToFloat();
			float z = split[2].ToFloat();
			return new Vector3(x,y,z);
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
		public static int ToInt(this string current){
			if(current.IsEmpty()){return 0;}
			return Convert.ToInt32(current);
		}
		public static float ToFloat(this string current){
			if(current.IsEmpty()){return 0;}
			return Convert.ToSingle(current);
		}
		public static bool ToBool(this string current){
			if(current.IsEmpty()){return false;}
			string lower = current.ToLower();
			return lower != "false" && lower != "f" && lower != "0";
		}
		public static byte ToByte(this string current){return (byte)current[0];}
		public static byte[] ToBytes(this string current){return Encoding.ASCII.GetBytes(current);}
		public static Color ToColor(this string current){
			if(!current.Contains(",")){return Color.white;}
			bool commaSplit = current.Contains(",");
			bool spaceSplit = current.Contains(" ");
			if(commaSplit || spaceSplit){
				string[] part = current.Split(commaSplit ? ',' : ' ');
				float r = Convert.ToSingle(part[0]) / 255.0f;
				float g = Convert.ToSingle(part[1]) / 255.0f;
				float b = Convert.ToSingle(part[2]) / 255.0f;
				float a = part.Length > 3 ? Convert.ToSingle(part[3]) / 255.0f : 1;
				return new Color(r,g,b,a);
			}
			else if(current.Length == 8 || current.Length == 6 || current.Length == 3){
				if(current.Length == 3){
					current += current;
				}
				float r = (float)Convert.ToInt32(current.Substring(0,2),16) / 255.0f;
				float g = (float)Convert.ToInt32(current.Substring(2,2),16) / 255.0f;
				float b = (float)Convert.ToInt32(current.Substring(4,2),16) / 255.0f;
				float a = current.Length == 8 ? (float)Convert.ToInt32(current.Substring(6,2),16) / 255.0f : 1;
				return new Color(r,g,b,a);
			}
			else{
				Debug.LogError("[StringExtension] Color strings can only be converted from Hexidecimal or comma/space separated Decimal.");
				return new Color(255,0,255);
			}
		}
		public static string Capitalize(this string current){
			return current[0].ToString().ToUpper() + current.Substring(1);
		}
		//============================
		// Standard
		//============================
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
		public static int IndexOf(this string current,string value,int start,bool ignoreCase){
			if(ignoreCase){
				return current.IndexOf(value,start,StringComparison.OrdinalIgnoreCase);
			}
			return current.IndexOf(value,start);
		}
		public static int IndexOf(this string current,string value,bool ignoreCase){
			if(ignoreCase){
				return current.IndexOf(value,StringComparison.OrdinalIgnoreCase);
			}
			return current.IndexOf(value);
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
			if(value.Length == 0){return new string[1]{current};}
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
		//============================
		// Path
		//============================
		public static string GetDirectory(this string current){
			int last = current.LastIndexOf('/');
			if(last < 0){
				if(current.Contains(".")){return "";}
				return current;
			}
			return current.Substring(0,last);
		}
		public static string GetFileName(this string current){
			var term = current.Split("/").Last();
			if(term.Contains(".")){return term.Split(".")[0];}
			return term;
		}
		public static string GetExtension(this string current){
			var term = current.Split("/").Last();
			if(term.Contains(".")){return term.Split(".")[1];}
			return term;
		}
		//============================
		// Extension
		//============================
		public static string[] GetLines(this string current){
			return current.Split("\n");
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
		public static string Cut(this string current,string start="",string end="",int offset=0,bool ignoreCase=true,int repeatEnd=1){
			int startIndex = start == "" ? 0 : current.IndexOf(start,offset,ignoreCase);
			if(startIndex != -1){
				if(end == ""){return current.Substring(startIndex);}
				int endIndex = current.IndexOf(end,startIndex + 1,ignoreCase);
				if(endIndex != -1){
					while(repeatEnd > 1){
						if(endIndex == -1){return "";}
						endIndex = current.IndexOf(end,endIndex + 1,ignoreCase);
						--repeatEnd;
					}
					int distance = endIndex - startIndex + end.Length;
					return current.Substring(startIndex,distance);
				}
				return "";
			}
			return "";
		}
		public static string Parse(this string current,string start="",string end="",int offset=0,bool ignoreCase=true,int repeatEnd=1){
			string value = current.Cut(start,end,offset,ignoreCase,repeatEnd);
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