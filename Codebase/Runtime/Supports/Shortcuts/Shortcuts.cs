namespace Zios.Shortcuts{
	public delegate void Method();
	public delegate void MethodStep(object collection,int key);
	public delegate void MethodObject(object value);
	public delegate void MethodInt(int value);
	public delegate void MethodFloat(float value);
	public delegate void MethodString(string value);
	public delegate void MethodBool(bool value);
	public delegate void MethodObjects(object[] values);
	public delegate object MethodReturn();
	public delegate object MethodObjectReturn(object value);
	public delegate object MethodIntReturn(int value);
	public delegate object MethodFloatReturn(float value);
	public delegate object MethodStringReturn(string value);
	public delegate object MethodBoolReturn(bool value);
}