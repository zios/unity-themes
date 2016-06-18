#!/bin/sh

rm ZiosThemes.dll
/Applications/Unity/Unity.app/Contents/MonoBleedingEdge/bin/xbuild ZiosThemesMono.sln
rm -R obj
