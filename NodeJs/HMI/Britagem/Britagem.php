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
	<title> Simulador Dinâmico </title>
	<meta charset="UTF-8">
   
    <meta name="viewport" content="target-densitydpi=device-dpi, initial-scale=1.0, user-scalable=no" />
   
    <head>
        <link href='https://fonts.googleapis.com/css?family=RobotoDraft' rel='stylesheet' type='text/css'>
        <link rel="stylesheet" href="https://www.w3schools.com/w3css/4/w3.css">
        <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/4.7.0/css/font-awesome.min.css">
        <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.5.1/jquery.min.js"></script>
      
        <link rel="stylesheet" href="./bodyBritagem.css">
        <link rel="stylesheet" href="./caixaBritagem.css">
        <link rel="stylesheet" href="./labelBritagem.css">
        <link rel="stylesheet" href="./menuBritagem.css">
    </head>

	<body> 
        <div>          
            <div> <img id="bodyBritagem"    src="../../ImagensHMI/Britagem.jpeg">  </div> <!--HMI--> 
            <div> <img id="LogoCemi"        src="../../ImagensHMI/Cemi.jpeg">      </div> <!--LOGO CEMI--> 
            <div> <img id="Vale"            src="../../ImagensHMI/Vale1.jpg">      </div> <!--LOGO VALE-->                                                                                                                                             
                 
            <nav class="dropdown">
                <a class="menu">MENU</a>
                <div class="dropdown-content">
                    <li>
                        <a> HMI </a>
                        <ul>
                            <li>    <a href="./Britagem.php">                                                           Britagem                            </a></li>
                            <li>    <a href="../Espessador/Espessador.php">                                             Espessador                          </a></li>
                            <li>    <a href="../Flotacao/Flotacao.php">                                                 Flotação                            </a></li>
                            <li>    <a href="../MoagemLinha01/MoagemLinha01.php">                                       Moagem - Linha 01                   </a></li>
                            <li>    <a href="../MoagemLinha02/MoagemLinha02.php">                                       Moagem - Linha 02                   </a></li>
                            <li>    <a href="../SeparacaoMagneticaAltaFrequencia/SeparacaoMagneticaAltaFrequencia.php"> Separação Magnética Alta Frequência </a></li>
                            <li>    <a href="../SeparacaoMagneticaTambor/SeparacaoMagneticaTambor.php">                 Separação Magnética Tambor          </a></li>
                        </ul>
                    </li>   
                    <a href="../../global.php">                                     Global                  </a>
                    <li>
                        <a> Gráfico </a>
                        <ul>
                            <li>    <a href="../../Graficos/GraficoCmai/GraficoCmai.php">                   CMAI            </a></li>  
                            <li>    <a href="../../Graficos/GraficoEspessamento/GraficoEspessamento.php">   Espessamento    </a></li>  
                            <li>    <a href="../../Graficos/GraficoFlotacao/GraficoFlotacao.php">           Flotação        </a></li>
                            <li>    <a href="../../Graficos/GraficoMoinho1/GraficoMoinho1.php">             Moinho 01       </a></li>
                            <li>    <a href="../../Graficos/GraficoMoinho2/GraficoMoinho2.php">             Moinho 02       </a></li>
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
            <label id="ControleAvancadoDeProcessos" type="text" name="ControleAvancadoDeProcessos"> Interface Homem Máquina - HMI   </label>
            <label id="EspessadorSupervisaoGeral"   type="text" name="EspessadorSupervisaoGeral">   Britagem - Supervisão Geral     </label>

            <!--SUBTITULO-->
            <label id="WDRE"    type="text" name="WDRE">    WDRE     </label>
            <label id="Moagem"  type="text" name="Moagem">  Moagem   </label>

            <!--NOME DOS ESQUIPAMENTOS-->
            <div id="BritagemCaixa01"  type="text" name="BritagemCaixa01"> 2015-PN-01102    </div>          
            <div id="BritagemCaixa02"  type="text" name="BritagemCaixa02"> 2025-TQ-001      </div>
            <div id="BritagemCaixa03"  type="text" name="BritagemCaixa03"> 2025-PN-001      </div>
            <div id="BritagemCaixa04"  type="text" name="BritagemCaixa04"> 2025-BR-004      </div>
            <div id="BritagemCaixa05"  type="text" name="BritagemCaixa05"> 2025-CT-011      </div>
             
        </div>
    </body>
</html> 