# 阶段 6：稳定性、性能与回归测试

执行日期：2026-11-04

阶段状态：已验收

前置阶段：阶段 5（已验收）

## 一、完成内容

- 图片解码从 UI 线程移到后台任务，打开大图时窗口和工具栏保持可响应。
- 连续打开时增加请求版本与取消机制，只允许最后一次打开请求更新画布；关闭窗口时取消未完成的解码。
- 文件选择器的异常被限制在事件边界内，不再可能从 `async void` 处理器逃逸并终止应用。
- 损坏图片、无权限文件和未知解码错误都会在状态栏反馈；失败时保留当前已显示的图片和视图。
- 增加图片安全预算：单边不超过 `32767` 像素，总量不超过 `6400` 万像素。超限文件在创建解码缓冲前被拒绝。
- 修复图片创建失败时可能遗留 `WriteableBitmap` 的资源路径，确保位图与像素缓冲初始化失败时均被释放。
- 偏好设置改为同目录临时文件原子替换；序列化或写入失败不会覆盖原设置，并清理残留临时文件。
- 修复指针离开图片后 HEX 与颜色预览未清空的问题。
- 缓存状态栏和取色预览的固定画刷，减少鼠标移动时反复创建临时画刷。
- 开发构建自动复制当前平台的 Skia 原生解码库，消除 .NET 10 无 RID 构建可用但解码测试缺少 `libSkiaSharp` 的差异。
- 保持阶段 0—5 的图片打开、透明棋盘、缩放、平移、测量、取色和放大镜功能。

## 二、修改文件

- `Directory.Build.targets`
- `src/PixelMate/Controls/ImageCanvas.cs`
- `src/PixelMate/Imaging/ImageDecodeLimits.cs`
- `src/PixelMate/Imaging/OriginalImageDecoder.cs`
- `src/PixelMate/MainWindow.axaml.cs`
- `src/PixelMate/PixelMate.csproj`
- `src/PixelMate/Preferences/AppPreferences.cs`
- `tests/PixelMate.Tests/AppPreferencesTests.cs`
- `tests/PixelMate.Tests/ImageDecodeLimitsTests.cs`
- `tests/PixelMate.Tests/OriginalImageDecoderTests.cs`
- `README.md`
- `docs/phase-status.md`
- `docs/reports/phase-6-report.md`

## 三、编译结果

Debug 构建命令：

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

Release 构建命令：

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

Debug 和 Release 输出均确认包含当前平台的 `libSkiaSharp.dylib`。

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

结果：72 个测试通过，0 个失败，0 个跳过；Debug 与 Release 均通过。

新增回归覆盖：

- PNG、JPEG、WebP、BMP 的真实文件解码。
- PNG 全透明像素的原始 RGB 通道保留。
- 损坏 PNG 被拒绝且不会保留文件句柄。
- 边长上限、6400 万像素边界和超限拒绝。
- 偏好设置覆盖写入、重新读取和临时文件清理。

## 四、实际运行测试

- 使用 `./scripts/run-macos.sh Debug` 启动最终构建。
- 打开 `64 × 48` 四象限透明 PNG，适应窗口缩放为 `1127%`，透明区域棋盘格正常。
- 使用取色工具读取并复制 `#E04848`；指针离开图片后 HEX 立即恢复为 `HEX —`。
- 回归测量工具，成功记录 1 条线并显示复制/清除操作。
- 回归放大镜，`12×` 网格、中心准星、坐标 `X 13 Y 9` 和 HEX 显示正常。
- 打开 `4096 × 3072`、文件大小约 374 KB 的 PNG；从点击“打开”到观察到完整窗口状态的实测等待为约 992 ms。
- 连续重新打开上述 12MP 图片 3 次，没有崩溃、窗口卡死或界面状态残留。
- 大图首次打开后进程常驻内存约 56,352 KB；连续重开后约 51,232 KB，未观察到随打开次数上升。
- 打开 14 字节的损坏 PNG，状态栏显示“无法打开图片：无法识别图片内容。”，原大图继续保留。
- 打开头部声明为 `9000 × 8000` 的 70 字节 PNG，状态栏显示“图片不能超过 64 百万像素。”，原大图继续保留。

## 五、Computer Use 测试结果

- Computer Use 可识别文件选择器、打开按钮、画布、工具栏、测量操作区、取色状态和放大镜倍率控件。
- 通过真实文件选择器打开测试图片，完成四象限图、大图、损坏图和超限图的交互。
- 通过真实鼠标点击、拖动和指针离开图片，验证取色、测量、放大镜以及颜色状态清理。
- 通过截图确认透明棋盘格、测量覆盖、放大镜网格和底座状态栏显示正常。
- 批量打开脚本曾因系统文件选择器不保留选中项而使自动化循环失效；改为逐次选择后完成 3 次连续大图重开，未将该自动化限制计作产品失败。

## 六、已知问题

- `6400` 万像素安全预算会主动拒绝更巨大的图片；这是为了避免单图解码同时占用源缓冲和显示位图后导致内存耗尽。
- 纯解码耗时会随图片内容和压缩格式变化；本次约 992 ms 是完整 GUI 操作观察值，不是脱离窗口的微基准。
- 本轮只验证 macOS ARM64。Windows x64 的原生 Skia 复制规则已配置，但必须在阶段 7 的真实 Windows 环境中验证。
- 调试构建和 Release 输出目录较大，主要来自 Avalonia、Skia 原生库及其跨平台依赖；最终体积只以阶段 7 的目标 RID 发布结果为准。

## 七、Git 提交记录

阶段实现提交：`377f596`

提交信息：`fix: stabilize phase 6 image loading`

阶段报告和状态提交：随本次文档更新一并提交。

## 八、用户验收

阶段 6 的代码、Debug/Release 构建、72 项回归测试、异常文件处理和 macOS 真实窗口测试均已完成。
用户已于 2026-11-04 明确回复“验收阶段6”，现正式记录为“已验收”。

本次验收不代表阶段 7 已获授权；下一步必须等待用户明确指令。
