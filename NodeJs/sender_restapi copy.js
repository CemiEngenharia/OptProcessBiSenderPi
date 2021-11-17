var http = require('http');
const url = require('url');
const MongoClient = require('mongodb').MongoClient;
const mongo = require('mongodb');
var net = require('net');
var mysql = require('mysql');
var qs = require('querystring');
const crypto = require('crypto');
var nodemailer = require('nodemailer');

var mysqlCon = mysql.createConnection({
  host: "mysql.optcemi.com",
  user: "optcemi",
  password: "c3m1t3ch",
  database: "optcemi"
});

mysqlCon.on("end", (data)=>{
    //print("mysql connection end", data);
    mysqlCon.connect((datacon)=>{
        //print("mysql connection connect called successfuly", datacon);
    });
});

mysqlCon.on("enqueue", (data) => {
    //print("mysql con enqueue", data);
});

//define um porta interna para intercomunicacao
_http_port = 21032;

//user = optcemi pass = c3m1t3ch
//username = DCDE3D475146D542AD90FAAE6C33416F
//password = 0E345608B2DEC5D12552C7D7FCAD660F

//basic auth key
const BASIC_AUTH_KEY = "Basic RENERTNENDc1MTQ2RDU0MkFEOTBGQUFFNkMzMzQxNkY6MEUzNDU2MDhCMkRFQzVEMTI1NTJDN0Q3RkNBRDY2MEY=";

//configura mongo
//const mongoUrl = 'mongodb://localhost:27017/?readPreference=primary&appname=OptProcessBISender&ssl=false&authSource=optcemi01&keepAlive=false&poolSize=5';

//const firstMongoUrl = 'mongodb://localhost:27017/?readPreference=primary&appname=OptProcessBISender&ssl=false&authSource=optcemi01&keepAlive=false&poolSize=5&agent=false';
const secondMongoUrl = 'mongodb://optcemi01:c3m1t3ch@mongodb.optcemi.com:27017/?readPreference=primary&appname=OptProcessBISender&ssl=false&authSource=optcemi01&keepAlive=false&poolSize=5&agent=false';
const thirdMongoUrl = 'mongodb+srv://optcemi01:c3m1t3ch@optcemi01.d3jgj.mongodb.net/test?authSource=admin&replicaSet=atlas-12tkwh-shard-0&readPreference=primary&appname=MongoDB%20Compass%20Community&ssl=true';
//const mongoUrl = 'mongodb://optcemi01:c3m1t3ch@mongodb.optcemi.com:27017/?readPreference=primary&appname=OptProcessBISender&ssl=false&authSource=optcemi01&keepAlive=false&poolSize=5';


//const mongoUrl = 'mongodb://optcemi01:c3m1t3ch@processbisync.optcemi.com:27017?authMechanism=DEFAULT&authSource=optcemi01';
//const mongoUrl = 'mongodb://mongodb.optcemi.com:27017';
const mongoDBName = 'optcemi01';

const ERROR_INVALIDDB = {"code": 500, "error": "Invalid Database", "data": null};
const ERROR_NOT_IMPLEMENTED = {"code": 500, "error": "Function Nor Implemented", "data": null};
const ERROR = {"code": 500, "error": "Undefined Error", "data": null};

var debugMode = true;

//let firstDatabase = null;
let secondDatabase = null;
let thirdDatabase = null;

let sessionTags = {}
    
//MongoClient.connect(firstMongoUrl, { "useUnifiedTopology": true, /*directConnection :true, keepAlive: false, maxPoolSize:20/*, autoReconnect: true*/},   function(err, client) {
//    if(err) throw err
//    
//   //conecta a base de dados ao iniciar o servidor
//    firstDatabase = client.db(mongoDBName);  
//});

MongoClient.connect(secondMongoUrl, { "useUnifiedTopology": true, /*directConnection :true, keepAlive: false, maxPoolSize:20/*, autoReconnect: true*/},   function(err, client) {
    if(err) throw err
    
    //conecta a base de dados ao iniciar o servidor
    secondDatabase = client.db(mongoDBName);  
});

MongoClient.connect(thirdMongoUrl, { "useUnifiedTopology": true, /*directConnection :true, keepAlive: false, maxPoolSize:20/*, autoReconnect: true*/},   function(err, client) {
    if(err) throw err
    
    //conecta a base de dados ao iniciar o servidor
    thirdDatabase = client.db(mongoDBName);  
    startServices();
});


//console.log("iniciado");

function print(data)
{
    if(debugMode)  
    {
        console.log("debug -> " , new Date().toLocaleString(),data);
    }
}
 
function contains(obj, key)
{
    if(Object.keys(obj).includes(key))      return true;
    return false;
}

function startServices(){
    
    console.log("Iniciados os serviços de socket");

    http.createServer(function (req, res) {

            var __bodyBuffer = "";
            var prefix = "";

            req.on('data', function (data) {
                //tenta transformar requisicao em algo legivel
                if(req.headers['content-type'].includes('multipart/form-data;'))
                {
                    prefix = req.headers['content-type'].substring(req.headers['content-type'].lastIndexOf("boundary=")).replace("boundary=" , "");
                }

                //console.log("data Received from request")
                __bodyBuffer += data.toString();
            });
                
            req.on('end', function () {
                console.log("pacote recebido de -> "+req.connection.remoteAddress+ " as => "+ Date().toString());

                let __postBody = {};

                const __userData__ = {};

                //adiciona cabecalho cors
                if(req.headers["origin"])
                    res.setHeader('Access-Control-Allow-Origin', req.headers["origin"]);
                else
                    res.setHeader('Access-Control-Allow-Origin', "*");

                res.setHeader('Access-Control-Allow-Credentials', 'true')
                res.setHeader('Access-Control-Allow-Methods', "GET, DELETE, PUT, POST, OPTIONS");
                res.setHeader('Access-Control-Request-Method', "GET, DELETE, PUT, POST, OPTIONS");
                res.setHeader('Access-Control-Allow-Headers' ,'X-Custom-Header, authorization, content-type');
                res.setHeader('content-type', 'application/json');

                //processa options do CORS
                if (req.method === "OPTIONS")
                {
                    res.statusCode = 200;
                    res.setHeader('Content-Type' ,'text/html; charset=utf-8');
                    res.write(JSON.stringify({"code":200, "status":"ok", "error":null, "data": ""}));        
                    res.end();
                    return;
                }

                const pathname = url.parse(req.url,true).pathname;
                const query = url.parse(req.url,true).query;

                if(pathname.includes("."))
                {
                    error = ERROR;
                    error["error"] = "Pathname Invalid";
                    res.write(JSON.stringify(error));
                    res.end();
                    return;
                }

                //tenta transformar requisicao em algo legivel
                if(prefix.length >= 3)
                {
                    //prefix = req.headers['content-type'].substring(req.headers['content-type'].lastIndexOf("boundary=")).replace("boundary=" , "");

                    sploteData = data.toString().split(prefix);

                    sploteData.forEach(part => {
                        matchingFields = part.match(/Content-Disposition:\sform-data;\sname\=\"(.+)\"\r\n\r\n(.+)\r\n/);
                        if(matchingFields != null)
                        {
                            //print(matchingFields);
                            if(matchingFields.length >= 3)
                            {
                                __postBody[matchingFields[1]] = matchingFields[2];
                            }
                        }
                    });

                    __postBody = __postBody.replace(/\r/g, "").replace(/\n/g, "")
                    print("__postBody line 138", __postBody.replace(/\r/g, "").replace(/\n/g, ""));
                }
                else
                {
                    try{
                        //console.log("__bodyBuffer.toString() => "+__bodyBuffer.toString());

                        //verifica se é um json e aciciona ao __postBody
                        var temp = JSON.parse(__bodyBuffer.toString());
                        Object.keys(temp).forEach((k) => __postBody[k] = temp[k]);
                    }
                    catch(e)
                    {
                        print("isnt JSON -> "+e.toString());
                    }
                }

                print("__postBody -> "+ JSON.stringify(__postBody));
                print("__Request -> "+ req.read()===null ? "null" : "notnull");
                print("__bodyBuffer -> "+ __bodyBuffer===null ? "null" : "notnull");

                //print(req.headers);
                //divide path em trechos
                slice = pathname.split(/\//g);

                print(slice);

                //Recebe uma lista de tags e retorna o valor delas
                //caso a tag nao exista ediciona a mesma ao dicionario de tags
                //dados recebidos como {'data': ['{tag name}:{tag address}:{tag type}','{tag name}:{tag address}:{tag type}']}
                //dados retornado como {'data': {'tag':'id','tag','id'}}
                if(slice["1"] == "setuptags")
                {
                    //verifica se conexao nao é tipo autenticacao
                    if (req.method === "POST")
                    {
                        print("is post");

                        if(__postBody['data'] != null)
                        {
                            print("has data");

                            //inicializa dados de tags para guardar ela pra secao
                            if(sessionTags[__postBody["project"]] == null)
                                sessionTags[__postBody["project"]] = {};

                            //adiciona device ao banco de dados caso nao exista
                            // entra na base de dados      
                            //////verificar conexão
    //                        findAndModifyTags(__postBody['data'], firstDatabase, 0, null, null);
                            //o project é para acesso aos dados da tag apos durante o insert economizando dados
                            if((secondDatabase) && (thirdDatabase))
                            {
                                findAndModifyTags(__postBody['project'], __postBody['data'], secondDatabase, 0, null, null);
                                findAndModifyTags(__postBody['project'], __postBody['data'], thirdDatabase, 0, res, null);
                            }
                            else
                            {
                                res.statusCode = 500;
                                res.write("{'status': 'error', 'desc':'there's one or more fields lost in request'}");
                                //res.write(JSON.stringify({"date": new Date().toISOString();, "status": false, "version": "1.0.10", "desc": "fail"}));
                                res.end()
                            }
                        }
                        else
                        {
                            res.statusCode = 500;
                            res.write("{'status': 'error to setup tags -> no data'}");
                            res.end();
                        }
                    }
                    else if (req.method === "GET")
                    {   
                        
                        print("is get");

                        res.write("{'status': 'ok'}");
                        res.end();
                    }
                    else if (req.method === "PUT")
                    {   
                        print("is get");

                        res.write("{'status': 'ok'}");
                        res.end();
                    }
                    else if (req.method === "DELETE")
                    {   
                        print("is get");

                        res.write("{'status': 'ok'}");
                        res.end();
                    }            
                }

                else if(slice["1"] == "tagdata")
                {
                    //verifica se conexao nao é tipo autenticacao
                    if (req.method === "POST")
                    {
                        //verifica se ha dados para processar
                        // project machineid date devicenumber deviceimei data
                        if((__postBody['project']  == null) ||
                            (__postBody['machineid']   == null) ||
                                (__postBody['date']   == null) ||
                                    (__postBody['devicenumber']   == null) || 
                                        (__postBody['deviceimei']   == null) ||
                                            (__postBody['data']   == null))
                        {
                            res.statusCode = 500;
                            res.write("{'status': 'error some lost fields'}");
                            res.end();
                            print("there's one or more fields lost in request")
                            return;
                        }
                        else
                        {
                            print("has data");
                            //adiciona device ao banco de dados caso nao exista       
                            //////verificar conexão 
    //                        addTagData(__postBody, firstDatabase, 0, null, null);
                            addTagData(__postBody, secondDatabase, 0, null, null);
                            addTagData(__postBody, thirdDatabase, 0, res, null);
                        }

                        
                    }
                    else if (req.method === "GET")
                    {   
                        print("get request");
                        print(query);

                        if(query["project"] != null && query["date_start"] != null && query["date_end"] != null)
                        {
                            var startD = query["date_start"].split(/\//g);
                            var endD = query["date_end"].split(/\//g);

                            query["date_start"] = new Date(startD[2], startD[1]-1, startD[0], 0, 0, 0, 0).toISOString();
                            query["date_end"] = new Date(endD[2], endD[1]-1, endD[0], 0, 0, 0, 0).toISOString();
                            //pega dados das tags
                            getTagData(query, thirdDatabase, res, client=null);
                        }
                        else
                        {
                            res.statusCode = 500;
                            res.write("{'status':'0', 'desc':'url query error'}");
                            res.end();
                        }
                    }
                    else if (req.method === "PUT")
                    {   
                        res.write(req.read());
                        res.end();
                    }
                    else if (req.method === "DELETE")
                    {   
                        res.write(req.read());
                        res.end();
                    }
                }

                /* Trabalha envio entre telemetrias */
                else if(slice[1] == "pertoper")
                {
                    //na url a posição sera de/para
                    senderid = slice[2];
                    getherid = slice[3];

                    //verifica se conexao nao é tipo autenticacao
                    if (req.method === "POST")
                    {
                        //verifica se ha dados para processar
                        // project machineid date devicenumber deviceimei data
                        if((senderid  == null) ||
                            (getherid   == null) ||
                                    (__postBody['data']   == null))
                        {
                            res.statusCode = 500;
                            res.write("{'status': 'error', 'desc':'there's one or more fields lost in request'}");
                            res.end();
                            print("there's one or more fields lost in request")
                            return;
                        }
                        else
                        {

                            //CALL getcycledata(sender_id, receiverid, tagname, tagvalue)
                            //CALL setcycledata("000000000000001","000000000000000","juniortester",1234)
                            //define query para adicao de dados
                            var updatequery = 'CALL setcycledata('+__postBody['senderid']+','+__postBody['receiverid']+','+"CYCLEDATA"+','+__postBody['tagdata']+');';

                            print("has data");

                            mysqlCon.query(updatequery, function (err, result, fields) {
                                if (err) throw err;
                                //console.log(result);
                                res.write(result)
                            });
                            
                            //adiciona device ao banco de dados caso nao exista       
                            //////verificar conexão 
                            res.end();
                        }
                    }
                    else if (req.method === "GET")
                    {   
                        //define query de leitura
                        //CALL getcycledata(sender_id, receiverid)
                        if((senderid  == null) ||
                            (getherid   == null))
                        {
                            res.statusCode = 500;
                            res.write("{'status': 'error', 'desc':'there's one or more fields lost in request'}");
                            res.end();
                            print("there's one or more fields lost in request")
                            return;
                        }
                        else
                        {
                            var updatequery =  'CALL getcycledata('+senderid+','+getherid+');';
                            
                            mysqlCon.query(updatequery, function (err, result, fields) {
                                if (err) throw err;
                                if(result)
                                {
                                    //console.log(result[0]);
                                    res.write(JSON.stringify(result[0]));
                                }
                                else
                                {
                                    res.write("{}");
                                }
                            });
                            
                            //res.write(req.read());
                            res.end();
                        }
                    }
                }
                else{
                    res.write("{'status':'ok'}")
                    res.end();
                }

            });            
    }).listen(_http_port);
}

//funcao para adicionar tags recursivamente
async function addTagData(data, _database, counter=0, res=null, client=null)
{
    let database = _database;

    query = {'project': data['project'],
    'machineid': data['machineid'],
    'date': data['date'],
    'device_number': data['devicenumber'],
    'device_imei': data['deviceimei']}

    //pega a chave do registro time
    time = (Object.keys(data["data"])[0]);
    var path = "data."+time;

    //cria registro das tags ou substitui
    value = {};

    tagAddress = null;
    tagIndex = null;
    
    if(! sessionTags.hasOwnProperty(data['project']))   sessionTags[data['project']] = {}

    //separa dados e lanca no bd
    for(j=0; j< data["data"][time].length; j++)
    {
        element = data["data"][time][j];

        //verificar se a tag existe no array, se nao existir carregar ela do banco e salvar no array
    

        if(! sessionTags[data['project']].hasOwnProperty(Object.keys(element)[0]))
        {
            //busca tag pelo indice 
            
            //seleciona a colecao das tags
            tagsCollection = database.collection('tag');
            

            //print("procurando -> " + Object.keys(element)[0].toString());

            
            //adiciona caso a caso
            tagsCollection.findOne({"index": parseInt(Object.keys(element)[0])}, function(err, result) {
                    //define valor a ser adicionado ao banco de dados
                    if (err) {print(err); throw(err)};

                    if(result === null)
                    {
                        tagAddress = null;

                        print("tagAddress Anulado")
                    }
                    else
                    {
                        //print(result);

                        tagAddress = result.address;
                        //define endereço de tags no documento atual
                        tagIndex = result.index;

                        sessionTags[data['project']][result.index] = result.address;
                    }
                });
            }
            else
            {
                tagAddress = sessionTags[data['project']][Object.keys(element)[0]];

                //define endereço de tags no documento atual
                tagIndex = Object.keys(element)[0];
            }

            try{
                /* using address as tag name
                value[path+"."+tagAddress+".value"] = element[Object.keys(element)[0]].split("`")[0];
                value[path+"."+tagAddress+".quality"] = element[Object.keys(element)[0]].split("`")[1];
                */

                /*using index as tag name*/
                value[path+"."+tagIndex+".value"] = element[Object.keys(element)[0]].split("`")[0];
                value[path+"."+tagIndex+".quality"] = element[Object.keys(element)[0]].split("`")[1];

                
                /*using index as tag name*/
                value["tag."+tagIndex] = tagAddress;
            }
            catch(except)
            {
                print(except);
            }
    }

    //print(value);

    //value[path] = data["data"][time];
    //value[path] = {"$each": data["data"][time]}; funciona para array
    //value[path] = data["data"][time]; //funciona para array
    //value[path] = data["data"][time];
    update = {};
    //update["$push"] = value;   funciona para array

    update["$set"] = value;

    //console.log("update")
    //console.log(update)
    //console.log("+++++++++++++++++++++++++++++++++++++")

    //seleciona coleção de dados
    tagCollection = database.collection('values');

    //procura e atualiza registro
    tagCollection.updateOne(query, update,{returnNewDocument: false, upsert: true, new: true}, (error) => 
    {
        if(!error) {
            //console.log(`Successfully updated document`)
            //console.log(`Successfully updated document: ${JSON.stringify(doc)}.`)

            if(res != null)
            {
                //resposta positiva    
                res.statusCode = 200;
                res.write("{'status': 'ok'}");
                res.end();
            }
        } else {
            //console.log("No document matches the provided query.")
            console.log(error)
            
            if(res != null)
            {
                //resposta positiva    
                res.statusCode = 500;
                res.write("{'status': 'error to update colections -> "+error.toString()+"'}");
                res.end();
            }
        }
        
    });  
}


//funcao para adicionar tags recursivamente
async function getTagData(data, database, res=null, client=null)
{
    /*
    query = [
        {
            "$match" : {
                'project' : data["project"],
            }
        },
        {
        '$project': {
            //'date':  {"$dateFromString" : { "dateString": '$date'}},
            'date':  '$date', 
            'project': '$project', 
            'data': {
            '$objectToArray': '$data'
            }
        }
        },
        {
            "$limit" : 3
        }, 
        {
        '$unwind': {
            'path': '$data'
        }
        }, 
        {
        '$project': {
            'project': '$project', 
            'date': {
            '$concat': [
                '$date', ' ', '$data.k'
            ]
            }, 
            'data': {
            '$objectToArray': '$data.v'
            }
        }
        },
        {
            "$sort" : {
                'date' : 1,                
            }
        },
        {
            "$match" : {
                "$and" : [
                    / *
                    {'date' : {"$gte" : {"$dateFromString" : { "dateString": data["date_start"]}}}},
                    {'date' : {"$lte" : {"$dateFromString" : { "dateString": data["date_end"]}}}}
                    * /
                    {'date' : {"$gte" : data["date_start"]}},
                    {'date' : {"$lte" : data["date_end"]}}
                ]
                
            }
        },
        {
        '$unwind': {
            'path': '$data'
        }
        },
        {
        '$project': {
            'project': '$project', 
            'date': '$date', 
            'tag': {
            '$toInt': '$data.k'
            }, 
            'value': {
            '$split': [
                '$data.v', '`'
            ]
            }
        }
        }, 
        {
        '$project': {
            'project': '$project', 
            'date': '$date', 
            'tag': '$tag', 
            'quality': {
            '$arrayElemAt': [
                '$value', 1
            ]
            }, 
            'value': {
            '$arrayElemAt': [
                '$value', 0
            ]
            }
        }
        },
        {
            "$sort" : {
                "tag" : 1
            }
        },
        {
        '$lookup': {
            'from': 'tag', 
            'localField': 'tag', 
            'foreignField': 'index', 
            'as': 'tag'
        }
        },
        {
        '$unwind': {
            'path': '$tag'
        }
        },
        {
        '$project': {
            'project': '$project', 
            'date': '$date', 
            'tag_name': '$tag.name', 
            'tag_address': '$tag.address', 
            'quality': '$quality', 
            'value': '$value'
        }
        }
    ];
*/

//adicionar date a query

    query = [{$match: {
        project: data["project"],
        date: "2021-04-20"
    }}, {$project: {
        date: '$date',
        project: '$project',
        data: {
        $objectToArray: '$data'
        }
    }}, {$unwind: {
        path: '$data'
    }}, {$project: {
        project: '$project',
        date: {
        $concat: [
            '$date',
            ' ',
            '$data.k'
        ]
        },
        data: {
        $objectToArray: '$data.v'
        }
    }}, {$project: {
        project: '$project',
        date: {
        $dateFromString: {
            dateString: '$date'
        }
        },
        data: "$data"
    }}, {$unwind: {
        path: '$data'
    }}, {$project: {
        project: '$project',
        date: '$date',
        tag: '$data.k',
        quality: "$data.v.quality",
        value: "$data.v.value"
    }}]
    ;

    print("\r\n\r\n");
    print(JSON.stringify(query));
    print("\r\n\r\n");
    
    //seleciona coleção de dados
    tagCollection = database.collection('values');

    //console.log(`criando cursor`);

    //procura e atualiza registro
    tagCollection.aggregate(query, {allowDiskUse: false, fullResponse: false}, (error, cursor) => 
    {
        if(!error) {
            //console.log(`cursor Successfully created to document`);
            //console.log(`Successfully updated document: ${JSON.stringify(doc)}.`)

            if(res != null)
            {
                cursor.get(function(err, result) {
                    if(err) print(err);

                    //assert.equal(null, cmdErr);
                    
                    print("________________result_________________");
                    print(result);
                    print("________________errors_________________");
                    print(err);
                    
                    //escreve resultado na resposta                    
                    //res.write(JSON.stringify({"code":200, "status":"ok", "error":null, "data": result}));
                    res.write(JSON.stringify(result));
                    res.end();
                    if(client != null)
                        client.close();
                });
            }
        } else {
            //console.log("No document matches the provided query.")
            //console.log(error)
            
            if(res != null)
            {
                //resposta positiva    
                res.statusCode = 500;
                res.write("{'status': 'error to agregate'}");
                res.end();
            }
        }
        
    });  
}

let output = {};

//funcao para adicionar tags recursivamente
async function findAndModifyTags(project, data, database, counter=0, res=null, client=null)
{
    if(counter == 0)
    {
        //console.log("flush de variaveis")
        respTimeout = null;
        output = {};
    }
    //encerra caso nao tenha mais dados no array
    if(counter >= data.length)
    {
        //limpa timeout
        if(respTimeout !== null)
            clearTimeout(respTimeout);

        //aguarda 1 segundo para receber outro chunk ou fechar a conexao
        respTimeout = setTimeout(() => {
            print("Tags Processadas Com Sucesso");
            // Close the connection
            if(client != null)
                client.close();

            if(res != null)
            {
                //finaliza consuta
                res.statusCode = 200;
                res.write(JSON.stringify(output))
                res.end();
            }
        }, 2000);
        return;
    }

    //pega elemento no array
    const element = data[counter];
    //divide a tag em duas partes, sendo a primeira o nome da tag e a outra o endereco da tag   retrona true se tg division for maior que 3
    //{tag name}:{tag address}:{tag type}


    const tag = element.split(/\:/g);
    if(tag.length > 3)     {findAndModifyTags(project, data, database, counter+1); return;}
    if(tag[1].length < 1)   {findAndModifyTags(project, data, database, counter+1); return;}

    tagType = "default";
    //cria hash com tagsddress + tagtype
    if(tag.length == 3)     tagType = tag[2];

    //seleciona a colecao das tags
    tagsCollection = database.collection('tag');

    //adiciona caso a caso
    await tagsCollection.findOne({"address": tag[1], "type": tagType}, async function(err, result) {
        //define valor a ser adicionado ao banco de dados
        if (err) {print(err); throw(err)};

        if(result === null)
        {
            //print("tag nao presente no banco");
            index = await getNextSequenceValue(database, "tagid");

            //define tagValue Padrao
            var tagValue = {"name": tag[0], "address": tag[1], "type": "default",  "index": index};

            if(tag.length == 3)
                tagValue["type"] = tag[2];

            await tagsCollection.insertOne(tagValue);
            
            //print("inserindo tag no banco");
            //print(tagValue);

            //responde na conexao
            //res.write(JSON.stringify(tagValue));
            //res.write(tagValue.address+":"+tagValue.index+",");
            output[tagValue.address] = tagValue.index;

            // e guarda dados das tags para insersao de dados
            sessionTags[project][tagValue.index] = tagValue.address;
                
            //chama funcao para o proximo do arary
            findAndModifyTags(project, data, database, counter+1, res, client)
        }
        else
        {
            //print("tag ja esta presente no banco");
            //res.write(JSON.stringify(result));
            //res.write(result.address+":"+result.index+",");
            output[result.address] = result.index;
            
            // e guarda dados das tags para insersao de dados
            sessionTags[project][result.index] = result.address;
                
            //chama funcao para o proximo do arary
            findAndModifyTags(project, data, database, counter+1, res, client)
        }
    });
}

var isDigit = (function() {
    var re = /^\d+$/;
    return function(c) {
        return re.test(c);
    }
}());

//implementa funcao de auto increment
async function getNextSequenceValue(db, sequenceName){

    var sequenceDocument = await db.collection('indexes').findAndModify(
        { "_id": sequenceName },
        {},
        { "$inc": { "sequence_value": 1 } }
    );

    return sequenceDocument.value.sequence_value;
}

function toUnixTimestamp(dateString)
{
    var newDate=new Date(dateString);
    return new Date(newDate).getTime()/1000;
}

function timeConverter(UNIX_timestamp){
    var a = new Date(UNIX_timestamp * 1000);
    /*
    var months = ['Jan','Feb','Mar','Apr','May','Jun','Jul','Aug','Sep','Oct','Nov','Dec'];
    var year = a.getFullYear();
    var month = months[a.getMonth()];
    var date = a.getDate();
    var hour = a.getHours();
    var min = a.getMinutes();
    var sec = a.getSeconds();
    */
    // Will display time in 10:30:23 format
    var time =  a.toLocaleDateString("br-BR") + " " + a.toLocaleTimeString("br-BR");
    //var time = date + ' ' + month + ' ' + year + ' ' + hour + ':' + min + ':' + sec ;
    return time;
}

function isDict(v) {
    //print("typeof", typeof(v));
    
    //print("typeof", typeof(v));
    
    return typeof v==='object' && v!==null && !(v instanceof Array) && !(v instanceof Date);
}

function isJson(v)
{
    try
    {
        JSON.parse(v);
        return true;
    }
    catch(e)
    {
        //print("isntJson");
        return false;
    }
}
print('Server running at :'+_http_port);