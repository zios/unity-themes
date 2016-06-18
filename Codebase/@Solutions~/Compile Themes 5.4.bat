@ECHO OFF
copy /Y 5.4\* . >NUL
del ZiosThemes.dll
IF EXIST "%programfiles%\MSBuild\14.0\Bin" GOTO USE_VS14_X32
IF EXIST "%programfiles(x86)%\MSBuild\14.0\Bin" GOTO USE_VS14_X64
IF EXIST "%programfiles%\MSBuild\15.0\Bin" GOTO USE_VS15_X32
IF EXIST "%programfiles(x86)%\MSBuild\15.0\Bin" GOTO USE_VS15_X64
GOTO CLOSE
:USE_VS14_X32
	echo Compiling ZiosThemes.dll - Visual Studio 2015 / MSBuild 2015 (on 32-bit host)...
	"%programfiles%\MSBuild\14.0\Bin\MSBuild.exe" ZiosThemes.sln
	GOTO CLOSE
:USE_VS14_X64
	echo Compiling ZiosThemes.dll - Visual Studio 2015 / MSBuild 2015 (on 64-bit host)...
	"%programfiles(x86)%\MSBuild\14.0\Bin\MSBuild.exe" ZiosThemes.sln
	GOTO CLOSE
:USE_VS15_X32
	echo Compiling ZiosThemes.dll - Visual Studio 15 / MSBuild 15 (on 32-bit host)...
	"%programfiles%\MSBuild\15.0\Bin\MSBuild.exe" ZiosThemes.sln
	GOTO CLOSE
:USE_VS15_X64
	echo Compiling ZiosThemes.dll - Visual Studio 15 / MSBuild 15 (on 64-bit host)...
	"%programfiles(x86)%\MSBuild\15.0\Bin\MSBuild.exe" ZiosThemes.sln
	GOTO CLOSE
:CLOSE
copy /Y 5.4\* . >NUL
rmdir /Q /S obj