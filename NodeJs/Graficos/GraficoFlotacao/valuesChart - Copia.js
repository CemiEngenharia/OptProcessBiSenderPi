$(document).ready(function(){
    console.log("teste");

    var ctx = document.getElementById('myChart').getContext('2d');

    var tagA = [];// [0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0];
    var tagB = [];
    var tagC = [];
    var tagD = [];
    var tagE = [];
    var tagF = [];
    var tagG = [];
    var tagH = [];
  
    var labels = [];// [0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0];

    selectedTagA = "Moagem_MO01_CargaCirculante_Percentual_OptProcess";
    selectedTagB = "Moagem_MO01_CargaCirculante_Percentual_sp_OptProcess";
    selectedTagC ="Moagem_CX01_VazaoAgua_OptSim";


    selectedTagD ="Moagem_CI01_Pressao_OptProcess";
    selectedTagE ="Moagem_MO01_Potencia_Especifica_OptProcess";
    selectedTagF ="Moagem_CI02_Alimentacao_Densidade_OptSim";
    selectedTagG ="Moagem_CI01_OF_Retido_mesh100_OptSim";
    selectedTagH ="Moagem_CI01_OF_Retido_mesh100_sp_OptProcess";

    var myChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [
                {
                    label: selectedTagA,
                    data: tagA,
                    backgroundColor: [ 'rgb(75, 192, 192)', ],
                    borderColor:     [ 'rgb(75, 192, 192)', ],
                    borderWidth: 1
                },
                {
                    label: selectedTagB,
                    data: tagB,
                    backgroundColor: [ 'rgb(33, 94, 94)', ],
                    borderColor:     [ 'rgb(33, 94, 94)', ],
                    borderWidth: 1
                },
               {
                    label: selectedTagC,
                    data: tagC,
                    backgroundColor: [ 'rgb(225, 77, 115)', ],
                    borderColor:     [ 'rgb(225, 77, 115)', ],
                    borderWidth: 1
                },
                {
                    label: selectedTagD,
                    data: tagD,
                    backgroundColor: [ 'rgb(255, 204, 0)', ],
                    borderColor:     [ 'rgb(255, 204, 0)', ],
                    borderWidth: 1
                }, {
                    label: selectedTagE,
                    data: tagE,
                    backgroundColor: [ 'rgb(92, 0, 153)', ],
                    borderColor:     [ 'rgb(92, 0, 153)', ],
                    borderWidth: 1
                }, {
                    label: selectedTagF,
                    data: tagF,
                    backgroundColor: [ 'rgb(153, 31, 0)', ],
                    borderColor:     [ 'rgb(153, 31, 0)', ],
                    borderWidth: 1
                }, {
                    label: selectedTagG,
                    data: tagG,
                    backgroundColor: [ 'rgb(102, 204, 255)', ],
                    borderColor:     [ 'rgb(102, 204, 255)', ],
                    borderWidth: 1
                }, {
                    label: selectedTagH,
                    data: tagH,
                    backgroundColor: [ 'rgb(0, 64, 128)', ],
                    borderColor:     [ 'rgb(0, 64, 128)', ],
                    borderWidth: 1
                },
            ],    
        },
        options: {
        responsive: true,
        maintainAspectRatio: false

        
    }
});

    currentDate = "";

    setInterval(() => {
        $.ajax({
            url: "http://optcemi.com:21509/simulation/Vale_Viga",
            type: "GET",
            crossDomain: true,
            //data: JSON.stringify(somejson),
            dataType: "json",
            success: function (response){
                console.log(response);
                /*
                tagA = [];
                tagB = [];
                labels = [];
*/
                ll = labels.length;
                for(i=0; i<ll; i++)
                {
                    tagA.shift();
                    tagB.shift();
                    tagC.shift();
                    tagD.shift();
                    tagE.shift();
                    tagF.shift();
                    tagG.shift();
                    tagH.shift();
                    labels.shift();					
                }        
					
					
                for(i=0; i<response.desc.length; i++){

                    tagdata = response.desc[i];

                    tagname =tagdata.fullname;

                    while(tagname.startsWith("."))
                    {
                        tagname = tagname.substring(1);
                    }

                    tagname =tagname.replace(/\./g,"-").replace(/ /g,"_");
                    
                    try{
							if(currentDate != new Date(tagdata.currenttime).toLocaleString("br-BR")){
								if(selectedTagA == tagname){
										tagA.push(tagdata.currentvalue);
								}      

								if(selectedTagB == tagname){
										tagB.push(tagdata.currentvalue);  
								}

								if(selectedTagC == tagname){
										tagC.push(tagdata.currentvalue);
								}      

								if(selectedTagD == tagname){
										tagD.push(tagdata.currentvalue);
								} 

								if(selectedTagE == tagname){
										tagE.push(tagdata.currentvalue);
								} 

								if(selectedTagF == tagname){
										tagF.push(tagdata.currentvalue);
								} 

								if(selectedTagG == tagname){
										tagG.push(tagdata.currentvalue);
								} 

								if(selectedTagH == tagname){
										tagH.push(tagdata.currentvalue);
								} 
							}
							
							if(!labels.includes(new Date(tagdata.currenttime).toLocaleString("br-BR"))){
								
								tagA.shift();
								tagB.shift();
								tagC.shift();
								tagD.shift();
								tagE.shift();
								tagF.shift();
								tagG.shift();
								tagH.shift();
								labels.shift();	
								
								labels.push(new Date(tagdata.currenttime).toLocaleString("br-BR"));		
							}
								
							if(i==response.desc.length-1){
								if(tagA.length < labels.length) tagA.push(0);
								if(tagB.length < labels.length) tagB.push(0);
								if(tagC.length < labels.length) tagC.push(0);
								if(tagD.length < labels.length) tagD.push(0);
								if(tagE.length < labels.length) tagE.push(0);
								if(tagF.length < labels.length) tagF.push(0);
								if(tagG.length < labels.length) tagG.push(0);
								if(tagH.length < labels.length) tagH.push(0);
								
								currentDate = new Date(tagdata.currenttime).toLocaleString("br-BR");  
							}

                    }catch(e){
                        console.log("erro to attribute tag value",e);
                    }
                    //console.log(tagname);
                }   
				
                console.log("tagA", tagA);  
                console.log("labels", labels);      
                        
                myChart.update();
                console.log("update");
            },
            error: function (xhr, status){
                console.log("error", xhr,status);
            }
        });
    }, 5000);
});