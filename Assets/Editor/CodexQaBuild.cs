using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class CodexQaBuild
{
    public static void BuildWebGL()
    {
        string[] scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        string outputPath = Path.GetFullPath(
            Path.Combine(Application.dataPath, "../../codex-qa-webgl-0909"));

        BuildPlayerOptions options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = outputPath,
            target = BuildTarget.WebGL,
            options = BuildOptions.Development,
        };

        BuildReport report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != BuildResult.Succeeded)
        {
            throw new Exception($"WebGL QA build failed: {report.summary.result}");
        }

        Debug.Log($"WebGL QA build completed: {outputPath}");
    }
}
