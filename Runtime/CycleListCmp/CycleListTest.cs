using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CycleListCmp
{
    class CloneItem : ICycleListItem
    {
        public GameObject GameObject { get; set; }
        public RectTransform Transform { get; set; }
        public int Index { get; set; }
        public Text textComp { get; set; }
        public ICycleListData Data { get; set; }
    }

    class ItemData: ICycleListData
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
        private readonly List<ICycleListItem> cycleItems = new List<ICycleListItem>();
    
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
                    GameObject = itemGo,
                    Transform = (RectTransform)itemGo.transform,
                    Index = i + 1,
                    textComp = itemGo.GetComponentInChildren<Text>()
                };
                itemGo.name = Key(ItemBaseGO.name, item.Index);
                cycleItems.Add(item);
            }
        }
    
        private List<ICycleListData> InitCycleDataList()
        {
            List<ICycleListData> dataList = new List<ICycleListData>();
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
            
                items = cycleItems,
                dataList = dataList,
            
                updateFunc = UpdateCycleList,
                //onLayoutFunc = OnLayOut,
            };

            void UpdateCycleList(ICycleListItem item, ICycleListData data)
            {
                var _item = (CloneItem)item;
                var _data = (ItemData)data;
                _item.Data = _data;
                _item.textComp.text = $"{_data.data}";
            }
        
            CycleList cycList = new CycleList(setting);
            cycList.SetItem();
        }

        private float OnLayOut(ICycleListData data)
        {
            var itemData = (ItemData)data;
            if (itemData.data == 4)
                return itemSpace + 20;
            return itemSpace;
        }
    }
}