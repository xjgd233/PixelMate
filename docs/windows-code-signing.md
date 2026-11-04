# PixelMate Windows 商业代码签名

## 结论

商业代码签名需要由软件发布者向受信任的证书机构申请 Authenticode 代码签名证书。
证书包含经过机构核验的身份信息，私钥必须由申请人或公司控制，不能由 Codex 代办。

签名可以明显改善 Windows SmartScreen 和智能应用控制体验，但不能绝对保证所有
Windows 设备立即不再提示。OV 证书通常仍需要积累下载和运行信誉；EV 证书、
Microsoft Trusted Signing 等方案、证书信誉和系统策略的具体效果需要以目标机器实测为准。

## 证书选择

优先选择支持以下能力的证书或签名服务：

- Authenticode Code Signing。
- SHA-256 文件摘要。
- RFC 3161 时间戳。
- Windows 可用的证书存储、硬件令牌、HSM 或云签名接口。
- 证书有效期内的重新签发和吊销支持。

现代 OV/EV 代码签名证书通常不会导出一个不受保护的 `.pfx` 私钥文件。证书可能：

- 安装在 Windows 证书存储中，并由 USB 硬件令牌保护。
- 由 HSM 或云端签名服务保护。
- 需要通过证书机构提供的中间件或命令行工具调用。

发布者先完成机构要求的主体核验。个人主体需要准备有效身份证明；公司主体通常还需
营业执照、组织地址和申请人授权材料。具体材料和费用由证书机构确认。

## 取得证书指纹

证书安装到 Windows 后，在 PowerShell 中列出签名证书：

```powershell
Get-ChildItem Cert:\CurrentUser\My |
    Where-Object { $_.HasPrivateKey -and $_.EnhancedKeyUsageList.ObjectId -contains "1.3.6.1.5.5.7.3.3" } |
    Format-List Subject, Thumbprint, NotAfter
```

OID `1.3.6.1.5.5.7.3.3` 表示代码签名用途。不要把证书私钥或令牌 PIN 提交到仓库。

## 签名与验证

先重新生成 Windows 发布目录：

```powershell
.\scripts\publish-windows-x64.ps1
```

再使用证书指纹签名并重建 ZIP：

```powershell
.\scripts\sign-windows-x64.ps1 -CertificateThumbprint "证书指纹"
```

证书位于本机计算机证书存储时：

```powershell
.\scripts\sign-windows-x64.ps1 `
    -CertificateThumbprint "证书指纹" `
    -MachineCertificateStore
```

脚本会：

1. 自动查找 `signtool.exe`。
2. 使用 SHA-256 签署 `PixelMate.exe` 和 `PixelMate.dll`。
3. 使用 DigiCert RFC 3161 服务添加时间戳。
4. 执行 `signtool verify /pa /all`。
5. 重新生成 `PixelMate-1.0.0-win-x64.zip` 并输出 SHA-256。

也可手动验证：

```powershell
signtool verify /pa /all /v .\artifacts\PixelMate-win-x64\PixelMate.exe
Get-AuthenticodeSignature .\artifacts\PixelMate-win-x64\PixelMate.exe
```

预期签名状态为 `Valid`，证书主体和证书机构应符合实际申请信息。

## 注意事项

- 不要把证书私钥、硬件令牌 PIN、云签名凭据写入仓库、ZIP 或聊天记录。
- 时间戳服务失败时不要发布无时间戳的签名包。
- 证书吊销或续期后重新签名，不要复用旧 ZIP。
- 新证书仍需要通过真实 Windows 的 SmartScreen、智能应用控制和多 DPI 验收。
- 自签名证书只能用于本机开发，不能作为公开分发方案，也不能保证通过智能应用控制。
