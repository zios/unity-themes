@ECHO OFF
setlocal ENABLEDELAYEDEXPANSION
svn info --show-item revision > revision.txt
set /p revision=<revision.txt
set /p unity=Unity Version? 
set versions=UNITY_EDITOR;UNITY_EDITOR_WIN;
if %unity% geq 5 if %unity% lss 6 set versions=%versions%UNITY_5;
if %unity% geq 5.3 set versions=%versions%UNITY_5_3_OR_NEWER;
if %unity% geq 5.4 set versions=%versions%UNITY_5_4_OR_NEWER;
if %unity% geq 5.5 set versions=%versions%UNITY_5_5_OR_NEWER;
if %unity% geq 5.6 set versions=%versions%UNITY_5_6_OR_NEWER;
set versions=%versions%UNITY_%unity:.=_%;
set extra=/p:DefineConstants="%versions%"
echo %extra%
copy /Y "%unity%\*" . >NUL
copy /Y "..\Systems\Interface\Themes\Editor\ThemeSystem.cs" . >NUL
copy /Y "..\Systems\Interface\Undo\Editor\Undo.asset*" . >NUL
powershell -Command "(gc ThemeSystem.cs) -replace '{revision}', '%revision%' | Out-File ThemeSystem.cs"
del /s ZiosThemes*.dll >nul 2>&1
IF EXIST "%programfiles%\MSBuild\14.0\Bin" GOTO USE_VS14_X32
IF EXIST "%programfiles(x86)%\MSBuild\14.0\Bin" GOTO USE_VS14_X64
IF EXIST "%programfiles%\MSBuild\15.0\Bin" GOTO USE_VS15_X32
IF EXIST "%programfiles(x86)%\MSBuild\15.0\Bin" GOTO USE_VS15_X64
GOTO CLOSE
:USE_VS14_X32
	echo Compiling ZiosThemes.dll - Visual Studio 2015 / MSBuild 2015 (on 32-bit host)...
	"%programfiles%\MSBuild\14.0\Bin\MSBuild.exe" ZiosThemes.sln %extra%
	GOTO CLOSE
:USE_VS14_X64
	echo Compiling ZiosThemes.dll - Visual Studio 2015 / MSBuild 2015 (on 64-bit host)...
	"%programfiles(x86)%\MSBuild\14.0\Bin\MSBuild.exe" ZiosThemes.sln %extra%
	GOTO CLOSE
:USE_VS15_X32
	echo Compiling ZiosThemes.dll - Visual Studio 15 / MSBuild 15 (on 32-bit host)...
	"%programfiles%\MSBuild\15.0\Bin\MSBuild.exe" ZiosThemes.sln %extra%
	GOTO CLOSE
:USE_VS15_X64$
	echo Compiling ZiosThemes.dll - Visual Studio 15 / MSBuild 15 (on 64-bit host)...
	"%programfiles(x86)%\MSBuild\15.0\Bin\MSBuild.exe" ZiosThemes.sln %extra%
	GOTO CLOSE
:CLOSE
copy /Y "%unity%\*" . >NUL
rmdir /Q /S obj
del revision.txt >nul 2>&1
del ThemeSystem.cs >nul 2>&1
del Unity*.dll >nul 2>&1
pause