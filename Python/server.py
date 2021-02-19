import socket
import requests
import re
import json
from datetime import datetime
import serial
import os
from os.path import expanduser
import platform    # For getting the operating system name
import subprocess  # For executing a shell command
import socket
import netifaces as ni
import thread
import time

class telemetryCore:
    def __init__(self):
        print("iniciado")
        
        #pega diretorio home
        _home = expanduser("~")
        print("home Directory")
        print(_home)
        print("________________")
        
        #define pastas principais
        self.home = _home+"/.cemi"
        self.configFile = _home+"/.cemi/config"
        self.tagDictFile = _home+"/.cemi/tags"
        self.queueFile = _home+"/.cemi/queue"
        self.queueSocket = _home+"/.cemi/queueSocket"
        self.mobileNetworkConfigFile = "/etc/wvdial.conf"
        self.wifiNetworkConfigFile = _home+"/.cemi/wpa_supplicant"
        #self.wifiNetworkConfigFile = "/var/run/wpa_supplicant"
        
        #finaliza socket unix
        if(os.path.exists(self.queueSocket)):
            os.remove(self.queueSocket)
        
        #verifica se existem as pastar principais
        if(not os.path.isdir(self.home)):
            os.mkdir(self.home)
            
        if(not os.path.isfile(self.configFile)):
            tmp = open(self.configFile, "wb")
            tmp.write("{}")
            tmp.close()
            
        if(not os.path.isfile(self.tagDictFile)):
            tmp = open(self.tagDictFile, "wb")
            tmp.write("{}")
            tmp.close()
            
        if(not os.path.isfile(self.queueFile)):
            tmp = open(self.queueFile, "wb")
            tmp.write("[]")
            tmp.close()
            
        if(not os.path.isfile(self.mobileNetworkConfigFile)):
            tmp = open(self.mobileNetworkConfigFile, "wb")
            tmp.write('''[Dialer 3gconnect]\n
                        Init1 = ATZ\n
                        Init2 = ATQ0 V1 E1 S0=0 &C1 &D2 +FCLASS=0\n
                        Init3 = AT+CGDCONT=1,"IP","internet"\n
                        Stupid Mode = 1\n
                        Modem Type = Analog Modem\n
                        ISDN = 0\n
                        Phone = *99#\n
                        Modem = /dev/gsmmodem\n
                        Username = %apnuser%\n
                        Password = %apnpass%\n
                        Baud = 460800''')
            tmp.close()
            
        if(not os.path.isfile(self.wifiNetworkConfigFile)):
            tmp = open(self.wifiNetworkConfigFile, "wb")
            tmp.write(
            '''country=US # Your 2-digit country code
            ctrl_interface=DIR=/var/run/wpa_supplicant GROUP=netdev
            network={
                ssid="%wifiuser%"
                psk="%wifipass%"
                key_mgmt=WPA-PSK
            }''')
            tmp.close()
        
#           define os comaandos aceitos array {"command": "fields separated by colma"}
        self.commandSet = {}
        self.commandSet["setapnname()"] = "data"
        self.commandSet["setapnpwd()"] = "data"
        self.commandSet["setwifissid()"] = "data"
        self.commandSet["setwifipwd()"] = "data"
        self.commandSet["setproject()"] = "data"
        self.commandSet["setmachineid()"] = "data"
        self.commandSet["sethostname()"] = "data"
        self.commandSet["sethostport()"] = "data"
        self.commandSet["gettagid()"] = "data"
        self.commandSet["settagdata()"] = "data,date"
        self.commandSet["getsignal()"] = ""
        self.commandSet["getconnectionstatus()"] = ""
        self.commandSet["getqueue()"] = ""
        self.commandSet["cleanqueue()"] = ""
        self.commandSet["getip()"] = ""
        self.commandSet["getpinginfo()"] = ""
        self.commandSet["restart()"] = ""
        
        #carrega configuracao no inicio da aplicacao
        self.loadConfig(self.configFile)
        
        #inicializa o gerente de tags
        self.tagsmgmt = tagsMgmt(self.tagDictFile)
        
        #inicializa servidor de filas
        self.queuemgmt = queueMgmt()
        thread.start_new_thread(self.queuemgmt.startSocket, tuple())
                
    def loadConfig(self, configFile):
        tmp = open(self.configFile, "rb")
        self.config = json.loads(tmp.read())
        tmp.close()
    
    def saveConfig(self, configFile):
        tmp = open(self.configFile, "wb")
        tmp.write(json.dumps(self.config))
        tmp.close()
        
    def messageValidation(self, message):
#           Predeclara Variaveis para evitar valores nulos ou flutuantes            
        length = 0
        data = ""
        jsonData = {}
        
#           verifica a presenca do tamanho da mensagem
        if(message.count("|") != 2):
            print("length not found")
            return None
        
#           recupera tamanho definido da mensagem
        try:
            length = int(message[message.index("|")+1:message.rindex("|")])
            print(length)
        except:
            print("DataLength is Not a Integer")
            return None
        
#           Recupera dados passados na mensagem
        try:
            data = message[message.rindex("|")+1:len(message)]
            print(data)
            print(len(data))
        except:
            print("data does not exists")
            return None
        
#           Verifica tamanho dos dados passados na mensagem
        if(len(data) != length):
            print("data Length Missmatch")
            return
        
#           Verifica se os dados sao um json valido            
        try:
            jsonData = json.loads(data)
            print(jsonData)
        except:
            print("Data is not a json data")
            return None
        
#           Verifica campos necessarios ao json
        if("command" not in jsonData.keys()):
            print("there is not a command on data")
            return None
        
#           Verifica se comando existe
        if(jsonData["command"] not in self.commandSet):
            print("invalid Command")
            return None
        
#           Verifica se tem os campos necessarios para o comando
        if(len(self.commandSet[jsonData["command"]]) >= 1):
            for field in self.commandSet[jsonData["command"]].split(","):
                if(field not in jsonData):
                    print("the data provided information is not enought -> " + field + " missmath")
                    return
                
                print(field)
                
#                   Valida data no Formato 2021-01-29 16:54:55
                if(field == "date"):
                    format = "%Y-%m-%d %H:%M:%S"
                    try:
                        datetime.strptime(jsonData[field], format)
                    except ValueError:
                        print("This is the incorrect date string format. It should be %Y-%m-d h:m:s")
                        jsonData[field] = jsonData[field]
                        return None
                
        
#           retorna Json apos Validacao
        return jsonData

    def sendResponse(self, message):
        try:
            json.loads(message)
        except:
            print("its not a json data")
            return None
        
        resp = "|" +str(len(message))+ "|" + message
        
        print(resp)
        return resp
    
    #Processa comando enviado
    def processCommand(self, command):
        #define o nome da apn de rede
        if(command["command"].lower() == "setapnname()"):
            if(len(command["data"]) >= 2):
                self.config["apn"] = command["data"]
                self.saveConfig(self.configFile)
                
                self.setApnConfig(command["data"], None, None)
                return (True, "success")
            else:
                return (False, "fail")
        
        #define usuario da apn de rede de telefonia
        elif(command["command"].lower() == "setapnuser()"):
            if(len(command["data"]) >= 2):
                self.config["apnuser"] = command["data"]
                self.saveConfig(self.configFile)
                
                self.setApnConfig(None, command["data"], None)
                return (True, "success")
            else:
                return (False, "fail")
        
        #define password da apn de telefonia    
        elif(command["command"].lower() == "setapnpass()"):
            if(len(command["data"]) >= 2):
                self.config["apnpass"] = command["data"]
                self.saveConfig(self.configFile)
                
                self.setApnConfig(None,None, command["data"])
                return (True, "success")
            else:
                return (False, "fail")
        
        #define o ssid da wireless    
        elif(command["command"].lower() == "setwifissid()"):
            if(len(command["data"]) >= 3):
                self.config["wifissid"] = command["data"]
                self.saveConfig(self.configFile)
                
                self.setWifiConfig(command["data"], None)
                    
                return (True, "success")
            else:
                return (False, "fail")
        
        #define o password de rede wifi    
        elif(command["command"].lower() == "setwifipwd()"):
            if(len(command["data"]) >= 8):
                self.config["wifipwd"] = command["data"]
                self.saveConfig(self.configFile)
                
                self.setWifiConfig(None, command["data"])
                    
                return (True, "success")
            else:
                return (False, "fail")
        
        #define o nome do projeto para sincronizar com a nuvem    
        elif(command["command"].lower() == "setproject()"):
            if(len(command["data"]) >= 3):
                self.config["project"] = command["data"]
                self.saveConfig(self.configFile)
                return (True, "success")
            else:
                return (False, "fail")
        
        #define o id da maquina para sincronizar com a nuvem   
        elif(command["command"].lower() == "setmachineid()"):
            if(len(command["data"]) == 32):
                self.config["machineid"] = command["data"]
                self.saveConfig(self.configFile)
                return (True, "success")
            else:
                return (False, "fail")
        
        #define o hostname da nuvem para sincronizacao   
        elif(command["command"].lower() == "sethostname()"):
            if(len(command["data"]) >= 4):
                self.config["hostname"] = command["data"]
                self.saveConfig(self.configFile)
                return (True, "success")
            else:
                return (False, "fail")
        
        #define porta de sincronizacao de rede    
        elif(command["command"].lower() == "sethostport()"):
            if(len(command["data"]) >= 4):
                self.config["hostport"] = command["data"]
                self.saveConfig(self.configFile)
                return (True, "success")
            else:
                return (False, "fail")
        
        #define todos ids das tags para sincronizar    
        elif(command["command"].lower() == "gettagid()"):
            print(type(command["data"]) is list)
            
            if(type(command["data"]) is not list):
                print("something went wrong with command data")
                return (False, "fail")
            
            response = tc.syncTags(command["data"])
            
            #adiciona ao arquivo de tags
            self.tagsmgmt.updateTags(response)
            
            print("gettagid")
            return (True, response)
        
        #envia dados de tag para a nuvem    
        elif(command["command"].lower() == "settagdata()"):
            print("settagdata")
            self.queueMgmtaddToQueue(command["data"])
            
            #envia todas as tags da fila
            count = int(self.queueMgmtgetLenght())
            print("count -> ", count)
            response = "{}"
            
            for i in range(count):
                #tenta fazer o envio
                response = self.sendTagData(self.queueMgmtgetNext())
            
            return (True, response)
        
        #faz leitura do sinal de rede    
        elif(command["command"].lower() == "getsignal()"):
            print("getsignal")
            return (True, "")
        
        #verifica status da conexao de rede atravez de ping a um dns    
        elif(command["command"].lower() == "getconnectionstatus()"):
            if(self.ping("1.1.1.1")):
                return (True, "connected")
            else:
                return (False, "disconnected")
        
        #recupera numero de arquivos na fila
        elif(command["command"].lower() == "getqueue()"):
            print(self.queueMgmtgetLenght())
            #print(self.queueMgmtgetNext())
            return (True, "{'length': "+str(self.queueMgmtgetLenght() + "}"))
        
        #limpa fila de envio    
        elif(command["command"].lower() == "cleanqueue()"):
            if(self.queueMgmtclean()):
                print()
                return (True, "success")
            else:
                return (False, "fail")
        
        #verifica o ip do dispositivo
        elif(command["command"].lower() == "getip()"):
            print("getip")
            ni.ifaddresses('eth0')
            ip = ni.ifaddresses('eth0')[ni.AF_INET][0]['addr']
            print(ip)  # should print "192.168.100.37"
            return (True, "{'ipaddress' : '" + ip + "'}")
        
        #faz analize de rete atravez de ping    
        elif(command["command"].lower() == "getpinginfo()"):
            print("getpinginfo")
            
            pingdnstime = str(self.pingSpeed("1.1.1.1"))
            pinghosttime = str(self.pingSpeed(self.config["hostname"]))
            
            print( (True, [pingdnstime, pinghosttime]))
            return (True, [pingdnstime, pinghosttime])
        
        #reinicia servico na maquina    
        elif(command["command"].lower() == "restart()"):
            print("restart")
            return (True, "")
        
    #faz a configuracao da apn
    def setApnConfig(self, apnName=None, apnUser=None, apnPass=None):        
        print("configurando apn")
        if(os.path.isfile(self.mobileNetworkConfigFile)):
            tmp = open(self.mobileNetworkConfigFile, "rb")
            data = tmp.read()
            tmp.close()
            
            if(apnUser):
                data = re.sub(r'Username = [\S*\d*_.-]*', 'Username = '+apnUser , data)  #data.replace("%apnuser%", apnUser)
                self.config["apnuser"] = apnUser
            if(apnPass):
                data = re.sub(r'Password = [\S*\d*_.-]*', 'Password = '+apnPass, data)  #data.replace("%apnpass%", apnPass)
                self.config["apnpass"] = apnPass
            if(apnName):
                data = re.sub(r'Apn = [\S*\d*_.-]*', 'Apn = '+apnName, data)  #data.replace("%apnname%", apnName)
                self.config["apnname"] = apnName
    
            tmp = open(self.mobileNetworkConfigFile, "wb")
            tmp.write(data)
            tmp.close()
            
    #faz a configuracao da wifi
    def setWifiConfig(self, wifiUser=None, wifiPass=None):        
        print("configurando wifi")
        if(os.path.isfile(self.wifiNetworkConfigFile)):
            tmp = open(self.wifiNetworkConfigFile, "rb")
            data = tmp.read()
            tmp.close()
            
            if(wifiUser):
                data =  re.sub(r'ssid="[\S*\d*_.-]*"', 'ssid="'+wifiUser+'"', data)  #data.replace("%wifiuser%", wifiUser)
                self.config["wifiuser"] = wifiUser
                
            if(wifiPass):
                data =  re.sub(r'psk="[\S*\d*_.-]*"', 'psk="'+wifiPass+'"', data)   #data.replace("%wifipass%", wifiPass)
                self.config["wifipass"] = wifiPass
    
            tmp = open(self.wifiNetworkConfigFile, "wb")
            tmp.write(data)
            tmp.close()


    def ping(self, host):
        """
        Returns True if host (str) responds to a ping request.
        Remember that a host may not respond to a ping (ICMP) request even if the host name is valid.
        """

        # Option for the number of packets as a function of
        param = '-n' if platform.system().lower()=='windows' else '-c'

        # Building the command. Ex: "ping -c 1 google.com"
        command = ['ping', param, '1', host]

        return subprocess.call(command) == 0
    
    def pingSpeed(self, host):
        """
        Returns True if host (str) responds to a ping request.
        Remember that a host may not respond to a ping (ICMP) request even if the host name is valid.
        """

        # Option for the number of packets as a function of
        param = '-n' if platform.system().lower()=='windows' else '-c'

        # Building the command. Ex: "ping -c 1 google.com"
        command = ['ping', param, '1', host]

        sprocResult = str(subprocess.check_output(command))
        
        pingdnstime = re.finditer(r"time=(\d+\.*\d*\s*ms)", sprocResult, re.MULTILINE)
        for matchNum, match in enumerate(pingdnstime, start=1):
            return (match.group(1) if matchNum == 1 else None)
        
        return None
    
    #cria uma thread para manipular a fila
    def queueMgmtgetNext(self):
        tcp = socket.socket(socket.AF_UNIX, socket.SOCK_STREAM)
        tcp.connect(self.queueSocket)
        tcp.send('{"command": "next"}')
        recvData = tcp.recv(1024)
        tcp.close()
        
        return recvData        

        
    def queueMgmtgetLenght(self):
        tcp = socket.socket(socket.AF_UNIX, socket.SOCK_STREAM)
        tcp.connect(self.queueSocket)
        tcp.send('{"command": "length"}')
        recvData = tcp.recv(1024)
        tcp.close()
        
        return recvData        
        
        
    def queueMgmtaddToQueue(self, data):
        tcp = socket.socket(socket.AF_UNIX, socket.SOCK_STREAM)
        tcp.connect(self.queueSocket)
        tcp.send('{"command": "write", "data":  '+json.dumps(data)+'}')
        recvData = tcp.recv(1024)
        tcp.close()
        
        return True    
        
    def queueMgmtclean(self):
        tcp = socket.socket(socket.AF_UNIX, socket.SOCK_STREAM)
        tcp.connect(self.queueSocket)
        tcp.send('{"command": "clean"}')
        recvData = tcp.recv(1024)
        tcp.close()    
        
        return True        
    
    def syncTags(self, tags):
        #post em /setuptags
        #dados enviados como {'data': ['{tag name}:{tag address}:{tag type}','{tag name}:{tag address}:{tag type}']}
        #dados recebidos como {'data': {'tag':'id','tag','id'}}
        
        if (("hostname" not in self.config) and 
                ("hostport" not in self.config)):
            print("Configuracao incompleta")
            return {}
        
        print("sync Tags")
        body = {'data': tags}
        
        req = requests.post("http://"+self.config["hostname"]+":"+self.config["hostport"]+"/setuptags", None, body)
        
        if(req.ok):
            resp = json.loads(req.text)
            return resp
        else:
            return {}
        
    def sendTagData(self, data):
        #post em /setuptags
        #dados para processar
        #project machineid date devicenumber deviceimei data(na requisicao)
        if(("project" not in self.config) or 
            ("machineid" not in self.config) or 
                ("deviceimei" not in self.config) or
                    ("hostname" not in self.config) or 
                        ("hostport" not in self.config)):
            
            print("configuracao incompleta")
            return {}
        
        #if(("data" not in data) and 
        #    ("date" not in data)):
        #    print("requisicao incompleta")
        #    return {}
        
        print("sync Tags")
        body = {'project': self.config["project"], 'machineid': self.config["machineid"], 'date': str(datetime.now()), 'deviceimei': self.config["deviceimei"], 'devicenumber': 0, 'data': data}
        
        req = requests.post("http://"+self.config["hostname"]+":"+self.config["hostport"]+"/tagdata", None, body)
        
        if(req.ok):
            print(req.text)                
            resp = json.loads(req.text.replace("\'", "\""))
            return resp
        else:
            return {}
        
        #connection.request("POST", '/setuptags', body=json.dumps(body))
        
class queueMgmt:
    def __init__(self):         
        
        #pega diretorio home
        _home = expanduser("~")
        
        #define pastas principais
        self.home = _home+"/.cemi"
        
        self.queueFile = _home+"/.cemi/queue"
        self.queueSocket = _home+"/.cemi/queueSocket"
        self.lock = False
        self.toWrite = ""
        self.lastWrite = None   
        self.socket = None
        self.next = {}
        
        #verifica se existem as pastar principais
        if(not os.path.isdir(self.home)):
            os.mkdir(self.home)
            
        if(not os.path.isfile(self.queueFile)):
            tmp = open(self.queueFile, "wb")
            tmp.write("[]")
            tmp.close()
        
    def messaging(self, con, client):    
        print('Conectado por' + str(client))

        while(True):
            msg = con.recv(1024)
            
            print(msg)
            
            if(not msg):
                break
            
            try:            
                loadedMsg = json.loads(msg)
                
                print("loadedMsg -> ", loadedMsg)
                
                if(loadedMsg["command"] == u"next"):
                    con.sendall(self.queueMgmtgetNext())
                elif(loadedMsg["command"] == u"length"):
                    con.sendall(self.queueMgmtgetLenght())
                elif(loadedMsg["command"] == u"clean"):
                    con.sendall(str(self.queueMgmtclean()))
                elif(loadedMsg["command"] == u"write"):
                    con.sendall(str(self.queueMgmtaddToQueue(loadedMsg["data"])))
                    
                del loadedMsg

            except Exception as e:
                print("Erro to Translate Json Command")
                print(e)
                con.sendall("error")
            
        print('Finalizando conexao do cliente', client)
        con.close()
        thread.exit()
        
    def startSocket(self):
        tcp = socket.socket(socket.AF_UNIX, socket.SOCK_STREAM)
        tcp.bind(self.queueSocket)
        tcp.listen(1)

        while True:
            con, cliente = tcp.accept()
            thread.start_new_thread(self.messaging, tuple([con, cliente]))

        tcp.close()
                
    #recupera proximo do aquivo de fila
    def queueMgmtgetNext(self):
        while(self.lock):
            print("data is locked")
            time.sleep(1000)
            
        #bloqueia o arquivo para escrita e leitura
        self.lock = True
            
        tmp = open(self.queueFile, "rb")
        loaded = json.loads(tmp.read())
        tmp.close()
        
        if(len(loaded) > 0):
            nextData = loaded.pop(0)
        else:
            nextData = ""
        
        tmp = open(self.queueFile, "wb")
        tmp.write(json.dumps(loaded))
        tmp.close()
        
        del loaded
            
        #desbloqueia o arquivo para escrita e leitura
        self.lock = False
        
        return json.dumps(nextData)
        
    #recupera tamanho da fila
    def queueMgmtgetLenght(self):
        while(self.lock):
            print("data is locked")
            time.sleep(1000)
            
        #bloqueia o arquivo para escrita e leitura
        self.lock = True
            
        tmp = open(self.queueFile, "rb")
        loaded = json.loads(tmp.read())
        tmp.close()
        
        l = len(loaded)
        
        del loaded
            
        #desbloqueia o arquivo para escrita e leitura
        self.lock = False
        
        return str(l)
        
        
    #adiciona os dados ao aquivo de fila
    def queueMgmtaddToQueue(self, data):
        while(self.lock):
            print("data is locked")
            time.sleep(1000)
            
        #bloqueia o arquivo para escrita e leitura
        self.lock = True
        
        tmp = open(self.queueFile, "rb")
        loaded = json.loads(tmp.read())
        tmp.close()
        
        loaded.append(data)
        
        tmp = open(self.queueFile, "wb")
        tmp.write(json.dumps(loaded))
        tmp.close()
        
        del loaded
        
        #desbloqueia o arquivo para escrita e leitura
        self.lock = False
        
        return True    
        
    #limpa aquivo de fila
    def queueMgmtclean(self):
        while(self.lock):
            print("data is locked")
            time.sleep(1000)
        
        #bloqueia o arquivo para escrita e leitura
        self.lock = True
            
        tmp = open(self.queueFile, "wb")
        tmp.write("[]")
        tmp.close()
            
        #desbloqueia o arquivo para escrita e leitura
        self.lock = False
        
        return True   
    
class tagsMgmt:
    def __init__(self, tagfile):
        self.tagFile = tagfile
    
    #adiciona tags a lista de tags do arquivo    
    def appendTags(self, tags):
        tagContents = open(self.tagFile, "rb")
        data = json.loads(tagContents.read())
        tagContents.close()
        
        if(type(tags) == list):
            print("tags is list")
            for tag in tags:
                print(type(tags))
                if(type(tag) == dict):
                    #adiciona tag a lissta do arquivo
                    print("tags is dict")
                    if(tag.keys()[0] not in data):
                        data[tag.keys()[0]] = tag[tag.keys()[0]]
                        
        elif(type(tags) == dict):
            print("tags is dict")
            #adiciona tag a lissta do arquivo
            if(tags.keys()[0] not in data):
                data[tags.keys()[0]] = tags[tags.keys()[0]]
    
        else:
            print("invalid")
            
        print(data)
        tagContents = open(self.tagFile, "wb")
        tagContents.write(json.dumps(data))
        tagContents.close()
            
    #verifica se contem dados no arquivo de tag
    def containsTag(self, tag):
        tagContents = open(self.tagFile, "r")
        data = json.loads(tagContents.read())
        tagContents.close()
        
        if(type(tag) == list):
            print("tags is list")
                
        elif(type(tag) == dict):
            print("tags is dict")
            #adiciona tag a lissta do arquivo
            if(tag.keys()[0] not in data):
                return True
        
        else:
            print("invalid")
            
        return False   
     
    #atualiza sem verificar se existe
    def updateTags(self, tags):
        tagContents = open(self.tagFile, "rb")
        data = json.loads(tagContents.read())
        tagContents.close()
        
        if(type(tags) == list):
            print("tags is list")
            for tag in tags:
                print(type(tags))
                if(type(tag) == dict):
                    #adiciona tag a lissta do arquivo
                    print("tags is dict")
                    data[tag.keys()[0]] = tag[tag.keys()[0]]
                        
        elif(type(tags) == dict):
            print("tags is dict")
            #adiciona tag a lissta do arquivo
            data[tags.keys()[0]] = tags[tags.keys()[0]]
    
        else:
            print("invalid")
            
        print(data)
        tagContents = open(self.tagFile, "wb")
        tagContents.write(json.dumps(data))
        tagContents.close()
            
      
    #atualiza sem verificar se existe
    def getTags(self, tags):
        tagContents = open(self.tagFile, "rb")
        data = json.loads(tagContents.read())
        tagContents.close()
        
        #variavel para guardar respostas
        response = {}
        
        print(type(tags))
        
        if(type(tags) == list):
            print("tags is list")
            for tag in tags:
                print(type(tags))
                if(type(tag) == dict):
                    #adiciona tag a lissta do arquivo
                    print("tags is dict")
                    response[tag.keys()[0]] = data[tag.keys()[0]]
                    
                elif(type(tag) == str):
                    #adiciona tag a lissta do arquivo
                    print("tags is text")
                    response[tag] = data[tag]
                        
        elif(type(tags) == dict):
            print("tags is dict")
            #adiciona tag a lissta do arquivo
            response[tags.keys()[0]] = data[tags.keys()[0]]
        
        elif(type(tags) == str):
            response[tags] = data[tags]
        
        return response
            
    #adiciona tags a lista de tags do arquivo    
    def removeTags(self, tags):
        tagContents = open(self.tagFile, "rb")
        data = json.loads(tagContents.read())
        tagContents.close()
        
        if(type(tags) == list):
            print("tags is list")
            for tag in tags:
                print(type(tags))
                if(type(tag) == dict):
                    #adiciona tag a lissta do arquivo
                    print("tags is dict")
                    if(tag.keys()[0] in data):
                        del data[tag.keys()[0]]
                        
        elif(type(tags) == dict):
            print("tags is dict")
            #adiciona tag a lissta do arquivo
            if(tags.keys()[0] in data):
                del data[tags.keys()[0]]
    
        else:
            if(tags in data):
                del data[tags]
            
        print(data)
        tagContents = open(self.tagFile, "wb")
        tagContents.write(json.dumps(data))
        tagContents.close()
            
    #adiciona tags a lista de tags do arquivo    
    def cleanTags(self):         
        tagContents = open(self.tagFile, "wb")
        tagContents.write(json.dumps({}))
        tagContents.close()
            

#inicializa o gerente de tags
_home = expanduser("~")
tagDictFile = _home+"/.cemi/tags"

tagsmgmt = tagsMgmt(tagDictFile)
tagsmgmt.appendTags([{"nome": "Eduardo"}])


print(tagsmgmt.getTags([{"nome": "Eduardo"}]))
print(tagsmgmt.getTags(["nome"]))
print(tagsmgmt.getTags("nome"))

tagsmgmt.removeTags("nome")
tagsmgmt.cleanTags()


#inicializa telemetry core
tc = telemetryCore()

ser = serial.Serial('/dev/ttyS0')  # open serial port
#print(ser.name)         # check which port was really used
#ser.write(b'hello')     # write a string
#ser.close()             # close port

print("Sync tags")
print(tc.syncTags(['{tag name}:{tag address}:{tag type}','{tag name}:{tag address}:{tag type}']))
print("------------------------------------")

while(True):
    jsonData = tc.messageValidation(ser.readline())
    if(jsonData == None):
        continue
    print(jsonData)    
    
    #captura resposta
    response = tc.processCommand(jsonData)
    
    #jsonData = tc.messageValidation("|79|{\"command\":\"settagdata()\", \"data\":\"settagdata()\", \"date\":\"2021-01-29 16:54:55\"}")

    ser.write(tc.sendResponse(json.dumps({'date' : str(datetime.now()), 'status': response[0], 'desc': response[1]})) + "\r\n")

