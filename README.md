# PixelMate

轻量级图片像素测量和取色工具。

当前进度：阶段 0—2 已完成开发，均等待用户验收；阶段 2 属于提前实现。

说明：阶段 2（图片打开、缩放、平移）此前按用户明确指令直接实施；阶段 1 已在之后补齐视觉系统与主界面。统一状态以 [`docs/phase-status.md`](docs/phase-status.md) 为准。

## 技术栈

- .NET 10 LTS
- C# 14
- Avalonia 12.1.5
- xUnit 2.9.3

当前阶段支持：

- 深色主窗口、原生感扩展标题栏、三组工具栏和分栏状态栏。
- 打开、测量、取色、放大镜工具入口及清晰单选状态。
- 空画布引导、图片标题、尺寸、坐标、颜色和缩放状态展示。
- `⌘O` / `Ctrl+O` 打开图片。
- 通过文件选择器或拖拽打开 PNG、JPG、JPEG、BMP、WebP。
- 鼠标滚轮以指针位置为中心缩放。
- 中键拖动平移。
- 适应窗口、恢复 100% 原始尺寸。
- 窗口尺寸变化时保持适应模式。
- 记住上次打开图片所在目录。
- 不修改源图片，不上传图片。

## 环境要求

- macOS 15 或更高版本
- Apple Silicon ARM64（当前开发目标）
- .NET SDK 10.0.403 或同一 10.0 feature band 的更新版本

如果 .NET SDK 安装在 `~/.dotnet`，请先加入当前 shell：

```sh
export PATH="$HOME/.dotnet:$PATH"
```

## 构建与运行

```sh
dotnet restore PixelMate.slnx
dotnet build PixelMate.slnx
dotnet test PixelMate.slnx
dotnet run --project src/PixelMate/PixelMate.csproj
```

macOS 临时应用包启动方式：

```sh
./scripts/run-macos.sh Debug
```

脚本会把本地构建复制到 Git 忽略的 `.local/PixelMate.app` 后启动，便于 LaunchServices
和 Computer Use 正常识别窗口。

项目必须按 `.agents/skills/pixelmate-engineer/SKILL.md` 的八个阶段逐段开发并验收。
