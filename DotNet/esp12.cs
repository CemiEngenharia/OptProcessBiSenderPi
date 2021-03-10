using System;
using System.IO;  
using System.Net; 
using System.Text; 
using System.IO.Ports;
using System.Net.Sockets;
using System.IO.Compression;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Security.Cryptography;
using System.Threading;
using TitaniumAS.Opc.Client;
using TitaniumAS.Opc.Client.Da.Browsing;
using TitaniumAS.Opc.Client.Common;
using TitaniumAS.Opc.Client.Da;
using System.Text.RegularExpressions;
using System.Web;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Web;

namespace OptProcessBiSenderService
{
    
    class espressific
    {
        public class Common:DeviceCommon{
            public override Dictionary<string, string> getDeviceData(SerialPort sp)
            {
                Dictionary<string, string> info = new Dictionary<string, string>();
                //recupera imei do dispositivo
                int errors = 0;
                int allowedErrors = 10;
                string incoming = "";

                sp.Write("\r\n");
                Thread.Sleep(20);
                sp.DiscardInBuffer();

                //desabilita echo
                echo:
                sp.DiscardInBuffer();
                if(errors > allowedErrors) throw new OperationCanceledException("Foi Alcançado o numero maximo de tentativas de acesso ao modem");
                sp.Write("ATE0\r\n");
                incoming = "";
                do{incoming += sp.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));
                if(! incoming.Contains("")){Thread.Sleep(1000); errors++; goto echo;}

                //recupera o imei do device
                imei:
                sp.DiscardInBuffer();
                if(errors > allowedErrors) throw new OperationCanceledException("Foi Alcançado o numero maximo de tentativas de acesso ao modem");
                sp.Write("AT+CIPSTAMAC_CUR?\r\n");
                incoming = "";
                do{incoming += sp.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));
                if(! incoming.Contains("")){Thread.Sleep(1000); errors++; goto imei;}

                incoming = incoming.Replace("+CIPSTAMAC_CUR:", "").Replace("OK", "").Replace("+PACSP1", "").Replace("\"","").Replace("\r", "").Replace("\n", "");
                
                info["imei"] = incoming;

                //recupera firmware version
                firmware:
                sp.DiscardInBuffer();
                if(errors > allowedErrors) throw new OperationCanceledException("Foi Alcançado o numero maximo de tentativas de acesso ao modem");
                sp.Write("AT+GMR\r\n");
                incoming = "";
                do{incoming += sp.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));
                if(! incoming.Contains("")){Thread.Sleep(1000); errors++; goto firmware;}

                incoming = incoming.Replace("OK", "").Replace("+PACSP1", "").Replace("\r", "").Replace("\n", "");
                
                info["firmware"] = incoming;

                //recupera firmware version
                simCardId:
                sp.DiscardInBuffer();
                if(errors > allowedErrors) throw new OperationCanceledException("Foi Alcançado o numero maximo de tentativas de acesso ao modem");
                sp.Write("AT+CWJAP_CUR?\r\n");
                incoming = "";
                do{incoming += sp.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));
                if(! incoming.Contains("")){Thread.Sleep(1000); errors++; goto simCardId;}

                //Console.WriteLine(incoming);    
                    //Console.Write((incoming));

                incoming = incoming.Replace("+CME ERROR: ", "").Replace("OK", "").Replace("+CWJAP_CUR:", "").Replace("\r", "").Replace("\n", "");
                
                info["SIMID"] = incoming;

                return info;
            }
        }
        
        public class conectionProcedures:DeviceconectionProcedures
        {
            string apn;
            string user;
            string pwd;
            int mode;     
            string ipAddress;  //0 caso seja orientado a socket, 1 caso seja oritentado a serial
            string application;
            string machineId;
            string deviceMac;
            SerialPort serial;
            Socket socket;
            string ftpFileDir;
            public string networkSSID;
            public string networkPASS;

            /* dados referentes ao wifi based */
            string ssid;
            string pass;
            string ip;
            string gateway;
            string netmask;

            public conectionProcedures(string _deviceMac, string _machineId, string _ssid, string _pass, string _ip, string _gateway, string _mask, SerialPort _serialPort)
            {                
                this.ssid = _ssid;
                this.pass = _pass;
                this.ip = _ip;
                this.gateway = _gateway;
                this.netmask = _mask;
                
                this.serial = _serialPort;
                this.mode = 1;
                this.deviceMac = _deviceMac;
                this.machineId = _machineId;
                this.ftpFileDir = ftpFileDir = this.deviceMac+"/"+this.machineId;
            }   
            public conectionProcedures(string _deviceMac, string _machineId, string _ssid, string _pass, string _ip, string _gateway, string _mask, Socket _socket)
            {
                this.ssid = _ssid;
                this.pass = _pass;
                this.ip = _ip;
                this.gateway = _gateway;
                this.netmask = _mask;
                
                this.socket = _socket;
                this.mode = 0;
                this.deviceMac = _deviceMac;
                this.machineId = _machineId;
                this.ftpFileDir = ftpFileDir = this.deviceMac+"/"+this.machineId;
            }    
            
            object sendPacket(string _buffer, string _responsePatern, string _start = "\r\n", string _end = "\r\n")
            {
                int _errors = 0;
                int _allowedErrors = 3;

                executeRequest:
                if(_errors > _allowedErrors)  return null;
                
                //limpa buffers
                serial.DiscardInBuffer();
                serial.DiscardOutBuffer();

                Console.Write("packets sent -> ");
                Console.Write(_buffer);

                serial.Write(_start+_buffer+_end);
                //Console.WriteLine("Enviando");
                //Console.WriteLine(_buffer);
                string incoming = "";
                int timeout = 600;      //timeout de 1 minuto devico ao post
                do{
                    string buff = serial.ReadExisting();
                    if(buff.Length >= 1)
                    {
                        incoming += buff;//.Replace("\r","").Replace("\n",""); 
                        timeout = 600;
                    }
                    else
                    {
                        timeout --;
                        Thread.Sleep(100);
                    }
                    if(timeout <= 0)    break;
                } while(!Regex.Match(incoming, _responsePatern,RegexOptions.Singleline).Success && !incoming.Contains("ERROR"));
                
                Console.Write(" --- packets received -> ");
                Console.WriteLine(incoming);

                Match match = Regex.Match(incoming, _responsePatern,RegexOptions.Singleline);
                if(!match.Success){Thread.Sleep(1000); _errors++; goto executeRequest;}

                if(match.Groups.Count >= 2)
                {
                    return(match.Groups[1].Value);
                }
                return true;
            }

            public override List<Dictionary<string, string>> listSSID(int allowedErrors=3)
            {
                List<Dictionary<string, string>> ssid = new List<Dictionary<string, string>>();

                string incoming = "";

                incoming = (string) this.sendPacket("AT+CWLAP", "(.*)OK.*", "");

                // response patern
                // \+CWLAP\:\((\d+),"(.+)",(-?\d+),"(\S+)",(\d+),(-?[\d+,\d+]+)\)
                // list all ap

                string _responsePatern = "\\+CWLAP\\:\\((\\d+),\"(.+)\",(-?\\d+),\"(\\S+)\",(\\d+),(-?\\d+),(-?\\d+)\\)";
            
                MatchCollection match = Regex.Matches(incoming, _responsePatern, RegexOptions.Multiline);

                for(int i=0; i< match.Count; i++)
                {
                    if(match[i].Groups.Count >= 6)
                    {
                        Dictionary<string, string> tmp = new Dictionary<string, string>();

                        tmp["encription"] = match[i].Groups[1].Value;
                        tmp["ssid"] = match[i].Groups[2].Value;
                        tmp["signal"] = match[i].Groups[3].Value;
                        tmp["mac"] = match[i].Groups[4].Value;
                        tmp["channel"] = match[i].Groups[5].Value;
                        tmp["freq_offset"] = match[i].Groups[6].Value;
                        tmp["freq_cali"] = match[i].Groups[7].Value;                    
                        ssid.Add(tmp);
                    }
                }

                return ssid;
            }
            public override bool pdpConection(int allowedErrors=3)
            {
                string incoming = "";
                int errors = 0;
                    
                int retry = 3;
                _retry:
                try{
                    //procedimentos orientados a serial
                    if(mode == 1)
                    {
                        Console.WriteLine("Definindo dados de Ip");
                        this.setIp(ip, gateway, netmask);
                        Console.WriteLine("Conectando a Rede -> " + this.ssid);
                        this.connectNetwork(this.ssid, this.pass);
                    }
                    return true;
                }
                catch(TimeoutException)
                {
                    if(retry <= 0)  return false;
                    retry--;
                    goto _retry;
                }
                catch(Exception except)
                {
                    if((except is InvalidOperationException) || (except is OperationCanceledException))
                        Console.WriteLine("The Serial Port is Closed");
                    return false;
                }
            }

            public override bool isConnected()
            {
                int retry = 3;
                _retry:
                int statusCode = 0;

                //AT+CIPSTATUS
                try
                {
                    string incoming = "";
                    serial.DiscardInBuffer();

                    incoming = (string) this.sendPacket("AT+CIPSTATUS", "(.*)OK.*", "");

                    // response patern
                    // \+CWLAP\:\((\d+),"(.+)",(-?\d+),"(\S+)",(\d+),(-?[\d+,\d+]+)\)
                    // list all ap

                    string _responsePatern = "STATUS\\:(\\d)";
                
                    Match match = Regex.Match(incoming, _responsePatern, RegexOptions.Singleline);

                    if(match.Groups.Count >= 1)
                    {
                        statusCode = int.Parse(match.Groups[1].Value);    
                    }
                }
                catch(TimeoutException)
                {
                    if(retry <= 0)  return false;
                    retry--;
                    goto _retry;
                }
                catch(Exception except)
                {
                    if((except is InvalidOperationException) || (except is OperationCanceledException))
                        Console.WriteLine("Thre Serial Port is Closed");
                    return false;
                }

                return statusCode < 5;
            }
            public bool setIp(string ipAddress, string gateway = null, string netmask = null)
            {
                int retry = 3;
                _retry:
                //AT+CIPSTATUS
                try
                {
                    string incoming = "";

                    //define o hostname do device
                    this.sendPacket("AT+CWHOSTNAME=\"CEMI_OPT_BI_SENDER\"", "");

                    if((gateway == null) || (netmask == null))
                        incoming = (string) this.sendPacket("AT+CIPSTA=\""+ipAddress+"\"", "(.*OK.*|.*FAIL.*)", "");
                    else
                        incoming = (string) this.sendPacket("AT+CIPSTA=\""+ipAddress+"\",\""+gateway+"\",\""+netmask+"\"", "(.*OK.*|.*FAIL.*)", "");

                    // response patern
                    // \+CWLAP\:\((\d+),"(.+)",(-?\d+),"(\S+)",(\d+),(-?[\d+,\d+]+)\)
                    // list all ap

                    //response
                    /*
                    AT+CIPSTA?

                    +CIPSTA:ip:"192.168.1.141"
                    +CIPSTA:gateway:"192.168.1.1"
                    +CIPSTA:netmask:"255.255.255.0"

                    OK
                    */

                    string _responsePatern = "(WIFI.*)(WIFI.*)OK";
                
                    Match match = Regex.Match(incoming, _responsePatern, RegexOptions.Singleline);

                    if(match.Groups.Count >= 2)
                    {
                        return true;
                    }
                    // "\r\nWIFI CONNECTED\r\nWIFI GOT IP\r\n\r\nOK";
                }
                catch(TimeoutException)
                {
                    if(retry <= 0)  return false;
                    retry--;
                    goto _retry;
                }
                catch(Exception except)
                {
                    if((except is InvalidOperationException) || (except is OperationCanceledException))
                        Console.WriteLine("Thre Serial Port is Closed");
                    return false;
                }
                
                return false;
            }
            public bool connectNetwork(string ssid, string password)
            {
                int retry = 3;
                _retry:
                //AT+CIPSTATUS
                try
                {
                    string incoming = "";

                    incoming = (string) this.sendPacket("AT+CWJAP=\""+ssid+"\",\""+password+"\"", "(.*OK.*|.*FAIL.*)", "");

                    // response patern
                    // \+CWLAP\:\((\d+),"(.+)",(-?\d+),"(\S+)",(\d+),(-?[\d+,\d+]+)\)
                    // list all ap

                    string _responsePatern = "(WIFI.*)(WIFI.*)OK";
                
                    Match match = Regex.Match(incoming, _responsePatern, RegexOptions.Singleline);

                    if(match.Groups.Count >= 2)
                    {
                        return true;
                    }
                    // "\r\nWIFI CONNECTED\r\nWIFI GOT IP\r\n\r\nOK";
                }
                catch(TimeoutException)
                {
                    if(retry <= 0)  return false;
                    retry--;
                    goto _retry;
                }
                catch(Exception except)
                {
                    if((except is InvalidOperationException) || (except is OperationCanceledException))
                        Console.WriteLine("Thre Serial Port is Closed");
                    return false;
                }
                
                return false;
            }
/*
            public string httpRequest(string _type, string _url, int _port = 80 ,string _header = null , string _data = null, string _user=null, string _pass=null)
            {
                __startRequest:
                
                //defne variavel de dados para retorno
                string data = "";

                object sendPacket(string _buffer, string _responsePatern, string _start = "\r\n", string _end = "\r\n")
                {
                    int _errors = 0;
                    int _allowedErrors = 3;

                    executeRequest:
                    if(_errors > _allowedErrors)  return null;
                    
                    //limpa buffers
                    serial.DiscardInBuffer();
                    serial.DiscardOutBuffer();

                    Console.Write("packets sent -> ");
                    Console.Write(_buffer);

                    serial.Write(_start+_buffer+_end);
                    //Console.WriteLine("Enviando");
                    //Console.WriteLine(_buffer);
                    string incoming = "";
                    int timeout = 600;      //timeout de 1 minuto devico ao post
                    do{
                        string buff = serial.ReadExisting();
                        if(buff.Length >= 1)
                        {
                            incoming += buff;//.Replace("\r","").Replace("\n",""); 
                            timeout = 600;
                        }
                        else
                        {
                            timeout --;
                            Thread.Sleep(100);
                        }
                        if(timeout <= 0)    incoming = "ERROR -> Request TimedOut";
                    } while(!Regex.Match(incoming, _responsePatern,RegexOptions.Singleline).Success && !incoming.Contains("ERROR"));
                    
                    Console.Write(" --- packets received -> ");
                    Console.WriteLine(incoming);

                    Match match = Regex.Match(incoming, _responsePatern,RegexOptions.Singleline);
                    if(!match.Success){Thread.Sleep(1000); _errors++; goto executeRequest;}

                    if(match.Groups.Count >= 2)
                    {
                        return(match.Groups[1].Value);
                    }

                    return null;
                }
                
                string url = _url;
                string query = "";

                Console.WriteLine(url);

                if(url.Contains("://"))
                    url = url.Substring(url.IndexOf("://")+3);

                Console.WriteLine(url);
                
                if(url.Contains("/"))
                {
                    query = url.Substring(url.IndexOf('/')+1);
                    url = url.Substring(0,url.IndexOf('/'));
                }

                Console.WriteLine(url);
                
                //resolve dns
                int dnsTrys = 3;
                __getServerIp:
                string serverIp = (string) sendPacket("AT+UDNSRN=0,\""+url+"\"",  ".*\\+UDNSRN\\:.*\\\"(.*)\\\"");
                if(serverIp == null)
                {
                    if(dnsTrys <=0) return null;
                    dnsTrys --;
                    Thread.Sleep(1000);
                    goto __getServerIp;
                }

                //faz requisicao ja no ip do dns
                sendPacket("AT+UHTTP=0",  @".*OK.*");
                sendPacket("AT+UHTTP=0,0,\""+serverIp+"\"",  @".*OK.*");

                sendPacket("AT+UHTTP=0,5,"+_port.ToString()+"",  @".*OK.*");

                if((_user != null) && (_pass != null))
                {
                    //preenche usuario e senha caso haja autenticação
                    sendPacket("AT+UHTTP=0,2,\""+_user+"\"",  @".*OK.*");
                    sendPacket("AT+UHTTP=0,3,\""+_pass+"\"",  @".*OK.*");
                    sendPacket("AT+UHTTP=0,4,1",  @".*OK.*");
                }

                if(_type == "GET")
                {
                    //faz requisicao get da url salvando no arquivo req.tmp internamente
                    string ret = (string) sendPacket("AT+UHTTPC=0,1,\"/"+query+"\",\"tmp.tmp\"", @"(.*\+UUHTTPCR\:.*\d,\d,1.*)");
                    Console.WriteLine("ret");
                    Console.WriteLine(ret);
                    if(ret == null)
                    {
                        Console.WriteLine("Falha ao Fazer Requisição");
                        goto __startRequest;
                    }
                }
                else if(_type == "HEAD")
                {
                    //faz requisicao get da url salvando no arquivo req.tmp internamente
                    if(sendPacket("AT+UHTTPC=0,0,\"/"+query+"\",\"tmp.tmp\"", @"(.*\+UUHTTPCR\:.*\d,\d,1.*)") == null)
                    {
                        Console.WriteLine("Falha ao Fazer Requisição");
                        goto __startRequest;
                    }
                }
                else if((_type == "POST") && (_data.Length >= 1))
                {
                    //faz requisicao get da url salvando no arquivo req.tmp internamente
                    Console.WriteLine("Sending Post Request");

                    //escreve o buffer do post
                    sendPacket("AT+UDELFILE=\"upload\"", ".*((OK)|(FILE NOT FOUND)).");
                    sendPacket("AT+UDWNFILE=\"upload\","+_data.Length.ToString() , @".*\>.*");
                    sendPacket(_data, ".*OK.*", "", "");

                    //faz upload dos dados do post
                    if(sendPacket("AT+UHTTPC=0,4,\"/"+query+"\",\"tmp.tmp\",\"upload\",0", @"(.*\+UUHTTPCR\:.*\d,\d,1.*)") == null)
                    {
                        Console.WriteLine("Falha ao Fazer Requisição");
                        goto __startRequest;
                    }

                    sendPacket("AT+UDELFILE=\"upload\"", ".*OK.");
                    //sendPacket("AT+UHTTPC=0,5,\"/"+query+"\",\"tmp.tmp\",\""+_data+"\",0", @".*\+UUHTTPCR\:.*\d,\d,\d.*");
                }

                //le o arquivo no device
            
                int errors = 0;
                int allowedErrors = 3;

                Console.WriteLine("Lendo Arquivo de Resposta da Requisição");

                string patern = "\\+URDFILE\\:.*\\\".*\\\",\\d+,\\\"(.*)\\\"";
                Console.WriteLine(patern);

                data = (string) sendPacket("\r\nAT+URDFILE=\"tmp.tmp\"\r\n", patern);

                Console.WriteLine("___________________data 0");
                Console.WriteLine(data);

                //caso nao haja um resultado 200 retorna null
                Match respCode = Regex.Match(data, @".*(HTTP/\d.\d 200).*" ,RegexOptions.Singleline);
                if(!respCode.Success)
                    return null;
                
                Console.WriteLine("___________________data 1");
                Console.WriteLine(data);

                if(data.IndexOf("\r\n\r\n") >= 0)
                    data = data.Substring(data.IndexOf("\r\n\r\n"));

                Console.WriteLine("___________________data 2");
                Console.WriteLine(data);

                return data;
            }
*/
            public override int startSocket(string url, string hostPort)
            {
                //defne variavel de dados para retorno
                string data = "";

                object sendPacket(string _buffer, string _responsePatern, string _start = "\r\n", string _end = "\r\n")
                {
                    int _errors = 0;
                    int _allowedErrors = 3;

                    executeRequest:
                    if(_errors > _allowedErrors)  return null;
                    
                    //limpa buffers
                    serial.DiscardInBuffer();
                    serial.DiscardOutBuffer();

                    Console.Write("packets sent -> ");
                    Console.Write(_buffer);

                    serial.Write(_start+_buffer+_end);
                    //Console.WriteLine("Enviando");
                    //Console.WriteLine(_buffer);
                    string incoming = "";
                    int timeout = 600;      //timeout de 1 minuto devico ao post
                    do{
                        string buff = serial.ReadExisting();
                        if(buff.Length >= 1)
                        {
                            incoming += buff;//.Replace("\r","").Replace("\n",""); 
                            timeout = 600;
                        }
                        else
                        {
                            timeout --;
                            Thread.Sleep(100);
                        }
                        if(timeout <= 0)    break;
                    } while(!Regex.Match(incoming, _responsePatern,RegexOptions.Singleline).Success && !incoming.Contains("ERROR"));
                    
                    Console.Write(" --- packets received -> ");
                    Console.WriteLine(incoming);

                    Match match = Regex.Match(incoming, _responsePatern,RegexOptions.Singleline);
                    if(!match.Success){Thread.Sleep(1000); _errors++; goto executeRequest;}

                    if(match.Groups.Count >= 2)
                    {
                        return(match.Groups[1].Value);
                    }
                    return true;
                }

                //resolve dns +CIPDOMAIN:191.6.204.199

                int dnsTryes = 0;
                _tryToGetDns:

                string serverIp = (string) sendPacket("AT+CIPDOMAIN=\""+url+"\"",  ".*\\+CIPDOMAIN\\:(.*)\r\n\r\n");

                if(serverIp.Length < 7)
                {
                    if(dnsTryes >= 5)
                        return -1;

                    dnsTryes ++;
                    goto _tryToGetDns;
                }
/*
                //cria socket e recupera seu numero         +USOCR: 0     
                string socketNumber = (string) sendPacket("AT+CIPSTART=6",  ".*\\+USOCR:\\s(\\d+)");

                Console.WriteLine("Socket aberto com numero -> " + socketNumber);
*/
                //slicita conexao no ip e porta
                string connection = (string) sendPacket("AT+CIPSTART=\"TCP\",\""+serverIp+"\","+hostPort, ".*(OK)|(ALREADY CONNECT).*");

                Console.WriteLine("Connection Response -> "+connection);

                if(connection != null)
                    if((connection.Equals("OK")) || (connection.Equals("ALREADY CONNECT")))
                        return int.Parse("0");
                
                return -1;
            }

            public override bool closeSocket(int socketNumber)
            {
                object sendPacket(string _buffer, string _responsePatern, string _start = "\r\n", string _end = "\r\n")
                {
                    int _errors = 0;
                    int _allowedErrors = 3;

                    executeRequest:
                    if(_errors > _allowedErrors)  return null;
                    
                    //limpa buffers
                    serial.DiscardInBuffer();
                    serial.DiscardOutBuffer();

                    Console.Write("packets sent -> ");
                    Console.Write(_buffer);

                    serial.Write(_start+_buffer+_end);
                    //Console.WriteLine("Enviando");
                    //Console.WriteLine(_buffer);
                    string incoming = "";
                    int timeout = 600;      //timeout de 1 minuto devico ao post
                    do{
                        string buff = serial.ReadExisting();
                        if(buff.Length >= 1)
                        {
                            incoming += buff;//.Replace("\r","").Replace("\n",""); 
                            timeout = 600;
                        }
                        else
                        {
                            timeout --;
                            Thread.Sleep(100);
                        }
                        if(timeout <= 0)    break;
                    } while(!Regex.Match(incoming, _responsePatern,RegexOptions.Singleline).Success && !incoming.Contains("ERROR"));
                    
                    Console.Write(" --- packets received -> ");
                    Console.WriteLine(incoming);

                    Match match = Regex.Match(incoming, _responsePatern,RegexOptions.Singleline);
                    if(!match.Success){Thread.Sleep(1000); _errors++; goto executeRequest;}

                    if(match.Groups.Count >= 2)
                    {
                        return(match.Groups[1].Value);
                    }
                    return null;
                }

                if(socketNumber < 0)
                    for(int i=0; i<7; i++)
                        sendPacket("AT+CIPCLOSE" ,  ".*(OK|ERROR).*");
                else
                {
                    string closed = (string) sendPacket("AT+CIPCLOSE" ,  ".*(OK).*");
                    if(closed == null)    closed = "";
                    if(closed == "OK")
                        return true;
                }
                return false;

            }

            public override bool startContinuousSocket(int socketNumber)
            {
                object sendPacket(string _buffer, string _responsePatern, string _start = "\r\n", string _end = "\r\n")
                {
                    int _errors = 0;
                    int _allowedErrors = 3;

                    executeRequest:
                    if(_errors > _allowedErrors)  return null;
                    
                    //limpa buffers
                    serial.DiscardInBuffer();
                    serial.DiscardOutBuffer();

                    Console.Write("packets sent -> ");
                    Console.Write(_buffer);

                    serial.Write(_start+_buffer+_end);
                    //Console.WriteLine("Enviando");
                    //Console.WriteLine(_buffer);
                    string incoming = "";
                    int timeout = 100;      //timeout de 1500 mili devico ao post
                    do{
                        string buff = serial.ReadExisting();
                        if(buff.Length >= 1)
                        {
                            incoming += buff;//.Replace("\r","").Replace("\n",""); 
                            timeout = 100;
                        }
                        else
                        {
                            timeout --;
                            Thread.Sleep(100);
                        }
                        if(timeout <= 0)    break;
                    } while(!Regex.Match(incoming, _responsePatern,RegexOptions.Singleline).Success && !incoming.Contains("ERROR"));
                    
                    Console.Write(" --- packets received -> ");
                    Console.WriteLine(incoming);

                    Match match = Regex.Match(incoming, _responsePatern,RegexOptions.Singleline);
                    if(!match.Success){Thread.Sleep(1000); _errors++; goto executeRequest;}

                    if(match.Groups.Count >= 2)
                    {
                        return(match.Groups[1].Value);
                    }
                    return null;
                }

                string socketReady = (string) sendPacket("AT+CIFSR",  ".*\\+CIFSR:STAIP,\"(\\d+.\\d+.\\d+.\\d+)\".*\\+CIFSR:STAMAC,\"([0-9a-f]+:[0-9a-f]+:[0-9a-f]+:[0-9a-f]+:[0-9a-f]+:[0-9a-f]+)\".*");
                if(socketReady != null)
                {
                    if(socketReady.Contains(">"))
                        return true;
                }

                return false;
            }
            
            static string GetMd5Hash(MD5 md5Hash, string input)
            {

                // Convert the input string to a byte array and compute the hash.
                byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                // Create a new Stringbuilder to collect the bytes
                // and create a string.
                StringBuilder sBuilder = new StringBuilder();

                // Loop through each byte of the hashed data
                // and format each one as a hexadecimal string.
                for (int i = 0; i < data.Length; i++)
                {
                    sBuilder.Append(data[i].ToString("x2"));
                }

                // Return the hexadecimal string.
                return sBuilder.ToString();
            }
            
            public override string writeContinuousSocket(string data, string response = "\r\n", string token = "qazxc123", int __timeout = 30000, int socket=0)
            {  
                int timeoutPerTryes = __timeout/10;

                //limpa buffers
                serial.DiscardInBuffer();
                //define que recebera os dados diretamente do servidor no ato da escrita
                serial.WriteLine("AT+CIPRECVMODE=0");
                Thread.Sleep(20);
                
                //limpa buffers
                serial.DiscardInBuffer();

                string IV = "CemiProcessOptim";

                using (MD5 md5Hash = MD5.Create())
                {
                    string Tokenhash = GetMd5Hash(md5Hash, token);
/*
                    Console.Write("RawData");
                    Console.Write(data);

                    Console.Write("keyd -> " + Tokenhash);
*/
                    byte[] EncodedData = EncryptStringToBytes_Aes(data, Encoding.ASCII.GetBytes(Tokenhash), Encoding.ASCII.GetBytes(IV));
                    data = Convert.ToBase64String(EncodedData);

                    //poe um final perceptivel no dado
                    data = data+"\r\n";

                    //faz reenvio em caso de erro
                    int _sendTry = 0;
                    _ressend:
/*
                    Console.Write("Encoded");
                    Console.Write(data);
*/
                    //serial.Write(data);

                    //tamanho maximo do pacote definido por hardware
                    int MaxPacketSize = 1023;
                    string buff = "";
                    string incoming = "";

                    //Envia pacotes de 20 em 20 ms para a rede
                    for(int i = 0; i< data.Length; i+=MaxPacketSize)
                    {
                        Console.WriteLine("Envando -> "+i.ToString());

                        Console.WriteLine("AT+CIPSEND=" + ((i+MaxPacketSize) / data.Length < 1 ? MaxPacketSize : data.Length % MaxPacketSize).ToString()+"\r\n");
                        Console.WriteLine(data.Substring(i, ((i+MaxPacketSize) / data.Length ) < 1 ? MaxPacketSize : data.Length % MaxPacketSize));

                        //limpa buffers
                        serial.DiscardInBuffer();

                        //os pacotes devem ser enviados de 2048 em 2048 byte a cada 20ms
                        serial.Write("AT+CIPSEND=" + ((i+MaxPacketSize) / data.Length < 1  ? MaxPacketSize : data.Length % MaxPacketSize).ToString()+"\r\n");
                        Thread.Sleep(10);
                         
                         //aguarda permissao de envio
                        __waitToSend:
                        buff = "";

                        buff += serial.ReadExisting();
                        if(buff.Length >= 1)
                        {    
                            Console.WriteLine("Waiting to send -> "+ buff);

                            if(!buff.Contains(">"))
                            {
                                if(_sendTry >= 10)
                                {
                                    Console.WriteLine("Socket Writing TimedOut");
                                    return null;
                                }
                                _sendTry ++;
                                Thread.Sleep(timeoutPerTryes);
                                goto __waitToSend;
                            }
                            else{
                                buff = "";
                                serial.DiscardInBuffer();
                            }
                        }
                        else
                        {
                            if(_sendTry >= 20)
                            {
                                Console.WriteLine("Socket Writing TimedOut");
                                return null;
                            }
                            _sendTry ++;
                            Thread.Sleep(timeoutPerTryes);
                            goto __waitToSend;
                        }

                        serial.Write(data.Substring(i, ((i+MaxPacketSize) / data.Length < 1 ? MaxPacketSize : data.Length % MaxPacketSize)));
                        Thread.Sleep(30);

                        Console.WriteLine("Enviado ");

                        if((i+MaxPacketSize) / data.Length < 1)
                        {
                            Console.WriteLine("---Entrou---");

                            buff = "";
                            __waitToSendNext:
                            buff += serial.ReadExisting();
                            if(buff.Length >= 1)
                            {    
                                Console.WriteLine("aguardando para enviar o proximo pacote -> "+ buff);

                                if(!buff.Contains("SEND OK"))
                                {
                                    if(_sendTry >= 10)
                                    {
                                        Console.WriteLine("Socket Writing TimedOut");
                                        return null;
                                    }
                                    _sendTry ++;
                                    Thread.Sleep(timeoutPerTryes);
                                    goto __waitToSendNext;
                                }
                                else{
                                    Console.WriteLine("Send OK Recebido");
                                }                        

                                if(buff.Contains("SEND FAIL"))   goto _ressend; 
                            }
                            else
                            {
                                Thread.Sleep(timeoutPerTryes);
                                goto __waitToSendNext;                                
                            }  
                        } 
                    }

                    incoming = "";
                    int timeout = 100;      //timeout de 1 minuto devico ao post
                    //verifica se ja tem dados no buffer
                    if(!buff.Contains("+IPD"))  buff = "";

                    do{
                        buff += serial.ReadExisting();
                        if(buff.Length >= 1)
                        {        
                            
                            Console.Write("buffer incoming -> ");
                            Console.WriteLine(buff.Replace("\n", "<lf>").Replace("\r", "<cr>"));
                                               
                            //define uma resposta para a requisicao do aithinker
                            string deviceResponsePatern = "\\+IPD,(\\d+):(.*\r\n\r\n)";
                            MatchCollection deviceResponses = Regex.Matches(buff, deviceResponsePatern, RegexOptions.Singleline);
                            foreach (Match deviceResponse in deviceResponses)
                            {
                                if(deviceResponse.Success)
                                {
                                    int length = int.Parse(deviceResponse.Groups[1].Value);
                                    
                                    incoming += deviceResponse.Groups[2].Value;
                                    
                                    Console.Write("matches incoming -> ");
                                    Console.WriteLine(incoming);
                                    buff = "";
                                }                                 
                            }
                            
                            //incoming += buff;//.Replace("\r","").Replace("\n",""); 
                            timeout = 100;
                            if(buff.Contains("SEND FAIL") || buff.Contains("CLOSED") || buff.Contains("ERROR"))
                            {
                                if(_sendTry >= 10)
                                {
                                    Console.WriteLine("Socket Writing TimedOut");
                                    return null;
                                }
                                Thread.Sleep(timeoutPerTryes);
                                _sendTry ++;
                                goto _ressend;
                            }
                        }
                        else
                        {
                            timeout --;
                            Thread.Sleep(100);
                        }
                        if(timeout <= 0)    break;
                    } while(!Regex.Match(incoming, response,RegexOptions.Singleline).Success && !incoming.Contains("ERROR"));

                    Console.Write("incoming end -> ");
                    Console.WriteLine(incoming);

                    //retorna valor definido no patern requerido
                    Match rec = Regex.Match(incoming, response,RegexOptions.Singleline);
                    if(rec.Success) return rec.Value;
                    else return null;
                }
            }
            
            public override bool fininshContinuousSocket(int socketNumber = -1)
            {           

                executeRequest:            
                //limpa buffers
                serial.DiscardInBuffer();
                serial.DiscardOutBuffer();
                
                int tryes = 0;
                string incoming = "";

                disconect:
                //finaliza envio no ftp
                Console.WriteLine("Finalizando o Socket");
                Thread.Sleep(2500);
                serial.Write("+");
                Thread.Sleep(500);
                serial.Write("+");
                Thread.Sleep(500);
                serial.Write("+");
                Thread.Sleep(2500);
                serial.Write("\r\nat\r\n");
                incoming = "";

                try
                {
                    do{incoming += serial.ReadLine().Replace("\r","").Replace("\n",""); Console.WriteLine(incoming);} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));
                    if((!incoming.Contains("OK")) && (!incoming.Contains("DISCONNECT")) && (!incoming.Contains("ERROR"))){Console.WriteLine("retrying to disconect"); goto disconect;}
                    else    Console.WriteLine("Successfuly Disconected");
                }
                catch(Exception e)
                {
                    //verifica tentativas de desconexao
                    if(tryes > 3)   return false;
                    //incrementa a tentativa de desconexao
                    tryes ++;
                    goto disconect;
                }

                if(incoming.Contains("OK"))    return true;

                return false;
            }


        }

        //processa criptografia aes
        static byte[] EncryptStringToBytes_Aes(string plainText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (plainText == null && plainText.Length <= 0)
                throw new ArgumentNullException("plainText");
            if (Key == null && Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null && IV.Length <= 0)
                throw new ArgumentNullException("IV");
            byte[] encrypted;
            
            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                aesAlg.Mode = CipherMode.CBC;

                // Create an encryptor to perform the stream transform.
                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for encryption.
                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            //Write all data to the stream.
                            swEncrypt.Write(plainText);
                        }
                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            // Return the encrypted bytes from the memory stream.
            return encrypted;
        }

        static string DecryptStringFromBytes_Aes(byte[] cipherText, byte[] Key, byte[] IV)
        {
            // Check arguments.
            if (cipherText == null && cipherText.Length <= 0)
                throw new ArgumentNullException("cipherText");
            if (Key == null && Key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null && IV.Length <= 0)
                throw new ArgumentNullException("IV");

            // Declare the string used to hold
            // the decrypted text.
            string plaintext = null;

            // Create an Aes object
            // with the specified key and IV.
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = Key;
                aesAlg.IV = IV;
                aesAlg.Mode = CipherMode.CBC;

                // Create a decryptor to perform the stream transform.
                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                // Create the streams used for decryption.
                using (MemoryStream msDecrypt = new MemoryStream(cipherText))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            // Read the decrypted bytes from the decrypting stream
                            // and place them in a string.
                            plaintext = srDecrypt.ReadToEnd();
                        }
                    }
                }
            }

            return plaintext;
        }
    }      
}