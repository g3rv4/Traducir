param (
    [Parameter(Mandatory=$true)][string]$BuildType,
    [Parameter(Mandatory=$true)][int]$BuildNumber,
    [Parameter(Mandatory=$false)][string]$CommitSHA
)

$dockerImage = "g3rv4/traducir-builder:6.0.201-alpine3.15"

$basePath = Get-Location
$buildPropsPath = Join-Path $basePath Directory.Build.props
$buildPath = Join-Path $basePath bin/build

[xml]$xmlDoc = Get-Content $buildPropsPath
$versionElement = $xmlDoc['Project']['PropertyGroup']['Version']
$version = [version]$versionElement.InnerText
$newVersion = "$($version.Major).$($version.Minor).$($BuildNumber)"

$versionWithoutHash = $newVersion
if ($CommitSHA) {
    $newVersion = "$($newVersion)+$($CommitSHA.SubString(0, 7))"
}

$versionElement.InnerText = $newVersion
$xmlDoc.Save($buildPropsPath)

if (Test-Path $buildPath -PathType Container) {
    rm -rf $buildPath
}

$uid = sh -c 'id -u'
$gid = sh -c 'id -g'

Write-Host "Linting..."
docker run --rm -v "$($basePath):/var/app" $dockerImage tslint --project /var/app/Traducir.Web/Scripts/tsconfig.json

Write-Host "Transpiling..."
docker run --rm -v "$($basePath):/var/app" $dockerImage ash -c "tsc --build /var/app/Traducir.Web/Scripts/tsconfig.json && chown -R $($uid):$($gid) /var/app"

# Bust sourcemap cache
$jsPath = 'Traducir.Web/wwwroot/js/dist/app.js'
$sha = Get-FileHash "$($jsPath).map"

$jsContent = Get-Content -Path $jsPath -Raw
$jsContent = $jsContent.Replace('app.js.map', "app.js.map?v=$($sha.Hash)")

Set-Content -Path $jsPath -Value $jsContent

Write-Host "Building..."
docker run --rm -v "$($basePath):/var/src" $dockerImage ash -c "dotnet publish -c $BuildType /var/src/Traducir.Web/Traducir.Web.csproj -o /var/src/bin/build && chown -R $($uid):$($gid) /var/src"

$nuspecPath = Join-Path $buildPath traducir.nuspec
$nupkgPath = Join-Path $buildPath "traducir.$($newVersion).nupkg"
cp traducir.nuspec $nuspecPath

[xml]$xmlDoc = Get-Content $nuspecPath
$xmlDoc['package']['metadata']['version'].InnerText = $versionWithoutHash
if ($BuildType -ne "Release") {
    $currentId = $xmlDoc['package']['metadata']['id'].InnerText
    $xmlDoc['package']['metadata']['id'].InnerText = "$currentId-$($BuildType.ToLower())"
}

$xmlDoc.Save($nuspecPath)

Compress-Archive -Path "$($buildPath)/*" -DestinationPath $nupkgPath

Write-Host "Compressed!"
Write-Host "::set-output name=version::$newVersion"
Write-Host "::set-output name=version_without_hash::$versionWithoutHash"

if ($env:GITHUB_ENV) {
    Write-Output "VERSION_WITHOUT_HASH=$versionWithoutHash" | Out-File -FilePath $env:GITHUB_ENV -Encoding utf8 -Append
    Write-Output "VERSION=$newVersion" | Out-File -FilePath $env:GITHUB_ENV -Encoding utf8 -Append
    Write-Output "PKG_PATH=$nupkgPath" | Out-File -FilePath $env:GITHUB_ENV -Encoding utf8 -Append
}