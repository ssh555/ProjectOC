using UnityEngine;
using UnityEditor;
using UnityEngine.Timeline;

[CreateAssetMenu(fileName = "NewCustomObject", menuName = "CustomObjects/CustomObjectWithTimeline", order = 1)]
public class CustomObjectWithTimeline : ScriptableObject
{
    public TimelineAsset timelineAsset;

    private void OnEnable()
    {
        if (timelineAsset == null)
        {
            CreateTimelineAsset();
        }
    }


    public void CreateTimelineAsset()
    {
        timelineAsset = ScriptableObject.CreateInstance<TimelineAsset>();
        timelineAsset.name = name + "Timeline";

        // Create a unique asset path
        string path = AssetDatabase.GenerateUniqueAssetPath("Assets/" + name + "Timeline.asset");

        // Save the TimelineAsset to disk
        AssetDatabase.CreateAsset(timelineAsset, path);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate); // Import the asset to save changes
        // Make the timeline asset a child of this object
        AssetDatabase.AddObjectToAsset(timelineAsset, this);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate); // Import the asset to save changes
    }
}
