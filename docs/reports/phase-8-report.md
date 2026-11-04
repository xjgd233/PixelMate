# 阶段 8：批注、设置与关于

执行日期：2026-11-04

阶段状态：提前实现，待补验

前置阶段：阶段 7（待验收）

## 一、完成内容

- 新增文字批注工具，快捷键为 `T`。
- 点击图片后打开批注输入框，批注锚点保存为原始图片坐标。
- 批注可包含多行文字，使用 `⌘/Ctrl+Enter` 保存、`Esc` 取消，最多 500 字符。
- 批注以青绿色锚点、引线和浮层显示，缩放、平移和窗口变化后仍跟随图片。
- 支持多条批注、`⌘/Ctrl+C` 复制全部批注，以及 `Delete/Backspace` 清除全部批注。
- 新增“保存图片”按钮及 `⌘/Ctrl+S`，使用系统保存位置选择器输出 PNG。
- 导出的 PNG 保持原图分辨率，并将当前全部测量线、长度标签和批注合成到输出图片。
- 新增 Office 风格撤销/前进按钮及工具提示。
- 撤销历史覆盖添加测量、清空测量、添加批注和清空批注；撤销后再次编辑会丢弃
  已失效的前进分支。缩放、平移和工具切换不进入编辑历史。
- 支持 `⌘Z` / `Ctrl+Z` 撤销，`⌘⇧Z` / `Ctrl+Y` / `Ctrl+Shift+Z` 前进，并根据历史
  状态自动启用或禁用按钮。
- 主窗口自绘标题栏支持拖动移动，双击可在最大化和还原之间切换。
- 移除 Inter 字体依赖；Windows 优先使用 Segoe UI / 微软雅黑，macOS 使用
  SF Pro / 苹方，避免跨平台字体度量混用。
- 字体改为运行时按平台绑定单一完整中文字体族：macOS 使用 `PingFang SC`，
  Windows 使用 `Microsoft YaHei UI`，避免英文与中文字形来自不同字体。
- 移除单行文本的固定行高和按钮固定高度，改用系统字体自然行框、
  `MinHeight` 与垂直居中，修复 Windows 高 DPI 下文字偏挤或被裁切的问题。
- 新增设置窗口，可从顶部工具栏的滑杆按钮打开。
- 设置支持默认放大镜倍率、记住上次图片文件夹、每次启动显示操作指南。
- 设置通过原有原子写入机制保存到本机 `settings.json`。
- 新增首次启动操作指南，包含打开、测量、取色、放大镜、批注、缩放和平移步骤。
- 完成首次指南后记录完成状态；后续只通过设置中的“操作指南”重新打开。
- 新增关于页，显示作者、版本、GitHub、QQ、邮箱、头像和赞助码区域。
- 已在浏览器核实 GitHub 账号并修正为 `https://github.com/xjgd233`。
- QQ 号码支持点击复制，邮箱与 GitHub 使用系统默认应用打开。
- 关于页会从系统图片文件夹自动查找文件名包含以下关键词的图片：
  - 头像：`avatar`、`头像`。
  - 赞助码：`sponsor`、`赞助`、`收款码`、`赞赏`。
- 未找到图片时显示明确的“图片文件夹 · 头像/赞助码”占位说明。
- 保持阶段 1—7 已有图片浏览、测量、取色、放大镜、缩放、平移与发布配置。

## 二、修改文件

- `README.md`
- `docs/release-notes-1.0.0.md`
- `docs/phase-status.md`
- `docs/reports/phase-8-report.md`
- `docs/windows-acceptance-checklist.md`
- `docs/windows-offline-package.md`
- `src/PixelMate/About/AboutImageLocator.cs`
- `src/PixelMate/AboutWindow.axaml`
- `src/PixelMate/AboutWindow.axaml.cs`
- `src/PixelMate/Annotations/PixelAnnotation.cs`
- `src/PixelMate/App.axaml`
- `src/PixelMate/AppTypography.cs`
- `src/PixelMate/AppIdentity.cs`
- `src/PixelMate/Controls/ImageCanvas.cs`
- `src/PixelMate/Editing/UndoRedoHistory.cs`
- `src/PixelMate/Imaging/EditedImageExport.cs`
- `src/PixelMate/MainWindow.axaml`
- `src/PixelMate/MainWindow.axaml.cs`
- `src/PixelMate/Preferences/AppPreferences.cs`
- `src/PixelMate/PixelMate.csproj`
- `src/PixelMate/Program.cs`
- `src/PixelMate/SettingsWindow.axaml`
- `src/PixelMate/SettingsWindow.axaml.cs`
- `src/PixelMate/TutorialWindow.axaml`
- `src/PixelMate/TutorialWindow.axaml.cs`
- `tests/PixelMate.Tests/AboutImageLocatorTests.cs`
- `tests/PixelMate.Tests/AppPreferencesTests.cs`
- `tests/PixelMate.Tests/EditedImageExportTests.cs`
- `tests/PixelMate.Tests/UndoRedoHistoryTests.cs`
- `tests/PixelMate.Tests/PixelAnnotationTests.cs`

## 三、编译结果

Debug 和 Release 构建均为 0 个警告、0 个错误。

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

结果：94 个测试通过，0 个失败，0 个跳过。

新增回归覆盖：

- 批注文字规范化、空文本拒绝、500 字符上限和剪贴板拼接。
- 首次指南状态、完成状态与每次启动显示设置的持久化。
- 关闭目录记忆后不写入目录。
- 非法放大镜倍率回退到 12×。
- 图片文件夹关键词匹配、扩展名过滤和缺图回退。
- 导出图片的默认文件名和测量/批注叠加比例。
- 平台原生字体优先级，避免 Windows 误用 Apple 字体度量。
- 撤销/前进状态恢复、分支截断和历史容量限制。

## 四、实际运行测试

- 使用 `./scripts/run-macos.sh Debug` 启动真实应用窗口。
- 首次启动自动显示操作指南，滚动内容、步骤说明和关闭按钮正常。
- 关闭首次指南后重启应用，指南不再自动重复显示。
- 从设置重新打开操作指南成功。
- 打开 `200 × 120` 透明测试 PNG，按 `T` 切换批注工具。
- 点击画布打开批注输入层，坐标显示为 `X 100 Y 59`。
- 输入中文多行批注并保存，画布显示青绿色锚点和文字浮层。
- 使用 `⌘C` 复制批注，状态栏显示“已复制 1 条批注”。
- 放大到 `704%`，批注锚点继续跟随原始图片位置。
- 在设置中将放大镜默认倍率改为 `16×`，保存后状态栏显示“设置已保存”。
- 重启应用后设置仍为 `16×`，按 `Z` 后 UI 正确选中 `16×`。
- 关于页正确显示 Version 1.0.0、GitHub、QQ 与邮箱。
- 点击 QQ 号码后显示“QQ 3163170085 已复制”。
- 关于页成功加载内置头像与微信收款码；二维码边缘清晰、比例正常，没有被拉伸。
- 双击主窗口自绘标题栏成功最大化，再次双击恢复窗口尺寸。
- 在 `400 × 300` 测试 WebP 上创建 1 条测量线和 1 条两行中文批注。
- 按 `⌘S` 打开“保存更改后的图片”对话框，默认文件名为
  `pixelmate-phase4-colors-已编辑.png`，可选择保存目录。
- 保存到 `/private/tmp/pixelmate-ui-export-test.png` 后，文件识别为
  `400 × 300`、8-bit RGBA PNG；打开导出图确认测量线、长度标签和两行批注均在。
- 设置页、状态栏和数字文本的基线及行高正常。
- 启动采用 `PingFang SC` 的新版后，设置页和关于页的文字、数字与按钮内容
  均由同一字体族排版。
- 在 `400 × 300` 测试 WebP 上添加测量后，“撤销”按钮自动启用；点击后测量消失且
  “前进”按钮启用，再点击后测量恢复。
- 使用 `⌘Z` / `⌘⇧Z` 重复验证撤销和前进快捷键。
- 清空测量后执行撤销，原测量恢复；前进后再次清空。
- 添加两行批注后使用 `⌘Z` 撤销，批注从画布和计数中移除。
- 关于页显示修正后的 GitHub 账号 `xjgd233`。

## 五、Computer Use 测试结果

- Computer Use 可识别首次指南、主窗口批注工具、批注输入层、设置窗口和关于窗口。
- 通过真实键盘输入、`⌘+Return` 保存、`⌘C` 复制、设置切换与重启验证完成交互。
- 通过真实标题栏双击、`⌘S`、系统保存对话框和导出结果截图验证本轮新增交互。
- 通过真实按钮点击和 `⌘Z` / `⌘⇧Z` 验证测量、清空与批注的撤销/前进流程。
- 通过截图确认批注浮层、工具栏布局、设置层级和关于页排版正常。
- 文件选择器与原有测量、取色、放大镜、缩放和平移未发现回归。

## 六、发布产物

- 发布目录：`artifacts/PixelMate-win-x64/`
- 压缩包：`artifacts/PixelMate-1.0.0-win-x64.zip`
- 离线压缩包：`artifacts/PixelMate-1.0.0-win-x64-offline.zip`
- 压缩包大小：`47,271,670` 字节
- ZIP SHA-256：
  `dd1c0c4b85ce662c1a09766322af222b2419ee12be2c6e35da3865177f28bdee`
- Windows 主程序、Skia、HarfBuzz 和 OpenGL 原生库仍为 x86-64 PE。
- 阶段 7 验收修正后已重新发布，发布包增加 Per-Monitor V2 高 DPI 清单和
  `WINDOWS-ACCEPTANCE.md` 实机验收清单。
- 本轮仍无法在真实 Windows x64 环境运行，平台验收状态不变。

## 七、已知问题

- 已接入用户提供的 `头像.jpg` 和 `收款码.jpg` 作为内置关于页资源；
  `~/Pictures` 中匹配关键词的图片仍会优先覆盖内置资源。
- GitHub 地址已按浏览器实际账号修正为 `https://github.com/xjgd233`。
- 阶段 7 仍在等待真实 Windows 验收，因此阶段 8 只能记为“提前实现，待补验”。
- 批注只保存在当前会话中，切换图片或关闭应用后清空，不修改源图片。

## 八、Git 提交记录

阶段实现提交：`2e1ee95`

内置头像与赞助码提交：`ed33874`

用户反馈修正提交：`3039749`

提交信息：

- `feat: add annotations settings and onboarding`
- `feat: bundle author images in about page`
- `fix: save edited images and polish native UI`

阶段状态和本报告随后续文档提交保存。

## 九、等待用户验收

请检查以下阶段 8 内容：

- `T` 批注及 `⌘/Ctrl+Enter` 保存。
- 顶部设置按钮与默认放大镜倍率保存。
- 首次启动指南及从设置重新打开。
- 关于页联系方式和布局。

头像、赞助码放入图片文件夹后需再次打开关于页确认；GitHub 地址已修正为
`https://github.com/xjgd233`。

另外请复测：

- 拖动顶部标题栏移动窗口。
- 点击“保存图片”，选择位置并确认输出 PNG 的尺寸和叠加内容。
- 查看设置页、状态栏和数字行高是否正常。
