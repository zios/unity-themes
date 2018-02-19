import os
start = os.getcwd() + "\\..\\"
def Scan(start):
	for item in os.listdir(start):
		path = start + item
		if os.path.isdir(path):
			os.chdir(path)
			Scan(path+"\\")
			os.chdir(start)
		if ".asmdef" in path and "Zios." in item and not item.startswith("."):
			fixedName = "."+item
			try: os.rename(item,fixedName)
			except : pass
Scan(start)
os.system("pause")
