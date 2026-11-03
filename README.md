# PixelMate

轻量级图片像素测量和取色工具。

当前进度：阶段 0（环境检查与项目初始化）。阶段 1 及后续功能尚未开始。

## 技术栈

- .NET 10 LTS
- C# 14
- Avalonia 12.1.5
- xUnit 2.9.3

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

项目必须按 `.agents/skills/pixelmate-engineer/SKILL.md` 的八个阶段逐段开发并验收。
