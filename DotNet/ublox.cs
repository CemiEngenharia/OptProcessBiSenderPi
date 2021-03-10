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
    class Common:DeviceCommon{
        public override Dictionary<string, string> getDeviceData(SerialPort sp)
        {
            Dictionary<string, string> info = new Dictionary<string, string>();
            //recupera imei do dispositivo
            int errors = 0;
            int allowedErrors = 10;
            string incoming = "";

            //desabilita echo
            echo:
            if(errors > allowedErrors) throw new OperationCanceledException("Foi Alcançado o numero maximo de tentativas de acesso ao modem");
            sp.Write("\r\nATE0\r\n");
            incoming = "";
            do{incoming += sp.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));
            if(! incoming.Contains("")){Thread.Sleep(1000); errors++; goto echo;}

            //recupera o imei do device
            imei:
            if(errors > allowedErrors) throw new OperationCanceledException("Foi Alcançado o numero maximo de tentativas de acesso ao modem");
            sp.Write("\r\nAT+CGSN=0\r\n");
            incoming = "";
            do{incoming += sp.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));
            if(! incoming.Contains("")){Thread.Sleep(1000); errors++; goto imei;}

            incoming = incoming.Replace("AT+CGSN=0", "").Replace("OK", "").Replace("+PACSP1", "");
            
            info["imei"] = incoming;

            //recupera firmware version
            firmware:
            if(errors > allowedErrors) throw new OperationCanceledException("Foi Alcançado o numero maximo de tentativas de acesso ao modem");
            sp.Write("\r\nAT+GMR\r\n");
            incoming = "";
            do{incoming += sp.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));
            if(! incoming.Contains("")){Thread.Sleep(1000); errors++; goto firmware;}

            incoming = incoming.Replace("AT+CGSN=0", "").Replace("OK", "").Replace("+PACSP1", "");
            
            info["firmware"] = incoming;

            //recupera firmware version
            simCardId:
            if(errors > allowedErrors) throw new OperationCanceledException("Foi Alcançado o numero maximo de tentativas de acesso ao modem");
            sp.Write("\r\nAT+CCID\r\n");
            incoming = "";
            do{incoming += sp.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));
            if(! incoming.Contains("")){Thread.Sleep(1000); errors++; goto simCardId;}

            //Console.WriteLine(incoming);    
				//Console.Write((incoming));

            incoming = incoming.Replace("+CME ERROR: ", "").Replace("OK", "").Replace("+CCID: ", "");
            
            info["SIMID"] = incoming;


            return info;
        }
    }

    class WebServer
        {
            string appDataPath;
            string tempFolder;
            string configFolder;
            string partFolder;
            string application;

            public WebServer()
            {                
                this.appDataPath = Environment.GetEnvironmentVariable("appdata");
                this.tempFolder = appDataPath+@"\CEMI\OptSync\temp";
                this.configFolder = appDataPath+@"\CEMI\OptSync\config";
                this.partFolder = appDataPath+@"\CEMI\OptSync\part";
                
                //verifica se o arquivo existe
                if(!Directory.Exists(this.tempFolder))
                {
                    Directory.CreateDirectory(this.tempFolder);
                }
                //verifica se o arquivo existe
                if(!Directory.Exists(this.configFolder))
                {
                    Directory.CreateDirectory(this.configFolder);
                }
                //verifica se o arquivo existe
                if(!Directory.Exists(this.partFolder))
                {
                    Directory.CreateDirectory(this.partFolder);
                }
            }

            public string getInternalServerConfig(String url)
            {
                // Create a request for the URL.   
                WebRequest request = WebRequest.Create(
                "http://www.cemi.eng.br/tools/dynamic_dns.php?type=get&apikey=cemi5977f012699e6ba7b0a144f24c79ea71&host=server");
                // If required by the server, set the credentials.  
                request.Credentials = CredentialCache.DefaultCredentials;
                
                // Get the response.  
                WebResponse response = request.GetResponse();
                // Display the status.  
                Console.WriteLine(((HttpWebResponse)response).StatusDescription);
                
                // Get the stream containing content returned by the server. 
                // The using block ensures the stream is automatically closed. 
                using (Stream dataStream = response.GetResponseStream())
                {
                    // Open the stream using a StreamReader for easy access.  
                    StreamReader reader = new StreamReader(dataStream);
                    // Read the content.  
                    string responseFromServer = reader.ReadToEnd();
                    // Display the content.  
                    Console.WriteLine(responseFromServer);
                }
                
                // Close the response.  
                response.Close();
                
                return "success";
            }
        }

        class FilePrepare
        {
            string appDataPath;
            string tempFolder;
            string configFolder;
            string partFolder;
            string cryptoKey;

            public FilePrepare()
            {
                this.appDataPath = Environment.GetEnvironmentVariable("appdata");
                this.tempFolder = appDataPath+@"\CEMI\OptSync\temp";
                this.configFolder = appDataPath+@"\CEMI\OptSync\config";
                this.partFolder = appDataPath+@"\CEMI\OptSync\part";
                
                //verifica se o arquivo existe
                if(!Directory.Exists(this.tempFolder))
                {
                    Directory.CreateDirectory(this.tempFolder);
                }
                //verifica se o arquivo existe
                if(!Directory.Exists(this.configFolder))
                {
                    Directory.CreateDirectory(this.configFolder);
                }
                //verifica se o arquivo existe
                if(!Directory.Exists(this.partFolder))
                {
                    Directory.CreateDirectory(this.partFolder);
                }

                //define chave de criptografia
                this.cryptoKey = "CemiTechnologyRegistered";
            }

            public List<string> prepareLogToSend(string filePath, bool _project=false, bool _log=true)
            {
                //verifica se pasta é pasta do optprocess
                string[] structure = {"Bkp", "Dat", "Est", "Exp", "His", "Hmi", "Inf", "Log", "Old", "Tag"};
                string projectFile = "";

                bool isProccessFolder = true;

                //caminho dos arquivos para sincronizar
                List<string> pathList = new List<string>();

                //verifica existencia da estrutura basica de pastas
                foreach(string name in structure)
                {
                    if(!Directory.Exists(filePath+@"\"+name))   isProccessFolder = false;
                }

                if(isProccessFolder == false) Console.WriteLine("Pasta é Invalida");
                
                //verifica existencia do arquivo sco                
                var files = Directory.GetFiles(filePath);                
                for(int i=0; i<files.Length; i++) 
                {
                    if(files[i].EndsWith(".sco"))  {projectFile = files[i].Replace(filePath+@"\", ""); break;}
                    if(i == files.Length-1)   isProccessFolder = false;     //ultima passada é verificado
                }

                if(isProccessFolder == false) Console.WriteLine("Pasta é Invalida");

                if(isProccessFolder)
                {
                    //define arquivo temporario e pasta temporaria
                    string tempProjectFolder = this.tempFolder+"\\project";
                    string tempProjectFile = this.tempFolder+"\\project.tmp";


                    if(_project)
                    {
                        //copia pastas para a temporaria criando ela caso nao exista
                        if(Directory.Exists(tempProjectFolder)) Directory.Delete(tempProjectFolder, true);
                        Directory.CreateDirectory(tempProjectFolder);
                        
                        foreach(string name in structure)
                        {
                            if((name !=  "Log") && (name !=  "Bkp"))
                            {
                                Console.WriteLine("Copiando de "+filePath+@"\"+name+ " para "+tempProjectFolder);
                                DirectoryCopy(filePath+@"\"+name, tempProjectFolder+@"\"+name, true);
                            }
                        }
                        
                        //copia arquivo de projeto do process
                        File.Copy(filePath+@"\"+projectFile, tempProjectFolder+@"\"+projectFile, true);

                        Console.WriteLine("Copiado com sucesso");

                        //verifica se arquivo ja existe e exclui nesse caso
                        if(File.Exists(tempProjectFile))    File.Delete(tempProjectFile);

                        //zipa arquivos do projeto e verifica modificações
                        ZipFile.CreateFromDirectory(tempProjectFolder, tempProjectFile);

                        //Exclui pasta temporaria
                        Directory.Delete(tempProjectFolder, true);

                        string path;
                        //faz hash do conteudo do arquivo de projeto
                        using (SHA512 shaHash = SHA512.Create())
                        {
                            path = BitConverter.ToString(shaHash.ComputeHash(File.ReadAllBytes(tempProjectFile))).Replace("-","");
                        }

                        //copia o arquivo para a pasta de saida do computador
                        if(!Directory.Exists("synchronizing"))  Directory.CreateDirectory("synchronizing");
                        path = "synchronizing\\"+path+".zip";

                        File.Copy(tempProjectFile, path, true);

                        pathList.Add(path);
                    }
                    if(_log)
                    {
                        //copia logs para a pasta de sincronização
                        string[] logFiles = Directory.GetFiles(filePath+@"\Log");
                        foreach(string logFile in logFiles)
                        {
                            if(logFile.EndsWith(".zip"))
                            {
                                string path = "";
                                using (SHA512 shaHash = SHA512.Create())
                                {
                                    path = BitConverter.ToString(shaHash.ComputeHash(File.ReadAllBytes(logFile))).Replace("-","");
                                }

                                //copia Arquivos para a nova pasta
                                if(!Directory.Exists("synchronizing"))  Directory.CreateDirectory("synchronizing");
                                path = "synchronizing\\"+path+".zip";

                                File.Copy(logFile, path, true);

                                //adiciona para a lista de pasta da resposta
                                pathList.Add(path);
                            }
                            else
                            {
                                Console.WriteLine("Arquivo Ignorado -> "+logFile);
                            }
                        }
                    }
                }
                return pathList;
            }

            private static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
            {
                // Get the subdirectories for the specified directory.
                DirectoryInfo dir = new DirectoryInfo(sourceDirName);

                if (!dir.Exists)
                {
                    throw new DirectoryNotFoundException(
                        "Source directory does not exist or could not be found: "
                        + sourceDirName);
                }

                DirectoryInfo[] dirs = dir.GetDirectories();
                // If the destination directory doesn't exist, create it.
                if (!Directory.Exists(destDirName))
                {
                    Directory.CreateDirectory(destDirName);
                }
                
                // Get the files in the directory and copy them to the new location.
                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo file in files)
                {
                    string temppath = Path.Combine(destDirName, file.Name);
                    file.CopyTo(temppath, false);
                }

                // If copying subdirectories, copy them and their contents to new location.
                if (copySubDirs)
                {
                    foreach (DirectoryInfo subdir in dirs)
                    {
                        string temppath = Path.Combine(destDirName, subdir.Name);
                        DirectoryCopy(subdir.FullName, temppath, copySubDirs);
                    }
                }
            }

            //extrai o arquivo e retorna o caminho completo do mesmo
            public string unzipLogFile(string fileName)
            {
                if(File.Exists(fileName))
                {
                    //abre o arquivo como stream
                    using (FileStream zipToOpen = new FileStream(fileName, FileMode.Open))
                    {
                        //abre o zip
                        using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update))
                        {
                            //navega entre os arquivos do zip
                            Console.Write("'o zip contem -> '");
                            foreach(var entry in archive.Entries)
                            {
                                Console.WriteLine(entry.FullName);  
                                //extrai caso seja um arquivo de log
                                if (entry.FullName.EndsWith(".log", StringComparison.OrdinalIgnoreCase))
                                {        
                                    Console.WriteLine("arquivo de log encontrado");
                                    //exclui o arquivo se ja existir
                                    if(File.Exists(this.tempFolder+"\\"+entry.FullName)) File.Delete(this.tempFolder+"\\"+entry.FullName);

                                    //extrai o arquivo para a pasta temporaria
                                    entry.ExtractToFile(this.tempFolder+"\\"+entry.FullName);
                                    return this.tempFolder+"\\"+entry.FullName;
                                }                       
                            }
                        }
                    }
                }
                else
                {
                    Console.WriteLine("O Arquivo referenciado nao existe");
                }
                return null;
            }

            public string getHeader(string fileName)
            {
                string header = "";

                //le informação das variaveis presentes
                using(StreamReader file = new StreamReader(fileName)) {  
                    string ln;  
            
                    //verifica se é um trecho do cabeçalho
                    while ((ln = file.ReadLine()) != null) {  
                        if(ln.Contains("1?"))
                        {
                            header += ln+"\r\n";
                        }
                    }  
                    file.Close();  
                }
            
                return header;
            }

            public bool clearPartFile()
            {
                try{
                    if(Directory.Exists(this.partFolder))
                    {
                        Directory.Delete(this.partFolder, true);
                        //cria diretorio logo apos
                        Directory.CreateDirectory(this.partFolder);
                    }
                    return true;
                }catch(Exception e)
                {
                    Console.WriteLine("Exceção ao dropar pasta de partes - "+e.ToString());
                    return false;
                }
            }
            //retorna a lista de arquivos criados a partir das horas
            public List<string> getHourSegments(string fileName, string __header)
            {
                List<string> segmentFiles = new List<string>();
                //procura trechos nos quais os arquivos serao separados
                List<int> date_marker = new List<int>();
                var timeMap = new Dictionary<string, int>(); //feito em string para marcar inicio e fim

                //recupera tamanho do arquivo
                long length = (new FileInfo(fileName)).Length;

                //faz uma hash do arquivo de log para nomear o arquivo
                string fileHash = "";
                using (MD5 md5Hash = MD5.Create())
                {
                    fileHash = BitConverter.ToString(md5Hash.ComputeHash(File.ReadAllBytes(fileName))).Replace("-","");
                }
                //limpa pasta de partes
                //clearPartFile();

                using (var mmf = MemoryMappedFile.CreateFromFile(fileName, FileMode.Open))
                {
                        //define se um marcador de data comecou
                        bool start = false;

                        //le de 2 em 2 bytes ate encontrar o marcador
                        using (var stream = mmf.CreateViewStream())                     
                        {   
                            for(int i=0; i<stream.Length-1; i+=2)
                            {
                                byte[] buff = new byte[2];                                
                                stream.Read(buff, 0, 2);

                                if(((char) buff[0] == '2') && ((char) buff[1] == '?'))
                                {
                                    date_marker.Add(i);
                                    start = true;
                                }
                                else if((((char) buff[0] == '\n') && ((char) buff[1] == '?')) && (start == true))
                                {
                                    date_marker.Add(i*-1);
                                    start = false;
                                }
                            }
                        }

                        //agora faz a leitura das datas com inicio de dados e fim adicionando ao um dicionario de datas e locais
                        for(int i=0; i<date_marker.Count-2;i+=2)
                        {
                            int accessorLength = Math.Abs(date_marker[i+1])-date_marker[i];

                            //cria um acessor para ler os locais corretods do arquivo
                            using (var accessor = mmf.CreateViewAccessor(date_marker[i], accessorLength))
                            {
                                //busca data do evento
                                string line = "";
                                char val;

                                for(int l=0;l<accessorLength;l++)
                                {
                                    val = (char) accessor.ReadByte(l);
                                    line += val.ToString();
                                }

                                //retira quebras de linah do dicionario
                                line = line.Replace("\r","").Replace("\n","");

                                //adiciona data ao dicionario com sua localização
                                if(line.Length >= 1) timeMap[line] = date_marker[i];
                            }
                        }
                        Console.Write(" numero de marcadores no arquivo -> "+timeMap.Count.ToString());
                        Console.WriteLine();


                        //lista de lugares nos quas o arquivo é dividido
                        List<string> fileDivisions = new List<string>();
                        string lastTime = "";
                        int oldHour = 0;
                        var _item = "";

                        //agora faz um indice de datas e locais
                        foreach (var item in timeMap.Keys)
                        {
                            //grava item para uso no final
                            _item = item;

                            //verifica se o ultimo existe
                            if(lastTime.Length > 0)        
                            {
                                //transforma em data para fins de calculo
                                DateTime eventDate = DateTime.Parse(item.Replace("2?","").Split("^")[0]);

                                if(eventDate.Hour != oldHour)
                                {
                                    //adiciona ao divisor de arquivos
                                    fileDivisions.Add(timeMap[lastTime].ToString()+"-"+timeMap[item]);
                                    Console.Write("Adicionada divisao em -> ");
                                    Console.Write(eventDate);
                                    Console.Write(" -> from : ");
                                    Console.Write(timeMap[lastTime]);
                                    Console.Write(" -> to: ");
                                    Console.WriteLine(timeMap[item]);

                                    //seta hora anterior
                                    oldHour = eventDate.Hour;

                                    //seta last time
                                    lastTime = item;
                                }
                            }
                            else
                            {
                                lastTime = item;
                            }
                        }

                        //adiciona ultimo ponto para o fim do arquivo
                        fileDivisions.Add(timeMap[lastTime].ToString()+"-"+length.ToString());

                        //reescreve o arquivo no disco com as divisoes
                        for(int i=0; i<fileDivisions.Count ;i++)
                        {
                            string[] marker = fileDivisions[i].Split("-");

                            int accessorStart = int.Parse(marker[0]);
                            int accessorLength = int.Parse(marker[1])-int.Parse(marker[0]);

                            
                            Console.Write("Processando Parte -> ");
                            Console.Write(i);

                            Console.Write(" - Tamanho da Parte -> ");
                            Console.WriteLine(accessorLength);
                            
                            //faz um nome de arquivo para a parte
                            string partFileName = fileHash+i.ToString("00")+".zip";
                            partFileName = partFolder+'\\'+partFileName;    
                            segmentFiles.Add(partFileName);                        

                            //cria um acessor para ler os locais corretods do arquivo
                            using (var viewStream = mmf.CreateViewStream(accessorStart, accessorLength, MemoryMappedFileAccess.Read))
                            {
                                using (BinaryReader binReader = new BinaryReader(viewStream))
                                {
                                    //cria array com o tamanho dos dois elementos a seren concatenados
                                    byte[] rv = new byte[Encoding.GetEncoding("ISO-8859-1").GetBytes(__header).Length + viewStream.Length];

                                    //concatena os dois arrays de bytes para escrever no arquivo
                                    //public static void BlockCopy (Array src, int srcOffset, Array dst, int dstOffset, int count);
                                    System.Buffer.BlockCopy(Encoding.GetEncoding("ISO-8859-1").GetBytes(__header), 0, rv, 0, Encoding.GetEncoding("ISO-8859-1").GetBytes(__header).Length);
                                    System.Buffer.BlockCopy(binReader.ReadBytes((int)viewStream.Length), 0, rv, Encoding.GetEncoding("ISO-8859-1").GetBytes(__header).Length, (int)viewStream.Length);

                                    // Encrypt the file to an array of bytes.
                                    using (Aes myAes = Aes.Create())
                                    {
                                        using (MD5 md5Hash = MD5.Create())
                                        {
                                            //cria um zip do arquivo
                                            using (FileStream zipToOpen = new FileStream(partFileName, FileMode.Create))
                                            {
                                                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
                                                {
                                                    ZipArchiveEntry readmeEntry = archive.CreateEntry("file.log");
                                                    using (StreamWriter writer = new StreamWriter(readmeEntry.Open()))
                                                    {
                                                            writer.Write(Encoding.GetEncoding("ISO-8859-1").GetString(rv));
                                                    }
                                                }
                                            }

                                            //criptografia aes -- adiada
                                            /*
                                            //define um valor de chave para o aes
                                            byte[] key = md5Hash.ComputeHash(Encoding.GetEncoding("ISO-8859-1").GetBytes(this.cryptoKey));

                                            myAes.Key = key;
                                            myAes.IV = key;

                                            // Encrypt the string to an array of bytes.
                                            byte[] encrypted = EncryptStringToBytes_Aes(Encoding.GetEncoding("ISO-8859-1").GetString(rv), myAes.Key, myAes.IV);

                                            string outStringBase64 = Convert.ToBase64String(encrypted);

                                            //escreve bytes no arquivo
                                            //File.WriteAllText(partFileName, outStringBase64);
                                            File.WriteAllText(partFileName, Encoding.GetEncoding("ISO-8859-1").GetString(rv));
                                            */
                                        }    
                                    }
                                }
                            }
                        }
                }

                return segmentFiles;
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

    public class conectionProcedures:DeviceconectionProcedures{
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
        public conectionProcedures(string _deviceMac, string _machineId, string _apn, string _user, string _pwd, SerialPort _serialPort)
        {
            this.apn = _apn;
            this.user = _user;
            this.pwd = _pwd;
            this.serial = _serialPort;
            this.mode = 1;
            this.deviceMac = _deviceMac;
            this.machineId = _machineId;
            this.ftpFileDir = ftpFileDir = this.deviceMac+"/"+this.machineId;
        }   
        public conectionProcedures(string _deviceMac, string _machineId, string _apn, string _user, string _pwd, Socket _socket)
        {
            this.apn = _apn;
            this.user = _user;
            this.pwd = _pwd;
            this.socket = _socket;
            this.mode = 0;
            this.deviceMac = _deviceMac;
            this.machineId = _machineId;
            this.ftpFileDir = ftpFileDir = this.deviceMac+"/"+this.machineId;
        }    
        
        public override List<Dictionary<string, string>> listSSID(int allowedErrors=3)
        {
            return new List<Dictionary<string, string>>();
        }
/*
        public string findDevice()
        {
            for(int port = 0; port < 99; port++)
            {
                Console.WriteLine("Procurando portas de comunicação do device");

                SerialPort serial = new SerialPort("COM"+port.ToString(), 115200);
                serial.ReadTimeout = 500;
                serial.WriteTimeout = 500;
                try
                {
                    serial.Open();
                    serial.Write("\r\nat\r\n");
                    string response = "";
                    do{
                     response += serial.ReadLine();
                    }while((!response.Contains("OK")) && (!response.Contains("ERROR")));
                    serial.Close();
                    return "COM"+port.ToString();
                }
                catch(Exception e)
                {
                    Console.Write("Porta "+port.ToString()+" Indisponivel -> ");
                    Console.WriteLine(e.Message);
                }
            }

            return "0";
        }
        */
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
                    a:
                    if(errors > allowedErrors)  return false;
                    serial.Write("\r\nATZ\r\n");
                    incoming = "";
                    do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));
                    Console.WriteLine("ATZ");
                    Console.WriteLine(incoming);
                    if(! incoming.Contains("OK")){Thread.Sleep(1000); errors++; goto a;}
                                        
                    DesactivateUPSDAConfig:
                    if(errors > allowedErrors)  return false;
                    serial.Write("\r\nAT+UPSDA=0,4\r\n");
                    incoming = "";
                    do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));
                    Console.WriteLine("AT+UPSDA=0,4");
                    Console.WriteLine(incoming);
                    if((!incoming.Contains("OK")) && (!incoming.Contains("ERROR"))){Thread.Sleep(1000); errors++; goto DesactivateUPSDAConfig;}

                    ResetUPSDAConfig:
                    if(errors > allowedErrors)  return false;
                    serial.Write("\r\nAT+UPSDA=0,0\r\n");
                    incoming = "";
                    do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));
                    Console.WriteLine("AT+UPSDA=0,0");
                    Console.WriteLine(incoming);
                    if(! incoming.Contains("OK")){Thread.Sleep(1000); errors++; goto ResetUPSDAConfig;}

                    //reseta toda configuracao de rede
                    enable_errorreport:
                    if(errors > allowedErrors)  return false;
                    serial.Write("\r\nAT+CMEE=2\r\n");
                    incoming = "";
                    do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));
                    Console.WriteLine("AT+CMEE=2");
                    Console.WriteLine(incoming);                
                    if(! incoming.Contains("OK")){Thread.Sleep(1000); errors++; goto enable_errorreport;}
                    

                    //reseta toda configuracao de rede
                    disable_echoes:
                    if(errors > allowedErrors)  return false;
                    serial.Write("\r\nATE0\r\n");
                    incoming = "";
                    do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));
                    Console.WriteLine("ATE0");
                    Console.WriteLine(incoming);                
                    if(! incoming.Contains("OK")){Thread.Sleep(1000); errors++; goto disable_echoes;}
                    

                    disable_cgact:
                    if(errors > allowedErrors)  return false;
                    serial.Write("\r\nAT+CGACT=0,1\r\n");
                    incoming = "";
                    do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));
                    Console.WriteLine("AT+CGACT=0,1");
                    Console.WriteLine(incoming);
                    if(! incoming.Contains("OK")){Thread.Sleep(1000); errors++; goto disable_cgact;}
                    

                    c:
                    if(errors > allowedErrors)  return false;
                    serial.Write("\r\nAT+UPSND=0,8\r\n");
                    incoming = "";
                    do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));
                    Console.WriteLine("AT+UPSND=0,8");
                    Console.WriteLine(incoming);
                    if(! incoming.Contains("OK")){Thread.Sleep(1000); errors++; goto c;}
                    

                    //not allowed é que ja esta feita a configuração
                    d:
                    if(errors > allowedErrors)  return false;
                    serial.Write("\r\nAT+UPSD=0,1,\""+this.apn+"\"\r\n");
                    incoming = "";
                    do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));
                    Console.WriteLine("AT+UPSD=0,1,\""+this.apn+"\"");
                    Console.WriteLine(incoming);
                    if((!incoming.Contains("OK")) && (!incoming.Contains("not allowed"))){Thread.Sleep(1000); errors++; goto d;}
                    

                    e:
                    if(errors > allowedErrors)  return false;
                    serial.Write("\r\nAT+UPSD=0,2,\""+this.user+"\"\r\n");
                    incoming = "";
                    do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));
                    Console.WriteLine("AT+UPSD=0,2,\""+this.user+"\"");
                    Console.WriteLine(incoming);
                    if((!incoming.Contains("OK")) && (!incoming.Contains("not allowed"))){Thread.Sleep(1000); errors++; goto e;}
                    

                    f:
                    if(errors > allowedErrors)  return false;
                    serial.Write("\r\nAT+UPSD=0,3,\""+this.pwd+"\"\r\n");
                    incoming = "";
                    do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));
                    Console.WriteLine("AT+UPSD=0,3,\""+this.pwd+"\"");
                    Console.WriteLine(incoming);
                    if((!incoming.Contains("OK")) && (!incoming.Contains("not allowed"))){Thread.Sleep(1000); errors++; goto f;}
                    

                    dns1:
                    if(errors > allowedErrors)  return false;
                    serial.Write("\r\nAT+UPSD=0,4,\"1.1.1.1\"\r\n");
                    incoming = "";
                    do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));
                    Console.WriteLine("AT+UPSD=0,4,\"1.1.1.1\"");
                    Console.WriteLine(incoming);
                    if((!incoming.Contains("OK")) && (!incoming.Contains("not allowed"))){Thread.Sleep(1000); errors++; goto dns1;}
                    

                    dns2:
                    if(errors > allowedErrors)  return false;
                    serial.Write("\r\nAT+UPSD=0,5,\"8.8.8.8\"\r\n");
                    incoming = "";
                    do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));
                    Console.WriteLine("AT+UPSD=0,5,\"8.8.8.8\"");
                    Console.WriteLine(incoming);
                    if((!incoming.Contains("OK")) && (!incoming.Contains("not allowed"))){Thread.Sleep(1000); errors++; goto dns2;}
                    
                    auth:
                    if(errors > allowedErrors)  return false;
                    serial.Write("\r\nAT+UPSD=0,6,3\r\n");
                    incoming = "";
                    do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));
                    Console.WriteLine("AT+UPSD=0,6,3");
                    Console.WriteLine(incoming);
                    if((!incoming.Contains("OK")) && (!incoming.Contains("not allowed"))){Thread.Sleep(1000); errors++; goto auth;}
                    
                    g:
                    if(errors > allowedErrors)  return false;
                    serial.Write("\r\nAT+UPSD=0,7,\"0.0.0.0\"\r\n");
                    incoming = "";
                    do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));
                    Console.WriteLine("AT+UPSD=0,7,\"0.0.0.0\"");
                    Console.WriteLine(incoming);
                    if((!incoming.Contains("OK")) && (!incoming.Contains("not allowed"))){Thread.Sleep(1000); errors++; goto g;}
                    

                    h:
                    if(errors > allowedErrors)  return false;
                    serial.Write("\r\nAT+UPSDA=0,1\r\n");
                    incoming = "";
                    do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));
                    Console.WriteLine("AT+UPSDA=0,1");
                    Console.WriteLine(incoming);
                    if(! incoming.Contains("OK")){Thread.Sleep(1000); errors++; goto h;}
                    

                    i:
                    if(errors > allowedErrors)  return false;
                    serial.Write("\r\nAT+UPSDA=0,3\r\n");
                    incoming = "";
                    do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));
                    Console.WriteLine("AT+UPSDA=0,3");
                    Console.WriteLine(incoming);
                    if((!incoming.Contains("OK")) && (!incoming.Contains("not allowed"))){Thread.Sleep(1000); errors++; goto i;}
                    

                    j:
                    if(errors > allowedErrors)  return false;
                    serial.Write("\r\nAT+UPSND=0,8\r\n");
                    incoming = "";
                    do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));
                    Console.WriteLine("AT+UPSDA=0,8");
                    Console.WriteLine(incoming);
                    if((!incoming.Contains("OK")) && (!incoming.Contains("not allowed"))){Thread.Sleep(1000); errors++; goto j;}
                    

                    k:
                    if(errors > allowedErrors)  return false;
                    serial.Write("\r\nAT+UPSND=0,0\r\n");
                    String ipAddress = serial.ReadLine();
                    incoming = "";
                    do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));
                    Console.WriteLine("AT+UPSND=0,0");
                    Console.WriteLine(incoming.Split(",")[2]);
                    if(! incoming.Contains("OK")){Thread.Sleep(1000); errors++; goto k;}

                    //registra ip do device
                    ipAddress = incoming.Split(",")[2].Replace("OK","");
                    /*
                    b:
                    if(errors > allowedErrors)  return false;
                    serial.Write("\r\nAT+CGACT=1,1\r\n");
                    incoming = "";
                    do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));
                    Console.WriteLine("AT+CGACT=1,1");
                    Console.WriteLine(incoming);
                    if(! incoming.Contains("OK")){Thread.Sleep(1000); errors++; goto b;}
                    */
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
            try{
                string incoming="";
                serial.Write("\r\n");
                serial.DiscardInBuffer();
                serial.Write("AT+UPSND=0,0\r\n");
                String ipAddress = serial.ReadLine();
                incoming = "";
                do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));

                Console.WriteLine(incoming);

                if(Regex.IsMatch(incoming, "\\+UPSND\\: \\d+,\\d+,\"\\d+\\.\\d+\\.\\d+\\.\\d+\""))
                {
                    
                    if(Regex.IsMatch(incoming, "\\+UPSND\\: \\d+,\\d+,\"0\\.0\\.0\\.0\""))
                    {
                        Console.WriteLine("Conexao nao adiquiriu um ip");
                        return false;
                    }
                    Console.WriteLine("Im Flying");
                    Console.WriteLine("Connectado com sucesso");
                    return true;
                }
                
                Console.WriteLine("Conexao nao esta ativa");
                return false;
                /*
                if((incoming.Contains("OK")) && (!incoming.Contains("ERROR"))) return true;
                else return false;
                */
            }
            catch(TimeoutException)
            {
                Console.WriteLine("Falha ao verificar estado da conexao");
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
        }
        public bool ftpIsConnected()
        {
            int retry = 3;
            _retry:
            try{
                serial.Write("\r\nat+uftpc=14\r\n");
                string incoming = "";
                do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("+UUFTPCR: 14,") && !incoming.Contains("ERROR"));
                if(incoming.Contains("+UUFTPCR: 14,1")) return true;
                
                return false;
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
        }
        public bool startFtpClient(string _domain, string _usr, string _pwd)
        {
            int retry = 3;
            _retry:
            try{
                string incoming = "";
                int errors = 0;
                int allowedErrors = 3;

                //procedimentos orientados a serial
                if(mode == 1)
                {

                    logout:
                    if(errors > allowedErrors)  return false;
                    serial.Write("\r\nat+uftpc=0\r\n");
                    incoming = "";
                    do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));
                    Console.WriteLine("Doing Login");
                    Console.WriteLine(incoming);
                    if(! incoming.Contains("OK")){Thread.Sleep(1000); errors++; goto logout;}

                    a:
                    if(errors > allowedErrors)  return false;
                    serial.Write("\r\nat+uftp=1,\""+_domain+"\"\r\n");
                    incoming = "";
                    do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));
                    Console.WriteLine("Seting Domain");
                    Console.WriteLine(incoming);
                    if(! incoming.Contains("OK")){Thread.Sleep(1000); errors++; goto a;}

                    b:
                    if(errors > allowedErrors)  return false;
                    serial.Write("\r\nat+uftp=2,\""+_usr+"\"\r\n");
                    incoming = "";
                    do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));
                    Console.WriteLine("Seting UserName");
                    Console.WriteLine(incoming);
                    if(! incoming.Contains("OK")){Thread.Sleep(1000); errors++; goto b;}

                    c:
                    if(errors > allowedErrors)  return false;
                    serial.Write("\r\nat+uftp=3,\""+_pwd+"\"\r\n");
                    incoming = "";
                    do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));
                    Console.WriteLine("Seting User Password");
                    Console.WriteLine(incoming);
                    if(! incoming.Contains("OK")){Thread.Sleep(1000); errors++; goto c;}

                    d:
                    if(errors > allowedErrors)  return false;
                    serial.Write("\r\nat+uftp=6,0\r\n");
                    incoming = "";
                    do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));
                    Console.WriteLine("Seting ConType");
                    Console.WriteLine(incoming);
                    if(! incoming.Contains("OK")){Thread.Sleep(1000); errors++; goto d;}

                    e:
                    if(errors > allowedErrors)  return false;
                    serial.Write("\r\nat+uftpc=1\r\n");
                    incoming = "";
                    do{incoming += serial.ReadLine().Replace("\r","").Replace("\n",""); Console.WriteLine(incoming);} while(!incoming.Contains("+UUFTPCR") && !incoming.Contains("ERROR"));
                    Console.WriteLine("Doing Login");
                    Console.WriteLine(incoming);
                    if(! incoming.Contains("+UUFTPCR")){Thread.Sleep(1000); errors++; goto e;}

                    if(incoming.Contains("+UUFTPCR: 1,0")){
                        Console.WriteLine("Ftp Login Fail");
                        return false;
                    }
                    else if(incoming.Contains("+UUFTPCR: 1,1")){
                        Console.WriteLine("Ftp Login Successfully");
                        Console.WriteLine("FileList");

                        
                        if(errors > allowedErrors)  return false;
                        serial.Write("\r\nat+uftpc=14\r\n");
                        incoming = "";
                        do{incoming += serial.ReadLine().Replace("\r","|").Replace("\n","|");} while(!incoming.Contains("+UUFTPCR") && !incoming.Contains("ERROR"));
                        Console.WriteLine("Doing Login");
                        Console.WriteLine(incoming);
                        if(! incoming.Contains("+UUFTPCR")){Thread.Sleep(1000); errors++;}
                    }
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
                    Console.WriteLine("Thre Serial Port is Closed");
                return false;
            }
        }

        public List<Dictionary<string,string>> getFtpFileDataList()
        {
            int retry = 3;
            _retry:

            //drwxr-xr-x   2 cemi01   cemi           96 Feb 19 17:44 .
            List<Dictionary<string,string>> fileDict = new List<Dictionary<string,string>>();
            Dictionary<string,string> properties = new Dictionary<string, string>();
            List<String> fileList = new List<string>();

            try{
                string incoming = "";
                int errors = 0;
                int allowedErrors = 3;
                
                //define diretorio de envio de acordo com preferencia do usuario
                a:
                if(errors > allowedErrors)  {Console.WriteLine("Limite de Tentativas alcançado em at+utftpc=8"); return fileDict;}
                serial.Write("\r\nat+uftpc=8,\"/\"\r\n");
                incoming = "";
                do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("+UUFTPCR: 8,1") && !incoming.Contains("ERROR"));
                if(! incoming.Contains("OK")){Thread.Sleep(1000); errors++; goto a;}
                    
                //exclui arquivo caso exista
                b:
                if(errors > allowedErrors)  {Console.WriteLine("Limite de Tentativas alcançado em at+utftpc=13"); return fileDict;}
                Console.WriteLine("\r\nAT+UFTPC=13,\""+this.ftpFileDir+"\"\r\n");
                serial.Write("\r\nAT+UFTPC=13,\""+this.ftpFileDir+"\"\r\n");
                incoming = "";
                do{incoming += serial.ReadLine().Replace("\r","|").Replace("\n","");} while(!incoming.Contains("+UUFTPCR: 13,") && !incoming.Contains("ERROR"));
                
                //retorna lista vazia indicando que o caminho nao existe
                if(!incoming.Contains("+UUFTPCR: 13,1")){Thread.Sleep(1000); errors++; goto b;}

                //retira lixo da mensagem lembrando que \n foi substituido por |
                incoming = Regex.Replace(incoming, @"(\|\|\+UUFTPCD:\s\d+,\d+,)", "", RegexOptions.IgnoreCase|RegexOptions.Multiline);
                string ftpFileList = incoming.Substring(incoming.IndexOf("\""), (incoming.LastIndexOf("\"")-incoming.IndexOf("\"")));

                fileList = new List<string>(ftpFileList.Replace("\"","").Split("|"));
                
                //divide propriedades em um dicionario de valores
                Console.WriteLine("estruturando arquivos");
                for(int i=0; i<fileList.Count; i++)
                {

                    string[] prop = Regex.Split(fileList[i], @"\s+");

                    //divide propriedades em um dicionario de valores
                    if(prop.Length >= 8)
                    {
                        properties["permission"] = prop[0];
                        properties["content"] = prop[1];
                        properties["user"] = prop[2];
                        properties["group"] = prop[3];
                        properties["size"] = prop[4];
                        properties["dateMonth"] = prop[5];
                        properties["dateDay"] = prop[6];
                        properties["time"] = prop[7];
                        
                        //concatena resto como nome do arquivo
                        properties["name"] = "";
                        for(int r=8; r<prop.Length; r++)
                        {
                            properties["name"] += prop[r]+" ";
                        }
                        
                        fileDict.Add(properties);
                    }
                }
                return fileDict;
            }
            catch(TimeoutException)
            {
                if(retry <= 0)  return fileDict;
                retry--;
                goto _retry;
            }
            catch(Exception except)
            {
                if((except is InvalidOperationException) || (except is OperationCanceledException))
                    Console.WriteLine("Thre Serial Port is Closed");
                return fileDict;
            }
        }

        /*******************************Faz Requisicoes sock*******************************/
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

            //resolve dns
            int dnsTrys = 3;
            __getServerIp:
            string serverIp = (string) sendPacket("AT+UDNSRN=0,\""+url+"\"",  ".*\\+UDNSRN\\:.*\\\"(.*)\\\"");
            if(serverIp == null)
            {
                //retorna -2 caso de erro de dns
                if(dnsTrys <=0) return -2;
                dnsTrys --;
                Thread.Sleep(1000);
                goto __getServerIp;
            }

            //cria socket e recupera seu numero         +USOCR: 0     
            string socketNumber = (string) sendPacket("AT+USOCR=6",  ".*\\+USOCR:\\s(\\d+)");

            Console.WriteLine("Socket aberto com numero -> " + socketNumber);

            //slicita conexao no ip e porta
            string connection = (string) sendPacket("AT+USOCO="+socketNumber+",\""+serverIp+"\","+hostPort, ".*(OK).*");
            if(connection != null)
                if(connection.Equals("OK"))
                    return int.Parse(socketNumber);
            
            return -1;
        }

        public override bool startContinuousSocket(int socketNumber)
        {
            /*
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

            string socketReady = (string) sendPacket("AT+USODL="+socketNumber.ToString(),  ".*(CONNECT).*");
            if(socketReady != null)
            {
                if(socketReady.Contains("CONNECT"))
                    return true;
            }
            */
            return true;
            return false;
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
/*
        public override string writeContinuousSocket(string data, string response = "\r\n", string token = "qazxc123", int __timeout = 60)
        {  
            //limpa buffers
            serial.DiscardInBuffer();

            string IV = "CemiProcessOptim";

            using (MD5 md5Hash = MD5.Create())
            {
                string Tokenhash = GetMd5Hash(md5Hash, token);

                Console.Write("RawData");
                Console.Write(data);                

                Console.Write("keyd -> " + Tokenhash);
                
                byte[] EncodedData = EncryptStringToBytes_Aes(data, Encoding.ASCII.GetBytes(Tokenhash), Encoding.ASCII.GetBytes(IV));
                data = Convert.ToBase64String(EncodedData);

                data = data+"\r\n";

                Console.Write("Encoded");
                Console.Write(data);

                serial.Write(data);

                string incoming = "";

                int timeout = __timeout;      //timeout de 1 minuto devico ao post
                string buff = "";
                do{
                    buff = serial.ReadExisting();
                    if(buff.Length >= 1)
                    {
                        incoming += buff;//.Replace("\r","").Replace("\n",""); 
                        buff = "";
                        timeout = __timeout;
                    }
                    else
                    {
                        timeout --;
                        Thread.Sleep(1000);
                    }
                    if(timeout <= 0)    break;
                } while(!Regex.Match(incoming, response,RegexOptions.Singleline).Success && !incoming.Contains("ERROR"));

                Match rec = Regex.Match(incoming, response,RegexOptions.Singleline);

                if(rec.Success) return rec.Value;
            }

            return null;
        }
        */
        public override string writeContinuousSocket(string data, string response = "\r\n", string token = "qazxc123", int __timeout = 60, int socket=0)
        {  
            int timeoutPerTryes = __timeout/10;

            //limpa buffers
            serial.DiscardInBuffer();
            //define que recebera os dados diretamente do servidor no ato da escrita
            //serial.WriteLine("AT+CIPRECVMODE=0");
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

                    Console.WriteLine("AT+USOWR=" + socket.ToString() + "," + ((i+MaxPacketSize) / data.Length < 1  ? MaxPacketSize : data.Length % MaxPacketSize).ToString()+"\r\n");
                    Console.WriteLine(data.Substring(i, ((i+MaxPacketSize) / data.Length ) < 1 ? MaxPacketSize : data.Length % MaxPacketSize));

                    //limpa buffers
                    serial.DiscardInBuffer();

                    //os pacotes devem ser enviados de 2048 em 2048 byte a cada 20ms
                    serial.Write("AT+USOWR=" + socket.ToString() + "," + ((i+MaxPacketSize) / data.Length < 1  ? MaxPacketSize : data.Length % MaxPacketSize).ToString()+"\r\n");
                    Thread.Sleep(10);
                        
                        //aguarda permissao de envio
                    __waitToSend:
                    buff = "";

                    buff += serial.ReadExisting();
                    if(buff.Length >= 1)
                    {    
                        Console.WriteLine("Waiting to send -> "+ buff);

                        if(!buff.Contains("@"))
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
                            Console.WriteLine("ready to Send");
                            buff = "";
                            serial.DiscardInBuffer();
                        }
                    }
                    else
                    {
                        if(_sendTry >= 10)
                        {
                            Console.WriteLine("Socket Writing TimedOut buffer stil clear");
                            return null;
                        }
                        _sendTry ++;
                        Thread.Sleep(timeoutPerTryes);
                        goto __waitToSend;
                    }
                    Thread.Sleep(100);
                    serial.DiscardInBuffer();
                    serial.Write(data.Substring(i, ((i+MaxPacketSize) / data.Length < 1 ? MaxPacketSize : data.Length % MaxPacketSize)));
                    Console.WriteLine(data.Substring(i, ((i+MaxPacketSize) / data.Length < 1 ? MaxPacketSize : data.Length % MaxPacketSize)));
                    Thread.Sleep(100);

                    Console.WriteLine("Enviado ");
                    
                    int recTryes = 0;
                    //if((i+MaxPacketSize) / data.Length < 1)
                   // {
                        Console.WriteLine("---Entrou---");

                        buff = "";
                        __waitToSendNext:
                        buff += serial.ReadExisting();
                        if(buff.Length >= 1)
                        {    
                            Console.WriteLine("aguardando para enviar o proximo pacote -> "+ buff);

                            if(!buff.Contains("+USOWR:"))
                            {
                                if(recTryes >= 10)
                                {
                                    Console.WriteLine("Socket Writing TimedOut");
                                    return null;
                                }
                                recTryes ++;
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
                            if(recTryes >= 10)
                            {
                                Console.WriteLine("Socket Writing TimedOut");
                                return null;
                            }
                            recTryes ++;
                            Thread.Sleep(timeoutPerTryes);
                            goto __waitToSendNext;                                
                        }  
                  //  } 
                }

                incoming = "";
                int timeout = 100;      //timeout de 1 minuto devico ao post
                //verifica se ja tem dados no buffer
                //if(!buff.Contains("+IPD"))  buff = "";
                //Aguarda 1 segundos para tentar o recebimento dos dados
                Thread.Sleep(1000);
                //lumpa buffer para enviar comando de recebimento
                buff = "";
                do{
                    serial.DiscardInBuffer();
                    Console.WriteLine("AT+USORD="+socket.ToString()+",1024\r\n"); 
                    serial.Write("AT+USORD="+socket.ToString()+",1024\r\n");      
                    Thread.Sleep(60);
                    __afterRequest:              
                    buff += serial.ReadExisting();
                    if(buff.Length >= 1)
                    {        
                        
                        //buff = buff.Replace("\r\nOK\r\n", "");
                        Console.Write("buffer incoming -> ");
                        Console.WriteLine(buff.Replace("\n", "<lf>").Replace("\r", "<cr>"));
                        
                        if(buff.Contains("+USORD: 0,0,\"\""))
                        {
                            buff = "";
                            if(timeout <= 0) break;
                            timeout --;
                            Thread.Sleep(timeoutPerTryes);
                            goto __afterRequest;
                        }
                        else
                        {
                            //define uma resposta para a requisicao do aithinker
                            string deviceResponsePatern = "\\+USORD:\\s\\d+,(\\d+),\"(.*)\"\r\n\r\nOK\r\n";
                            MatchCollection deviceResponses = Regex.Matches(buff, deviceResponsePatern, RegexOptions.Singleline);
                            foreach (Match deviceResponse in deviceResponses)
                            {
                                Console.WriteLine("Regex MATCHES");
                                
                                if(deviceResponse.Success)
                                {
                                    Console.WriteLine("Regex SUCCESS");
                                
                                    int length = int.Parse(deviceResponse.Groups[1].Value);
                                    Console.WriteLine("length -> "+length.ToString());
                                    if(length > 0)
                                    {
                                        incoming += deviceResponse.Groups[2].Value;
                                        
                                        Console.Write("matches incoming -> ");
                                        Console.WriteLine(incoming);
                                    }              
                                }   

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
                            _sendTry ++;
                            Thread.Sleep(timeoutPerTryes);
                            goto _ressend;
                        }    
                    } 
                } while(!Regex.Match(incoming, response,RegexOptions.Singleline).Success && !incoming.Contains("ERROR"));

                Console.Write("incoming end -> ");
                Console.WriteLine(incoming);

                //retorna valor definido no patern requerido
                Match rec = Regex.Match(incoming, response,RegexOptions.Singleline);

                //Environment.Exit(1);

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

        public bool writeSocket(int socketNumber, string _data)
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

            string socketReady = (string) sendPacket("AT+USOWR="+socketNumber.ToString()+","+_data.Length+"",  ".*(@).*");
            if(socketReady != null)
            {
                //+USOWR: 1,206
                string socketWrote = (string) sendPacket(_data, "(.*\\+USOWR\\:\\s\\d+,\\d+.*)", "", "");
                if(socketWrote != null)
                {
                    if(socketWrote.Contains("OK"))
                        return true;
                    else
                        return false;
                }
                else
                    return false;
            }

            return false;
        }

        public String readSocket(int socketNumber, string ending="\r\n")
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

            string socketReady = (string) sendPacket("AT+USORD="+socketNumber.ToString()+",1024",  "\\+USORD\\:\\s\\d+,\\d+,\"(.*)\"");
            return socketReady;
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
                    sendPacket("AT+USOCL="+i.ToString() ,  ".*(OK|ERROR).*");
            else
            {
                string closed = (string) sendPacket("AT+USOCL="+socketNumber.ToString() ,  ".*(OK).*");
                if(closed == null)    closed = "";
                if(closed == "OK")
                    return true;
            }
            return false;

        }

        /**********************************************************************************/

        /*******************************Faz Requisicoes http*******************************/
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
            /*
            if(data.Split("\r\n\r\n").Length >= 2)
                data = data.Split("\r\n\r\n")[1];
            */

            return data;
        }

        /**********************************************************************************/
        
        public bool findUpdate(string currentVersion)
        {
        ///
        ///     filename = arquivo para envio endereço completo
        ///     storeName = nome do arquivo no servidor
        ///     destination = pasta no servidor separada por /
        ///     

            string incoming = "";
            int errors = 0;
            int allowedErrors = 3;

            //procedimentos orientados a serial
            if(mode == 1)
            {
                //define diretorio de envio de acordo com preferencia do usuario
                int workingDirError = 3;

                a:
                if(errors > allowedErrors)  return false;
                serial.Write("\r\nat+uftpc=8,\"/upgrade\"\r\n");
                incoming = "";
                do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("+UUFTPCR: 8,") && !incoming.Contains("ERROR"));
                Console.WriteLine("Updater - Seting Working Directory");
                Console.WriteLine(incoming);
                if(! incoming.Contains("OK")){Thread.Sleep(1000); errors++; goto a;}

                if(incoming.Contains("+UUFTPCR: 8,0"))
                {
                    return false;
                }
                
                // Recupera pastas no ftp em busca de versoes mais novas
                   
                b:
                if(errors > allowedErrors)  {Console.WriteLine("Limite de Tentativas alcançado em at+utftpc=13"); return false;}
                Console.WriteLine("\r\nUpdater - AT+UFTPC=13\r\n");
                serial.Write("\r\nAT+UFTPC=13\r\n");
                incoming = "";
                do{incoming += serial.ReadLine().Replace("\r","|").Replace("\n","");} while(!incoming.Contains("+UUFTPCR: 13,") && !incoming.Contains("ERROR"));
                
                //retorna lista vazia indicando que o caminho nao existe
                if(!incoming.Contains("+UUFTPCR: 13,1")){Thread.Sleep(1000); errors++; goto b;}

                //retira lixo da mensagem lembrando que \n foi substituido por |
                incoming = Regex.Replace(incoming, @"(\|\|\+UUFTPCD:\s\d+,\d+,)", "", RegexOptions.IgnoreCase|RegexOptions.Multiline);
                string ftpFileList = incoming.Substring(incoming.IndexOf("\""), (incoming.LastIndexOf("\"")-incoming.IndexOf("\"")));

                //Cria Variaveis que receberão os dados
                List<string> fileList = new List<string>(ftpFileList.Replace("\"","").Split("|"));
                List<Dictionary<string, string>> files = new List<Dictionary<string, string>>();

                //divide propriedades em um dicionario de valores
                Console.WriteLine("estruturando arquivos");
                for(int i=0; i<fileList.Count; i++)
                {

                    string[] prop = Regex.Split(fileList[i], @"\s+");
                    Dictionary<string, string> properties = new Dictionary<string, string>();

                    //divide propriedades em um dicionario de valores
                    if(prop.Length >= 8)
                    {
                        properties["permission"] = prop[0];
                        properties["content"] = prop[1];
                        properties["user"] = prop[2];
                        properties["group"] = prop[3];
                        properties["size"] = prop[4];
                        properties["dateMonth"] = prop[5];
                        properties["dateDay"] = prop[6];
                        properties["time"] = prop[7];
                        
                        //concatena resto como nome do arquivo
                        properties["name"] = "";
                        for(int r=8; r<prop.Length; r++)
                        {
                            properties["name"] += prop[r]+" ";
                        }
                        files.Add(properties);
                    }
                }
                
                //transforma versao atual em inteiro
                int cv = int.Parse(currentVersion.Replace(".",""));
                string versionToDowload = "0";

                //verifica arquivo por arquivo para ver possibilidade de update buscando a versão mais avançada online pelo nome da pasta
                for(int i=0; i< files.Count; i++)
                {
                    //verifica se é uma pasta
                    if(files[i]["content"] == "2")
                    {
                        //verifica se versao é maior que a atual
                        int onlineVersion = 0;
                        int.TryParse(files[i]["name"].Replace(".", ""), out onlineVersion);

                        if(onlineVersion > cv)
                        {
                            Console.WriteLine("Updater - Versão superior encontrada!!");
                            
                            //verifica se é a maior versão presente
                            if(int.Parse(versionToDowload.Replace(".", "")) < onlineVersion)
                            {
                                versionToDowload = files[i]["name"];
                            }
                        }
                    }
                }

                //retira espaços ao final do caminho
                while(versionToDowload.EndsWith(" "))
                {
                    //retira ultimo caractere
                    versionToDowload = versionToDowload.Substring(0, versionToDowload.Length-1);
                }
                Console.WriteLine("Ultima Versão Disponivel");
                Console.WriteLine(versionToDowload);

                //define a pasta da versao como working directory
                c:
                if(errors > allowedErrors)  return false;
                serial.Write("\r\nat+uftpc=8,\"/upgrade/"+versionToDowload+"\"\r\n");
                Console.Write("\r\nat+uftpc=8,\"/upgrade/"+versionToDowload+"\"\r\n");
                incoming = "";
                do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("+UUFTPCR: 8,") && !incoming.Contains("ERROR"));
                Console.WriteLine("Updater - Seting Update Directory");
                Console.WriteLine(incoming);
                if(! incoming.Contains("OK")){Thread.Sleep(1000); errors++; goto c;}

                if(incoming.Contains("+UUFTPCR: 8,0"))
                {
                    return false;
                }

                d:
                if(errors > allowedErrors)  {Console.WriteLine("Limite de Tentativas alcançado em at+utftpc=13"); return false;}
                Console.WriteLine("\r\nUpdater - AT+UFTPC=13\r\n");
                serial.Write("\r\nAT+UFTPC=13\r\n");
                incoming = "";
                do{incoming += serial.ReadLine().Replace("\r","|").Replace("\n","");} while(!incoming.Contains("+UUFTPCR: 13,") && !incoming.Contains("ERROR"));
                
                //retorna lista vazia indicando que o caminho nao existe
                if(!incoming.Contains("+UUFTPCR: 13,1")){Thread.Sleep(1000); errors++; goto d;}

                //retira lixo da mensagem lembrando que \n foi substituido por |
                incoming = Regex.Replace(incoming, @"(\|\|\+UUFTPCD:\s\d+,\d+,)", "", RegexOptions.IgnoreCase|RegexOptions.Multiline);
                ftpFileList = incoming.Substring(incoming.IndexOf("\""), (incoming.LastIndexOf("\"")-incoming.IndexOf("\"")));

                //Cria Variaveis que receberão os dados
                fileList = new List<string>(ftpFileList.Replace("\"","").Split("|"));
                files = new List<Dictionary<string, string>>();

                //divide propriedades em um dicionario de valores
                Console.WriteLine("estruturando arquivos");
                for(int i=0; i<fileList.Count; i++)
                {

                    string[] prop = Regex.Split(fileList[i], @"\s+");
                    Dictionary<string, string> properties = new Dictionary<string, string>();

                    //divide propriedades em um dicionario de valores
                    if(prop.Length >= 8)
                    {
                        properties["permission"] = prop[0];
                        properties["content"] = prop[1];
                        properties["user"] = prop[2];
                        properties["group"] = prop[3];
                        properties["size"] = prop[4];
                        properties["dateMonth"] = prop[5];
                        properties["dateDay"] = prop[6];
                        properties["time"] = prop[7];
                        
                        //concatena resto como nome do arquivo
                        properties["name"] = "";
                        for(int r=8; r<prop.Length; r++)
                        {
                            properties["name"] += prop[r]+" ";
                        }
                        
                        //retira espaços ao final do caminho
                        while(properties["name"].EndsWith(" "))
                        {
                            //retira ultimo caractere
                            properties["name"] = properties["name"].Substring(0, properties["name"].Length-1);
                        }
                        
                        //filtra para arquivos somente excluindo pastas
                        if((properties["content"] == "1") && (properties["name"].EndsWith(".zip")))
                        {
                            files.Add(properties);
                        }
                    }
                }
                
                for(int i = 0; i< files.Count; i++)
                {
                    long startFrom = 0;

                    Console.Write("Arquivo Encontrado -> ");
                    Console.WriteLine(files[i]["name"]);
                    Console.WriteLine(files[i]["content"]);

                    //inicia download do arquivo
                    if(!Directory.Exists("upgrade"))    Directory.CreateDirectory("upgrade");
                    if(!Directory.Exists(@"upgrade\"+versionToDowload))    Directory.CreateDirectory(@"upgrade\"+versionToDowload);

                    //define endereços
                    string updatePath = @"upgrade\"+versionToDowload;
                    string updateFileName = @"upgrade\"+versionToDowload+@"\"+files[i]["name"];
                    if(File.Exists(updateFileName)) startFrom = new FileInfo(updateFileName).Length;
                    
                    Console.Write("Updater -> Iniciando Download a Partir de -> ");
                    Console.Write(startFrom);
                    Console.Write(" de um total de -> ");
                    Console.WriteLine(files[i]["size"]);

                    if(startFrom < int.Parse(files[i]["size"]))
                    {
                        //inicia o download do arquivo de atualização
                        if(errors > allowedErrors)  return false;
                        serial.Write("\r\nAT+UFTPC=6,\""+@"/upgrade/"+versionToDowload+@"/"+files[i]["name"]+"\", "+startFrom.ToString()+"\r\n");
                        Console.WriteLine("\r\nAT+UFTPC=6,\""+@"/upgrade/"+versionToDowload+@"/"+files[i]["name"]+"\", "+startFrom.ToString()+"\r\n");
                        incoming = "";
                        do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("CONNECT") && !incoming.Contains("ERROR"));
                        Console.WriteLine("Seting Send");
                        Console.WriteLine(incoming);
                        if(incoming.Contains("error"))  {Thread.Sleep(1000); errors++; goto a;}
                        else if(incoming.Contains("CONNECT"))
                        {
                            FileStream writer;

                            if(!File.Exists(updateFileName))
                            {
                                writer = File.Open(updateFileName, FileMode.CreateNew);
                                writer.Close();
                            }
                                
                            writer= File.Open(updateFileName, FileMode.Append);
                            byte[] lastTeen = new byte[20];
                            byte[] buff = new byte[4096];

                            try{
                                while(true)
                                {
                                //reseta buffer por garantia    10 k de buffer
                                    int received = 0;
                                    buff = new Byte[4096];

                                    received = serial.Read(buff, 0, buff.Length);

                                    //corta os ultimos 10 para 9 adicionando novo bytae ao fim para capturar disconnect
                                    if(received > lastTeen.Length)
                                        Array.Copy(buff,received-lastTeen.Length, lastTeen,0,lastTeen.Length);

                                    //Console.WriteLine(UTF8Encoding.Default.GetString(lastTeen));
                                    //escreve no arquivo
                                    if(!UTF8Encoding.Default.GetString(lastTeen).Contains("DISCONNECT"))
                                        writer.Write(buff, 0, received);
                                    else
                                    {
                                        //sai do loop while cortnando no ultimo apareces do DISCONNECT
                                        writer.Write(buff, 0, UTF8Encoding.Default.GetString(buff).IndexOf("DISCONNECT"));
                                        if(UTF8Encoding.Default.GetString(lastTeen).Contains("DISCONNECT"))    break;
                                    }
                                }
                                writer.Close();
                            }catch(TimeoutException)
                            {
                                writer.Close();
                            }
                            catch(Exception e)
                            {
                                Console.WriteLine("Exceção desconhecida -> "+e.Message);
                            }
                            //Download Finalizado
                        }
                        Console.WriteLine("Download Finalizado");

                        //extrai arquivos para a pasta de update
                        try{
                            //Verifica se diretorio existe antes de extrair o arquivo
                            if(Directory.Exists(updatePath+@"\temp"))
                                Directory.Delete(updatePath+@"\temp", true);


                            ZipFile.ExtractToDirectory(updateFileName, updatePath+@"\temp");
                            
                            // Get the current process.
                            System.Diagnostics.Process currentProcess = System.Diagnostics.Process.GetCurrentProcess();
                            string id = currentProcess.Id.ToString();
                            string processPath = currentProcess.MainModule.FileName;

                            Console.WriteLine("Proccess ID");
                            Console.WriteLine(id);
                            Console.WriteLine("Process Path");
                            Console.WriteLine(processPath);
                            Console.WriteLine("Update Path");
                            Console.WriteLine(updatePath+@"\temp");

                            //separa so em caminho da pasta
                            processPath = processPath.Substring(0, processPath.LastIndexOf("\\"));

                            
                            Console.WriteLine("Executando Updater");
                            Console.WriteLine("Updater.exe -pid "+id+" -pathfrom \""+updatePath+"\\temp\""+" -pathto \""+processPath+"\"");

                            ProcessStartInfo startInfo = new ProcessStartInfo("Updater.exe ", "-pid "+id+" -pathfrom \""+updatePath+"\\temp\""+" -pathto \""+processPath+"\"");
                            startInfo.UseShellExecute = true;//This should not block your program
                            Process updater = Process.Start(startInfo);
                            
                            Console.WriteLine("Updater Exit Code");
                            Console.WriteLine(updater.ExitCode);
                        }
                        catch(Exception e)
                        {
                            Console.WriteLine("Updater - Erro ao Extrair o Arquivo Zip -> "+e.Message);
                        }
                    }
                    else
                    {
                        Console.WriteLine("Arquivo Ja Presente em Disco");
                    }
                }

                //verifica conteudo da pasta pra download

            }
            else
            {
                return false;
            }
            
            return true;
        }
        public bool sendFtpFile(string fileName, string storeName, string destination, long fromOffset)
        {
        ///
        ///     filename = arquivo para envio endereço completo
        ///     storeName = nome do arquivo no servidor
        ///     destination = pasta no servidor separada por /
        ///     

            string incoming = "";
            int errors = 0;
            int allowedErrors = 3;
            long fileSize = new FileInfo(fileName).Length;

            //procedimentos orientados a serial
            if(mode == 1)
            {
                //define diretorio de envio de acordo com preferencia do usuario
                a:
                if(errors > allowedErrors)  return false;
                serial.Write("\r\nat+uftpc=8,\""+destination+"\"\r\n");
                incoming = "";
                do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("+UUFTPCR: 8,") && !incoming.Contains("ERROR"));
                Console.WriteLine("Seting Working Directory");
                Console.WriteLine(incoming);
                if(! incoming.Contains("OK")){Thread.Sleep(1000); errors++; goto a;}

                if(incoming.Contains("+UUFTPCR: 8,0"))
                {
                    //criando a pasta pois nao existe
                    Console.WriteLine("Criando pasta no servidor");
                    string[] path = destination.Split("/");

                    string filesPath = "";
                    foreach(string part in path)
                    {
                        filesPath += "/"+part.Replace("/","");
                        //AT+UFTPC=10,<directory_name>
                        b:
                        if(errors > allowedErrors)  return false;
                        serial.Write("\r\nAT+UFTPC=10,\""+filesPath+"\"\r\n");
                        incoming = "";
                        do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("+UUFTPCR: 10,") && !incoming.Contains("ERROR"));
                        Console.WriteLine("Creating Working Directory");
                        Console.WriteLine(incoming);
                        if((incoming.Contains("+UUFTPCR: 10,0")) || (!incoming.Contains("OK"))){Thread.Sleep(1000); errors++; goto b;}
                    }

                    goto a;
                }
                
                /*/exclui arquivo caso exista
                b:
                if(errors > allowedErrors)  return false;
                serial.Write("\r\nAT+UFTPC=2,\""+storeName+"\"\r\n");
                incoming = "";
                do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("+UUFTPCR: 2,") && !incoming.Contains("ERROR"));
                Console.WriteLine("Seting Working Directory");
                Console.WriteLine(incoming);
                if(! incoming.Contains("OK")){Thread.Sleep(1000); errors++; goto b;}
                */

                //prepara servidor para envio do arquivo    a partir de onde parou
                if(errors > allowedErrors)  return false;
                serial.Write("\r\nAT+UFTPC=7,\""+storeName+"\","+fromOffset.ToString()+"\r\n");
                incoming = "";
                do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("CONNECT") && !incoming.Contains("ERROR"));
                Console.WriteLine("Seting Send");
                Console.WriteLine(incoming);
                if(incoming.Contains("error"))  {Thread.Sleep(1000); errors++; goto a;}
                else if(incoming.Contains("CONNECT"))
                {
                    //abre arquivo mapeando em memoria e escreve na porta serial  
                    using (var mmf = MemoryMappedFile.CreateFromFile(fileName, FileMode.Open))
                    {
                        //Navega Pelas Intrações com o arquivo
                        for(long len=fromOffset; len<fileSize; len+=1024)
                        {
                            long accessorLength = 1024;
                            if((fileSize - len) < 1024) accessorLength = (fileSize-len)%1024;

                            //recupera uma parte dos arquivos a partir do acessor
                            using (var accessor = mmf.CreateViewAccessor(len, accessorLength))
                            {
                                try{
                                    //define tamanho do buffer serial
                                    byte[] buff = new byte[accessorLength];
                                    for(int i=0; i<accessorLength;i++)
                                    {
                                        buff[i] = accessor.ReadByte(i);
                                    }
                                    serial.Write(buff,0, (int) accessorLength);

                                    Console.Write("Enviado : ");
                                    Console.Write((len/(float)fileSize)*100);
                                    Console.WriteLine(" %");
                                }
                                catch(Exception e)
                                {
                                    Console.WriteLine(e);
                                    Console.WriteLine(serial.WriteBufferSize);
                                    Console.WriteLine(serial.BytesToWrite);
                                    return false;
                                }
                            }

                            next:;
                        }
                        
                        //define contador de tentativas de desconexao max 3
                        int tryes = 0;

                        disconect:
                        //finaliza envio no ftp
                        Console.WriteLine("Finalizando o Envio do Arquivo");
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
                    }
                }
                else
                {
                    return false;
                }
            }
            
            return true;
        }
/*
        public void resetDevice()
        {
            string port = findDevice();
            if(!port.Equals("0"))
            {
                //fecha serial de comunicação atual
                this.serial.Close();

                //abre nova serial
                SerialPort sp = new SerialPort(port, 115200);
                sp.Open();
                Console.WriteLine("Resetando device");
                sp.Write("\r\nAT+CFUN=16\r\n");
                sp.Close();

                //apos reset abre a serial atual
                Thread.Sleep(60000);
                this.serial.Open();
                Console.WriteLine("Religando device");
            }
        }
        */
        public bool veryfyNetwork()
        {
            Console.WriteLine("Aguardando Registro de Rede");
            int trys = 0;

            verifyNetwork:
            if(trys >= 3) return false;
            
            Console.WriteLine("tentativa : "+trys.ToString());

            serial.Write("\r\nat+creg?\r\n");
            string incoming = "";

            do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));

            if(incoming.Contains("+CREG: 0,1"))
            {
                Console.WriteLine("Registrado na rede com sucesso");
            }
            else {trys++; Thread.Sleep(5000); goto verifyNetwork;}

            return true;
        }
    }

    public class opcProcedure
    {
        public opcProcedure()
        {
            Bootstrap.Initialize();
        }

        public void teste()
        {
            // Make an URL of OPC DA server using builder.
            Uri url = UrlBuilder.Build("Matrikon.OPC.Simulation.1");

            Console.WriteLine(url);

            using (var server = new OpcDaServer(url))
            {
                // Connect to the server first.
                server.Connect();
                
                // Create a browser and browse all elements recursively.
                var browserAuto = new OpcDaBrowserAuto(server);
                BrowseChildren(browserAuto);

                void BrowseChildren(IOpcDaBrowser browser, string itemId = null, int indent = 0)
                {
                    // When itemId is null, root elements will be browsed.
                    OpcDaBrowseElement[] elements = browser.GetElements(itemId);

                    // Output elements.
                    foreach (OpcDaBrowseElement element in elements)
                    {
                        // Output the element.
                        Console.Write(new String(' ', indent));
                        Console.WriteLine(element);

                        // Skip elements without children.
                        if (!element.HasChildren)
                            continue;

                        // Output children of the element.
                        BrowseChildren(browser, element.ItemId, indent + 2);
                    }
                }
            }
        }
    }

    public class webProcedure
    {
        string domain;
        string configPath;
        string commandsPath;
        string user;
        string password;
        string deviceId;
        string ComputerId;
        string reqBody;
        SerialPort serial;
        Func<int> callback;

        public webProcedure(SerialPort port, string _computerId, string _deviceImei, string _domain, Func<int> _callback)
        {
            this.callback = _callback;
            this.serial = port;
            this.ComputerId = _computerId;
            this.deviceId = _deviceImei;

            //define o dominio para alcancar as config
            this.domain = _domain;
            this.configPath = "/optsync/config.php";
            this.commandsPath = "/optsync/commands.php";

            //define usuario e senha da pagina para adiquirir as config            
            this.user = "cemi";
            this.password = "Cemi2020*";

            //define corpo da requisicao
            reqBody = "VER=10";
            reqBody += "&IMEI="+this.deviceId;
            reqBody += "&MACHINEID="+this.ComputerId;
        }

        public void setConfigPath(string _configPath)
        {
            this.configPath = _configPath;
        }
        public void setComandPath(string _commandPath)
        {
            this.commandsPath = _commandPath;
        }
        public void setCredencials(string _user, string _password)
        {
            //define usuario e senha da pagina para adiquirir as config            
            this.user = _user;
            this.password = _password;
        }

        public string getImei()
        {
            return deviceId;
        }
        public string getMachine()
        {
            return ComputerId;
        }
        public Dictionary<string,string> getConfigOnline()
        {
            Dictionary<string, string> config = new Dictionary<string, string>();
            int errors = 0;
            int allowedErrors = 3;            
            string incoming = "";

            resetContext:
            if(errors > allowedErrors) throw new OperationCanceledException("Foi Alcançado o numero maximo de tentativas de acesso ao modem");
            serial.Write("\r\nAT+UHTTP=0\r\n");
            incoming = "";
            do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));
            Console.WriteLine(incoming);
            if(! incoming.Contains("OK")){Thread.Sleep(1000); errors++; goto resetContext;}

            
            setDomain:
            if(errors > allowedErrors) throw new OperationCanceledException("Foi Alcançado o numero maximo de tentativas de acesso ao modem");
            serial.Write("\r\nAT+UHTTP=0,1,\""+this.domain+"\"\r\n");
            incoming = "";
            do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));
            Console.WriteLine(incoming);
            if(! incoming.Contains("OK")){Thread.Sleep(1000); errors++; goto setDomain;}

            
            setPort:
            if(errors > allowedErrors) throw new OperationCanceledException("Foi Alcançado o numero maximo de tentativas de acesso ao modem");
            serial.Write("\r\nAT+UHTTP=0,5,80\r\n");
            incoming = "";
            do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));
            Console.WriteLine(incoming);
            if(! incoming.Contains("OK")){Thread.Sleep(1000); errors++; goto setPort;}

            
            setUser:
            if(errors > allowedErrors) throw new OperationCanceledException("Foi Alcançado o numero maximo de tentativas de acesso ao modem");
            Console.WriteLine(incoming);
            serial.Write("\r\nAT+UHTTP=0,2,\""+this.user+"\"\r\n");
            incoming = "";
            do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));
            Console.WriteLine(incoming);
            if(! incoming.Contains("")){Thread.Sleep(1000); errors++; goto setUser;}

            
            setPass:
            if(errors > allowedErrors) throw new OperationCanceledException("Foi Alcançado o numero maximo de tentativas de acesso ao modem");
            serial.Write("\r\nAT+UHTTP=0,3,\""+this.password+"\"\r\n");
            incoming = "";
            do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));
            Console.WriteLine(incoming);
            if(! incoming.Contains("OK")){Thread.Sleep(1000); errors++; goto setPass;}
            
            setAuthBasic:
            if(errors > allowedErrors) throw new OperationCanceledException("Foi Alcançado o numero maximo de tentativas de acesso ao modem");
            serial.Write("\r\nAT+UHTTP=0,4,1\r\n");
            incoming = "";
            do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));
            Console.WriteLine(incoming);
            if(! incoming.Contains("OK")){Thread.Sleep(1000); errors++; goto setAuthBasic;}


            
            doPostRequest:
            if(errors > allowedErrors) throw new OperationCanceledException("Foi Alcançado o numero maximo de tentativas de acesso ao modem");
            serial.Write("\r\nAT+UHTTPC=0,5,\""+this.configPath+"\",\"response.json\",\""+this.reqBody+"\",0\r\n");
            incoming = "";
            do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("+UUHTTPCR: 0,5,") && !incoming.Contains("ERROR"));
            Console.WriteLine(incoming);
            if(! incoming.Contains("+UUHTTPCR: 0,5,1")){Thread.Sleep(1000); errors++; goto doPostRequest;}

            
            readResponse:
            if(errors > allowedErrors) throw new OperationCanceledException("Foi Alcançado o numero maximo de tentativas de acesso ao modem");
            serial.Write("\r\nAT+URDFILE=\"response.json\"\r\n");
            incoming = "";
            do{incoming += serial.ReadLine().Replace("\r","|").Replace("\n","");} while(!incoming.Contains("|OK|") && !incoming.Contains("ERROR"));
            if(! incoming.Contains("|OK|")){Thread.Sleep(1000); errors++; goto readResponse;}            
            incoming = incoming.Replace("|","\r\n");
            
            //recupera dado atravez da regex
            string response = "";
            Match mch = Regex.Match(incoming, "\\+URDFILE: \\\"response.json\\\",(\\d+),\\\"([\\s+\\S+]+)\\\"");
            response = mch.Groups[2].Value;
            
            //separa cabecalho da mensagem
            response = response.Split("\r\n\r\n")[1];

            Console.WriteLine("httpResponse");
            Console.WriteLine(response);

            return config;
        }
        public List<string> getCommandsOnline()
        {
            List<string> commands = new  List<string>();
            int errors = 0;
            int allowedErrors = 3;            
            string incoming = "";

            resetContext:
            if(errors > allowedErrors) throw new OperationCanceledException("Foi Alcançado o numero maximo de tentativas de acesso ao modem");
            serial.Write("\r\nAT+UHTTP=0\r\n");
            incoming = "";
            do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));
            Console.WriteLine(incoming);
            if(! incoming.Contains("OK")){Thread.Sleep(1000); errors++; goto resetContext;}

            
            setDomain:
            if(errors > allowedErrors) throw new OperationCanceledException("Foi Alcançado o numero maximo de tentativas de acesso ao modem");
            serial.Write("\r\nAT+UHTTP=0,1,\""+this.domain+"\"\r\n");
            incoming = "";
            do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));
            Console.WriteLine(incoming);
            if(! incoming.Contains("OK")){Thread.Sleep(1000); errors++; goto setDomain;}

            
            setPort:
            if(errors > allowedErrors) throw new OperationCanceledException("Foi Alcançado o numero maximo de tentativas de acesso ao modem");
            serial.Write("\r\nAT+UHTTP=0,5,80\r\n");
            incoming = "";
            do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));
            Console.WriteLine(incoming);
            if(! incoming.Contains("OK")){Thread.Sleep(1000); errors++; goto setPort;}

            
            setUser:
            if(errors > allowedErrors) throw new OperationCanceledException("Foi Alcançado o numero maximo de tentativas de acesso ao modem");
            Console.WriteLine(incoming);
            serial.Write("\r\nAT+UHTTP=0,2,\""+this.user+"\"\r\n");
            incoming = "";
            do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));
            Console.WriteLine(incoming);
            if(! incoming.Contains("")){Thread.Sleep(1000); errors++; goto setUser;}

            
            setPass:
            if(errors > allowedErrors) throw new OperationCanceledException("Foi Alcançado o numero maximo de tentativas de acesso ao modem");
            serial.Write("\r\nAT+UHTTP=0,3,\""+this.password+"\"\r\n");
            incoming = "";
            do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));
            Console.WriteLine(incoming);
            if(! incoming.Contains("OK")){Thread.Sleep(1000); errors++; goto setPass;}
            
            setAuthBasic:
            if(errors > allowedErrors) throw new OperationCanceledException("Foi Alcançado o numero maximo de tentativas de acesso ao modem");
            serial.Write("\r\nAT+UHTTP=0,4,1\r\n");
            incoming = "";
            do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("OK") && !incoming.Contains("ERROR"));
            Console.WriteLine(incoming);
            if(! incoming.Contains("OK")){Thread.Sleep(1000); errors++; goto setAuthBasic;}


            
            doPostRequest:
            if(errors > allowedErrors) throw new OperationCanceledException("Foi Alcançado o numero maximo de tentativas de acesso ao modem");
            serial.Write("\r\nAT+UHTTPC=0,5,\""+this.commandsPath+"\",\"response.json\",\""+this.reqBody+"\",0\r\n");
            incoming = "";
            do{incoming += serial.ReadLine().Replace("\r","").Replace("\n","");} while(!incoming.Contains("+UUHTTPCR: 0,5,") && !incoming.Contains("ERROR"));
            Console.WriteLine(incoming);
            if(! incoming.Contains("+UUHTTPCR: 0,5,1")){Thread.Sleep(1000); errors++; goto doPostRequest;}

            
            readResponse:
            if(errors > allowedErrors) throw new OperationCanceledException("Foi Alcançado o numero maximo de tentativas de acesso ao modem");
            serial.Write("\r\nAT+URDFILE=\"response.json\"\r\n");
            incoming = "";
            do{incoming += serial.ReadLine().Replace("\r","|").Replace("\n","");} while(!incoming.Contains("|OK|") && !incoming.Contains("ERROR"));
            if(! incoming.Contains("|OK|")){Thread.Sleep(1000); errors++; goto readResponse;}            
            incoming = incoming.Replace("|","\r\n");
            
            //recupera dado atravez da regex
            string response = "";
            Match mch = Regex.Match(incoming, "\\+URDFILE: \\\"response.json\\\",(\\d+),\\\"([\\s+\\S+]+)\\\"");
            response = mch.Groups[2].Value;
            
            //separa cabecalho da mensagem
            response = response.Split("\r\n\r\n")[1];

            Console.WriteLine("httpResponse");
            Console.WriteLine(response);

            return commands;
        }
    }

    public class configurationServer
    {
        public HttpListener listener;
        public string url = "http://localhost";
        public string imei = "";
        public string compid = "";
        public string projPath = "";
        public int pageViews = 0;
        public int requestCount = 0;
        public string domaindAddress;
        public string configFile;
        public string user;
        public string pass;
        public string opcserver;
        public string opchost;
        Dictionary<string, Dictionary<string, object>> opcTags;
        Worker selfInstance;
        SerialPort serialPort;
        Func<int> callback;
        CancellationToken listeningCancelationToken;
        CancellationTokenSource listeningCancelationTokenSource;
        public bool isPause;
        public string apn;
        public string apnUser;
        public string apnPass;
                        
        public string wifissid;
        public string wifipass;
        public string wifiip;
        public string wifigateway  ;
        public string wifinetmask;

        public async Task HandleIncomingConnections(CancellationToken ct)
        {
            // Were we already canceled?
            ct.ThrowIfCancellationRequested();

            bool runServer = true;
            string extra = "";

            // While a user hasn't visited the `shutdown` url, keep on handling requests
            while (runServer)
            {

                // Make sure we don't increment the page views counter if `favicon.ico` is requested
                byte[] data = {new byte()};
                
                // Will wait here until we hear from a connection
                HttpListenerContext ctx = await this.listener.GetContextAsync();

                // Peel out the requests and response objects
                HttpListenerRequest req = ctx.Request;
                HttpListenerResponse resp = ctx.Response;
                
                if(isPause)
                {
                    if (req.Url.AbsolutePath.Equals("/favicon.ico"))
                    {
                        data = File.ReadAllBytes("logo.svg");   

                        // Write the response info
                        string disableSubmit = !runServer ? "disabled" : "";
                        resp.ContentType = "image/svg+xml";
                        resp.ContentLength64 = data.LongLength;

                        
                        Console.WriteLine(req.Url.AbsolutePath);
                        Console.WriteLine(data.LongLength);
                    }
                    else if (req.Url.AbsolutePath.Equals("/logo.svg"))
                    {
                        data = File.ReadAllBytes("logo.svg");   

                        // Write the response info
                        string disableSubmit = !runServer ? "disabled" : "";
                        resp.ContentType = "image/svg+xml";
                        resp.ContentLength64 = data.LongLength;

                        
                        Console.WriteLine(req.Url.AbsolutePath);
                        Console.WriteLine(data.LongLength);
                    }
                    else if((req.Url.AbsolutePath.Contains(".html")) || (req.Url.AbsolutePath.Equals("/"))){
                        data = Encoding.UTF8.GetBytes(Regex.Replace(File.ReadAllText("index.html"), "<body>.*</body>", "<body><h1>Server in Pause to Reconfiguration</h1></body>", RegexOptions.Singleline | RegexOptions.IgnoreCase));

                        resp.ContentType = "text/html";
                        resp.ContentEncoding = Encoding.UTF8;
                        resp.ContentLength64 = data.LongLength;
                    }
                    // Write out to the response stream (asynchronously), then close it
                    await resp.OutputStream.WriteAsync(data, 0, data.Length);
                    resp.Close();
                }
                else
                {
                    // Poll on this property if you have to do
                    // other cleanup before throwing.
                    if (ct.IsCancellationRequested)
                    {
                        // Clean up here, then...
                        ct.ThrowIfCancellationRequested();
                    }
    /*
                    // Print out some info about the request
                    Console.WriteLine("Request #: {0}", ++requestCount);
                    Console.WriteLine(req.Url.ToString());
                    Console.WriteLine(req.HttpMethod);
                    Console.WriteLine(req.UserHostName);
                    Console.WriteLine(req.UserAgent);
                    Console.WriteLine();
    */
                    // If `shutdown` url requested w/ POST, then shutdown the server after serving the page
                    if((req.HttpMethod == "POST") && (req.HasEntityBody))
                    {
                        Console.WriteLine("data Received");

                        string reqData = "";

                        using (System.IO.Stream body = req.InputStream) // here we have data
                        {
                            using (System.IO.StreamReader reader = new System.IO.StreamReader(body, req.ContentEncoding))
                            {
                                reqData = reader.ReadToEnd();
                            }
                        }
                        //separa dados de configuração
                        string config = HttpUtility.UrlDecode(reqData).Replace("&","\r\n");

                        //escreve configuração no arquivo
                        File.WriteAllText(configFile, config);

                        //chama função de callback
                        if(this.callback() == 1)
                            //adiciona mensagem a pagina
                            extra = "<script>alert(\"Sucesso ao Salvar Configurações\")</script>";
                        else
                            extra = "<script>alert(\"Falha ao Salvar Configurações\")</script>";
                    }

                    // If `shutdown` url requested w/ POST, then shutdown the server after serving the page
                    else if (req.HttpMethod == "GET")
                    {
                        Console.WriteLine("Get requested");
                    }
                    
                    Console.WriteLine(req.Url.AbsolutePath);
                    Console.WriteLine(req.Url.AbsolutePath.Length);

                    if (req.Url.AbsolutePath.Equals("/favicon.ico"))
                    {
                        data = File.ReadAllBytes("logo.svg");   

                        // Write the response info
                        string disableSubmit = !runServer ? "disabled" : "";
                        resp.ContentType = "image/svg+xml";
                        resp.ContentLength64 = data.LongLength;

                        
                        Console.WriteLine(req.Url.AbsolutePath);
                        Console.WriteLine(data.LongLength);
                    }
                    else if (req.Url.AbsolutePath.Equals("/logo.svg"))
                    {
                        data = File.ReadAllBytes("logo.svg");   

                        // Write the response info
                        string disableSubmit = !runServer ? "disabled" : "";
                        resp.ContentType = "image/svg+xml";
                        resp.ContentLength64 = data.LongLength;

                        
                        Console.WriteLine(req.Url.AbsolutePath);
                        Console.WriteLine(data.LongLength);
                    }
                    else if((req.Url.AbsolutePath.Contains(".html")) || (req.Url.AbsolutePath.Equals("/"))){

                        //recupera configuracao
                        Dictionary<string,string> _projectconfig = selfInstance.getConfiguration();

                        if(!_projectconfig.TryGetValue("apn", out this.apn))   this.apn = "";
                        if(!_projectconfig.TryGetValue("apnuser", out this.apnUser))   this.apnUser = "";
                        if(!_projectconfig.TryGetValue("apnpass", out this.apnPass))   this.apnPass = "";

                        if(!_projectconfig.TryGetValue("ssid", out this.wifissid))   this.wifissid = "";
                        if(!_projectconfig.TryGetValue("password", out this.wifipass))   this.wifipass = "";
                        if(!_projectconfig.TryGetValue("ip", out this.wifiip))   this.wifiip = "";
                        if(!_projectconfig.TryGetValue("gateway", out this.wifigateway))   this.wifigateway = "";
                        if(!_projectconfig.TryGetValue("netmask", out this.wifinetmask))   this.wifinetmask = "";

                        if(!_projectconfig.TryGetValue("projectpath", out this.projPath))   this.projPath = "";
                        if(!_projectconfig.TryGetValue("ftpdomain", out this.domaindAddress))   this.domaindAddress = "";
                        if(!_projectconfig.TryGetValue("ftpuser", out this.user))   this.user = "";
                        if(!_projectconfig.TryGetValue("ftppass", out this.pass))   this.pass = "";
                        if(!_projectconfig.TryGetValue("opchost", out this.opchost))   this.opchost = "";
                        if(!_projectconfig.TryGetValue("opcserver", out this.opcserver))   this.opcserver = "";
                        

                        //pega dados sobre o sinal  fazendo dentro de um try para exitar exceção da porta fechada
                        List<string> sigStatus = new List<string>();
                        sigStatus.Add("Serial Port Error");
                        sigStatus.Add("-1");

                        try{
                            if(!serialPort.IsOpen)  serialPort.Open();
                            sigStatus = selfInstance.getSignalStatus(serialPort);
                            if(serialPort.IsOpen)  serialPort.Close();
                        }
                        catch(Exception e)
                        {
                            Console.WriteLine(e);
                        }

                        opcTags = selfInstance.getOpcTags();


                        
                        //lista tags do opc em checkboxes
                        string tagList = "";
                        Dictionary<string, string> tagListDict = new Dictionary<string, string>();

                        foreach(string tag in opcTags.Keys)
                        {
                            try{
                            int end = tag.IndexOf(".") >= 0 ? tag.IndexOf("."): tag.Length;
                            string key = tag.Substring(0,end);// Convert.ToBase64String(Encoding.ASCII.GetBytes(tag.Substring(0,end)));

                            if(!tagListDict.ContainsKey(key))
                                tagListDict[key] = "";

                            if((bool) opcTags[tag]["active"])    tagListDict[key] += "<div class=\"tag\"><input type=\"checkbox\" id=\"tag_"+tag+"\" name=\"$"+tag+"\" checked>  <label for=\"tag_"+tag+"\">"+tag+"</label></div>\r\n";
                            else                tagListDict[key] += "<div class=\"tag\"><input type=\"checkbox\" id=\"tag_"+tag+"\" name=\"$"+tag+"\">  <label for=\"tag_"+tag+"\">"+tag+"</label></div>\r\n";

                            }
                            catch(Exception e)
                            {
                                //Console.WriteLine(e);
                            }
                        }

                        foreach(string listValue in tagListDict.Keys)
                        {
                            tagList += "<fieldset style=\"display: block;\" class=\""+listValue+"\"> "+
                                            "<legend>"+listValue+"</legend>";
                            tagList += tagListDict[listValue];
                            tagList += "<input type=\"button\" onclick=\"SelectAll(event)\" value=\"SelectAll\" />"+
                                    "<input type=\"button\" onclick=\"UnselectAll(event)\" value=\"UnselectAll\" />";
                            tagList += "</fieldset>";
                        }

                        string hostList = "";
                        var hosts = selfInstance.getNetworkHosts();
                        foreach(string host in hosts.Keys)
                        {
                            hostList += "<option value=\""+hosts[host]+"\">"+host+"</option>";
                        }
                        if(hostList.Length <= 1)    hostList = "<option>localhost</option><option>127.0.0.1</option>";

                        string opcList = "";
                        foreach(string opc in selfInstance.getNetworkOpc())
                        {
                            opcList += "<option>"+opc.Substring(opc.LastIndexOf("/")).Replace("/","")+"</option>";
                        }
                        if(opcList.Length <= 1)    opcList = "<option>Matrikon.OPC.Simulation.1</option><option>Kepware.KEPServerEX.V6</option><option>KEPServerEXV6_OPCNET</option> ";

                        // Write the response info
                        data = Encoding.UTF8.GetBytes(File.ReadAllText("index.html")

                        .Replace("%wifissid%", this.wifissid)
                        .Replace("%wifipass%", this.wifipass)
                        .Replace("%ipaddress%", this.wifiip)
                        .Replace("%ipgateway%", this.wifigateway)
                        .Replace("%ipnetmask%", this.wifinetmask)

                        .Replace("%apn%", this.apn)
                        .Replace("%apnuser%", this.apnUser)
                        .Replace("%apnpass%", this.apnPass)
                        .Replace("%deviceid%", this.imei)
                        .Replace("%computerid%", this.compid)
                        .Replace("%projectpath%", this.projPath)
                        .Replace("%domain%", this.domaindAddress)
                        .Replace("%user%", this.user)
                        .Replace("%pass%", this.pass)
                        .Replace("%opcserver%", this.opcserver)
                        .Replace("%opchost%", this.opchost)
                        .Replace("%availabletags%", tagList)           
                        .Replace("%hostList%", hostList)           
                        .Replace("%opcList%", opcList)                    
                        .Replace("%signal%", sigStatus[1]+" || "+sigStatus[0])
                        .Replace("%signalpower%", sigStatus[1].Replace("%","").Replace(" ",""))
                        .Replace("</head>", extra+"</head>")
                        );
                        string disableSubmit = !runServer ? "disabled" : "";
                        resp.ContentType = "text/html";
                        resp.ContentEncoding = Encoding.UTF8;
                        resp.ContentLength64 = data.LongLength;
                    }
                    // Write out to the response stream (asynchronously), then close it
                    await resp.OutputStream.WriteAsync(data, 0, data.Length);
                    resp.Close();
                }
            }
        }

        public void stopServer()
        {
            this.listener.Close();
            listeningCancelationTokenSource.Cancel();            
            listeningCancelationTokenSource.Dispose();

            //aborta a thread
            throw new Exception("Thread Stopped Successfully");
        }

        public configurationServer(int port, string _imei, string _compid, SerialPort sp, string _configFile, Dictionary<string, Dictionary<string, object>> _opcTags, Func<int> _callback)
        {
            // Define the cancellation token.
            listeningCancelationTokenSource = new CancellationTokenSource();
            listeningCancelationToken = listeningCancelationTokenSource.Token;

            this.isPause = false;

            // configuration["projectpath"], configuration["ftpdomain"], configuration["ftpuser"], configuration["ftppass"]
            //instancia o programa principoar para adquirir alguns metodos
            this.selfInstance = new Worker();
            this.serialPort = sp;

            this.opcTags = _opcTags;
            this.imei = _imei;
            this.compid = _compid;
            this.configFile = _configFile;
            this.callback = _callback;
            
            // Create a Http server and start listening for incoming connections
            this.listener = new HttpListener();
            this.listener.Prefixes.Add(url+":"+port.ToString()+"/");
            this.listener.Start();
            Console.WriteLine("Listening for connections on {0}", url+":"+port.ToString()+"/");
        }

        public void startServer()
        {
            // Handle requests
            Task listenTask = HandleIncomingConnections(listeningCancelationToken);
            listenTask.GetAwaiter().GetResult();

            // Close the listener
            this.listener.Close();
        }
        public void pauseServer()
        {
            this.isPause = true;
        }
        public void resumeServer()
        {
            this.isPause = false;
        }
    }
}