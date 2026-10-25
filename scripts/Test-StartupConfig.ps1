$ErrorActionPreference = 'Stop'
$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
$temp = Join-Path ([IO.Path]::GetTempPath()) ('compranexus-' + [guid]::NewGuid().ToString('N') + '.env')
try {
    [IO.File]::WriteAllText($temp, "MSSQL_SA_PASSWORD=notsecure`nACCOUNTING_SERVICE_URL=`n")
    & (Join-Path $PSScriptRoot 'Start-CompraNexus.ps1') -EnvFile $temp -PrepareOnly
    $content = [IO.File]::ReadAllText($temp)
    $value = ([regex]::Match($content, '(?m)^MSSQL_SA_PASSWORD=(.*)$')).Groups[1].Value.Trim()
    if ($value.Length -lt 12 -or $value -notmatch '[0-9]' -or $value -cnotmatch '[A-Z]' -or $value -notmatch '[^a-zA-Z0-9]') {
        throw 'No se creó una contraseña segura.'
    }
    & (Join-Path $PSScriptRoot 'Start-CompraNexus.ps1') -EnvFile $temp -PrepareOnly
    $value2 = ([regex]::Match([IO.File]::ReadAllText($temp), '(?m)^MSSQL_SA_PASSWORD=(.*)$')).Groups[1].Value.Trim()
    if ($value2 -cne $value) { throw 'La segunda ejecución alteró una contraseña válida.' }
    Write-Host 'Pruebas de preparación de .env: OK'
}
finally {
    if (Test-Path -LiteralPath $temp) { Remove-Item -LiteralPath $temp -Force }
}
