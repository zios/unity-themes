using System.Text;
namespace Zios{
    public static class StringBuilderExtension{
	    public static void Clear(this StringBuilder current){
		    current.Length = 0;
		    current.Capacity = 0;
	    }
    }
}