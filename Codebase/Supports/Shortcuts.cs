using UnityEngine;
using System;
using Zios;
using System.Collections.Generic;
// -------------
// Delegates
// -------------
public delegate bool KeyShortcut(KeyCode code);
public delegate void Method();
public delegate void MethodObject(object value);
public delegate void MethodInt(int value);
public delegate void MethodFloat(float value);
public delegate void MethodString(string value);
public delegate void MethodBool(bool value);
public delegate void MethodVector2(Vector2 value);
public delegate void MethodVector3(Vector3 value);
public delegate void MethodFull(object[] values);
public delegate object MethodReturn();
public delegate object MethodObjectReturn(object value);
public delegate object MethodIntReturn(int value);
public delegate object MethodFloatReturn(float value);
public delegate object MethodStringReturn(string value);
public delegate object MethodBoolReturn(bool value);
public delegate object MethodVector2Return(Vector2 value);
public delegate object MethodVector3Return(Vector3 value);
// -------------
// Non-Generic Lists
// -------------
[Serializable] public class FloatList : List<float>{}
[Serializable] public class IntList : List<int>{}
[Serializable] public class BoolList: List<bool>{}
[Serializable] public class StringList: List<string>{}
[Serializable] public class Vector3List: List<Vector3>{}
[Serializable] public class AttributeFloatList : List<AttributeFloat>{}
[Serializable] public class AttributeIntList : List<AttributeInt>{}
[Serializable] public class AttributeBoolList: List<AttributeBool>{}
[Serializable] public class AttributeStringList: List<AttributeString>{}
[Serializable] public class AttributeVector3List: List<AttributeVector3>{}
[Serializable] public class ListFloat : List<float>{}
[Serializable] public class ListInt : List<int>{}
[Serializable] public class ListBool : List<bool>{}
[Serializable] public class ListString : List<string>{}
[Serializable] public class ListVector3 : List<Vector3>{}
[Serializable] public class ListAttributeFloat : List<AttributeFloat>{}
[Serializable] public class ListAttributeInt : List<AttributeInt>{}
[Serializable] public class ListAttributeBool : List<AttributeBool>{}
[Serializable] public class ListAttributeString : List<AttributeString>{}
[Serializable] public class ListAttributeVector3 : List<AttributeVector3>{}