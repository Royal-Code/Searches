@echo off
setlocal enabledelayedexpansion

echo ============================================================
echo      Build e Copia de Pacotes NuGet
echo ============================================================

REM Definir diretórios
set "REPO_ROOT=%~dp0"
set "SRC_DIR=%REPO_ROOT%src"
set "PUBLISH_DIR=%REPO_ROOT%.publish"

echo Diretorio raiz do repositorio: %REPO_ROOT%
echo Diretorio de codigo-fonte: %SRC_DIR%
echo Diretorio de publicacao: %PUBLISH_DIR%
echo.

REM Verificar se o diretório src existe
if not exist "%SRC_DIR%" (
    echo Erro: O diretoorio de codigo-fonte 'src' nao foi encontrado.
    echo Verifique se este script está na raiz do repositorio.
    goto :ERROR
)


REM Excluir arquivos NuGet existentes em src e subpastas
echo Excluindo arquivos NuGet existentes no diretorio src e subpastas...
for /r "%SRC_DIR%" %%F in (*.nupkg *.snupkg) do (
    echo Excluindo: %%F
    del "%%F"
)
echo.

REM Excluir arquivos NuGet existentes em .publish
echo Excluindo arquivos NuGet existentes no diretorio .publish...
for /r "%PUBLISH_DIR%" %%F in (*.nupkg *.snupkg) do (
    echo Excluindo: %%F
    del "%%F"
)
echo.

REM Navegar para o diretório src
echo Navegando para o diretorio de codigo-fonte...
cd /d "%SRC_DIR%"

REM Executar o build
echo.
echo Executando build dos projetos...
dotnet build --configuration Release

REM Verificar se o build foi bem-sucedido
if %ERRORLEVEL% neq 0 (
    echo Erro durante o build dos projetos.
    goto :ERROR
)

echo.
echo Build concluido com sucesso!
echo.

REM Copiar arquivos NuGet gerados para o diretório .publish
echo Copiando arquivos NuGet para o diretorio .publish...
set "COPIED_FILES=0"

for /r "%SRC_DIR%" %%F in (*.nupkg *.snupkg) do (
    echo Copiando: %%F
    copy "%%F" "%PUBLISH_DIR%"
    set /a COPIED_FILES+=1
)

REM Verificar se existem arquivos para copiar
if %COPIED_FILES% equ 0 (
    echo Nenhum pacote NuGet '.nupkg' ou '.snupkg' foi gerado pelo build.
    goto :END
)

echo.
echo Total de arquivos copiados para .publish: %COPIED_FILES%
echo.
echo Pacotes NuGet disponiveis em: %PUBLISH_DIR%
goto :END

:ERROR
echo.
echo Processo concluido com erros.
pause
exit /b 1

:END
echo.
echo Processo concluido com sucesso!
pause
exit /b 0