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
        <!-- <script src= "https://github.com/google/gson.git"></script>-->
        <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.5.1/jquery.min.js"></script>
        
        <link rel="stylesheet" href="./bodyEspessador.css">
        <link rel="stylesheet" href="./caixaEspessador.css">
        <link rel="stylesheet" href="./menuEspessador.css">
        <link rel="stylesheet" href="./labelEspessador.css">
    </head>

	<body>
        <div>
            <div> <img id="bodyEspessador"  src="../../ImagensHMI/Espessador.png"> </div> <!--HMI--> 
            <div> <img id="LogoCemi"        src="../../ImagensHMI/Cemi.jpeg">      </div> <!--LOGO CEMI--> 
            <div> <img id="Vale"            src="../../ImagensHMI/Vale1.jpg">      </div> <!--LOGO VALE-->          
                                                                                                                                                            
            <nav class="dropdown">
                <a class="menu">MENU</a>
                <div class="dropdown-content">
                    <li>
                        <a> HMI </a>
                        <ul>
                            <li>    <a href="../Britagem/Britagem.php">                                                 Britagem                            </a></li>
                            <li>    <a href="./Espessador.php">                                                         Espessador                          </a></li>
                            <li>    <a href="../Flotacao/Flotacao.php">                                                 Flotação                            </a></li>
                            <li>    <a href="../MoagemLinha01/MoagemLinha01.php">                                       Moagem - Linha 01                   </a></li>
                            <li>    <a href="../MoagemLinha02/MoagemLinha02.php">                                       Moagem - Linha 02                   </a></li>
                            <li>    <a href="../SeparacaoMagneticaAltaFrequencia/SeparacaoMagneticaAltaFrequencia.php"> Separação Magnética Alta Frequência </a></li>
                            <li>    <a href="../SeparacaoMagneticaTambor/SeparacaoMagneticaTambor.php">                 Separação Magnética Tambor          </a></li>
                        </ul>
                    </li>   
                    <a href="../../global.php">     Global  </a>
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
            <label id="ControleAvancadoDeProcessos" type="text" name="ControleAvancadoDeProcessos"> Controle Avançado de Processos  </label>
            <label id="EspessadorSupervisaoGeral"   type="text" name="EspessadorSupervisaoGeral">   Espessador - Supervisão Geral   </label>
            
            <!--SUBTITULO-->
            <label id="SistemaDiluicaoDeFloculante" type="text" name="SistemaDiluicaoDeFloculante"> Sistema Diluição de Floculante  </label>
            <label id="SistemaDiluicaoDeCoagulante" type="text" name="SistemaDiluicaoDeCoagulante"> Sistema Diluição de Coagulante  </label>

            <!--LABEL-->
            <label id="AguaDeProcesso1"             type="text" name="AguaDeProcesso1">             Água de Processo                </label>
            <label id="AguaDeProcesso2"             type="text" name="AguaDeProcesso2">             Água de Processo                </label>
            <label id="Rejeitoduto"                 type="text" name="Rejeitoduto">                 Rejeitoduto                     </label>
            <label id="Principal"                   type="text" name="Principal">                   Principal                       </label>
            <label id="Reserva"                     type="text" name="Reserva">                     Reserva                         </label>
              
            <!--NOME DOS ESQUIPAMENTOS-->
            <div id="EspessadorCaixa01"  type="text" name="EspessadorCaixa01"> 2070-TQ-001      </div>          
            <div id="EspessadorCaixa02"  type="text" name="EspessadorCaixa02"> 2070-BQ-014      </div>
            <div id="EspessadorCaixa03"  type="text" name="EspessadorCaixa03"> 2070-BQ-014R     </div>
            <div id="EspessadorCaixa04"  type="text" name="EspessadorCaixa04"> 2070-CX-004      </div>
            <div id="EspessadorCaixa05"  type="text" name="EspessadorCaixa05"> 2060-CX-001      </div>
            <div id="EspessadorCaixa06"  type="text" name="EspessadorCaixa06"> Tote Bim         </div>
            <div id="EspessadorCaixa07"  type="text" name="EspessadorCaixa07"> Tote Bim Reserva </div>
            <div id="EspessadorCaixa08"  type="text" name="EspessadorCaixa08"> 2070-BQ-013      </div>
            <div id="EspessadorCaixa09"  type="text" name="EspessadorCaixa09"> 2070-BQ-013R     </div>
            <div id="EspessadorCaixa10"  type="text" name="EspessadorCaixa10"> 2060-ES001M1     </div>
            <div id="EspessadorCaixa11"  type="text" name="EspessadorCaixa11"> 2062-BP001       </div>
            <div id="EspessadorCaixa12"  type="text" name="EspessadorCaixa12"> 2062-BP001R      </div>
            <div id="EspessadorCaixa13"  type="text" name="EspessadorCaixa13"> 4015-TQ01        </div>

        </div>
    </body>
</html> 