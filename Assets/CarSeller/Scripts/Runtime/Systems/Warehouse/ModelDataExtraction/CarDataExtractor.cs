using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

//TODO Make this work safely, saving changes, and being careful with the config
public class CarDataExtractor : MonoBehaviour
{
#if UNITY_EDITOR

    public CarBaseConfig carBaseConfig;

    public List<Transform> slots = new List<Transform>();
    public Transform frame;
    public SpriteRenderer windshieldSpriteRenderer;
    public SpriteRenderer frameSpriteRenderer;

    [ShowInInspector]
    public static string frameChildName = "body";
    [ShowInInspector]
    public static string windshieldChildName = "windshield";

    private Dictionary<PartSlotBaseConfig, Transform> slotsMap = new Dictionary<PartSlotBaseConfig, Transform>();

    // load slot data
    [Button]
    [ContextMenu("Load The Config To This Object")]
    public void LoadConfigToThisObject()
    {
        if (carBaseConfig == null)
        {
            Debug.LogError("CarBaseConfig is not assigned.");
            return;
        }
        cleanUp();

        if(carBaseConfig.CarFrameBaseConfig != null)
        {
            var frameGO = Instantiate(carBaseConfig.CarFrameBaseConfig.Prefab,
                transform);
            windshieldSpriteRenderer = CommonTools.AllChildren(frameGO?.transform).
                First(item => item.name.Contains(windshieldChildName,System.StringComparison.OrdinalIgnoreCase))?.GetComponent<SpriteRenderer>();
            frameSpriteRenderer = CommonTools.AllChildren(frameGO?.transform).
                First(item => item.name.Contains(frameChildName, System.StringComparison.OrdinalIgnoreCase))?.GetComponent<SpriteRenderer>();

            windshieldSpriteRenderer.color = carBaseConfig.CarFrameBaseConfig.WindshieldColor;
            frameSpriteRenderer.color = carBaseConfig.CarFrameBaseConfig.FrameColor;
            frame = frameGO.transform;
        }

        foreach(var slotConfig in carBaseConfig.SlotConfigs)
        {
            if (slotConfig.partSlotData.Hidden)
                continue;
            var slotTransform = new GameObject().transform;
            slotTransform.SetParent(transform);
            slotTransform.localPosition = slotConfig.partSlotData.LocalPosition;
            slotTransform.localRotation = Quaternion.Euler(slotConfig.partSlotData.LocalRotation);
            slotTransform.localScale = slotConfig.partSlotData.LocalScale;
            slotTransform.name = slotConfig.SlotType.ToString() + " Slot";
            slots.Add(slotTransform);
            slotsMap.Add(slotConfig, slotTransform);
        }
    }

    private void cleanUp()
    {
        var list = new List<GameObject>(new[] { gameObject });
        list.AddRange(slots.Select(transformItem => transformItem.gameObject));
        if (frame != null)
        {
            list.Add(frame.gameObject);
        }
        Undo.RecordObjects(list.ToArray(), "Load Car Config");
        destroySlotsRepresintation();
        if (frame != null)
        {
            DestroyImmediate(frame.gameObject);
        }

        windshieldSpriteRenderer = null;
        frameSpriteRenderer = null;
    }

    private void destroySlotsRepresintation()
    {
        foreach(var slot in slots)
        {
            DestroyImmediate(slot.gameObject);
        }
        slots.Clear();
        slotsMap.Clear();
    }

    // save all slot data
    [Button]
    public void SaveThisObjectToConfig()
    {
        if (carBaseConfig == null) { Debug.LogError("CarBaseConfig is not assigned."); return; }
        // Record for undo first:
        Undo.RecordObject(carBaseConfig, "Save Car Base Config");
        if (carBaseConfig.CarFrameBaseConfig != null)
        {
            Undo.RecordObject(carBaseConfig.CarFrameBaseConfig, "Save Car Frame Config");
        }

        if (windshieldSpriteRenderer != null && carBaseConfig.CarFrameBaseConfig != null)
        {
            carBaseConfig.CarFrameBaseConfig.WindshieldColor = windshieldSpriteRenderer.color;
        }
        if (frameSpriteRenderer != null && carBaseConfig.CarFrameBaseConfig != null)
        {
            carBaseConfig.CarFrameBaseConfig.FrameColor = frameSpriteRenderer.color;
        }

        foreach (var slotConfig in carBaseConfig.SlotConfigs)
        {
            if (slotsMap.TryGetValue(slotConfig, out var slotTransform))
            {

                slotConfig.partSlotData.LocalPosition = slotTransform.localPosition;
                slotConfig.partSlotData.LocalRotation = slotTransform.localRotation.eulerAngles;
                slotConfig.partSlotData.LocalScale = slotTransform.localScale;
            }
        }

        // Explicitly mark assets dirty so Unity knows to serialize them:
        EditorUtility.SetDirty(carBaseConfig);
        if (carBaseConfig.CarFrameBaseConfig != null)
        {
            EditorUtility.SetDirty(carBaseConfig.CarFrameBaseConfig);
        }


        // Optional but useful to avoid losing work on editor close:
        AssetDatabase.SaveAssets();
    }
#endif
}
