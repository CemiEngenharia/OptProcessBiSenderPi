using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TitaniumAS.Opc.Client.Da;
using TitaniumAS.Opc.Client.Common;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Text.RegularExpressions;
using System.Management;
using System.Net;
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
//using System.Web.Helpers;
using Newtonsoft.Json;
using Microsoft.Management.Infrastructure;

namespace OptProcessBiSenderService
{
    public class JsonWebResponse
    {
        public string date{ get; set; }
        public bool status { get; set; }
        public string version { get; set; }
        public Dictionary<string, int> desc { get; set; }     //Dictionary<string, int>
    }
    public class JsonWebErrorResponse
    {
        public string date{ get; set; }
        public bool status { get; set; }
        public string version { get; set; }
        public string desc { get; set; }     //Dictionary<string, int>
    }

    public class Worker : BackgroundService
    {
        static string currentVersion = "2.0.0";
        static string configFile = "optsync.cfg";
        static Dictionary<string, string> configuration;
        static new Dictionary<string, Dictionary<string, object>> opcTags;
        static Dictionary<string,string> NetworkHosts;
        static List<string> opcServers;
        static opcManagement opcMan;
        static Thread opcReadingThread;
        static Dictionary<string, string> deviceInfo;
        static SerialPort _infoSerialPort;
        static SerialPort _comSerialPort;
        static bool stopAllThreads;
        static bool reloadThreadConfig;
        static int sockNro;
        static Stack<string> logQueue;
        static int deviceType;
        static int _uartSpeed = 115200;

        static Device optdevice;
        
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        public Worker()
        {
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //			writelog("Inicando Serviço de Aplicação");
                __retry:
                try{
                    runProcess(stoppingToken);
                }
                catch(Exception e)
                {
                    writelog("_________________________Falha Geral_____________________________", 3);
                    writelog(e, 3);
                    goto __retry;
                }
            }

            			writelog("cancelation was Requested by service", 3);
        }
        
        static void runProcess(CancellationToken stoppingToken)
        {
            //variavel para armazenamento do device location
            string deviceLocation = "";

            //Console.Write("Start Running Dir");            
            //Console.Write(Directory.GetCurrentDirectory());
            Console.Write("Current Running Dir");
            Directory.SetCurrentDirectory(System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase).Replace(@"file:\", ""));
            Console.Write(Directory.GetCurrentDirectory());
            
            //caso a thread tenha sido cancelada
            if(stoppingToken.IsCancellationRequested)   return;

            //			writelog(Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)));
            //define localizacao do arquivo de configuração
            if(!Directory.Exists(Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)) + "ProgramData\\CEMI"))
            {
                Directory.CreateDirectory(Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)) + "ProgramData\\CEMI");
            }
            
            if(!Directory.Exists(Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)) + "ProgramData\\CEMI\\OptBiSender"))
            {
                Directory.CreateDirectory(Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)) + "ProgramData\\CEMI\\OptBiSender");
            }
            
            configFile = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)) + "ProgramData\\CEMI\\OptBiSender\\optsync.cfg";


            //define o opcManagement
            opcMan = new opcManagement();
            Dictionary<string,string> tagsOnlineInfo = new Dictionary<string, string>();
            List<string> tags = new List<string>();

            //instancia device
            optdevice = new Device();

            //inicializa variaveis globais
            configuration = new Dictionary<string, string>();
            opcTags = new Dictionary<string, Dictionary<string, object>>();


            //carrega configuração do device

            			writelog("Carregando configuração");
            //inicializa a configuração
            while(loadConfiguration() == 0)
            {
                Thread.Sleep(5000);
                if(stoppingToken.IsCancellationRequested)   return;
            }
            
            //trata qualquer excecao nao tratada no resto do app
            int findTrys = 0;
            __findDevice:

            //caso a thread tenha sido cancelada
            if(stoppingToken.IsCancellationRequested)   return;

            try{
               
                //procura dispositivo nas portas serial
                _comSerialPort = optdevice.devicePort();

                if(_comSerialPort == null) { 			writelog("Device Nao Encontrado", 2); Thread.Sleep(2000); goto __findDevice;}

                			writelog(_comSerialPort);

                			writelog("portas Abertas");
                        
                //Pega Dados de Informação do Device como Mac e Demais Dados Importantes
                			writelog("lendo dados do dispositivo");
                
                //define funções comuns para os devices como leitura de dados internos
                DeviceCommon commonFunctions;
                if(deviceType == 1) commonFunctions = new Common();
                else commonFunctions = new espressific.Common();
                        
                //verifica se as tags sao tags
                bool isTag = false;
                foreach(string tagsKeys in opcTags.Keys)    if((bool) opcTags[tagsKeys]["active"] == true)   isTag = true;
                if(isTag == false)
                {
                    			writelog("Tags Nao Configuradas, Acesse a Pagina de Configuração", 2);
                }

                __setDeviceConfig:
                //caso a thread tenha sido cancelada
                if(stoppingToken.IsCancellationRequested)   return;

                //configura dados da apn
                optdevice.setApnName(configuration["apn"], _comSerialPort);
                optdevice.setApnUser(configuration["apnuser"], _comSerialPort);
                optdevice.setApnPass(configuration["apnpass"], _comSerialPort);

                //configura dados do wifi
                optdevice.setWifiSSID(configuration["ssid"], _comSerialPort);
                			writelog("Aguardando 1 minuto para reestabelecer a rede");
                //Thread.Sleep(5000);
                optdevice.setWifiPass(configuration["password"], _comSerialPort);
                			writelog("Aguardando 1 minuto para reestabelecer a rede");
                //Thread.Sleep(5000);
                optdevice.setWifiIp(configuration["ip"], _comSerialPort);
                			writelog("Aguardando 1 minuto para reestabelecer a rede");
                //Thread.Sleep(5000);
                optdevice.setWifiGateway(configuration["gateway"], _comSerialPort);
                			writelog("Aguardando 1 minuto para reestabelecer a rede");
                //Thread.Sleep(5000);
                optdevice.setWifiNetmask(configuration["netmask"], _comSerialPort);
                			writelog("Aguardando 1 minuto para reestabelecer a rede");
                //Thread.Sleep(5000);

                
                optdevice.setCloudHost(configuration["server"], _comSerialPort);
                optdevice.setCloudPort(configuration["serverport"], _comSerialPort);

                //faz conexao e verifica se esta conectado
                optdevice.setMode(configuration["mode"], _comSerialPort);

                //verifica conexao e inicia os procedimentos
                __verifyNetwork:
                Thread.Sleep(1000);
                if(optdevice.getConectionStatus(_comSerialPort))
                { 
                    //caso a thread tenha sido cancelada
                    if(stoppingToken.IsCancellationRequested)   return; 
                    			writelog("Dispositivo conectado com sucesso");
                }
                else
                {
                    if(stoppingToken.IsCancellationRequested)   return; 
                    			writelog("Dispositivo nao esta conectado a rede", 2);
                    goto __verifyNetwork;
                }

                int pdpStartTrys = 3;
                _startpdp:                        
                //caso a thread tenha sido cancelada
                if(stoppingToken.IsCancellationRequested)   return; 

                //flag para paras threads
                stopAllThreads = false;
                reloadThreadConfig = true;

                			writelog("Iniciando conexao");

                //inicializa processo de envio dos dados
/*                
                //pega posicao global do device
                if(deviceType == 1) deviceLocation = getLocation(_infoSerialPort);

                //converte para base 64
                deviceLocation = Convert.ToBase64String(Encoding.ASCII.GetBytes(deviceLocation));
*/
                //Define dados importantes no device
                FingerPrint fp = new FingerPrint();

                //optdevice.setMachineId(getComputerId(), _comSerialPort);
                optdevice.setMachineId(fp.Value(), _comSerialPort);
                optdevice.setProjectName(configuration["projectalias"], _comSerialPort);

                /**********************************Le Tags no Servidor OPC******************************************/

                //cria uma flag para os dados sendo enviados
                bool alreadySent = true;

                //define um callback de leitura para teste
                void completed(OpcDaItemValue[] values)
                {
                    alreadySent = false;

                    List<Dictionary<string, string>> dataToWrite = new List<Dictionary<string, string>>();
                    int dataToWriteLen = 0;
                    //pega data da ultima leitura
                    string readedDateTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    
                    /*************************************************/
                    try{
                        			writelog("Leitura Completa");    
                        //Console.Write(("Leitura Completa"));
                        
                        if(!reloadThreadConfig)
                        {
                            //navega resultado das tags
                            writelog("values.Length -> " + values.Length.ToString());    
                            //Console.Write((values.Length));

                            //recupera timestamp atual
                            Int32 unixTimestamp = (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
                            for(int i=0; i<values.Length; i++)
                            {
                                //writelog(i);    
                                //Console.Write((i));
                                OpcDaItemValue val = values[i];
                                
                                
                                Console.Write(val.Item.ItemId);
                                Console.Write(" -- ");
                                Console.Write(val.Value == null? "null Value" : val.Value);
                                Console.Write(val.Value == null? "null Type" : val.Value.GetType().ToString());
                                Console.Write(val.Value);
                                Console.Write(" -- ");
                                //Console.Write((int) val.Quality);
                                //Console.Write(" -- ");
                                //Console.Write(val.Timestamp);
                                //Console.Write(" -- ");
                                //			writelog((int) val.Error);    
                                //Console.Write(((int) val.Error));
                                //Console.Write(" -- ");
                                //			writelog(val.Value.GetType());    
                                //Console.Write((val.Value.GetType()));
                                
                                dataToWriteLen ++;

                                //cria a string para escrever no tcp

                                string formatedValue = "";

                                try{
                                    if(val.Value != null)
                                    {
                                        if(val.Value.GetType().ToString().Contains("[]"))
                                        {
                                            formatedValue += "[";

                                            if(val.Value.GetType().ToString().Contains("Double"))
                                            {    
                                                foreach(double v in (double[]) val.Value)
                                                    formatedValue += formatedValue.Length >= 2? ","+v.ToString(): v.ToString();
                                            }   
                                            else if(val.Value.GetType().ToString().Contains("Single"))
                                            {    
                                                foreach(float v in (float[]) val.Value)
                                                    formatedValue += formatedValue.Length >= 2? ","+v.ToString(): v.ToString();
                                            }
                                            else if(val.Value.GetType().ToString().Contains("Int16"))
                                            {    
                                                foreach(Int16 v in (Int16[]) val.Value)
                                                    formatedValue += formatedValue.Length >= 2? ","+v.ToString(): v.ToString();
                                            }
                                            else if(val.Value.GetType().ToString().Contains("Int32"))
                                            {    
                                                foreach(Int32 v in (Int32[]) val.Value)
                                                    formatedValue += formatedValue.Length >= 2? ","+v.ToString(): v.ToString();
                                            }
                                            else if(val.Value.GetType().ToString().Contains("Int64"))
                                            {    
                                                foreach(Int64 v in (Int64[]) val.Value)
                                                    formatedValue += formatedValue.Length >= 2? ","+v.ToString(): v.ToString();
                                            }
                                            else if(val.Value.GetType().ToString().Contains("Int"))
                                            {    
                                                foreach(int v in (int[]) val.Value)
                                                    formatedValue += formatedValue.Length >= 2? ","+v.ToString(): v.ToString();
                                            }
                                            else if(val.Value.GetType().ToString().Contains("String"))
                                            {    
                                                foreach(string v in (string[]) val.Value)
                                                    formatedValue += formatedValue.Length >= 2? ","+v.ToString(): v.ToString();
                                            }
                                            else if(val.Value.GetType().ToString().Contains("Byte"))
                                            {    
                                                foreach(byte v in (byte[]) val.Value)
                                                    formatedValue += formatedValue.Length >= 2? ","+v.ToString(): v.ToString();
                                            }                            
                                            else if(val.Value.GetType().ToString().Contains("SByte"))
                                            {    
                                                foreach(sbyte v in (sbyte[]) val.Value)
                                                    formatedValue += formatedValue.Length >= 2? ","+v.ToString(): v.ToString();
                                            }         
                                            formatedValue += "]";
                                        }
                                        else
                                        {
                                            formatedValue = val.Value.ToString();
                                        }
                                    }
                                    else
                                    {
                                        formatedValue = "null";
                                    }
                                }
                                catch(Exception e)
                                {
                                    			writelog("------ Problema ao pegar dado da tag linha 345", 3);
                                    			writelog(e.ToString(), 3);
                                    
                                }

                                try
                                {
                                    //analizar retirada do tagonline info, usando so a id no matrikon
                                    
                                    //dataToWrite += dataToWrite.Length >= 1? "^"+tagsOnlineInfo[val.Item.ItemId] +"`"+formatedValue+"`"+((int) val.Quality).ToString() : tagsOnlineInfo[val.Item.ItemId] +"`"+formatedValue+"`"+((int) val.Quality).ToString();
                                    Dictionary<string, string> tmp = new Dictionary<string, string>();
                                    //tmp[int.Parse(tagsOnlineInfo[val.Item.ItemId])] = formatedValue+"`"+((int) val.Quality).ToString();
                                    tmp[val.Item.ItemId] = formatedValue+"`"+((int) val.Quality).ToString();
                                    dataToWrite.Add(tmp);
                                    tmp = null;
                                }
                                catch(Exception e)
                                {
                                    			writelog("------ Problema ao add tag para dataToWrite linha 409", 3);
                                    			writelog(e.ToString(), 3);
                                }
                                //			writelog("___________________________________");    
                                //Console.Write(("___________________________________"));
                                //Console.Write("now -> ");
                                //Console.Write(DateTime.UtcNow);
                                //Console.Write(" timestamp -> ");
                                //Console.Write(val.Timestamp);
                                //Console.Write(" tagValue -> ");
                                //Console.Write(formatedValue);
                                
                                //Console.Write(" dataToWriteLen -> ");
                                //			writelog(dataToWriteLen);    
                                //Console.Write((dataToWriteLen));

                                if(dataToWriteLen == int.Parse(configuration["tagsperrequest"]))
                                {                                        

                                    //			writelog("___________________________________");    
                                    //Console.Write(("___________________________________"));
                                    //Console.Write(" unixTimestamp -> ");
                                    //Console.Write(unixTimestamp);

                                    //			writelog("dataToWrite ");    
                                    //Console.Write(("dataToWrite "));
                                    //			writelog(dataToWrite);    
                                    //Console.Write((dataToWrite));
                                    //			writelog("___________________________________");    
                                    //Console.Write(("___________________________________"));
                                    

                                    			writelog(JsonConvert.SerializeObject(dataToWrite));

                                    int trys = 0;

                                    _rewrite:                                
                                    //caso a thread tenha sido cancelada
                                    if(stoppingToken.IsCancellationRequested)   return;

                                    string res = optdevice.setTagData(readedDateTime, dataToWrite, _comSerialPort);
//                                  string res = cp.writeContinuousSocket("{\"type\": \"writevalue\", \"data\" : \""+unixTimestamp+"|"+dataToWrite+"\"}\r\n\r\n", ".*(ACK).*", response["token"], int.Parse(configuration["timeout"]), sockNro);

                                    //			writelog("Wrote");    
                                    //Console.Write(("Wrote"));
                                    //			writelog(res);    
                                    //Console.Write((res));

                                    while(res == null)
                                    {
                                        			writelog("null response received from server on post data", 2);
                                        trys ++;
                                        if(trys >= 3)
                                        {
                                            //Mata todas as threads                                
                                            stopAllThreads = true;
                                            alreadySent = true;
//                                                cp.fininshContinuousSocket();
                                            return;
                                        }
                                        //Console.Write(("Tentando novamente"));
                                        //espera 2 segundos
                                        Thread.Sleep(2000);
                                        goto _rewrite;
                                    }

                                    //reseta marcadores
                                    dataToWrite = new List<Dictionary<string, string>>();
                                    dataToWriteLen = 0;
                                }
                            }
                            
                            //wroteCount;
                            //escreve dados remanecentes
                            if(dataToWriteLen > 0)
                            {
                                //			writelog("___________________________________");    
                                //Console.Write(("___________________________________"));
                                //Console.Write(" unixTimestamp -> ");
                                //Console.Write(unixTimestamp);

                                //			writelog("dataToWrite ");    
                                //Console.Write(("dataToWrite "));
                                //			writelog(dataToWrite);    
                                //Console.Write((dataToWrite));
                                //			writelog("___________________________________");    
                                //Console.Write(("___________________________________"));

                                
                                int trys = 0;

                                _rewrite:                                
                                //caso a thread tenha sido cancelada
                                if(stoppingToken.IsCancellationRequested)   return;
                                
                                //string res = cp.writeContinuousSocket(unixTimestamp+"|"+dataToWrite+"\r\n\r\n", ".*(ACK).*");
//                                    string res = cp.writeContinuousSocket("{\"type\": \"writevalue\", \"data\" : \""+unixTimestamp+"|"+dataToWrite+"\"}\r\n\r\n", ".*(ACK).*", response["token"], int.Parse(configuration["timeout"]), sockNro);
                                
                                string res = optdevice.setTagData(readedDateTime, dataToWrite, _comSerialPort);

                                //			writelog("Wrote");    
                                //Console.Write(("Wrote"));
                                //			writelog(res);    
                                //Console.Write((res));

                                while(res == null)
                                {
                                    trys ++;
                                    if(trys >= 3)
                                    {
                                        //Mata todas as threads                                
                                        stopAllThreads = true;
                                        alreadySent = true;
//                                            cp.fininshContinuousSocket();
                                        return;
                                    }
                                    Console.Write(("Tentando Envio Novamente novamente", 2));
                                    //espera 2 segundos
                                    Thread.Sleep(2000);
                                    goto _rewrite;
                                }

                                //reseta marcadores
                                dataToWrite = new List<Dictionary<string, string>>();
                                dataToWriteLen = 0;

                                //Environment.Exit(1);
                            }

                            //marca que os dados foram enviados
                            alreadySent = true;
                        }
                        else
                        {
                            //			writelog("Escrita desabilitada para configuracao");    
                            Console.Write(("Escrita desabilitada para configuracao", 2));

                            //marca que os dados foram enviados
                            alreadySent = true;

                            
                        }
                    }
                    catch(Exception e)
                    {
                        			writelog("Exceção disparada ao escrever tags -> "+e.ToString(), 3);
                        Console.Write("Exceção disparada ao escrever tags -> "+e.ToString());
                        alreadySent = true;
                    }
                }
                        
                    //opcMan.readTag(tags.ToArray(), false, completed);

                    //cria uma thread para leitura assyncrona do opc de tempos em tempos
                    if((opcReadingThread == null) || (!opcReadingThread.IsAlive))
                    {
                        			writelog("Criando Thread");    
                        //Console.Write(("Criando Thread"));

                        opcReadingThread = new Thread(() => {
                            try{
                                while(!stopAllThreads)
                                {
                                    //numero de tentativas de leitura
                                    int readingTryes = 0;

                                    //verifica se as configuracoes foram mudadas
                                    if(reloadThreadConfig)
                                    {
                                        			writelog("iniciando postagem dos dados");
                                        //desativa conexao atual caso exista
                                        //adiciona as tags ao banco e recupera os dados 
                                        //string postData = ""
                                        List<String> postData = new List<string>();
                                        int postLenCount = 0;
                                        foreach(string tag in opcTags.Keys)
                                        {
                                            //adiciona tag a string
                                            if((bool) opcTags[tag]["active"] == true)
                                            {
                                                if(tags.IndexOf(tag) < 0)   tags.Add(tag);
                                            }
                                        }
                                            
                                        //cria o grupo de leitura
                                        opcMan.startReadingGroup(tags.ToArray());

                                        reloadThreadConfig = false;
                                    }

                                    if(stopAllThreads == false)
                                    {
                                        //tenta ler o tempo de ciclo   caso nao consiga define o ciclo de leitura para 1 segundo
                                        string cycle = "10000";
                                        if(!configuration.TryGetValue("cycle", out cycle)) cycle = "10000";
                                        if(int.Parse(cycle) < 10000)   cycle = "10000";

                                        if(!reloadThreadConfig)
                                        {
                                            if(alreadySent)
                                            {
                                                //			writelog("Lendo Tags do OPC");    
                                                //Console.Write(("Lendo Tags do OPC"));
                                                //faz leitura das tags
                                                opcMan.readTag(false, completed);
                                                readingTryes = 0;
                                            }
                                            else
                                            {
                                                if(readingTryes >= 3)
                                                {
                                                    writelog("Falha ao Escrever Tags -> Nenhuma resposta recebida do servidor", 2);
                                                    writelog("Abortando e Reiniciando conexao Tags", 2);
                                                    readingTryes = 0;
                                                    alreadySent = true;
                                                    stopAllThreads = true;
                                                    reloadThreadConfig = true;
                                                    return;
                                                }
                                                else
                                                {
                                                    readingTryes ++;
                                                    //			writelog("Esperando para Ler Tags do OPC");    
                                                    writelog(("Esperando para Ler Tags do OPC"), 2);
                                                }
                                            }
                                        }

                                        Thread.Sleep(int.Parse(cycle));
                                    }
                                }
                            }

                            catch(Exception e)
                            {
                                			writelog("Uma Exceção foi encontrada em opcReadingThread " + e.ToString(), 3);
                                Console.Write("Uma Exceção foi encontrada em opcReadingThread " + e.ToString());
                            }

                            //			writelog("All Thread are Stopped");    
                            //Console.Write(("All Thread are Stopped"));

                            if(_comSerialPort.IsOpen)  
                                //fecha socket no device
                                if(sockNro >= 0)
                                {
                                    //			writelog("Tentando Fechar Socket");    
				                    //Console.Write(("Tentando Fechar Socket"));
//                                    cp.fininshContinuousSocket();
//                                    cp.closeSocket(sockNro);  
                                }

                            //			writelog("Finalizadno Thread");    
				            //Console.Write(("Finalizadno Thread"));
                            reloadThreadConfig = true;
                            ////Environment.Exit(1);
                            //Thread.CurrentThread.Abort();

                            return;
                        });

                        //			writelog("All Thread are Stopped");    
                        //Console.Write(("All Thread are Stopped"));

                        if(!opcReadingThread.IsAlive)
                            opcReadingThread.Start();  
                    }

                    while(true)
                    {
                        //caso a thread tenha sido cancelada
                        if(stoppingToken.IsCancellationRequested)
                        {                 
                            			writelog("Cancelation Requested", 3);
                            stopAllThreads = true;
                            return;
                        }

                        Thread.Sleep(1000);
                        //if((!_comSerialPort.IsOpen)  || (!opcReadingThread.IsAlive))
                        if((!opcReadingThread.IsAlive))
                        {
                            			writelog("Algum erro com a seguinte clausula (!_comSerialPort.IsOpen)  || (!opcReadingThread.IsAlive)", 3);  

                            if(_comSerialPort.IsOpen) _comSerialPort.Close();

                            			writelog("Serial Port Is Closed", 2);   
                            stopAllThreads = true;
                            Thread.Sleep(10000);
                            goto __findDevice;
                        }
                    }
                
            }
            catch(Exception e)
            {
                Console.WriteLine("Thread Principal -> Uma Exceção nao tratada foi encontrada, reiniciando serviços -->> \t Stack Trace \r\n {0} \r\n\r\n \t Exception Message \r\n {1} \r\n\r\n \t Exception Source \r\n {2} \r\n\r\n", e.StackTrace, e.ToString(), e.Source);    
				writelog("Thread Principal -> Uma Exceção nao tratada foi encontrada, reiniciando serviços -->> \t Stack Trace \r\n "+e.StackTrace+" \r\n\r\n \t Exception Message \r\n "+e.ToString()+" \r\n\r\n \t Exception Source \r\n "+e.Source+" \r\n\r\n", 3);    
                //caso a thread tenha sido cancelada
                if(stoppingToken.IsCancellationRequested)   return;
                goto __findDevice;
            }
        }

        static void getHostList()
        {
            Dictionary<string, string> ipResponse = new Dictionary<string, string>();
            
            string GetLocalIPAddress()
            {
                var host = Dns.GetHostEntry(Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == AddressFamily.InterNetwork)
                    {
                        //			writelog(ip.ToString());    
				        ////Console.Write((ip.ToString()));
                        return ip.ToString();
                    }
                }

                return "127.0.0.1";
            }

            if(System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
            {
                NetworkHosts = new Dictionary<string, string>();
                opcServers = new List<string>();


                string ip = GetLocalIPAddress();
                ip = ip.Substring(0, ip.LastIndexOf("."));

                void addAddressToList(object sender, PingCompletedEventArgs e)
                {                   
                    var rep = e.Reply;

                    if (rep.Status == System.Net.NetworkInformation.IPStatus.Success)
                    {
                        try{
                            var host = Dns.GetHostEntry(rep.Address.ToString());
                            NetworkHosts[rep.Address.ToString()] = host.HostName; 

                            //Console.Write(e.Reply.Status + " => ");
                            //Console.Write(e.Reply.Address + " => ");                                  
                            //			writelog(host.HostName);    
				            //Console.Write(e.Reply.Address + " => " + host.HostName);  

                            //			writelog("Buscando Servidores OPC");    
				            //Console.Write(("Buscando Servidores OPC"));
                            //busca servidores opc na maquina
                            var enumerator = new OpcServerEnumeratorAuto();                            
                            var serverDescriptions = enumerator.Enumerate(host.HostName, OpcServerCategory.OpcDaServer20);

                            foreach(OpcServerDescription desc in serverDescriptions)
                            {
                                //			writelog(desc.ToString());    
				                //Console.Write((desc.ToString()));
                                if(!opcServers.Contains(desc.ToString()))
                                    opcServers.Add(desc.ToString());
                            }                   
                            //			writelog("Fim da Busca por Servidores OPC");    
				            //Console.Write(("Fim da Busca por Servidores OPC"));

                        }
                        catch(Exception)
                        {
                            try{
                                //Console.Write("Tentando acessar dicionatia de network horts");
                                NetworkHosts[rep.Address.ToString()] = rep.Address.ToString();
                                //			writelog(rep.Address);    
				                //Console.Write((rep.Address));

                                //busca servidores opc na maquina
                                var enumerator = new OpcServerEnumeratorAuto();                            
                                var serverDescriptions = enumerator.Enumerate(rep.Address.ToString(), OpcServerCategory.OpcDaServer30);

                                foreach(OpcServerDescription desc in serverDescriptions)
                                {
                                    //			writelog(desc.ToString());    
				                    //Console.Write((desc.ToString()));
                                    if(!opcServers.Contains(desc.ToString()))
                                        opcServers.Add(desc.ToString());
                                }     
                            }
                            catch(Exception ex)
                            {
                                //			writelog("N/D");    
				                //Console.Write(("N/D"));
                            }
                        }
                    }   
                }

                for(int i=1;i<255;i++)
                {
                    PingOptions options = new PingOptions (32, true);
                    byte[] buffer = Encoding.ASCII.GetBytes("ping");
                    System.Net.NetworkInformation.Ping p = new System.Net.NetworkInformation.Ping();

                    // the PingCompletedCallback method is called.
                    //comentado para nao ter erros de DCOM
                    //p.PingCompleted += new PingCompletedEventHandler(addAddressToList);
                    p.SendAsync(ip+"."+i.ToString(), 120, buffer,options);
                }
            }

        }

        public static Dictionary<string, string> simpleJsonLoad(string json)
        {
            Dictionary<string, string> retData = new Dictionary<string, string>();

            json = json.Replace("{", "").Replace("}", "").Replace("\"", "");
            string[] itens = json.Split(",");

            foreach(string val in itens)
            {
                string[] tmp = val.Split(":");
                if(tmp.Length == 2) 
                {
                    //retira espacos em branco do inicio e do fim da string
                    while(tmp[0].StartsWith(" "))   tmp[0] = tmp[0].Substring(1);
                    while(tmp[0].EndsWith(" "))   tmp[0] = tmp[0].Substring(0, tmp[0].Length-2);
                    
                    //retira espacos em branco do inicio e do fim da string
                    while(tmp[1].StartsWith(" "))   tmp[1] = tmp[1].Substring(1);
                    while(tmp[1].EndsWith(" "))   tmp[1] = tmp[1].Substring(0, tmp[1].Length-2);

                    retData[tmp[0]] = tmp[1];
                }
            }

            return retData;
        }
        public Dictionary<string, Dictionary<string, object>> getOpcTags()
        {
            return opcTags;
        }
        public Dictionary<string, string> getNetworkHosts()
        {
            return NetworkHosts;
        }
        public List<string> getNetworkOpc()
        {
            return opcServers;
        }
        public Dictionary<string, string> getConfiguration()
        {
            return configuration;
        }

        /*herdados da aplicacao de arquivos de log*/
        static int loadConfiguration()
        {
            //opcServers = new List<string>();
            //reseta as tags a cara loading
            opcTags = new Dictionary<string, Dictionary<string, object>>();

            try{

                if(!File.Exists(configFile))
                {
                    var cfg = new StreamWriter(File.Open(configFile, FileMode.Create));
                    cfg.Write(  "apn=timbrasil.br\r\n"+
                                "apnuser=tim\r\n"+
                                "apnpass=tim\r\n"+
                                "projectpath=C:\\CEMI\\OPTPROCESS\r\n"+
                                "ftpdomain=ftp.optcemi.com\r\n"+
                                "server=optcemi.com\r\n"+
                                "serverport=21032\r\n"+
                                "ftpuser=ftpuser\r\n"+
                                "ftppass=ftppass\r\n"+
                                "opcserver=Matrikon.OPC.Simulation.1\r\n"+
                                "opchost=127.0.0.1\r\n"+
                                "log=true\r\n"+
                                "cycle=30000\r\n"+
                                "timeout=30000\r\n"+
                                "tagsperrequest=100\r\n"+
                                "ssid=CEMI_AP\r\n"+
                                "password=c3m1t3ch\r\n"+
                                "ip=192.168.1.222\r\n"+
                                "gateway=192.168.1.1\r\n"+
                                "mode=mobile\r\n"+
                                "project=null\r\n"+
                                "projectalias=null\r\n"+
                                "log=true\r\n"+
                                "loglevel=0\r\n"+
                                "netmask=255.255.255.0\r\n");
                    cfg.Close();
                }

                configuration["cycle"] = configuration.ContainsKey("cycle") ? configuration["cycle"]: "30000";
                configuration["timeout"] = configuration.ContainsKey("timeout") ? configuration["timeout"]: configuration["cycle"];
                configuration["apn"] = configuration.ContainsKey("apn") ? configuration["apn"]: "timbrasil.br";
                configuration["apnuser"] = configuration.ContainsKey("apnuser") ? configuration["apnuser"]: "tim";
                configuration["apnpass"] = configuration.ContainsKey("apnpass") ? configuration["apnpass"]: "tim";
                configuration["projectpath"] = configuration.ContainsKey("projectpath") ? configuration["projectpath"]: "";
                configuration["ftpdomain"] = configuration.ContainsKey("ftpdomain") ? configuration["ftpdomain"]: "ftp.optcemi.com";
                configuration["server"] = configuration.ContainsKey("server") ? configuration["server"]: "optcemi.com";
                configuration["serverport"] = configuration.ContainsKey("serverport") ? configuration["serverport"]: "21032";
                configuration["ftpuser"] = configuration.ContainsKey("ftpuser") ? configuration["ftpuser"]: "optcemi01";
                configuration["ftppass"] = configuration.ContainsKey("ftppass") ? configuration["ftppass"]: "qazxc123";
                configuration["tagsperrequest"] = configuration.ContainsKey("tagsperrequest") ? configuration["tagsperrequest"]: "100";
                configuration["log"] = configuration.ContainsKey("log") ? configuration["log"]: "false";
                configuration["browseopc"] = configuration.ContainsKey("browseopc") ? configuration["browseopc"]: "false";
                configuration["projectalias"] = configuration.ContainsKey("projectalias") ? configuration["projectalias"]: "null";
                configuration["mode"] = configuration.ContainsKey("mode") ? configuration["mode"]: "mobile";
                
                //log data
                configuration["log"] = configuration.ContainsKey("log") ? configuration["log"]: "true";
                configuration["loglevel"] = configuration.ContainsKey("loglevel") ? configuration["loglevel"]: "0";
                
                //wifi model
                configuration["ssid"] = configuration.ContainsKey("ssid") ? configuration["ssid"]: "CEMI Network";
                configuration["password"] = configuration.ContainsKey("password") ? configuration["password"]: "c3m1t3ch";
                configuration["ip"] = configuration.ContainsKey("ip") ? configuration["ip"]: "0.0.0.0";
                configuration["gateway"] = configuration.ContainsKey("gateway") ? configuration["gateway"]: "192.168.1.1";
                configuration["netmask"] = configuration.ContainsKey("netmask") ? configuration["netmask"]: "255.255.255.0";

                string configData = File.ReadAllText(configFile);
                string[] configLines = configData.Split("\r\n");
                foreach(string line in configLines)
                {
                    //ignora linhas comentadas e le configuração separada por :
                    if(!line.StartsWith('#'))
                    {
                        if(!line.StartsWith('$'))
                        {
                            if(line.Contains("="))
                            {
                                string[] temp = line.Split("=");
                                if(temp[0].Replace(" ","").Length > 0)
                                    configuration[temp[0]] = temp[1];
                            }
                        }
                        //pega as tags
                        else if(line.Contains("="))
                        {
                            string[] temp = line.Split("=");
                            if(temp[0].Replace(" ","").Length > 0)
                            {
                                /*
                                opcTags[temp[0].Replace("$","")] = new Dictionary<string, object>();
                                opcTags[temp[0].Replace("$","")]["active"] = (temp[1]=="on") ? true : false;     
                                opcTags[temp[0].Replace("$","")]["type"] = "double";     
                                */

                                string tempKey = temp[0].Substring(1);



                                opcTags[tempKey] = new Dictionary<string, object>();
                                opcTags[tempKey]["active"] = (temp[1]=="on") ? true : false;     
                                opcTags[tempKey]["type"] = "double";                           
                            }
                        }
                    
                    }
                }

                //verifica se tem valor default para o nome do opc
                string opcserver = "";
                if(!configuration.TryGetValue("opcserver", out opcserver)) configuration["opcserver"] = "Matrikon.OPC.Simulation.1";
                if(!configuration.TryGetValue("opchost", out opcserver)) configuration["opchost"] = "127.0.0.1";
            
                /*****************************Busca Tags Dentro do OPC******************************/
                opcMan.host = configuration["opchost"];
                opcMan.name = configuration["opcserver"];

                //recarrega configuração da transferencia
                reloadThreadConfig = true;          

                //tenta iniciar uma conxao com o opc
                if(opcMan.startClient())    			writelog("Conexao Opc Iniciada com Sucesso");
                else
                {
				    writelog("Falha ao Iniciar Conexão do Opc", 3);
                    return 0;
                }

                //busca tags presentes e adiciona a lista caso configurado para fazêlo
                List<string> tmp = new List<string>();
                if(configuration["browseopc"] == "true")
                    tmp = opcMan.getAllTags();

                //adiciona tags selecionadas sem verificar o opc
                Dictionary<string, Dictionary<string, object>> tempOpcTags  = opcTags;

                //adiciona demais tags do opc a lista para selecao
                foreach(string tag in tmp)
                {
                    string[] tagInfo = tag.Split(",");

                    if(!opcTags.ContainsKey(tagInfo[0]))
                    {
                        tempOpcTags[tagInfo[0]] = new Dictionary<string, object>();
                        tempOpcTags[tagInfo[0]]["active"] = false;
                        tempOpcTags[tagInfo[0]]["type"] = tagInfo[1];
                    }
                    else
                    {
                        tempOpcTags[tagInfo[0]] = new Dictionary<string, object>();
                        tempOpcTags[tagInfo[0]]["active"] = true;
                        tempOpcTags[tagInfo[0]]["type"] = tagInfo[1];
                    }
                }

                opcTags = tempOpcTags;    

                return 1;
            }
            catch(Exception e)
            {
                			writelog("Configuração Nao Carregada no Sistema", 3);    
				Console.Write(("Configuração Nao Carregada no Sistema"), 3);
                //			writelog(e.ToString());    
				Console.Write((e.ToString()));

                return 0;
            }
        }

        public List<string> getSignalStatus(SerialPort sp)
        {
            List<string> signal = new List<string>();

            
            //recupera imei do dispositivo
            int errors = 0;
            int allowedErrors = 10;
            string incoming = "";

            //desabilita echo
            echo:
            if(errors > allowedErrors) throw new OperationCanceledException("Foi Alcançado o numero maximo de tentativas de acesso ao modem");
            sp.Write("\r\nAT+CSQ\r\n");
            incoming = "";
            do{incoming += sp.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));
            if(! incoming.Contains("")){Thread.Sleep(1000); errors++; goto echo;}

            incoming = incoming.Replace("+CSQ: ", "").Replace("OK", "").Replace("+CCID: ", "");

            //			writelog("Signal Check");    
				//Console.Write(("Signal Check"));
            //			writelog(incoming);    
				//Console.Write((incoming));

            try{
                if(incoming.Contains("ERROR"))
                {
                    signal.Add("-1");
                    signal.Add("-1");
                }
                else
                {
                    string[] d = incoming.Split(",");

                    if(d[1].Equals("0"))    d[1] = "5%";
                    else{
                        double l = int.Parse(d[1]);
                        l = (((l-1)*4) +10) * 3.33; //multiplica por 3.33 para ficar de 0 a 100
                        d[1] = l.ToString().Replace(",",".") + " %";
                    }

                    switch(d[0])
                    {
                        case "0":
                            d[0] = "-110.5 dBm";
                            break;
                        case "2":
                            d[0] = "-105.5 dBm";
                            break;
                        case "4":
                            d[0] = "-100.5 dBm";
                            break;
                        case "7":
                            d[0] = "-95.5 dBm";
                            break;
                        case "10":
                            d[0] = "-90.5 dBm";
                            break;
                        case "11":
                            d[0] = "-87.5 dBm";
                            break;
                        case "12":
                            d[0] = "-85.5 dBm";
                            break;
                        case "13":
                            d[0] = "-82.5 dBm";
                            break;
                        case "14":
                            d[0] = "-80.5 dBm";
                            break;
                        case "17":
                            d[0] = "-75.5 dBm";
                            break;
                        case "19":
                            d[0] = "-70.5 dBm";
                            break;
                        case "22":
                            d[0] = "-65.5 dBm";
                            break;
                        case "24":
                            d[0] = "-60.5 dBm";
                            break;
                        case "27":
                            d[0] = "-55.5 dBm";
                            break;
                        case "30":
                            d[0] = "-50.5 dBm";
                            break;
                        case "31":
                            d[0] = "-46.5 dBm";
                            break;
                    }
                    signal.Add(d[0]);
                    signal.Add(d[1]);

                    //processa qualidade
                }
            }
            catch(Exception e)
            {
                signal = new List<string>();
                signal.Add("Error");
                signal.Add("0");
            }
            
            return signal;
        }

        static string getLocation(SerialPort serial)
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
                //			writelog("Enviando");
                //			writelog(_buffer);
                string incoming = "";
                int timeout = 900;      //timeout de 1 minuto e meio devico ao post
                do{
                    string buff = serial.ReadExisting();
                    if(buff.Length >= 1)
                    {
                        incoming += buff;//.Replace("\r","").Replace("\n",""); 
                        timeout = 900;
                    }
                    else
                    {
                        timeout --;
                        Thread.Sleep(100);
                    }
                    if(timeout <= 0)    incoming = "ERROR -> Request TimedOut";
                } while(!Regex.Match(incoming, _responsePatern,RegexOptions.Singleline).Success && !incoming.Contains("ERROR"));
            
                Match match = Regex.Match(incoming, _responsePatern,RegexOptions.Singleline);
                if(!match.Success){Thread.Sleep(1000); _errors++; goto executeRequest;}

                if(match.Groups.Count >= 2)
                {
                    return(match.Groups[1].Value);
                }

                return null;
            }

            
            sendPacket("AT+UGAOP=\"eval1-les.services.u-blox.com\",46434,1000,0",  ".*(OK).*");
            sendPacket("AT+ULOCCELL=0",  ".*(OK).*");
            sendPacket("AT+ULOC=2,2,1,100,5000",  ".*(OK).*");
            string location = (string) sendPacket("AT+ULOC=2,2,1,100,5000",  @"\+UULOC\:\s(.*)");

            			writelog("location -> "+location.ToString());

            return location;
        }

        static string identifier(string key, string value)
        {
/*
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("select * from " + key);
            foreach (ManagementObject share in searcher.Get())
            {
                if(share[value] != null)
                    return share[value].ToString();
            }
*/
            var query =  CimSession.Create(null) // null instead of localhost which would otherwise require certain MMI services running
                .QueryInstances(@"root\cimv2", "WQL", "SELECT * FROM " + key);
                
                foreach (var share in query)
                {
                    if(share.CimInstanceProperties != null)
                        return share.CimInstanceProperties[value].ToString();
                }
                //.FirstOrDefault().CimInstanceProperties[value].Value.ToString();

            return "";
        }

        static string getComputerId()
        {            
            /*
            			writelog("Hard Disk");
            			writelog("Model -> " + identifier("Win32_DiskDrive", "Model"));
            			writelog("Manufacturer -> " + identifier("Win32_DiskDrive", "Manufacturer"));
            			writelog("Signature -> " + identifier("Win32_DiskDrive", "Signature"));
            			writelog("TotalHeads -> " + identifier("Win32_DiskDrive", "TotalHeads"));
            			writelog("Win32_BaseBoard");
            			writelog("Model -> " + identifier("Win32_BaseBoard", "Model"));
            			writelog("Manufacturer -> " + identifier("Win32_BaseBoard", "Manufacturer"));
            			writelog("Name -> " + identifier("Win32_BaseBoard", "Name"));
            			writelog("SerialNumber -> " + identifier("Win32_BaseBoard", "SerialNumber"));
            			writelog("Win32_Processor");
            			writelog("Architecture -> " + identifier("Win32_Processor", "Architecture"));
            			writelog("Caption -> " + identifier("Win32_Processor", "Caption"));
            			writelog("Family -> " + identifier("Win32_Processor", "Family"));
            			writelog("ProcessorId -> " + identifier("Win32_Processor", "ProcessorId"));
*/
            string deviceIdFile = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)) + "ProgramData\\CEMI\\OptBiSender\\machine.id";

            //verifica se existe um arquivo de id
            if(!File.Exists(deviceIdFile))
            {
                string ComputerId = "000000000-0000-0000-0000-000000000000";

                //Main physical hard drive ID
                string diskId()
                {
                    //return "";

                    return identifier("Win32_DiskDrive", "Model")
                    + identifier("Win32_DiskDrive", "Manufacturer")
                    + identifier("Win32_DiskDrive", "Signature")
                    + identifier("Win32_DiskDrive", "TotalHeads");
                }
                //Motherboard ID
                string baseId()
                {
                    return identifier("Win32_BaseBoard", "Model")
                    + identifier("Win32_BaseBoard", "Manufacturer")
                    + identifier("Win32_BaseBoard", "Name")
                    + identifier("Win32_BaseBoard", "SerialNumber");
                }
                //Processor ID
                string processorId()
                {
                    return identifier("Win32_Processor", "Architecture")
                    + identifier("Win32_Processor", "Caption")
                    + identifier("Win32_Processor", "Family")
                    + identifier("Win32_Processor", "ProcessorId"); 

                }
                //Processor ID
                string networkId()
                {
                    return identifier("Win32_NetworkAdapterConfiguration", "MacAddress");
                }

                using (SHA256 sha256Hash = SHA256.Create())
                {

                            
                    // Convert the input string to a byte array and compute the hash.
                    byte[] data = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(diskId()+baseId()+processorId()+networkId()));

                    // Create a new Stringbuilder to collect the bytes
                    // and create a string.
                    var sBuilder = new StringBuilder();

                    // Loop through each byte of the hashed data 
                    // and format each one as a hexadecimal string.
                    for (int i = 0; i < data.Length; i++)
                    {
                        sBuilder.Append(data[i].ToString("x2"));
                    }

                    // Return the hexadecimal string.
                    string hash = sBuilder.ToString();
                    
                    //			writelog("hash = {0}", hash);    
                    //Console.Write(("hash = {0}", hash));


                    File.WriteAllText(deviceIdFile, hash);
                }

            }

            			writelog("Device Id File -> ");
            			writelog(File.ReadAllText(deviceIdFile));

            return File.ReadAllText(deviceIdFile);
        }
        static void connectNetwork(DeviceconectionProcedures cp)
        {
            int _connectNetworkTrys = 3;

            startNetwork:
            //inicia conexao de rede
            if(!cp.isConnected())
            {
                try{
                    //			writelog("Start Pdp Event - Trying to start pdp connection");    
				//Console.Write(("Start Pdp Event - Trying to start pdp connection"));

                    if(cp.pdpConection()) {
                        //			writelog("Sucessfuly connected");    
				        Console.Write("Sucessfuly connected");
                        }
                    else {
                        if(_connectNetworkTrys == 0) return;
                        _connectNetworkTrys--;
                        //			writelog("Fail to Conect to Network");    
				        Console.Write(("Fail to Conect to Network"));
                        /*
                        //finaliza thread de configuração
                        cs.pauseServer();
                        resetDevice();
                        cs.resumeServer();
                        */
                        goto startNetwork;
                    }
                }
                catch(TimeoutException e)
                {
                    if(_connectNetworkTrys == 0) return;                    
                    _connectNetworkTrys--;

                    //			writelog("Start Pdp Error - Serial has Timeout");    
				    Console.Write(("Start Pdp Error - Serial has Timeout"));
                    goto startNetwork;                    
                }
            }
        }
        static void resetDevice()
        {
            /*
            List<string> ports = findDevice();

            if(_comSerialPort != null)
                if(_comSerialPort.IsOpen)
                    _comSerialPort.Close();
            if(_infoSerialPort != null)
                if(_infoSerialPort.IsOpen)
                    _infoSerialPort.Close();

            foreach(string port in ports)
            {
                if(!port.Equals("0"))
                {
                    SerialPort sp = new SerialPort(port, 115200);
                    sp.Open();
                    //			writelog("Resetando device");    
				    //Console.Write(("Resetando device"));
                    sp.Write("\r\nAT+CFUN=16\r\n");
                    sp.Close();

                    //apos reset abre a serial atual
                    Thread.Sleep(60000);
                    //			writelog("Religando device");    
				    //Console.Write(("Religando device"));

                    //apos religar device sai do loop
                    break;
                }
            }
            */
        }
        static List<string> findDevice()
        {
            List<string> devices = new List<string>();
            //Console.Clear();
            //			writelog("Procurando Portas de Comunicação");
            
		    Console.Write("Procurando Portas de Comunicação");
            
            for(int i=0; i<100; i++)
            {
                //declara a porta serial
                SerialPort sp = new SerialPort("COM"+i.ToString(), _uartSpeed);
                sp.ReadTimeout = 500;
                sp.Handshake = Handshake.XOnXOff;

                //uma boolean como flag de disconexao
                bool disconect = true;

                Console.Write("Buscando Device Na Porta -> " + "COM"+i.ToString()+"\r\n");

                verifying:
                try{
                    
                    sp.Open();
                    sp.DiscardInBuffer();
                    sp.DiscardOutBuffer();
                    sp.Write("AT+GMR\r\n");
                    string result = "";
                    int trys = 0;
                    do{if(trys >= 6) goto jump; trys ++; result += sp.ReadLine().Replace("\r", "").Replace("\n", "");} while(!result.Contains("OK") && !result.Contains("ERROR"));
                    sp.DiscardInBuffer();
                    sp.Close();
                    
                    if(result.Contains("ERROR") && (!result.Contains("OK")))
                    {
                        			writelog("Sim Card Not Found");    
				        Console.Write(("Sim Card Not Found"));
                        goto jump;
                    }

                    if(result.Contains("SDK"))  deviceType = 0;
                    else  deviceType = 1;
                    
                    			writelog(result);    
                    //Console.Write((result));
                    //			writelog("PORT FOUD") ;    
                    //Console.Write(("PORT FOUD") );   
                    Console.Write(" - Device Encontrado em -> "+ "COM"+i.ToString()+"\r\n");      
                    
				    //Console.Write("Device Encontrado em -> "+ "COM"+i.ToString());         

                    devices.Add("COM"+i.ToString());
                    jump:;

                    /*
                    sp.Open();
                    sp.DiscardInBuffer();
                    sp.DiscardOutBuffer();
                    sp.Write("\r\nat+ccid\r\n");
                    string result = "";
                    int trys = 0;
                    do{if(trys >= 6) goto jump; trys ++; result += sp.ReadLine().Replace("\r", "").Replace("\n", "");} while(!result.Contains("CCID") && !result.Contains("ERROR"));
                    sp.DiscardInBuffer();
                    sp.Close();
                    
                    if(result.Contains("ERROR"))
                    {
                        //			writelog("Sim Card Not Found");    
				        //Console.Write(("Sim Card Not Found"));
                        goto jump;
                    }
                    
                    //			writelog(result);    
                    //Console.Write((result));
                    //			writelog("PORT FOUD") ;    
                    //Console.Write(("PORT FOUD") );   
                    //Console.Write(" - Device Encontrado em -> "+ "COM"+i.ToString());      
                    
				    //Console.Write("Device Encontrado em -> "+ "COM"+i.ToString());         

                    devices.Add("COM"+i.ToString());
                    jump:;
                    */
                }
                catch(TimeoutException)
                {
                    if(!disconect)
                    {
                        //executa desconexao caso necessario
                        //Console.Write(" -> Enviando comando de desconexao");
                        //Console.Write(" | ");
                        sp.Write("+");
                        Thread.Sleep(2500);
                        //Console.Write(" | ");
                        sp.Write("+");
                        Thread.Sleep(500);
                        //Console.Write(" | ");
                        sp.Write("+");
                        Thread.Sleep(500);
                        //Console.Write(" | ");
                        sp.Write("+");
                        Thread.Sleep(2500);
                        //Console.Write(" | ");
                        sp.Write("+");
                        sp.Close();

                        disconect = true;
                        //verifica novamente
                        goto verifying;
                    }
                    sp.Close();
                }
                catch(Exception e)
                {
                    sp.Close();
                    //Console.Write(" - Device Nao Encontrado em -> "+ "COM"+i.ToString() + " Devido a Exceção -> " + e.ToString());
                    //Console.Write(" - Device Nao Encontrado em -> "+ "COM"+i.ToString() + " Devido a Exceção -> " + e.ToString());
                }
                
                //			writelog("");    
				//Console.Write((""));
            }
            return devices;
        }

        static void writelog(Object Data, int eventType=0)
        {
            /// <summary>
            /// Write to logfile in programdata/cemi/optsync folder
            /// </summary>
            /// <param name="eventType"> Is the type of event \n 0 - all \n 1 - info \n 2 - alert \n 3 - error</param>
            /// <returns></returns>
            /// 
           
            string type = "info";
            if(eventType == 1) type = "info";
            else if(eventType == 2) type = "warning";
            else if(eventType == 3) type = "error";
            
            if(!configuration.ContainsKey("log") || (configuration["log"] == "true"))
            {
                try{
                    if(Data.ToString().Length <= 2) return;

                    if(logQueue == null)        logQueue = new Stack<string>();

                    if(configuration.ContainsKey("loglevel"))
                    {
                        if(eventType >= int.Parse(configuration["loglevel"]))
                        {
                            if(!logQueue.Contains(DateTime.Now.ToString() + " => " + type + " => " + Data.ToString() + "\r\n"))
                                logQueue.Push(DateTime.Now.ToString() + " => " + type + " => " + Data.ToString() + "\r\n");
                        }

                    }
                    else
                    {
                            if(!logQueue.Contains(DateTime.Now.ToString() + " => " + type + " => " + Data.ToString() + "\r\n"))
                                logQueue.Push(DateTime.Now.ToString() + " => " + type + " => " + Data.ToString() + "\r\n");
                    }
                }
                catch(Exception e)
                {
                    			Console.WriteLine("Erro ao Escrever Log");
                    			Console.WriteLine(e);
                }
                
                while(logQueue.Count > 0)
                {
                    try{
                        //			Console.WriteLine("Tentando Escrever Log");
                        string logFile  = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles)) + "ProgramData\\CEMI\\OptBiSender\\optsync.log";

                        if(File.Exists(logFile))
                            File.AppendAllText(logFile, logQueue.Pop());
                        else
                            File.WriteAllText(logFile,  logQueue.Pop());
                    }
                    catch(Exception e)
                    {
                        			Console.WriteLine("Erro ao Escrever Log");
                        			Console.WriteLine(e);
                    }
                }
            }
        }
    }
}
