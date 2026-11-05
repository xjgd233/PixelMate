# 阶段 8：批注、设置与关于

初次执行日期：2026-11-04

验收复跑日期：2026-11-05

阶段状态：已验收

前置阶段：阶段 7（待验收）

## 一、完成内容

- 新增文字批注工具，快捷键为 `T`。
- 点击图片后打开批注输入框，批注锚点保存为原始图片坐标。
- 批注可包含多行文字，使用 `⌘/Ctrl+Enter` 保存、`Esc` 取消，最多 500 字符。
- 批注以半透明圆角方框显示，缩放、平移和窗口变化后仍跟随图片；成品不绘制引线
  或锚点圆点。
- 批注编辑器支持六种快捷字体颜色、`#RRGGBB` 自定义颜色和实时预览，每条批注
  独立保存颜色并应用到画布与 PNG 导出。
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

2026-11-05 验收复跑结果：

- Release 构建 0 个警告、0 个错误。
- `PixelMate.Tests` 94 项全部通过，0 项失败，0 项跳过。

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
- 2026-11-05 阶段 8 验收复跑使用最新 `Debug` 构建：
  - 设置窗口和操作指南可正常打开，设置项、版本号和滚动内容显示完整。
  - 关于页显示 Version 1.0.0、`xjgd233`、QQ、邮箱、内置头像与微信收款码。
  - 在 `400 × 300` WebP 上通过 `T` 添加两行中文批注。
  - 撤销后批注和计数消失，前进后批注和计数恢复。
  - 切换到 100% 后，批注锚点继续对准原始图片坐标。
  - 保存得到 `/private/tmp/stage8-acceptance.png`，确认尺寸为 `400 × 300`、
    8-bit RGBA，测量/批注合成逻辑输出正常。

## 五、Computer Use 测试结果

- Computer Use 可识别首次指南、主窗口批注工具、批注输入层、设置窗口和关于窗口。
- 通过真实键盘输入、`⌘+Return` 保存、`⌘C` 复制、设置切换与重启验证完成交互。
- 通过真实标题栏双击、`⌘S`、系统保存对话框和导出结果截图验证本轮新增交互。
- 通过真实按钮点击和 `⌘Z` / `⌘⇧Z` 验证测量、清空与批注的撤销/前进流程。
- 通过截图确认批注浮层、工具栏布局、设置层级和关于页排版正常。
- 文件选择器与原有测量、取色、放大镜、缩放和平移未发现回归。
- 2026-11-05 再次通过真实键盘、鼠标和系统保存对话框完成上述阶段 8 验收复跑。

## 六、发布产物

- 发布目录：`artifacts/PixelMate-win-x64/`
- 压缩包：`artifacts/PixelMate-1.0.0-win-x64.zip`
- 离线压缩包：`artifacts/PixelMate-1.0.0-win-x64-offline.zip`
- 压缩包大小：`47,271,877` 字节
- ZIP SHA-256：
  `82a00400b65b50cc3424d758c3a16684f5102dc4fd3ca33fdf3b556623fcd657`
- Windows 主程序、Skia、HarfBuzz 和 OpenGL 原生库仍为 x86-64 PE。
- 阶段 7 验收修正后已重新发布，发布包增加 Per-Monitor V2 高 DPI 清单和
  `WINDOWS-ACCEPTANCE.md` 实机验收清单。
- 本轮仍无法在真实 Windows x64 环境运行，平台验收状态不变。

## 七、已知问题

- 已接入用户提供的 `头像.jpg` 和 `收款码.jpg` 作为内置关于页资源；
  `~/Pictures` 中匹配关键词的图片仍会优先覆盖内置资源。
- GitHub 地址已按浏览器实际账号修正为 `https://github.com/xjgd233`。
- 阶段 8 已按用户明确指令在 macOS 完成功能验收；Windows 平台回归仍与阶段 7
  一并在真实 Windows 环境中补做，不能用本报告代替阶段 7 的平台验收。
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

## 九、验收结论

- 用户于 2026-11-05 明确要求先验收阶段 8，并随后发布到 GitHub。
- 阶段 8 的 macOS 功能、保存导出、设置、首次指南和关于页已完成复核。
- 阶段 8 状态更新为“已验收”，用户验收日期为 2026-11-05。
- Windows 实机回归仍与阶段 7 的待补验项合并处理，阶段 7 保持“待验收”。

macOS 复核范围：

- `T` 批注及 `⌘/Ctrl+Enter` 保存。
- 顶部设置按钮与默认放大镜倍率保存。
- 首次启动指南及从设置重新打开。
- 关于页联系方式和布局。

上述 macOS 项目均已复核。明天的 Windows 远程验收将重点覆盖标题栏移动、
保存对话框、字体行高、批注坐标、撤销/前进和快捷键。

## 十、后续字体修正

- 用户在真实 Windows x64 环境确认阶段 8 功能除全局字体显示外均正常。
- 经 Avalonia 官方文档与 GitHub 问题 `#22464` 核实，Avalonia 12 的 Windows
  系统字体缓存可能让显式字体仍回退到其他 CJK 字体。
- 后续阶段 7 修复已改为随包内置 Noto Sans CJK SC 常规/粗体，所有平台通过
  `avares://PixelMate/Assets/Fonts#Noto Sans CJK SC` 使用同一字体族。
- 最新离线包为 `artifacts/PixelMate-1.0.0-win-x64-offline.zip`，SHA-256：
  `0378242f3bd70366e9285cf20cf72a94dc925d12670907381929538128b7a501`。

## 十一、后续批注样式修订

- 用户要求去掉批注到锚点的位置线，只保留方框；方框背景稍微透明，避免遮挡图片。
- 用户要求批注字体颜色可自行设置。编辑器现支持六种快捷色块、`#RRGGBB` 输入、
  实时颜色预览和格式错误提示。
- 每条批注独立保存字体颜色；新增批注自动继承本次运行中最后使用的颜色。
- 完成后的画布与导出 PNG 均不再绘制引线或锚点圆点，仅保留 72% 不透明的圆角方框。
- Release 构建 0 警告、0 错误。
- 2026-11-05 全量自动化测试 101 项通过。
- macOS 真实窗口验证了黄色自定义输入、蓝色快捷色块、上次颜色继承、无效颜色拒绝、
  两行多色批注显示和 PNG 导出；导出文件为
  `/private/tmp/pixelmate-annotation-style.png`。
- 阶段 8 原有用户验收状态不改写；本次视觉修订等待用户复验。
