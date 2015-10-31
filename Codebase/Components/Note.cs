using UnityEngine;
namespace Zios{
	[AddComponentMenu("Zios/Component/General/Note")]
	public class Note : MonoBehaviour{
		[TextArea] public string description;
	}
}