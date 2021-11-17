<?php

    if(isset($_COOKIE["session"]) == true){
        if(strlen($_COOKIE["session"]) !=  32){
            header("Location:  ../../index.php");
        }
    }	
    else{
        header("Location: ../../index.php");
    }
?>

<!DOCTYPE html>
<html lang="pt-br">
	<title>Simulador Dinâmico</title>
	<meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
   
    <head>
        <link rel="stylesheet" href="https://www.w3schools.com/w3css/4/w3.css">
        <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/4.7.0/css/font-awesome.min.css">
        <!-- <script src= "https://github.com/google/gson.git"></script>-->

        <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.5.1/jquery.min.js"></script> 
        <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>

        <script src="../../values.js"> </script>
        <script src="./valuesChart.js"> </script>
        <link rel="stylesheet" href="./bodyGrafico.css">
        <link rel="stylesheet" href="./labelGrafico.css">  
        <link rel="stylesheet" href="./menuGrafico.css">
        <link rel="stylesheet" href="./table1.css">
        <link rel="stylesheet" href="./Max.css">
        <link rel="stylesheet" href="./Min.css">
    </head>

    <body>
        
        <div> <img id="LogoCemi"    src="../../ImagensHMI/Cemi.jpeg">   </div> <!--LOGO CEMI--> 
        <div> <img id="Vale"        src="../../ImagensHMI/Vale1.jpg">   </div> <!--LOGO VALE-->                                                                                                                                           
             
        <nav class="dropdown">
            <a class="menu">MENU</a>
            <div class="dropdown-content">
                <li>
                    <a> HMI </a>
                    <ul>
                        <li>    <a href="../../HMI/Britagem/Britagem.php">                                                 Britagem                            </a></li>
                        <li>    <a href="../../HMI/Espessador/Espessador.php">                                             Espessador                          </a></li>
                        <li>    <a href="../../HMI/Flotacao/Flotacao.php">                                                 Flotação                            </a></li>
                        <li>    <a href="../../HMI/MoagemLinha01/MoagemLinha01.php">                                       Moagem - Linha 01                   </a></li>
                        <li>    <a href="../../HMI/MoagemLinha02/MoagemLinha02.php">                                       Moagem - Linha 02                   </a></li>
                        <li>    <a href="../../HMI/SeparacaoMagneticaAltaFrequencia/SeparacaoMagneticaAltaFrequencia.php"> Separação Magnética Alta Frequência </a></li>
                        <li>    <a href="../../HMI/SeparacaoMagneticaTambor/SeparacaoMagneticaTambor.php">                 Separação Magnética Tambor          </a></li>
                    </ul>
                </li>   
                <a href="../../global.php">    Global                  </a>
                <li>
                    <a> Gráfico </a>
                    <ul>
                        <li>    <a href="../GraficoCmai/GraficoCmai.php">                   CMAI            </a></li>  
                        <li>    <a href="../GraficoEspessamento/GraficoEspessamento.php">   Espessamento    </a></li>  
                        <li>    <a href="../GraficoFlotacao/GraficoFlotacao.php">           Flotação        </a></li>
                        <li>    <a href="./GraficoMoinho01.php">                            Moinho 01       </a></li>
                        <li>    <a href="../GraficoMoinho2/GraficoMoinho2.php">             Moinho 02       </a></li>
                    </ul>
                </li> 
                <li>
                    <a> Simulação Dinâmica </a>
                    <ul>
                        <li>    <a href="../../SimulacaoDinamica/SimulacaoDinamicaBritagem/SimulacaoDinamicaBritagem.php">          Britagem           </a></li>
                        <li>    <a href="../../SimulacaoDinamica/SimulacaoDinamicaCmai1/SimulacaoDinamicaCmai1.php">                CMAI               </a></li>
                        <li>    <a href="../../SimulacaoDinamica/SimulacaoDinamicaDeslamagem/SimulacaoDinamicaDeslamagem.php">      Deslamagem         </a></li>
                        <li>    <a href="../../SimulacaoDinamica/SimulacaoDinamicaEspessamento/SimulacaoDinamicaEspessamento.php">  Espessamento       </a></li>
                        <li>    <a href="../../SimulacaoDinamica/SimulacaoDinamicaFlotacao/SimulacaoDinamicaFlotacao.php">          Flotação           </a></li>
                        <li>    <a href="../../SimulacaoDinamica/SimulacaoDinamicaMoagem/SimulacaoDinamicaMoagem.php">              Moagem             </a></li>
                        <li>    <a href="../../SimulacaoDinamica/SimulacaoDinamicaRom/SimulacaoDinamicaRom.php">                    ROM                </a></li>
                    </ul>
                </li>   
                <a href="../../SimulacaoIndicadores/SimulacaoIndicadores.php"> Simulação Indicadores   </a>
            </div>
        </nav>
            
            <!--TITULO-->           
            <label id="SimulacaoDeIndicadores"  type="text" name="SimulacaoDeIndicadores"> Simulação de Indicadores  </label>
            <label id="SalaDeControle"          type="text" name="SalaDeControle">          Gráfico - Moinho 01       </label>

            <div id="divmyChart" >
                <canvas id="myChart" ></canvas>
            </div>
            <!--------------------------------------------------------------------------------------------------------------------------------->
                <select type="text" id="Time" name="Time">
                    <option value="null">                      </option>
                    <option value="30"> 30 seconds  </option>
                    <option value="60">     01 minutes  </option>
                    <option value="300">  05 minutes  </option>
                    <option value="900"> 15 minutes  </option>
                    <option value="1800"> 30 minutes  </option>
                    <option value="3600">       01 hours    </option>                              
                    <option value="14400">     02 hours    </option>
                    <option value="28800">   04 hours    </option>
                    <option value="43200">     08 hours    </option> 
                    <option value="86400">     12 hours    </option>
                    <option value="">         01 day      </option>    
                    <option value="">       02 day      </option>   
                    <option value="">       03 day      </option>   
                </select>    
            <!----------------------------------------------------------------------------------------------------------------------------------------------------------------------->
            <table id="ValueChart">
                <tr class="tr2"><!-- LINHA 1-->
                    <td> <input class="input2 TagA" type="text" id="Moagem_MO01_CargaCirculante_Percentual_OptProcess"      name="Moagem_MO01_CargaCirculante_Percentual_OptProcess"    value="0.00" disabled></td>
                    <td> <input class="input2 TagB" type="text" id="Moagem_MO01_CargaCirculante_Percentual_sp_OptProcess"   name="Moagem_MO01_CargaCirculante_Percentual_sp_OptProcess" value="0.00" disabled></td>
                </tr> 
                
                <tr class="tr2"><!-- LINHA 2-->
                    <td> <input class="input2 TagC" type="text" id="Moagem_CX01_VazaoAgua_OptSim"   name="Moagem_CX01_VazaoAgua_OptSim"     value="0.00" disabled></td>
                    <td> <input class="input2 TagD" type="text" id="Moagem_CI01_Pressao_OptProcess" name="Moagem_CI01_Pressao_OptProcess"   value="0.00" disabled></td>
                </tr> 
           
                <tr class="tr2"><!-- LINHA 3 -->
                    <td> <input class="input2 TagE" type="text" id="Moagem_MO01_Potencia_Especifica_OptProcess" name="Moagem_MO01_Potencia_Especifica_OptProcess" value="0.00" disabled></td>
                    <td> <input class="input2 TagF"  type="text" id="Moagem_CI01_Alimentacao_Densidade_OptSim" name="Moagem_CI01_Alimentacao_Densidade_OptSim" value="0.00" disabled></td>
                </tr> 

                <tr class="tr2"><!-- LINHA 4 -->
                    <td> <input class="input2 TagG" type="text" id="Moagem_CI01_OF_Retido_mesh100_OptSim"        name="Moagem_CI01_OF_Retido_mesh100_OptSim"        value="0.00" disabled></td>
                    <td> <input class="input2 TagH" type="text" id="Moagem_CI01_OF_Retido_mesh100_sp_OptProcess" name="Moagem_CI01_OF_Retido_mesh100_sp_OptProcess" value="0.00" disabled></td>
                </tr>
            </table>
            <!----------------------------------------------------------------------------------------------------------------------------------------------------------------------->
            <table id="Max">
                <tr class="tr2"><!-- LINHA 1-->
                    <td> <input class="input2 TagA" type="text" id="max_Moagem_MO01_CargaCirculante_Percentual_OptProcess"      name="Moagem_MO01_CargaCirculante_Percentual_OptProcess"    value="0.00" disabled></td>
                    <td> <input class="input2 TagB" type="text" id="max_Moagem_MO01_CargaCirculante_Percentual_sp_OptProcess"   name="Moagem_MO01_CargaCirculante_Percentual_sp_OptProcess" value="0.00" disabled></td>

                </tr> 
                
                <tr class="tr2"><!-- LINHA 2-->
                    <td> <input class="input2 TagC" type="text" id="max_Moagem_CX01_VazaoAgua_OptSim"   name="Moagem_CX01_VazaoAgua_OptSim"     value="0.00" disabled></td>
                    <td> <input class="input2 TagD" type="text" id="max_Moagem_CI01_Pressao_OptProcess" name="Moagem_CI01_Pressao_OptProcess"   value="0.00" disabled></td>
                </tr> 
           
                <tr class="tr2"><!-- LINHA 3 -->
                    <td> <input class="input2 TagE" type="text" id="max_Moagem_MO01_Potencia_Especifica_OptProcess" name="Moagem_MO01_Potencia_Especifica_OptProcess" value="0.00" disabled></td>
                    <td> <input class="input2 TagF"  type="text" id="max_Moagem_CI01_Alimentacao_Densidade_OptSim" name="Moagem_CI01_Alimentacao_Densidade_OptSim" value="0.00" disabled></td>
                </tr> 

                <tr class="tr2"><!-- LINHA 4 -->
                    <td> <input class="input2 TagG" type="text" id="max_Moagem_CI01_OF_Retido_mesh100_OptSim"        name="Moagem_CI01_OF_Retido_mesh100_OptSim"        value="0.00" disabled></td>
                    <td> <input class="input2 TagH" type="text" id="max_Moagem_CI01_OF_Retido_mesh100_sp_OptProcess" name="Moagem_CI01_OF_Retido_mesh100_sp_OptProcess" value="0.00" disabled></td>
                </tr>
            </table>
            <!----------------------------------------------------------------------------------------------------------------------------------------------------------------------->  
            <table id="Min">
                <tr class="tr2"><!-- LINHA 1-->
                    <td> <input class="input2 TagA" type="text" id="min_Moagem_MO01_CargaCirculante_Percentual_OptProcess"      name="Moagem_MO01_CargaCirculante_Percentual_OptProcess"    value="0.00" disabled></td>
                    <td> <input class="input2 TagB" type="text" id="min_Moagem_MO01_CargaCirculante_Percentual_sp_OptProcess"   name="Moagem_MO01_CargaCirculante_Percentual_sp_OptProcess" value="0.00" disabled></td>
                </tr> 
                
                <tr class="tr2"><!-- LINHA 2-->
                    <td> <input class="input2 TagC" type="text" id="min_Moagem_CX01_VazaoAgua_OptSim"   name="Moagem_CX01_VazaoAgua_OptSim"     value="0.00" disabled></td>
                    <td> <input class="input2 TagD" type="text" id="min_Moagem_CI01_Pressao_OptProcess" name="Moagem_CI01_Pressao_OptProcess"   value="0.00" disabled></td>
                </tr> 
           
                <tr class="tr2"><!-- LINHA 3 -->
                    <td> <input class="input2 TagE" type="text" id="min_Moagem_MO01_Potencia_Especifica_OptProcess" name="Moagem_MO01_Potencia_Especifica_OptProcess" value="0.00" disabled></td>
                    <td> <input class="input2 TagF"  type="text" id="min_Moagem_CI01_Alimentacao_Densidade_OptSim" name="Moagem_CI01_Alimentacao_Densidade_OptSim" value="0.00" disabled></td>
                </tr> 

                <tr class="tr2"><!-- LINHA 4 -->
                    <td> <input class="input2 TagG" type="text" id="min_Moagem_CI01_OF_Retido_mesh100_OptSim"        name="Moagem_CI01_OF_Retido_mesh100_OptSim"        value="0.00" disabled></td>
                    <td> <input class="input2 TagH" type="text" id="min_Moagem_CI01_OF_Retido_mesh100_sp_OptProcess" name="Moagem_CI01_OF_Retido_mesh100_sp_OptProcess" value="0.00" disabled></td>
                </tr>
            </table>

        </div>
    </body>
</html> 