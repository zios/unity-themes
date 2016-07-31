using UnityEngine;
namespace Zios.Utilities{
	[AddComponentMenu("Zios/Component/General/Note")]
	public class Note : MonoBehaviour{
		[TextArea] public string description;
	}
}