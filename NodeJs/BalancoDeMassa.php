<?php

    if(isset($_COOKIE["session"]) == true){
        if(strlen($_COOKIE["session"]) !=  32){
            header("Location:  ../index.php");
        }
    }	
    else{
        header("Location: ../index.php");
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

        <script src="../values.js"> </script>
        
        <link rel="stylesheet" href="./bodyBalancoDeMassa.css">
        <link rel="stylesheet" href="./labelBalancoDeMassa.css">
        <link rel="stylesheet" href="./menuBalancoDeMassa.css">
        <link rel="stylesheet" href="./table1.css">
        <link rel="stylesheet" href="./table2.css">
        <link rel="stylesheet" href="./table3.css">
        <link rel="stylesheet" href="./table4.css">  
        <link rel="stylesheet" href="./table5.css">
        <link rel="stylesheet" href="./table6.css">
        <link rel="stylesheet" href="./table7.css">

    </head>

	<body>
        <!--IMAGENS-->         
        <div> <img id="LogoCemi"    src="../ImagensHMI/Cemi.png">   </div> <!--LOGO CEMI--> 
        <div> <img id="Vale"        src="../ImagensHMI/Vale1.jpg">  </div> <!--LOGO VALE-->                                                                                                                                             
             
        <nav class="dropdown">  <!--MENU-->     
            <a class="menu">MENU</a>
            <div class="dropdown-content">

                <a href="./BalancoDeMassa.php">   Balanço de Massas   </a> 
                <a href="../global.php">          Global              </a>
                <li>
                    <a> Gráfico </a>
                    <ul>
                        <li>    <a href="../Graficos/GraficoCmai/GraficoCmai.php">                  CMAI            </a></li>  
                        <li>    <a href="../Graficos/GraficoEspessamento/GraficoEspessamento.php">  Espessamento    </a></li>  
                        <li>    <a href="../Graficos/GraficoFlotacao/GraficoFlotacao.php">          Flotação        </a></li>
                        <li>    <a href="../Graficos/GraficoMoinho1/GraficoMoinho1.php">            Moinho 01       </a></li>
                        <li>    <a href="../Graficos/GraficoMoinho2/GraficoMoinho2.php">            Moinho 02       </a></li>
                    </ul>
                </li> 
                <li>
                    <a> HMI </a>
                    <ul>
                        <li>    <a href="../HMI/Britagem/Britagem.php">                                                 Britagem                            </a></li>
                        <li>    <a href="../HMI/Deslamagem/Deslamagem.php">                                             Deslamagem                          </a></li>
                        <li>    <a href="../HMI/Espessador/Espessador.php">                                             Espessador  Rejeito                </a></li>
                        <li>    <a href="../HMI/Flotacao/Flotacao.php">                                                 Flotação                            </a></li>
                        <li>    <a href="../HMI/MoagemLinha01/MoagemLinha01.php">                                       Moagem - Linha 01                   </a></li>
                        <li>    <a href="../HMI/MoagemLinha02/MoagemLinha02.php">                                       Moagem - Linha 02                   </a></li>
                        <li>    <a href="../HMI/SeparacaoMagneticaAltaFrequencia/SeparacaoMagneticaAltaFrequencia.php"> Separação Magnética Alta Frequência </a></li>
                        <li>    <a href="../HMI/SeparacaoMagneticaTambor/SeparacaoMagneticaTambor.php">                 Separação Magnética Tambor          </a></li>
                    </ul>
                </li>              
                <li>
                    <a> Simulação Dinâmica </a>
                    <ul>
                        <li>    <a href="../SimulacaoDinamica/SimulacaoDinamicaBritagem/SimulacaoDinamicaBritagem.php">                 Britagem           </a></li>
                        <li>    <a href="../SimulacaoDinamica/SimulacaoDinamicaCmai/SimulacaoDinamicaCmai1/SimulacaoDinamicaCmai1.php"> CMAI               </a></li>
                        <li>    <a href="../SimulacaoDinamica/SimulacaoDinamicaDeslamagem/SimulacaoDinamicaDeslamagem.php">             Deslamagem         </a></li>
                        <li>    <a href="../SimulacaoDinamica/SimulacaoDinamicaEspessamento/SimulacaoDinamicaEspessamento.php">         Espessamento       </a></li>
                        <li>    <a href="../SimulacaoDinamica/SimulacaoDinamicaFlotacao/SimulacaoDinamicaFlotacao.php">                 Flotação           </a></li>
                        <li>    <a href="../SimulacaoDinamica/SimulacaoDinamicaMoagem/SimulacaoDinamicaMoagem.php">                     Moagem             </a></li>
                        <li>    <a href="../SimulacaoDinamica/SimulacaoDinamicaRom/SimulacaoDinamicaRom.php">                           ROM                </a></li>
                    </ul>
                </li>   
                <a href="../SimulacaoIndicadores/SimulacaoIndicadores1/SimulacaoIndicadores.php"> Simulação Indicadores   </a>
            </div>
        </nav>
            
        <!--TITULO-->           
        <label id="SimulacaoDeIndicadores"  type="text" name="SimulacaoDeIndicadores"> Balanço de Massas  </label>
        <!-------------------------------------------------------------------------------------------------------------------------------------------------->
        <table id="table2">
            <th rowspan="7"> ALIMENTAÇÃO</th><!-- TÍTULO COLUNA-->
        
            <tr class="tr1"><!-- LINHA 1-->
                <td> ROM </td>
                <td class="teste"> <input class="input1" type="text" id="SI_BP_DF" name="SI_BP_DF" value="0.00" disabled> %</td>
            </tr> 
            
            <tr class="tr2"><!-- LINHA 2-->
                <td> Fe </td>
                <td class="teste"> <input class="input2" type="text" id="SI_BP_UF" name="SI_BP_UF" value="0.00" disabled> %</td>
            </tr> 

            <tr class="tr1"><!-- LINHA 3 -->
                <td> SiO2 </td>
                <td class="teste"> <input class="input1" type="text" id="SI_BP_RO" name="SI_BP_RO" value="0.00" disabled> %</td>
            </tr> 
            
            <tr class="tr2"><!-- LINHA 4 -->
                <td> Massa de Fe</td>
                <td class="teste"> <input class="input2" type="text" id="BritagemTerciaria_Peneira_OS_Taxa_BaseSeca_OptSim" name="BritagemTerciaria_Peneira_OS_Taxa_BaseSeca_OptSim" value="0.00" disabled> t/h</td>
            </tr> 

            <tr class="tr1"><!-- LINHA 3 -->
                <td> Massa de SiO2 </td>
                <td class="teste"> <input class="input1" type="text" id="SI_BP_RO" name="SI_BP_RO" value="0.00" disabled> %</td>
            </tr> 

            <tr class="tr2"><!-- LINHA 4 -->
                <td> Totalizador de Massa </td>
                <td class="teste"> <input class="input2" type="text" id="BritagemTerciaria_Peneira_OS_Taxa_BaseSeca_OptSim" name="BritagemTerciaria_Peneira_OS_Taxa_BaseSeca_OptSim" value="0.00" disabled> t/h</td>
            </tr> 
        </table>
        <!-------------------------------------------------------------------------------------------------------------------------------------------------->
        <table id="table4">
            <th rowspan="6">  PRODUÇÃO PCVI </th><!-- TÍTULO COLUNA-->            
            
            <tr class="tr1"><!-- LINHA 2-->
                <td> Fe </td>
                <td class="teste"> <input class="input1" type="text" id="SI_BP_UF" name="SI_BP_UF" value="0.00" disabled> %</td>
            </tr> 

            <tr class="tr2"><!-- LINHA 3 -->
                <td> SiO2 </td>
                <td class="teste"> <input class="input2" type="text" id="SI_BP_RO" name="SI_BP_RO" value="0.00" disabled> %</td>
            </tr> 

            <tr class="tr1"><!-- LINHA 4 -->
                <td> Massa de Fe</td>
                <td class="teste"> <input class="input1" type="text" id="BritagemTerciaria_Peneira_OS_Taxa_BaseSeca_OptSim" name="BritagemTerciaria_Peneira_OS_Taxa_BaseSeca_OptSim" value="0.00" disabled> t/h</td>
            </tr> 

            <tr class="tr2"><!-- LINHA 3 -->
                <td> Massa de SiO2 </td>
                <td class="teste"> <input class="input2" type="text" id="SI_BP_RO" name="SI_BP_RO" value="0.00" disabled> %</td>
            </tr> 
            
            <tr class="tr1"><!-- LINHA 4 -->
                <td> Totalizador de Massa </td>
                <td class="teste"> <input class="input1" type="text" id="BritagemTerciaria_Peneira_OS_Taxa_BaseSeca_OptSim" name="BritagemTerciaria_Peneira_OS_Taxa_BaseSeca_OptSim" value="0.00" disabled> t/h</td>
            </tr> 
        </table>
        <!-------------------------------------------------------------------------------------------------------------------------------------------------->
        <table id="table3">
            <th rowspan="6">  PRODUÇÃO PFVI</th><!-- TÍTULO COLUNA-->            
            
            <<tr class="tr1"><!-- LINHA 2-->
                <td> Fe </td>
                <td class="teste"> <input class="input1" type="text" id="SI_BP_UF" name="SI_BP_UF" value="0.00" disabled> %</td>
            </tr> 

            <tr class="tr2"><!-- LINHA 3 -->
                <td> SiO2 </td>
                <td class="teste"> <input class="input2" type="text" id="SI_BP_RO" name="SI_BP_RO" value="0.00" disabled> %</td>
            </tr> 

            <tr class="tr1"><!-- LINHA 4 -->
                <td> Massa de Fe</td>
                <td class="teste"> <input class="input1" type="text" id="BritagemTerciaria_Peneira_OS_Taxa_BaseSeca_OptSim" name="BritagemTerciaria_Peneira_OS_Taxa_BaseSeca_OptSim" value="0.00" disabled> t/h</td>
            </tr> 

            <tr class="tr2"><!-- LINHA 3 -->
                <td> Massa de SiO2 </td>
                <td class="teste"> <input class="input2" type="text" id="SI_BP_RO" name="SI_BP_RO" value="0.00" disabled> %</td>
            </tr> 
            
            <tr class="tr1"><!-- LINHA 4 -->
                <td> Totalizador de Massa </td>
                <td class="teste"> <input class="input1" type="text" id="BritagemTerciaria_Peneira_OS_Taxa_BaseSeca_OptSim" name="BritagemTerciaria_Peneira_OS_Taxa_BaseSeca_OptSim" value="0.00" disabled> t/h</td>
            </tr> 
        </table>
        <!-------------------------------------------------------------------------------------------------------------------------------------------------->
        <table id="table1">
            <th rowspan="5"> REJEITO FLOTAÇÃO </th><!-- TÍTULO COLUNA-->

            <tr class="tr1"><!-- LINHA 2-->
                <td> Fe </td>
                <td class="teste"> <input class="input1" type="text" id="SI_BP_UF" name="SI_BP_UF" value="0.00" disabled> %</td>
            </tr> 

            <tr class="tr2"><!-- LINHA 3 -->
                <td> SiO2 </td>
                <td class="teste"> <input class="input2" type="text" id="SI_BP_RO" name="SI_BP_RO" value="0.00" disabled> %</td>
            </tr> 

            <tr class="tr1"><!-- LINHA 4 -->
                <td> Massa de Fe</td>
                <td class="teste"> <input class="input1" type="text" id="BritagemTerciaria_Peneira_OS_Taxa_BaseSeca_OptSim" name="BritagemTerciaria_Peneira_OS_Taxa_BaseSeca_OptSim" value="0.00" disabled> t/h</td>
            </tr> 

            <tr class="tr2"><!-- LINHA 3 -->
                <td> Massa de SiO2 </td>
                <td class="teste"> <input class="input2" type="text" id="SI_BP_RO" name="SI_BP_RO" value="0.00" disabled> %</td>
            </tr> 
        </table>
        <!-------------------------------------------------------------------------------------------------------------------------------------------------->
        <table id="table6">
            <th rowspan="5"> REJEITO FINAL</th><!-- TÍTULO COLUNA-->

            <tr class="tr1"><!-- LINHA 2-->
                <td> Fe </td>
                <td class="teste"> <input class="input1" type="text" id="SI_BP_UF" name="SI_BP_UF" value="0.00" disabled> %</td>
            </tr> 

            <tr class="tr2"><!-- LINHA 3 -->
                <td> SiO2 </td>
                <td class="teste"> <input class="input2" type="text" id="SI_BP_RO" name="SI_BP_RO" value="0.00" disabled> %</td>
            </tr> 

            <tr class="tr1"><!-- LINHA 4 -->
                <td> Massa de Fe</td>
                <td class="teste"> <input class="input1" type="text" id="BritagemTerciaria_Peneira_OS_Taxa_BaseSeca_OptSim" name="BritagemTerciaria_Peneira_OS_Taxa_BaseSeca_OptSim" value="0.00" disabled> t/h</td>
            </tr> 

            <tr class="tr2"><!-- LINHA 3 -->
                <td> Massa de SiO2 </td>
                <td class="teste"> <input class="input2" type="text" id="SI_BP_RO" name="SI_BP_RO" value="0.00" disabled> %</td>
            </tr> 
        </table>
        <!-------------------------------------------------------------------------------------------------------------------------------------------------->
        <table id="table7">
            <th rowspan="5"> REJEITO GROSSO</th><!-- TÍTULO COLUNA-->

            <tr class="tr1"><!-- LINHA 2-->
                <td> Fe </td>
                <td class="teste"> <input class="input1" type="text" id="SI_BP_UF" name="SI_BP_UF" value="0.00" disabled> %</td>
            </tr> 

            <tr class="tr2"><!-- LINHA 3 -->
                <td> SiO2 </td>
                <td class="teste"> <input class="input2" type="text" id="SI_BP_RO" name="SI_BP_RO" value="0.00" disabled> %</td>
            </tr> 

            <tr class="tr1"><!-- LINHA 4 -->
                <td> Massa de Fe</td>
                <td class="teste"> <input class="input1" type="text" id="BritagemTerciaria_Peneira_OS_Taxa_BaseSeca_OptSim" name="BritagemTerciaria_Peneira_OS_Taxa_BaseSeca_OptSim" value="0.00" disabled> t/h</td>
            </tr> 

            <tr class="tr2"><!-- LINHA 3 -->
                <td> Massa de SiO2 </td>
                <td class="teste"> <input class="input2" type="text" id="SI_BP_RO" name="SI_BP_RO" value="0.00" disabled> %</td>
            </tr> 
        </table>
        <!-------------------------------------------------------------------------------------------------------------------------------------------------->
            <table id="table5">
            <th rowspan="6">  RECUPERAÇÕES </th><!-- TÍTULO COLUNA-->            
            
            <tr class="tr1"><!-- LINHA 1-->
                <td> Rec. Mássíca Global (b.s.)</td>
                <td class="teste"> <input class="input1" type="text" id="SI_BP_DF" name="SI_BP_DF" value="0.00" disabled> %</td>
            </tr> 
            
            <tr class="tr2"><!-- LINHA 2-->         
                <td>Rec. Mássíca Global (b.u.) </td>
                <td class="teste"><input class="input2" type="text" id="SI_BP_UF" name="SI_BP_UF" value="0.00" disabled> %</td>
            </tr> 

            <tr class="tr1"><!-- LINHA 3 -->
                <td> Rec. Metalúrgica Global </td>
                <td class="teste"> <input class="input1" type="text" id="SI_BP_RO" name="SI_BP_RO" value="0.00" disabled>  %</td>
            </tr> 
            
            <tr class="tr2"><!-- LINHA 4 -->
                <td>  Rec. Metalúrgica da Flotação Global </td>
                <td class="teste"> <input class="input2" type="text" id="SI_BP_PRODUTIVIDADE" name="SI_BP_PRODUTIVIDADE" value="0.00" disabled>  %</td>
            </tr> 
        </table>
        <!-------------------------------------------------------------------------------------------------------------------------------------------------->
    </body>
</html> 