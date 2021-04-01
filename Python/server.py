#!/usr/bin/env python
# -*- coding: utf-8 -*-
 
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
from gpiozero import LED

class StatusIndication:
    def __init__(self):
        self.networkLed = LED(2) #indicador de rede
        self.scriptLed  = LED(3) #indicador de atividade
        self.workingLed = LED(4) #indicador de ligado

    def networkStatus(self):
        while(True):
            self.networkLed.off()
            time.sleep(1)
            if(self.ping("1.1.1.1")):
                self.networkLed.on()
            time.sleep(1)
                
    def scriptStatus(self):
        while(True):
            self.scriptLed.on()
            time.sleep(1)
            self.scriptLed.off()
            time.sleep(1)
        
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

class telemetryCore:
    def __init__(self):
        print("iniciado")
        
        #registra o ultimo reinicio
        self.lastStart = datetime.now()
        
        #define o diretorio de funcionamento
        ppath = "/var/cemi/"
        #define o diretorio de funcionamento do script
        os.chdir(ppath)


        #pega diretorio home
        #        _home = expanduser("~")
        #        print("home Directory")
        #        print(_home)
        #        print("________________")
        
        #define pastas principais
        self.home = ppath+"/.config"
        self.configFile = ppath+"/.config/config"
        self.tagDictFile = ppath+"/.config/tags"
        self.queueFile = ppath+"/.config/queue"
        self.queueSocket = ppath+"/.config/queueSocket"
        self.mobileNetworkConfigFile = "/etc/wvdial.conf"
        #self.wifiNetworkConfigFile = ppath+"/.config/wpa_supplicant"
        self.wifiNetworkConfigFile = "/etc/wpa_supplicant/wpa_supplicant.conf"
        
        #finaliza socket unix
        if(os.path.exists(self.queueSocket)):
            os.remove(self.queueSocket)
        
        #verifica se existem as pastar principais
        if(not os.path.isdir(self.home)):
            os.mkdir(self.home)
            
        if(not os.path.isfile(self.configFile)):
            tmp = open(self.configFile, "wb")
            tmp.write("{\"cycle\":60}")
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
            tmp.write('''[Dialer Defaults]\n
                Init1 = ATZ\n
                Init2 = ATQ0 V1 E1 S0=0 &C1 &D2 +FCLASS=0\n
                Init3 = AT+CGDCONT=1,"IP","internet"\n
                Dial Command = ATD
                Modem Type = Analog Modem\n
                Phone = *99#\n
                ISDN = 0\n
                Username = %apnuser%\n
                Password = %apnpass%\n
                Init1 = ATZ
                Modem = /dev/ttyUSB0
                New PPPD = 1
                Carrier Check = no
                stupid mode=1
                Baud = 460800''')
            tmp.close()
            
        if(not os.path.isfile(self.wifiNetworkConfigFile)):
            tmp = open(self.wifiNetworkConfigFile, "wb")
            tmp.write(
            '''ctrl_interface=DIR=/var/run/wpa_supplicant GROUP=netdev
            country=BR # Your 2-digit country code
            ipdate_config=1
            
            network={
                ssid="%wifiuser%"
                psk="%wifipass%"
                key_mgmt=WPA-PSK
            }''')
            tmp.close()
        
        #verifica integridade dos arquivos necessarios evitando corrupcao
        try:
            with open(self.configFile, "rb") as tmp:
                json.loads(tmp.read())
                tmp.close()
                print("config integro")
                
        except Exception as e:
            print("config corrompido")
            print(e)
            tmp = open(self.configFile, "wb")
            tmp.write("{\"cycle\":60}")
            tmp.close()
            
        try:
            with open(self.tagDictFile, "rb") as tmp:
                json.loads(tmp.read())
                tmp.close()
                print("tagfile integro")
        except Exception as e:
            print("tagfile corrompido")
            print(e)
            tmp = open(self.tagDictFile, "wb")
            tmp.write("{}")
            tmp.close()
            
        try:
            with open(self.queueFile, "rb") as tmp:
                json.loads(tmp.read())
                tmp.close()
                print("queue integro")
        except Exception as e:
            print("queue file corrompido")
            print(e)
            tmp = open(self.queueFile, "wb")
            tmp.write("[]")
            tmp.close()
                    
        
        
#           define os comaandos aceitos array {"command": "fields separated by colma"}
        self.commandSet = {}
        self.commandSet["setpertoperreceiver()"] = "data"
        self.commandSet["setpertopersender()"] = "data"
        self.commandSet["setpertopertag()"] = "data"
        self.commandSet["setapnname()"] = "data"
        self.commandSet["setapnuser()"] = "data"
        self.commandSet["setapnpass()"] = "data"
        self.commandSet["setwifissid()"] = "data"
        self.commandSet["setwifipwd()"] = "data"
        self.commandSet["setip()"] = "data"
        self.commandSet["setnetmask()"] = "data"
        self.commandSet["setgateway()"] = "data"
        self.commandSet["setdns()"] = "data"
        self.commandSet["setproject()"] = "data"
        self.commandSet["setmachineid()"] = "data"
        self.commandSet["sethostname()"] = "data"
        self.commandSet["sethostport()"] = "data"
        self.commandSet["setcycle()"] = "data"
        self.commandSet["gettagid()"] = "data"
        self.commandSet["settagdata()"] = "data,date"
        self.commandSet["getsignal()"] = ""
        self.commandSet["getconnectionstatus()"] = ""
        self.commandSet["getqueue()"] = ""
        self.commandSet["cleanqueue()"] = ""
        self.commandSet["getip()"] = ""
        self.commandSet["getpinginfo()"] = ""        
        self.commandSet["identify()"] = ""
        self.commandSet["setmode()"] = "data"
        self.commandSet["restart()"] = ""
        self.commandSet["laststart()"] = ""
        self.commandSet["help()"] = ""
        
        #carrega configuracao no inicio da aplicacao
        self.loadConfig(self.configFile)
        
        #inicializa o gerente de tags
        self.tagsmgmt = tagsMgmt(self.tagDictFile)
        
        #inicializa servidor de filas
        self.queuemgmt = queueMgmt(ppath, self.queueFile, self.queueSocket)
        thread.start_new_thread(self.queuemgmt.startSocket, tuple())
        
        #verifica o modo de operacao e inicia a rede
        if("mode" not in self.config):
            self.setMobileMode()
        elif(self.config["mode"] == "wifi"):
            self.setWifiMode()            
        else:
            self.setMobileMode()
            
        #indica status do dispositivo
        statusIndication = StatusIndication()
        thread.start_new_thread(statusIndication.networkStatus, ())
        thread.start_new_thread(statusIndication.scriptStatus, ())
        
        #inicia thread de envio de dados
        thread.start_new_thread(self.sendDataThread, ())    

                    
    def loadConfig(self, configFile):
        tmp = open(self.configFile, "rb")
        self.config = json.loads(tmp.read())
        tmp.close()
        
        #campos obrigatorios
        if("deviceimei" not in self.config):
            self.config["deviceimei"] = "00000000000000"
    
    def saveConfig(self, configFile):
        tmp = open(self.configFile, "wb")
        tmp.write(json.dumps(self.config))
        tmp.close()
    
    def sendDataThread(self):
        while(True):
            try:
                print("****************************************************************************")
                print("*************************Enviando dados para nuvem**************************")
                print("****************************************************************************")
                #envia todas as tags da fila
                count = int(self.queueMgmtgetLenght())
                print("count -> ", count)
                response = "{}"
                
                for i in range(count):
                    #tenta fazer o envio
                    nextMessage = self.queueMgmtgetNext()
                    response = self.sendTagData(nextMessage)
                    
                    print("webresponse")
                    print(response)
                    print("_____________________")
                    
                    #caso de erro adiciona novamente a fila
                    if(response == None):
                        #Erro ao Enviar Realocando em fila
                        self.queueMgmtaddToQueue(nextMessage)
                                        
            except Exception as e:
                #salva buffer no disco
                self.queueMgmtSaveBuffer()
                print(e)
                
            finally:        
                #aguarda o ciclo definido para enviar os dados
                if("cycle" in self.config):
                    time.sleep(self.config["cycle"])
                else:
                    time.sleep(10)
                
    def messageValidation(self, message):
#       Predeclara Variaveis para evitar valores nulos ou flutuantes            
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
        if(command["command"].lower() == "identify()"):
            return (True, "processbisender")
            
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
            
        #define o ip de rede wifi    000.000.000.000
        elif(command["command"].lower() == "setip()"):
            if(re.search(r"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$",command["data"], re.MULTILINE)):
                self.config["ipaddress"] = command["data"]
                self.saveConfig(self.configFile)
                
                #define endereco ip e mascara de rede do dispositivo
                self.setipaddress(command["data"], "255.255.255.0" if "netmask" not in self.config else self.config["netmask"])
                    
                return (True, "success")
            else:
                return (False, "fail")
            
        #define o ip de rede wifi    000.000.000.000
        elif(command["command"].lower() == "setnetmask()"):
            if(re.search(r"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$",command["data"], re.MULTILINE)):
                self.config["netmask"] = command["data"]
                self.saveConfig(self.configFile)
                
                #define endereco ip e mascara de rede do dispositivo
                self.setipaddress("192.168.0.254" if "ipaddress" not in self.config else self.config["ipaddress"], command["data"])
                    
                return (True, "success")
            else:
                return (False, "fail")
                
        #define o gateway de rede wifi    
        elif(command["command"].lower() == "setgateway()"):
            if(re.search(r"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$",command["data"], re.MULTILINE)):
                self.config["gateway"] = command["data"]
                self.saveConfig(self.configFile)
                
                #define endereco ip e mascara de rede do dispositivo
                self.setgateway(command["data"])
                    
                return (True, "success")
            else:
                return (False, "fail")
            
        #define o dns de rede wifi    
        elif(command["command"].lower() == "setdns()"):
            if(re.search(r"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$",command["data"], re.MULTILINE)):
                self.config["dns"] = command["data"]
                self.saveConfig(self.configFile)
                
                #define endereco ip e mascara de rede do dispositivo
                self.setdns(command["data"])
                    
                return (True, "success")
            else:
                return (False, "fail")
        
        #define o nome do projeto para sincronizar com a nuvem    
        elif(command["command"].lower() == "setcycle()"):
            if(len(command["data"]) >= 3):
                self.config["cycle"] = int(command["data"])
                self.saveConfig(self.configFile)
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
            if(len(command["data"]) == 64):
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
            print(command["data"])
            print(type(command["data"]) is list)
            
            if(type(command["data"]) is not list):
                print("something went wrong with command data")
                return (False, "fail")
            
            response = tc.syncTags(command["data"])
            
            if(response == None):
                print("something went wrong with connection command data")
                return (False, "fail")
            
            #adiciona ao arquivo de tags
            self.tagsmgmt.updateTags(response)
            
            print("gettagid")
            return (True, response)
        
        #envia dados de tag para a nuvem    
        elif(command["command"].lower() == "settagdata()"):
            print("settagdata")    
            self.config["ready"] = True
            self.queueMgmtaddToQueue(command)
            
#            #envia todas as tags da fila
            count = int(self.queueMgmtgetLenght())
            print("count -> ", count)
#            response = "{}"
#            
#            for i in range(count):
#                #tenta fazer o envio
#                nextMessage = self.queueMgmtgetNext()
#                response = self.sendTagData(nextMessage)
#                #caso de erro adiciona novamente a fila
#                if(response == None):
#                    #Erro ao Enviar Realocando em fila
#                    self.queueMgmtaddToQueue(nextMessage)
#            
            return (True, "success")
        
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
            return (True, int(self.queueMgmtgetLenght()))
        
        #limpa fila de envio    
        elif(command["command"].lower() == "cleanqueue()"):
            if(self.queueMgmtclean()):
                print()
                return (True, "success")
            else:
                return (False, "fail")
        
        #verifica o ip do dispositivo
        elif(command["command"].lower() == "getip()"):
            try:
                print("getip")
                ni.ifaddresses('wlan0')
                ip = ni.ifaddresses('wlan0')[ni.AF_INET][0]['addr']
                print(ip)  # should print "192.168.100.37"
                return (True, "{'ipaddress' : '" + ip + "'}")
            except:
                print("getip")
                ni.ifaddresses('ppp0')
                ip = ni.ifaddresses('ppp0')[ni.AF_INET][0]['addr']
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
        elif(command["command"].lower() == "setmode()"):
            try:
                if(command["data"] == "wifi"):
                    self.setWifiMode()
                    self.config["mode"] = command["data"]
                    self.saveConfig(self.configFile)
                    return (True, "success")
                elif(command["data"] == "mobile"):
                    self.config["mode"] = command["data"]
                    self.setMobileMode()
                    self.saveConfig(self.configFile)
                    return (True, "success")
            except Exception as e:    
                return (False, "error")
        
        elif(command["command"].lower() == "laststart()"):
            return(True, str(self.lastStart))
        
        elif(command["command"].lower() == "help()"):
            return(True, json.dumps(self.commandSet))
        
        #reinicia servico na maquina    
        elif(command["command"].lower() == "restart()"):
            print("restart")
            #Sai do script para o launcher reexecutar
            exit()
            return (True, "")
        
    def setipaddress(self, ip, netmask):
        #ifconfig eth0 192.168.1.5 netmask 255.255.255.0 up
        command = ["ifconfig","wlan0",ip,"netmask",netmask,"up"]
        
        if(ip == "0.0.0.0"):
                command = ['dhclient','wlan0','-v']
                
        return subprocess.call(command) == 0
        
    def setgateway(self, gateway):
        #route add default gw 192.168.1.1
        command = ['route','add','default','gw',gateway]
        return subprocess.call(command) == 0
        
    def setdns(self, dns):
        #echo "nameserver 1.1.1.1" > /etc/resolv.conf
        command = ['echo','"nameserver '+dns+'"','>','/etc/resolv.conf']
        
        return subprocess.call(command) == 0
        
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
            
            #reinicia conexao wifi
            os.system("ifconfig wlan0 down")
            time.sleep(2)
            os.system("ifconfig wlan0 up")


    def ping(self, host):
        """
        Returns True if host (str) responds to a ping request.
        Remember that a host may not respond to a ping (ICMP) request even if the host name is valid.
        """

        # Option for the number of packets as a function of
        param = '-n' if platform.system().lower()=='windows' else '-c'

        # Building the command. Ex: "ping -c 1 google.com"
        FNULL = open(os.devnull, 'w')
        command = ['ping', param, '1', host]

        return subprocess.call(command, stdout=FNULL, stderr=subprocess.STDOUT) == 0
    
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
        
        return json.loads(recvData)

        
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
    
    def queueMgmtSaveBuffer(self):
        tcp = socket.socket(socket.AF_UNIX, socket.SOCK_STREAM)
        tcp.connect(self.queueSocket)
        tcp.send('{"command": "savebuffer"}')
        recvData = tcp.recv(1024)
        tcp.close()    
        
        return True        
    
    def syncTags(self, tags):
        try:
            #post em /setuptags
            #dados enviados como {'data': ['{tag name}:{tag address}:{tag type}','{tag name}:{tag address}:{tag type}']}
            #dados recebidos como {'data': {'tag':'id','tag','id'}}
            
            print("sync Tags")
            
            if (("hostname" not in self.config) and 
                    ("hostport" not in self.config)):
                print("Configuracao incompleta")
                return {}
            
            body = {'data': tags}
            
            req = requests.post("http://"+self.config["hostname"]+":"+self.config["hostport"]+"/setuptags", None, body)
            
            if(req.ok):
                resp = json.loads(req.text)
                return resp
            else:
                return None
        except:
            print("falha ao enviar registro")
            return None
        
    def sendTagData(self, data):
        try:
            #post em /setuptags
            #dados para processar
            #project machineid date devicenumber deviceimei data(na requisicao)
            if(("project" not in self.config) or 
                ("machineid" not in self.config) or 
#                    ("deviceimei" not in self.config) or
                        ("hostname" not in self.config) or 
                            ("hostport" not in self.config)):
                
                print("configuracao incompleta")
                return None
            
            #if(("data" not in data) and 
            #    ("date" not in data)):
            #    print("requisicao incompleta")
            #    return {}
            
            #separa data e hora para formatar os dados
            dt = str(data["date"]).split(" ")[0]
            hr = str(data["date"]).split(" ")[1]
            
            
            print("sync Tags")
            body = {'project': self.config["project"], 'machineid': self.config["machineid"], 'date': dt, 'deviceimei': self.config["deviceimei"], 'devicenumber': 0, 'data': {hr: data["data"]}}
            
            req = requests.post("http://"+self.config["hostname"]+":"+self.config["hostport"]+"/tagdata", None, body)
            
            if(req.ok):
                print(req.text)                
                resp = json.loads(req.text.replace("\'", "\""))
                return resp
            else:
                return None
        except:
            print("falha ao enviar dados ao servidor")       
            return None
    
    ##executa em modo p2p para envio e recebimento de dados
    def sendPerTagData(self, data):
        try:
            #post em /setuptags
            #dados para processar
            #project machineid date devicenumber deviceimei data(na requisicao)
            if(("pertoperreceiver" not in self.config) or 
                ("pertopersender" not in self.config) or
                        ("hostname" not in self.config) or 
                            ("hostport" not in self.config) or
                                (data is not None)):
                
                print("configuracao incompleta")
                return None
            
            body = {'tagdata': data}
            
            req = requests.post("http://"+self.config["hostname"]+":"+self.config["hostport"]+"/pertoper/"+self.config["pertopersender"]+"/"+self.config["pertoperreceiver"], None, body)
            
            if(req.ok):
                print(req.text)                
                resp = json.loads(req.text.replace("\'", "\""))
                return resp
            else:
                return None
        except:
            print("falha ao enviar dados ao servidor")       
            return None
        
    def receivePerTagData(self):
        try:
            #post em /setuptags
            #dados para processar
            #project machineid date devicenumber deviceimei data(na requisicao)
            if(("pertoperreceiver" not in self.config) or 
                ("pertopersender" not in self.config) or
                        ("hostname" not in self.config) or 
                            ("hostport" not in self.config)):
                
                print("configuracao incompleta")
                return None
            
            print("sync Tags")
            #body = body = {'senderid': self.config["pertopersender"], 'receiverid': self.config["pertoperreceiver"]}
            
            req = requests.get("http://"+self.config["hostname"]+":"+self.config["hostport"]+"/pertoper/"+self.config["pertopersender"]+"/"+self.config["pertoperreceiver"], None, None)
            
            if(req.ok):
                print(req.text)                
                resp = json.loads(req.text.replace("\'", "\""))
                return resp
            else:
                return None
        except:
            print("falha ao enviar dados ao servidor")       
            return None
        
        #connection.request("POST", '/setuptags', body=json.dumps(body))
    def setWifiMode(self):
        os.system("killall wvdial")
        os.system("ifconfig wlan0 up")
        
    def setMobileMode(self):    
        os.system("ifconfig wlan0 down")
        os.system("wvdial")
        
        
class queueMgmt:
    def __init__(self, home, queueFile, queueSocket):         
                
        #define pastas principais
        self.home = home
        
        self.queueFile = queueFile
        self.queueSocket = queueSocket
        self.lock = False
        self.toWrite = ""
        self.lastWrite = None   
        self.socket = None
        self.next = {}
        self.buffer = []
        
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
                elif(loadedMsg["command"] == u"savebuffer"):
                    con.sendall(str(self.queueMgmtSaveBuffer()))
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
        try:
                                
            if(len(self.buffer) > 0):
                nextData = self.buffer.pop(0)
                
            elif(self.lock == True):
                print(">>>>>>>>>>>>>>>>>>>data is locked")
                return json.dumps({})
                
                
            elif(len(self.buffer) == 0):
                #bloqueia o arquivo para escrita e leitura
                self.lock = True
                
                tmp = open(self.queueFile, "rb")
                self.buffer = json.loads(tmp.read())
                tmp.close()
                
                #remove dados ja carregados em buffer (pode ser prejudicial por perder dados no caminho - verificar opcoes)
                tmp = open(self.queueFile, "wb")
                tmp.write("[]")
                #self.buffer = json.loads(tmp.read())
                tmp.close()
            
                #desbloqueia o arquivo para escrita e leitura
                self.lock = False
                                
                if(len(self.buffer) > 0):
                    nextData = self.buffer.pop(0)
                
                else:
                    nextData = {}
                
            else:
                nextData = {}
                
            '''
            tmp = open(self.queueFile, "wb")
            tmp.write(json.dumps(loaded))
            tmp.close()
            '''
            
            return json.dumps(nextData)
        except Exception as e:
            print(">>>>>>>>>>>>>>>>>>>falha ao add dado a fila")
            print(e)
            self.lock = False
            return json.dumps("")
        
    #recupera tamanho da fila
    def queueMgmtgetLenght(self):
        try:
            #inicializa l em 0
            l = "0"
            
            if(len(self.buffer) > 0):
                l = len(self.buffer)
            
            elif(self.lock == True):
                print(">>>>>>>>>>>>>>>>>>>data is locked")
            
            else:
                #bloqueia o arquivo para escrita e leitura
                self.lock = True
                
                tmp = open(self.queueFile, "rb")
                loaded = json.loads(tmp.read())
                tmp.close()
            
                #desbloqueia o arquivo para escrita e leitura
                self.lock = False
            
                l = len(loaded)
                
                del loaded
            
            return str(l)
        
        except Exception as e:
            print(">>>>>>>>>>>>>>>>>>>falha ao add dado a fila")
            print(e)
            self.lock = False
            return "0"
        
        
    #adiciona os dados ao aquivo de fila
    def queueMgmtaddToQueue(self, data):
        try:
            if(self.lock == True):
                print(">>>>>>>>>>>>>>>>>>>data is locked")
                return False
                
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
        except Exception as e:
            print(">>>>>>>>>>>>>>>>>>>falha ao add dado a fila")
            print(e)
            self.lock = False
            return True    
        
    #limpa aquivo de fila
    def queueMgmtclean(self):
        if(self.lock == True):
            print(">>>>>>>>>>>>>>>>>>>data is locked")
            return False
        
        #bloqueia o arquivo para escrita e leitura
        self.lock = True
            
        tmp = open(self.queueFile, "wb")
        tmp.write("[]")
        tmp.close()
            
        #desbloqueia o arquivo para escrita e leitura
        self.lock = False
        
        return True   
        
    #salva buffer de fila
    def queueMgmtSaveBuffer(self):
        
        try:
            if(self.lock == True):
                print(">>>>>>>>>>>>>>>>>>>data is locked")
                return False
                
            #bloqueia o arquivo para escrita e leitura
            self.lock = True
            
            tmp = open(self.queueFile, "rb")
            loaded = json.loads(tmp.read())
            tmp.close()
            
            
            for i in range(len(self.buffer)):
                loaded.append(self.buffer[i])
            
            tmp = open(self.queueFile, "wb")
            tmp.write(json.dumps(loaded))
            tmp.close()
            
            del loaded
            self.buffer = []
            
            #desbloqueia o arquivo para escrita e leitura
            self.lock = False
            
            return True    
        
        except Exception as e:
            print(">>>>>>>>>>>>>>>>>>>falha ao add dado a fila")
            print(e)
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

            
#inicializa telemetry core
tc = telemetryCore()
version = "1.0.10"

ser = serial.Serial('/dev/serial0', 115200)  # open serial port
#ser.set_buffer_size(rx_size = 12800, tx_size = 12800)
#print(ser.name)         # check which port was really used
ser.write('Iniciando\r\n'+str(datetime.now())+'\r\n')     # write a string
ser.write(b'OptProcessBI Sender\r\n')     # write a string
ser.write(b'Version = '+str(version) + "\r\n")     # write a string
ser.write(b"\r\n")     # write a string
#ser.close()             # close port
time.sleep(1)

buffer = ""

while(True):
    try:
        if(not ser.isOpen()):
            ser = serial.Serial('/dev/serial0', 115200)  # open serial port
            
#       while("\n" not in buffer):
#           buffer += ser.read()
        
#       jsonData = tc.messageValidation(buffer)
        message = ser.readline()
        jsonData = tc.messageValidation(message)
        message = ""
        
        print("**********************Mensagem recebida********************************")
#       buffer = ""
        
        if(jsonData == None):
            continue
        print(jsonData)    
        
        #captura resposta
        response = tc.processCommand(jsonData)
        
        #jsonData = tc.messageValidation("|79|{\"command\":\"settagdata()\", \"data\":\"settagdata()\", \"date\":\"2021-01-29 16:54:55\"}")

        ser.write(tc.sendResponse(json.dumps({'date' : str(datetime.now()), 'status': response[0], 'version': version, 'desc': response[1]})) + "\r\n")
    except Exception as e:
        print("******************Erro ao receber Mensagem***************************")
        print(e)
        print("*********************************************************************")
        if(ser.isOpen()):
            ser.close()
        
