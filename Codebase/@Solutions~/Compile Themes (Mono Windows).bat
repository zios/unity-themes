@ECHO OFF
echo Compiling ZiosThemes.dll - Mono Framework 4.5 / Unity 5.x...
del ZiosThemes*.dll
call "%programfiles%\Unity\Editor\Data\MonoBleedingEdge\bin\xbuild.bat" ZiosThemes.sln
rmdir /Q /S obj