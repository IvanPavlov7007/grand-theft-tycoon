using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

static class RoadNodeAuthorShortcuts
{
    [MenuItem("Tools/CarSeller/City/Connect Selected Road Nodes", true)]
    static bool ConnectSelectedRoadNodes_Validate()
    {
        return TryGetTwoSelectedNodes(out _, out _);
    }

    [MenuItem("Tools/CarSeller/City/Connect Selected Road Nodes")]
    static void ConnectSelectedRoadNodes_Menu()
    {
        ConnectSelectedRoadNodes();
    }

    // Default hotkey: Ctrl+Alt+E (change in Edit > Shortcuts...)
    [Shortcut("CarSeller/City/Connect Selected Road Nodes", KeyCode.E, ShortcutModifiers.Control | ShortcutModifiers.Alt)]
    static void ConnectSelectedRoadNodes()
    {
        if (Application.isPlaying)
        {
            Debug.Log("Connect Selected Road Nodes: disabled in Play Mode.");
            return;
        }

        if (!TryGetTwoSelectedNodes(out var fromNode, out var toNode))
        {
            Debug.Log("Select exactly 2 GameObjects that have RoadNodeAuthor components.");
            return;
        }

        // ConnectToSelected() reads Selection.activeGameObject as the target, so force it.
        Selection.activeGameObject = toNode.gameObject;

        fromNode.ConnectToSelected();
    }

    static bool TryGetTwoSelectedNodes(out RoadNodeAuthor fromNode, out RoadNodeAuthor toNode)
    {
        fromNode = null;
        toNode = null;

        var selected = Selection.gameObjects;
        if (selected == null || selected.Length != 2)
            return false;

        var aGo = selected[0];
        var bGo = selected[1];

        var a = aGo != null ? aGo.GetComponent<RoadNodeAuthor>() : null;
        var b = bGo != null ? bGo.GetComponent<RoadNodeAuthor>() : null;
        if (a == null || b == null)
            return false;

        var activeGo = Selection.activeGameObject;

        // Prefer the active object as the "to" node (the one Selection.activeGameObject points to).
        if (activeGo == bGo)
        {
            fromNode = a;
            toNode = b;
            return true;
        }

        if (activeGo == aGo)
        {
            fromNode = b;
            toNode = a;
            return true;
        }

        // Fallback if active isn't one of them.
        fromNode = a;
        toNode = b;
        return true;
    }
}