# PixelMate

轻量级图片像素测量和取色工具。

当前进度：阶段 0—6 已验收；阶段 7（Windows x64 正式发布）处于待验收；
阶段 8（批注、设置与关于）已提前实现，待补验。

说明：阶段 0—6 已于 2026-11-04 经用户验收。
统一状态以 [`docs/phase-status.md`](docs/phase-status.md) 为准。

当前版本：`1.0.0`

## 技术栈

- .NET 10 LTS
- C# 14
- Avalonia 12.1.5
- xUnit 2.9.3

当前阶段支持：

- 深色主窗口、原生感扩展标题栏、三组工具栏和分栏状态栏。
- 打开、测量、取色、放大镜、批注工具入口及清晰单选状态。
- 空画布引导、图片标题、尺寸、坐标、颜色和缩放状态展示。
- `⌘O` / `Ctrl+O` 打开图片。
- Photoshop 风格工具快捷键：`M` 测量、`I` 取色、`Z` 放大镜、`T` 批注。
- 按住空格临时切换抓手拖动；`⌘/Ctrl+0` 适应窗口，`⌘/Ctrl+1` 恢复 100%。
- `⌘/Ctrl++`、`⌘/Ctrl+-` 缩放，`[` / `]` 调整放大镜倍率。
- `⌘/Ctrl+C` 按当前工具复制测量结果、批注或 HEX；`Delete` / `Backspace` 清除当前结果。
- 点击图片添加文字批注，批注锚定原始图片坐标，并随缩放和平移保持位置。
- 批注编辑框支持 `⌘/Ctrl+Enter` 保存和 `Esc` 取消，不上传或改写源图。
- 设置面板支持放大镜默认倍率、是否记住上次文件夹、是否每次启动显示指南。
- 首次启动自动显示操作指南，之后可从设置再次打开。
- 关于页展示内置作者头像、GitHub、QQ、邮箱与微信赞助码。
- 关于页会从系统图片文件夹读取文件名包含 `avatar` / `头像` 和
  `sponsor` / `赞助` / `收款码` / `赞赏` 的图片。
- 通过文件选择器或拖拽打开 PNG、JPG、JPEG、BMP、WebP。
- 鼠标滚轮以指针位置为中心缩放。
- 中键拖动平移。
- 适应窗口、恢复 100% 原始尺寸。
- 窗口尺寸变化时保持适应模式。
- 记住上次打开图片所在目录。
- 左键拖动测量原始图片像素距离，画布上以 `130.79px` 这类最简格式实时显示长度。
- 拖动时按住 `Shift` 自动锁定为水平或垂直测量。
- 同时保留多条测量线，支持复制全部结果和清除。
- 复制的测量文本仍保留水平距离、垂直距离和起止图片坐标。
- 测量坐标不受缩放、平移和窗口调整影响。
- 悬停实时读取原始图片像素，显示 HEX、RGB 或 RGBA 和透明颜色预览。
- 左键点击复制 HEX，状态栏可分别复制 HEX 与 RGB。
- 未预乘 BGRA 解码保留透明和半透明像素的原始通道。
- 放大镜在鼠标附近显示最近邻像素视图、像素网格、中心十字和当前像素强调框。
- 放大镜支持 `8×`、`12×`、`16×` 三档倍率，并在浮层中同步显示图片坐标与 HEX。
- 放大镜浮层会根据窗口边缘自动换边和收拢，不遮挡指针所在位置。
- 图片在后台线程解码，打开大图时界面保持响应；连续打开时只应用最后一次请求。
- 损坏或不支持的图片会在状态栏给出明确提示，并保留当前已打开的图片。
- 单张图片限制为边长不超过 32767 像素、总像素不超过 6400 万，避免异常文件耗尽内存。
- 读取偏好设置时采用临时文件原子替换，降低异常退出造成配置损坏的风险。
- 当前平台的 Skia 原生解码库会随开发构建自动复制，避免“编译成功但运行时无法解码”。
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

项目按阶段状态账本逐段开发并验收。阶段 8 是用户明确新增的扩展阶段，其顺序状态记录为
“提前实现，待补验”，不回写或代替阶段 7 的 Windows 验收。

## Windows x64 发布

macOS 仓库根目录：

```sh
./scripts/publish-windows-x64.sh
```

Windows PowerShell：

```powershell
.\scripts\publish-windows-x64.ps1
```

输出目录：

```text
artifacts/PixelMate-win-x64/
```

该目录采用 `win-x64`、Release、自包含发布，用户无需另装 .NET Runtime。
发布脚本会检查主程序、图形原生库，并复制根目录 README 与
[`docs/release-notes-1.0.0.md`](docs/release-notes-1.0.0.md) 发布说明。

当前 Windows 包已从 macOS 成功交叉发布并完成 PE 架构与依赖检查，但尚未在真实
Windows 机器上运行。阶段 7 必须完成 Windows 启动、文件选择器、剪贴板、WebP、
透明 PNG、高 DPI 和快捷键验证后才能验收。
