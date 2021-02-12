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
            if(command["command"].lower() == "setapnname()"):
                if(len(command["data"]) >= 2):
                    self.config["apn"] = command["data"]
                    self.saveConfig(self.configFile)
                    return (True, "success")
                else:
                    return (False, "fail")
                
            elif(command["command"].lower() == "setapnuser()"):
                if(len(command["data"]) >= 2):
                    self.config["apnuser"] = command["data"]
                    self.saveConfig(self.configFile)
                    return (True, "success")
                else:
                    return (False, "fail")
                
            elif(command["command"].lower() == "setapnpass()"):
                if(len(command["data"]) >= 2):
                    self.config["apnpass"] = command["data"]
                    self.saveConfig(self.configFile)
                    return (True, "success")
                else:
                    return (False, "fail")
                
            elif(command["command"].lower() == "setwifissid()"):
                if(len(command["data"]) >= 3):
                    self.config["wifissid"] = command["data"]
                    self.saveConfig(self.configFile)
                    return (True, "success")
                else:
                    return (False, "fail")
                
            elif(command["command"].lower() == "setwifipwd()"):
                if(len(command["data"]) >= 8):
                    self.config["wifipwd"] = command["data"]
                    self.saveConfig(self.configFile)
                    return (True, "success")
                else:
                    return (False, "fail")
                
            elif(command["command"].lower() == "setproject()"):
                if(len(command["data"]) >= 3):
                    self.config["project"] = command["data"]
                    self.saveConfig(self.configFile)
                    return (True, "success")
                else:
                    return (False, "fail")
                
            elif(command["command"].lower() == "setmachineid()"):
                if(len(command["data"]) == 32):
                    self.config["machineid"] = command["data"]
                    self.saveConfig(self.configFile)
                    return (True, "success")
                else:
                    return (False, "fail")
                
            elif(command["command"].lower() == "sethostname()"):
                if(len(command["data"]) >= 4):
                    self.config["hostname"] = command["data"]
                    self.saveConfig(self.configFile)
                    return (True, "success")
                else:
                    return (False, "fail")
                
            elif(command["command"].lower() == "sethostport()"):
                if(len(command["data"]) >= 4):
                    self.config["hostport"] = command["data"]
                    self.saveConfig(self.configFile)
                    return (True, "success")
                else:
                    return (False, "fail")
                
            elif(command["command"].lower() == "gettagid()"):
                print(type(command["data"]) is list)
                
                if(type(command["data"]) is not list):
                    print("something went wrong with command data")
                    return (False, "fail")
                
                response = tc.syncTags(command["data"])
                
                tdf = open(self.tagDictFile, "wb")
                tdf.write(json.dumps(response))
                tdf.close()
                
                print("gettagid")
                return (True, response)
                
            elif(command["command"].lower() == "settagdata()"):
                print("settagdata")
                self.queueMgmtaddToQueue(command["data"])
                
                #envia todas as tags da fila
                count = self.queueMgmtgetLenght()                
                response = "{}"
                
                for i in range(count):
                    #tenta fazer o envio
                    response = self.sendTagData(self.queueMgmtgetNext())
                
                return (True, response)
                
            elif(command["command"].lower() == "getsignal()"):
                print("getsignal")
                return (True, "")
                
            elif(command["command"].lower() == "getconnectionstatus()"):
                if(self.ping("1.1.1.1")):
                    return (True, "connected")
                else:
                    return (False, "disconnected")
                
            elif(command["command"].lower() == "getqueue()"):
                print(self.queueMgmtgetLenght())
                #print(self.queueMgmtgetNext())
                return (True, "{'length': "+str(self.queueMgmtgetLenght() + "}"))
                
            elif(command["command"].lower() == "cleanqueue()"):
                if(self.queueMgmtclean()):
                    print()
                    return (True, "success")
                else:
                    return (False, "fail")
            
            elif(command["command"].lower() == "getip()"):
                print("getip")
                ni.ifaddresses('eth0')
                ip = ni.ifaddresses('eth0')[ni.AF_INET][0]['addr']
                print(ip)  # should print "192.168.100.37"
                return (True, "{'ipaddress' : '" + ip + "'}")
                
            elif(command["command"].lower() == "getpinginfo()"):
                print("getpinginfo")
                return (True, "")
                
            elif(command["command"].lower() == "restart()"):
                print("restart")
                return (True, "")
            
        #faz a configuracao da apn
        def setApnConfigPwd(self, apnUsr):
            print("cionfigurando apn")
       

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
        
        #cria uma thread para manipular a fila
        def queueMgmtgetNext(self):
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
            
            return nextData
            
        def queueMgmtgetLenght(self):
            tmp = open(self.queueFile, "rb")
            loaded = json.loads(tmp.read())
            tmp.close()
            
            l = len(loaded)
            
            del loaded
            
            return l
            
            
        def queueMgmtaddToQueue(self, data):
            tmp = open(self.queueFile, "rb")
            loaded = json.loads(tmp.read())
            tmp.close()
            
            loaded.append(data)
            
            tmp = open(self.queueFile, "wb")
            tmp.write(json.dumps(loaded))
            tmp.close()
            
            del loaded
            
            return True    
            
        def queueMgmtclean(self):
            tmp = open(self.queueFile, "wb")
            tmp.write("[]")
            tmp.close()
            
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