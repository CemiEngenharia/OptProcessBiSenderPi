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
import socket, struct
import netifaces as ni
import thread
import time
from gpiozero import LED
import base64
import zlib

#mongo connection
from pymongo import MongoClient
from pymongo import operations as mongoOperations
from bson import ObjectId

class StatusIndication:
    def __init__(self):
        self.networkLed = LED(2) #indicador de rede
        self.scriptLed  = LED(3) #indicador de atividade
        self.workingLed = LED(4) #indicador de ligado

    def get_default_gateway_linux(self):
        """Read the default gateway directly from /proc."""
        with open("/proc/net/route") as fh:
            for line in fh:
                fields = line.strip().split()
                if fields[1] != '00000000' or not int(fields[3], 16) & 2:
                    # If not default route or not RTF_GATEWAY, skip it
                    continue

                return socket.inet_ntoa(struct.pack("<L", int(fields[2], 16)))
            
    def networkStatus(self):
        tryes = 0
        
        self.networkLed.on()
        time.sleep(20)
        
        while(True):
            try:
                print("_______ ping___________")
                self.networkLed.off()
                time.sleep(1)
                if(self.ping("1.1.1.1")):
                    print("pingando success")
                    self.networkLed.on()
                    tryes = 0
                else:
                    gateway = str(self.get_default_gateway_linux())
                    if(gateway == "None"):
                        gateway = "1.1.1.1"
                        
                    print("pingando gateway -> " + gateway)
                    if(not self.ping(gateway)):
                        tryes += 1
                        if(tryes >= 10):
                            print("reiniciando wpa_cli")
                            #reconfigura wpa e espera 5 seg
                            os.system("wpa_cli -i wlan0 disconnect")
                            os.system("wpa_cli -i wlan0 reconfigure")
                            os.system("wpa_cli -i wlan0 sta_autoconnect 1")
                            os.system("wpa_cli -i wlan0 reassociate")
                            time.sleep(6)
                            
                        elif(tryes >= 20):
                            print("reiniciando dhcpcd")
                            os.system("sudo services dhcpcd restart")
                            time.sleep(6)
                            
                        elif(tryes >= 40):
                            print("reiniciando dispositivo")
                            os.system("sudo reboot now")
                            time.sleep(6)
                            
                time.sleep(1)
            except Exception as e:
                print("___________________________________led Status Exception -> "+str(e))
                
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


        FNULL = open(os.devnull, 'w')

        # Building the command. Ex: "ping -c 1 google.com"
        command = ['ping', param, '1', host]

        return subprocess.call(command, stdout=FNULL, stderr=subprocess.STDOUT) == 0

class telemetryCore:
    def __init__(self):             
        #print("iniciado")
        
        #define variavel de token para o device
        self.deviceToken = ""
        
        #registra o ultimo reinicio
        self.lastStart = datetime.now()
        
        #define o diretorio de funcionamento
        ppath = "/var/cemi/"
        #define o diretorio de funcionamento do script
        os.chdir(ppath)

        self.dhcpsample = '''# A sample configuration for dhcpcd.
# See dhcpcd.conf(5) for details.

# Allow users of this group to interact with dhcpcd via the control socket.
#controlgroup wheel

# Inform the DHCP server of our hostname for DDNS.
hostname

# Use the hardware address of the interface for the Client ID.
clientid
# or
# Use the same DUID + IAID as set in DHCPv6 for DHCPv4 ClientID as per RFC4361.
# Some non-RFC compliant DHCP servers do not reply with this set.
# In this case, comment out duid and enable clientid above.
#duid

# Persist interface configuration when dhcpcd exits.
persistent

# Rapid commit support.
# Safe to enable by default because it requires the equivalent option set
# on the server to actually work.
option rapid_commit

# A list of options to request from the DHCP server.
option domain_name_servers, domain_name, domain_search, host_name
option classless_static_routes
# Respect the network MTU. This is applied to DHCP routes.
option interface_mtu

# Most distributions have NTP support.
#option ntp_servers

# A ServerID is required by RFC2131.
require dhcp_server_identifier

# Generate SLAAC address using the Hardware Address of the interface
#slaac hwaddr
# OR generate Stable Private IPv6 Addresses based from the DUID
slaac private

# Example static IP configuration:
#interface eth0
#static ip_address=192.168.0.10/24
#static ip6_address=fd51:42f8:caae:d92e::ff/64
#static routers=192.168.0.1
#static domain_name_servers=192.168.0.1 8.8.8.8 fd51:42f8:caae:d92e::1

# It is possible to fall back to a static IP if DHCP fails:
# define static profile
#profile static_eth0
#static ip_address=192.168.1.23/24
#static routers=192.168.1.1
#static domain_name_servers=192.168.1.1
\n'''

        self.dhcpConfig = '''
# fallback to static profile on eth0
#interface eth0
#fallback static_eth0
static ip_address=%ip%/24
static routers=%gateway"
static domain_name_servers=%dns%
'''

        #pega diretorio home
        #        _home = expanduser("~")
        #        #print("home Directory")
        #        #print(_home)
        #        #print("________________")
        
        #define pastas principais
        self.home = ppath+"/.config"
        self.configFile = ppath+"/.config/config"
        self.tagDictFile = ppath+"/.config/tags"
        self.queueFile = ppath+"/.config/queue"
        self.queueSocket = ppath+"/.config/queueSocket"
        self.ipConfigFile = "/etc/dhcpcd.conf"
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
            
        self.dnsTryes = 10
        
        #verifica integridade dos arquivos necessarios evitando corrupcao
        try:
            with open(self.configFile, "rb") as tmp:
                json.loads(tmp.read())
                tmp.close()
                #print("config integro")
                
        except Exception as e:
            print("config corrompido")
            #print(e)
            tmp = open(self.configFile, "wb")
            tmp.write("{\"cycle\":60}")
            tmp.close()
            
        try:
            with open(self.tagDictFile, "rb") as tmp:
                json.loads(tmp.read())
                tmp.close()
                #print("tagfile integro")
        except Exception as e:
            print("tagfile corrompido")
            #print(e)
            tmp = open(self.tagDictFile, "wb")
            tmp.write("{}")
            tmp.close()
            
        try:
            with open(self.queueFile, "rb") as tmp:
                json.loads(tmp.read())
                tmp.close()
                #print("queue integro")
        except Exception as e:
            print("queue file corrompido")
            #print(e)
            tmp = open(self.queueFile, "wb")
            tmp.write("[]")
            tmp.close()
                    
        
        
#           define os comaandos aceitos array {"command": "fields separated by colma"}
        self.commandSet = {}
        self.commandSet["setpertopertag()"] = "data"
        self.commandSet["getpertopertag()"] = ""
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
        self.commandSet["sendfile()"] = "header,data"
        self.commandSet["getconnectionstatus()"] = ""
        self.commandSet["getqueue()"] = ""
        self.commandSet["cleanqueue()"] = ""
        self.commandSet["getip()"] = ""
        self.commandSet["getpinginfo()"] = ""        
        self.commandSet["identify()"] = ""
        self.commandSet["setmode()"] = "data"
        self.commandSet["setreceiverid()"] = "data"
        self.commandSet["setsenderid()"] = "data"
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
                #print("count -> ", count)
                response = "{}"
                
                for i in range(count):
                    #tenta fazer o envio
                    nextMessage = self.queueMgmtgetNext()
                    response = self.sendTagData(nextMessage)
                    
                    print("webresponse")
                    print(response)
                    #print("_____________________")
                    
                    #caso de erro adiciona novamente a fila
                    if(response == None):
                        #Erro ao Enviar Realocando em fila
                        #valida mensagem pra por na fila novamente caso esteja vazia
                        if("date" in nextMessage):
                            print("readicionando a fila")
                            self.queueMgmtaddToQueue(nextMessage)
                        else:
                            print(nextMessage)
                            print("nenhuma data encontrada")
                                        
            except Exception as e:
                for j in range(8):
                    try:
                        #salva buffer no disco
                        self.queueMgmtSaveBuffer()
                        print(e)
                        break
                    except Exception as e:
                        print("falha ao salvar dados novamente tentando novamente")
                        time.sleep(100)
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
            print(message)
            return None
        
#           recupera tamanho definido da mensagem
        try:
            length = int(message[message.index("|")+1:message.rindex("|")])
            print("length")
            print(length)
        except:
            print("DataLength is Not a Integer")
            return None
        
#           Recupera dados passados na mensagem
        try:
            data = message[message.rindex("|")+1:len(message)]
            #print(data)
            #print(len(data))
        except:
            print("data does not exists")
            return None
        
#           Verifica tamanho dos dados passados na mensagem
        if(len(data) != length):
            #print("data Length Missmatch")
            return
        
#           Verifica se os dados sao um json valido            
        try:
            jsonData = json.loads(data)
            #print(jsonData)
        except:
            print("Data is not a json data")
            return None
        
#           Verifica campos necessarios ao json
        if("command" not in jsonData.keys()):
            #print("there is not a command on data")
            return None
        
#           Verifica se comando existe
        if(jsonData["command"] not in self.commandSet):
            #print("invalid Command")
            return None
        
#           Verifica se tem os campos necessarios para o comando
        if(len(self.commandSet[jsonData["command"]]) >= 1):
            for field in self.commandSet[jsonData["command"]].split(","):
                if(field not in jsonData):
                    #print("the data provided information is not enought -> " + field + " missmath")
                    return
                
                #print(field)
                
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
        
        #print(resp)
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
                #self.setipaddress(command["data"], "255.255.255.0" if "netmask" not in self.config else self.config["netmask"])
                #self.setipaddress(command["data"], "" if "netmask" not in self.config else self.config["netmask"])
                    
                return self.setipaddress(command["data"], "" if "netmask" not in self.config else self.config["netmask"])
            else:
                return (False, "fail")
            
        #define o ip de rede wifi    000.000.000.000
        elif(command["command"].lower() == "setnetmask()"):
            if(re.search(r"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$",command["data"], re.MULTILINE)):
                self.config["netmask"] = command["data"]
                self.saveConfig(self.configFile)
                
                #define endereco ip e mascara de rede do dispositivo
                #self.setipaddress("0.0.0.0" if "ipaddress" not in self.config else self.config["ipaddress"], command["data"])
                #self.setipaddress("" if "ipaddress" not in self.config else self.config["ipaddress"], command["data"])
                    
                return self.setipaddress("" if "ipaddress" not in self.config else self.config["ipaddress"], command["data"])
            else:
                return (False, "fail")
                
        #define o gateway de rede wifi    
        elif(command["command"].lower() == "setgateway()"):
            if(re.search(r"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}$",command["data"], re.MULTILINE)):
                self.config["gateway"] = command["data"]
                self.saveConfig(self.configFile)
                
                #define endereco ip e mascara de rede do dispositivo
                #self.setgateway(command["data"])
                    
                return self.setgateway(command["data"])
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
        
        #define porta de sincronizacao de rede    
        elif(command["command"].lower() == "setreceiverid()"):
            if(len(command["data"]) >= 4):
                self.config["receiverid"] = command["data"]
                self.saveConfig(self.configFile)
                return (True, "success")
            else:
                return (False, "fail")
        
        #define porta de sincronizacao de rede    
        elif(command["command"].lower() == "setsenderid()"):
            if(len(command["data"]) >= 4):
                self.config["senderid"] = command["data"]
                self.saveConfig(self.configFile)
                return (True, "success")
            else:
                return (False, "fail")
        
        #reinicia servico na maquina    
        elif(command["command"].lower() == "sendfile()"):
            
            if(isinstance(command["header"], "unicode")):
                if(isinstance(command["data"], "unicode")):                
                    #print("header and data found")
                    return self.receiveFile(command["header"], command["data"])
                
                return (False, "fail data isnot string")                        
            return (False, str(type(command["header"])) + " -> " + str(type(command["data"])))
        
        
        #define todos ids das tags para sincronizar    
        elif(command["command"].lower() == "gettagid()"):
            #print(command["data"])
            #print(type(command["data"]) is list)
            
            if(type(command["data"]) is not list):
                #print("something went wrong with command data")
                return (False, "fail")
            
            response = tc.syncTags(command["data"])
            
            if(response == None):
                #print("something went wrong with connection command data")
                return (False, "fail")
            
            #adiciona ao arquivo de tags
            self.tagsmgmt.updateTags(response)
            
            #print("gettagid")
            return (True, response)
        
        #envia dados de tag para a nuvem    
        elif(command["command"].lower() == "settagdata()"):
            try:
                #print("settagdata")    
                self.config["ready"] = True
                self.queueMgmtaddToQueue(command)
                
    #            #envia todas as tags da fila
                count = int(self.queueMgmtgetLenght())
                
                print("adicionado a fila, count -> ", count)
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
            
            except Exception as e:
                print(e)
                return (False, "settagdate error -> " + str(e))
                
        
        #envia dados de tag para a nuvem    
        elif(command["command"].lower() == "setpertopertag()"):
            #print("setp2ptagdata")    
            self.config["ready"] = True
            
            self.sendPerTagData(command["data"])
            
            return (True, "success")
        
        #envia dados de tag para a nuvem    
        elif(command["command"].lower() == "getpertopertag()"):
            #print("setp2ptagdata")    
            self.config["ready"] = True
            
            data = self.receivePerTagData()
            
            return (True, data)
        
        #faz leitura do sinal de rede    
        elif(command["command"].lower() == "getsignal()"):
            #print("getsignal")
            return (True, "")
        
        #verifica status da conexao de rede atravez de ping a um dns    
        elif(command["command"].lower() == "getconnectionstatus()"):
            if(self.ping("1.1.1.1")):
                return (True, "connected")
            else:
                return (False, "disconnected")
        
        #recupera numero de arquivos na fila
        elif(command["command"].lower() == "getqueue()"):
            #print(self.queueMgmtgetLenght())
            ##print(self.queueMgmtgetNext())
            return (True, int(self.queueMgmtgetLenght()))
        
        #limpa fila de envio    
        elif(command["command"].lower() == "cleanqueue()"):
            if(self.queueMgmtclean()):
                #print()
                return (True, "success")
            else:
                return (False, "fail")
        
        #verifica o ip do dispositivo
        elif(command["command"].lower() == "getip()"):
            try:
                #print("getip")
                ni.ifaddresses('wlan0')
                ip = ni.ifaddresses('wlan0')[ni.AF_INET][0]['addr']
                #print(ip)  # should #print "192.168.100.37"
                return (True, "{'ipaddress' : '" + ip + "'}")
            except:
                print("getip")
                ni.ifaddresses('ppp0')
                ip = ni.ifaddresses('ppp0')[ni.AF_INET][0]['addr']
                #print(ip)  # should #print "192.168.100.37"
                return (True, "{'ipaddress' : '" + ip + "'}")
                
        #faz analize de rete atravez de ping    
        elif(command["command"].lower() == "getpinginfo()"):
            #print("getpinginfo")
            
            pingdnstime = str(self.pingSpeed("1.1.1.1"))
            pinghosttime = str(self.pingSpeed(self.config["hostname"]))
            
            #print( (True, [pingdnstime, pinghosttime]))
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
            #print("restart")
            #Sai do script para o launcher reexecutar
            exit()
            return (True, "")
        
        
    def receiveFile(self, header, data):        
        if(len(header.split(",")) == 3):
            try:
                #decodifica body
                base64_decoded = base64.b64decode(data)
                
                #separa campos do header
                headerData = header.split(",")
                
                #verifica se a pasta existe
                if(not os.path.exists("/var/cemi")):
                    os.mkdir("/var/cemi")
                
                if(not os.path.exists("/var/cemi/.tempFiles")):
                    os.mkdir("/var/cemi/.tempFiles")
                    
                #abre arquivo pra escrita
                fle = open("/var/cemi/.tempFiles/"+headerData[0], "wb")
                fle.write(base64_decoded)
                fle.close()
                
                return (True, "success")
            
            except Exception as ex:
                print("erro ao decodificar -> "+str(ex))
                return (False, str(ex))
                
        return (False,  "header malformed")
                
    def setipaddress(self, ip, netmask):
        #ifconfig eth0 192.168.1.5 netmask 255.255.255.0 up
        
        config = "interface wlan0"
            
        try:
            if(("ipaddress" in self.config) or (len(ip) <= 6)):
                #define ip no arquivo
                if (len(ip) <= 6):
                    if(ip == "0.0.0.0"):
                        config = "#interface wlan0\n"
                        config += "#static ip_address="+ip
                    else:
                        config = "interface wlan0\n"
                        config += "static ip_address="+ip
                else:
                    if(self.config["ipaddress"] == "0.0.0.0"):
                        config = "#interface wlan0\n"
                        config += "#static ip_address="+self.config["ipaddress"]
                    else:
                        config = "interface wlan0\n"
                        config += "static ip_address="+self.config["ipaddress"]
                    
                #calcula mascara de rede
                mascara = -1
                mask = ""
                
                #recupera mascara de rede
                if(("netmask" in self.config) or (len(netmask) <= 6)):
                    if(len(netmask) <= 6):
                        mask = netmask
                    else:
                        mask = self.config["netmask"]
                
                #divide a mascara em inteiros e converte para string binaria    
                parts = mask.split(".")
                
                mask = ""
                
                if(len(parts) == 4):
                    for part in parts:
                        mask += "{0:b}".format(int(part))

                    #conta numero de  1 pra virar decimal
                    mascara = mask.count("1")
                
                #adiciona mascara de rede
                if(mascara >= 0):
                    config+= "/"+str(mascara)
                    
                #insere gateway caso esteja configurado sendo ele zerado deixa automatico
                if("gateway" in self.config):
                    if(self.config["gateway"] == "0.0.0.0"):
                        config += "\n#static routers="+self.config["gateway"]+"\n"
                    else:
                        config += "\nstatic routers="+self.config["gateway"]+"\n"
                else:
                    config+= "\n#static static routers=gateway\n"            
                    
                #insere dns caso esteja configurado sendo ele zerado deixa automatico
                if("dns" in self.config):
                    if(self.config["dns"] == "0.0.0.0"):
                        config += "#static domain_name_servers="+self.config["dns"]+"\n"
                    else:
                        config += "static domain_name_servers="+self.config["dns"]+"\n"
                else:
                    config+= "#static domain_name_servers=dns\n"
            
                    
            else:
                config = "#interface wlan0\n"
                config += "#static ip_address=ipaddress"
                config+= "\n#static static routers=gateway\n" 
                config+= "#static domain_name_servers=dns\n"
            
            open(self.ipConfigFile, "w").write(self.dhcpsample + config)
            
            #os.system("wpa_cli -i wlan0 reconfigure")
            subprocess.call("systemctl daemon-reload")
            subprocess.call("systemctl restart dhcpcd")
            subprocess.call("service dhcpcd restart")
            #os.system("ifconfig wlan0 down")
            #time.sleep(5000)
            #
            # os.system("setwifi")
            #reinicia wifi
            #subprocess.call(["service","dhcpcd", "restart"])
            
            return (True, "sucess to set ip config")
        
        except Exception as e:
            return (False, self.ipConfigFile + " -> " + str(e))
        
        #metodo antigo
        '''           
        command = ["ifconfig","wlan0",ip,"netmask",netmask,"up"]
        
        if(ip == "0.0.0.0"):
                command = ['dhclient','wlan0','-v']
                
        #return subprocess.call(command) == 0
        subprocess.Popen(command)
        
        command = ["service", "dhcpcd","restart"]
        
        return subprocess.Popen(command)
        '''
        
    def setgateway(self, gateway):
        
        config = ""
            
        try:
            if("ipaddress" in self.config):
                if(self.config["ipaddress"] == "0.0.0.0"):
                    config = "#interface wlan0\n"
                    config += "#static ip_address="+self.config["ipaddress"]
                else:
                    
                    config = "interface wlan0\n"
                    config += "static ip_address="+self.config["ipaddress"]
                    
                #calcula mascara de rede
                mascara = -1
                mask = ""
                
                #recupera mascara de rede
                if("netmask" in self.config):
                        mask = self.config["netmask"]
                
                #divide a mascara em inteiros e converte para string binaria    
                parts = mask.split(".")
                mask = ""
                
                if(len(parts) == 4):
                    for part in parts:
                        mask += "{0:b}".format(int(part))


                    #conta numero de  1 pra virar decimal
                    mascara = mask.count("1")
                
                #adiciona mascara de rede
                if(mascara >= 0):
                    config+= "/"+str(mascara)
            
                if(("gateway" in self.config) or (len(gateway) >= 7)):
                    if(gateway == "0.0.0.0"):
                        config+= "\n#static static routers=gateway\n"    
                        
                    elif(len(gateway) >= 7):
                        config += "\nstatic routers="+gateway+"\n"
                    
                    elif("gateway" in self.config):
                        config += "\nstatic routers="+self.config["gateway"]+"\n"
                        
                else:
                    config+= "\n#static static routers=gateway\n"            
                    
                if("dns" in self.config):
                    if(self.config["dns"] == "0.0.0.0"):
                        config += "#static domain_name_servers="+self.config["dns"]+"\n"
                    else:
                        config += "static domain_name_servers="+self.config["dns"]+"\n"
                else:
                    config+= "#static domain_name_servers=dns\n"
                    
            else:
                config = "#interface wlan0\n"
                config += "#static ip_address=ipaddress"
                config+= "\n#static static routers=gateway\n" 
                config+= "#static domain_name_servers=dns\n"
                        
            open(self.ipConfigFile, "w").write(self.dhcpsample + config)
            
            #os.system("wpa_cli -i wlan0 reconfigure")
            subprocess.call("systemctl daemon-reload")
            subprocess.call("systemctl restart dhcpcd")
            subprocess.call("service dhcpcd restart")
            
            #reinicia wifi
            #subprocess.call(["service","dhcpcd", "restart"])
            
            return (True, "sucess to set gateway config")
        
        except Exception as e:
            return (False, self.ipConfigFile + " -> " + str(e))
        
        #metodo antigo
        '''  
        #route add default gw 192.168.1.1
        open("/var/cemi/.config/network", "w").write("-net 0.0.0.0 gw "+gateway)
        #command = ['route','add','default','gw',gateway]
        command = ["setwifi"]
        
        #return subprocess.call(command) == 0
        return subprocess.Popen(command)
        #return true;
        '''
        
    def setdns(self, dns):
        #echo "nameserver 1.1.1.1" > /etc/resolv.conf
        command = ['echo','"nameserver '+dns+'"','>','/etc/resolv.conf']
        
        return subprocess.call(command) == 0
        
    #faz a configuracao da apn
    def setApnConfig(self, apnName=None, apnUser=None, apnPass=None):        
        #print("configurando apn")
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
        try:      
            #print("configurando wifi")
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
                #os.system("service dhcpcd restart")
                #os.system("wpa_cli -i wlan0 reconfigure")
                subprocess.call("systemctl daemon-reload")
                subprocess.call("systemctl restart dhcpcd")
                subprocess.call("service dhcpcd restart")
            
            return True
        
        except Exception as e:
            return False

    def ping(self, host):
        """
        Returns True if host (str) responds to a ping request.
        Remember that a host may not respond to a ping (ICMP) request even if the host name is valid.
        """

        # Option for the number of packets as a function of
        param = '-n' if platform.system().lower()=='windows' else '-c'

        # Building the command. Ex: "ping -c 1 google.com"
        FNULL = open(os.devnull, 'w')
        command = ['ping', param, '1',"-i","0.2", host]

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
        tcp.send('{"command": "next"}\n\n\n')
        recvData = tcp.recv(1024000)
        tcp.close()
        
        return json.loads(recvData)

        
    def queueMgmtgetLenght(self):
        tcp = socket.socket(socket.AF_UNIX, socket.SOCK_STREAM)
        tcp.connect(self.queueSocket)
        tcp.send('{"command": "length"}\n\n\n')
        recvData = tcp.recv(1024)
        tcp.close()
        
        return recvData        
        
        
    def queueMgmtaddToQueue(self, data):
        tcp = socket.socket(socket.AF_UNIX, socket.SOCK_STREAM)
        tcp.connect(self.queueSocket)
        tcp.send('{"command": "write", "data":  '+json.dumps(data)+'}\n\n\n')
        recvData = tcp.recv(1024)
        tcp.close()
        
        return True    
        
    def queueMgmtclean(self):
        tcp = socket.socket(socket.AF_UNIX, socket.SOCK_STREAM)
        tcp.connect(self.queueSocket)
        tcp.send('{"command": "clean"}\n\n\n')
        recvData = tcp.recv(1024)
        tcp.close()    
        
        return True      
    
    def queueMgmtSaveBuffer(self):
        tcp = socket.socket(socket.AF_UNIX, socket.SOCK_STREAM)
        tcp.connect(self.queueSocket)
        tcp.send('{"command": "savebuffer"}\n\n\n')
        recvData = tcp.recv(1024)
        tcp.close()    
        
        return True        
    
    def syncTags(self, tags):
        try:
            #post em /setuptags
            #dados enviados como {'data': ['{tag name}:{tag address}:{tag type}','{tag name}:{tag address}:{tag type}']}
            #dados recebidos como {'data': {'tag':'id','tag','id'}}
            
            #print("sync Tags")
            
            if (("hostname" not in self.config) and 
                    ("hostport" not in self.config)):
                #print("Configuracao incompleta")
                return {}
            
            #envia dados da tag e o projeto a qual pertence
            body = {'project':self.config["project"], 'data': tags}
            
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
        #inicia conexao
        if(not hasattr(self, 'mongoClient')):
            try:
                print("Connectando ao banco de dados")
                self.mongoClient = MongoClient("mongodb+srv://optcemi01:c3m1t3ch@optcemi01.d3jgj.mongodb.net/myFirstDatabase?retryWrites=true&w=majority", appname="OptSync30", retryWrites=True, compressors="zlib", zlibCompressionLevel=9)
                
            except:
                print("Não foi possivel connectar ao servidor")
                print("post error response")
                resp = {"status": 0, "desc":"ln 1304 fail to add data on database"}
                return None
            
        elif(not self.mongoClient.admin.command('ping')):
            try:
                print("Reconnectando ao banco de dados")
                self.mongoClient = MongoClient("mongodb+srv://optcemi01:c3m1t3ch@optcemi01.d3jgj.mongodb.net/myFirstDatabase?retryWrites=true&w=majority", appname="OptSync30", retryWrites=True, compressors="zlib", zlibCompressionLevel=9)
                
            except:
                print("Não foi possivel connectar ao servidor")
                print("post error response")
                resp = {"status": 0, "desc":"ln 1304 fail to add data on database"}
                return None
                 
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
            
            if("date" not in data):
                print("date not found")
                return None
            
            #if(("data" not in data) and 
            #    ("date" not in data)):
            #    #print("requisicao incompleta")
            #    return {}
            
            #separa data e hora para formatar os dados
            '''
            dt = str(data["date"]).split(" ")[0]
            hr = str(data["date"]).split(" ")[1]
            
            
            #print("sync Tags")
            body = {'project': self.config["project"], 'machineid': self.config["machineid"], 'date': dt, 'deviceimei': self.config["deviceimei"], 'devicenumber': 0, 'data': {hr: data["data"]}}
            '''
            
            body = {'project': self.config["project"], 'machineid': self.config["machineid"], 'date': datetime.strptime(data["date"], '%Y-%m-%d %H:%M:%S'), 'deviceimei': self.config["deviceimei"], 'devicenumber': 0}
            
            #cria uma copia dos dados para criar uma querye de documento unico
            query =  {'project': self.config["project"], 'machineid': self.config["machineid"], 'date': datetime.strptime(data["date"], '%Y-%m-%d %H:%M:%S'), 'deviceimei': self.config["deviceimei"], 'devicenumber': 0}
            
            #transforma dados em json
            #data["data"] = json.loads(data["data"])
                        
            for i in data["data"]:
                body[i.keys()[0]] = {}
                body[i.keys()[0]]["value"] = self.toFloat(i[i.keys()[0]].replace(",",".").split("`")[0])
                body[i.keys()[0]]["quality"] = (int(i[i.keys()[0]].replace(",",".").split("`")[1]))
            
            '''    
            #manda dado em base 64 compactado
            body = {"data": base64.b64encode(zlib.compress(json.dumps(body)))}
            print("sending to -> " + "http://"+self.config["hostname"]+":"+self.config["hostport"]+"/tagdata")
            #print("body -> " + json.dumps(body))
            
            req = requests.post("http://"+self.config["hostname"]+":"+self.config["hostport"]+"/tagdata", None, body)
            '''
            #declara um resultado de erro para caso a opeeracao falhe
            reqResult = 0
            
            try:
                #faz o anexo dos dados no banco de dados diretamnete
                #seleciona uma colecao
                valueCollection = self.mongoClient["optcemi01"].get_collection("values")
                
                #requisicao de operacoes mongo
                operationRequest = []
                
                #prepara uma requisicao adicionando documento se necessario, caso contrario atualiza o mesmo "nao deve acontecer mas por via de duvidas"
                operationRequest.append(mongoOperations.UpdateMany(query , {"$set": body}, True))

                #executa sequencia de operacoes sequenciais
                result = valueCollection.bulk_write(operationRequest)
                
                print("_________________________")
                print(result.bulk_api_result)
                print("_________________________")
                
                #verufica resultado sendo que cada um responde 0 ou 1
                reqResult = result.bulk_api_result["nModified"] + result.bulk_api_result["nUpserted"] + result.bulk_api_result["nMatched"]+ result.bulk_api_result["nRemoved"]+ result.bulk_api_result["nInserted"]
                
            except Exception as e:
                print("Mongo Server not available")
                print(e)
                        
            if(reqResult >= 1):
                #print(req.text)                
                resp = {"status": 1, "desc":"success to add data on database"}
                return resp
            else:
                print("post error response")
                resp = {"status": 0, "desc":"ln 1304 fail to add data on database"}
                return None
            
        except Exception as e:
            print(data)
            print(e)
            print("falha ao enviar dados ao servidor -> "+str(e))      
            if("Temporary failure in name resolution" in str(e)):
                print("Erro de DNS Detectado - Reiniciando em " + str(self.dnsTryes))
                '''
                #reconfigura wpa e espera 5 seg
                os.system("wpa_cli -i wlan0 disconnect")
                os.system("wpa_cli -i wlan0 reconfigure")
                os.system("wpa_cli -i wlan0 reassociate")
                time.sleep(5000)
                
                #reinicia serviço de dns
                if(self.dnsTryes <= 0):
                    os.system("reboot now")
                '''
                self.dnsTryes -= 1
                #espera 30 segundos para a rede retornar
                time.sleep("30000")
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
                
                #print("configuracao incompleta")
                return None
            
            body = {'tagdata': data}
            
            req = requests.post("http://"+self.config["hostname"]+":"+self.config["hostport"]+"/pertoper/"+self.config["pertopersender"]+"/"+self.config["pertoperreceiver"], None, body)
            
            if(req.ok):
                #print(req.text)                
                resp = json.loads(req.text.replace("\'", "\""))
                return resp
            else:
                return None
        except Exception as e:
            print(e)
            print("falha ao enviar dados ao servidor -> "+str(e))         
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
                
                #print("configuracao incompleta")
                return None
            
            #print("sync Tags")
            #body = body = {'senderid': self.config["pertopersender"], 'receiverid': self.config["pertoperreceiver"]}
            
            req = requests.get("http://"+self.config["hostname"]+":"+self.config["hostport"]+"/pertoper/"+self.config["pertopersender"]+"/"+self.config["pertoperreceiver"], None, None)
            
            if(req.ok):
                #print(req.text)                
                resp = json.loads(req.text.replace("\'", "\""))
                return resp
            else:
                return None
        except Exception as e:
            print(e)
            print("falha ao enviar dados ao servidor -> "+str(e))        
            return None
        
        #connection.request("POST", '/setuptags', body=json.dumps(body))
    def setWifiMode(self):
        os.system("setwifi")
        
    def setMobileMode(self):    
        os.system("setppp")        

    def toFloat(self, d):
        try:
             return float(d)
        except:
            return str(d)
        
        
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
        #print('Conectado por' + str(client))

        while(True):
            try:
                msg = ""
                
                while(not msg.endswith("\n\n\n")):  
                    msg += con.recv(1024)
                
                if(not msg):
                    msg = ""
                    continue
                
                if(len(msg) < 4):
                    msg = ""
                    continue
                
                #retira termino
                msg = msg[0:-3]
                #print(msg)
                       
                loadedMsg = json.loads(msg)
                
                #print("loadedMsg -> ", loadedMsg)
                
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
                con.sendall("messaging error -> "+str(e))
                
            finally:
                #print('Finalizando conexao do cliente', client)
                con.close()
                thread.exit()
        
    def startSocket(self):
        tcp = socket.socket(socket.AF_UNIX, socket.SOCK_STREAM)
        tcp.bind(self.queueSocket)
        tcp.listen(2)

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
                #print(">>>>>>>>>>>>>>>>>>>data is locked")
                return json.dumps({})
                
                
            elif(len(self.buffer) == 0):
                #bloqueia o arquivo para escrita e leitura
                self.lock = True
                
                #variavel temporaria para armazenar fila
                queueData = []
                
                tmp = open(self.queueFile, "rb")
                queueData = json.loads(tmp.read())
                tmp.close()
                
                #preenche o buffer com o proximo dado
                if(len(queueData) > 0):
                    self.buffer = queueData.pop(0)
                
                #remove dados ja carregados em buffer (pode ser prejudicial por perder dados no caminho - verificar opcoes) guaradando o resto
                tmp = open(self.queueFile, "wb")
                tmp.write(json.dumps(queueData))
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
                #print(">>>>>>>>>>>>>>>>>>>data is locked")
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
            #print(">>>>>>>>>>>>>>>>>>>data is locked")
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
                #print(">>>>>>>>>>>>>>>>>>>data is locked")
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
            #print("tags is list")
            for tag in tags:
                #print(type(tags))
                if(type(tag) == dict):
                    #adiciona tag a lissta do arquivo
                    #print("tags is dict")
                    if(tag.keys()[0] not in data):
                        data[tag.keys()[0]] = tag[tag.keys()[0]]
                        
        elif(type(tags) == dict):
            #print("tags is dict")
            #adiciona tag a lissta do arquivo
            if(tags.keys()[0] not in data):
                data[tags.keys()[0]] = tags[tags.keys()[0]]
    
        else:
            print("invalid")
            
        #print(data)
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
            #print("tags is dict")
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
            #print("tags is list")
            for tag in tags:
                #print(type(tags))
                if(type(tag) == dict):
                    #adiciona tag a lissta do arquivo
                    #print("tags is dict")
                    data[tag.keys()[0]] = tag[tag.keys()[0]]
                        
        elif(type(tags) == dict):
            #print("tags is dict")
            #adiciona tag a lissta do arquivo
            data[tags.keys()[0]] = tags[tags.keys()[0]]
    
        else:
            print("invalid")
            
        #print(data)
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
        
        #print(type(tags))
        
        if(type(tags) == list):
            #print("tags is list")
            for tag in tags:
                #print(type(tags))
                if(type(tag) == dict):
                    #adiciona tag a lissta do arquivo
                    #print("tags is dict")
                    response[tag.keys()[0]] = data[tag.keys()[0]]
                    
                elif(type(tag) == str):
                    #adiciona tag a lissta do arquivo
                    #print("tags is text")
                    response[tag] = data[tag]
                        
        elif(type(tags) == dict):
            #print("tags is dict")
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
            #print("tags is list")
            for tag in tags:
                #print(type(tags))
                if(type(tag) == dict):
                    #adiciona tag a lissta do arquivo
                    #print("tags is dict")
                    if(tag.keys()[0] in data):
                        del data[tag.keys()[0]]
                        
        elif(type(tags) == dict):
            #print("tags is dict")
            #adiciona tag a lissta do arquivo
            if(tags.keys()[0] in data):
                del data[tags.keys()[0]]
    
        else:
            if(tags in data):
                del data[tags]
            
        #print(data)
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
version = "2.0.22"

ser = serial.Serial('/dev/serial0', 115200)  # open serial port
#ser.set_buffer_size(rx_size = 12800, tx_size = 12800)
##print(ser.name)         # check which port was really used
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
        
        try:  
            buffer = ""
            while("\r\n" not in buffer):
                #verifica se tem dados para ler
                if(ser.in_waiting >= 1):
                    buffer += ser.read(ser.in_waiting)
        
        except Exception as e:
            print(e)
            
                    
        #os.system('cls' if os.name == 'nt' else 'clear')
        
        #limpa caracteres de quebra de linha
        buffer = buffer.replace("\r","").replace("\n", "")
        
        print("buffer")
        print(len(buffer))
            
        
        #message = ser.read_until("\r\n", 1024000)
        
        #print("message -> "+ buffer)
            
        jsonData = tc.messageValidation(buffer)
#        message = ser.readline()
        #jsonData = tc.messageValidation(message)
        
        
        #message = ""
        
        if(jsonData == None):
            continue
        
        print("jsonData")
        print(jsonData["command"])    
        
        #captura resposta
        response = tc.processCommand(jsonData)
        
        print("response")
        print({'status': response[0], 'lenght': len(response[1])})
        
        #jsonData = tc.messageValidation("|79|{\"command\":\"settagdata()\", \"data\":\"settagdata()\", \"date\":\"2021-01-29 16:54:55\"}")

        ser.write(tc.sendResponse(json.dumps({'date' : str(datetime.now()), 'status': response[0], 'version': version, 'desc': response[1]})) + "\r\n")
    except Exception as e:
        print("******************Erro ao receber Mensagem***************************")
        print(e)
        print("*********************************************************************")
        exit()
        try:
            if(ser.isOpen()):
                ser.close()
        except:
            print("error")
