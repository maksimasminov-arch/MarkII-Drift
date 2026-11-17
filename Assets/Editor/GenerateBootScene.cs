#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GenerateBootScene
{
    [MenuItem("Mark II Drift/Create Boot Scene")]
    public static void Create()
    {
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        var go=new GameObject("Prototype Bootstrap");
        go.AddComponent<PrototypeBootstrap>();
        if(!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets","Scenes");
        EditorSceneManager.SaveScene(scene,"Assets/Scenes/Boot.unity");
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Scenes/Boot.unity",true)};
        Debug.Log("Boot scene created. Press Play to test.");
    }
}
#endif
