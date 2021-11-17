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
    <meta name="viewport" content="width=device-width, initial-scale=1">
    <head>
        <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.5.1/jquery.min.js"></script>
        <link rel="stylesheet" href="https://www.w3schools.com/w3css/4/w3.css">
        <link href='https://fonts.googleapis.com/css?family=RobotoDraft' rel='stylesheet' type='text/css'>
        <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/4.7.0/css/font-awesome.min.css">
        <link rel="stylesheet" href="./visual.css">
        <script src= "https://github.com/google/gson.git"></script>
        <link href='https://icons8.com.br/icons'>

        <script src="../../values.js"> </script>
        
        <link rel="stylesheet" href=".//bodySimulacaoDinamicaBritagem.css">
        <link rel="stylesheet" href="./labelSimulacao.css">
        <link rel="stylesheet" href="./menuSimulacao.css">
        <link rel="stylesheet" href="./table2.css">
        <link rel="stylesheet" href="./table3.css">

    </head>
    <body>
        <div>           
            <div> <img id="LogoCemi"    src="../../ImagensHMI/Cemi.jpeg">    </div> <!--LOGO CEMI--> 
            <div> <img id="Vale"        src="../../ImagensHMI/Vale1.jpg">    </div> <!--LOGO VALE-->      

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
                    <a href="../../global.php">                                     Global                  </a>
                    <li>
                        <a> Gráfico </a>
                        <ul>
                            <li>    <a href="../../Graficos/GraficoCmai/GraficoCmai.php">            CMAI            </a></li>  
                            <li>    <a href="../../Graficos/GraficoEspessamento/GraficoEspessamento.php">    Espessamento    </a></li>  
                            <li>    <a href="../../Graficos/GraficoFlotacao/GraficoFlotacao.php">        Flotação        </a></li>
                            <li>    <a href="../../Graficos/GraficoMoinho1/GraficoMoinho1.php">         Moinho 01       </a></li>
                            <li>    <a href="../../Graficos/GraficoMoinho2/GraficoMoinho2.php">         Moinho 02       </a></li>
                        </ul>
                    </li>   
                    <li>
                        <a> Simulação Dinâmica </a>
                        <ul>
                            <li>    <a href="./SimulacaoDinamicaBritagem.php">                                     Britagem           </a></li>
                            <li>    <a href="../SimulacaoDinamicaCmai1/SimulacaoDinamicaCmai1.php">                CMAI               </a></li>
                            <li>    <a href="../SimulacaoDinamicaDeslamagem/SimulacaoDinamicaDeslamagem.php">      Deslamagem         </a></li>
                            <li>    <a href="../SimulacaoDinamicaEspessamento/SimulacaoDinamicaEspessamento.php">  Espessamento       </a></li>
                            <li>    <a href="../SimulacaoDinamicaFlotacao/SimulacaoDinamicaFlotacao.php">          Flotação           </a></li>
                            <li>    <a href="../SimulacaoDinamicaMoagem/SimulacaoDinamicaMoagem.php">              Moagem             </a></li>
                            <li>    <a href="../SimulacaoDinamicaRom/SimulacaoDinamicaRom.php">                    ROM                </a></li>
                        </ul>
                    </li>   
                    <a href="../../SimulacaoIndicadores/SimulacaoIndicadores.php"> Simulação Indicadores   </a>
                </div>
            </nav>
            
            <!--TITULO--> 
            <label id="SimulacaoDeIndicadores"  type="text" name="SimulacaoDeIndicadores"> Simulação Dinâmica       </label>
            <label id="SalaDeControle"          type="text" name="SalaDeControle">         Britagem         </label>

            <!-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------->
            <table id="table2">
                <th rowspan="24"> BRITAGEM </th><!-- TÍTULO COLUNA-->
               
                <tr class="tr1"><!-- LINHA 1-->
                    <td> 75 </td>
                    <td> BritagemTerciaria_Britador_Descarga_Granulometria_D80_OptSim</td>
                    <td class="teste"><input  class="input1" type="text" id="BritagemTerciaria_Britador_Descarga_Granulometria_D80_OptSim" name="BritagemTerciaria_Britador_Descarga_Granulometria_D80_OptSim" value="0.00" disabled>&#x3BCm</td>
                </tr> 
                <!--------------------------------------------->                
                <tr class="tr2"><!-- LINHA 2-->
                    <td> 75 </td>
                    <td> BritagemTerciaria_Britador_Descarga_PercSol_OptSim</td>
                    <td class="teste"><input  class="input2" type="text" id="BritagemTerciaria_Britador_Descarga_PercSol_OptSim" name="BritagemTerciaria_Britador_Descarga_PercSol_OptSim" value="0.00" disabled> %</td>
                </tr> 
                <!--------------------------------------------->
                <tr class="tr1"><!-- LINHA 3 -->
                    <td> 75 </td>
                    <td> BritagemTerciaria_Britador_Descarga_Taxa_BaseSeca_OptSim</td>
                    <td class="teste"><input  class="input1" type="text" id="BritagemTerciaria_Britador_Descarga_Taxa_BaseSeca_OptSim" name="BritagemTerciaria_Britador_Descarga_Taxa_BaseSeca_OptSim" value="0.00" disabled> t/h</td>
                </tr> 
                <!--------------------------------------------->
                <tr class="tr2"><!-- LINHA 4 -->
                    <td> 75 </td>
                    <td> BritagemTerciaria_Britador_Descarga_Taxa_BaseUmida_OptSim  </td>
                    <td class="teste"><input  class="input2" type="text" id="BritagemTerciaria_Britador_Descarga_Taxa_BaseUmida_OptSim" name="BritagemTerciaria_Britador_Descarga_Taxa_BaseUmida_OptSim" value="0.00" disabled> t/h<t>
                    <h></h></td>
                </tr>
                <!--------------------------------------------->
                <tr class="tr1"><!-- LINHA 5 -->
                    <td> 75 </td>
                    <td> BritagemTerciaria_Britador_Descarga_Vazao_OptSIm</td>
                    <td class="teste"><input  class="input1" type="text" id="BritagemTerciaria_Britador_Descarga_Vazao_OptSIm" name="BritagemTerciaria_Britador_Descarga_Vazao_OptSIm" value="0.00" disabled> m&#x00B3/h</td>
                </tr>
                <!--------------------------------------------->
                <tr class="tr2"><!-- LINHA 6 -->
                    <td> 03 </td>
                    <td> BritagemTerciaria_Peneira0102_Alimentacao_Granulometria_D80_OptSim</td>
                    <td class="teste"><input  class="input2" type="text" id="BritagemTerciaria_Peneira0102_Alimentacao_Granulometria_D80_OptSim" name="BritagemTerciaria_Peneira0102_Alimentacao_Granulometria_D80_OptSim" value="0.00" disabled> &#x3BCm</td>
                </tr> 
                <!--------------------------------------------->
                <tr class="tr1"><!-- LINHA 7-->
                    <td> 03 </td>
                    <td> BritagemTerciaria_Peneira0102_Alimentacao_PercSol_OptSim</td>
                    <td class="teste"><input  class="input1" type="text" id="BritagemTerciaria_Peneira0102_Alimentacao_PercSol_OptSim" name="BritagemTerciaria_Peneira0102_Alimentacao_PercSol_OptSim" value="0.00" disabled> %</td>
                </tr> 
                <!--------------------------------------------->
                <tr class="tr2"><!-- LINHA 8-->
                    <td> 03 </td>
                    <td> BritagemTerciaria_Peneira0102_Alimentacao_Taxa_BaseSeca_OptSim</td>
                    <td class="teste"><input  class="input2" type="text" id="BritagemTerciaria_Peneira0102_Alimentacao_Taxa_BaseSeca_OptSim" name="BritagemTerciaria_Peneira0102_Alimentacao_Taxa_BaseSeca_OptSim" value="0.00" disabled> t/h</td>
                </tr> 
                <!--------------------------------------------->
                <tr class="tr1"><!-- LINHA 9 -->
                    <td> 03 </td>
                    <td> BritagemTerciaria_Peneira0102_Alimentacao_Taxa_BaseUmida_OptSim</td>
                    <td class="teste"><input class="input1" type="text" id="BritagemTerciaria_Peneira0102_Alimentacao_Taxa_BaseUmida_OptSim" name="BritagemTerciaria_Peneira0102_Alimentacao_Taxa_BaseUmida_OptSim" value="0.00" disabled> t/h</td>
                </tr>
                <!--------------------------------------------->             
                <tr class="tr2"><!-- LINHA 10 -->
                    <td> 03 </td>
                    <td> BritagemTerciaria_Peneira0102_Alimentacao_Vazao_OptSim</td>
                    <td class="teste"><input  class="input2" type="text" id="BritagemTerciaria_Peneira0102_Alimentacao_Vazao_OptSim" name="BritagemTerciaria_Peneira0102_Alimentacao_Vazao_OptSim" value="0.00" disabled> m&#x00B3/h</td>
                </tr> 
                <!--------------------------------------------->
                <tr class="tr1"><!-- LINHA 11-->
                    <td> 05 </td>
                    <td> BritagemTerciaria_Peneira0102_OS_Granulometria_D80_OptSim</td>
                    <td class="teste"><input  class="input1" type="text" id="BritagemTerciaria_Peneira0102_OS_Granulometria_D80_OptSim" name="BritagemTerciaria_Peneira0102_OS_Granulometria_D80_OptSim" value="0.00" disabled> &#x3BCm</td>
                </tr> 
                <!--------------------------------------------->
                <tr class="tr2"><!-- LINHA 12-->
                    <td> 04 </td>
                    <td> BritagemTerciaria_Peneira0102_US_Granulometria_D80_OptSim</td>
                    <td class="teste"><input  class="input2" type="text" id="BritagemTerciaria_Peneira0102_US_Granulometria_D80_OptSim" name="BritagemTerciaria_Peneira0102_US_Granulometria_D80_OptSim" value="0.00" disabled> &#x3BCm</td>
                </tr>
                <!--------------------------------------------->
                <tr class="tr1"><!-- LINHA 13-->
                    <td> 71 </td>
                    <td> BritagemTerciaria_Peneira_Alimentacao_Granulometria_D80_OptSim</td>
                    <td class="teste"><input  class="input1" type="text" id="BritagemTerciaria_Peneira_Alimentacao_Granulometria_D80_OptSim" name="BritagemTerciaria_Peneira_Alimentacao_Granulometria_D80_OptSim" value="0.00" disabled> &#x3BCm</td>
                </tr>
                <!--------------------------------------------->
                <tr class="tr2"><!-- LINHA 14-->
                    <td> 71 </td>
                    <td> BritagemTerciaria_Peneira_Alimentacao_PercSol_OptSim</td>
                    <td class="teste"><input  class="input2" type="text" id="BritagemTerciaria_Peneira_Alimentacao_PercSol_OptSim" name="BritagemTerciaria_Peneira_Alimentacao_PercSol_OptSim" value="0.00" disabled> %</td>
                </tr>
                <!--------------------------------------------->
                <tr class="tr1"><!-- LINHA 15-->
                    <td> 71 </td>
                    <td> BritagemTerciaria_Peneira_Alimentacao_Taxa_BaseSeca_OptSim</td>
                    <td class="teste"><input  class="input1" type="text" id="BritagemTerciaria_Peneira_Alimentacao_Taxa_BaseSeca_OptSim" name="BritagemTerciaria_Peneira_Alimentacao_Taxa_BaseSeca_OptSim" value="0.00" disabled> t/h</td>
                </tr>
                <!--------------------------------------------->
                <tr class="tr2"><!-- LINHA 16-->
                    <td> 71 </td>
                    <td> BritagemTerciaria_Peneira_Alimentacao_Taxa_BaseUmida_OptSim</td>
                    <td class="teste"><input  class="input2" type="text" id="BritagemTerciaria_Peneira_Alimentacao_Taxa_BaseUmida_OptSim" name="BritagemTerciaria_Peneira_Alimentacao_Taxa_BaseUmida_OptSim" value="0.00" disabled> t/h</td>
                </tr>
                <!--------------------------------------------->
                <tr class="tr1"><!-- LINHA 17-->
                    <td> 71 </td>
                    <td> BritagemTerciaria_Peneira_Alimentacao_Vazao_OptSim</td>
                    <td class="teste"><input  class="input1" type="text" id="BritagemTerciaria_Peneira_Alimentacao_Vazao_OptSim" name="BritagemTerciaria_Peneira_Alimentacao_Vazao_OptSim" value="0.00" disabled> m&#x00B3/h</td>
                </tr> 
                <!--------------------------------------------->
                <tr class="tr2"><!-- LINHA 18-->
                    <td> 74 </td>
                    <td> BritagemTerciaria_Peneira_OS_Granulometria_D80_OptSim</td>
                    <td class="teste"><input  class="input2" type="text" id="BritagemTerciaria_Peneira_OS_Granulometria_D80_OptSim" name="BritagemTerciaria_Peneira_OS_Granulometria_D80_OptSim" value="0.00" disabled> &#x3BCm</td>
                </tr>
                <!--------------------------------------------->
                <tr class="tr1"><!-- LINHA 19-->
                    <td> 74 </td>
                    <td> BritagemTerciaria_Peneira_OS_PercSol_OptSim</td>
                    <td class="teste"><input  class="input1" type="text" id="BritagemTerciaria_Peneira_OS_PercSol_OptSim" name="BritagemTerciaria_Peneira_OS_PercSol_OptSim" value="0.00" disabled> %</td>
                </tr> 
                <!--------------------------------------------->
                <tr class="tr2"><!-- LINHA 20-->
                    <td> 74 </td>
                    <td> BritagemTerciaria_Peneira_OS_Taxa_BaseSeca_OptSim</td>
                    <td class="teste"><input  class="input2" type="text" id="BritagemTerciaria_Peneira_OS_Taxa_BaseSeca_OptSim" name="BritagemTerciaria_Peneira_OS_Taxa_BaseSeca_OptSim" value="0.00" disabled> t/h</td>
                </tr>
                <!--------------------------------------------->
                <tr class="tr1"><!-- LINHA 21-->
                    <td> 74 </td>
                    <td> BritagemTerciaria_Peneira_OS_Taxa_BaseUmida_OptSim</td>
                    <td class="teste"><input  class="input1" type="text" id="BritagemTerciaria_Peneira_OS_Taxa_BaseUmida_OptSim" name="BritagemTerciaria_Peneira_OS_Taxa_BaseUmida_OptSim" value="0.00" disabled> t/h</td>
                </tr> 
                <!--------------------------------------------->
                <tr class="tr2"><!-- LINHA 22-->
                    <td> 74 </td>
                    <td> BritagemTerciaria_Peneira_OS_Vazao_OptSim</td>
                    <td class="teste"><input  class="input2" type="text" id="BritagemTerciaria_Peneira_OS_Vazao_OptSim" name="BritagemTerciaria_Peneira_OS_Vazao_OptSim" value="0.00" disabled> m&#x00B3/h</td>
                </tr>
                <!--------------------------------------------->
                <tr class="tr1"><!-- LINHA 23-->
                    <td> 73 </td>
                    <td> BritagemTerciaria_Peneira_US_Granulometria_D80_OptSim</td>
                    <td class="teste"><input  class="input1" type="text" id="BritagemTerciaria_Peneira_US_Granulometria_D80_OptSim" name="BritagemTerciaria_Peneira_US_Granulometria_D80_OptSim" value="0.00" disabled> &#x3BCm</td>
                </tr> 
            </table>

        </div>
    </body>
</html> 