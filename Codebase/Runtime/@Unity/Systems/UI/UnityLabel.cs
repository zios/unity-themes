using UnityEngine;
namespace Zios.Unity.EditorUI{
	public class UnityLabel{
		public GUIContent value = new GUIContent("");
		public UnityLabel(string value){this.value = new GUIContent(value);}
		public UnityLabel(GUIContent value){this.value = value;}
		public override string ToString(){return this.value.text;}
		public GUIContent ToContent(){return this.value;}
		//public static implicit operator string(UnityLabel current){return current.value.text;}
		public static implicit operator GUIContent(UnityLabel current){
			if(current == null){return GUIContent.none;}
			return current.value;
		}
		public static implicit operator UnityLabel(GUIContent current){return new UnityLabel(current);}
		public static implicit operator UnityLabel(string current){return new UnityLabel(current);}
	}
	public static class LabelExtensions{
		public static UnityLabel ToLabel(this GUIContent current){
			return new UnityLabel(current);
		}
		public static UnityLabel ToLabel(this string current){
			return new UnityLabel(current);
		}
	}
}