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
using Newtonsoft.Json;

namespace OptProcessBiSenderService
{   
    //|96|{"date": "2021-03-15 12:40:44.663113", "status": true, "version": "1.0.10", "desc": "connected"}
    public class JsonIsConnected
    {
        public string date{ get; set; }
        public bool status { get; set; }
        public string version { get; set; }
        public string desc { get; set; }     //Dictionary<string, int>
    }

    //|100|{"date": "2021-03-15 12:23:41.387990", "status": true, "version": "1.0.10", "desc": "{'length': 0}"}
    public class JsonQueueLength
    {
        public string date{ get; set; }
        public bool status { get; set; }
        public string version { get; set; }
        public int desc { get; set; }
    }

    class Device{

        const string MODE_WIFI = "wifi";
        const string MODE_3G = "mobile";
        public SerialPort devicePort()
        {
            /// <summary>
            /// busca pelo device nas portas de comunicação
            /// </summary>
            /// <value></value>
            string portName = "COM";
            SerialPort sp = new SerialPort();
            int i=1;
            for(; i<=100; i++)
            {
                Console.WriteLine("tentando porta COM"+i.ToString());
                try
                {
                    sp.PortName = portName+i.ToString();
                    sp.ReadTimeout = 2000;
                    sp.BaudRate = 115200;
                    if(!sp.IsOpen) sp.Open();
                    sp.WriteLine("\r\n|26|{\"command\":\"identify()\"}\r\n");
                    string response = sp.ReadLine();
                    Console.WriteLine("Found Device");
                    Console.WriteLine(response);
                    sp.Close();

                    break;
                }
                catch(Exception e)
                {
                    //Console.WriteLine("exceção ao procurar device -> ");
                    //Console.WriteLine(e);
                }
                finally
                {

                    sp.ReadTimeout = 10000;
                    //verifica se porta esta aberta e fecha
                    if(sp.IsOpen) 
                        sp.Close();
                }
            }
            if((i>0) && (i<100)) return sp;
            else return null;
        }
        public bool setApnName(string apnName, SerialPort sp)
        {
            /// <summary>
            /// define apn para conexao mobile do dispositivo
            /// </summary>
            /// <value></value>
            try
            {
                string command = "{\"date\":\""+DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")+"\", \"data\" : \""+apnName+"\", \"command\":\"setapnname()\"}";
                if(!sp.IsOpen) sp.Open();
                sp.WriteLine("\r\n|"+(command.Length+2).ToString()+"|"+command+"\r\n");
                Console.WriteLine("|"+(command.Length+2).ToString()+"|"+command+"\r\n");
                string response = sp.ReadLine();
                Console.WriteLine(response);
                sp.Close();
                if(response.Contains("success"))    return true;
            }
            catch(Exception e)
            {
                Console.WriteLine("exceção ao procurar device -> ");
                Console.WriteLine(e);
                //Environment.Exit(1);
            }
            finally
            {
                sp.ReadTimeout = 10000;

                //verifica se porta esta aberta e fecha
                if(sp.IsOpen) 
                    sp.Close();
            }
            return false;
        }
        public bool setApnUser(string apnUser, SerialPort sp)
        {
            /// <summary>
            /// define user para conexao mobile do dispositivo
            /// </summary>
            /// <value></value>
            try
            {
                string command = "{\"date\":\""+DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")+"\", \"data\" : \""+apnUser+"\", \"command\":\"setapnuser()\"}";
                if(!sp.IsOpen) sp.Open();
                sp.WriteLine("\r\n|"+(command.Length+2).ToString()+"|"+command+"\r\n");
                Console.WriteLine("|"+(command.Length+2).ToString()+"|"+command+"\r\n");
                string response = sp.ReadLine();
                Console.WriteLine(response);
                sp.Close();
                if(response.Contains("success"))    return true;
            }
            catch(Exception e)
            {
                Console.WriteLine("exceção ao procurar device -> ");
                Console.WriteLine(e);
                //Environment.Exit(1);
            }
            finally
            {
                sp.ReadTimeout = 10000;

                //verifica se porta esta aberta e fecha
                if(sp.IsOpen) 
                    sp.Close();
            }
            return false;
        }
        public bool setApnPass(string apnPass, SerialPort sp)
        {
            /// <summary>
            /// define apn para conexao mobile do dispositivo
            /// </summary>
            /// <value></value>
            try
            {
                string command = "{\"date\":\""+DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")+"\", \"data\" : \""+apnPass+"\", \"command\":\"setapnpass()\"}";
                if(!sp.IsOpen) sp.Open();
                sp.WriteLine("\r\n|"+(command.Length+2).ToString()+"|"+command+"\r\n");
                Console.WriteLine("|"+(command.Length+2).ToString()+"|"+command+"\r\n");
                string response = sp.ReadLine();
                Console.WriteLine(response);
                sp.Close();
                if(response.Contains("success"))    return true;
            }
            catch(Exception e)
            {
                Console.WriteLine("exceção ao procurar device -> ");
                Console.WriteLine(e);
                //Environment.Exit(1);
            }
            finally
            {
                sp.ReadTimeout = 10000;

                //verifica se porta esta aberta e fecha
                if(sp.IsOpen) 
                    sp.Close();
            }
            return false;
        }
        public bool setWifiSSID(string ssid, SerialPort sp)
        {
            /// <summary>
            /// define ssid para conexao wifi do dispositivo
            /// </summary>
            /// <value></value>
            try
            {
                string command = "{\"date\":\""+DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")+"\", \"data\" : \""+ssid+"\", \"command\":\"setwifissid()\"}";
                if(!sp.IsOpen) sp.Open();
                sp.WriteLine("\r\n|"+(command.Length+2).ToString()+"|"+command+"\r\n");
                Console.WriteLine("|"+(command.Length+2).ToString()+"|"+command+"\r\n");
                string response = sp.ReadLine();
                Console.WriteLine(response);
                sp.Close();
                if(response.Contains("success"))    return true;
            }
            catch(Exception e)
            {
                Console.WriteLine("exceção ao procurar device -> ");
                Console.WriteLine(e);
                //Environment.Exit(1);
            }
            finally
            {
                sp.ReadTimeout = 10000;

                //verifica se porta esta aberta e fecha
                if(sp.IsOpen) 
                    sp.Close();
            }
            return false;
        }
        public bool setWifiPass(string password, SerialPort sp)
        {
            /// <summary>
            /// define password para conexao wifi do dispositivo
            /// </summary>
            /// <value></value>
            try
            {
                string command = "{\"date\":\""+DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")+"\", \"data\" : \""+password+"\", \"command\":\"setwifipwd()\"}";
                if(!sp.IsOpen) sp.Open();
                sp.WriteLine("\r\n|"+(command.Length+2).ToString()+"|"+command+"\r\n");
                Console.WriteLine("|"+(command.Length+2).ToString()+"|"+command+"\r\n");
                string response = sp.ReadLine();
                Console.WriteLine(response);
                sp.Close();
                if(response.Contains("success"))    return true;
            }
            catch(Exception e)
            {
                Console.WriteLine("exceção ao procurar device -> ");
                Console.WriteLine(e);
                //Environment.Exit(1);
            }
            finally
            {
                sp.ReadTimeout = 10000;

                //verifica se porta esta aberta e fecha
                if(sp.IsOpen) 
                    sp.Close();
            }
            return false;
        }
        public bool setWifiIp(string ipaddress, SerialPort sp)
        {
            /// <summary>
            /// define endereco ip para conexao do dispositivo
            /// </summary>
            /// <value></value>
            try
            {
                string command = "{\"date\":\""+DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")+"\", \"data\" : \""+ipaddress+"\", \"command\":\"setip()\"}";
                if(!sp.IsOpen) sp.Open();
                sp.WriteLine("\r\n|"+(command.Length+2).ToString()+"|"+command+"\r\n");
                Console.WriteLine("|"+(command.Length+2).ToString()+"|"+command+"\r\n");
                string response = sp.ReadLine();
                Console.WriteLine(response);
                sp.Close();
                if(response.Contains("success"))    return true;
            }
            catch(Exception e)
            {
                Console.WriteLine("exceção ao procurar device -> ");
                Console.WriteLine(e);
                //Environment.Exit(1);
            }
            finally
            {
                sp.ReadTimeout = 10000;

                //verifica se porta esta aberta e fecha
                if(sp.IsOpen) 
                    sp.Close();
            }
            return false;
        }
        public bool setWifiNetmask(string netmask, SerialPort sp)
        {
            /// <summary>
            /// define mascara de subrede para conexao do dispositivo
            /// </summary>
            /// <value></value>
            try
            {
                string command = "{\"date\":\""+DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")+"\", \"data\" : \""+netmask+"\", \"command\":\"setnetmask()\"}";
                if(!sp.IsOpen) sp.Open();
                sp.WriteLine("\r\n|"+(command.Length+2).ToString()+"|"+command+"\r\n");
                Console.WriteLine("|"+(command.Length+2).ToString()+"|"+command+"\r\n");
                string response = sp.ReadLine();
                Console.WriteLine(response);
                sp.Close();
                if(response.Contains("success"))    return true;
            }
            catch(Exception e)
            {
                Console.WriteLine("exceção ao procurar device -> ");
                Console.WriteLine(e);
                //Environment.Exit(1);
            }
            finally
            {
                sp.ReadTimeout = 10000;

                //verifica se porta esta aberta e fecha
                if(sp.IsOpen) 
                    sp.Close();
            }
            return false;
        }
        public bool setWifiGateway(string gateway, SerialPort sp)
        {
            /// <summary>
            /// define gateway para conexao do dispositivo
            /// </summary>
            /// <value></value>
            try
            {
                string command = "{\"date\":\""+DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")+"\", \"data\" : \""+gateway+"\", \"command\":\"setgateway()\"}";
                if(!sp.IsOpen) sp.Open();
                sp.WriteLine("\r\n|"+(command.Length+2).ToString()+"|"+command+"\r\n");
                Console.WriteLine("|"+(command.Length+2).ToString()+"|"+command+"\r\n");
                string response = sp.ReadLine();
                Console.WriteLine(response);
                sp.Close();
                if(response.Contains("success"))    return true;
            }
            catch(Exception e)
            {
                Console.WriteLine("exceção ao procurar device -> ");
                Console.WriteLine(e);
                //Environment.Exit(1);
            }
            finally
            {
                sp.ReadTimeout = 10000;

                //verifica se porta esta aberta e fecha
                if(sp.IsOpen) 
                    sp.Close();
            }
            return false;
        }
        public bool setWifiDns(string dns, SerialPort sp)
        {
            /// <summary>
            /// define dns para conexao do dispositivo
            /// </summary>
            /// <value></value>
            try
            {
                string command = "{\"date\":\""+DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")+"\", \"data\" : \""+dns+"\", \"command\":\"setdns()\"}";
                if(!sp.IsOpen) sp.Open();
                sp.WriteLine("\r\n|"+(command.Length+2).ToString()+"|"+command+"\r\n");
                Console.WriteLine("|"+(command.Length+2).ToString()+"|"+command+"\r\n");
                string response = sp.ReadLine();
                Console.WriteLine(response);
                sp.Close();
                if(response.Contains("success"))    return true;
            }
            catch(Exception e)
            {
                Console.WriteLine("exceção ao procurar device -> ");
                Console.WriteLine(e);
                //Environment.Exit(1);
            }
            finally
            {
                sp.ReadTimeout = 10000;

                //verifica se porta esta aberta e fecha
                if(sp.IsOpen) 
                    sp.Close();
            }
            return false;
        }
        public bool setProjectName(string project, SerialPort sp)
        {
            /// <summary>
            /// define o nome do projeto que o device esta enviando
            /// </summary>
            /// <value></value>
            try
            {
                string command = "{\"date\":\""+DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")+"\", \"data\" : \""+project+"\", \"command\":\"setproject()\"}";
                if(!sp.IsOpen) sp.Open();
                sp.WriteLine("\r\n|"+(command.Length+2).ToString()+"|"+command+"\r\n");
                Console.WriteLine("|"+(command.Length+2).ToString()+"|"+command+"\r\n");
                string response = sp.ReadLine();
                Console.WriteLine(response);
                sp.Close();
                if(response.Contains("success"))    return true;
            }
            catch(Exception e)
            {
                Console.WriteLine("exceção ao procurar device -> ");
                Console.WriteLine(e);
                //Environment.Exit(1);
            }
            finally
            {
                sp.ReadTimeout = 10000;

                //verifica se porta esta aberta e fecha
                if(sp.IsOpen) 
                    sp.Close();
            }
            return false;
        }
        public bool setMachineId(string machineid, SerialPort sp)
        {
            /// <summary>
            /// define o id da maquina no qual o device esta instalado
            /// </summary>
            /// <value></value>
            try
            {
                string command = "{\"date\":\""+DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")+"\", \"data\" : \""+machineid+"\", \"command\":\"setmachineid()\"}";
                if(!sp.IsOpen) sp.Open();
                sp.WriteLine("\r\n|"+(command.Length+2).ToString()+"|"+command+"\r\n");
                Console.WriteLine("|"+(command.Length+2).ToString()+"|"+command+"\r\n");
                string response = sp.ReadLine();
                Console.WriteLine(response);
                sp.Close();
                if(response.Contains("success"))    return true;
            }
            catch(Exception e)
            {
                Console.WriteLine("exceção ao procurar device -> ");
                Console.WriteLine(e);
                //Environment.Exit(1);
            }
            finally
            {
                sp.ReadTimeout = 10000;

                //verifica se porta esta aberta e fecha
                if(sp.IsOpen) 
                    sp.Close();
            }
            return false;
        }        
        public bool setCloudHost(string host, SerialPort sp)
        {
            /// <summary>
            /// define o endereço para a qual o device vai mandar os dados
            /// </summary>
            /// <value></value>
            try
            {
                string command = "{\"date\":\""+DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")+"\", \"data\" : \""+host+"\", \"command\":\"sethostname()\"}";
                if(!sp.IsOpen) sp.Open();
                sp.WriteLine("\r\n|"+(command.Length+2).ToString()+"|"+command+"\r\n");
                Console.WriteLine("|"+(command.Length+2).ToString()+"|"+command+"\r\n");
                string response = sp.ReadLine();
                Console.WriteLine(response);
                sp.Close();
                if(response.Contains("success"))    return true;
            }
            catch(Exception e)
            {
                Console.WriteLine("exceção ao procurar device -> ");
                Console.WriteLine(e);
                //Environment.Exit(1);
            }
            finally
            {
                sp.ReadTimeout = 10000;

                //verifica se porta esta aberta e fecha
                if(sp.IsOpen) 
                    sp.Close();
            }
            return false;
        }
        public bool setCloudPort(string port, SerialPort sp)
        {
            /// <summary>
            /// define a porta para a qual o device vai mandar os dados
            /// </summary>
            /// <value></value>
            try
            {
                string command = "{\"date\":\""+DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")+"\", \"data\" : \""+port+"\", \"command\":\"sethostport()\"}";
                if(!sp.IsOpen) sp.Open();
                sp.WriteLine("\r\n|"+(command.Length+2).ToString()+"|"+command+"\r\n");
                Console.WriteLine("|"+(command.Length+2).ToString()+"|"+command+"\r\n");
                string response = sp.ReadLine();
                Console.WriteLine(response);
                sp.Close();
                if(response.Contains("success"))    return true;
            }
            catch(Exception e)
            {
                Console.WriteLine("exceção ao procurar device -> ");
                Console.WriteLine(e);
                //Environment.Exit(1);
            }
            finally
            {
                sp.ReadTimeout = 10000;

                //verifica se porta esta aberta e fecha
                if(sp.IsOpen) 
                    sp.Close();
            }
            return false;
        }
        //envia o nome das tags para a nuvem na seguinte estrutura
        //
        //|144|{"date":"2021-02-11 16:33:15", "data" : ["{tag name}:{tag address}:{tag type}","{tag name}:{tag address}:{tag type}"], "command":"gettagid()"}
        //
        public string setTagId(List<string> tagsList, SerialPort sp)
        {
            /// <summary>
            /// envia header da Tag para o device na seguinte estrutura
            /// |144|{"date":"2021-02-11 16:33:15", "data" : ["{tag name}:{tag address}:{tag type}","{tag name}:{tag address}:{tag type}"], "command":"gettagid()"}
            /// </summary>
            /// <value></value>
            try
            {
                sp.ReadTimeout = 60000;

                string command = "{\"date\":\""+DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")+"\", \"data\" : "+JsonConvert.SerializeObject(tagsList)+", \"command\":\"gettagid()\"}";
                if(!sp.IsOpen) sp.Open();

                sendSerial("\r\n|"+(command.Length+2).ToString()+"|"+command+"\r\n", sp);

                //sp.WriteLine("\r\n|"+(command.Length+2).ToString()+"|"+command+"\r\n");
                //Console.WriteLine("|"+(command.Length+2).ToString()+"|"+command+"\r\n");
                string response = sp.ReadLine();
                sp.Close();

                sp.ReadTimeout = 10000;

                return response;
            }
            catch(Exception e)
            {
                Console.WriteLine("exceção ao procurar device -> ");
                Console.WriteLine(e);
                //Environment.Exit(1);
            }
            finally
            {
                sp.ReadTimeout = 10000;

                //verifica se porta esta aberta e fecha
                if(sp.IsOpen) 
                    sp.Close();
            }
            return null;
        }
        //envia o nome das tags para a nuvem na seguinte estrutura
        //
        //|86|{"date":"2021-02-11 16:33:15", "data" : {"itfunfs":"yep"}, "command":"settagdata()"}
        //
        public string setTagData(string readdatetime, List<Dictionary<int, string>> tagsData, SerialPort sp)
        {
            /// <summary>
            /// envia dados da Tag para o device na seguinte estrutura
            /// |86|{"date":"2021-02-11 16:33:15", "data" : {"itfunfs":"yep"}, "command":"settagdata()"}
            /// </summary>
            /// <value></value>
            try
            {
                sp.ReadTimeout = 60000;
                
                string command = "{\"date\":\""+readdatetime+"\", \"data\" : "+JsonConvert.SerializeObject(tagsData)+", \"command\":\"settagdata()\"}";
                if(!sp.IsOpen) sp.Open();

                sendSerial("\r\n|"+(command.Length+2).ToString()+"|"+command+"\r\n", sp);

                Console.WriteLine("|"+(command.Length+2).ToString()+"|"+command+"\r\n");
                string response = sp.ReadLine();
                Console.WriteLine(response);
                sp.Close();

                sp.ReadTimeout = 10000;

                return response;
            }
            catch(Exception e)
            {
                Console.WriteLine("exceção ao procurar device -> ");
                Console.WriteLine(e);
                //Environment.Exit(1);
            }
            finally
            {
                sp.ReadTimeout = 10000;

                //verifica se porta esta aberta e fecha
                if(sp.IsOpen) 
                    sp.Close();
            }
            return null;
        }
        public bool getSignalLength(SerialPort sp)
        {
            /// <summary>
            /// Verifica Sinal do Dispositivo
            /// </summary>
            /// <value></value>
            try
            {
                sp.ReadTimeout = 10000;

                string command = "{\"date\":\""+DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")+"\", \"command\":\"getsignal()\"}";
                if(!sp.IsOpen) sp.Open();
                sp.WriteLine("\r\n|"+(command.Length+2).ToString()+"|"+command+"\r\n");
                Console.WriteLine("|"+(command.Length+2).ToString()+"|"+command+"\r\n");
                string response = sp.ReadLine();
                Console.WriteLine(response);
                sp.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine("exceção ao procurar device -> ");
                Console.WriteLine(e);
                //Environment.Exit(1);
            }
            finally
            {
                //verifica se porta esta aberta e fecha
                if(sp.IsOpen) 
                    sp.Close();
            }
            return true;
        }
        public bool getConectionStatus(SerialPort sp)
        {
            /// <summary>
            /// Verifica se o device esta conectado a rede retornando true caso esteja conectado
            /// </summary>
            /// <value></value>
            try
            {
                sp.ReadTimeout = 10000;

                string command = "{\"date\":\""+DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")+"\", \"command\":\"getconnectionstatus()\"}";
                if(!sp.IsOpen) sp.Open();
                sp.WriteLine("\r\n|"+(command.Length+2).ToString()+"|"+command+"\r\n");
                Console.WriteLine("|"+(command.Length+2).ToString()+"|"+command+"\r\n");
                string response = sp.ReadLine();
                Console.WriteLine(response);
                sp.Close();

                response = Regex.Replace(response, @"\|\d+\|", "");
                JsonIsConnected status = JsonConvert.DeserializeObject<JsonIsConnected>(response);
                
                if(status.desc == "connected")  return true;
            }
            catch(Exception e)
            {
                Console.WriteLine("exceção ao procurar device -> ");
                Console.WriteLine(e);
                //Environment.Exit(1);
            }
            finally
            {
                //verifica se porta esta aberta e fecha
                if(sp.IsOpen) 
                    sp.Close();
            }
            return false;
        }
        public int getQueueLength(SerialPort sp)
        {
            /// <summary>
            /// Recupera o tamanho atual da fila de envio
            /// </summary>
            /// <value></value>
            try
            {
                sp.ReadTimeout = 10000;

                string command = "{\"date\":\""+DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")+"\", \"command\":\"getqueue()\"}";
                if(!sp.IsOpen) sp.Open();
                sp.WriteLine("\r\n|"+(command.Length+2).ToString()+"|"+command+"\r\n");
                Console.WriteLine("|"+(command.Length+2).ToString()+"|"+command+"\r\n");
                string response = sp.ReadLine();
                Console.WriteLine(response);
                sp.Close();

                response = Regex.Replace(response, @"\|\d+\|", "");
                var jsonResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonQueueLength>(response);
                //deve verificar desc.length por um numero
                return jsonResponse.desc;
            }
            catch(Exception e)
            {
                Console.WriteLine("exceção ao procurar device -> ");
                Console.WriteLine(e);
                //Environment.Exit(1);
            }
            finally
            {
                //verifica se porta esta aberta e fecha
                if(sp.IsOpen) 
                    sp.Close();
            }
            return 0;
        }
        public bool cleanQueue(SerialPort sp)
        {
            /// <summary>
            /// Limpa a Fila de Envio
            /// </summary>
            /// <value></value>
            try
            {
                sp.ReadTimeout = 10000;

                string command = "{\"date\":\""+DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")+"\", \"command\":\"cleanqueue()\"}";
                if(!sp.IsOpen) sp.Open();
                sp.WriteLine("\r\n|"+(command.Length+2).ToString()+"|"+command+"\r\n");
                Console.WriteLine("|"+(command.Length+2).ToString()+"|"+command+"\r\n");
                string response = sp.ReadLine();
                Console.WriteLine(response);
                sp.Close();
                if(response.Contains("success"))    return true;
            }
            catch(Exception e)
            {
                Console.WriteLine("exceção ao procurar device -> ");
                Console.WriteLine(e);
                //Environment.Exit(1);
            }
            finally
            {
                //verifica se porta esta aberta e fecha
                if(sp.IsOpen) 
                    sp.Close();
            }
            return false;
        }
        public bool getIpAddress(SerialPort sp)
        {
            /// <summary>
            /// Recupera o endereco ip do device
            /// </summary>
            /// <value></value>
            try
            {
                sp.ReadTimeout = 10000;

                string command = "{\"date\":\""+DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")+"\", \"command\":\"getip()\"}";
                if(!sp.IsOpen) sp.Open();
                sp.WriteLine("\r\n|"+(command.Length+2).ToString()+"|"+command+"\r\n");
                Console.WriteLine("|"+(command.Length+2).ToString()+"|"+command+"\r\n");
                string response = sp.ReadLine();
                Console.WriteLine(response);
                sp.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine("exceção ao procurar device -> ");
                Console.WriteLine(e);
                //Environment.Exit(1);
            }
            finally
            {
                //verifica se porta esta aberta e fecha
                if(sp.IsOpen) 
                    sp.Close();
            }
            return true;
        }
        public bool getPingTime(SerialPort sp)
        {
            /// <summary>
            /// Verifica o tempo de ping para o servidor e para o dns
            /// </summary>
            /// <value></value>
            try
            {
                sp.ReadTimeout = 10000;

                string command = "{\"date\":\""+DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")+"\", \"command\":\"getpinginfo()\"}";
                if(!sp.IsOpen) sp.Open();
                sp.WriteLine("\r\n|"+(command.Length+2).ToString()+"|"+command+"\r\n");
                Console.WriteLine("|"+(command.Length+2).ToString()+"|"+command+"\r\n");
                string response = sp.ReadLine();
                Console.WriteLine(response);
                sp.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine("exceção ao procurar device -> ");
                Console.WriteLine(e);
                //Environment.Exit(1);
            }
            finally
            {
                //verifica se porta esta aberta e fecha
                if(sp.IsOpen) 
                    sp.Close();
            }
            return true;
        }
        public bool setMode(string mode, SerialPort sp)
        {
            /// <summary>
            /// Define modo de operação
            /// </summary>
            /// <value>Modos Disponiveis : wifi, mobile</value>
            try
            {
                sp.ReadTimeout = 10000;

                string command = "{\"date\":\""+DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")+"\", \"data\": \""+mode+"\",\"command\":\"setmode()\"}";
                if(!sp.IsOpen) sp.Open();
                sp.WriteLine("\r\n|"+(command.Length+2).ToString()+"|"+command+"\r\n");
                Console.WriteLine("|"+(command.Length+2).ToString()+"|"+command+"\r\n");
                string response = sp.ReadLine();
                Console.WriteLine(response);
                sp.Close();
                if(response.Contains("success"))    return true;
            }
            catch(Exception e)
            {
                Console.WriteLine("exceção ao procurar device -> ");
                Console.WriteLine(e);
                //Environment.Exit(1);
            }
            finally
            {
                //verifica se porta esta aberta e fecha
                if(sp.IsOpen) 
                    sp.Close();
            }
            return false;
        }
        public bool getLastStartup(SerialPort sp)
        {
            /// <summary>
            /// Recupera a data da ultima inicialização
            /// </summary>
            /// <value></value>
            try
            {
                sp.ReadTimeout = 10000;

                string command = "{\"date\":\""+DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss")+"\", \"command\":\"laststart()\"}";
                if(!sp.IsOpen) sp.Open();
                sp.WriteLine("\r\n|"+(command.Length+2).ToString()+"|"+command+"\r\n");
                Console.WriteLine("|"+(command.Length+2).ToString()+"|"+command+"\r\n");
                string response = sp.ReadLine();
                Console.WriteLine(response);
                sp.Close();
            }
            catch(Exception e)
            {
                Console.WriteLine("exceção ao procurar device -> ");
                Console.WriteLine(e);
                //Environment.Exit(1);
            }
            finally
            {
                //verifica se porta esta aberta e fecha
                if(sp.IsOpen) 
                    sp.Close();
            }
            return true;
        }

        public void sendSerial(string data, SerialPort ser)
        {
            int len = data.Length;
            for(int i=0; i<=len; i+=1000)
            {
                ser.Write(data.Substring(i,len-i > 1000 ? 1000 : len - i));
            }

            ser.Write("\n");
        }

    }
}