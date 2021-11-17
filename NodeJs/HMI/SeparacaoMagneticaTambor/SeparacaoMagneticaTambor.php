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
        <link href='https://fonts.googleapis.com/css?family=RobotoDraft' rel='stylesheet' type='text/css'>
        <link rel="stylesheet" href="https://www.w3schools.com/w3css/4/w3.css">
        <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/4.7.0/css/font-awesome.min.css">
        <!-- <script src= "https://github.com/google/gson.git"></script>-->
        <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.5.1/jquery.min.js"></script>
    
        <link rel="stylesheet" href="./teste.css">
        <script src="./values.js"></script>
        
        <link rel="stylesheet" href="./bodySeparacaoMagneticaTambor.css">
        <link rel="stylesheet" href="./caixaSeparacaoMagneticaTambor.css">
        <link rel="stylesheet" href="./labelSeparacaoMagneticaTambor.css">
        <link rel="stylesheet" href="./menuSeparacaoMagneticaTambor.css">
        
        
    </head>

	<body>
        <div>
            <div> <img id="bodySeparacaoMagneticaTambor"    src="../../ImagensHMI/SeparacaoMagneticaTambor.jpeg">   </div> <!--HMI--> 
            <div> <img id="LogoCemi"                        src="../../ImagensHMI/Cemi.jpeg">                       </div> <!--LOGO CEMI--> 
            <div> <img id="Vale"                            src="../../ImagensHMI/Vale2.jpg">                       </div> <!--LOGO VALE-->                                                                                      
                 
            <nav class="dropdown">
                <a class="menu">MENU</a>
                <div class="dropdown-content">
                    <li>
                        <a> HMI </a>
                        <ul>
                            <li>    <a href="../Britagem/Britagem.php">                                                 Britagem                            </a></li>
                            <li>    <a href="../Espessador/Espessador.php">                                             Espessador                          </a></li>
                            <li>    <a href="../Flotacao/Flotacao.php">                                                 Flotação                            </a></li>
                            <li>    <a href="../MoagemLinha01/MoagemLinha01.php">                                       Moagem - Linha 01                   </a></li>
                            <li>    <a href="../MoagemLinha02/MoagemLinha02.php">                                       Moagem - Linha 02                   </a></li>
                            <li>    <a href="../SeparacaoMagneticaAltaFrequencia/SeparacaoMagneticaAltaFrequencia.php"> Separação Magnética Alta Frequência </a></li>
                            <li>    <a href="./SeparacaoMagneticaTambor.php">                                           Separação Magnética Tambor          </a></li>
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
            <label id="ControleAvancadoDeProcessos" type="text" name="ControleAvancadoDeProcessos"> Interface Homem Máquina - HMI                   </label>
            <label id="EspessadorSupervisaoGeral"   type="text" name="EspessadorSupervisaoGeral">   Separação Magnética Tambor - Supervisão Geral   </label>
            
            <!--SUBTITULO-->
            <label id="Moagem1"  type="text" name="Moagem">  Moagem  </label>
            <label id="Moagem2"  type="text" name="Moagem">  Moagem  </label>
            <label id="WHIMS"    type="text" name="WHIMS">   WHIMS   </label>
            <label id="Rejeito"  type="text" name="Rejeito"> Rejeito </label>

            <!--LABEL-->
           <label id="PeneiramentoPrimario"       type="text" name="PeneiramentoPrimario">  Peneiramento Primário           </label>
           <label id="Pcvi"                       type="text" name="Pcvi">                  PCVI                            </label>

            <!--NOME DOS ESQUIPAMENTOS-->
            <div id="SeparacaoMagneticaTamborCaixa01"  type="text" name="SeparacaoMagneticaTamborCaixa01"> 2037-DI-001/002  </div>          
            <div id="SeparacaoMagneticaTamborCaixa02"  type="text" name="SeparacaoMagneticaTamborCaixa02"> 2037-SM-001a008  </div>
            <div id="SeparacaoMagneticaTamborCaixa03"  type="text" name="SeparacaoMagneticaTamborCaixa03"> 2037-PN-102      </div>
            <div id="SeparacaoMagneticaTamborCaixa04"  type="text" name="SeparacaoMagneticaTamborCaixa04"> 2037-PN-01/02/03 </div>
            <div id="SeparacaoMagneticaTamborCaixa05"  type="text" name="SeparacaoMagneticaTamborCaixa05"> 2037-SM-13a16    </div>
            <div id="SeparacaoMagneticaTamborCaixa06"  type="text" name="SeparacaoMagneticaTamborCaixa06"> 2037-CI-004      </div>
            <div id="SeparacaoMagneticaTamborCaixa07"  type="text" name="SeparacaoMagneticaTamborCaixa07"> 3037-PN-05109    </div>

        </div>
    </body>
</html> 