import os

print("Git helper")

print("What do you want to do?")
print("1. Push changes to server")

branchName = "VFX"

choice = input("Choice:")

def confirm():
    correct = input("Is all of the info above correct (Y/N)?")

    return correct == "Y"

if choice == "1":
    print("Please enter a message describing the work you've undertaken")
    commitMessage = input("Commit message: ")

    confirmed = confirm()

    if confirmed == True:
        os.system("git add .")
        os.system("git commit -m \""+commitMessage+"\"")
        os.system("git push origin " + branchName)
    
    
