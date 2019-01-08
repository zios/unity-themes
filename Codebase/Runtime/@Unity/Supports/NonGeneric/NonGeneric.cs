using System;
using System.Collections.Generic;
namespace Zios.Unity.Supports.NonGeneric{
	using Zios.Attributes.Supports;
	using Zios.Supports.NonGeneric;
	[Serializable] public class AttributeFloatList : List<AttributeFloat>{}
	[Serializable] public class AttributeIntList : List<AttributeInt>{}
	[Serializable] public class AttributeBoolList: List<AttributeBool>{}
	[Serializable] public class AttributeStringList: List<AttributeString>{}
	[Serializable] public class ListAttributeFloat : ListBox<AttributeFloat>{}
	[Serializable] public class ListAttributeInt : ListBox<AttributeInt>{}
	[Serializable] public class ListAttributeBool : ListBox<AttributeBool>{}
	[Serializable] public class ListAttributeString : ListBox<AttributeString>{}
}