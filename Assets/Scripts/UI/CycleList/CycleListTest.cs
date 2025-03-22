using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

class CloneItem : CycleListItem
{
    public GameObject gameObject { get; set; }
    public RectTransform transform { get; set; }
    public int index { get; set; }
    public Text textComp { get; set; }
    public CycleListData itemData { get; set; }
}

class ItemData: CycleListData
{
    public int data;
}

public class CycleListTest : MonoBehaviour
{
    #region Component
        
    ScrollRect scrollRect;
    RectTransform scrollRectTrs;
    RectTransform contentTrs;
    
    #endregion
    
    private int count;
    private float itemSpace;
    private readonly List<CycleListItem> CycleItems = new List<CycleListItem>();
    
    void Start() => InitComponents();

    void InitComponents()
    {
        scrollRect = transform.Find("Scroll View").GetComponent<ScrollRect>();
        scrollRectTrs = (RectTransform)scrollRect.transform;
        contentTrs = scrollRect.content;
        InitCycleItems();
        InitCycleList();
    }

    public static string Key(string x, int y) => $"{x}-{y}";
    
    private void InitCycleItems()
    {
        var ItemBaseGO = contentTrs.Find("BaseClone").gameObject;
        var ItemBaseTrs = (RectTransform)ItemBaseGO.transform;
        var itemWidth = ItemBaseTrs.rect.width;
        itemSpace = itemWidth + 10;
        ItemBaseGO.SetActive(false);
        count = Mathf.CeilToInt(scrollRectTrs.rect.width / itemSpace) + 2;
        for (int i = 0; i < count; i++)
        {
            var itemGo = Instantiate(ItemBaseGO, contentTrs);
            var item = new CloneItem
            {
                gameObject = itemGo,
                transform = (RectTransform)itemGo.transform,
                index = i + 1,
                textComp = itemGo.GetComponentInChildren<Text>()
            };
            itemGo.name = Key(ItemBaseGO.name, item.index);
            CycleItems.Add(item);
        }
    }
    
    private List<CycleListData> InitCycleDataList()
    {
        List<CycleListData> dataList = new List<CycleListData>();
        for (int i = 0; i < 30; i++)
        {
            var dataCell = new ItemData
            {
                data = i + 1
            };
            dataList.Add(dataCell);
        }
        return dataList;
    }
    
    private void InitCycleList()
    {
        var dataList = InitCycleDataList();
        scrollRect.enabled = dataList.Count > Mathf.FloorToInt(scrollRectTrs.rect.width / itemSpace);
        
        CycleListSetting setting = new CycleListSetting
        {
            grid = new GridSetting
            {
              isVertical = false,
              itemSpace = itemSpace,
            },
            
            scrollRect = scrollRect,
            content = contentTrs,
            
            items = CycleItems,
            dataList = dataList,
            
            updateFunc = UpdateCycleList,
        };

        void UpdateCycleList(CycleListItem item, CycleListData data)
        {
            var _item = (CloneItem)item;
            var _data = (ItemData)data;
            _item.itemData = _data;
            _item.textComp.text = $"{_data.data}";
        }
        
        CycleList cycList = new CycleList(setting);
        cycList.SetItem();
    }

    
}
