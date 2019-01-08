import os
class AsmData:
	def __init__(self):
		self.path = ""
		self.scope = ""
		self.references = []
class AsmGenerate:
	def __init__(self):
		self.start = os.getcwd() + "\\..\\"
		self.template = open("Asmdef Template.txt","r").read()
		self.data = {}
		self.duplicates = []
		self.active = None
		self.Scan(self.start)
		self.Save()
	def Scan(self,currentPath):
		for item in os.listdir(currentPath):
			path = currentPath + item
			if os.path.isdir(path):
				os.chdir(path)
				self.Scan(path+"\\")
				os.chdir(currentPath)
			if ".cs" in path and ".cs.meta" not in path and ".csproj" not in path:
				for line in open(item,"r").readlines():
					if "Zios." in line:
						if "namespace" in line :
							currentScope = line.split("namespace")[-1].replace("{","").strip()
							if currentScope not in self.data:
								if currentPath not in self.duplicates and [currentPath == self.data[x].path for x in self.data].count(True) > 0:
									print "Duplicate path found -- " + currentPath
									self.duplicates.append(currentPath)
								self.data[currentScope] = self.active = AsmData()
								self.active.path = currentPath
								self.active.scope = currentScope
							self.active = self.data[currentScope]
							if self.active.path not in currentPath:
								while self.active.path not in currentPath:
									self.active.path = self.active.path[:self.active.path.strip("\\").rfind("\\")]+"\\"
								#print currentPath + " forced repath to -- " + self.active.path
						entry = None
						if "//asm" in line:
							entry = line.split("//asm")[-1].strip().strip(";")
						if "using" in line:
							entry = line.split("using")[-1].strip().strip(";")
							if "=" in line:
								entry =  ".".join(entry.split("= ")[-1].split(".")[:-1])
						if entry is not None and entry not in self.active.references and entry != self.active.scope:
							self.active.references.append(entry)
	def Save(self):
		for name in self.data:
			active = self.data[name]
			active.references.sort()
			active.references = [('"'+x+'"') for x in active.references]
			refs = ""
			if len(active.references) > 0:
				refs = "\n\t\t"+",\n\t\t".join(active.references)+"\n\t"
			asm = self.template.replace("$asmdef",active.scope)
			asm = asm.replace("$references",refs)
			if ".Editor." in active.scope:
				asm = asm.replace("$editorOnly",'"'+"Editor"+'"')
			asm = asm.replace("$editorOnly","")
			asmPath = active.path+"\\"+active.scope+".asmdef"
			file = open(asmPath,"w")
			file.write(asm)
			file.close()
AsmGenerate()
os.system("pause")
