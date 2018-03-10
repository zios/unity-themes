import os
target = "Zios.Unity.Editor.Themes.csproj"
output = open(target,"w")
start = os.getcwd() + "\\..\\..\\..\\..\\"
include = '    <Compile Include="$item" />\n'
os.chdir(start)
def Scan(start):
	if not os.path.exists(target):
		print "Could not find target : " + target
		return
	Handle(target,True)
def Handle(csproj,save):
	references = []
	ignore = False
	for line in open(csproj):
		if "<ProjectReference Include" in line:
			ignore = True
			start = line.find("=")+1
			end = line.find(">")
			entry = line[start:end].strip('"')
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
			start = line.find("=")+1
			end = line.find("/>")
			entry = line[start:end].strip().strip('"')
			if entry not in references:
				references.append(entry)
		if save and not ignore:
			output.write(line)
	return references
Scan(start)
os.system("pause")