#!/bin/sh
rm ZiosThemes*.dll
/Applications/Unity/Unity.app/Contents/MonoBleedingEdge/bin/xbuild ZiosThemes.sln
rm -R obj
