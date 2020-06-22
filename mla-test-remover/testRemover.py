import os
import shutil


def checkDirectory(savepath, dirname=None):
    if dirname is not None:
        if os.path.exists(savepath + dirname):
            return True
        else:
            return False
    else:
        if os.path.exists(savepath):
            return True
        else:
            return False


def checkFile(complete_filename):
    if os.path.exists(complete_filename):
        return True
    else:
        return False


print("ML-AGENTS TEST REMOVER by Francesco Abate")
config_path = str(input("Insert your config folder: "))
ok_config = False
deletion_done = False
while not ok_config:
    if config_path is not None:
        if checkDirectory(config_path) and checkDirectory(config_path, "\\models") and \
                checkDirectory(config_path, "\\summaries"):
            ok_config = True
        else:
            config_path = str(input("Wrong config folder. Retry: "))
behavior_name = str(input("Insert the behavior name (NN name) for this session: "))
while not deletion_done:
    testname = str(input("Insert the test name that you want to delete: "))
    if checkFile(config_path + "\\models\\" + testname):
        shutil.rmtree(config_path + "\\models\\" + testname)
        print("> Removed folder: ", config_path + "\\models\\" + testname)
    if checkFile(config_path + "\\summaries\\" + testname + "_" + behavior_name):
        shutil.rmtree(config_path + "\\summaries\\" + testname + "_" + behavior_name)
        print("> Removed directory: ", config_path + "\\summaries\\" + testname + "_" + behavior_name)
    if checkFile(config_path + "\\summaries\\" + testname + "_" + behavior_name + ".csv"):
        os.remove(config_path + "\\summaries\\" + testname + "_" + behavior_name + ".csv")
        print("> Removed file: ", config_path + "\\summaries\\" + testname + "_" + behavior_name + ".csv")
    if checkFile(config_path + "\\summaries\\" + testname + "_timers.json"):
        os.remove(config_path + "\\summaries\\" + testname + "_timers.json")
        print("> Removed file: ", config_path + "\\summaries\\" + testname + "_timers.json")
    choise = str(input("Do you want to delete another test? Insert y for YES, n for NO: "))
    if choise == 'n':
        deletion_done = True
print("Closed")
exit()
