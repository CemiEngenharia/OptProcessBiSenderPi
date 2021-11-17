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

        <link rel="stylesheet" href="./bodySimulacaoDinamicaFlotacao.css">
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
                            <li>    <a href="./SimulacaoDinamicaFlotacao.php">                                     Flotação           </a></li>
                            <li>    <a href="../SimulacaoDinamicaMoagem/SimulacaoDinamicaMoagem.php">              Moagem             </a></li>
                            <li>    <a href="../SimulacaoDinamicaRom/SimulacaoDinamicaRom.php">                    ROM                </a></li>
                        </ul>
                    </li>   
                    <a href="../../SimulacaoIndicadores/SimulacaoIndicadores.php"> Simulação Indicadores   </a>
                </div>
            </nav>

            <!--TITULO--> 
            <label id="SimulacaoDeIndicadores"  type="text" name="SimulacaoDeIndicadores"> Simulação  Dinâmica         </label>
            <label id="SalaDeControle"          type="text" name="SalaDeControle">         Flotação - Página 2         </label>

            <!--LABEL-->
            <label id="Pagina"         type="text" name="Pagina">        Páginas     </label>

            <!--LINK--> 
            <a id="LinkPagina1"     href="../SimulacaoDinamicaFlotacao/SimulacaoDinamicaFlotacao.php"> 1   </a>
            <a id="LinkPagina2"     href="./SimulacaoDinamicaFlotacao2.php">                           2   </a>

            <!-------------------------------------------------------------------------------------------------------------------------------------------->
            <table id="table2">
                <th rowspan="23"> FLOTAÇÃO </th><!-- TÍTULO COLUNA-->

                <tr class="tr1"><!-- LINHA 45-->
                    <td> 110 </td>
                    <td> Flotacao_Scavenger_Concentrado_Teor_Outros_OptSim  </td>
                    <td class="teste"><input class="input1" type="text" id="Flotacao_Scavenger_Concentrado_Teor_Outros_OptSim" name="Flotacao_Scavenger_Concentrado_Teor_Outros_OptSim" value="0.00" disabled> %</td>
                </tr> 
                <!--------------------------------------------->
                <tr class="tr2"><!-- LINHA 46-->
                    <td> 110 </td>
                    <td> Flotacao_Scavenger_Concentrado_Teor_SiO2_OptSim</td>
                    <td class="teste"><input class="input2" type="text" id="Flotacao_Scavenger_Concentrado_Teor_SiO2_OptSim" name="Flotacao_Scavenger_Concentrado_Teor_SiO2_OptSim" value="0.00" disabled> %</td>
                </tr>
                <!--------------------------------------------->
                <tr class="tr1"><!-- LINHA 47-->
                    <td> 110 </td>
                    <td> Flotacao_Scavenger_Concentrado_Vazao_OptSim  </td>
                    <td class="teste"><input class="input1" type="text" id="Flotacao_Scavenger_Concentrado_Vazao_OptSim" name="Flotacao_Scavenger_Concentrado_Vazao_OptSim" value="0.00" disabled> m&#x00B3/h</td>
                </tr>            
                <!--------------------------------------------->
                <tr class="tr2"><!-- LINHA 48-->
                    <td> 110 </td>
                    <td> Flotacao_Scavenger_Rejeito_PercSol_OptSim</td>
                    <td class="teste"><input class="input2" type="text" id="Flotacao_Scavenger_Rejeito_PercSol_OptSim" name="Flotacao_Scavenger_Rejeito_PercSol_OptSim" value="0.00" disabled> %</td>
                </tr>
                <!--------------------------------------------->
                <tr class="tr1"><!-- LINHA 49-->
                    <td> 110 </td>
                    <td> Flotacao_Scavenger_Rejeito_Taxa_BaseSeca_OptSim  </td>
                    <td class="teste"><input class="input1" type="text" id="Flotacao_Scavenger_Rejeito_Taxa_BaseSeca_OptSim" name="Flotacao_Scavenger_Rejeito_Taxa_BaseSeca_OptSim" value="0.00" disabled> t/h</td>
                </tr>
                <!--------------------------------------------->
                <tr class="tr2"><!-- LINHA 50-->
                    <td> 110 </td>
                    <td> Flotacao_Scavenger_Rejeito_Taxa_BaseUmida_OptSim</td>
                    <td class="teste"><input class="input2" type="text" id="Flotacao_Scavenger_Rejeito_Taxa_BaseUmida_OptSim" name="Flotacao_Scavenger_Rejeito_Taxa_BaseUmida_OptSim" value="0.00" disabled> t/h</td>
                </tr>     
                <!--------------------------------------------->
                <tr class="tr1"><!-- LINHA 51-->
                    <td> 110 </td>
                    <td> Flotacao_Scavenger_Rejeito_Teor_Fe_OptSim  </td>
                    <td class="teste"><input class="input1" type="text" id="Flotacao_Scavenger_Rejeito_Teor_Fe_OptSim" name="Flotacao_Scavenger_Rejeito_Teor_Fe_OptSim" value="0.00" disabled> %</td>
                </tr> 
                <!--------------------------------------------->
                <tr class="tr2"><!-- LINHA 52-->
                    <td> 110 </td>
                    <td> Flotacao_Scavenger_Rejeito_Teor_Outros_OptSim</td>
                    <td class="teste"><input class="input2" type="text" id="Flotacao_Scavenger_Rejeito_Teor_Outros_OptSim" name="Flotacao_Scavenger_Rejeito_Teor_Outros_OptSim" value="0.00" disabled> %</td>
                </tr>
                <!--------------------------------------------->
                <tr class="tr1"><!-- LINHA 53-->
                    <td> 110 </td>
                    <td> Flotacao_Scavenger_Rejeito_Teor_SiO2_OptSim</td>
                    <td class="teste"><input class="input1" type="text" id="Flotacao_Scavenger_Rejeito_Teor_SiO2_OptSim" name="Flotacao_Scavenger_Rejeito_Teor_SiO2_OptSim" value="0.00" disabled> %</td>
                </tr>
                <!--------------------------------------------->
                <tr class="tr2"><!-- LINHA 54-->
                    <td> 110 </td>
                    <td> Flotacao_Scavenger_Rejeito_Vazao_OptSim  </td>
                    <td class="teste"><input class="input2" type="text" id="Flotacao_Scavenger_Rejeito_Vazao_OptSim" name="Flotacao_Scavenger_Rejeito_Vazao_OptSim" value="0.00" disabled> m&#x00B3/h</td>
                </tr>
            </table>
             
        </div>
    </body>
</html> 