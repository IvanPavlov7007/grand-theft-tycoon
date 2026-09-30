using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class BuildDisplayer : MonoBehaviour
{
    private TextMeshProUGUI Text;

    private void Awake()
    {
        Text = GetComponent<TextMeshProUGUI>();
        ResourceRequest request = Resources.LoadAsync("Build", typeof(BuildScriptableObject));
        request.completed += Request_completed;
    }

    private void Request_completed(AsyncOperation obj)
    {
        BuildScriptableObject buildScriptableObject = ((ResourceRequest)obj).asset as BuildScriptableObject;

        if (buildScriptableObject == null)
        {
            Debug.LogError("Build scriptable object not found in resources directory! Check build log for errors!");
        }
        else
        {
            Text.SetText($"Build: {buildScriptableObject.GetVersionString()}");
        }
    }
}
