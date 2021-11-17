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
        
        <link rel="stylesheet" href="./bodySeparacaoMagneticaAltaFrequencia.css">
        <link rel="stylesheet" href="./CaixaSeparacaoMagneticaAltaFrequencia.css">
        <link rel="stylesheet" href="./labelSeparacaoMagneticaAltaFrequencia.css">
        <link rel="stylesheet" href="./menuSeparacaoMagneticaAltaFrequencia.css">
        
        
    </head>

    <body>
        <div>
            <div> <img id="bodySeparacaoMagneticaAltaFrequencia"    src="../../ImagensHMI/SeparacaoMagneticaAltaFrequencia.jpeg">   </div> <!--HMI--> 
            <div> <img id="LogoCemi"                                src="../../ImagensHMI/Cemi.jpeg">                               </div> <!--LOGO CEMI--> 
            <div> <img id="Vale"                                    src="../../ImagensHMI/Vale1.jpg">                               </div> <!--LOGO VALE-->         

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
                            <li>    <a href="./SeparacaoMagneticaAltaFrequencia.php">                                   Separação Magnética Alta Frequência </a></li>
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
            <label id="ControleAvancadoDeProcessos" type="text" name="ControleAvancadoDeProcessos"> Interface Homem Máquina - HMI  </label>
            <label id="EspessadorSupervisaoGeral"   type="text" name="EspessadorSupervisaoGeral">   Separação Magnética Alta Frequêcia - Supervisão Geral   </label>
            
            <!--SUBTITULO-->
            <label id="Rejeito" type="text" name="Rejeito"> Rejeito  </label>
            <label id="Moagem"  type="text" name="Moagem">  Moagem   </label>

            <!--LABEL-->
            <label id="PNProtecao"             type="text" name="PNProtecao">             PN Proteção               </label>
            <label id="RejeitoGrosso"          type="text" name="RejeitoGrosso">          Rejeito Grosso            </label>

            <!--NOME DOS ESQUIPAMENTOS-->
            <div id="SeparacaoMagneticaAltaFrequenciaCaixa01"  type="text" name="SeparacaoMagneticaAltaFrequenciaCaixa01"> 2037-CI-001/002  </div>          
            <div id="SeparacaoMagneticaAltaFrequenciaCaixa02"  type="text" name="SeparacaoMagneticaAltaFrequenciaCaixa02"> 2037-SM-010      </div>
            <div id="SeparacaoMagneticaAltaFrequenciaCaixa03"  type="text" name="SeparacaoMagneticaAltaFrequenciaCaixa03"> 2037-CI-03       </div>
            <div id="SeparacaoMagneticaAltaFrequenciaCaixa04"  type="text" name="SeparacaoMagneticaAltaFrequenciaCaixa04"> 2037-CX-00       </div>
            <div id="SeparacaoMagneticaAltaFrequenciaCaixa05"  type="text" name="SeparacaoMagneticaAltaFrequenciaCaixa05"> 2037-SM-09       </div>
            <div id="SeparacaoMagneticaAltaFrequenciaCaixa06"  type="text" name="SeparacaoMagneticaAltaFrequenciaCaixa06"> 2037-CI-05       </div>
            <div id="SeparacaoMagneticaAltaFrequenciaCaixa07"  type="text" name="SeparacaoMagneticaAltaFrequenciaCaixa07"> PN-06/08         </div>
             
        </div>
    </body>
</html> 