using System;
using System.Collections.Generic;
namespace Zios{
	public class NeutralEntity{
		public int entityHashCode;
		public string name;
		public string entityType;
		public Dictionary<string, object> objectAttributes = new Dictionary<string,object>();
		public Dictionary<string, List<object>> collectionAttributes = new Dictionary<string,List<object>>();
		public override string ToString(){
			return entityType + "_" + entityHashCode;
		}
		public void AddAttribute(string key,object entity){
			if(!objectAttributes.ContainsKey(key)){
				objectAttributes.Add(key,entity);
			}
		}
		public void PopulateArray(string key,object entity){
			if(!collectionAttributes.ContainsKey(key)){
				List<object> entities = new List<object>();
				collectionAttributes.Add(key,entities);
			}
			collectionAttributes[key].Add(entity);
		}
	}
}