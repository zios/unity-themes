namespace Zios{
	public enum StoreMethod{Registry,File}
	public class Storage<Type>{
		public static StoreMethod type;
		public string name;
		public Type defaultValue;
		public Type Get(){return Utility.GetPlayerPref<Type>(this.name,this.defaultValue);}
		public Other Get<Other>(){return this.Get().As<Other>();}
		public void Set(Type value){Utility.SetPlayerPref<Type>(this.name,value);}
		public Storage(string name,Type value=default(Type)){
			this.name = name;
			this.defaultValue = value;
		}
		public static implicit operator Type(Storage<Type> current){return current.Get();}
	}
}