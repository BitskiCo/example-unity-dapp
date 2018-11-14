using UnityEditor;
using UnityEngine;
using UnityEditor.Build.Reporting;

class Builder {
    [MenuItem("Build/Build WebGL")]
    static void BuildWebGL() {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        buildPlayerOptions.scenes = new[] {"Assets/scenes/LoginScene.unity"};
        buildPlayerOptions.locationPathName = "builds/WebGLversion";
        buildPlayerOptions.target = BuildTarget.WebGL;
        buildPlayerOptions.options = BuildOptions.None;

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
        }

        if (summary.result == BuildResult.Failed)
        {
            Debug.Log("Build failed");
        }   
    }
}

