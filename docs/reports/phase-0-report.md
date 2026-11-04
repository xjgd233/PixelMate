# 阶段 0：环境检查和项目初始化

执行日期：2026-11-04

阶段状态：待验收

前置阶段：无

## 一、完成内容

- 检查并确认开发主机为 Apple Silicon：Darwin arm64，macOS 26.7.1。
- 检查时本机没有可用的 .NET SDK；已从 Microsoft 官方发布元数据确认 .NET 10 为 LTS，并安装 SDK 10.0.403、运行时 10.0.14 到 `/Users/admin/.dotnet`。
- 确认 .NET 10.0 处于 Active 支持阶段，结束支持日期为 2028-11-14；官方支持系统列表包含 macOS 26 ARM64。
- 安装 Avalonia 官方模板 12.1.5。模板明确支持 `net10.0`，NuGet 包同时提供 `ref/net10.0` 与 `lib/net10.0` 资产。
- 确认 Avalonia 12.1.5 包许可证为 MIT，SkiaSharp 图形依赖版本为 3.119.4。
- 创建解决方案、Avalonia 桌面应用和 xUnit 测试项目。
- 使用 `global.json` 固定 .NET SDK 10.0.403，并允许同一 feature band 的补丁升级。
- 将主窗口缩减为阶段 0 所需的空白窗口，仅设置标题和基础窗口尺寸。
- 初始化 Git 仓库；未创建阶段验收 Tag。
- 未实现图片、工具栏、测量、取色、放大镜或任何阶段 1 及后续功能。

## 二、修改文件

- `global.json`
- `PixelMate.slnx`
- `README.md`
- `.gitignore`
- `src/PixelMate/PixelMate.csproj`
- `src/PixelMate/Program.cs`
- `src/PixelMate/App.axaml`
- `src/PixelMate/App.axaml.cs`
- `src/PixelMate/MainWindow.axaml`
- `src/PixelMate/MainWindow.axaml.cs`
- `src/PixelMate/app.manifest`
- `tests/PixelMate.Tests/PixelMate.Tests.csproj`
- `tests/PixelMate.Tests/ProjectSmokeTests.cs`
- `docs/reports/phase-0-report.md`

项目说明文件 `.agents/skills/pixelmate-engineer/SKILL.md` 与 `START-HERE.md` 保持原样。

## 三、编译结果

还原命令：

```sh
/Users/admin/.dotnet/dotnet restore PixelMate.slnx \
  --packages /tmp/pixelmate-nuget \
  -p:NuGetAudit=false
```

标准构建命令：

```sh
/Users/admin/.dotnet/dotnet build PixelMate.slnx -c Debug --no-restore
```

结果：0 个警告，0 个错误。

产物检查：

- 应用入口：`src/PixelMate/bin/Debug/net10.0/PixelMate`
- `file`：Mach-O 64-bit executable arm64
- `lipo -info`：Non-fat file，architecture arm64

测试命令：

```sh
/Users/admin/.dotnet/dotnet test PixelMate.slnx -c Debug \
  --no-build --no-restore
```

结果：总计 1 个测试，通过 1 个，失败 0 个。测试内容为开发运行时架构必须是 Apple Silicon ARM64。

## 四、实际运行测试

- 使用编译产物实际启动 Avalonia 桌面进程，并通过临时 macOS `.app` 包装让 LaunchServices 注册应用。
- 窗口成功创建，标题为 `PixelMate`，内容区为空白。
- 窗口带有标准关闭、最小化、全屏按钮，应用菜单栏正常出现。
- 通过 Computer Use 截图确认窗口可见、无启动崩溃、无阶段 1 UI 内容。
- 通过重新绑定应用并点击关闭按钮完成退出；再次查询 Codex 应用状态后，PixelMate 已不在运行应用列表中。

## 五、Computer Use 测试结果

- Computer Use 可用，能够按 bundle identifier 获取原生 PixelMate 应用。
- Accessibility 树识别结果：标准窗口 `PixelMate`、窗口容器、关闭/最小化/全屏按钮、标题文本和菜单栏。
- 截图显示窗口为纯空白内容区。
- 本轮没有可测试的业务按钮或图片，因为按阶段规则尚未实现阶段 1 及后续功能。

## 六、已知问题

- `.NET SDK` 安装在 `/Users/admin/.dotnet`，未修改用户 shell 配置；新终端需先执行 `export PATH="$HOME/.dotnet:$PATH"`，或继续使用绝对路径。
- 当前机器只有 Apple Command Line Tools，没有完整 Xcode。阶段 0 的 .NET/Avalonia 编译与运行不受影响；后续如增加原生 App Bundle 签名或公证，需要另行核验完整 Xcode。
- Codex 受限沙箱会拦截 Avalonia 构建服务的网络连接；在获得联网权限后，标准构建 1.01 秒完成且无警告。该现象不来自项目代码。
- 阶段 0 未创建验收 Tag；只有用户验收后才应按规范决定是否记录阶段版本。

## 七、Git 提交记录

阶段提交信息：`chore: initialize PixelMate phase 0`

提交哈希在最终阶段报告及 `git log -1` 中给出。

## 八、等待用户验收

阶段 0 已完成并停止。未进入阶段 1，未继续实现任何后续功能，等待用户明确验收。
