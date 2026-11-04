# PixelMate

轻量级图片像素测量和取色工具。

当前进度：阶段 2（图片打开、缩放、平移）开发完成，等待验收。

说明：工作区此前只包含阶段 0 的空白窗口。阶段 2 按用户明确指令直接实施，并补齐了图片浏览所必需的窗口宿主；阶段 1 尚未单独验收。

## 技术栈

- .NET 10 LTS
- C# 14
- Avalonia 12.1.5
- xUnit 2.9.3

当前阶段支持：

- 通过文件选择器或拖拽打开 PNG、JPG、JPEG、BMP、WebP。
- 鼠标滚轮以指针位置为中心缩放。
- 中键拖动平移。
- 适应窗口、恢复 100% 原始尺寸。
- 窗口尺寸变化时保持适应模式。
- 记住上次打开图片所在目录。
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

项目必须按 `.agents/skills/pixelmate-engineer/SKILL.md` 的八个阶段逐段开发并验收。
