# 在 Unity 中直接编辑霓虹突围

当前工程包含真实的关卡Prefab、Transform、SpriteRenderer、BoxCollider2D以及战斗配置资产。运行时读取这些资产；平台、出口、道具、机关的位置不再写在运行代码里，人物、枪械和敌人图片也不再在试玩运行时生成。

## 入口

1. 用 Unity 2022.3.62f3c1 打开 `UnityProject/`。
2. 打开 `Assets/Scenes/Main.unity`。
3. 菜单选择 **霓虹突围 → 关卡工作台**。
4. 选择关卡，点击 **载入到Scene视图**。工作台会将对应Prefab放进场景，并在Scene视图框选地图。

新版源码压缩包与Git仓库采用相同目录结构，解压后同样打开其中的 `UnityProject/`。

Hierarchy中的 **关卡编辑工作台** 包含当前关卡。切换到Scene视图、开启2D，可直接选中其中的平台、出生点、出口、任务道具、机关和Boss出生点。选中对象后，按 **W** 拖动、**R** 缩放，或修改Inspector的Position/Scale。Ctrl+D复制，Delete删除，Ctrl+Z撤销。

## 调布局和任务

| 要改的内容 | 修改位置 |
| --- | --- |
| 平台位置、宽度、高度 | 平台对象的Transform；BoxCollider2D定义对应可站立矩形 |
| 玩家出生位置、出口位置 | 玩家出生点、关卡出口对象的Position；出生点与出口的原点均在脚下 |
| 道具数量和位置 | 增删/移动任务道具对象；收集关的目标总数按对象数量计算 |
| 机关位置和操作顺序 | 移动机关对象；order为0开始的编号；关卡根对象的switchOrder填写编号顺序 |
| 地图大小、地面高度 | 关卡根对象的mapWidth、mapHeight、floorHeight |
| 刷怪总数、波次间隔 | 根对象的enemyQuota、firstWaveDelay、waveInterval |
| 额外布置敌人 | 工作台添加EnemySpawn，选择enemyKind，再移动该出生点 |
| Boss位置和生命 | Boss出生点Position；根对象的hasBoss、bossHealth |
| 任务类型 | 根对象goal：ClearEnemies清敌、CollectItems收集、Circuit机关、BossAndCircuit守卫+机关 |
| 关卡名称、任务文字、背景、平台边线色 | 根对象的displayName、mission、background、accent |

平台目前使用轴对齐矩形碰撞，请保持Z旋转为0。关卡根对象保持原点、单位缩放和零旋转，移动/缩放内部部件。纵向地图改完平台高度或跳跃数值后，需试玩确认能到达。机关编号须连续且不重复；增加机关后，也需将它加入switchOrder。编辑关卡数量仍为八关；新增任务类型/职业技能类型需要扩展行为代码。

点击 **检查布局与任务配置** 可检查出生点、出口、任务与机关编号。点击 **保存关卡：应用到Prefab并保存场景**，将修改写入共享资源。Prefab保存在 `Assets/Resources/Authoring/Levels/Sector01.prefab` 至 `Sector08.prefab`，也可以直接双击对应Prefab在Prefab模式编辑并保存。

点击 **试玩当前编辑关**，进入Play后按正常出击流程选择职业和枪械，将从当前关开始。此开关仅影响Unity编辑器；Windows试玩始终从第一关开始。工作台可恢复从第一关开始测试。

**场景覆盖与正式保存的区别：** 在Main场景拖动关卡对象后，即使尚未应用Prefab，也可用于本机Play测试；发布构建读取已保存的Prefab。要把布局交给其他人或发布，务必点击工作台的保存关卡。退出Play前的临时运行状态不会自动保存；停止Play后再改正式对象与配置。

## 调战斗参数

工作台点击 **打开战斗数值与职业/枪械贴图配置**，或选中 `Assets/Resources/Authoring/Balance.asset`。

- **guns** 固定顺序：步枪、霰弹、狙击、机枪、冲锋枪。可改名称、damage伤害、interval射击间隔、speed弹速、lifetime存活时间、pellets弹丸数、pelletAngle散射角、randomSpread随机散布、penetration穿透、distanceFalloff衰减、usesHeat过热、heatPerShot单次热量、muzzleDistance枪口位置、displayWidth外观宽度、color弹道/特效/UI颜色、artwork图片。
- **roles** 固定顺序：突击、医疗、工程、侦察。duration持续时间、cooldown冷却、moveSpeed移动速度。医疗兵healing控制治疗量；工程师turretDamage/turretInterval控制炮台；侦察兵scanRadius/scanMultiplier控制扫描半径与倍率。技能类型保持原有逻辑。torso/head分别为可替换的躯干和头部图片。
- **difficulties** 固定顺序：简单、中等、困难。health、damage和attackInterval分别控制敌人生命、伤害和攻击间隔倍率。
- **玩家运动**：startingHealth、jumpVelocity、gravity、dashSpeed、dashDuration、dashCooldown等。

职业技能数字、枪械伤害/间隔、难度倍率页面随配置显示。字段中带数值的资源文件修改后，重新开始一次测试以得到完整一致的状态。不要删除或打乱固定数量的职业、枪械和难度条目。

## 换图片和放装饰

`Assets/Resources/Authoring/Art/` 内有实际PNG：四职业躯干/头部、五类枪械、敌人、无人机、Boss、地面与平台材质。修改PNG，或将新Texture2D拖入Balance/Campaign的对应字段，便可更换外观。透明像素图建议Point过滤、无mipmap。场景背景也可在每关根对象单独指定。

工作台 **打开战役美术配置** 对应 `Campaign.asset`：street、catwalk、drone、boss、guardTorso/guardHead可替换场景与敌人图片。portraitModels为旧轮廓预览兼容资源；实际人物预览使用roles的torso/head。不要清空必需的图片引用。

每个平台可单独指定platformArtwork。添加Decoration对象后，可在它的SpriteRenderer中指定Sprite、Color、Flip，使用Transform移动、缩放和绕Z旋转。导入这类装饰图片时将Texture Type设为Sprite (2D and UI)、Sprite Mode设为Single；运行画面读取其位置、尺寸、旋转、图片和颜色。装饰无碰撞，平台与交互逻辑仍由对应的StageElement类型决定。

Scene视图中的彩色标识用于摆放，游戏仍使用现有像素美术表现。当前HUD和技能行为仍由脚本实现；本轮开放的是关卡部件、任务/波次配置、战斗数值和可替换图片。并未把整套HUD改为可拖拽的Canvas界面。

## 版本控制与验证

保存关卡/配置后，在仓库根目录查看 `git status`，提交 `.prefab`、`.asset`、PNG及其 `.meta`；不要提交Library、Logs和UserSettings。Unity资源的.meta必须随资源一起提交。

默认配置保留现有战斗数值与八关布局。验证包括Prefab修改/保存/重新载入，以及实际运行中修改平台位置/尺寸、出生点/出口、道具数量、机关顺序、枪械参数、技能参数、难度倍率后的结果。现有八关、五枪与技能流程也继续运行回归。验证为自动检查，修改后的关卡路线仍需人工试玩。

`NeonAuthoringMigration`只是首次迁移工具，只补缺失资产，不会在普通启动或构建时重建关卡、覆盖人工摆放。运行时不再生成这些PNG美术资源。
