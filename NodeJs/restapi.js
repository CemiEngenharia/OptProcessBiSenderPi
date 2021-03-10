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
const mongoUrl = 'mongodb://localhost:27017/?readPreference=primary&appname=MongoDB%20Compass&ssl=false&authSource=optcemi01';
//const mongoUrl = 'mongodb://optcemi01:c3m1t3ch@processbisync.optcemi.com:27017?authMechanism=DEFAULT&authSource=optcemi01';
//const mongoUrl = 'mongodb://mongodb.optcemi.com:27017';
const mongoDBName = 'sample';

const ERROR_INVALIDDB = {"code": 500, "error": "Invalid Database", "data": null};
const ERROR_NOT_IMPLEMENTED = {"code": 500, "error": "Function Nor Implemented", "data": null};
const ERROR = {"code": 500, "error": "Undefined Error", "data": null};

var debugMode = true;

console.log("iniciado");

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
 
http.createServer(function (req, res) {
    var __bodyBuffer = "";
    var prefix = "";

    req.on('data', function (data) {
        //tenta transformar requisicao em algo legivel
        if(req.headers['content-type'].includes('multipart/form-data;'))
        {
            prefix = req.headers['content-type'].substring(req.headers['content-type'].lastIndexOf("boundary=")).replace("boundary=" , "");
        }

        console.log("data Received from request")
        __bodyBuffer += data.toString();
    });
        
    req.on('end', function () {
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
                console.log("__bodyBuffer.toString() => "+__bodyBuffer.toString());

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
                    //adiciona device ao banco de dados caso nao exista
                    MongoClient.connect(mongoUrl, { "useUnifiedTopology": true },   async function(err, client) {
                        if(err) throw err
    
                        // entra na base de dados      
                        const database = client.db(mongoDBName);  
                        findAndModifyTags(__postBody['data'], database, 0, res, null);
                    });
                }
                else
                {
                    res.write("{'status': 'error'}");
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
                    res.write("{'status': 'error'}");
                    res.end();
                    print("there's one or more fields lost in request")
                    return;
                }
                else
                {
                    print("has data");
                    //adiciona device ao banco de dados caso nao exista
                    MongoClient.connect(mongoUrl, { "useUnifiedTopology": true },   async function(err, client) {
                        if(err) throw err
    
                        // entra na base de dados      
                        const database = client.db(mongoDBName);  
                        addTagData(__postBody, database, 0, res, null);
                    });
                }
            }
            else if (req.method === "GET")
            {   
                res.write(req.read());
                res.end();
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
        else{
            res.write("{'status':'ok'}")
        }

    });
        
}).listen(_http_port);

//funcao para adicionar tags recursivamente
async function addTagData(data, database, counter=0, res=null, client=null)
{
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
    //value[path] = data["data"][time];
    value[path] = {"$each": data["data"][time]};
    update = {};
    //update["$set"] = value;
    update["$push"] = value;

    console.log("update")
    console.log(update)
    console.log("+++++++++++++++++++++++++++++++++++++")

    //seleciona coleção de dados
    tagCollection = database.collection('values');

    //procura e atualiza registro
    tagCollection.findOneAndUpdate(query, update,{returnNewDocument: true, upsert: true, new: true}, (error, doc) => 
    {
        if(!error) {
            console.log(`Successfully updated document: ${JSON.stringify(doc)}.`)
        } else {
            console.log("No document matches the provided query.")
            console.log(error)
        }

        //resposta positiva    
        res.write("{'status': 'ok'}");
        res.end();
    });  
}

let output = {};

//funcao para adicionar tags recursivamente
async function findAndModifyTags(data, database, counter=0, res=null, client=null)
{
    if(counter == 0)
    {
        console.log("flush de variaveis")
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
            res.write(JSON.stringify(output))
            res.end();
        }, 2000);
        return;
    }

    //pega elemento no array
    const element = data[counter];
    //divide a tag em duas partes, sendo a primeira o nome da tag e a outra o endereco da tag   retrona true se tg division for maior que 3
    //{tag name}:{tag address}:{tag type}

    const tag = element.split(/\:/g);
    if(tag.length > 3)     {findAndModifyTags(data, database, counter+1); return;}
    if(tag[1].length < 1)   {findAndModifyTags(data, database, counter+1); return;}

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
            //chama funcao para o proximo do arary
            findAndModifyTags(data, database, counter+1, res, client)
        }
        else
        {
            //print("tag ja esta presente no banco");
            //res.write(JSON.stringify(result));
            //res.write(result.address+":"+result.index+",");
            output[result.address] = result.index;
            //chama funcao para o proximo do arary
            findAndModifyTags(data, database, counter+1, res, client)
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