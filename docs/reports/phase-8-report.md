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
- 新增设置窗口，可从顶部工具栏的滑杆按钮打开。
- 设置支持默认放大镜倍率、记住上次图片文件夹、每次启动显示操作指南。
- 设置通过原有原子写入机制保存到本机 `settings.json`。
- 新增首次启动操作指南，包含打开、测量、取色、放大镜、批注、缩放和平移步骤。
- 完成首次指南后记录完成状态；后续只通过设置中的“操作指南”重新打开。
- 新增关于页，显示作者、版本、GitHub、QQ、邮箱、头像和赞助码区域。
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
- `src/PixelMate/About/AboutImageLocator.cs`
- `src/PixelMate/AboutWindow.axaml`
- `src/PixelMate/AboutWindow.axaml.cs`
- `src/PixelMate/Annotations/PixelAnnotation.cs`
- `src/PixelMate/AppIdentity.cs`
- `src/PixelMate/Controls/ImageCanvas.cs`
- `src/PixelMate/MainWindow.axaml`
- `src/PixelMate/MainWindow.axaml.cs`
- `src/PixelMate/Preferences/AppPreferences.cs`
- `src/PixelMate/SettingsWindow.axaml`
- `src/PixelMate/SettingsWindow.axaml.cs`
- `src/PixelMate/TutorialWindow.axaml`
- `src/PixelMate/TutorialWindow.axaml.cs`
- `tests/PixelMate.Tests/AboutImageLocatorTests.cs`
- `tests/PixelMate.Tests/AppPreferencesTests.cs`
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

结果：83 个测试通过，0 个失败，0 个跳过。

新增回归覆盖：

- 批注文字规范化、空文本拒绝、500 字符上限和剪贴板拼接。
- 首次指南状态、完成状态与每次启动显示设置的持久化。
- 关闭目录记忆后不写入目录。
- 非法放大镜倍率回退到 12×。
- 图片文件夹关键词匹配、扩展名过滤和缺图回退。

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

## 五、Computer Use 测试结果

- Computer Use 可识别首次指南、主窗口批注工具、批注输入层、设置窗口和关于窗口。
- 通过真实键盘输入、`⌘+Return` 保存、`⌘C` 复制、设置切换与重启验证完成交互。
- 通过截图确认批注浮层、工具栏布局、设置层级和关于页排版正常。
- 文件选择器与原有测量、取色、放大镜、缩放和平移未发现回归。

## 六、发布产物

- 发布目录：`artifacts/PixelMate-win-x64/`
- 压缩包：`artifacts/PixelMate-1.0.0-win-x64.zip`
- 压缩包大小：约 `46 MB`
- ZIP SHA-256：
  `1e4a4887cb1aa356001555ba89ede233e76636cec9c8f003ab4a66ed4ce0e9c2`
- Windows 主程序、Skia、HarfBuzz 和 OpenGL 原生库仍为 x86-64 PE。
- 本轮仍无法在真实 Windows x64 环境运行，平台验收状态不变。

## 七、已知问题

- 用户表示头像和赞助码会放入图片文件夹，但截至本次测试，`~/Pictures`
  中尚未出现可识别的头像或赞助码；关于页当前显示占位提示。
- GitHub 地址暂按邮箱前缀使用 `https://github.com/xiaowenhao2333`；
  如实际主页不同，需要替换 `AppIdentity.GitHubUrl` 后重新构建。
- 阶段 7 仍在等待真实 Windows 验收，因此阶段 8 只能记为“提前实现，待补验”。
- 批注只保存在当前会话中，切换图片或关闭应用后清空，不修改源图片。

## 八、Git 提交记录

阶段实现提交：`2e1ee95`

提交信息：`feat: add annotations settings and onboarding`

阶段状态和本报告随后续文档提交保存。

## 九、等待用户验收

请检查以下阶段 8 内容：

- `T` 批注及 `⌘/Ctrl+Enter` 保存。
- 顶部设置按钮与默认放大镜倍率保存。
- 首次启动指南及从设置重新打开。
- 关于页联系方式和布局。

头像、赞助码放入图片文件夹后需再次打开关于页确认；如 GitHub 地址不是
`https://github.com/xiaowenhao2333`，也需补充正确地址。
