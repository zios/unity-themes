using UnityEngine;
namespace Zios{
	public static class ComponentExtension{
		public static void UpdateSerialized(this Component current){
			#if UNITY_EDITOR
			Utility.UpdateSerialized(current);
			#endif
		}
		public static GameObject GetPrefabRoot(this Component current){
			return current.gameObject.GetPrefabRoot();
		}
		public static GameObject GetParent(this Component current){
			return current.gameObject.GetParent();
		}
		public static string GetPath(this Component current,bool includeSelf=true){
			if(current.IsNull() || current.gameObject.IsNull()){return "Null";}
			string path = current.gameObject.GetPath();
			if(includeSelf){path += current.GetAlias();}
			return path;
		}
		public static bool IsPrefab(this Component current){
			return current.gameObject.IsPrefab();
		}
		public static bool IsEnabled(this Component current){
			bool enabled = !current.IsNull() && current.gameObject.activeInHierarchy;
			if(current is MonoBehaviour){enabled = enabled && current.As<MonoBehaviour>().enabled;}
			return enabled;
		}
		public static void Move(this Component current,int amount){
			Utility.DisconnectPrefabInstance(current);
			while(amount != 0){
				if(amount > 0){
					Utility.MoveComponentDown(current);
					amount -= 1;
				}
				if(amount < 0){
					Utility.MoveComponentUp(current);
					amount += 1;
				}
			}
		}
		public static void MoveUp(this Component current){
			Component[] components = current.GetComponents<Component>();
			int position = components.IndexOf(current);
			int amount = 1;
			if(position != 0){
				while(components[position-1].hideFlags.Contains(HideFlags.HideInInspector)){
					position -= 1;
					amount += 1;
				}
			}
			current.Move(-amount);
		}
		public static void MoveDown(this Component current){
			Component[] components = current.GetComponents<Component>();
			int position = components.IndexOf(current);
			int amount = 1;
			if(position < components.Length-1){
				while(components[position+1].hideFlags.Contains(HideFlags.HideInInspector)){
					position += 1;
					amount += 1;
				}
			}
			current.Move(amount);
		}
		public static void MoveToTop(this Component current){
			Utility.DisconnectPrefabInstance(current);
			Component[] components = current.GetComponents<Component>();
			int position = components.IndexOf(current);
			current.Move(-position);
		}
		public static void MoveToBottom(this Component current){
			Utility.DisconnectPrefabInstance(current);
			Component[] components = current.GetComponents<Component>();
			int position = components.IndexOf(current);
			current.Move(components.Length-position);
		}
		//====================
		// Interface
		//====================
		public static Component[] GetComponentsByInterface<T>(this Component current) where T : Component{
			return current.gameObject.GetComponentsByInterface<T>();
		}
		public static T GetComponent<T>(this Component current,bool includeInactive) where T : Component{
			return current.gameObject.GetComponent<T>(includeInactive);
		}
		public static T[] GetComponents<T>(this Component current,bool includeInactive) where T : Component{
			return current.gameObject.GetComponents<T>(includeInactive);
		}
		public static T GetComponentInParent<T>(this Component current,bool includeInactive) where T : Component{
			return current.gameObject.GetComponentInParent<T>(includeInactive);
		}
		public static T GetComponentInChildren<T>(this Component current,bool includeInactive) where T : Component{
			return current.gameObject.GetComponentInChildren<T>(includeInactive);
		}
	}
}