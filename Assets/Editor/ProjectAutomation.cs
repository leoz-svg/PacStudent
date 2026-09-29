using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Local, fixed-action development helper. No network listener or arbitrary code execution.
[InitializeOnLoad]
public static class ProjectAutomation
{
    static string Work => Path.GetFullPath(Path.Combine(Application.dataPath,"../../../work"));
    static ProjectAutomation() { EditorApplication.update += Tick; }
    static void Tick()
    {
        if(EditorApplication.isCompiling || EditorApplication.isUpdating) return;
        string refresh=Path.Combine(Work,"unity-refresh.txt");
        if(File.Exists(refresh)) { File.Delete(refresh);AssetDatabase.Refresh();return; }
        string file=Path.Combine(Work,"unity-command.txt");
        if(!File.Exists(file)) return;
        string command=File.ReadAllText(file).Trim();File.Delete(file);
        try
        {
            switch(command)
            {
                case "setup": CompleteProject.Run(); break;
                case "finish": CompleteProject.Finish(); break;
                case "layout": CompleteProject.FixLayout(); break;
                case "wide": GameViewAudit.SetSize(1280,720); break;
                case "classic": GameViewAudit.SetSize(1024,768); break;
                case "build": CompleteProject.Build(); break;
                case "play": EditorSceneManager.OpenScene("Assets/Scenes/StartScene.unity"); EditorApplication.isPlaying=true;break;
                case "stop": EditorApplication.isPlaying=false;break;
                case "smoke": SessionState.SetBool("PacStudent.Smoke",true);EditorSceneManager.OpenScene("Assets/Scenes/StartScene.unity");EditorApplication.isPlaying=true;break;
                default: throw new InvalidOperationException("Unknown local action: "+command);
            }
            File.WriteAllText(Path.Combine(Work,"unity-result.txt"),DateTime.Now+" SUCCESS "+command);
        }
        catch(Exception e) { File.WriteAllText(Path.Combine(Work,"unity-result.txt"),e.ToString());Debug.LogException(e); }
    }
    [MenuItem("PacStudent/Complete project setup")]
    public static void Setup() { CompleteProject.Run(); }
}
