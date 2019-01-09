using System.Collections.Generic;
namespace Zios.Supports.Hierarchy{
	public class Hierarchy<Key,Value> : Dictionary<Key,Value>{public Hierarchy():base(){}}
	public class Hierarchy<KeyA,KeyB,Value> : Dictionary<KeyA,Dictionary<KeyB,Value>>{public Hierarchy():base(){}}
	public class Hierarchy<KeyA,KeyB,KeyC,Value> : Dictionary<KeyA,Dictionary<KeyB,Dictionary<KeyC,Value>>>{public Hierarchy():base(){}}
	public class Hierarchy<KeyA,KeyB,KeyC,KeyD,Value> : Dictionary<KeyA,Dictionary<KeyB,Dictionary<KeyC,Dictionary<KeyD,Value>>>>{public Hierarchy():base(){}}
	public class Hierarchy<KeyA,KeyB,KeyC,KeyD,KeyE,Value> : Dictionary<KeyA,Dictionary<KeyB,Dictionary<KeyC,Dictionary<KeyD,Dictionary<KeyE,Value>>>>>{public Hierarchy():base(){}}
	#if ENABLE_MONO
	public class Hierarchy<KeyA,KeyB,KeyC,KeyD,KeyE,KeyF,Value> : Dictionary<KeyA,Dictionary<KeyB,Dictionary<KeyC,Dictionary<KeyD,Dictionary<KeyE,Dictionary<KeyF,Value>>>>>>{public Hierarchy():base(){}}
	public class Hierarchy<KeyA,KeyB,KeyC,KeyD,KeyE,KeyF,KeyG,Value> : Dictionary<KeyA,Dictionary<KeyB,Dictionary<KeyC,Dictionary<KeyD,Dictionary<KeyE,Dictionary<KeyF,Dictionary<KeyG,Value>>>>>>>{public Hierarchy():base(){}}
	public class Hierarchy<KeyA,KeyB,KeyC,KeyD,KeyE,KeyF,KeyG,KeyH,Value> : Dictionary<KeyA,Dictionary<KeyB,Dictionary<KeyC,Dictionary<KeyD,Dictionary<KeyE,Dictionary<KeyF,Dictionary<KeyG,Dictionary<KeyH,Value>>>>>>>>{public Hierarchy():base(){}}
	#endif
}