# 阶段 2：图片打开、缩放、平移

执行日期：2026-11-04

阶段状态：提前实现，待补验

前置阶段：阶段 1（实现已补齐，等待验收）

## 一、完成内容

- 支持通过文件选择器或拖拽打开 PNG、JPG、JPEG、BMP、WebP 图片。
- 图片默认按原始宽高比适应窗口并居中显示。
- 鼠标滚轮以指针所在位置为中心缩放，单次滚轮输入限制为合理步长。
- 中键拖动平移；放大后平移范围限制在图片边界内。
- 提供“适应窗口”和“100%”操作，放大/缩小按钮每次按 1.25 倍调整。
- 窗口尺寸变化时，如果处于“适应窗口”模式，会重新计算缩放并居中。
- 底部状态栏显示文件名、原始像素尺寸、指针原始像素坐标和当前缩放比例。
- 切换图片或打开失败时清空旧指针状态；损坏图片不会终止程序，也不会替换当前图片。
- 使用透明棋盘背景正确显示带透明通道的 PNG。
- 记住上次成功打开图片所在目录。
- 增加独立、可测试的画布坐标变换模块，为后续测量和取色复用原始图片像素坐标。
- 增加可重复的 macOS 临时 `.app` 启动脚本，本地包保存在 Git 忽略目录。

## 二、修改文件

- `.gitignore`
- `README.md`
- `src/PixelMate/App.axaml`
- `src/PixelMate/MainWindow.axaml`
- `src/PixelMate/MainWindow.axaml.cs`
- `src/PixelMate/PixelMate.csproj`
- `src/PixelMate/Controls/ImageCanvas.cs`
- `src/PixelMate/Imaging/ImageCanvasTransform.cs`
- `src/PixelMate/Imaging/ImageFileSupport.cs`
- `src/PixelMate/Preferences/AppPreferences.cs`
- `scripts/macos/Info.plist`
- `scripts/macos/pixelmate-launcher.sh`
- `scripts/run-macos.sh`
- `tests/PixelMate.Tests/PixelMate.Tests.csproj`
- `tests/PixelMate.Tests/ProjectSmokeTests.cs`（删除仅适用于开发机的架构断言）
- `tests/PixelMate.Tests/ImageCanvasTransformTests.cs`
- `tests/PixelMate.Tests/ImageFileSupportTests.cs`
- `tests/PixelMate.Tests/AppPreferencesTests.cs`
- `docs/reports/phase-2-report.md`

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

测试命令：

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

测试覆盖：

- 适应窗口比例与居中。
- 100% 原始尺寸。
- 指针锚点缩放。
- 50% 和 200% 缩放后的图片坐标往返。
- 平移边界。
- 窗口变化后的适应模式重算。
- 图片边缘像素判定。
- 最小和最大缩放限制。
- 支持格式与不支持格式识别。
- 最近目录持久化和损坏设置文件容错。

## 四、实际运行测试

- 使用 `./scripts/run-macos.sh Debug` 生成并启动本地临时 `.app`。
- 实际打开 1125 × 750 的透明 PNG；默认缩放为 75.3%，透明棋盘和图片内容显示正常。
- 实际打开同一图片转换出的 JPG 和 BMP；文件名、1125 × 750 尺寸及缩放状态正确。
- 点击 100% 后缩放显示为 100%。
- 点击放大两次后由 75.3% 变为 118%；修正滚轮步长后，单次滚轮缩放为 75.3% → 94.5%。
- 点击“适应窗口”恢复 75.3%。
- 调整窗口大小后，适应模式自动重算为 101%。
- 将非图片内容伪装成 `.png` 打开时，状态栏显示“无法打开图片”，原图继续保留，应用未崩溃。
- 打开对话框会回到上一次成功打开图片的目录。

## 五、Computer Use 测试结果

- Computer Use 能通过本地 `.app` 路径识别并操作 PixelMate。
- Accessibility 树能够读取打开按钮、缩放按钮、状态栏、文件名和缩放比例。
- 截图确认深色工具栏、中央画布、空状态、透明棋盘、图片比例及状态栏布局正常。
- 文件选择器、PNG/JPG/BMP 打开、适应窗口、100%、放大、滚轮缩放、窗口缩放和损坏文件提示均已实际操作。
- 当前 Computer Use 输入接口没有可配置为“中键”的跨应用拖动操作，也无法可靠模拟 Finder 到当前全尺寸窗口的跨应用拖放；因此这两条交互没有声称已通过真实 UI 操作。中键平移的坐标与边界逻辑已由单元测试覆盖，拖放处理已实现并编译通过。

## 六、已知问题

- 阶段 2 曾按用户明确指令在阶段 1 尚未实现时直接执行，并补齐图片浏览所需的最小窗口宿主；阶段 1 已在之后补做，但该顺序例外和阶段 1 的独立验收仍不能省略。
- Avalonia/Skia 图形后端支持 WebP，且文件选择器和扩展名校验已包含 WebP；本机 `sips` 没有 WebP 编码器，因此未找到可生成的 WebP 测试图片，真实 WebP 解码仍待补充样本验证。
- 中键拖动和跨应用拖放未被 Computer Use 自动化实际执行，需在用户验收时人工确认手感。
- `.NET SDK` 仍安装在 `/Users/admin/.dotnet`，新终端需使用绝对路径或加入 PATH。
- Codex 沙箱会阻止 Avalonia 构建服务写用户缓存和测试宿主创建回环套接字；构建和测试在获得对应权限后通过。
- 按阶段规则未创建正式验收 Tag。

## 七、Git 提交记录

阶段提交信息：`feat: implement phase 2 image viewer`

实际提交哈希在最终阶段报告和 `git log -1` 中给出。

## 八、等待用户验收

阶段 2 已完成并停止。未进入测量、取色或放大镜功能，等待用户明确验收。
