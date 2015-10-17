psake .\Source\Scripts\CI.Tasks.ps1 $args -parameters "@{BasePath='$PSScriptRoot'}" -nologo

if ($LASTEXITCODE -ne 0) {
    throw "PSake failed with exit code $LASTEXITCODE.  See previous errors for details."
}