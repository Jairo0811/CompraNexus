<#
.SYNOPSIS
  Repara la configuracion local e inicia CompraNexus sin borrar el volumen de SQL Server.
.DESCRIPTION
  Prepara una contrasena fuerte privada en .env, inicia SQL Server y luego la API
  ASP.NET Core 10 y React. Nunca imprime la contrasena en consola.
  -PrepareOnly sirve para validar la configuracion sin ejecutar Docker.
.EXAMPLE
  .\scripts\Start-CompraNexus.ps1
.EXAMPLE
  .\scripts\Start-CompraNexus.ps1 -PrepareOnly
#>
[CmdletBinding()]
param(
    [switch]$PrepareOnly,
    [string]$EnvFile = ""
)
$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$root = (Resolve-Path (Join-Path $PSScriptRoot '..')).Path
if ([string]::IsNullOrWhiteSpace($EnvFile)) {
    $path = Join-Path $root '.env'
}
elseif ([IO.Path]::IsPathRooted($EnvFile)) {
    $path = [IO.Path]::GetFullPath($EnvFile)
}
else {
    $path = [IO.Path]::GetFullPath((Join-Path $root $EnvFile))
}

function Test-SqlPassword([string]$value) {
    if ([string]::IsNullOrWhiteSpace($value) -or $value.Length -lt 12 -or $value.Length -gt 128) {
        return $false
    }
    if ($value -match '(?i)CHANGE_THIS|YOUR_PASSWORD|EJEMPLO|CAMBIE_ESTA') {
        return $false
    }
    $groups = 0
    if ($value -cmatch '[A-Z]') { $groups++ }
    if ($value -cmatch '[a-z]') { $groups++ }
    if ($value -match '[0-9]') { $groups++ }
    if ($value -match '[^a-zA-Z0-9]') { $groups++ }
    return $groups -ge 3
}

$parent = Split-Path $path -Parent
if (-not (Test-Path -LiteralPath $parent)) {
    New-Item -ItemType Directory -Path $parent -Force | Out-Null
}
if (-not (Test-Path -LiteralPath $path)) {
    Copy-Item -LiteralPath (Join-Path $root '.env.example') -Destination $path
    Write-Host 'Archivo .env creado para uso local.'
}

$content = [IO.File]::ReadAllText($path)
$passwordPattern = [regex]'(?m)^[ \t]*MSSQL_SA_PASSWORD[ \t]*=[ \t]*(.*)[ \t]*$'
$found = $passwordPattern.Match($content)
$password = if ($found.Success) { $found.Groups[1].Value.Trim().Trim('"').Trim("'") } else { '' }

if (-not (Test-SqlPassword $password)) {
    # Compatible con Windows PowerShell 5.1 y PowerShell 7.
    $bytes = New-Object 'System.Byte[]' 20
    $rng = [Security.Cryptography.RandomNumberGenerator]::Create()
    try { $rng.GetBytes($bytes) }
    finally { $rng.Dispose() }
    $password = 'Cn' + [BitConverter]::ToString($bytes).Replace('-', '') + '!'
    if ($found.Success) {
        $content = $passwordPattern.Replace($content, ('MSSQL_SA_PASSWORD=' + $password), 1)
    }
    else {
        $content = $content + [Environment]::NewLine + 'MSSQL_SA_PASSWORD=' + $password + [Environment]::NewLine
    }
    [IO.File]::WriteAllText($path, $content, (New-Object System.Text.UTF8Encoding($false)))
    Write-Host 'Se guardo una nueva contrasena segura en .env (no se muestra).'
    Write-Warning 'Con un volumen SQL ya inicializado, modificar .env NO cambia automaticamente la clave sa almacenada. Consulte docs/DEPLOYMENT.md.'
}
else {
    Write-Host 'La contrasena local cumple la politica minima de SQL Server.'
}

if ($PrepareOnly) {
    Write-Host 'Configuracion lista; Docker no se ha ejecutado.'
    return
}

if ([IO.Path]::GetFullPath($path) -ne [IO.Path]::GetFullPath((Join-Path $root '.env'))) {
    throw 'Para iniciar Docker use el archivo .env de la raiz. -EnvFile diferente solo es valido con -PrepareOnly.'
}

Push-Location $root
try {
    & docker info --format '{{.ServerVersion}}' 2>$null | Out-Null
    if ($LASTEXITCODE -ne 0) {
        throw 'Docker Desktop no esta disponible o el daemon no esta iniciado.'
    }

    Write-Host 'Iniciando SQL Server 2022 en localhost,14330...'
    & docker compose up -d --force-recreate sqlserver
    if ($LASTEXITCODE -ne 0) { throw 'No fue posible iniciar SQL Server.' }

    Write-Host 'Compilando y levantando la API .NET y React...'
    & docker compose up --build -d
    if ($LASTEXITCODE -ne 0) {
        throw 'Docker Compose fallo. Revise docker compose ps y sus logs; no borre el volumen de datos.'
    }

    $ready = $false
    for ($attempt = 0; $attempt -lt 30; $attempt++) {
        try {
            $response = Invoke-RestMethod -Uri 'http://localhost:5098/health' -TimeoutSec 3
            if ($response.status -eq 'ok') { $ready = $true; break }
        }
        catch {
            # SQL Server podria continuar configurando sus bases de sistema.
        }
        Start-Sleep -Seconds 2
    }

    & docker compose ps
    if (-not $ready) {
        Write-Warning 'La API no confirmo la conexion. Ejecute docker compose logs --tail 60 sqlserver y docker compose logs --tail 60 api.'
        throw 'No se confirmo API / SQL Server. El volumen de datos sigue intacto.'
    }

    Write-Host 'CompraNexus operativo: http://localhost:8088'
    Write-Host 'API: http://localhost:5098/health'
    Write-Host 'SQL Server para SSMS: localhost,14330 (usuario sa).'
}
finally {
    Pop-Location
}
