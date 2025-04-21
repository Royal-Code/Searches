@echo off
setlocal enabledelayedexpansion

echo ===================================================
echo Publicador de Pacotes NuGet para nuget.org
echo ===================================================

REM Verificar se a fonte nuget.org está configurada
echo Verificando configuracao da fonte nuget.org...
dotnet nuget list source | findstr /i "nuget.org" > nul
set "SOURCE_EXISTS=%ERRORLEVEL%"

if %SOURCE_EXISTS% neq 0 (
    echo Fonte nuget.org nao encontrada. Configurando...
    dotnet nuget add source https://api.nuget.org/v3/index.json --name nuget.org
    if !ERRORLEVEL! neq 0 (
        echo Erro ao configurar fonte nuget.org.
        pause
        exit /b 1
    ) else (
        echo Fonte nuget.org configurada com sucesso!
    )
)

REM Verificar se deseja configurar uma chave API
set /p API_KEY="Digite sua chave API do NuGet: "

echo.
echo Procurando pacotes NuGet '.nupkg' na pasta atual...

set FOUND_PACKAGES=0

for %%f in (*.nupkg) do (
    set /a FOUND_PACKAGES+=1
    echo Encontrado: %%f
)

if %FOUND_PACKAGES% equ 0 (
    echo Nenhum pacote NuGet '.nupkg' encontrado na pasta atual.
    pause
    exit /b 0
)

echo.
echo Total de pacotes encontrados: %FOUND_PACKAGES%
echo.

set /p CONFIRM="Deseja publicar todos estes pacotes para nuget.org? (S/N): "

if /i not "%CONFIRM%"=="S" (
    echo Operação cancelada pelo usuário.
    pause
    exit /b 0
)

echo.
echo Iniciando publicacao dos pacotes...
echo.

for %%f in (*.nupkg) do (
    echo Publicando: %%f
    dotnet nuget push "%%f" --source nuget.org --api-key "%API_KEY%"
	del "%%f"
    
    if !ERRORLEVEL! neq 0 (
        echo Erro ao publicar %%f!
    ) else (
        echo %%f publicado com sucesso!
    )
    echo.
)

echo.
echo Processo de publicacao concluido!
pause