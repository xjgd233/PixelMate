# PixelMate — Codex 首次启动指令

请读取并严格遵循项目中的 `.agents/skills/pixelmate-engineer/SKILL.md`。

现在启动 PixelMate 项目。

重要：开始工作前必须读取 `docs/phase-status.md`，并以其中记录的默认当前阶段为准。不得仅凭文件名、README 摘要或历史对话推断验收状态。

首次启动时，请先检查 macOS 开发环境、CPU 架构、.NET SDK、Git、项目目录和 Codex 当前可用工具。

然后创建项目基础结构，确定 Avalonia 与 .NET 10 的兼容性，建立 Git 仓库，完成最小化的 macOS ARM64 空白窗口运行测试。

阶段 0 的历史实现记录见 `docs/reports/phase-0-report.md`。任何后续阶段都必须遵守 `.agents/skills/pixelmate-engineer/SKILL.md` 的阶段门禁。

不要一次性创建全部功能。不要为了赶进度跳过实际运行验证。

如果本机不是 Apple Silicon，请先检查实际架构，并据此调整 macOS 运行目标；不要假设设备架构。
