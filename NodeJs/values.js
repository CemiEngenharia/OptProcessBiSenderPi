$(document).ready(function(){
    console.log("teste");
	 $.ajax({
            url: "http://optcemi.com:21509/simulation/Vale_Viga_simulation",
            type: "GET",
            crossDomain: true,
            //data: JSON.stringify(somejson),
            dataType: "json",
            success: function (response){
                console.log(response);

                for(i=0; i<response.desc.length; i++){
                    tagdata = response.desc[i];
                    tagname =tagdata.fullname;
					while(tagname.startsWith("."))
					{
						tagname = tagname.substring(1);
					}
                    tagname = tagname.replace(/\./g,"-").replace(/ /g,"_");
					
                    try{
                        $("input[name="+tagname+"]").val(tagdata.currentvalue.toFixed(2));
                    }catch(e){
                        console.log("erro to attribute tag value",e);
                    }
                }
            },
            error: function (xhr, status){
                console.log("error", xhr,status);
            }
        });

    setInterval(() => {
        $.ajax({
            url: "http://optcemi.com:21509/simulation/Vale_Viga_simulation",
            type: "GET",
            crossDomain: true,
            //data: JSON.stringify(somejson),
            dataType: "json",
            success: function (response){
                console.log(response);
                ultimoTime =0;
                maiorTag = 0;

                for(i=0; i<response.desc.length; i++){
                    tagdata = response.desc[i];
                    tagname =tagdata.fullname;
					while(tagname.startsWith(".")){
						tagname = tagname.substring(1);
					}
                    
                    tagname = tagname.replace(/\./g,"-").replace(/ /g,"_");
					if(tagdata.currenttime > ultimoTime){
                        maiorTag = tagdata.currenttime;

                        try{
                            $("input[name="+tagname+"]").val(tagdata.currentvalue.toFixed(2));
                            
                        }catch(e){
                            console.log("erro to attribute tag value",e);
                        }
                    }
                }
                ultimoTime =   maiorTag;
            },
            error: function (xhr, status){
                console.log("error", xhr,status);
            }
        });
    }, 5000);
});