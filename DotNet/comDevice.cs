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
    abstract class DeviceCommon{
        public abstract Dictionary<string, string> getDeviceData(SerialPort sp);
    }
    abstract class DeviceWebServer
        {
            //public abstract WebServer();
           
            public abstract string getInternalServerConfig(String url);
        }
        abstract class DeviceFilePrepare
        {

            //public abstract FilePrepare();
           
            public abstract List<string> prepareLogToSend(string filePath, bool _project=false, bool _log=true);
           
            //private abstract static void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs);           

            //extrai o arquivo e retorna o caminho completo do mesmo
            public abstract string unzipLogFile(string fileName);
           
            public abstract string getHeader(string fileName);
            public abstract bool clearPartFile();
            public abstract List<string> getHourSegments(string fileName, string __header);    
        }   

        public abstract class DeviceconectionProcedures{
        public abstract bool pdpConection(int allowedErrors=3);
        public abstract bool isConnected();
        //public abstract bool connectNetwork(string ssid, string password);
        //public abstract bool setIp(string ipAddress, string gateway = null, string netmask = null);
        public abstract List<Dictionary<string, string>> listSSID(int allowedErrors=3);
        //public abstract List<Dictionary<string,string>> getFtpFileDataList();
        public abstract int startSocket(string url, string hostPort);
        public abstract bool startContinuousSocket(int socketNumber);
        public abstract bool fininshContinuousSocket(int socketNumber = -1);

        public abstract bool closeSocket(int socketNumber);
        public abstract string writeContinuousSocket(string data, string response = "\r\n", string token = "qazxc123", int __timeout = 60, int socket=0);

        //public abstract conectionProcedures(string _deviceMac, string _machineId, string _apn, string _user, string _pwd, SerialPort _serialPort);
        //public abstract conectionProcedures(string _deviceMac, string _machineId, string _apn, string _user, string _pwd, Socket _socket);
        //public abstract bool ftpIsConnected();
        //public abstract bool startFtpClient(string _domain, string _usr, string _pwd);
        //public abstract bool writeSocket(int socketNumber, string _data);
        //public abstract String readSocket(int socketNumber, string ending="\r\n");
        /**********************************************************************************/

        /*******************************Faz Requisicoes http*******************************/
        //public abstract string httpRequest(string _type, string _url, int _port = 80 ,string _header = null , string _data = null, string _user=null, string _pass=null);

        /**********************************************************************************/

        //public abstract bool findUpdate(string currentVersion);

        //public abstract bool sendFtpFile(string fileName, string storeName, string destination, long fromOffset);

        //public abstract bool veryfyNetwork();
       
    }

    public abstract class DeviceopcProcedure
    {
        //public abstract opcProcedure();
     
        public abstract void teste();
       
    }

    public abstract class DevicewebProcedure
    {

        //public abstract webProcedure(SerialPort port, string _computerId, string _deviceImei, string _domain, Func<int> _callback);
     
        public abstract void setConfigPath(string _configPath);
      
        public abstract void setComandPath(string _commandPath);
       
        public abstract void setCredencials(string _user, string _password);
        

        public abstract string getImei();
       
        public abstract string getMachine();
       
        public abstract Dictionary<string,string> getConfigOnline();
       
        public abstract List<string> getCommandsOnline();
        
    }

    public abstract class DeviceconfigurationServer
    {
        public abstract Task HandleIncomingConnections(CancellationToken ct);
      
        public abstract void stopServer();
     

        //public abstract configurationServer(int port, string _imei, string _compid, SerialPort sp, string _configFile, Dictionary<string, Dictionary<string, object>> _opcTags, Func<int> _callback);
       

        public abstract void startServer();
       
        public abstract void pauseServer();
        
        public abstract void resumeServer();
    }
}