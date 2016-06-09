@ECHO OFF
copy /Y 4.7.2\* . >NUL
IF EXIST "%programfiles%\MSBuild\14.0\Bin" GOTO USE_VS14_X64
IF EXIST "%programfiles%\MSBuild\15.0\Bin" GOTO USE_VS15_X64
IF EXIST "%programfiles(x86)%\MSBuild\14.0\Bin" GOTO USE_VS14_X86
IF EXIST "%programfiles(x86)%\MSBuild\15.0\Bin" GOTO USE_VS15_X86
GOTO CLOSE
:USE_VS14_X86
	echo Compiling ZiosThemes.dll- Visual Studio 2015 (32-bit)...
	"%programfiles(x86)%\MSBuild\14.0\Bin\MSBuild.exe" ZiosThemes.sln
	GOTO CLOSE
:USE_VS15_X86
	echo Compiling ZiosThemes.dll- Visual Studio 15 (32-bit)...
	"%programfiles(x86)%\MSBuild\15.0\Bin\MSBuild.exe" ZiosThemes.sln
	GOTO CLOSE
:USE_VS14_X64
	echo Compiling ZiosThemes.dll- Visual Studio 2015 (64-bit)...
	"%programfiles%\MSBuild\14.0\Bin\MSBuild.exe" ZiosThemes.sln
	GOTO CLOSE
:USE_VS15_X64
	echo Compiling ZiosThemes.dll- Visual Studio 15 (64-bit)...
	"%programfiles%\MSBuild\15.0\Bin\MSBuild.exe" ZiosThemes.sln
	GOTO CLOSE
:CLOSE
copy /Y 5.4\* . >NUL