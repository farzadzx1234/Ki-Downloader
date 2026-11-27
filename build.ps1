$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
if ([string]::IsNullOrEmpty($root)) { $root = "d:\KIDown" }
$csc = "C:\Windows\Microsoft.NET\Framework64\v4.0.30319\csc.exe"

# Ensure WebView2 NuGet bindings exist (supports clean CI runners)
if (!(Test-Path "$root\lib\wv2\lib\net462\Microsoft.Web.WebView2.Core.dll")) {
    New-Item -ItemType Directory -Force -Path "$root\lib\wv2" | Out-Null
    $ver = "1.0.4370.26"
    $nupkg = "$root\lib\webview2.nupkg"
    $url = "https://api.nuget.org/v3-flatcontainer/microsoft.web.webview2/$ver/microsoft.web.webview2.$ver.nupkg"
    Invoke-WebRequest -Uri $url -OutFile $nupkg -UseBasicParsing
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $archive = [System.IO.Compression.ZipFile]::OpenRead($nupkg)
    foreach ($entry in $archive.Entries) {
        if ($entry.Name -ne "" -and ($entry.FullName -like "lib/net462/*" -or $entry.FullName -like "runtimes/win-x64/native/*")) {
            $dest = Join-Path "$root\lib\wv2" $entry.FullName
            $dir = Split-Path $dest -Parent
            if (!(Test-Path $dir)) { New-Item -ItemType Directory -Force -Path $dir | Out-Null }
            [System.IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $dest, $true)
        }
    }
    $archive.Dispose()
}

Copy-Item "$root\lib\wv2\lib\net462\Microsoft.Web.WebView2.Core.dll" "$root\" -Force
Copy-Item "$root\lib\wv2\lib\net462\Microsoft.Web.WebView2.WinForms.dll" "$root\" -Force
Copy-Item "$root\lib\wv2\runtimes\win-x64\native\WebView2Loader.dll" "$root\" -Force

$refs = @(
    "/reference:System.dll",
    "/reference:System.Core.dll",
    "/reference:System.Drawing.dll",
    "/reference:System.Windows.Forms.dll",
    "/reference:System.Web.Extensions.dll",
    "/reference:System.Net.Http.dll",
    "/reference:$root\Microsoft.Web.WebView2.Core.dll",
    "/reference:$root\Microsoft.Web.WebView2.WinForms.dll"
)

New-Item -ItemType Directory -Force -Path "$root\assets" | Out-Null
if (!(Test-Path "$root\assets\ki-downloader.ico")) {
    Add-Type -ReferencedAssemblies "System.Drawing.dll" -TypeDefinition (Get-Content "$root\src\AppIcons.cs" -Raw)
    [KiDownloader.AppIcons]::SaveAssets("$root\assets")
}

$srcFiles = @(
    "$root\src\Models.cs",
    "$root\src\AppIcons.cs",
    "$root\src\NetworkHelper.cs",
    "$root\src\DownloadEngine.cs",
    "$root\src\IpcServer.cs",
    "$root\src\MainWindow.cs"
)

& $csc /nologo /platform:x64 /target:winexe /optimize+ /win32icon:"$root\assets\ki-downloader.ico" /out:"$root\Ki-Downloader.exe" $refs $srcFiles
if ($LASTEXITCODE -ne 0) { throw "Compilation failed with exit code $LASTEXITCODE" }
Write-Output "Built Ki-Downloader.exe successfully."
