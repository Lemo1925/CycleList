using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

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
    
    public List<CycleListItem> items;
    public List<CycleListData> dataList;
    
    public Action<CycleListItem, CycleListData> updateFunc;
}

public class CycleList
{
    private readonly CycleListSetting setting;

    #region Grid
    
    private GridSetting grid;
   
    private bool isVertical;
    private float itemSpace;
    
    #endregion
    
    #region CycleList

    private int canShowNum;
    private float totalScroll;
    private int startIndex, endIndex;
    private List<CycleListItem> items;
    private List<CycleListData> dataList;
    
    private ScrollRect scrollRect;
    private RectTransform content;
    private Vector2 scrollLastPosition;
    
    private Action<CycleListItem, CycleListData> updateFunc;
    
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
    }

    private void InitArgs()
    {
        startIndex = 0;
        isVertical = grid.isVertical;
        itemSpace = grid.itemSpace;
        canShowNum = isVertical
            ? Mathf.CeilToInt(scrollRect.GetComponent<RectTransform>().rect.height / itemSpace)
            : Mathf.CeilToInt(scrollRect.GetComponent<RectTransform>().rect.width / itemSpace);
        canShowNum = Mathf.Min(canShowNum, dataList.Count);
        endIndex = startIndex + canShowNum - 1 + 2; //items[^1]对应的endIndex = startIndex + 总共创建出来的Item数 - 1
    }

    
    public void SetItem() => SetItem(dataList);

    public void SetItem(List<CycleListData> datalist)
    {
        dataList = datalist;
        content.sizeDelta = isVertical
            ? new Vector2(content.sizeDelta.x, dataList.Count * itemSpace)
            : new Vector2(dataList.Count * itemSpace, content.sizeDelta.y);
        scrollLastPosition = content.anchoredPosition;
        scrollRect.onValueChanged.AddListener(OnScroll);
        InitCycleList();
    }
    
    
    private void OnScroll(Vector2 scrollPosition)
    {
        var verticalDelta = scrollPosition.y - scrollLastPosition.y;
        var horizontalDelta = scrollPosition.x - scrollLastPosition.x; //左移是正的
        scrollLastPosition = scrollPosition;
        totalScroll = isVertical ? totalScroll + verticalDelta : totalScroll + horizontalDelta;
        if ((totalScroll < 0 && items[0].itemData == dataList[0])  
            || (totalScroll > 0 && items[^1].itemData == dataList[^1]) ) 
            return;
        
        DoScroll();
    }

    private void DoScroll()
    {
        var viewPortDelta = isVertical 
            ? (content.rect.height - scrollRect.viewport.rect.height) * totalScroll
            : (content.rect.width - scrollRect.viewport.rect.width) * totalScroll;
        float rightestEdge = isVertical 
            ? items[^1].transform.anchoredPosition.y  + itemSpace
            : items[^1].transform.anchoredPosition.x + itemSpace;
        float rightArea = isVertical 
            ? scrollRect.viewport.rect.height + (-content.anchoredPosition.y) 
            : scrollRect.viewport.rect.width + (-content.anchoredPosition.x);
        //       Item移动超过1个间隔  ||       右边有空位但是Item[^1]不是数据第^1位
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
                        ? new Vector2(firstItem.transform.anchoredPosition.x,
                            items[^1].transform.anchoredPosition.y - itemSpace)
                        : new Vector2(items[^1].transform.anchoredPosition.x + itemSpace,
                            firstItem.transform.anchoredPosition.y);
                    firstItem.transform.anchoredPosition = newPos;
                    items.RemoveAt(0);
                    items.Add(firstItem);
                    
                    startIndex++;  
                    endIndex++;
                    updateFunc(items[^1], dataList[endIndex]);
                }
            }
        }
        else
        {
            float leftestItemMove =
                isVertical ? content.anchoredPosition.y + items[0].transform.anchoredPosition.y 
                            : content.anchoredPosition.x + items[0].transform.anchoredPosition.x;
            //         右移1个间隔          ||            左边有空位但是Item[0]不是数据第0位
            if (viewPortDelta < -itemSpace || (startIndex > 0 && leftestItemMove > 0))
            {
                //item往右
                
                int moveIndex = Mathf.FloorToInt(Mathf.Abs(viewPortDelta) / itemSpace); //移动的次数，移动过快可能会跨Item
                moveIndex = Mathf.Max(1, moveIndex); //至少一次
                totalScroll = 0;

                while (moveIndex-- > 0)
                {
                    if (startIndex > 0 )
                    {
                        var lastItem = items[^1];
                        var newPos = isVertical
                            ? new Vector2(lastItem.transform.anchoredPosition.x, 
                                items[0].transform.anchoredPosition.y + itemSpace)
                            : new Vector2(items[0].transform.anchoredPosition.x - itemSpace, 
                                lastItem.transform.anchoredPosition.y);
                        lastItem.transform.anchoredPosition = newPos;
                        items.Remove(lastItem);
                        items.Insert(0, lastItem);
                        startIndex--;
                        endIndex--;
                        updateFunc(items[0], dataList[startIndex]);
                    }
                }
            }
        }
    }
    

    private void InitCycleList()
    {
        var count = Mathf.Min(items.Count, dataList.Count);
        for (int i = 0; i < count; i++)
        {
            var dataIndex = startIndex + i;
            var item = items[i];
            updateFunc(item, dataList[dataIndex]);
            item.gameObject.SetActive(true);
            item.transform.anchoredPosition = isVertical
                ? new Vector2(item.transform.anchoredPosition.x, itemSpace * i)
                : new Vector2(itemSpace * i, item.transform.anchoredPosition.y);
        }
    }
}
