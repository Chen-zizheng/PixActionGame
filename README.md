# PixActionGame

用团结引擎制作的 2D 像素横版动作游戏，围绕一场 Boss 战实现移动、跳跃、近战、冲刺无敌和受击反馈。

<img width="603" height="305" alt="战斗画面" src="https://github.com/user-attachments/assets/4866a1e0-2b61-48b7-b7bf-fcfe19235dc2" />
<img width="605" height="302" alt="菜单" src="https://github.com/user-attachments/assets/082763b4-8d8b-44cc-a618-1a160e9b1741" />

## 操作与战斗

| 操作 | 按键 |
|---|---|
| 左右移动 | A / D 或方向键 ← / → |
| 跳跃 | 空格 |
| 冲刺 | 左 Shift |
| 近战攻击 | 鼠标左键 |

冲刺期间无敌；受击后有短暂无敌和击退硬直。Boss 会追踪玩家，在距离范围内切换普攻与技能，攻击判定随动画开启和关闭。

控制器脚本的默认参数为：冲刺持续 0.2 秒、冷却 0.5 秒；受击无敌 1 秒、击退硬直 0.25 秒。具体数值可在 Inspector 中调整。

## 运行

工程记录的版本为**团结引擎 1.8.2（2022.3.62t4）**。场景使用 `.scene` 格式，建议使用对应版本的团结引擎打开。

1. 在团结引擎 Hub 中添加仓库根目录。
2. 打开 `Assets/Scenes/MainMenu.scene`，点击 Play，从菜单进入游戏。
3. 也可以直接打开 `Assets/Scenes/SampleScene.scene` 测试战斗。

目前没有发布独立试玩包，需要通过编辑器运行。

## 代码结构

| 目录 | 内容 |
|---|---|
| `Assets/Scripts/Player/` | 移动、跳跃、冲刺、攻击判定与玩家血条 |
| `Assets/Scripts/Boss/` | Boss 行为、普攻与技能判定、受伤闪红与血条 |
| `Assets/Scripts/Camera/` | 带前瞻偏移的跟随镜头 |
| `Assets/Scripts/MainMenu/` | 菜单跳转、背景滚动、标题与按钮动效 |

角色动作由 `Player.controller` 和 `Boss.controller` 配合脚本参数及动画事件切换。受击闪红使用 `Assets/Shaders/SpriteFlashRed.shader`。

## 素材与来源

TextMesh Pro 附带的 shader 属于第三方包，不计作本项目原创实现。

美术等素材的逐项来源与许可尚未整理完整，原有记录提到 Unity 官方示例及开源素材，但具体文件仍需核对。**仓库公开不代表素材可以自由再分发或商用**；使用前请确认对应授权。
