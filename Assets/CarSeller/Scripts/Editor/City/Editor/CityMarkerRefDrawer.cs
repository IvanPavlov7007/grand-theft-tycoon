#if UNITY_EDITOR && !ODIN_INSPECTOR
using System.Linq;
using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(CityMarkerRef))]
public class CityMarkerRefDrawer : PropertyDrawer
{
    private readonly float Line = EditorGUIUtility.singleLineHeight;
    private const float Pad = 2f;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Graph field + marker popup + optional tag filter
        return Line * 2f + Pad * 3f;
    }

    public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(rect, label, property);

        var graphProp = property.FindPropertyRelative("Graph");
        var markerIdProp = property.FindPropertyRelative("MarkerId");

        // Draw label
        rect = EditorGUI.IndentedRect(rect);
        var lineRect = new Rect(rect.x, rect.y, rect.width, Line);
        EditorGUI.LabelField(lineRect, label);

        // Graph field
        lineRect.y += Line + Pad;
        EditorGUI.PropertyField(lineRect, graphProp, new GUIContent("Graph"));

        CityGraphAsset graph = graphProp.objectReferenceValue as CityGraphAsset;

        // Marker popup
        lineRect.y += Line + Pad;

        if (graph == null)
        {
            EditorGUI.HelpBox(lineRect, "Assign a CityGraph to select a marker.", MessageType.Info);
        }
        else
        {
            var markers = graph.Markers ?? new System.Collections.Generic.List<CityGraphAsset.MarkerData>();
            // Build display list: Name [tags] (id)
            var options = markers
                .Select(m =>
                {
                    var tagStr = m.Tags != null && m.Tags.Length > 0 ? string.Join(",", m.Tags) : "";
                    var display = string.IsNullOrEmpty(tagStr) ? m.DisplayName : $"{m.DisplayName} [{tagStr}]";
                    return new { display, id = m.Id };
                })
                .ToList();

            int currentIndex = Mathf.Max(0, options.FindIndex(o => o.id == markerIdProp.stringValue));
            if (options.Count == 0)
            {
                EditorGUI.HelpBox(lineRect, "No markers found in the selected graph.", MessageType.Warning);
            }
            else
            {
                var displayNames = options.Select(o => o.display).ToArray();
                int newIndex = EditorGUI.Popup(lineRect, "Marker", currentIndex, displayNames);
                if (newIndex >= 0 && newIndex < options.Count)
                {
                    markerIdProp.stringValue = options[newIndex].id;
                }
            }
        }

        EditorGUI.EndProperty();
    }
}
#endif