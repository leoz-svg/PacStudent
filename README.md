# PacStudent — Assessment 4

一款使用 Unity 制作的 2D 迷宫吃豆游戏。清空地图中的豆子、躲避四只行为不同的幽灵，并在第二关利用能量脉冲创造逃生和得分机会。

本项目由原来的 Pacstudent_Assess3 继续开发，当前版本已更新到 Assessment 4，并包含按住移动、可区分的受惊幽灵和独立通关提示。

## 下载与玩法

- **[点击在线试玩](https://leoz-svg.github.io/PacStudent/)**：使用带键盘的电脑打开，点击加载游戏即可体验。

- **[下载 Windows 游戏](https://github.com/leoz-svg/PacStudent/releases/latest)**：进入发布页，下载 `PacStudent-Windows.zip`，完整解压后运行 `PacStudent.exe`。无需安装 Unity。
- **[详细游戏玩法说明](docs/GAMEPLAY.zh-CN.md)**：包含操作、计分、幽灵行为、关卡机制和常见问题。
- **[源码和实现说明](README_COMPLETION.md)**：适合在 Unity 中打开和继续开发。

提供 Windows 64 位下载版和 WebGL 浏览器版。网页发布文件位于 docs/，需要通过 HTTP/HTTPS 访问；不要双击 HTML 直接打开。

## v1.4.0：主菜单与柔和音效

首页 START GAME → 关卡选择，BACK / Esc 返回；主页保留 SETTINGS。移除持续移动/吃豆循环，改为收豆时触发短促音效，避免声部叠加。

![主菜单](Validation/Menu/home.png)

![关卡选择](Validation/Menu/levels.png)

## v1.3.0：独立地图、暂停与结算

第二关换为双环迷宫：安全绕行与中央危险捷径相结合，235 颗豆子全部可达。两关支持 Esc / PAUSE 暂停、调音乐和重开；通关或失败后展示本局统计，等待选择再来一局或返回菜单。[更新详情](docs/EXPANSION_v1.3.md)。

![独立地图](Validation/Expansion/1280x720/map.png)

![暂停菜单](Validation/Expansion/1280x720/pause.png)

![新版结算页](Validation/Expansion/1280x720/victory.png)

## 第二关：能量追猎

连续收豆提升至 ×2 / ×3 得分，每 30 秒经历收集、预警和集中追猎。50 能量可用于应急脉冲，攒满 100 则释放范围更大的强化脉冲。界面显示波次、连击、剩余豆子及冷却；第一关继续保留经典玩法。[第二关详细说明](docs/LEVEL2_ENERGY_HUNT.md)。



## 最新版实机画面

以下截图来自 Unity Play Mode 的自动验证。

### 全局声音与连贯音乐（v1.3.1）

SETTINGS 和暂停菜单现在统一控制全部音乐与音效，开关和音量会保存。Neon Maze 主题包含开局、常态、惊吓和返回变奏，状态切换保持音乐位置并淡入淡出。

### 四种受惊幽灵

幽灵保留各自外形，编号旁的 `!` 表示可以捕捉；恢复阶段显示 `!!` 并闪烁。

![四种受惊幽灵](Validation/Revision/1280x720/distinct-scared-ghosts.png)

### 通关

收集完全部普通豆与能量豆后，显示 GAME COMPLETE! / CONGRATULATIONS!。

![通关界面](Validation/Expansion/1280x720/victory.png)

### 失败

生命耗尽时显示 GAME OVER。

![失败界面](Validation/Expansion/1024x768/defeat.png)

## 快速操作

| 操作 | 按键 |
|---|---|
| 移动 | 按住 WASD 或方向键 |
| 停下 | 松开方向键，走完当前一格后停止 |
| 第二关能量脉冲 | 空格：50 能量普通脉冲；满 100 能量强化脉冲 |
| 暂停 / 继续 | Esc / PAUSE |
| 返回菜单 / 重开 | 暂停菜单内选择 |

每局初始 3 条生命。普通豆 10 分、能量豆 50 分、樱桃 100 分、捕捉受惊幽灵 300 分。两个关卡各自保存本机最高分及对应时间。

## 打开源码

1. 克隆或下载本仓库。
2. 在 Unity Hub 中添加包含 `Assets`、`Packages`、`ProjectSettings` 的项目根目录。
3. 使用 **Unity 6000.6.0f1** 打开，等待导入完成。
4. 打开 `Assets/Scenes/StartScene.unity`，点击 Play。

`Library` 等缓存不随仓库发布，首次打开时会自动生成。原始项目使用 2023.2.10f1；当前版本按项目所有者的选择升级到 Unity 6.6。

## 验证记录

v1.3.1 音频更新通过 17 项专项检查：[音频报告](Validation/Audio/audio-tests.txt)。此前地图与流程测试见 [v1.3 记录](Validation/Expansion/README.md)。


本次三项反馈修改在 1280×720、1024×768 下各通过 19 项检查，报告均为 `TOTAL_ERRORS=0`。覆盖移动逻辑、受惊状态和胜负结算；移动测试直接调用逻辑，不模拟物理键盘输入。Windows 64 位构建成功，并完成无图形启动检查。

- [1280×720 最新报告](Validation/Revision/1280x720/revision-tests.txt)
- [1024×768 最新报告](Validation/Revision/1024x768/revision-tests.txt)

`Validation` 中其他分辨率目录是此前版本的基线记录，包含旧版持续移动规则；最新修改以 `Validation/Revision` 为准。

本次完善工作包含 AI 辅助实现。课程提交时请按课程要求填写声明；按住移动和胜负文字是项目所有者选择的玩法调整。
