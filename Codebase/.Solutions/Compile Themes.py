import os,sys
from shutil import copyfile as copy

company = "Zios"
project = "ZiosThemes.sln"
guid = "90c40447-c2b4-4725-9cea-9e0e8199eefc"
title = project.split(".")[0]

if len(sys.argv) == 1 :
	isWindows = os.name == "nt"
	if isWindows : os.system("cls")
	else : os.system("clear")
msBuildPath = '"D:/Installed/Microsoft Visual Studio 2017/MSBuild/15.0/Bin/amd64/MSBuild.exe" '
versionSVN = os.popen('svn info -r HEAD --show-item revision').read().strip()
versionUnity = len(sys.argv) > 1 and sys.argv[1] or raw_input("Unity Version? ")

def Version(path):
	file = open(path,"r+b")
	contents = ""
	for line in file:
		line = line.replace("{company}",company)
		line = line.replace("{title}",title)
		line = line.replace("{guid}",guid)
		line = line.replace("{revision}",versionSVN)
		line = line.replace("{unityversion}",versionUnity)
		contents += line
	file.seek(0)
	file.write(contents)
	file.truncate()
	file.close()

versionValue = versionUnity.count(".") > 1 and float("".join(versionUnity.rsplit(".",1))) or float(versionUnity)
platform = len(sys.argv) > 2 and sys.argv[2] or raw_input("Platform? ").lower()
if "win" in platform : platform = "WIN"
if "osx" in platform or "mac" in platform : platform = "OSX"
directives = "UNITY_EDITOR;UNITY_EDITOR_"+platform+";ZIOS_MINIMAL;"
if versionValue >= 5    and versionValue < 6   : directives += "UNITY_5;"
if versionValue >= 5.0  and versionValue < 5.1 : directives += "UNITY_5_0;"
if versionValue >= 5.1  and versionValue < 5.2 : directives += "UNITY_5_1;"
if versionValue >= 5.2  and versionValue < 5.3 : directives += "UNITY_5_2;"
if versionValue >= 5.3  and versionValue < 5.4 : directives += "UNITY_5_3;"
if versionValue >= 5.4  and versionValue < 5.5 : directives += "UNITY_5_4;"
if versionValue >= 5.5  and versionValue < 5.6 : directives += "UNITY_5_5;"
if versionValue >= 5.6  and versionValue < 5.7 : directives += "UNITY_5_6;"
if versionValue >= 2017 and versionValue < 2018 : directives += "UNITY_2017;"
if versionValue >= 5.3     : directives += "UNITY_5_3_OR_NEWER;"
if versionValue >= 5.34    : directives += "UNITY_5_3_4_OR_NEWER;"
if versionValue >= 5.4     : directives += "UNITY_5_4_OR_NEWER;"
if versionValue >= 5.5     : directives += "UNITY_5_5_OR_NEWER;"
if versionValue >= 5.6     : directives += "UNITY_5_6_OR_NEWER;"
if versionValue >= 2017    : directives += "UNITY_2017_OR_NEWER;"
if versionValue >= 2017.1  : directives += "UNITY_2017_1_OR_NEWER;"
if versionValue >= 2017.2  : directives += "UNITY_2017_2_OR_NEWER;"
if versionValue >= 2017.3  : directives += "UNITY_2017_3_OR_NEWER;"
if versionValue >= 2018    : directives += "UNITY_2018_OR_NEWER;"
if versionValue >= 2018.1  : directives += "UNITY_2018_1_OR_NEWER;"
if versionValue >= 2018.2  : directives += "UNITY_2018_2_OR_NEWER;"
if versionValue >= 2018.3  : directives += "UNITY_2018_3_OR_NEWER;"
directives = ' /p:DefineConstants="' + directives + '"'

copy("./Versions/"+versionUnity+"/UnityEngine.dll","UnityEngine.dll")
copy("./Versions/"+versionUnity+"/UnityEditor.dll","UnityEditor.dll")
copy("AssemblyInfoTemplate.cs","AssemblyInfo.cs")
copy("../@Unity/Editor/Systems/Themes/ThemeSystem.cs","ThemeSystem.cs")
Version("ThemeSystem.cs")
Version("AssemblyInfo.cs")

outputPath = ' /p:AssemblyName="ZiosThemes-r'+versionSVN+"-"+platform.title()+"-"+versionUnity+'"'
print "Compiling " + outputPath
print "-----------------------------"
open("Temp.bat","w").write(msBuildPath + project + directives + outputPath)
os.system("Temp.bat")
os.remove("Temp.bat")
os.remove("ThemeSystem.cs")
os.remove("AssemblyInfo.cs")
os.remove("UnityEngine.dll")
os.remove("UnityEditor.dll")
os.remove("./Release/UnityEngine.dll")
os.remove("./Release/UnityEditor.dll")
#if os.path.exists(outputPath) : os.remove(outputPath)
#os.rename("./Release/Zios.Unity.Editor.Themes.dll",outputPath)
if len(sys.argv) == 1 : raw_input("Press any key to close...")