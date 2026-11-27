$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
if ([string]::IsNullOrEmpty($root)) { $root = "d:\KIDown" }
$csc = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"

& "$root\build.ps1"

# Ensure fonts exist for clean CI builds (preserve local YekanBakh if already present)
New-Item -ItemType Directory -Force -Path "$root\ui\fonts" | Out-Null
if (!(Test-Path "$root\ui\fonts\GoogleSans-400.woff2")) {
    $ua = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36"
    $css = (Invoke-WebRequest -Uri "https://fonts.googleapis.com/css2?family=Google+Sans:wght@400;500;600;700&display=swap" -Headers @{ "User-Agent" = $ua } -UseBasicParsing).Content
    $ms = [regex]::Matches($css, '/\* latin \*/\s*@font-face\s*\{[^}]*font-weight:\s*(\d+);[^}]*src:\s*url\((https://[^)]+)\)', 'Singleline')
    foreach ($m in $ms) {
        $w = $m.Groups[1].Value; $u = $m.Groups[2].Value
        Invoke-WebRequest -Uri $u -OutFile "$root\ui\fonts\GoogleSans-$w.woff2" -UseBasicParsing
    }
}
if (!(Test-Path "$root\ui\fonts\YekanBakh-VF.woff2") -and !(Test-Path "$root\ui\fonts\Vazirmatn-VF.woff2")) {
    $vUrl = "https://cdn.jsdelivr.net/gh/rastikerdar/vazirmatn@v33.003/fonts/webfonts/Vazirmatn[wght].woff2"
    Invoke-WebRequest -Uri $vUrl -OutFile "$root\ui\fonts\Vazirmatn-VF.woff2" -UseBasicParsing
}

New-Item -ItemType Directory -Force -Path "$root\browser-extension\icons","$root\dist" | Out-Null
if (!(Test-Path "$root\browser-extension\icons\icon128.png")) {
    if (-not ("KiDownloader.AppIcons" -as [type])) {
        Add-Type -ReferencedAssemblies "System.Drawing.dll" -TypeDefinition (Get-Content "$root\src\AppIcons.cs" -Raw)
    }
    foreach ($sz in @(16, 48, 128)) {
        $bmp = [KiDownloader.AppIcons]::RenderLogoBitmap($sz)
        $bmp.Save("$root\browser-extension\icons\icon$sz.png", [System.Drawing.Imaging.ImageFormat]::Png)
        $bmp.Dispose()
    }
}

$extZip = "$root\dist\Ki-Downloader-Extension.zip"
if (Test-Path $extZip) { Remove-Item $extZip -Force }
Compress-Archive -Path "$root\browser-extension\*" -DestinationPath $extZip -Force

$stage = "$root\lib\stage"
if (Test-Path $stage) { Remove-Item $stage -Recurse -Force }
New-Item -ItemType Directory -Force -Path "$stage\ui\fonts","$stage\assets","$stage\browser-extension\icons" | Out-Null
Copy-Item "$root\Ki-Downloader.exe","$root\Microsoft.Web.WebView2.Core.dll","$root\Microsoft.Web.WebView2.WinForms.dll","$root\WebView2Loader.dll" "$stage\" -Force
Copy-Item "$root\ui\*" "$stage\ui\" -Force -Exclude "fonts"
Copy-Item "$root\ui\fonts\*" "$stage\ui\fonts\" -Force
Copy-Item "$root\assets\*" "$stage\assets\" -Force
Copy-Item "$root\browser-extension\*" "$stage\browser-extension\" -Force -Exclude "icons"
Copy-Item "$root\browser-extension\icons\*" "$stage\browser-extension\icons\" -Force
Copy-Item "$root\installer\Install.cmd","$root\installer\Uninstall.cmd" "$stage\" -Force

$portableZip = "$root\dist\Ki-Downloader-Portable.zip"
if (Test-Path $portableZip) { Remove-Item $portableZip -Force }
Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($stage, $portableZip, [System.IO.Compression.CompressionLevel]::Optimal, $false)

$payloadZip = "$root\lib\payload.zip"
if (Test-Path $payloadZip) { Remove-Item $payloadZip -Force }
Copy-Item $portableZip $payloadZip -Force

& $csc /nologo /platform:x64 /target:winexe /optimize+ `
    /win32icon:"$root\assets\ki-downloader.ico" `
    /resource:"$payloadZip",payload.zip `
    /reference:System.dll /reference:System.Drawing.dll /reference:System.Windows.Forms.dll /reference:System.IO.Compression.dll /reference:System.IO.Compression.FileSystem.dll /reference:Microsoft.CSharp.dll `
    /out:"$root\dist\Ki-Downloader-Setup.exe" `
    "$root\installer\Setup.cs"

if ($LASTEXITCODE -ne 0) { throw "Installer compilation failed" }
Remove-Item $stage -Recurse -Force
Remove-Item $payloadZip -Force
Write-Output "Built packages:"
Get-Item "$root\dist\Ki-Downloader-Portable.zip", "$root\dist\Ki-Downloader-Setup.exe", "$root\dist\Ki-Downloader-Extension.zip" | Select-Object Name, Length
