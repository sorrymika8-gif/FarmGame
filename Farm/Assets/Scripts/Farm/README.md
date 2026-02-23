# 农场系统使用指南

## 快速开始

### 1. 生成牧草配置

打开 Unity 编辑器后，点击菜单：
```
FarmGame -> Farm -> Generate Pasture Config
```

该步骤会自动生成并写入仅牧草配置（SeedConfigId=10000）。

### 2. 配置 FarmTileSet

1. 选中 `Assets/ScriptableObjects/Farm/FarmTileSet.asset`
2. 在 Inspector 中配置：
   - **Untilled Tile**: 未耕地的 Tile
   - **Tilled Tile**: 已耕地的 Tile
   - **Highlight Tile**: 高亮选中的 Tile
    - **Plant Tile Configs**: 当前仅保留牧草（SeedConfigId=10000）

### 3. 配置 game_settings.xlsx

在 `Assets/Configs/game_settings.xlsx` 中添加以下配置：

| setting_key          | setting_value | value_type | description          |
|----------------------|---------------|------------|----------------------|
| growth_tick_interval | 1000          | int        | 生长周期间隔(毫秒)   |

### 4. 在场景中使用

1. `init_map.prefab` 已直接挂载农田层级（FarmMap）
2. 调用初始化：
```csharp
// 在 FarmManager 初始化后调用
var farmView = FindObjectOfType<FarmTilemapView>();
farmView.Initialize(FarmManager.Instance.CurrentMap);

// 绑定到玩家
var player = PlayerManager.Instance.Player;
var inputHandler = player.GetComponent<PlayerInputHandler>();
inputHandler.SetFarmView(farmView);
```

## 交互说明

### 右键菜单操作

- **未耕地** → 右键显示「耕地」按钮
- **已耕地（无作物）** → 右键显示「种植」按钮，可选择背包中的种子
- **有成熟作物** → 右键显示「收获」按钮
- **生长中作物** → 无可用操作

### 交互距离

玩家必须在 2 个单位距离内才能与土地交互（可在 `PlayerInputHandler.cs` 中修改 `FARM_INTERACTION_DISTANCE`）

## 文件结构

```
Assets/Scripts/Farm/
├── FarmManager.cs          # 农场管理器（业务逻辑）
├── FarmMapData.cs          # 地图数据容器
├── FarmTileSet.cs          # Tile 配置 ScriptableObject
├── PlantEntity.cs          # 作物实体
├── SoilEntity.cs           # 土地实体
├── View/
│   └── FarmTilemapView.cs  # Tilemap 视图组件
└── Editor/
    └── PastureConfigGenerator.cs # 牧草配置生成工具

Assets/Scripts/Game/UI/FarmContextMenu/
├── FarmContextMenuPanel.cs # 右键菜单面板
├── FarmActionButton.cs     # 操作按钮
└── SeedItem.cs             # 种子选择条目
```

## 当前作物配置

- 当前版本仅使用 **牧草**。
- 对应配置：`seed.xlsx` 中 `class_id=10000`。
- 对应 Tile 阶段：`Tile_牧草_0`、`Tile_牧草_1`。

### 添加新操作（如浇水、施肥）

1. 在 `FarmActionType` 枚举中添加新类型
2. 在 `FarmManager` 中添加对应的业务方法
3. 在 `FarmContextMenuPanel.RefreshActionButtons()` 中添加显示逻辑
4. 在 `FarmContextMenuPanel.OnActionClick()` 中添加执行逻辑
