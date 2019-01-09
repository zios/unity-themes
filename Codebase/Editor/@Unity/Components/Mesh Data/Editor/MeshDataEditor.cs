using UnityEditor;
namespace Zios.Unity.Editor.Components.MeshData{
	using Zios.Extensions;
	using Zios.Extensions.Convert;
	using Zios.Unity.Components.MeshData;
	using Zios.Unity.EditorUI;
	using Zios.Unity.Extensions;
	using Zios.Unity.Supports.MeshInfo;
	using Zios.Unity.Supports.MeshWrap;
	using Editor = UnityEditor.Editor;
	//asm Zios.Supports.Worker;
	[CustomEditor(typeof(MeshData))]
	public class MeshDataEditor : Editor{
		public override void OnInspectorGUI(){
			EditorUI.Reset();
			this.serializedObject.Update();
			var target = this.target.As<MeshData>();
			var mesh = target.GetMesh();
			if(!mesh.IsNull()){
				var wrap = MeshWrap.Get(mesh);
				this.Display<Vertex>(wrap,"Vertexes",target.vertexes.Count);
				this.Display<Triangle>(wrap,"Triangles",target.triangles.Count);
				this.Display<Edge>(wrap,"Edges",target.edges.Count);
			}
			this.serializedObject.ApplyModifiedProperties();
		}
		public void Display<Type>(MeshWrap mesh,string group,int amount){
			var builder = MeshBuild.Get<Type>(mesh);
			if(!builder.IsNull() && !builder.worker.IsNull()){
				var worker = builder.worker;
				var title = "["+mesh.name+".mesh"+"] Building "+group;
				var message = worker.progress + " / " + worker.size;
				title.DrawProgressBar(message,worker.progress/worker.size.ToFloat(),true);
			}
			else{
				EditorUI.ClearProgressBar();
				amount.ToString().Draw(group);
			}
		}
	}
}