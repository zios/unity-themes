#pragma warning disable CS0618
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace Zios.Unity.Editor.Menus{
	using Zios.Extensions;
	using Zios.File;
	using Zios.Unity.ProxyEditor;
	using Zios.Unity.Log;
	using Zios.Unity.SpriteManager;
	public static class HelperMenu{
		public static bool IsSprite(GameObject instance){
			MeshFilter filter = instance.GetComponent<MeshFilter>();
			Sprite sprite = SpriteManager.GetSprite(instance.name);
			bool inGroup = instance.transform.parent != null && instance.transform.parent.name.Contains("SpriteGroup");
			return (inGroup && sprite != null) || (filter != null && filter.sharedMesh.name == "plane");
		}
		[MenuItem ("Zios/Sprites/Remove Invisible")]
		public static void RemoveInvisibleSprites(){
			var objects = (GameObject[])Resources.FindObjectsOfTypeAll(typeof(GameObject));
			int removed = 0;
			ProxyEditor.RecordObjects(objects,"Remove Invisible Sprites");
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
					Log.Show("[HelperMenu]" + gameObject.name + " has been removed.");
					GameObject.DestroyImmediate(gameObject);
					++removed;
				}
			}
			Log.Show("[HelperMenu]" + removed + " null game objects removed.");
		}
		[MenuItem ("Zios/Animation/Stepped Curves")]
		public static void SteppedCurves(){
			HelperMenu.SplitAnimations(Mathf.Infinity);
		}
		[MenuItem ("Zios/Animation/Separate Animations")]
		public static void SeparateAnimations(){
			HelperMenu.SplitAnimations();
		}
		public static void SplitAnimations(float forceTangent=-1){
			foreach(Transform selection in Selection.transforms){
				var animation = (Animation)selection.GetComponent("Animation");
				if(animation != null){
					AnimationClip[] clips = AnimationUtility.GetAnimationClips(selection.gameObject);
					AnimationClip[] newClips = new AnimationClip[clips.Length];
					Log.Show("[HelperMenu] Converting " + clips.Length + " animations...");
					int clipIndex = 0;
					foreach(AnimationClip clip in clips){
						if(clip == null){
							++clipIndex;
							continue;
						}
						string clipPath = clip.name + ".anim";
						string originalPath = File.GetPath(clip);
						string savePath = originalPath.GetDirectory() + "/" + clipPath;
						var newClip = new AnimationClip();
						if(originalPath.Contains(".anim")){
							Log.Show("[HelperMenu] [" + clipIndex + "] " + clip.name + " skipped.  Already separate .anim file.");
							newClip = clip;
						}
						else{
							newClip.wrapMode = clip.wrapMode;
							AnimationClipCurveData[] curves = AnimationUtility.GetAllCurves(clip);
							foreach(AnimationClipCurveData data in curves){
								var newKeys = new List<Keyframe>();
								foreach(Keyframe key in data.curve.keys){
									var newKey = new Keyframe(key.time,key.value);
									newKey.inTangent = forceTangent != -1 ? forceTangent : key.inTangent;
									newKey.outTangent = forceTangent != -1 ? forceTangent : key.outTangent;
									newKeys.Add(newKey);
								}
								newClip.SetCurve(data.path,data.type,data.propertyName,new AnimationCurve(newKeys.ToArray()));
							}
							Log.Show("[HelperMenu] [" + clipIndex + "] " + clip.name + " processed -- " + savePath);
							ProxyEditor.CreateAsset(newClip,savePath);
						}
						newClips[clipIndex] = newClip;
						++clipIndex;
					}
					AnimationUtility.SetAnimationClips(animation,newClips);
				}
				else{
					Log.Show("[HelperMenu] No animation component found on object -- " + selection.name);
				}
			}
		}
		[MenuItem ("Zios/Sprites/Snap Positions")]
		public static void SnapPositions(){
			var all = (GameObject[])GameObject.FindObjectsOfType(typeof(GameObject));
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
}