var Time;
var Seconds;

$(document).ready(function(){
    console.log("teste");
    //var varTrintaSeconds = trintaSeconds;
	//var NewTime = Time;
    var segundos;
    var minutos;
    var horas;
    var dia;
    var calc;

        $("#Time").change(function(ev) {
            ev.target;
            newdate - 
           alert("nada "  +$("#Time").val());

            console.log("valor de Time="  +$("#Time").val());
           
           
          //  $(".element").remove(".element");
         //   $(".resizer").remove(".resizer");
        });
    
   // });
       
       // nome= trintaSeconds;
        
       // if(NewTime == nome){
            
            
            
           // Seconds = 30;

       //    console.log("valor de Time"  +Time);
            
          //  return Seconds;

      //  }  
       
       // });
        /*
         <option value="trintaSeconds"> 30 seconds  </option>
                    <option value="umMinutes">     01 minutes  </option>
                    <option value="cincoMinutes">  05 minutes  </option>
                    <option value="quinzeMinutes"> 15 minutes  </option>
                    <option value="trintaMinutes"> 30 minutes  </option>
                    <option value="umHours">       01 hours    </option>                              
                    <option value="doisHours">     02 hours    </option>
                    <option value="quatroHours">   04 hours    </option>
                    <option value="oitoHours">     08 hours    </option> 
                    <option value="dozeHours">     12 hours    </option>
                    <option value="umDay">         01 day      </option>    
                    <option value="doisDay">       02 day      </option>   
                    <option value="tresDay">       03 day      </option>   
        */
        
    
	//pega valor maximo E MINIMO das tags
	var max = {};
	var min = {};
	
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
    selectedTagC = "Moagem_CX01_VazaoAgua_OptSim";
    selectedTagD = "Moagem_CI01_Pressao_OptProcess";
    selectedTagE = "Moagem_MO01_Potencia_Especifica_OptProcess";
    selectedTagF = "Moagem_CI01_Alimentacao_Densidade_OptSim";
    selectedTagG = "Moagem_CI01_OF_Retido_mesh100_OptSim";
    selectedTagH = "Moagem_CI01_OF_Retido_mesh100_sp_OptProcess";

    var myChart = new Chart(ctx, {
        type: 'line',
        data: {
            labels: labels,
            datasets: [
                {
                    label: selectedTagA,
                    data: tagA,
                    yAxisID: "A",
                    backgroundColor: [ 'rgb(0, 204, 0)', ],
                    borderColor:     [ 'rgb(0, 204, 0)', ],
                    borderWidth: 1
                },
                {
                    label: selectedTagB,
                    data: tagB,
                    yAxisID: 'B',
                    backgroundColor: [ 'rgb(51, 153, 102)', ],
                    borderColor:     [ 'rgb(51, 153, 102)', ],
                    borderWidth: 1
                },
               {
                    label: selectedTagC,
                    data: tagC,
                    yAxisID: 'C',
                    backgroundColor: [ 'rgb(225, 77, 115)', ],
                    borderColor:     [ 'rgb(225, 77, 115)', ],
                    borderWidth: 1

                },
                {
                    label: selectedTagD,
                    data: tagD,
                    yAxisID: 'D',
                    backgroundColor: [ 'rgb(255, 204, 0)', ],
                    borderColor:     [ 'rgb(255, 204, 0)', ],
                    borderWidth: 1
                }, 
                {
                    label: selectedTagE,
                    data: tagE,
                    yAxisID: 'E',
                    backgroundColor: [ 'rgb(92, 0, 153)', ],
                    borderColor:     [ 'rgb(92, 0, 153)', ],
                    borderWidth: 1
                }, {
                    label: selectedTagF,
                    data: tagF,
                    yAxisID: 'F',
                    backgroundColor: [ 'rgb(153, 31, 0)', ],
                    borderColor:     [ 'rgb(153, 31, 0)', ],
                    borderWidth: 1
                }, 
                {
                    label: selectedTagG,
                    data: tagG,
                    yAxisID: 'G',
                    backgroundColor: [ 'rgb(102, 204, 255)', ],
                    borderColor:     [ 'rgb(102, 204, 255)', ],
                    borderWidth: 1
                }, 
                {
                    label: selectedTagH,
                    data: tagH,
                    yAxisID: 'H',
                    backgroundColor: [ 'rgb(0, 64, 128)', ],
                    borderColor:     [ 'rgb(0, 64, 128)', ],
                    borderWidth: 1
                },
            ],    
        },
        options: {
            plugins: {
                legend: {
                    display: true,
                    position: 'bottom',
                    labels: {
                        fontColor: "#FFFFFFF",
                    }
                }
            },
            interaction: {
                display: false,
                mode: 'index',
                axis: 'y',
                boolean:true,
            },
			
			onHover: (evt, act) => {
				pai = "#ValueChart";
				
				//console.log(evt);
				//console.log(myChart.data.datasets);
				//console.log(evt.chart.config._config.data);
				
				if(act.length >= 1){
					//pega posicao do dado no eixo x
					x = act[0].element.parsed.x;
					
					//pega o data set do grafico
					dataset = evt.chart.config._config.data.datasets;
					console.log(act[0].element.parsed);
					console.log(evt.chart.config._config.data.labels[x]);
					
					//navega nos datasets lembrando que cada linha é um array
					for(l=0;l<dataset.length; l++){/*
						console.log(dataset[l].backgroundColor);
						console.log(dataset[l].borderColor);*/
						
						//escreve atual (verificar multiplicacao)
						console.log(dataset[l].label, dataset[l].data[x], max[dataset[l].label], (dataset[l].data[x] * max[dataset[l].label]));
						
						//pega valor do  ponto em que o mouse esta em x e a cor da linha atual atribuindo ao texto a cor da linha
						//divide por 100 devido a multiplicação la embaixo
						$("#"+dataset[l].label).val(parseFloat(dataset[l].data[x]));
						$("#"+dataset[l].label).css("color", dataset[l].backgroundColor[0]);

						//escreve maxima
						
						//console.log("max_"+dataset[l].label, dataset[l].data[x], max[dataset[l].label], dataset[l].data[x] * max[dataset[l].label]);
						
						//pega valor do  ponto em que o mouse esta em x e a cor da linha atual atribuindo ao texto a cor da linha
						$("#max_"+dataset[l].label).val(parseFloat(max[dataset[l].label]));
						$("#max_"+dataset[l].label).css("color", dataset[l].backgroundColor[0]);
						
						//escreve minima
						
						//console.log("min_"+dataset[l].label, dataset[l].data[x], max[dataset[l].label], dataset[l].data[x] * max[dataset[l].label]);
						
						//pega valor do  ponto em que o mouse esta em x e a cor da linha atual atribuindo ao texto a cor da linha
						$("#min_"+dataset[l].label).val(parseFloat(min[dataset[l].label]));
						$("#min_"+dataset[l].label).css("color", dataset[l].backgroundColor[0]);
					}
					
					console.log();
				}
			},

		 	scales : {
				y:{
					type: 'linear',
					position: 'left',
					ticks: {
						display: false,
						max: 1,
						min: 0
					},
					
				},
				A:{
					type: 'linear',
					position: 'left',
					display: false,
					ticks: {
						max: 1,
						min: 0
					},					
				},
                B:{
					type: 'linear',
					position: 'left',
						display: false,
					ticks: {
						max: 1,
						min: 0
					},
                },
                C:{
					type: 'linear',
					position: 'left',
						display: false,
					ticks: {
						max: 1,
						min: 0
					},
				},
                D:{
					type: 'linear',
					position: 'left',
						display: false,
					ticks: {
						max: 1,
						min: 0
					},
				},
                E:{
					type: 'linear',
					position: 'left',
						display: false,
					ticks: {
						max: 1,
						min: 0
					},
				},
                F:{
					type: 'linear',
					position: 'left',
						display: false,
					ticks: {
						max: 1,
						min: 0
					},
				},
                G:{
					type: 'linear',
					position: 'left',
						display: false,
					ticks: {
						max: 1,
						min: 0
					},
				},
                H:{
					type: 'linear',
					position: 'left',
						display: false,
					ticks: {
						max: 1,
						min: 0
					},
				},                
				x: {
					display: false,
				}
			},
             
            responsive: true,
            maintainAspectRatio: false,
  
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

                    tagname = tagdata.fullname;

                    while(tagname.startsWith(".")){
                        tagname = tagname.substring(1);
                    }

                    tagname =tagname.replace(/\./g,"-").replace(/ /g,"_");
				
                    function clone(obj) {
                        /*
                        if (null == obj || "object" != typeof obj) return obj;
                        var copy = obj.constructor();
                        for (var attr in obj) {
                            if (obj.hasOwnProperty(attr)) copy[attr] = obj[attr];
                        }
                        */
                        return obj*1;
                    }
                    
                    // PEGA O VALOR MAX DAS TAG'S
                    max[selectedTagA] = -1;
                    max[selectedTagB] = -1;
                    max[selectedTagC] = -1;
                    max[selectedTagD] = -1;
                    max[selectedTagE] = -1;
                    max[selectedTagF] = -1;
                    max[selectedTagG] = -1;
                    max[selectedTagH] = -1;
                    
                    
                    //PEGA O VALOR MIN DAS TAG'S 
                    min[selectedTagA] = 9999999;
                    min[selectedTagB] = 9999999;
                    min[selectedTagC] = 9999999;
                    min[selectedTagD] = 9999999;
                    min[selectedTagE] = 9999999;
                    min[selectedTagF] = 9999999;
                    min[selectedTagG] = 9999999;
                    min[selectedTagH] = 9999999;

                    try{
							if(currentDate < new Date(tagdata.currenttime).toLocaleString("br-BR")){
								if(selectedTagA == tagname){
									tagA.push(tagdata.currentvalue);
									j = tagA.length-1;	{if(max[selectedTagA] < tagA[j]) max[selectedTagA] = clone(tagA[j]);	if(min[selectedTagA] > tagA[j]) min[selectedTagA] = clone(tagA[j]);}
                                    
								}      

								if(selectedTagB == tagname){
									tagB.push(tagdata.currentvalue); 
                                    j = tagA.length-1;	{if(max[selectedTagB] < tagB[j]) max[selectedTagB] = clone(tagB[j]);	if(min[selectedTagB] > tagB[j]) min[selectedTagB] = clone(tagB[j]);}
                                    
								}

								if(selectedTagC == tagname){
									tagC.push(tagdata.currentvalue);
                                    j = tagA.length-1;	{if(max[selectedTagC] < tagC[j]) max[selectedTagC] = clone(tagC[j]);	if(min[selectedTagC] > tagC[j]) min[selectedTagC] = clone(tagC[j]);}
                                    
								}      

								if(selectedTagD == tagname){
									tagD.push(tagdata.currentvalue);
                                    j = tagA.length-1;	{if(max[selectedTagD] < tagD[j]) max[selectedTagD] = clone(tagD[j]);	if(min[selectedTagD] > tagD[j]) min[selectedTagD] = clone(tagD[j]);}				
                                    
								} 

								if(selectedTagE == tagname){
									tagE.push(tagdata.currentvalue);
                                    j = tagA.length-1;	{if(max[selectedTagE] < tagE[j]) max[selectedTagE] = clone(tagE[j]);	if(min[selectedTagE] > tagE[j]) min[selectedTagE] = clone(tagE[j]);}
                                    
								} 

								if(selectedTagF == tagname){
									tagF.push(tagdata.currentvalue);
                                    j = tagA.length-1;	{if(max[selectedTagF] < tagF[j]) max[selectedTagF] = clone(tagF[j]);	if(min[selectedTagF] > tagF[j]) min[selectedTagF] = clone(tagF[j]);}
                                    
								} 

								if(selectedTagG == tagname){
									tagG.push(tagdata.currentvalue);
                                    j = tagA.length-1;	{if(max[selectedTagG] < tagG[j]) max[selectedTagG] = clone(tagG[j]);	if(min[selectedTagG] > tagG[j]) min[selectedTagG] = clone(tagG[j]);}
                                    
								} 

								if(selectedTagH == tagname){
									tagH.push(tagdata.currentvalue);
                                    j = tagA.length-1;	{if(max[selectedTagH] < tagH[j]) max[selectedTagH] = clone(tagH[j]);	if(min[selectedTagH] > tagH[j]) min[selectedTagH] = clone(tagH[j]);}
								} 
							
								if(!labels.includes(new Date(tagdata.currenttime).toLocaleString("br-BR"))){								
									if(tagA.length < labels.length) tagA.push(0);
									if(tagB.length < labels.length) tagB.push(0);
									if(tagC.length < labels.length) tagC.push(0);
									if(tagD.length < labels.length) tagD.push(0);
									if(tagE.length < labels.length) tagE.push(0);
									if(tagF.length < labels.length) tagF.push(0);
									if(tagG.length < labels.length) tagG.push(0);
									if(tagH.length < labels.length) tagH.push(0);
									
									labels.push(new Date(tagdata.currenttime).toLocaleString("br-BR"));		
								}
							}
								
							if(i==response.desc.length-1){
								currentDate = new Date(tagdata.currenttime).toLocaleString("br-BR");  
							}

                    }catch(e){
                        console.log("erro to attribute tag value",e);
                    }
                    //console.log(tagname);
                }   
				
                myChart.update();
                console.log("update");
            },
            error: function (xhr, status){
                console.log("error", xhr,status);
            }
        });
    }, 5000);
});