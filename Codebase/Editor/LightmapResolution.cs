using UnityEditor;

public class LightmapResolution512 : EditorWindow
{
    [MenuItem("Zios/Process/Generate Lightmap/512x512")]
    static void Init()
    {
        LightmapEditorSettings.maxAtlasHeight = 512;
        LightmapEditorSettings.maxAtlasWidth = 512;
    }
}

public class LightmapResolution1024 : EditorWindow
{
    [MenuItem("Zios/Process/Generate Lightmap/1024x1024")]
    static void Init()
    {
        LightmapEditorSettings.maxAtlasHeight = 1024;
        LightmapEditorSettings.maxAtlasWidth = 1024;
    }
}

public class LightmapResolution2048 : EditorWindow
{
    [MenuItem("Zios/Process/Generate Lightmap/2048x2048")]
    static void Init()
    {
        LightmapEditorSettings.maxAtlasHeight = 2048;
        LightmapEditorSettings.maxAtlasWidth = 2048;
    }
}

public class LightmapResolution4096 : EditorWindow
{
    [MenuItem("Zios/Process/Generate Lightmap/4096x4096")]
    static void Init()
    {
        LightmapEditorSettings.maxAtlasHeight = 4096;
        LightmapEditorSettings.maxAtlasWidth = 4096;
    }
}
