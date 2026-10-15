$ErrorActionPreference = 'Stop'
$taskRoot = $PSScriptRoot
$unityEditor = 'D:\Unity Hub Editor\2022.3.62f3c1\Editor\Unity.exe'
if (!(Test-Path -LiteralPath $unityEditor)) { throw '请将 unityEditor 改为本机 Unity 2022.3 编辑器路径。' }
$env:NEON_BUILD_PATH = Join-Path $taskRoot '试玩版\NeonBreakout.exe'
$argsList = @('-batchmode', '-nographics', '-noUpm', '-quit', '-projectPath', ('"' + (Join-Path $taskRoot 'UnityProject') + '"'), '-executeMethod', 'NeonBuild.Windows', '-logFile', ('"' + (Join-Path $taskRoot '验证\build.log') + '"'))
$buildProcess = Start-Process -FilePath $unityEditor -ArgumentList $argsList -WindowStyle Hidden -PassThru
$buildProcess.WaitForExit()
if ($buildProcess.ExitCode -ne 0) { throw ('构建失败，请查看 验证\build.log；退出码 ' + $buildProcess.ExitCode) }
Write-Host '构建完成：试玩版\NeonBreakout.exe'
