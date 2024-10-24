using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

public class ChangeApiCompatibility : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        PlayerSettings.SetApiCompatibilityLevel(BuildTargetGroup.Standalone, ApiCompatibilityLevel.NET_4_6);
    }
}
