import os,sys
path = os.getcwd().replace("\\","/") + "/../"
os.system("cls")
class Parser:
	def __init__(self):
		self.types = ["cs","sln","csproj"]
		self.newline = "\n"
	def Scan(self,path):
		for item in os.listdir(path):
			currentPath = path.strip("/")+"/"+item
			if os.path.isdir(currentPath):
				self.Scan(currentPath)
			for type in self.types:
				if type in currentPath:
					print('\033[95m'+currentPath+'\033[0m')
					output = ""
					try:
						file = open(currentPath,'r+b')
						for line in file:
							if line.strip() == "" : continue
							#line = line.rstrip().replace("[MenuItem(\"Zios/","[MenuItem(\"Edit/")
							output += line.rstrip() + self.newline
						file.close()
						file = open(currentPath,'wb')
						file.write(output.rstrip())
						file.close()
					except: pass
Parser().Scan(path)
os.system("pause")
