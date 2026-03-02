using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CycleListCmp
{
    public struct GridSetting
    {
        public float itemSpace;
        public bool isVertical;
    }

    public struct CycleListSetting 
    {
        public GridSetting grid;
    
        public ScrollRect scrollRect;
        public RectTransform content;
    
        public List<ICycleListItem> items;
        public List<ICycleListData> dataList;
    
        public Action<ICycleListItem, ICycleListData> updateFunc;
        public Func<ICycleListData, float> onLayoutFunc;
    }

    public class CycleList
    {
        private readonly CycleListSetting setting;

        #region Grid
    
        private GridSetting grid;
   
        private bool isVertical;
        private float itemSpace;
        private int count;

        #endregion
    
        #region CycleList

        private int canShowNum;
        private float totalScroll;
        private int startIndex, endIndex;
        private List<ICycleListItem> items;
        private List<ICycleListData> dataList;
    
        private ScrollRect scrollRect;
        private RectTransform content;
        private Vector2 scrollLastPosition;
    
        private Action<ICycleListItem, ICycleListData> updateFunc;
        public Func<ICycleListData, float> onLayoutFunc;

        #endregion


        public CycleList(CycleListSetting setting)
        {
            this.setting = setting;
            UpdateSetting();
            InitArgs();
        }
    
        private void UpdateSetting()
        {
            grid = setting.grid;
        
            content = setting.content;
            scrollRect = setting.scrollRect;
        
            items = setting.items;
            dataList = setting.dataList;
            updateFunc = setting.updateFunc;
            onLayoutFunc = setting.onLayoutFunc;
        }

        private void InitArgs()
        {
            startIndex = 0;
            isVertical = grid.isVertical;
            itemSpace = grid.itemSpace;
            count = Mathf.Min(items.Count, dataList.Count); //需要的item数量
            canShowNum = isVertical
                ? Mathf.CeilToInt(scrollRect.GetComponent<RectTransform>().rect.height / itemSpace)
                : Mathf.CeilToInt(scrollRect.GetComponent<RectTransform>().rect.width / itemSpace);
            canShowNum = Mathf.Min(canShowNum, dataList.Count);
            endIndex = startIndex + canShowNum - 1 + 2; //items[^1]对应的endIndex = startIndex + 总共创建出来的Item数 - 1
        }

    
        public void SetItem() => SetItem(dataList);

        public void SetItem(List<ICycleListData> datalist)
        {
            dataList = datalist;
       
            scrollLastPosition = content.anchoredPosition;
            scrollRect.onValueChanged.AddListener(OnScroll);
            InitCycleList();
        }

        private void InitCycleList()
        {
            var spacingOffset = 0f;
            for (int i = 0; i < count; i++)
            {
                var dataIndex = startIndex + i;
                var item = items[i];
                item.Data = dataList[dataIndex];
                updateFunc(item, dataList[dataIndex]);
                var spacing = onLayoutFunc == null ? itemSpace : onLayoutFunc(item.Data);
                item.GameObject.SetActive(true);
                if (onLayoutFunc != null)
                {
                    if (isVertical)
                    {
                        item.Transform.sizeDelta = new Vector2(item.Transform.sizeDelta.x, spacing);
                        item.Transform.anchoredPosition = new Vector2(item.Transform.anchoredPosition.x, spacingOffset);
                    }
                    else
                    {
                        item.Transform.sizeDelta = new Vector2(spacing, item.Transform.sizeDelta.y);
                        item.Transform.anchoredPosition = new Vector2(spacingOffset, item.Transform.anchoredPosition.y);
                    }
                    spacingOffset += spacing;
                }
                else
                {
                    item.Transform.anchoredPosition = isVertical
                        ? new Vector2(item.Transform.anchoredPosition.x, itemSpace * i)
                        : new Vector2(itemSpace * i, item.Transform.anchoredPosition.y);
                }
            }
            CalculateContent();
        }

        private void OnScroll(Vector2 scrollPosition)
        {
            var verticalDelta = scrollPosition.y - scrollLastPosition.y;
            var horizontalDelta = scrollPosition.x - scrollLastPosition.x; //左移是正的
            scrollLastPosition = scrollPosition;
            totalScroll = isVertical ? totalScroll + verticalDelta : totalScroll + horizontalDelta;
            if ((totalScroll < 0 && items[0].Data == dataList[0])  
                || (totalScroll > 0 && items[^1].Data == dataList[^1]) ) 
                return;
        
            DoScroll();
            ReCalculateItem();
        }

        private void DoScroll()
        {
            var viewPortDelta = isVertical 
                ? (content.rect.height - scrollRect.viewport.rect.height) * totalScroll
                : (content.rect.width - scrollRect.viewport.rect.width) * totalScroll;
            float rightestEdge = isVertical 
                ? items[^1].Transform.anchoredPosition.y  + itemSpace
                : items[^1].Transform.anchoredPosition.x + itemSpace;
            float rightArea = isVertical 
                ? scrollRect.viewport.rect.height + (-content.anchoredPosition.y) 
                : scrollRect.viewport.rect.width + (-content.anchoredPosition.x);
            float leftestItemMove = isVertical
                ? content.anchoredPosition.y + items[0].Transform.anchoredPosition.y
                : content.anchoredPosition.x + items[0].Transform.anchoredPosition.x;
            //       Item移动超过1个间隔          右边有空位但是Item[^1]不是数据第^1位
            if (viewPortDelta > itemSpace || (endIndex < dataList.Count - 1 && rightestEdge < rightArea))
            {
                //item往左
                int moveIndex = Mathf.FloorToInt(viewPortDelta / itemSpace); //移动的次数，移动过快可能会跨Item
                moveIndex = Mathf.Max(1, moveIndex); //至少一次
                totalScroll = 0;

                while (moveIndex-- > 0)
                {
                    if (endIndex < dataList.Count - 1)  
                    {
                   
                        var firstItem = items[0];
                        var newPos = isVertical
                            ? new Vector2(firstItem.Transform.anchoredPosition.x,
                                items[^1].Transform.anchoredPosition.y - itemSpace)
                            : new Vector2(items[^1].Transform.anchoredPosition.x + itemSpace,
                                firstItem.Transform.anchoredPosition.y);
                        firstItem.Transform.anchoredPosition = newPos;
                        items.RemoveAt(0);
                        items.Add(firstItem);
                    
                        startIndex++;  
                        endIndex++;
                        updateFunc(items[^1], dataList[endIndex]);
                    }
                }
            }
            //         右移1个间隔              左边有空位但是Item[0]不是数据第0位
            else if (viewPortDelta < -itemSpace || (startIndex > 0 && leftestItemMove > 0))
            {
                //item往右
                //移动的次数，移动过快可能会跨Item
                int moveIndex = Mathf.FloorToInt(Mathf.Abs(viewPortDelta) / itemSpace);
                //至少一次
                moveIndex = Mathf.Max(1, moveIndex); 
                totalScroll = 0;

                while (moveIndex-- > 0)
                {
                    if (startIndex > 0 )
                    {
                        var lastItem = items[^1];
                        var newPos = isVertical
                            ? new Vector2(lastItem.Transform.anchoredPosition.x, 
                                items[0].Transform.anchoredPosition.y + itemSpace)
                            : new Vector2(items[0].Transform.anchoredPosition.x - itemSpace, 
                                lastItem.Transform.anchoredPosition.y);
                        lastItem.Transform.anchoredPosition = newPos;
                        items.Remove(lastItem);
                        items.Insert(0, lastItem);
                        startIndex--;
                        endIndex--;
                        updateFunc(items[0], dataList[startIndex]);
                      
                    }
                }
            }
        }

        private void ReCalculateItem()
        {
            if (onLayoutFunc == null) 
                return;
        
            var spacingOffset = 0f;
            var startPos = items[0].Data == dataList[0] 
                ? Vector2.zero  // 如果是第一个数据就用（0，0）
                : items[0].Transform.anchoredPosition;
            for (int i = 0; i < count; i++)
            {
                var item = items[i];
                var spacing = onLayoutFunc(item.Data);
                if (isVertical)
                {
                    item.Transform.sizeDelta = new Vector2(item.Transform.sizeDelta.x, spacing);
                    item.Transform.anchoredPosition = new Vector2(item.Transform.anchoredPosition.x, startPos.y - spacingOffset);
                }
                else
                {
                    item.Transform.sizeDelta = new Vector2(spacing, item.Transform.sizeDelta.y);
                    item.Transform.anchoredPosition = new Vector2(startPos.x + spacingOffset, item.Transform.anchoredPosition.y);
                }
                spacingOffset += spacing;
            }
        }

        private void CalculateContent()
        {
            var contentSize = 0f;
            var originPos = content.anchoredPosition;
            if (onLayoutFunc != null)
            {
                for (int i = 0; i < count; i++)
                { 
                    var item = items[i];
                    var newSize = onLayoutFunc(item.Data);
                    // 重算content大小
                    contentSize += newSize;
                }
                // 把少算的长度补回来
                contentSize += itemSpace * (dataList.Count - count);
            }
            else
            {
                contentSize = dataList.Count * itemSpace;
            }

            content.sizeDelta = isVertical
                ? new Vector2(content.sizeDelta.x, contentSize)
                : new Vector2(contentSize, content.sizeDelta.y);
            content.anchoredPosition = originPos;
        }
    }
}