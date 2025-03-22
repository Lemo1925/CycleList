using UnityEngine;

public interface CycleListItem
{
    public GameObject gameObject { get; }
    public RectTransform transform { get; }
    public int index { get; }
    
    public CycleListData data { get; set; }
}
