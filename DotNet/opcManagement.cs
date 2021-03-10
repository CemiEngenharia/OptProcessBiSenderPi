#define debugMode

using System;
using TitaniumAS.Opc.Client;
using TitaniumAS.Opc.Client.Da;
using TitaniumAS.Opc.Client.Da.Browsing;
using TitaniumAS.Opc.Client.Common;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Win32;
using System.Threading;
using System.Threading.Tasks;

namespace OptProcessBiSenderService
{
    public class Tags
    {
        public string TipoTag { get; set; }
        public string NomeTag { get; set; }
        public string IndiceTag { get; set; }
        public string StatusTag { get; set; }
        public string RefereciaTag { get; set; }
        public string ComentarioTag { get; set; }
        public string TagSelecionadoParaVisualizacao { get; set; }
        public string ValorTag { get; set; }
        public string ValorInicial { get; set; }
        public string Persistente { get; set; }
        public string TextoEndereco { get; set; }
        public string Qualidade { get; set; }
        public string Escala { get; set; }
        public string Offset { get; set; }
        public string ValorMinimo { get; set; }
        public string ValorMinimoAcao { get; set; }
        public string ValorMinimoOvFl { get; set; }
        public string ValorMaximo { get; set; }
        public string ValorMaximoAcao { get; set; }
        public string ValorMaximoOvFl { get; set; }
        public string FrozenCycles { get; set; }
        public string AutoSend { get; set; }
        public string LogSet { get; set; }
        public string LogReset { get; set; }
        public string MailTo { get; set; }
        public string TextoEquacao { get; set; }
        public string CaracteretoParse { get; set; }
        public string NomeTagAssociado { get; set; }
        public string IndiceTagAssociado { get; set; }
        public string Periodo { get; set; }
        public string Atualizar { get; set; }
        public string NomeRegraEstatisticaCondicional { get; set; }
        public string PeriodoEmSegundos { get; set; }
        public string Ordem { get; set; }
    }

    public class opcManagement
    {

        public string host {get;set;}
        public string name {get; set;}
        private OpcDaServer server;
        private bool isActive;
        OpcDaGroup readingGroup;
        OpcDaGroup writingGroup;
        CancellationToken readingCancelationToken;
        CancellationTokenSource readingCancelationTokenSource;
        public opcManagement()
        {
            isActive = false;
            host = "";
            name = "";

        }
        public bool startClient()
        {
            try{
                //Tenta Encerrar Cliente OPC
                if(this.server != null) this.stopClient();

                // Define the cancellation token.
                this.readingCancelationTokenSource = new CancellationTokenSource();
                this.readingCancelationToken = this.readingCancelationTokenSource.Token;

                Uri url = UrlBuilder.Build(this.name, this.host);
                this.server = new OpcDaServer(url);   
                this.server.Connect();   

                Console.WriteLine("Servidor OPC Instanciado na url {0}", url);
                return true;      
            }
            catch(Exception e)
            {
                //Console.Write("Falha ao Iniciar Conexão do Opc");
                
                Console.Write("Falha ao Iniciar Conexão do Opc -> ");
                //Console.Write(e.ToString());
                
                Console.Write(e.ToString());
            }
            return false;

        }
        public void  stopClient()
        {
            //cancela a thread de leitura
            if(this.readingCancelationTokenSource != null)
            {
                this.readingCancelationTokenSource.Cancel();
                Thread.Sleep(1000);
            }

            //mata servidor de leitura
            if(this.server != null)
            {
                if(this.server.IsConnected)
                    this.server.Disconnect();
                this.server.Dispose();
            }       
        }

        public bool isConnected()
        {
            //mata servidor de leitura
            if(this.server != null)
            {
                if(this.server.IsConnected)
                    return true;
            }       
            return false;
            
        }

        public void buildMatrikonConfig(string processProject)
        {
        
        ///structure
        ///<?xml version="1.0"?>
        ///<Matrikon.OPC.Simulation>
        ///    <CSimRootDevLink description="Sim this.server Root"/>
        ///    <PSTAliasGroup>
        ///        <PSTAlias itemPath="asdsada" name="asasdsad"/>
        ///        <PSTAlias itemPath="sdasd" name="asdasd"/>
        ///        <PSTAlias itemPath="asdasd" name="sadsad"/>
        ///    </PSTAliasGroup>
        ///</Matrikon.OPC.Simulation> 
        /// 

            //template de todo o arquivo de configuração
            string outputFileStructure = "<?xml version=\"1.0\"?>\r\n";
            outputFileStructure += "<Matrikon.OPC.Simulation>\r\n";
            outputFileStructure += "<CSimRootDevLink description=\"OptProcess Auto Generated\"/>\r\n";
            outputFileStructure += "<PSTAliasGroup>\r\n";
            //<PSTAlias itemPath="ActualValue" name="ActualValue" dataType="0"/>
            outputFileStructure += "</PSTAliasGroup>\r\n";
            outputFileStructure += "</Matrikon.OPC.Simulation>\r\n";

            //busca tags no optprocess
            List<Tags> processTags = getProjectTags(processProject);

            //dicionario para organizar os dados
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(outputFileStructure);

            //seleciona primeiro grupo
            string rootNode = "Matrikon.OPC.Simulation/PSTAliasGroup";
            XmlElement root = doc.DocumentElement;;

            if(processTags != null)
            {
                //navega todas as tags e adiciona ao arquivo
                foreach(Tags td in processTags)
                {
                    if((td.TextoEndereco != null) && (td.TextoEndereco.Length > 0))
                    {
                        //retira endereço do opc do address
                        string address = td.TextoEndereco.Substring(td.TextoEndereco.IndexOf("|")+1);
                    
                        string[] nodePath = address.Split(@".");

                        //inicializa o no atual
                        XmlNode currentNode = doc.SelectSingleNode(rootNode);

                        for(int i=0; i<= nodePath.LongLength - 1; i++)
                        {
                            if(i < nodePath.Length-1)
                            {
                                string currentPath = rootNode;
                                
                                for(int j=0; j<i; j++)
                                {
                                    if(currentPath.Length > 0)      currentPath += "/";
                                    currentPath += "PSTAliasGroup[@name='"+nodePath[j]+"']";
                                }

                                string nodeName = "PSTAliasGroup[@name='"+nodePath[i]+"']";

                                //Console.WriteLine(root.SelectSingleNode(currentPath);
                                if(currentNode.SelectSingleNode(nodeName) == null)
                                {
                                    //Console.WriteLine(currentNode.NamespaceURI);

                                    XmlNode node = doc.CreateNode("element", "PSTAliasGroup", currentPath);

                                    //Create a new attribute
                                    XmlAttribute name = doc.CreateAttribute("name");
                                    name.Value = nodePath[i];    
                                    node.Attributes.Append(name);
                                    
                                    currentNode = currentNode.AppendChild(node);
                                }
                            }
                            else
                            {
                                string currentPath = rootNode;
                                
                                for(int j=0; j<i; j++)
                                {
                                    if(currentPath.Length > 0)      currentPath += "/";
                                    currentPath += "PSTAlias[@name='"+nodePath[j]+"']";
                                }

                                string nodeName = "PSTAlias[@name='"+nodePath[i]+"']";

                                //Console.WriteLine(root.SelectSingleNode(currentPath);
                                if(currentNode.SelectSingleNode(nodeName) == null)
                                {
                                    //Console.WriteLine(currentNode.NamespaceURI);

                                    XmlNode node = doc.CreateNode("element", "PSTAlias", currentPath);

                                    //Create a new attribute
                                    XmlAttribute name = doc.CreateAttribute("name");
                                    name.Value = nodePath[i];    
                                    node.Attributes.Append(name);
                                    
                                    currentNode = currentNode.AppendChild(node);
                                }
                                
                            }
                        }
                    }                        
                }
            } 

            File.WriteAllText("Matrikon_New.xml", doc.InnerXml);           

        }

        public string getMatrikonConfigPath()
        {
            string value = "";
            try
            {
                using(RegistryKey result = Registry.ClassesRoot.OpenSubKey(@"WOW6432Node\CLSID\{F8582CF4-88FB-11D0-B850-00C0F0104305}\Options"))
                {
                    value = (string) result.GetValue("DefaultConfig");
                }
                return value;
            }
            catch(Exception)
            {
                return value;
            }
            
        }
        public bool setMatrikonConfigPath(string configurationPath)
        {
            try
            {
                Registry.SetValue(@"HKEY_CLASSES_ROOT\WOW6432Node\CLSID\{F8582CF4-88FB-11D0-B850-00C0F0104305}\Options" ,@"DefaultConfig" , configurationPath);
                return true;
            }          
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        public List<Tags> getProjectTags(string projectDirectory)
        {
            //verifica se diretorio existe
            if(!Directory.Exists(projectDirectory))     return null;

            string[] fList = Directory.GetFiles(projectDirectory+@"\Tag");
            string tagsFile = "";

            //procura arquivo de tag
            foreach(string f in fList)  if(f.EndsWith(".Tag"))  tagsFile = f;

            //verifica se arquivo de tag existe
            if(!File.Exists(tagsFile))   return null;

            //inicializa lista de tags
            List<Tags> result = new List<Tags>();

            //carrega xml do disco
            string xmlContent = File.ReadAllText(tagsFile);
            XmlDocument tagsDoc = new XmlDocument();
            tagsDoc.LoadXml(xmlContent);

            //busca o nó das tags
            XmlNodeList nodes = tagsDoc.SelectNodes("dsallTags/*");

            //adiciona cada uma a lista de tags
            foreach(var n in nodes)
            {
                XmlElement element = (XmlElement) n;
                
                if(element.GetElementsByTagName("TipoTag").Count <= 0)  continue;

                Tags tg = new Tags();

                tg.TipoTag = element.GetElementsByTagName("TipoTag")[0].InnerText;
                tg.NomeTag = element.GetElementsByTagName("NomeTag")[0].InnerText;
                tg.IndiceTag = element.GetElementsByTagName("IndiceTag")[0].InnerText;
                tg.StatusTag = element.GetElementsByTagName("StatusTag")[0].InnerText;
                tg.RefereciaTag = element.GetElementsByTagName("RefereciaTag")[0].InnerText;
                tg.ComentarioTag = element.GetElementsByTagName("ComentarioTag")[0].InnerText;
                tg.TagSelecionadoParaVisualizacao = element.GetElementsByTagName("TagSelecionadoParaVisualizacao")[0].InnerText; 

                switch(int.Parse(element.GetElementsByTagName("TipoTag")[0].InnerText))
                {
                    case 0:
                            tg.ValorTag = element.GetElementsByTagName("ValorTag")[0].InnerText;
                            tg.ValorInicial = element.GetElementsByTagName("ValorInicial")[0].InnerText;
                            tg.Persistente = element.GetElementsByTagName("Persistente")[0].InnerText;   
                        break;
                    case 1:
                            tg.TextoEndereco = element.GetElementsByTagName("TextoEndereco")[0].InnerText;
                            tg.Qualidade = element.GetElementsByTagName("Qualidade")[0].InnerText;
                            tg.Escala = element.GetElementsByTagName("Escala")[0].InnerText;   
                            tg.Offset = element.GetElementsByTagName("Offset")[0].InnerText;   
                            tg.ValorMinimo = element.GetElementsByTagName("ValorMinimo")[0].InnerText;   
                            tg.ValorMinimoAcao = element.GetElementsByTagName("ValorMinimoAcao")[0].InnerText;   
                            tg.ValorMinimoOvFl = element.GetElementsByTagName("ValorMinimoOvFl")[0].InnerText;   
                            tg.ValorMaximo = element.GetElementsByTagName("ValorMaximo")[0].InnerText;     
                            tg.ValorMaximoAcao = element.GetElementsByTagName("ValorMaximoAcao")[0].InnerText;     
                            tg.ValorMaximoOvFl = element.GetElementsByTagName("ValorMaximoOvFl")[0].InnerText;     
                            tg.FrozenCycles = element.GetElementsByTagName("FrozenCycles")[0].InnerText;   
                        break;
                    case 2:
                            tg.TextoEndereco = element.GetElementsByTagName("TextoEndereco")[0].InnerText;
                            tg.Qualidade = element.GetElementsByTagName("Qualidade")[0].InnerText;
                            tg.Escala = element.GetElementsByTagName("Escala")[0].InnerText;   
                            tg.Offset = element.GetElementsByTagName("Offset")[0].InnerText;   
                            tg.ValorMinimo = element.GetElementsByTagName("ValorMinimo")[0].InnerText;   
                            tg.ValorMinimoAcao = element.GetElementsByTagName("ValorMinimoAcao")[0].InnerText;   
                            tg.ValorMinimoOvFl = element.GetElementsByTagName("ValorMinimoOvFl")[0].InnerText;   
                            tg.ValorMaximo = element.GetElementsByTagName("ValorMaximo")[0].InnerText;     
                            tg.ValorMaximoAcao = element.GetElementsByTagName("ValorMaximoAcao")[0].InnerText;     
                            tg.ValorMaximoOvFl = element.GetElementsByTagName("ValorMaximoOvFl")[0].InnerText;     
                            tg.AutoSend = element.GetElementsByTagName("AutoSend")[0].InnerText;   
                        break;
                    case 3:
                            tg.TextoEquacao = element.GetElementsByTagName("TextoEquacao")[0].InnerText;
                            tg.Qualidade = element.GetElementsByTagName("Qualidade")[0].InnerText;
                            tg.Escala = element.GetElementsByTagName("Escala")[0].InnerText;    
                            tg.Offset = element.GetElementsByTagName("Offset")[0].InnerText;    
                            tg.CaracteretoParse = element.GetElementsByTagName("CaracteretoParse")[0].InnerText;    
                        break;
                    case 4:
                            tg.LogSet = element.GetElementsByTagName("LogSet")[0].InnerText;
                            tg.LogReset = element.GetElementsByTagName("LogReset")[0].InnerText;
                            tg.MailTo = element.GetElementsByTagName("MailTo")[0].InnerText;   
                        break;  
                    case 5:
                            tg.NomeTagAssociado = element.GetElementsByTagName("NomeTagAssociado")[0].InnerText;
                            tg.IndiceTagAssociado = element.GetElementsByTagName("IndiceTagAssociado")[0].InnerText;
                            tg.Periodo = element.GetElementsByTagName("Periodo")[0].InnerText;   
                            tg.Atualizar = element.GetElementsByTagName("Atualizar")[0].InnerText;   
                            tg.NomeRegraEstatisticaCondicional = element.GetElementsByTagName("NomeRegraEstatisticaCondicional")[0].InnerText;   
                            tg.PeriodoEmSegundos = element.GetElementsByTagName("PeriodoEmSegundos")[0].InnerText;   
                            tg.Ordem = element.GetElementsByTagName("Ordem")[0].InnerText;   
                        break;  
                    case 6:
                            tg.NomeTagAssociado = element.GetElementsByTagName("NomeTagAssociado")[0].InnerText;
                            tg.IndiceTagAssociado = element.GetElementsByTagName("IndiceTagAssociado")[0].InnerText;
                            tg.Periodo = element.GetElementsByTagName("Periodo")[0].InnerText;   
                            tg.Atualizar = element.GetElementsByTagName("Atualizar")[0].InnerText;   
                            tg.NomeRegraEstatisticaCondicional = element.GetElementsByTagName("NomeRegraEstatisticaCondicional")[0].InnerText;   
                            tg.PeriodoEmSegundos = element.GetElementsByTagName("PeriodoEmSegundos")[0].InnerText;   
                        break;    
                }

                result.Add(tg);
            }

            return result;
        }
        
        public List<string>  getAllTags()
        {
            Console.Write("this.server.ComObject");
            Console.Write(this.server.ComObject);
            
            // Connect to the this.server first.
            this.server.Connect();
            var browser = new OpcDaBrowserAuto(this.server);

            //busca tags presentes e adiciona a lista
            return this.BrowseChildren(browser);
        }

        public void writeTag(string _tagName, string _tagValue, bool _sync=false)
        {
            // Create a group with items.
            if(writingGroup == null)
                writingGroup = this.server.AddGroup("writingGroup");

            writingGroup.IsActive = true;

            OpcDaItemDefinition def = new OpcDaItemDefinition
            {
                ItemId = _tagName,
                IsActive = true
            };

            OpcDaItemDefinition[] definitions = { def };
            OpcDaItemResult[] results = writingGroup.AddItems(definitions);

            // Handle adding results.
            foreach (OpcDaItemResult result in results)
            {
            }
            
            OpcDaItem[] items = { writingGroup.Items[0] };
            object[] values = { _tagValue };

            if(_sync)
            {
                // Write values to the items synchronously.
                HRESULT[] hresults = writingGroup.Write(items, values);
            }/*
            else
            {
                // Write values to the items asynchronously.
                HRESULT[] results = await group.WriteAsync(items, values);
            }*/
        }

        public void startReadingGroup(string[] _tagName)
        {
            Console.WriteLine("Adicionando {0} itens ao grupo de leitura", _tagName.Length);

            //remove grupo para uma nova leitura
            if(readingGroup != null)
                try{this.server.RemoveGroup(readingGroup);} catch(Exception e){Console.WriteLine(e);}

            readingGroup = this.server.AddGroup("readingGroup");

            readingGroup.IsActive = true;

            List<OpcDaItemDefinition> def = new List<OpcDaItemDefinition>();

            foreach(string t in _tagName)
            {
                Console.WriteLine("Tag ItemID");
                Console.WriteLine(t);
                def.Add(new OpcDaItemDefinition
                {
                    ItemId = t,
                    IsActive = true
                });
            }

            //cria array com as definicoes
            OpcDaItemDefinition[] definitions = def.ToArray();
            
            //readingGroup.Items.RemoveAll();

            //readingGroup.RemoveItems(definitions.GetValue());      //remove itens para adicionar eles
            OpcDaItemResult[] results = readingGroup.AddItems(definitions);

            // Handle adding results.
            foreach (OpcDaItemResult result in results)
            {
                if(result.Item != null)
                    Console.WriteLine("Status of item: {0}", result.Item.ItemId);

                if (result.Error.Failed)
                    Console.WriteLine("Error adding items: {0}", result.Error);
                else
                    Console.WriteLine("Success adding items: ");
            }

        }

        public OpcDaItemValue[] readTag(bool _sync=false, Action<OpcDaItemValue[]> _callBack = null)
        {
            //da excecao caso o argumento callback seja nulo com o comando assyncrono
            if((_sync == false) && (_callBack == null)) throw(new ArgumentNullException());

            // Create a group with items.
            readingGroup.SyncItems();
            Console.Write("reading group -> ");
            Console.WriteLine(readingGroup);

            Console.WriteLine("lendo");
            //faz leitura das tags
            if(_sync)
            {
                // Read all items of the group synchronously.
                var values =  readingGroup.Read(readingGroup.Items, OpcDaDataSource.Device);
                
                foreach(var val in values)
                {
                    Console.Write(val.Item.ItemId);
                    Console.Write(" -- \t");
                    Console.Write(val.Value);
                    Console.Write(" -- \t\t\t\t");
                    Console.Write((int) val.Quality);
                    Console.Write(" -- \t\t");
                    Console.Write(val.Timestamp);
                    Console.Write(" -- \t\t");
                    Console.WriteLine((int) val.Error);
                }
                
            }
            else
            {
                void completed(Task<OpcDaItemValue[]> values)
                {/*
                    //OpcDaItemValue[] values = group.ReadAsync(group.Items);
                    foreach(OpcDaItemValue val in values.Result)
                    {
                        Console.Write(val.Item.ItemId);
                        Console.Write(" -- \t");
                        Console.Write(val.Value);
                        Console.Write(" -- \t\t\t\t");
                        Console.Write((int) val.Quality);
                        Console.Write(" -- \t\t");
                        Console.Write(val.Timestamp);
                        Console.Write(" -- \t\t");
                        Console.WriteLine((int) val.Error);
                    }
*/
                    //chama o callback definido na funcao
                    _callBack((OpcDaItemValue[]) values.Result);
                }

                var readingTask = readingGroup.ReadAsync(readingGroup.Items, this.readingCancelationToken);
                Action<Task<OpcDaItemValue[]>> receiverData = completed; 
                readingTask.ContinueWith(receiverData);
                
                // Read all items of the group asynchronously.
                //OpcDaItemValue[] values = group.ReadAsync(group.Items);

                //foreach(var val in values)
                //{
                //    Console.WriteLine(val);
                //}
            }
            return null;
        }

        public List<String> BrowseChildren(IOpcDaBrowser browser, string itemId = null, int indent = 0)
        {
            List<string> result =  new List<string>();

            // When itemId is null, root elements will be browsed.
            OpcDaBrowseElement[] elements = browser.GetElements(itemId);

            // Output elements.
            foreach (OpcDaBrowseElement element in elements)
            {                
                if (!element.HasChildren)
                {
                    //Console.WriteLine(element);

                    OpcDaItemProperties[] ppts = browser.GetProperties(new []{element.ItemId}, new OpcDaPropertiesQuery(true, new int[] { 1 }));
                    OpcDaItemProperties ppt = (OpcDaItemProperties) ppts.GetValue(0);
                    
                    /*
                    Console.WriteLine(element.ItemId);
                    Console.WriteLine(ppt.Properties[0].DataType);
                    */
                    /*

                    1 Item Canonical DataType (ItemId: #MonitorACLFile, DataType System.Int16, Value: , ErrorId: S_OK: Success code)
                    2 Item Value (ItemId: #MonitorACLFile, DataType TitaniumAS.Opc.Client.Common.IllegalType, Value: , ErrorId: S_OK: Success code)
                    3 Item Quality (ItemId: #MonitorACLFile, DataType System.Int16, Value: , ErrorId: S_OK: Success code)
                    4 Item Timestamp (ItemId: #MonitorACLFile, DataType System.DateTime, Value: , ErrorId: S_OK: Success code)
                    5 Item Access Rights (ItemId: #MonitorACLFile, DataType System.Int32, Value: , ErrorId: S_OK: Success code)
                    6 this.server Scan Rate (ItemId: #MonitorACLFile, DataType System.Single, Value: , ErrorId: S_OK: Success code)
                    7 Item EU Type (ItemId: #MonitorACLFile, DataType System.Int32, Value: , ErrorId: S_OK: Success code)
                    8 Item EUInfo (ItemId: #MonitorACLFile, DataType System.String[], Value: , ErrorId: S_OK: Success code)
                    */
                    /*
                    foreach(OpcDaItemProperty _prop in ppt.Properties)
                    {
                        Console.WriteLine(_prop);
                    }
                    */
                    

                    //element.ItemProperties.Properties.DataType;   elemento mais tag datatype                    
                    result.Add(element.ItemId + "," + tagTypeConvert((Int16) ppt.Properties[0].Value));
                    continue;
                }

                // Output children of the element.
                if(element.ItemId != "_Hints")
                    result.AddRange(BrowseChildren(browser, element.ItemId, indent + 2));
            }

            return result;
        }

        public string tagTypeConvert(Int16 type)
        {

            //Definição em https://www.kepware.com/getattachment/1b1e7d7c-6eeb-4f8f-ad41-5d6a7bae1b64/canonical-data-types.pdf
            switch(type)
            {
                case 8196:
                    return  "ArrayOfReal4";
                    break;
                case 8197:
                    return  "ArrayOfReal8";
                    break;
                case 8194:
                    return  "ArrayOfUint2";
                    break;
                case 8195:
                    return  "ArrayOfInt4";
                    break;
                case 8200:
                    return  "ArrayOfString";
                    break;
                case 8203:
                    return  "ArrayOfBoolean";
                    break;
                case 8209:
                    return  "ArrayOfByte";
                    break;
                case 8211:
                    return  "ArrayOfUint4";
                    break;
                case 8212:
                    return  "ArrayOfLLong";
                    break;
                case 8213:
                    return  "ArrayOfQWord";
                    break;
                case 8210:
                    return  "ArrayOfWord";
                    break;
                case 2:
                    return  "Int2";
                    break;
                case 3:
                    return  "Int4";
                    break;
                case 4:
                    return  "Real4";
                    break;
                case 5:
                    return  "Real8";
                    break;
                case 6:
                    return  "Money";
                    break;
                case 7:
                    return  "Time";
                    break;
                case 8:
                    return  "String";
                    break;
                case 11:
                    return  "Boolean";
                    break;
                case 16:
                    return  "Int1";
                    break;
                case 17:
                    return  "UInt1";
                    break;
                case 18:
                    return  "UInt2";
                    break;
                case 19:
                    return  "UInt4";
                    break;
                case 20:
                    return  "LLong";
                    break;
                case 21:
                    return  "QWord";
                    break;
            }

            return "undefined";
        }
    }
}
