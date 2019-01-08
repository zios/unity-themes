using UnityEngine;
namespace Zios.Unity.Shortcuts{
	public delegate bool KeyShortcut(KeyCode code);
	public delegate void MethodVector2(Vector2 value);
	public delegate void MethodVector3(Vector3 value);
	public delegate object MethodVector2Return(Vector2 value);
	public delegate object MethodVector3Return(Vector3 value);
}