using Zios;
using UnityEngine;
[AddComponentMenu("")]
public class AttributeExposer : MonoBehaviour{
	public virtual void Reset(){this.Awake();}
	public virtual void OnApplicationQuit(){this.Awake();}
	public virtual void Awake(){}
}