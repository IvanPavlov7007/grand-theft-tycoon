using System;
using System.Linq;
using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(PolygonCollider2D))]
public class CityAreaAuthor : MonoBehaviour
{
    [SerializeField] private string _id;
    public string Id => _id;

    [Header("Identification")]
    public string DisplayName;
    public string[] Tags;

    public PolygonCollider2D PolygonCollider => GetComponent<PolygonCollider2D>();

    private void OnValidate()
    {
        EnsureId();
    }

    public void EnsureId()
    {
        if (string.IsNullOrEmpty(_id))
            _id = Guid.NewGuid().ToString("N");
    }

#if UNITY_EDITOR
    void EnsureUniqueIdInScene()
    {
        var all = FindObjectsOfType<CityAreaAuthor>(true);
        if (all.Count(x => x != this && x._id == _id) > 0)
            _id = Guid.NewGuid().ToString("N");
    }
#endif
}
