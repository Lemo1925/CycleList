# CycleListCmp - Unity 循环列表组件

一个高性能的 Unity UI 循环列表组件，支持垂直和水平滚动，适用于大量数据的展示场景。

## 🎯 功能特性

- ✅ **高性能渲染**: 只渲染可见区域的Item，大幅减少DrawCall
- ✅ **双向滚动支持**: 支持垂直和水平方向滚动
- ✅ **动态Item尺寸**: 支持自定义每个Item的高度/宽度
- ✅ **无缝循环**: 滚动时自动复用Item，实现流畅的循环效果
- ✅ **灵活配置**: 可自定义间距、布局方式等参数

## 📁 项目结构

```shell
Assets/ 
└── Scripts/ 
└── CycleListCmp/ 
    ├── CycleList.cs # 核心循环列表类 
    ├── CycleListData.cs # 数据接口定义 
    ├── CycleListItem.cs # Item接口定义
    └── CycleListTest.cs # 测试示例
```
## 🚀 快速开始

### 1. 基本使用

```csharp
using CycleListCmp; using UnityEngine; using UnityEngine.UI;
public class MyListView : MonoBehaviour {
    
    private CycleList cycleList;
    
    void Start()
    {
        // 准备数据
        List<ICycleListData> dataList = new List<ICycleListData>();
        for(int i = 0; i < 1000; i++)
        {
            dataList.Add(new MyData { id = i, name = $"Item {i}" });
        }
        
        // 配置设置
        CycleListSetting setting = new CycleListSetting
        {
            grid = new GridSetting
            {
                isVertical = true,      // 垂直滚动
                itemSpace = 100f        // Item间距
            },
            scrollRect = GetComponent<ScrollRect>(),
            content = GetComponent<ScrollRect>().content,
            items = GetItemList(),      // 预制的Item列表
            dataList = dataList,
            updateFunc = UpdateItem     // 更新Item的回调函数
        };
        
        // 创建循环列表
        cycleList = new CycleList(setting);
        cycleList.SetItem();
    }

    private void UpdateItem(ICycleListItem item, ICycleListData data)
    {
        // 在这里更新你的Item显示
        var myItem = (MyItem)item;
        var myData = (MyData)data;
        myItem.SetText(myData.name);
    }
}
```
### 2. 自定义Item尺寸

如果需要不同Item有不同的尺寸，可以使用 `onLayoutFunc`：

```csharp
setting.onLayoutFunc = (data) => {
    var myData = (MyData)data; 
    return myData.isSpecial ? 150f : 100f; // 特殊Item更高 
};
```
## 🔧 核心类说明

### CycleListSetting 配置类

| 属性 | 类型 | 说明 |
|------|------|------|
| `grid` | GridSetting | 网格设置 |
| `scrollRect` | ScrollRect | 滚动视图组件 |
| `content` | RectTransform | 内容容器 |
| `items` | List<ICycleListItem> | 预制Item列表 |
| `dataList` | List<ICycleListData> | 数据列表 |
| `updateFunc` | Action<ICycleListItem, ICycleListData> | Item更新回调 |
| `onLayoutFunc` | Func<ICycleListData, float> | 自定义布局回调(可选) |

### GridSetting 网格设置

| 属性 | 类型 | 说明 |
|------|------|------|
| `isVertical` | bool | 是否垂直滚动 |
| `itemSpace` | float | Item间距 |

### 接口定义

#### ICycleListData (数据接口)

```csharp
csharp public interface ICycleListData {}
```

#### ICycleListItem (Item接口)
```csharp
public interface ICycleListItem 
{
    GameObject GameObject { get; } 
    RectTransform Transform { get; } 
    int Index { get; } 
    ICycleListData Data { get; set; }
}
```

## 📖 使用示例

查看 `CycleListTest.cs` 文件获取完整的使用示例，包括：
- 如何初始化组件
- 如何创建Item池
- 如何处理数据绑定
- 如何实现自定义布局

## ⚙️ 最佳实践

1. **Item预设数量**: 建议预设 `可视区域/(Item高度+间距) + 2` 个Item
2. **性能优化**: 避免在 `updateFunc` 中进行复杂计算
3. **内存管理**: 及时清理不再使用的资源
4. **数据更新**: 使用 `SetItem(List<ICycleListData>)` 方法更新数据

## 🐛 常见问题

**Q: 为什么Item显示不完整？**
A: 检查Item预制体的数量是否足够，建议按照最佳实践设置

**Q: 滚动卡顿怎么办？**
A: 优化 `updateFunc` 回调函数，避免复杂操作

**Q: 如何实现横向滚动？**
A: 设置 `GridSetting.isVertical = false`

## 📄 许可证

MIT License

## 🤝 贡献

欢迎提交 Issue 和 Pull Request 来改进这个项目！

## 📞 联系方式

如有问题，请联系：Mo_Lemo@outlook.com
