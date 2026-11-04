# PixelMate Windows 开发验收包

此包用于在 Windows 11“智能应用控制”保持开启时尝试完成功能验收。

## 原理与限制

- 主程序是未签名的 `PixelMate.dll`。
- `START-PIXELMATE-DEVHOST.cmd` 调用微软签名的 `dotnet.exe` 承载该程序，
  避免直接启动未签名的 `PixelMate.exe`。
- 该方式不修改 Windows 安全设置，也不绕过证书购买。
- 智能应用控制仍可能拦截通过其他方式加载的未签名代码；如果也被阻止，则必须在
  安全设置中关闭智能应用控制，或使用商业代码签名。
- 此包需要 .NET 10 Runtime x64，不是最终发布包。
- 由于进程宿主是 `dotnet.exe`，高 DPI 测试只能作为功能参考，最终 DPI 结论仍应以
  签名后的 `PixelMate.exe` 验收为准。

## 使用

1. 完整解压 ZIP。
2. 如果未安装 .NET 10 Runtime，在 PowerShell 中运行：

```powershell
winget install --id Microsoft.DotNet.Runtime.10 --exact
```

3. 双击 `START-PIXELMATE-DEVHOST.cmd`。
4. 按 Windows 验收清单测试图片打开、测量、取色、放大镜、批注、设置和快捷键。

此包仅用于开发验收，不替代阶段 7 的正式 Windows 发布验收。
