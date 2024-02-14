<#
.SYNOPSIS
Setsup new customization project

.EXAMPLE
PS> .\csMake.ps1 -Project test1 -Output C:\src\ -Options features,webhooks
#>

[CmdletBinding()]
param(
    [Parameter(Position=0, Mandatory=$false, ValueFromPipeline=$false, ValueFromPipelineByPropertyName=$false)]
    [string]$Project,
    [Parameter(Position=1, Mandatory=$false, ValueFromPipeline=$false, ValueFromPipelineByPropertyName=$false)]
    [string]$SitePath = ($env:ACC_SITE_PATH),
    [Parameter(Position=2, Mandatory=$false, ValueFromPipeline=$false, ValueFromPipelineByPropertyName=$false)]
    [string]$Output = (Get-Location),
    [string[]] $Templates= @(),
    [switch] $Help
)

$ESC=[char]27
$RED="$ESC[0;91m"
$YELLOW="$ESC[1;33m"
$NC="$ESC[0m"


function Show-Usage {
    Write-Output "`nUsage: $YELLOW csMake [options] <Project Name> $NC"
@"

Options:
 -h|--help      Show command line help and exit.
 -output        Location to place the generated output. The default is the current directory.
 -templates     Comma delimited list of source code templates to include. e.g. Features,Webhooks,Plugin
                    Features - Adds custom features - https://help-2023r1.acumatica.com/Help?ScreenId=ShowWiki&pageid=8285172e-d3b1-48d9-bcc1-5d20e39cc3f0
                    Plugin - Adds Customization Plugin
                    * - Adds all code options

 The above arguments can be shortened as much as to be unambiguous (e.g. -p for project, -o for output, etc.).
"@
}


function Copy-File($Src, $Dst, [switch]$batch)  {
    # Create the directory tree first so Copy-Item succeeds
    # If $Dst is the target directory, make sure it ends with "\"
    $DstDir = [IO.Path]::GetDirectoryName($Dst)
    if ($batch) {
        Write-Output "md `"$DstDir`""
        Write-Output "copy /Y `"$Src`" `"$Dst`""
    }
    else {
        New-Item -ItemType Directory -ErrorAction Ignore $DstDir | Out-Null
        Copy-Item -Force $Src $Dst
    }
}

function Copy-Directory($Src, $Dst) {
    New-Item -ItemType Directory -ErrorAction Ignore $Dst | Out-Null
    Copy-Item -Force -Recurse $Src $Dst
  }

  function Get-VSOpen {
    $vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
    $VSInstallRoot = & $vswhere -nologo -latest -products "*" -all -prerelease -property installationPath
    $devenv = "$VSInstallRoot\Common7\IDE\devenv.exe"
   
return @"
@echo off
setlocal
set ACC_SITE_PATH=#SITE_PATH#
echo ---------------
start /B "$devenv" "%~dp0\#PROJECT#.sln %*"
"@ 
}

function Ensure-Solution ($Dst) {
       
    # Make solution directory
    New-Item -ItemType Directory -ErrorAction Ignore $Dst | Out-Null

    # Folders
    @("tools", "assets", "database", ".config") | ForEach-Object { Copy-Directory "$TemplatePath\$_" "$Dst" }

    # Files        
    @("Directory.Build.props", "README.md",
        "build.ps1", "build.cake", ".gitignore", ".editorconfig") | % {
        $tmp = (Get-Content "$TemplatePath/$_" | % { $_ -replace "#PROJECT#", $Project }) 
        $tmp | Out-File -FilePath "$Dst/$_"
    }
   
    (Get-VSOpen) -replace "#PROJECT#", $Project -replace "#SITE_PATH#", $SitePath | Out-File "$Dst/OpenVS.cmd" -Encoding UTF8
    (Get-Content "$TemplatePath/WebsiteSolution.sln") -replace "#PROJECT#", $Project -replace, "#SITE_PATH#", $SitePath | Out-File -Encoding utf8 "$Dst/$Project.sln"
}

function Ensure-Project ($Dst, $ProjectName, $Sources) {    
    $targetDir = Join-Path $Dst "src/$ProjectName"

    $csprojPath = "$targetDir/$ProjectName.csproj"
    New-Item -ItemType Directory -ErrorAction Ignore $targetDir | Out-Null
    (Get-Content "$TemplatePath/src/ProjectTemplate.csproj") -replace "#PROJECT#", $ProjectName | Out-File -Encoding utf8 $csprojPath

    
    if ($Sources.Length -eq 1) { $Sources = $Sources[0].Split(",") }
    if ($Sources -contains "*") {
        $Sources = @("features", "plugin")
    }

    if ($Sources -notcontains "features") {

        $csprojContent = Get-Content -Path $csprojPath -Raw

        $startMarker = "<!--a1[-->"
        $endMarker = "<!--]-->"       
        
        # Find the indices of the start and end markers
        $startIndex = $csprojContent.IndexOf($startMarker)
        $endIndex = $csprojContent.IndexOf($endMarker, $startIndex) + $endMarker.Length

        # Remove the XML content between the start and end markers
        $csprojContent = $csprojContent.Remove($startIndex, $endIndex - $startIndex)

        # Write the modified content back to the .csproj file
        $csprojContent | Set-Content -Path $csprojPath
    }
     
    $Sources  += "default"
    $Sources = @($Sources | ForEach-Object {
        switch ($_) {
            #"webhooks" { @('WebhookHandler.cs') }
            "features" { @('Features.xml', 'Features.cs') }
            "plugin" { @('CustomizationPlugin.cs') }
            "default" { @('Messages.cs', 'Helper.cs', 'MyGraph.cs') }
            default { throw "Unknown source template $_" }
        }
    })


    foreach ($file in $Sources) {
        Copy-File (Resolve-Path (Join-Path $TemplatePath/src $file)) $targetDir
    } 
}

function Ensure-Tests ($Dst, $ProjectName) {
    $targetDir = Join-Path $Dst "tests/$ProjectName.Tests"
    New-Item -ItemType Directory -ErrorAction Ignore $targetDir | Out-Null
    
   (Get-Content "$TemplatePath/tests/ProjectTemplate.Tests.csproj" ) -replace "#PROJECT#", $ProjectName | Out-File -Encoding utf8 "$targetDir/$ProjectName.Tests.csproj"   
}

try {
    
    if ($Help -or (($null -ne $Project) -and ($Project.Contains('/help') -or $Project.Contains('/?')))) {
        Show-Usage
        exit 0
    }
        
    if (-not $Project) {
        Write-Output "`n$RED ERROR: Missing project name $NC"
        Show-Help    
        exit 1
    }
        
    $SriptDirectory = Split-Path $MyInvocation.MyCommand.Path
    $TemplatePath = Join-Path $SriptDirectory "template"
    
    if (-not (Test-Path "$TemplatePath")) {
        Write-Output $RED "ERROR: Template directory not found."
        exit 1
    }
    
    $SolutionDir += $Output + "\" + $Project
    
    if (Test-Path $SolutionDir -PathType Container) {
        Write-Output $RED "ERROR: Cannot generate solution. The directory already exists: " $YELLOW $SolutionDir $NC
        exit 1
    }
    
    & {
        Ensure-Solution $SolutionDir
        Ensure-Project $SolutionDir $Project $Templates
        Ensure-Tests $SolutionDir $Project
    }

    Write-Output "Completed successfully..."
}
catch {
    Write-Host $_.ScriptStackTrace
    exit 1
}
exit 0
