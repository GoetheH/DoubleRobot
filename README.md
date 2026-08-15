# 像素风机甲对战 · Pixel Robot Battle

一款使用 Unity + C# 开发的复古像素风 2D 机甲对战小游戏。支持**人机对战**与**双人对战**两种模式，机甲可移动、跳跃、射击、防御，具备血量、伤害反馈与胜负判定。

---

## 特性一览

- **像素风美术**：所有机甲、子弹、爆炸特效均通过代码动态生成 `Texture2D`，无需外部美术资源。
- **两种对战模式**：单人模式（智能 AI 对战）与双人模式（本地双人对战）。
- **真实物理**：`Rigidbody2D` + 重力模拟，支持跳跃、实体地面与四壁碰撞，机甲不会掉出场景。
- **子弹战斗系统**：双方朝面向方向水平发射子弹，子弹带爆炸特效、墙体销毁、3 秒自毁。
- **智能 AI**：躲避子弹、反应防御、低血后撤、走位射击、逼防、头顶逃离等多级行为决策。
- **战斗反馈**：受击闪烁、击退震动、伤害数字飘字、血条低血变色（绿→黄→红）。
- **完整 UI 流程**：开始页 → 模式选择页 → 战斗 → 胜利/失败页（含"再来一次"直接重开、"返回开始页面"）。
- **场景自适应**：背景、地面、墙壁随相机视口分辨率动态缩放。

---

## 技术栈

| 项 | 说明 |
|---|---|
| 引擎 | Unity **2022.3.62f3c1** |
| 语言 | C# |
| 渲染管线 | Built-in |
| UI | uGUI（`com.unity.ugui`）|
| 物理 | 2D Physics（`Rigidbody2D` + `BoxCollider2D`）|
| 资源 | 全部代码生成，零外部美术依赖 |

---

## 快速开始

1. 安装 Unity 2022.3.62f3c1（或同 2022.3.x 版本）。
2. 用 Unity Hub 打开本项目的 `DoubleRobot` 文件夹。
3. 等待 Unity 完成首次导入与编译。
4. 打开场景 `Assets/Scenes/Main.unity`。
5. 点击顶部 **Play** 按钮（或按 F5）开始游戏。

> 场景运行时会由 `SceneSetup` 自动搭建相机、背景、地面、墙壁、UI、玩家与敌人，无需手动拖拽预制体。

---

## 操作说明

### 玩家 1（蓝色机甲 · 左侧）

| 操作 | 按键 |
|---|---|
| 左右移动 | `A` / `D` |
| 跳跃 | `W` 或 `Space` |
| 射击 | 鼠标左键 |
| 防御（按住生效）| 鼠标右键 |

### 玩家 2（红色机甲 · 右侧，双人模式）

| 操作 | 按键 |
|---|---|
| 左右移动 | `←` / `→` |
| 跳跃 | `↑` |
| 射击 | `L` |
| 防御（按住生效）| `K` |

> 单人模式下，红色机甲由 AI 接管，自动追击与战斗。防御机制为**按住防御、松开解除**，持续期间受到伤害减半。

---

## 游戏机制

- **血量**：双方初始 100 HP，子弹命中扣 20 HP。
- **防御**：按住右键/K 进入防御，受击伤害减少 50%；防御中无法移动与攻击。
- **胜负**：一方血量归零即判胜负。单人模式玩家获胜显示"胜利"，失败显示"失败"；双人模式显示对应玩家获胜。
- **再来一次**：结束页点击"再来一次"会**直接重开同一模式**，无需重新选择。

---

## 项目结构

```
DoubleRobot/
├── Assets/
│   ├── Scenes/
│   │   └── Main.unity                  # 唯一场景，运行时自动搭建
│   ├── Scripts/
│   │   ├── Combat/
│   │   │   ├── RobotController.cs      # 机甲基类：血量/移动/攻击/防御/受击/死亡
│   │   │   └── Bullet.cs               # 子弹：飞行/碰撞/爆炸特效/自毁
│   │   ├── Player/
│   │   │   ├── PlayerController.cs      # 玩家1输入（WASD+鼠标）
│   │   │   └── Player2Controller.cs     # 玩家2输入（方向键+KL）
│   │   ├── Enemy/
│   │   │   └── EnemyController.cs       # AI行为：多级优先级决策
│   │   ├── Managers/
│   │   │   ├── GameManager.cs           # 全局状态/模式/胜负/重开
│   │   │   └── SceneSetup.cs            # 场景自动搭建（相机/背景/墙/UI/角色）
│   │   ├── UI/
│   │   │   ├── UIManager.cs             # 页面切换与自动重开
│   │   │   └── HealthBar.cs             # 血条（anchorMax.x控制宽度+变色）
│   │   ├── CameraFollow.cs              # 相机跟随（当前为固定相机）
│   │   └── PixelSpriteGenerator.cs      # 像素纹理生成（机甲/背景）
│   └── Editor/                          # 编辑器辅助工具
├── Packages/manifest.json               # 依赖：2d.sprite / ugui / textmeshpro 等
├── ProjectSettings/                     # Unity项目配置（含Tag/Layer）
└── .gitignore
```

---

## 代码架构

### 状态机

`GameManager` 管理全局状态：

```
Menu → SelectMode → Playing → Victory / Defeat
                                      ↓ (再来一次)
                                   Playing（同模式直接重开）
                                      ↓ (返回开始页面)
                                    Menu
```

- `GameState`：`Menu` / `SelectMode` / `Playing` / `Victory` / `Defeat`
- `GameMode`：`None` / `SinglePlayer` / `TwoPlayer`

### 类继承关系

```
RobotController（基类：战斗与物理）
   ├── PlayerController   （玩家1）
   ├── Player2Controller  （玩家2）
   └── EnemyController    （AI）
```

`EnemyController` 同时挂在红色机甲上；双人模式下 `Player2Controller` 接管输入，`EnemyController` 自动休眠。

### AI 行为优先级

`EnemyController.Update` 按以下优先级依次判断：

1. **头顶逃离**：玩家站在 AI 头上时侧移+跳跃甩开。
2. **躲避子弹**：检测来袭子弹，近距防御、远距跳跃。
3. **反应防御**：玩家攻击时按概率举盾。
4. **低血后撤**：自身低血且对方高血时拉开距离。
5. **战斗决策**：太远靠近、理想距离走位射击、近距攻击或逼防。

### 跨场景状态传递

"再来一次"通过 `GameManager` 的**静态变量** `PendingAutoStart` / `PendingMode` 传递重开状态，场景重载后由 `UIManager.Start()` 读取并直接进入游戏，避免 PlayerPrefs 时序问题。

---

## 关键技术点

- **零美术依赖**：`PixelSpriteGenerator` 用像素数组绘制 16×16 机甲，`Bullet`/`SceneSetup` 动态生成子弹与爆炸纹理。
- **血条可靠性**：`HealthBar` 用 `Image.Type.Simple` + `anchorMax.x` 控制填充宽度，避免 `Filled` 类型在无 Sprite 时不渲染的问题。
- **墙壁自适应**：`BackgroundScaler` 让左右墙、顶部墙、地面随相机 `orthographicSize` 与 `aspect` 动态缩放，防止机甲掉出场景。
- **输入隔离**：玩家1 仅响应 A/D/W 与鼠标，玩家2 仅响应方向键与 K/L，避免双人模式操控冲突。
- **防御不卡顿**：`PlayerController.Update` 不在防御时 return，持续检测鼠标释放，保证按住防御时帧率正常。

---

## 配置说明

### Tag / Layer

`ProjectSettings/TagManager.asset` 中已配置：
- **Tag**：`Player`、`Enemy`
- **Layer**：`Player`、`Enemy`

子弹与检测依赖这些 Tag/Layer 区分敌我。

### 依赖包

见 `Packages/manifest.json`，核心为：
- `com.unity.2d.sprite`
- `com.unity.ugui`
- `com.unity.textmeshpro`
- `com.unity.modules.physics2d`

---

## 开发说明

- 修改战斗数值（血量、伤害、速度、冷却等）见 `RobotController` 与 `EnemyController` 的 `[Header]` 字段，均可在 Inspector 调整。
- AI 智能参数（检测范围、躲避概率、防御概率、后撤血量阈值等）见 `EnemyController` 顶部 `AI Settings`。
- 场景布局参数（相机大小、地面高度、墙壁厚度）见 `SceneSetup` 各方法。

---

## License

本项目仅供学习与交流使用。
