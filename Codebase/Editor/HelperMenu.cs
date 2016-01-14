using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
#pragma warning disable 618
namespace Zios{
	public static class HelperMenu{
		public static bool IsSprite(GameObject instance){
			MeshFilter filter = instance.GetComponent<MeshFilter>();
			Sprite sprite = SpriteManager.GetSprite(instance.name);
			bool inGroup = instance.transform.parent != null && instance.transform.parent.name.Contains("SpriteGroup");
			return (inGroup && sprite != null) || (filter != null && filter.sharedMesh.name == "plane");
		}
		[MenuItem ("Zios/Process/Sprites/Remove Invisible")]
		public static void RemoveInvisibleSprites(){
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
					Debug.Log("[HelperMenu]" + gameObject.name + " has been removed.");
					GameObject.DestroyImmediate(gameObject);
					++removed;
				}
			}
			Debug.Log("[HelperMenu]" + removed + " null game objects removed.");
		}
		[MenuItem ("Zios/Process/Animation/Stepped Curves")]
		public static void SteppedCurves(){
			HelperMenu.SplitAnimations(Mathf.Infinity);
		}
		[MenuItem ("Zios/Process/Animation/Separate Animations")]
		public static void SeparateAnimations(){
			HelperMenu.SplitAnimations();
		}
		public static void SplitAnimations(float forceTangent=-1){
			foreach(Transform selection in Selection.transforms){
				Animation animation = (Animation)selection.GetComponent("Animation");
				if(animation != null){
					AnimationClip[] clips = AnimationUtility.GetAnimationClips(selection.gameObject);
					AnimationClip[] newClips = new AnimationClip[clips.Length];
					Debug.Log("[HelperMenu] Converting " + clips.Length + " animations...");
					int clipIndex = 0;
					foreach(AnimationClip clip in clips){
						if(clip == null){
							++clipIndex;
							continue;
						}
						string clipPath = clip.name + ".anim";
						string originalPath = AssetDatabase.GetAssetPath(clip);
						string savePath = Path.GetDirectoryName(originalPath) + "/" + clipPath;
						AnimationClip newClip = new AnimationClip();
						if(originalPath.Contains(".anim")){
							Debug.Log("[HelperMenu] [" + clipIndex + "] " + clip.name + " skipped.  Already separate .anim file.");
							newClip = clip;
						}
						else{
							newClip.wrapMode = clip.wrapMode;
							AnimationClipCurveData[] curves = AnimationUtility.GetAllCurves(clip);
							foreach(AnimationClipCurveData data in curves){
								List<Keyframe> newKeys = new List<Keyframe>();
								foreach(Keyframe key in data.curve.keys){
									Keyframe newKey = new Keyframe(key.time,key.value);
									newKey.inTangent = forceTangent != -1 ? forceTangent : key.inTangent;
									newKey.outTangent = forceTangent != -1 ? forceTangent : key.outTangent;
									newKeys.Add(newKey);
								}
								newClip.SetCurve(data.path,data.type,data.propertyName,new AnimationCurve(newKeys.ToArray()));
							}
							Debug.Log("[HelperMenu] [" + clipIndex + "] " + clip.name + " processed -- " + savePath);
							AssetDatabase.CreateAsset(newClip,savePath);
						}
						newClips[clipIndex] = newClip;
						++clipIndex;
					}
					AnimationUtility.SetAnimationClips(animation,newClips);
				}
				else{
					Debug.Log("[HelperMenu] No animation component found on object -- " + selection.name);
				}
			}
		}
		[MenuItem ("Zios/Process/Sprites/Snap Positions")]
		public static void SnapPositions(){
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
		[MenuItem ("Zios/Process/Action/Reset Manual Input")]
		public static void FixManualIntensity(){
			foreach(var script in Locate.GetSceneComponents<InputHeld>()){
				Utility.RecordObject(script,"Reset Manual Input");
				script.manual.Set(Mathf.Infinity);
				Utility.SetDirty(script);
			}
			foreach(var script in Locate.GetSceneComponents<InputPressed>()){
				Utility.RecordObject(script,"Reset Manual Input");
				script.manual.Set(Mathf.Infinity);
				Utility.SetDirty(script);
			}
		}
	}
}