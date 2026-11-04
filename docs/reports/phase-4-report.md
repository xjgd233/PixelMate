# 阶段 4：颜色吸取系统

执行日期：2026-11-04

阶段状态：待验收

前置阶段：阶段 3（已验收）

## 一、完成内容

- 取色工具启用后，鼠标悬停会实时读取原始图片像素。
- 画布指针附近显示棋盘底色块、HEX 和 RGB 或 RGBA 预览。
- 状态栏同步显示颜色预览和当前 HEX，并保留鼠标原始图片坐标。
- 左键点击图片会复制当前 HEX。
- 状态栏提供独立的 HEX 和 RGB 复制按钮，并显示明确的复制成功反馈。
- 不透明颜色使用 `#RRGGBB` 与 `rgb(r, g, b)`。
- 半透明或全透明颜色使用 `#RRGGBBAA` 与 `rgba(r, g, b, a)`，保留原始 RGB 与 Alpha。
- 图片通过 Skia 解码为未预乘 BGRA，显示与取色共用同一批原始像素，避免 Alpha 预乘造成通道误差。
- 鼠标离开画布后保留最近一次取色结果，便于继续点击状态栏复制按钮。
- 测量浮层简化为直接的 `130.79px` 长度文本，同时减细线段、端点、阴影和标签间距。
- 测量复制文本继续保留 ΔX、ΔY 与起止坐标，不因浮层简化丢失数据。
- 切换到其他工具、打开新图片或关闭窗口时清理取色状态和像素缓冲。
- 保留阶段 0—3 的图片打开、缩放、平移、测量和异常文件容错。

## 二、修改文件

- `README.md`
- `src/PixelMate/Colors/PixelColor.cs`
- `src/PixelMate/Controls/ImageCanvas.cs`
- `src/PixelMate/Imaging/ImagePixelBuffer.cs`
- `src/PixelMate/Imaging/OriginalImageDecoder.cs`
- `src/PixelMate/Measurement/PixelMeasurement.cs`
- `src/PixelMate/MainWindow.axaml`
- `src/PixelMate/MainWindow.axaml.cs`
- `tests/PixelMate.Tests/ImagePixelBufferTests.cs`
- `tests/PixelMate.Tests/PixelColorTests.cs`
- `tests/PixelMate.Tests/PixelMeasurementTests.cs`
- `docs/phase-status.md`
- `docs/reports/phase-4-report.md`

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

结果：44 个测试通过，0 个失败，0 个跳过。

取色专项覆盖：

- BGRA 通道顺序与坐标读取。
- 带行填充的像素缓冲。
- 图片边界外的坐标拒绝。
- 输入缓冲区所有权与长度校验。
- 不透明颜色的六位 HEX 和 `rgb(...)`。
- 半透明颜色的八位 HEX 和 `rgba(...)`。
- 全透明颜色保留原始 RGB 通道。

## 四、实际运行测试

- 使用 `./scripts/run-macos.sh Debug` 生成并启动本地临时 `.app`。
- 打开 400 × 300 的四象限测试 PNG，适应窗口缩放为 180%。
- 红色像素读取为 `#FF0000` 和 `rgb(255, 0, 0)`。
- Alpha 为 128 的绿色像素读取为 `#00FF0080` 和 `rgba(0, 255, 0, 0.502)`。
- Alpha 为 0 的蓝色像素读取为 `#0000FF00`，原始蓝色通道未丢失。
- Alpha 为 64 的青蓝色像素区域正确显示在透明棋盘上。
- 在画布左键点击后，状态栏显示“已复制 #00FF0080”。
- 点击状态栏 RGB 按钮后，状态栏显示“已复制 rgba(0, 255, 0, 0.502)”。
- 点击状态栏 HEX 按钮后，状态栏显示“已复制 #00FF0080”。
- 切换到测量工具后，取色面板、浮动颜色卡和 HEX 状态均被清理。
- 回归打开 JPG 与 BMP，图片尺寸、缩放和取色正常。
- WebP 回归读取半透明绿色仍为 `#00FF0080`。
- 打开损坏 PNG 时保留当前有效图片，并显示友好的打开失败提示。
- 实际绘制斜向测量线后，画布只显示 `130.79px`；线条与端点比阶段 3 更细，视觉层级更轻。

## 五、Computer Use 测试结果

- Computer Use 可识别取色开关、HEX/RGB 复制按钮、画布和状态栏数值。
- 通过真实鼠标移动读取了红色、半透明绿色、全透明蓝色和 WebP 像素。
- 通过真实点击验证了画布 HEX 复制与状态栏 HEX/RGB 复制反馈。
- 通过真实工具栏点击验证了取色状态清理。
- 通过真实文件选择器回归 PNG、JPG、BMP、WebP 和损坏图片。
- 自绘浮动颜色卡通过截图检查；数值同时通过状态栏可访问文本交叉验证。

## 六、已知问题

- 透明颜色的 HEX 复制采用 CSS 八位格式 `#RRGGBBAA`；不透明颜色仍使用常见的六位格式。
- 状态栏颜色浮层与工具按钮共用紧凑布局，极窄窗口下会优先压缩中间状态文字。
- 本轮验证覆盖 macOS ARM64；Windows 的剪贴板、文件选择器和渲染行为留待阶段 7 真实验收。

## 七、Git 提交记录

阶段实现提交：`6c78323`

提交信息：`feat: implement phase 4 color picker`

验收反馈提交：`97afea9`

提交信息：`style: simplify measurement overlay`

## 八、等待用户验收

阶段 4 的代码、构建、单元测试、四格式回归和真实 UI 测试均已完成。当前状态为
“待验收”，等待用户明确确认。

阶段 5（像素放大镜）尚未启动，本次工作不得解释为阶段 4 已验收或阶段 5 已获授权。
