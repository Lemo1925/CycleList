using UnityEngine;

namespace CycleListCmp
{
    public interface ICycleListItem
    {
        public GameObject GameObject { get; }
        public RectTransform Transform { get; }
        public int Index { get; }
    
        public ICycleListData Data { get; set; }
    }
}
