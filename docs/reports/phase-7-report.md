# 阶段 7：Windows x64 正式发布

执行日期：2026-11-04

阶段状态：待验收

前置阶段：阶段 6（已验收）

## 一、完成内容

- 增加 PixelMate `1.0.0` 产品、程序集、文件和信息版本信息。
- 生成并接入统一的 PNG、ICO、ICNS 应用图标；Windows 可执行文件嵌入 ICO，
  macOS 临时应用包使用 ICNS。
- 增加 Windows `win-x64`、Release、自包含发布脚本，附带主程序及原生依赖完整性检查。
- 发布目录自动移除无关调试符号，并附带根目录 README 和 1.0.0 发布说明。
- 发布脚本关闭 Avalonia/.NET 遥测、清理旧发布目录，并直接生成不含
  macOS 元数据的 Windows ZIP。
- Windows 应用清单增加 `PerMonitorV2` 高 DPI 感知，避免系统整体位图拉伸。
- 发布目录附带 Windows 实机验收清单。
- 增加可选商业 Authenticode 签名脚本，按证书指纹签署主程序并执行时间戳与验证。
- 增加不关闭 Windows 智能应用控制的开发验收包，通过已签名的 `dotnet.exe`
  承载未签名程序；该方案只用于功能验收，需要 .NET 10 Runtime。
- 增加 Photoshop 风格的常用快捷键：
  - `M` 测量、`I` 取色、`Z` 放大镜。
  - 按住空格临时切换抓手，左键拖动平移。
  - `⌘/Ctrl+0` 适应窗口、`⌘/Ctrl+1` 恢复 100%。
  - `⌘/Ctrl++` 与 `⌘/Ctrl+-` 缩放。
  - `[` / `]` 调整放大镜倍率。
  - `⌘/Ctrl+C` 根据当前上下文复制测量结果或 HEX。
  - `Delete` / `Backspace` 清除全部测量。
- 快捷键提示在 macOS 显示 `⌘`，在 Windows 显示 `Ctrl+`。
- 修复干净还原后 Debug 构建暴露的 Avalonia 12 开发者工具挂接方式：
  从已过时的 `WithDeveloperTools()` 改为 `App.Initialize()` 中的
  `AttachDeveloperTools()`。
- 修正开发者工具 NuGet 包的 Debug 条件，避免 Release 依赖被恢复为 `_._` 空资源。

## 二、修改文件

- `README.md`
- `docs/release-notes-1.0.0.md`
- `docs/phase-status.md`
- `docs/reports/phase-7-report.md`
- `docs/windows-offline-package.md`
- `scripts/generate-app-icons.py`
- `scripts/publish-windows-x64.ps1`
- `scripts/publish-windows-x64.sh`
- `scripts/publish-windows-devhost-x64.sh`
- `scripts/sign-windows-x64.ps1`
- `scripts/windows/START-PIXELMATE.cmd`
- `scripts/windows/START-PIXELMATE-DEVHOST.cmd`
- `scripts/macos/Info.plist`
- `scripts/run-macos.sh`
- `src/PixelMate/app.manifest`
- `src/PixelMate/App.axaml.cs`
- `src/PixelMate/Program.cs`
- `src/PixelMate/PixelMate.csproj`
- `src/PixelMate/MainWindow.axaml`
- `src/PixelMate/MainWindow.axaml.cs`
- `src/PixelMate/Controls/ImageCanvas.cs`
- `src/PixelMate/Assets/PixelMate.png`
- `src/PixelMate/Assets/PixelMate.ico`
- `src/PixelMate/Assets/PixelMate.icns`

## 三、编译结果

Debug 构建：

```sh
AVALONIA_TELEMETRY_OPTOUT=1 \
/Users/admin/.dotnet/dotnet build PixelMate.slnx \
  -c Debug \
  --no-restore \
  -m:1 \
  -nodeReuse:false \
  -p:UseSharedCompilation=false \
  -v:minimal
```

结果：0 个警告，0 个错误。

Release 构建：

```sh
AVALONIA_TELEMETRY_OPTOUT=1 \
/Users/admin/.dotnet/dotnet build PixelMate.slnx \
  -c Release \
  --no-restore \
  -m:1 \
  -nodeReuse:false \
  -p:UseSharedCompilation=false \
  -v:minimal
```

结果：0 个警告，0 个错误。

测试命令：

```sh
AVALONIA_TELEMETRY_OPTOUT=1 \
/Users/admin/.dotnet/dotnet test tests/PixelMate.Tests/PixelMate.Tests.csproj \
  -c Release \
  --no-build \
  --no-restore \
  -nodeReuse:false \
  -p:UseSharedCompilation=false \
  -v:minimal
```

初次结果为 72 个测试通过；后续批注、设置、导出与字体修正追加回归后，
当前代码共 91 个测试通过，
0 个失败，0 个跳过。

Windows 发布命令：

```sh
./scripts/publish-windows-x64.sh
```

结果：成功生成 Release、`win-x64`、自包含发布目录。

## 四、实际运行测试

- 使用 `./scripts/run-macos.sh Debug` 启动最终 Debug 构建。
- 应用图标正常显示在窗口标题区和 macOS 应用包中。
- 通过真实文件选择器打开 `200 × 120` 四色透明测试 PNG。
- 初始适应窗口缩放为 `451%`，透明区域棋盘格正常。
- `M` 成功切换到测量工具；真实拖动后记录 1 条测量。
- `⌘C` 成功复制 1 条测量，状态栏显示“已复制 1 条测量结果”。
- `I` 成功切换到取色工具；点击红色区域复制 `#E04848`。
- `Z` 成功切换到放大镜；`[` / `]` 在 `8×`、`12×`、`16×` 间切换。
- `⌘1` 切换到 `100%`，`⌘0` 恢复 `451%`。
- `⌘=` 将缩放提高到 `564%`，`⌘-` 恢复到 `451%`。
- 放大到 `1101%` 后，按住空格并拖动画布，彩色象限边界按拖动方向移动，
  证明临时抓手模式生效。
- `Delete` 成功清除测量；原有测量、取色、放大镜、透明背景和状态栏状态均正常。

## 五、Computer Use 测试结果

- Computer Use 可识别应用窗口、菜单栏、工具栏、画布、状态栏和文件选择器。
- 通过真实键盘事件验证了工具切换、缩放、适应窗口、100%、复制、倍率调整、
  清除和空格临时抓手。
- 通过真实鼠标拖动验证测量，通过真实点击验证像素取色。
- 通过截图确认应用图标、画布内容、透明棋盘格、放大镜浮层和快捷键提示正常。

## 六、Windows 发布产物检查

- 发布目录：`artifacts/PixelMate-win-x64/`
- 压缩包：`artifacts/PixelMate-1.0.0-win-x64.zip`
- 离线压缩包：`artifacts/PixelMate-1.0.0-win-x64-offline.zip`
- 压缩包大小：`47,267,679` 字节
- 解压后目录大小：约 `106 MB`
- ZIP SHA-256：
  `3ca6ba0d4a4f49d6bb1267c6d35efef31ca4ba794dd33a6291020f2f931b56a9`
- `PixelMate.exe`：PE32+ GUI，x86-64。
- `PixelMate.dll`：PE32+ .NET 程序集，x86-64。
- `libSkiaSharp.dll`、`libHarfBuzzSharp.dll`、`av_libglesv2.dll`：x86-64 PE。
- `.NET 10.0.14` 运行时已包含，发布目录无需用户另装 .NET Runtime。
- 发布包新增 `START-PIXELMATE.cmd` 与 `OFFLINE-README.md`，明确自包含包与需要系统的
  `devhost` 包的用途区别。
- 发布脚本已验证必需文件存在，压缩包完整性检查无错误。
- ZIP 内已确认没有 `__MACOSX` 或 AppleDouble `._*` 元数据文件。
- 已从 `PixelMate.exe` 内确认嵌入 `dpiAware=true/pm` 与
  `dpiAwareness=PerMonitorV2` 清单。

## 七、已知问题

- 当前机器只有 macOS ARM64，没有真实 Windows x64 运行环境。
- 因此本轮只能确认 Windows 包的结构、架构、依赖和压缩完整性，不能声称启动、
  文件选择器、剪贴板、WebP、透明 PNG、高 DPI 和快捷键已在 Windows 通过。
- 自包含包约 `106 MB`，主要来自 .NET 运行时与图形原生库。本轮按项目规范没有
  启用有兼容风险的裁剪。
- 发布包未进行 Windows 代码签名；Windows SmartScreen 可能显示未知发布者提示。
- Windows 11 智能应用控制已实际拦截未签名 `PixelMate.exe`，且弹窗没有“仍要运行”
  选项。正式公开分发仍应使用商业 Authenticode 证书；开发验收包仅用于临时测试。
- 用户实机截图显示直接运行 `PixelMate-devhost-win-x64/PixelMate.exe` 会要求安装
  .NET 10 Runtime。该目录本来就是框架依赖开发包；本轮已重新生成并单独命名
  `PixelMate-1.0.0-win-x64-offline.zip`，完整包内含 .NET 10.0.14 运行时。

## 八、Git 提交记录

阶段实现提交：`88077f7`

提交信息：`feat: prepare phase 7 Windows release`

阶段状态和本报告随后续文档提交保存。

Windows 验收加固提交：`21f067b`

提交信息：`fix: harden windows release acceptance`

Windows 签名与智能应用控制兼容工具提交：`0c4a30a`

提交信息：`feat: add windows signing and devhost acceptance`

用户反馈与离线包修正提交：`3039749`

提交信息：`fix: save edited images and polish native UI`

## 九、等待用户验收

阶段 7 的代码、图标、快捷键、macOS 回归、自动化测试和 Windows x64 交叉发布均已完成。
请在真实 Windows x64 环境解压并运行
`artifacts/PixelMate-1.0.0-win-x64-offline.zip`，重点验证：

详细步骤和记录模板已随包放入 `WINDOWS-ACCEPTANCE.md`。

- 应用可启动，图标、名称和 `1.0.0` 版本正确。
- 文件选择器可打开 PNG、JPG、BMP、WebP。
- 透明 PNG 棋盘格、取色和复制正常。
- 测量、放大镜、滚轮缩放、中键/空格平移正常。
- `Ctrl+O/M/I/Z/C/0/1/+/-`、`[`/`]`、`Delete` 正常。
- 高 DPI 下画布不模糊、不偏移。

Windows 实测完成并明确确认后，才能将阶段 7 改记为“已验收”。
