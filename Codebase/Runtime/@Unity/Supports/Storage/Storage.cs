namespace Zios.Unity.Supports.Storage{
	using Zios.Extensions.Convert;
	using Zios.Unity.Pref;
	public enum StoreMethod{Registry,File}
	public class Storage<Type>{
		public static StoreMethod type;
		public string name;
		public Type defaultValue;
		public Type Get(){return PlayerPref.Get<Type>(this.name,this.defaultValue);}
		public Other Get<Other>(){return this.Get().As<Other>();}
		public void Set(Type value){PlayerPref.Set<Type>(this.name,value);}
		public Storage(string name,Type value=default(Type)){
			this.name = name;
			this.defaultValue = value;
		}
		public static implicit operator Type(Storage<Type> current){return current.Get();}
	}
}