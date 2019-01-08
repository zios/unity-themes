using UnityEngine;
namespace Zios.Unity.Components.Note{
	[AddComponentMenu("Zios/Component/General/Note")]
	public class Note : MonoBehaviour{
		[TextArea] public string description;
	}
}