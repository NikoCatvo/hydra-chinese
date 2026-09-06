# Hydra
<div align="center">
  <img src="https://github.com/MrDiamond64/Hydra/blob/main/img/main.png?raw=true" alt="显示 Hydra 玩家 UI 的截图"/>
</div>

Hydra 是一个基于 [BepInEx](https://github.com/BepInEx/BepInEx) 的 Among Us 模组，旨在增强 Among Us 的游戏体验。Hydra 增加了生活质量功能、有趣的恶搞功能，以及一个反作弊系统来检测大厅中作弊的玩家。

我们有一个 Discord 服务器，欢迎加入交流、寻求帮助或提供建议：https://discord.gg/N7azGPHm5F

# 功能特性
- 零 AI 生成代码
- 游戏内通知
- 显示幽灵玩家的聊天消息（对管理员判断玩家是否公平游戏很有用）
- 始终可见的聊天按钮
- 作为房主时可翻转 Skeld 地图
- 作为房主时始终成为内鬼
- 玩家颜色随机化
- 传送器
- 作为船员也可破坏和关门
- 设备和版本伪装
- 查看其他玩家的角色
- [无需房主权限即可将玩家从大厅封禁](https://streamable.com/wtb7jl)
- [获得不死之身](https://streamable.com/k1b0m0)
- [可配置的反作弊系统，检测常见作弊和漏洞利用](https://github.com/MrDiamond64/Hydra?tab=readme-ov-file#hydra-anticheat)
- 以及更多！

# Hydra 反作弊
Hydra 反作弊可能是这个模组的核心。它能检测玩家何时试图作弊，例如在没有相应角色的情况下进入通风管道，或试图在地图上传送。检测到作弊者后，Hydra 能自动将该玩家从大厅封禁。你不必是大厅房主也能使用 Hydra 反作弊，非房主使用时只会发送通知而不会封禁玩家。Hydra 反作弊旨在扩展原版 Among Us 反作弊，增加其目前未检测的作弊手段检查，只要遵循基准要求，也可用于反作弊较宽松的自定义 Among Us 服务器。

Hydra 反作弊附带一个基础基准：后端服务器必须能防止玩家冒充。如果作弊者能代表其他玩家发送 RPC，Hydra 反作弊就无法准确判断谁在作弊，可能会误判无辜玩家。原版 Among Us 服务器已自带冒充检查，因此在官方服务器上这通常不是问题。

# 安装与使用
> [!WARNING]
> 使用 Hydra 前，请务必阅读并完全同意 [免责声明](https://github.com/MrDiamond64/Hydra?tab=readme-ov-file#disclaimer) 部分的警告。

## 安装 BepInEx
[BepInEx](https://github.com/bepinex/bepinex) 是一个模组框架，允许为 Unity 游戏创建模组。Among Us 使用 Unity 作为游戏引擎，因此 Hydra 使用 BepInEx 模组框架来修改游戏。在下载 Hydra 之前，你首先需要安装 BepInEx。BepInEx 有许多变体，我们特别需要 il2cpp 版本，因为 Among Us 使用 il2cpp 编译器实现跨平台兼容。你可以从 [Releases](https://github.com/MrDiamond64/Hydra/releases) 标签页下载 BepInEx，或者从 [BepInEx 官网](https://builds.bepinex.dev/projects/bepinex_be) 获取 BepInEx 二进制文件。你可能会注意到 BepInEx Il2Cpp 有两种架构：x86 和 x64，这一点很重要，因为 Among Us 根据下载来源不同可能是 32 位或 64 位。BepInEx 的架构必须与你的 Among Us 安装架构匹配。通常来说，Microsoft Store 和 Epic Games 提供 Among Us 的 x64 版本，而 Steam 和 Itch.io 提供 x86 版本。如果仍不确定，可按 `ctrl` + `shift` + `esc` 打开任务管理器，在运行进程列表中找到 Among Us，查看是否显示 "Among Us.exe (32-bit)"，若显示则为 32 位版本，否则为 64 位版本。

下载适合你 Among Us 版本的 BepInEx 后，打开 Among Us 安装目录（`Among Us.exe` 和 `GameAssembly.dll` 所在目录），将 BepInEx 的内容解压到该目录。在 `Among Us.exe` 旁边，你应该能看到 `winhttp.dll`、`BepInEx` 和 `dotnet` 等新文件和文件夹。如果没看到这些文件，可能是你的解压工具将它们解压到了单独的文件夹，请打开新建的文件夹，将所有内容拖拽到 `Among Us.exe` 所在的目录。假设一切操作正确，BepInEx 应已安装完成，你现在可以下载 Hydra 了。

## 安装 Hydra
下载 Hydra 只需前往 [Releases](https://github.com/MrDiamond64/Hydra/releases) 标签页，下载 `HydraMenu.dll` 文件，并将其复制粘贴到 `./BepInEx/plugins/` 目录中。完成后打开 Among Us。安装 BepInEx 后首次打开 Among Us 会与平时不同，启动时间可能会长很多，且会出现一个终端窗口。这完全正常，启动延迟是 BepInEx 预检过程的一部分。后续启动 Among Us 将不再有此延迟。稍等片刻，Among Us 应该会打开，屏幕右上角会出现一个模组图标，此时 Hydra 已准备就绪，你可以开始畅玩了！

## 使用 Hydra
按键盘上的 `Insert` 键即可打开 Hydra UI。视键盘类型而定，可能需要切换 Num Lock 或同时按功能键和 Insert 键才能显示菜单。按下 Insert 后，你应该能看到 Hydra UI。Hydra UI 包含多个部分：分区面板和功能面板。分区面板会有标签页列表，如 `Self`、`Host` 和 `Anticheat`。点击任意标签页会在功能面板显示该分区的功能。功能面板包含滑块、按钮和复选框，可用于配置 Hydra。

# 待办事项
- [x] 完善反作弊，增加更多检查（如船员破坏）
- [ ] 探索模组版原版协议，其似乎拥有更宽松的反作弊
- [x] 可保存的配置

# 免责声明
> [!CAUTION]
> **Hydra 在任何情况下都绝不应被用于损害其他玩家的体验。如果你使用某些恶搞功能，请确保在经所有玩家同意的私人大厅中使用。你可以在公共大厅开启 Hydra，前提是你以改善自己的 Among Us 游戏体验为目的。权力越大，责任越大！**

我认识到像 Hydra 这样的实用模组会为恶意用户在大厅制造破坏敞开大门。我已通过从公开版本中移除强大且易被滥用的功能，并添加防护措施来限制滥用。即使有这些保护，滥用和恶意行为的可能性始终存在。我只能请求你——使用 Hydra 的人——请勿将 Hydra 用于恶意目的，并遵守 [Innersloth 行为准则](https://www.innersloth.com/code-of-conduct/) 及你所在大厅的规则。仅在公共大厅用于检测作弊者，或在所有人同意使用 Hydra 高级功能的情况下使用。

如果你不遵循我的建议，请不要指望获得任何形式的支持或免责。你的账号也可能被 Innersloth 制裁，导致失去 Among Us 账号及其关联的所有数据，如好友列表、解锁的装饰、购买记录、豆子和硬币等。

本模组与 Among Us 或 Innersloth LLC 无关，其内容未获 Innersloth LLC 认可或赞助。其中部分材料为 Innersloth LLC 所有。© Innersloth LLC.