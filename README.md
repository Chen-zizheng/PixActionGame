# PixActionGame

Unity 做的 2D 像素横版动作游戏，Boss 战能完整打一场。

<img width="603" height="305" alt="战斗画面" src="https://github.com/user-attachments/assets/4866a1e0-2b61-48b7-b7bf-fcfe19235dc2" />
<img width="605" height="302" alt="菜单" src="https://github.com/user-attachments/assets/082763b4-8d8b-44cc-a618-1a160e9b1741" />

## 能玩什么

WASD 移动、空格跳、Shift 冲刺、鼠标左键近战。冲刺有 0.2 秒无敌和 0.5 秒冷却，用来穿过 Boss 的招式。

玩家 50 血，受击后有 1 秒无敌和 0.25 秒击退硬直。Boss 100 血，普攻冷却 1 秒、技能冷却 5 秒，掉血到阈值以下进虚弱状态，虚弱的窗口是打输出的时间。

## 代码

16 个 C# 脚本，1,269 行。自写的 shader 只有一个：`Assets/Shaders/SpriteFlashRed.shader`，71 行，管受击闪红的时长和颜色曲线。

```
Assets/Scripts/Player/   PlayerController（移动、跳跃、冲刺、攻击判定）、TriggerSender、血条
Assets/Scripts/Boss/     BossController（普攻与技能状态机）、招式判定、受伤闪红、血条
Assets/Scripts/Camera/   HollowKnightCamera，跟随带前瞻偏移
Assets/Scripts/MainMenu/ 背景滚动、标题呼吸、按钮悬停缩放
```

动作切换交给两个 Animator Controller（`Player.controller`、`Boss.controller`），状态机在 Animator 里配，脚本只往里喂参数。

## 跑起来

Unity 2022.3.62t7（团结引擎 1.8.5）打开仓库根目录，进 `Assets/Scenes/SampleScene.scene` 点 Play。主菜单在 `MainMenu.scene`。

## 已知问题

- 没有发布构建，现在只能开编辑器跑。
- 仓库里 shader 的行数大头不是我的。TextMesh Pro 包自带 17 个 shader、2,428 行，我写的那 71 行只占全仓 shader 代码的 3%。GitHub 的语言占比按全仓行数统计，看着 ShaderLab 很高，里面大部分是包的代码。
- 素材没有逐项记录来源，部分取自 Unity 官方示例或开源素材 [待确认: 逐个文件的来源与许可]。
- 工程正在把 `Assets/Scripts` 重构到 `Assets/PixAction`，这份目录说明对应的是已经提交的版本，重构完成后要跟着改。
