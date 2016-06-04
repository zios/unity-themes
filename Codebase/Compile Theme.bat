@ECHO OFF
IF EXIST "%programfiles(x86)%\MSBuild\14.0\Bin" GOTO USE_VS14_X86
IF EXIST "%programfiles(x86)%\MSBuild\14.0\Bin" GOTO USE_VS14_X64
IF EXIST "%programfiles(x86)%\MSBuild\15.0\Bin" GOTO USE_VS15_X86
IF EXIST "%programfiles(x86)%\MSBuild\15.0\Bin" GOTO USE_VS15_X64
:USE_VS14_X86
	echo Compiling Theme using Visual Studio 2015 MSBuild (32-bit)...
	"%programfiles%\MSBuild\14.0\Bin\MSBuild.exe" ZiosThemes.sln
:USE_VS15_X86
	echo Compiling Theme using Visual Studio 15 MSBuild (32-bit)...
	"%programfiles%\MSBuild\15.0\Bin\MSBuild.exe" ZiosThemes.sln
:USE_VS14_X64
	echo Compiling Theme using Visual Studio 2015 MSBuild (64-bit)...
	"%programfiles(x86)%\MSBuild\14.0\Bin\MSBuild.exe" ZiosThemes.sln
:USE_VS15_X64
	echo Compiling Theme using Visual Studio 15 MSBuild (64-bit)...
	"%programfiles(x86)%\MSBuild\15.0\Bin\MSBuild.exe" ZiosThemes.sln