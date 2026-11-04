# PixelMate Windows x64 离线包

`PixelMate-1.0.0-win-x64-offline.zip` 是完整的自包含发布包。

## 不需要安装 .NET

该包已经包含 PixelMate 所需的 .NET 10 运行时和图形原生库，因此：

- 不需要联网。
- 不需要安装 .NET Runtime。
- 不需要单独下载或保留任何 `.NET` 安装程序。

## 使用方式

1. 完整解压 ZIP，不要只把 `PixelMate.exe` 单独拖出来。
2. 双击 `START-PIXELMATE.cmd`，或直接双击 `PixelMate.exe`。
3. 打开图片后使用“保存图片”选择输出位置。

## 不要与开发验收包混淆

文件名包含 `devhost` 的包需要系统中已经安装 .NET 10 Runtime，只用于智能应用控制
兼容性排查和开发验收，不是离线正式包。

如果文件夹名称是 `PixelMate-devhost-win-x64`，请改用
`PixelMate-win-x64` 或 `PixelMate-1.0.0-win-x64-offline.zip`。

## Windows 提醒

未签名的正式应用仍可能被 Windows 11 智能应用控制或 SmartScreen 拦截。该限制不能
通过“包含 .NET 运行时”消除；正式公开分发仍建议使用商业 Authenticode 证书签名。
