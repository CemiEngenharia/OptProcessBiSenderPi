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

<!DOCTYPE html>
<html lang="pt-br">
	<title>SITE</title>
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
        <link rel="stylesheet" href="./bodySimulacaoDinamicaCmai2.css">
        <link rel="stylesheet" href="./labelSimulacao.css">
        <link rel="stylesheet" href="./menuSimulacao.css">
        <link rel="stylesheet" href="./table1.css">
        <link rel="stylesheet" href="./table2.css">
        <link rel="stylesheet" href="./table3.css">

    </head>

	<body>
        <div>
            <div> <img id="LogoCemi"        src="../../ImagensHMI/Cemi.jpeg">      </div> <!--LOGO CEMI--> 
            <div> <img id="Vale"            src="../../ImagensHMI/Vale1.jpg">      </div> <!--LOGO VALE-->                                                                                                                                                  
                 
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
                            <li>    <a href="../SimulacaoDinamicaBritagem/SimulacaoDinamicaBritagem.php">          Britagem           </a></li>
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
            <label id="SimulacaoDeIndicadores"  type="text" name="SimulacaoDeIndicadores"> Simulação Dinâmica   </label>
            <label id="SalaDeControle"          type="text" name="SalaDeControle">         CMAI - Página 2      </label> 
            
            <!--LABEL-->
            <label id="Pagina"         type="text" name="Pagina">        Página     </label>

            <!--LINK--> 
            <a id="LinkPagina1"     href="../SimulacaoDinamicaCmai1/SimulacaoDinamicaCmai1.php">   1   </a>
            <a id="LinkPagina2"     href="./SimulacaoDinamicaCmai2.php">                           2   </a>
            <a id="LinkPagina3"     href="../SimulacaoDinamicaCmai3/SimulacaoDinamicaCmai3.php">   3   </a>

            <!-------------------------------------------------------------------------------------------------------------------------------------------->
            <table id="table2">
                <th rowspan="23"> CMAI </th><!-- TÍTULO COLUNA--> 

                <tr class="tr1"><!-- LINHA 45-->
                    <td>  </td>
                    <td> CMAI_Rejeito_TC06_Teor_SiO2_OptSim  </td>
                   <td class="teste"><input  class="input1" type="text" id="CMAI_Rejeito_TC06_Teor_SiO2_OptSim" name="CMAI_Rejeito_TC06_Teor_SiO2_OptSim" value="0.00" disabled> %</td>
                </tr>
                <!-------------------------------------->
                <tr class="tr2"><!-- LINHA 46-->
                    <td>  </td>
                    <td> CMAI_Rougher_AltoCampo_Alimentacao_PercSol_OptSim  </td>
                   <td class="teste"><input  class="input2" type="text" id="CMAI_Rougher_AltoCampo_Alimentacao_PercSol_OptSim" name="CMAI_Rougher_AltoCampo_Alimentacao_PercSol_OptSim" value="0.00" disabled> %</td>
                </tr> 
                <!-------------------------------------->
                <tr class="tr1"><!-- LINHA 47-->
                    <td>  </td>
                    <td> CMAI_Rougher_AltoCampo_Alimentacao_Taxa_OptSim  </td>
                   <td class="teste"><input  class="input1" type="text" id="CMAI_Rougher_AltoCampo_Alimentacao_Taxa_OptSim" name="CMAI_Rougher_AltoCampo_Alimentacao_Taxa_OptSim" value="0.00" disabled> %</td>
                </tr>
                <!-------------------------------------->
                <tr class="tr2"><!-- LINHA 48-->
                    <td>  </td>
                    <td> CMAI_Rougher_AltoCampo_Alimentacao_Teor_Fe_OptSim  </td>
                   <td class="teste"><input  class="input2" type="text" id="CMAI_Rougher_AltoCampo_Alimentacao_Teor_Fe_OptSim" name="CMAI_Rougher_AltoCampo_Alimentacao_Teor_Fe_OptSim" value="0.00" disabled> %</td>
                </tr> 
                <!-------------------------------------->
                <tr class="tr1"><!-- LINHA 49-->
                    <td> 27 </td>
                    <td> CMAI_Rougher_AltoCampo_Alimentacao_Teor_SiO2_OptSim  </td>
                   <td class="teste"><input  class="input1" type="text" id="CMAI_Rougher_AltoCampo_Alimentacao_Teor_SiO2_OptSim" name="CMAI_Rougher_AltoCampo_Alimentacao_Teor_SiO2_OptSim" value="0.00" disabled> %</td>
                </tr> 
                 <!-------------------------------------->   
                <tr class="tr2"><!-- LINHA 50-->
                    <td> 27 </td>
                    <td> CMAI_Rougher_AltoCampo_Alimentacao_Vazao_OptSim</td>
                   <td class="teste"><input  class="input2" type="text" id="CMAI_Rougher_AltoCampo_Alimentacao_Vazao_OptSim" name="CMAI_Rougher_AltoCampo_Alimentacao_Vazao_OptSim" value="0.00" disabled> m&#x00B3/h</td>
                </tr>
                 <!-------------------------------------->   
                <tr class="tr1"><!-- LINHA 51-->
                    <td> 27 </td>
                    <td> CMAI_Rougher_AltoCampo_Concentrado_PercSol_OptSim  </td>
                   <td class="teste"><input  class="input1" type="text" id="CMAI_Rougher_AltoCampo_Concentrado_PercSol_OptSim" name="CMAI_Rougher_AltoCampo_Concentrado_PercSol_OptSim" value="0.00" disabled> %</td>
                </tr>
                <!-------------------------------------->                
                <tr class="tr2"><!-- LINHA 52 -->
                    <td> 30 </td>
                    <td> CMAI_Rougher_AltoCampo_Concentrado_Taxa_OptSim </td>
                   <td class="teste"><input  class="input2" type="text" id="CMAI_Rougher_AltoCampo_Concentrado_Taxa_OptSim" name="CMAI_Rougher_AltoCampo_Concentrado_Taxa_OptSim" value="0.00" disabled> t/h</td>
                </tr>
                <!-------------------------------------->
                <tr class="tr1"><!-- LINHA 53-->
                    <td> 29 </td>
                    <td> CMAI_Rougher_AltoCampo_Concentrado_Teor_Fe_OptSim  </td>
                   <td class="teste"><input  class="input1" type="text" id="CMAI_Rougher_AltoCampo_Concentrado_Teor_Fe_OptSim" name="CMAI_Rougher_AltoCampo_Concentrado_Teor_Fe_OptSim" value="0.00" disabled> %</td>
                </tr> 
                <!-------------------------------------->
                <tr class="tr2"><!-- LINHA 54-->
                    <td> 29 </td>
                    <td> CMAI_Rougher_AltoCampo_Concentrado_Teor_SiO2_OptSim  </td>
                   <td class="teste"><input  class="input2" type="text" id="CMAI_Rougher_AltoCampo_Concentrado_Teor_SiO2_OptSim" name="CMAI_Rougher_AltoCampo_Concentrado_Teor_SiO2_OptSim" value="0.00" disabled> %</td>
                </tr> 
                <!-------------------------------------->
                <tr class="tr1"><!-- LINHA 55 -->
                    <td> 29 </td>
                    <td> CMAI_Rougher_AltoCampo_Concentrado_Vazao_OptSim</td>
                   <td class="teste"><input  class="input1" type="text" id="CMAI_Rougher_AltoCampo_Concentrado_Vazao_OptSim" name="CMAI_Rougher_AltoCampo_Concentrado_Vazao_OptSim" value="0.00" disabled> m&#x00B3/h</td>
                </tr>
                <!-------------------------------------->            
                <tr class="tr2"><!-- LINHA 56-->
                    <td> 27 </td>
                    <td> CMAI_Rougher_AltoCampo_Rejeito_PercSol_OptSim  </td>
                   <td class="teste"><input  class="input2" type="text" id="CMAI_Rougher_AltoCampo_Rejeito_PercSol_OptSim" name="CMAI_Rougher_AltoCampo_Rejeito_PercSol_OptSim" value="0.00" disabled> %</td>
                </tr> 
                <!-------------------------------------->    
                <tr class="tr1"><!-- LINHA 57-->
                    <td> 27 </td>
                    <td> CMAI_Rougher_AltoCampo_Rejeito_Taxa_OptSim</td>
                   <td class="teste"><input  class="input1" type="text" id="CMAI_Rougher_AltoCampo_Rejeito_Taxa_OptSim" name="CMAI_Rougher_AltoCampo_Rejeito_Taxa_OptSim" value="0.00" disabled> t/h</td>
                </tr>
                 <!-------------------------------------->   
                <tr class="tr2"><!-- LINHA 58-->
                    <td> 27 </td>
                    <td> CMAI_Rougher_AltoCampo_Rejeito_Teor_Fe_OptSim  </td>
                   <td class="teste"><input  class="input2" type="text" id="CMAI_Rougher_AltoCampo_Rejeito_Teor_Fe_OptSim" name="CMAI_Rougher_AltoCampo_Rejeito_Teor_Fe_OptSim" value="0.00" disabled> %</td>
                </tr>
                 <!-------------------------------------->   
                <tr class="tr1"><!-- LINHA 59-->
                    <td> 27 </td>
                    <td> CMAI_Rougher_AltoCampo_Rejeito_Teor_SiO2_OptSim</td>
                   <td class="teste"><input  class="input1" type="text" id="CMAI_Rougher_AltoCampo_Rejeito_Teor_SiO2_OptSim" name="CMAI_Rougher_AltoCampo_Rejeito_Teor_SiO2_OptSim" value="0.00" disabled> %</td>
                </tr>
                <!-------------------------------------->
                <tr class="tr2"><!-- LINHA 60-->
                    <td> 27 </td>
                    <td> CMAI_Rougher_AltoCampo_Rejeito_Vazao_OptSim  </td>
                   <td class="teste"><input class="input2" type="text" id="CMAI_Rougher_AltoCampo_Rejeito_Vazao_OptSim" name="CMAI_Rougher_AltoCampo_Rejeito_Vazao_OptSim" value="0.00" disabled> m&#x00B3/h</td>
                </tr>
                <!-------------------------------------->
                <tr class="tr1"><!-- LINHA 61 -->
                    <td> 24 </td>
                    <td> CMAI_Rougher_CI0102_Alimentacao_PercSol_OptSim </td>
                   <td class="teste"><input class="input1" type="text" id="CMAI_Rougher_CI0102_Alimentacao_PercSol_OptSim" name="CMAI_Rougher_CI0102_Alimentacao_PercSol_OptSim" value="0.00" disabled> %</td>
                </tr>
                <!-------------------------------------->
                <tr class="tr2"><!-- LINHA 62 -->
                    <td> 24 </td>
                    <td> CMAI_Rougher_CI0102_Alimentacao_Taxa_OptSim </td>
                   <td class="teste"><input class="input2" type="text" id="CMAI_Rougher_CI0102_Alimentacao_Taxa_OptSim" name="CMAI_Rougher_CI0102_Alimentacao_Taxa_OptSim" value="0.00" disabled> t/h</td>
                </tr>
                <!-------------------------------------->
                <tr class="tr1"><!-- LINHA 63-->
                    <td> 24 </td>
                    <td> CMAI_Rougher_CI0102_Alimentacao_Teor_Fe_OptSim  </td>
                   <td class="teste"><input class="input1" type="text" id="CMAI_Rougher_CI0102_Alimentacao_Teor_Fe_OptSim" name="CMAI_Rougher_CI0102_Alimentacao_Teor_Fe_OptSim" value="0.00" disabled> %</td>
                </tr> 
                <!-------------------------------------->
                <tr class="tr2"><!-- LINHA 64-->
                    <td> 24 </td>
                    <td> CMAI_Rougher_CI0102_Alimentacao_Teor_SiO2_OptSim </td>
                   <td class="teste"><input  class="input2" type="text" id="CMAI_Rougher_CI0102_Alimentacao_Teor_SiO2_OptSim" name="CMAI_Rougher_CI0102_Alimentacao_Teor_SiO2_OptSim" value="0.00" disabled> %</td>
                </tr>
                <!-------------------------------------->
                <tr class="tr1"><!-- LINHA 65-->
                    <td> 24 </td>
                    <td> CMAI_Rougher_CI0102_Alimentacao_Vazao_OptSim  </td>
                   <td class="teste"><input  class="input1" type="text" id="CMAI_Rougher_CI0102_Alimentacao_Vazao_OptSim" name="CMAI_Rougher_CI0102_Alimentacao_Vazao_OptSim" value="0.00" disabled> m&#x00B3/h</td>
                </tr> 
                <!-------------------------------------->
                <tr class="tr1"><!-- LINHA 66-->
                    <td> 25 </td>
                    <td> CMAI_Rougher_CI0102_OF_PercSol_OptSim </td>
                   <td class="teste"><input  class="input1" type="text" id="CMAI_Rougher_CI0102_OF_PercSol_OptSim" name="CMAI_Rougher_CI0102_OF_PercSol_OptSim" value="0.00" disabled> %</td>
                </tr>
            </table>
            <!------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------->
            <table id="table3">
                <th rowspan="23"> CMAI </th><!-- TÍTULO COLUNA-->

                <tr class="tr1"><!-- LINHA 67-->
                    <td> 25 </td>
                    <td> CMAI_Rougher_CI0102_OF_Taxa_OptSim  </td>
                   <td class="teste"><input  class="input1" type="text" id="CMAI_Rougher_CI0102_OF_Taxa_OptSim" name="CMAI_Rougher_CI0102_OF_Taxa_OptSim" value="0.00" disabled> t/h</td>
                </tr>
                <!-------------------------------------->
                <tr class="tr2"><!-- LINHA 68-->
                    <td> 25 </td>
                    <td> CMAI_Rougher_CI0102_OF_Teor_Fe_OptSim </td>
                   <td class="teste"><input  class="input2" type="text" id="CMAI_Rougher_CI0102_OF_Teor_Fe_OptSim" name="CMAI_Rougher_CI0102_OF_Teor_Fe_OptSim" value="0.00" disabled> %</td>
                </tr>
                <!-------------------------------------->
                <tr class="tr1"><!-- LINHA 69-->
                    <td> 25 </td>
                    <td> CMAI_Rougher_CI0102_OF_Teor_SiO2_OptSim  </td>
                   <td class="teste"><input  class="input1" type="text" id="CMAI_Rougher_CI0102_OF_Teor_SiO2_OptSim" name="CMAI_Rougher_CI0102_OF_Teor_SiO2_OptSim" value="0.00" disabled> %</td>
                </tr> 
                <!-------------------------------------->
                <tr class="tr2"><!-- LINHA 70-->
                    <td> 25 </td>
                    <td> CMAI_Rougher_CI0102_OF_Vazao_OptSim</td>
                   <td class="teste"><input  class="input2" type="text" id="CMAI_Rougher_CI0102_OF_Vazao_OptSim" name="CMAI_Rougher_CI0102_OF_Vazao_OptSim" value="0.00" disabled> m&#x00B3/h</td>
                </tr>
                <!-------------------------------------->
                <tr class="tr1"><!-- LINHA 71-->
                    <td> 26 </td>
                    <td> CMAI_Rougher_CI0102_UF_PercSol_OptSim  </td>
                   <td class="teste"><input  class="input1" type="text" id="CMAI_Rougher_CI0102_UF_PercSol_OptSim" name="CMAI_Rougher_CI0102_UF_PercSol_OptSim" value="0.00" disabled> %</td>
                </tr> 
                <!-------------------------------------->
                <tr class="tr2"><!-- LINHA 72-->
                    <td> 26 </td>
                    <td> CMAI_Rougher_CI0102_UF_Taxa_OptSim</td>
                   <td class="teste"><input  class="input2" type="text" id="CMAI_Rougher_CI0102_UF_Taxa_OptSim" name="CMAI_Rougher_CI0102_UF_Taxa_OptSim" value="0.00" disabled> t/h</td>
                </tr>                         
                <!-------------------------------------->
                <tr class="tr1"><!-- LINHA 73-->
                    <td> 26 </td>
                    <td> CMAI_Rougher_CI0102_UF_Teor_SiO2_OptSim  </td>
                   <td class="teste"><input  class="input1" type="text" id="CMAI_Rougher_CI0102_UF_Teor_SiO2_OptSim" name="CMAI_Rougher_CI0102_UF_Teor_SiO2_OptSim" value="0.00" disabled> %</td>
                </tr> 
                <!-------------------------------------->
                <tr class="tr2"><!-- LINHA 74-->
                    <td> 26 </td>
                    <td> CMAI_Rougher_CI0102_UF_Vazao_OptSim</td>
                   <td class="teste"><input  class="input2" type="text" id="CMAI_Rougher_CI0102_UF_Vazao_OptSim" name="CMAI_Rougher_CI0102_UF_Vazao_OptSim" value="0.00" disabled> m&#x00B3/h</td>
                </tr>
                <!-------------------------------------->
                <tr class="tr1"><!-- LINHA 75-->
                    <td> 08 </td>
                    <td> CMAI_Rougher_Tambor_Alimentacao_PercSol_OptSim  </td>
                   <td class="teste"><input  class="input1" type="text" id="CMAI_Rougher_Tambor_Alimentacao_PercSol_OptSim" name="CMAI_Rougher_Tambor_Alimentacao_PercSol_OptSim" value="0.00" disabled> %</td>
                </tr> 
                <!-------------------------------------->
                <tr class="tr2"><!-- LINHA 76-->
                    <td> 08 </td>
                    <td> CMAI_Rougher_Tambor_Alimentacao_Taxa_BaseSeca_OptSim</td>
                   <td class="teste"><input  class="input2" type="text" id="CMAI_Rougher_Tambor_Alimentacao_Taxa_BaseSeca_OptSim" name="CMAI_Rougher_Tambor_Alimentacao_Taxa_BaseSeca_OptSim" value="0.00" disabled> t/h</td>
                </tr>
                <!-------------------------------------->
                <tr class="tr1"><!-- LINHA 77-->
                    <td> 08 </td>
                    <td> CMAI_Rougher_Tambor_Alimentacao_Taxa_BaseUmida_OptSim  </td>
                   <td class="teste"><input  class="input1" type="text" id="CMAI_Rougher_Tambor_Alimentacao_Taxa_BaseUmida_OptSim" name="CMAI_Rougher_Tambor_Alimentacao_Taxa_BaseUmida_OptSim" value="0.00" disabled> %</td>
                </tr> 
                <!-------------------------------------->
                <tr class="tr2"><!-- LINHA 78-->
                    <td> 08 </td>
                    <td> CMAI_Rougher_Tambor_Alimentacao_Vazao_OptSim</td>
                   <td class="teste"><input  class="input2" type="text" id="CMAI_Rougher_Tambor_Alimentacao_Vazao_OptSim" name="CMAI_Rougher_Tambor_Alimentacao_Vazao_OptSim" value="0.00" disabled> m&#x00B3/h</td>
                </tr> 
                <!-------------------------------------->
                <tr class="tr1"><!-- LINHA 79-->
                    <td> 11 </td>
                    <td> CMAI_Rougher_Tambor_Concentrado_Granulometria_D80_OptSim  </td>
                   <td class="teste"><input  class="input1" type="text" id="CMAI_Rougher_Tambor_Concentrado_Granulometria_D80_OptSim" name="CMAI_Rougher_Tambor_Concentrado_Granulometria_D80_OptSim" value="0.00" disabled> &#x3BCm/mm</td>
                </tr>
                <!-------------------------------------->
                <tr class="tr2"><!-- LINHA 80-->
                    <td> 11 </td>
                    <td> CMAI_Rougher_Tambor_Concentrado_Peneira_OS_Granulometria_D80_OptSim</td>
                   <td class="teste"><input  class="input2" type="text" id="CMAI_Rougher_Tambor_Concentrado_Peneira_OS_Granulometria_D80_OptSim" name="CMAI_Rougher_Tambor_Concentrado_Peneira_OS_Granulometria_D80_OptSim" value="0.00" disabled> &#x3BCm/mm</td>
                </tr>
                <!-------------------------------------->
                <tr class="tr1"><!-- LINHA 81-->
                    <td> 11 </td>
                    <td> CMAI_Rougher_Tambor_Concentrado_Peneira_US_Granulometria_D80_OptSim  </td>
                   <td class="teste"><input  class="input1" type="text" id="CMAI_Rougher_Tambor_Concentrado_Peneira_US_Granulometria_D80_OptSim" name="CMAI_Rougher_Tambor_Concentrado_Peneira_US_Granulometria_D80_OptSim" value="0.00" disabled> &#x3BCm/mm</td>
                </tr> 
                <!-------------------------------------->
                <tr class="tr2"><!-- LINHA 82-->
                    <td> 11 </td>
                    <td> CMAI_Rougher_Tambor_Concentrado_PercSol_OptSim</td>
                   <td class="teste"><input  class="input2" type="text" id="CMAI_Rougher_Tambor_Concentrado_PercSol_OptSim" name="CMAI_Rougher_Tambor_Concentrado_PercSol_OptSim" value="0.00" disabled> %</td>
                </tr>
                <!-------------------------------------->   
                <tr class="tr1"><!-- LINHA 83-->
                    <td> 11 </td>
                    <td> CMAI_Rougher_Tambor_Concentrado_Taxa_BaseSeca_OptSim  </td>
                   <td class="teste"><input  class="input1" type="text" id="CMAI_Rougher_Tambor_Concentrado_Taxa_BaseSeca_OptSim" name="CMAI_Rougher_Tambor_Concentrado_Taxa_BaseSeca_OptSim" value="0.00" disabled> t/h</td>
                </tr>
                <!-------------------------------------->
                <tr class="tr2"><!-- LINHA 84-->
                    <td> 11 </td>
                    <td> CMAI_Rougher_Tambor_Concentrado_Taxa_BaseUmida_OptSim</td>
                   <td class="teste"><input  class="input2" type="text" id="CMAI_Rougher_Tambor_Concentrado_Taxa_BaseUmida_OptSim" name="CMAI_Rougher_Tambor_Concentrado_Taxa_BaseUmida_OptSim" value="0.00" disabled> t/h</td>
                </tr>
                <!-------------------------------------->   
                <tr class="tr1"><!-- LINHA 85-->
                    <td> 11 </td>
                    <td> CMAI_Rougher_Tambor_Concentrado_Vazao_OptSim  </td>
                   <td class="teste"><input  class="input1" type="text" id="CMAI_Rougher_Tambor_Concentrado_Vazao_OptSim" name="CMAI_Rougher_Tambor_Concentrado_Vazao_OptSim" value="0.00" disabled> m&#x00B3/h</td>
                </tr>
                <!-------------------------------------->
                <tr class="tr2"><!-- LINHA 86-->
                    <td> 10 </td>
                    <td> CMAI_Rougher_Tambor_Rejeito_Granulometria_D80_OptSim</td>
                   <td class="teste"><input  class="input2" type="text" id="CMAI_Rougher_Tambor_Rejeito_Granulometria_D80_OptSim" name="CMAI_Rougher_Tambor_Rejeito_Granulometria_D80_OptSim" value="0.00" disabled> &#x3BCm/mm</td>
                </tr>
                <!-------------------------------------->
                <tr class="tr1"><!-- LINHA 87-->
                    <td> 10 </td>
                    <td> CMAI_Rougher_Tambor_Rejeito_Peneira_OS_Granulometria_D80_OptSim  </td>
                   <td class="teste"><input  class="input1" type="text" id="CMAI_Rougher_Tambor_Rejeito_Peneira_OS_Granulometria_D80_OptSim" name="CMAI_Rougher_Tambor_Rejeito_Peneira_OS_Granulometria_D80_OptSim" value="0.00" disabled> &#x3BCm/mm</td>
                </tr> 
               <!-------------------------------------->     
                <tr class="tr2"><!-- LINHA 88-->
                    <td> 10 </td>
                    <td> CMAI_Rougher_Tambor_Rejeito_Peneira_US_Granulometria_D80_OptSim</td>
                   <td class="teste"><input  class="input2" type="text" id="CMAI_Rougher_Tambor_Rejeito_Peneira_US_Granulometria_D80_OptSim" name="CMAI_Rougher_Tambor_Rejeito_Peneira_US_Granulometria_D80_OptSim" value="0.00" disabled> &#x3BCm/mm</td>
                </tr> 
            </table>

        </div>
    </body>
</html> 