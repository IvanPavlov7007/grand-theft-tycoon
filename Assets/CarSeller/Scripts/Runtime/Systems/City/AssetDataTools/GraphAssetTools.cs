public static class GraphAssetTools
{
    public static CityGraphAsset.MarkerData GetMarkerDataById(CityGraphAsset graph, string markerId)
    {
        if (graph == null || string.IsNullOrEmpty(markerId))
        {
            return null;
        }
        foreach (var marker in graph.Markers)
        {
            if (marker.Id == markerId)
            {
                return marker;
            }
        }
        return null;
    }
}