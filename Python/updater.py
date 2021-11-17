#servico de atualizacao da interface
import subprocess
import time
import os, sys
from datetime import datetime

#repositorio de atualizacoes
repositorio = "https://CemiEngenharia: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@github.com/CemiEngenharia/OptProcessBISenderUpdater.git"

#progam path
ppath = "/var/cemi/.tmp/"
ppathold = "/var/cemi/.old/"
ppexe = "/var/cemi/"
ppexefile = "server.py"

#verifica se a pasta existe
if(not os.path.exists(ppexe)):
    os.mkdir(ppath)
    
#verifica se a pasta existe
if(not os.path.exists(ppath)):
    os.mkdir(ppath)
    
#verifica se a pasta existe
if(not os.path.exists(ppathold)):
    os.mkdir(ppathold)

#define a seguranca da pasta atual
os.system("chmod 777 /etc/cemi -R")
os.system("chown pi:pi /etc/cemi -R")

#define o diretorio de funcionamento do script
os.chdir(ppath)

#os commandos git
gitSetHttpsA = ['git','config','--global','url."https://github.com/".insteadOf','git@github.com:']      #configura git para https
gitSetHttpsB = ['git','config','--global','url."https://".insteadOf','git://']                          #configura git para https
gitClone = ["git","clone",repositorio, "."]                                                             #clona repositorio
gitPull = ["git","pull"]                                                                                #atualiza a partir do repositorio

#os command process
stopServer = ["kill","-9","pid"]
startServer = ["/usr/bin/python","server.py","&"]
validateServer = ["/usr/bin/python","-m", "py_compile", "server.py"]

#preconfigura o git para https
subprocess.call(gitSetHttpsA)
subprocess.call(gitSetHttpsB)

#programm pid
pid = None

def log(data):
    print(data)
    f = open("/etc/cemi/logfile.log", "a")
    f.write(str(datetime.now()) + " - " + data+"\r\n")
    f.close()

#cria um loop para iniciar a aplicacao
while True:
    try:
        log("verificando se executavel existe")
        if(not os.path.isfile(ppexefile)):
            #caso nao exista clona repositorio
            subprocess.call(gitClone)

    
        #atualiza projeto na pasta
        pullResult = subprocess.check_output(gitPull)
        log("resultado do pull -> "+ str(pullResult))
        
        #Mata o Porjeto caso o pull tenha sucesso
        if("Already up to date" not in pullResult):
            #mata processo para atualizacao
            if(pid is not None):
                pid.kill()
                                   
            #copia dados para a pasta de execucao
            log("fazendo Backup")
            os.system("cp -vfr "+ppexe+"*.* "+ppathold)
            
            #copia dados para a pasta de execucao
            log("copiando novos arquivos")
            os.system("cp -vfr "+ppath+"* "+ppexe)
            pid = None
        
        #inicia processo do servidor guardando o pid
        if(pid is None):
            #define o diretorio de funcionamento do script
            os.chdir(ppexe)
            
            try:
                #Verifica script antes de executar
                if(subprocess.check_output(validateServer).lower().count("sorry") >= 1):
                    #copia dados para a pasta de execucao
                    log("recuperando versao anterior")
                    os.system("cp -vfr "+ppathold+"* "+ppexe)                    
                
            except:
                #copia dados para a pasta de execucao
                log("recuperando versao anterior")
                os.system("cp -vfr "+ppathold+"* "+ppexe)
            
            #executa aplicacao
            FNULL = open(os.devnull, 'w')
            pid = subprocess.Popen(startServer, stdout=FNULL, stderr=subprocess.STDOUT)
            #define o diretorio de funcionamento do script
            os.chdir(ppath)
                
            log("pid do processo criado -> "+ str(pid.pid))
            
        else:
            #verifica se processo continua rodando    
            poll = pid.poll()
            if poll is not None:
                log("process is Dead")
                # p.subprocess is alive
                pid = None
            else:
                log("script is running")
                
        #da um tempo de 10 minutos para conferir de novo
        time.sleep(5)
    
    except KeyboardInterrupt: 
        if(pid is not None):
            log("keyboard interrupt -> encerrando processo")
                
        # quit
        sys.exit()
        
    except Exception as e:
        print(e)