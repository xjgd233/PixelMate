# 阶段 1：主界面与 UI

执行日期：2026-11-04

阶段状态：待验收

前置阶段：阶段 0（待验收）

## 一、完成内容

- 完成紧凑的桌面工具主界面：集成标题栏、顶部工具栏、中央图片画布和底部状态栏。
- 工具栏按功能和频率收敛为三组：打开图片、测量/取色/放大镜、缩放。
- 使用统一的自绘线性图标；工具按钮支持互斥选中态，并在状态栏反馈当前模式。
- 没有图片时显示轻量空状态和主操作按钮；打开图片后显示文件名与图片内容。
- 打开、缩放、适应窗口、1:1、图片尺寸、指针坐标、HEX 占位和缩放比例均已纳入统一视觉层级。
- 深色界面采用近黑画布、分层工具栏、低对比边框、半透明选中填充和 macOS 系统蓝强调色，避免大面积荧光色与装饰性卡片。
- 修正窗口标题策略：初始显示“未打开图片”，打开文件后显示文件名，不再重复附加应用名称。
- 使用扩展客户区将 macOS 标题栏与深色 UI 融为一体，保留标准窗口按钮与拖拽行为。
- 增加 `⌘O` / `Ctrl+O` 打开图片快捷键，并与工具栏及空状态按钮复用同一流程。
- 保留阶段 2 已实现的图片打开、缩放、平移、透明棋盘和状态更新逻辑。
- 测量、取色和放大镜在阶段 1 仅提供工具入口和单选状态，不包含对应业务实现。

设计参考：

- [Apple Human Interface Guidelines — Toolbars](https://developer.apple.com/design/human-interface-guidelines/toolbars)：按功能分组、减少工具栏分组数量、优先清晰图标、减少自定义工具栏背景和过度着色、让内容本身主导颜色。
- [Apple 官方 macOS 照片使用手册](https://support.apple.com/zh-cn/guide/photos/welcome/mac)：以图片内容为视觉核心，工具栏和状态信息退居次要层级；编辑相关控制不扩展到无关常驻面板。

## 二、修改文件

- `README.md`
- `src/PixelMate/MainWindow.axaml`
- `src/PixelMate/MainWindow.axaml.cs`
- `src/PixelMate/Controls/ImageCanvas.cs`
- `docs/reports/phase-1-report.md`

## 三、编译结果

构建命令：

```sh
/Users/admin/.dotnet/dotnet build PixelMate.slnx \
  -c Debug \
  --no-restore \
  -m:1 \
  -nodeReuse:false \
  -p:UseSharedCompilation=false \
  -v:minimal
```

结果：0 个警告，0 个错误。

回归测试命令：

```sh
/Users/admin/.dotnet/dotnet test tests/PixelMate.Tests/PixelMate.Tests.csproj \
  -c Debug \
  --no-build \
  --no-restore \
  -nodeReuse:false \
  -p:UseSharedCompilation=false \
  -v:minimal
```

结果：21 个测试通过，0 个失败，0 个跳过。

## 四、实际运行测试

- 使用 `./scripts/run-macos.sh Debug` 生成并启动临时 macOS `.app`。
- 实际检查初始空状态、集成深色标题栏、工具栏三组布局和底部状态栏。
- 通过空状态中的“选择图片”按钮实际打开 1125 × 750 透明 PNG。
- 打开后标题栏和状态栏均显示 `pixelmate-test.png`，原始尺寸正确显示为 `1125 × 750`。
- 透明棋盘、图片比例、适应窗口缩放值和图片边框显示正常。
- 实际点击测量、取色工具，确认选中态互斥且状态栏反馈正确。
- 使用 `⌘O` 成功重新唤起系统文件选择器。
- 窗口进入全屏后，画布随可用区域重算缩放，工具栏与状态栏仍保持可见。

## 五、Computer Use 测试结果

- 能够按 bundle identifier 获取 PixelMate 原生窗口。
- Accessibility 树可读取标题、全部工具栏按钮、工具开关状态、缩放值、空状态操作与状态栏字段。
- 截图确认深色标题栏与工具栏连续、图标对齐、状态栏信息分区清晰、图片不被控制层遮挡。
- 空状态、图片打开、工具单选、缩放更新及 `⌘O` 均由实际 UI 操作验证。
- 测量线、像素取色和放大镜浮层尚未实现，因此没有在阶段 1 声称验证这些业务能力。

## 六、已知问题

- 阶段 1 只实现工具选择状态；测量、取色和放大镜的交互及结果显示将在各自阶段实现。
- 窗口快捷键提示在 macOS 使用 `⌘O`；Windows 文案适配将在 Windows 发布阶段处理。
- `.NET SDK` 位于 `/Users/admin/.dotnet`，新终端需使用绝对路径或加入 `PATH`。
- Avalonia 构建服务需要访问用户缓存目录，因此沙箱内构建会被拦截；使用授权后的标准命令构建通过。
- 按阶段规则未创建正式验收 Tag。

## 七、Git 提交记录

阶段提交信息：`feat: design phase 1 desktop interface`

实际提交哈希在阶段完成后由 `git log -1` 记录。

## 八、等待用户验收

阶段 1 已完成并停止。未进入新的功能阶段，不把阶段 2 已提前实现的内容视为阶段 1 验收结果，等待用户明确验收或提出界面修改意见。
