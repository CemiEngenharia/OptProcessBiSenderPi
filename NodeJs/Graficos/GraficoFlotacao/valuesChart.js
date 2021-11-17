$(document).ready(function(){
    console.log("teste");

    var ctx = document.getElementById('myChart').getContext('2d');

    var tagA = [];// [0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0];
    var tagB = [];
    var tagC = [];
    var tagD = [];
    var tagE = [];
    var tagF = [];

    var labels = [];// [0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0];

    selectedTagA =  "Flotacao_Cleaner_Concentrado_Teor_Fe_OptSim";
    selectedTagB =  "Flotacao_Cleaner_Concentrado_Teor_Fe_sp_OptProcess";
    selectedTagC =  "Flotacao_Cleaner_Concentrado_Teor_SiO2_OptSim";
    selectedTagD =  "Flotacao_Cleaner_Concentrado_Teor_SiO2_sp_OptProcess";
    selectedTagE =  "Flotacao_Scavenger_Rejeito_Teor_Fe_OptSim";
    selectedTagF =  "Flotacao_Scavenger_Rejeito_Teor_SiO2_OptSim";

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
                }, 
                {
                    label: selectedTagE,
                    data: tagE,
                    backgroundColor: [ 'rgb(92, 0, 153)', ],
                    borderColor:     [ 'rgb(92, 0, 153)', ],
                    borderWidth: 1
                }, 
                {
                    label: selectedTagF,
                    data: tagF,
                    backgroundColor: [ 'rgb(153, 31, 0)', ],
                    borderColor:     [ 'rgb(153, 31, 0)', ],
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

	var first = 0;
	
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
				/*
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
                }        */
					
					
                for(i=0; i<response.desc.length; i++){

                    tagdata = response.desc[i];

                    tagname =tagdata.fullname;

                    while(tagname.startsWith("."))
                    {
                        tagname = tagname.substring(1);
                    }

                    tagname =tagname.replace(/\./g,"-").replace(/ /g,"_");
                    
                    try{
							if(currentDate < new Date(tagdata.currenttime).toLocaleString("br-BR")){
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

								if(!labels.includes(new Date(tagdata.currenttime).toLocaleString("br-BR")))
								{								
									if(tagA.length < labels.length) tagA.push(0);
									if(tagB.length < labels.length) tagB.push(0);
									if(tagC.length < labels.length) tagC.push(0);
									if(tagD.length < labels.length) tagD.push(0);
                                    if(tagE.length < labels.length) tagE.push(0);
									if(tagF.length < labels.length) tagF.push(0);
									
									labels.push(new Date(tagdata.currenttime).toLocaleString("br-BR"));		
								}
							}
								
							if(i==response.desc.length-1){
								/*
								for(j = 0 ; j< labels.length; j++)
								{	
									if(tagA.length < labels.length) tagA.push(0);
									if(tagB.length < labels.length) tagB.push(0);
									if(tagC.length < labels.length) tagC.push(0);
									if(tagD.length < labels.length) tagD.push(0);
									if(tagE.length < labels.length) tagE.push(0);
									if(tagF.length < labels.length) tagF.push(0);
									if(tagG.length < labels.length) tagG.push(0);
									if(tagH.length < labels.length) tagH.push(0);
								}
								*/
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