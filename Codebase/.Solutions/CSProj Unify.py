import os
target = "Zios.Unity.Editor.Themes.csproj"
output = open(target,"w")
start = os.getcwd() + "\\..\\..\\..\\..\\"
include = '    <Compile Include="$item" />\n'
hintPath = "      <HintPath>$item</HintPath>\n"
os.chdir(start)
def Scan(start):
	if not os.path.exists(target):
		print "Could not find target : " + target
		return
	Handle(target,True)
def Parse(text,start,end):
	startPosition = text.find(start)+len(start)
	endPosition = text.find(end)
	return text[startPosition:endPosition].strip().strip('"')
def Handle(csproj,save):
	references = []
	ignore = False
	file = open(csproj)
	for line in file:
		line = line.replace("\\","/")
		line = line.replace("Assets/@Zios/Codebase/","../")
		line = line.replace("Assets/@Zios/","../../")
		line = line.replace("Assets/","../../../")
		if "<None Include" in line :
			continue
		if "<TargetFrameworkProfile>Unity Full v3.5</TargetFrameworkProfile>" in line:
			ignore = True
		if "Release|AnyCPU" in line:
			if save : output.write("  </PropertyGroup>\n")
			ignore = False
		if "<Reference Include" in line:
			ignore = "Sigil" in line or "nunit.framework" in line or "Boo.Lang" in line
			if "Unity" in line:
				ignore = '"UnityEditor"' not in line and '"UnityEngine"' not in line;
		if "</Reference>" in line and ignore:
			ignore = False
			continue
		if "<HintPath>" in line:
			entry = Parse(line,"<HintPath>","</HintPath>")
			fixed = entry[entry.rfind("/")+1:]
			line = hintPath.replace("$item",fixed)
		if "<ProjectReference Include" in line:
			ignore = True
			entry = Parse(line,"=",">")
			for entry in Handle(entry,False):
				if entry not in references:
					references.append(entry)
			continue
		if "</ProjectReference>" in line:
			ignore = False
			continue
		if "Compile Include=" in line:
			if save and len(references) > 0:
				for entry in references:
					output.write(include.replace("$item",entry))
				references = []
			entry = Parse(line,"=","/>")
			if entry not in references:
				references.append(entry)
		if save and not ignore:
			output.write(line)
	file.close()
	return references
Scan(start)
os.system("pause")