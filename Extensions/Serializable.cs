namespace Zios{
	using System.IO;
	using System.Runtime.Serialization.Formatters.Binary;
	public static class SerializableExtension{
		public static T DeepCopy<T>(this T target){
			using(var stream = new MemoryStream()){
				var formatter = new BinaryFormatter();
				formatter.Serialize(stream,target);
				stream.Position = 0;
				return (T)formatter.Deserialize(stream);
			}
		}
	}
}