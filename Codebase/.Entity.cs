using UnityEngine;
using System.Collections;
public class Entity : MonoBehaviour{
	Dictionary<string,string> attributesString = new Dictionary<string,string>();
	Dictionary<string,float> attributesFloat = new Dictionary<string,float>();
	Dictionary<string,int> attributesInt = new Dictionary<string,int>();
	Dictionary<string,bool> attributesBool = new Dictionary<string,bool>();
	List<Action> actions = new List<Action>();
	List<Visual> visuals = new List<Visual>();
	List<Audial> audials = new List<Audial>();
}
public class Visual{
	Renderer renderer;
	MeshFilter mesh;
	List<Material> materials = new List<Material>();
	List<string> animations = new List<string>();
}
public class Audial{
	Renderer renderer;
	MeshFilter mesh;
	List<Material> materials = new List<Material>();
	List<string> animations = new List<string>();
}