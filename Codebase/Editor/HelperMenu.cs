using UnityEditor;
using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
#pragma warning disable 618
public static class HelperMenu {
	static public bool IsSprite(GameObject instance){
		MeshFilter filter = instance.GetComponent<MeshFilter>();
		Sprite sprite = SpriteManager.GetSprite(instance.name);
		bool inGroup = instance.transform.parent != null && instance.transform.parent.name.Contains("SpriteGroup");
		return (inGroup && sprite != null) || (filter != null && filter.sharedMesh.name == "plane");
	}
    [MenuItem ("Zios/Process/Remove Invisible Sprites")]
	static void RemoveObjects(){
		GameObject[] objects = (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject));
		int removed = 0;
		Undo.RecordObjects(objects,"Remove Invisible Sprites");
		foreach(GameObject gameObject in objects){
			PrefabType type = PrefabUtility.GetPrefabType(gameObject);
			if(type == PrefabType.Prefab || type == PrefabType.ModelPrefab || gameObject == null){continue;}
			Vector3 position = gameObject.transform.localPosition;
			bool isRoot = gameObject.transform.parent == null;
			string pureName = gameObject.name.Contains("@") ? gameObject.name.Split('@')[0] : gameObject.name;
			string parentName = !isRoot ? gameObject.transform.parent.name : "";
			bool nested = !isRoot && gameObject.transform.parent.parent == null;
			nested = nested || (parentName.Contains(pureName) && !parentName.Contains("SpriteGroup"));
			if(IsSprite(gameObject) && (isRoot || !nested) && position.x == 0 && position.z == 0){
				Debug.Log(gameObject.name + " has been removed.");
				GameObject.DestroyImmediate(gameObject);
				++removed;
			}
		}
		Debug.Log(removed + " null game objects removed.");
	}
    [MenuItem ("Zios/Process/Animation/Stepped Curves")]
    static void SteppedCurves(){
		HelperMenu.SplitAnimations(Mathf.Infinity);
	}
    [MenuItem ("Zios/Process/Animation/Separate Animations")]
    static void SeparateAnimations(){
		HelperMenu.SplitAnimations(-1);
	}
    static void SplitAnimations(float tangents=-1){
		string savePath = "Assets/Characters/Shared/"; 
		foreach(Transform selection in Selection.transforms){
			Animation animation = (Animation)selection.GetComponent("Animation");
			if(animation != null){
				AnimationClip[] clips = AnimationUtility.GetAnimationClips(selection.gameObject);
				AnimationClip[] newClips = new AnimationClip[clips.Length];
				Debug.Log("Converting " + clips.Length + " animations...");
				int clipIndex = 0;
				foreach(AnimationClip clip in clips){
					if(clip == null){
						++clipIndex;
						continue;
					}
					AnimationClip newClip = new AnimationClip();
					newClip.wrapMode = clip.wrapMode;
					string clipPath = clip.name + ".anim";
					AnimationClipCurveData[] curves = AnimationUtility.GetAllCurves(clip);
					foreach(AnimationClipCurveData data in curves){
						List<Keyframe> newKeys = new List<Keyframe>();
						foreach(Keyframe key in data.curve.keys){
							Keyframe newKey = new Keyframe(key.time,key.value);
							if(tangents!=-1){
								newKey.inTangent = Mathf.Infinity;
								newKey.outTangent = Mathf.Infinity;
							}
							newKeys.Add(newKey);
						}
						newClip.SetCurve(data.path,data.type,data.propertyName,new AnimationCurve(newKeys.ToArray()));
					}
					Debug.Log("[" + clipIndex + "] " + clip.name + " processed.");
					Directory.CreateDirectory(savePath+selection.gameObject.name);
					AssetDatabase.CreateAsset(newClip,savePath+selection.gameObject.name+"/"+clipPath);
					//AssetDatabase.SaveAssets();
					//AssetDatabase.Refresh();
					newClips[clipIndex] = newClip;
					++clipIndex;
				}
				AnimationUtility.SetAnimationClips(animation,newClips);
			}
			else{
				Debug.Log("No animation component found on object -- " + selection.name);
			}
		}
    }
    [MenuItem ("Zios/Process/Snap Positions")]
    static void SnapPositions(){
		GameObject[] all = (GameObject[])GameObject.FindObjectsOfType(typeof(GameObject));
		for(int index=0;index < all.Length;++index){
			Transform current = all[index].transform;
			Vector3 position = current.localPosition;
			position.x = Mathf.Round(position.x);
			position.y = Mathf.Round(position.y);
			position.z = Mathf.Round(position.z);
			current.localPosition = position;
		}
    }
}