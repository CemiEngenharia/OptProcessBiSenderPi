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
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
   
    <head>
        <link href='https://fonts.googleapis.com/css?family=RobotoDraft' rel='stylesheet' type='text/css'>
        <link rel="stylesheet" href="https://www.w3schools.com/w3css/4/w3.css">
        <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/4.7.0/css/font-awesome.min.css">
       <!-- <script src= "https://github.com/google/gson.git"></script>-->
        <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.5.1/jquery.min.js"></script>

        <link rel="stylesheet" href="./bodyMoagemLinha02.css">
        <link rel="stylesheet" href="./caixaMoagemLinha02.css">
        <link rel="stylesheet" href="./labelMoagemLinha02.css">
        <link rel="stylesheet" href="./menuMoagemLinha02.css">
    </head>

    <body>
        <div>
            <div> <img id="bodyMoagem"      src="../../ImagensHMI/Moagem.png">     </div> <!--HMI--> 
            <div> <img id="LogoCemi"        src="../../ImagensHMI/Cemi.jpeg">      </div> <!--LOGO CEMI--> 
            <div> <img id="Vale"            src="../../ImagensHMI/Vale1.jpg">      </div> <!--LOGO VALE-->                                                                                                                              
                 
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
                            <li>    <a href="./MoagemLinha02.php">                                                      Moagem - Linha 02                   </a></li>
                            <li>    <a href="../SeparacaoMagneticaAltaFrequencia/SeparacaoMagneticaAltaFrequencia.php"> Separação Magnética Alta Frequência </a></li>
                            <li>    <a href="../SeparacaoMagneticaTambor/SeparacaoMagneticaTambor.php">                 Separação Magnética Tambor          </a></li>
                        </ul>
                    </li>   
                    <a href="../../global.php">                                     Global                  </a>
                    <li>
                        <a> Gráfico </a>
                        <ul>
                            <li>    <a href="../../Graficos/GraficoCmai/GraficoCmai.php">                    CMAI            </a></li>  
                            <li>    <a href="../../Graficos/GraficoEspessamento/GraficoEspessamento.php">    Espessamento    </a></li>  
                            <li>    <a href="../../Graficos/GraficoFlotacao/GraficoFlotacao.php">            Flotação        </a></li>
                            <li>    <a href="../../Graficos/GraficoMoinho1/GraficoMoinho1.php">              Moinho 01       </a></li>
                            <li>    <a href="../../Graficos/GraficoMoinho2/GraficoMoinho2.php">              Moinho 02       </a></li>
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
            <label id="ControleAvancadoDeProcessos"     type="text" name="ControleAvancadoDeProcessos">     Controle Avançado de Processos          </label>
            <label id="MoagemLinha01SupervisaoGeral"    type="text" name="MoagemLinha01SupervisaoGeral">    Moagem - Linha 02 - Supervisão Geral    </label>
   
            <!--SUBTITULO-->
            <label id="Lamas"       type="text" name="Lamas">     Lamas     </label>
            <label id="Flotacao"    type="text" name="Flotacao">  Flotação  </label>

            <!--NOME DOS ESQUIPAMENTOS-->
            <div id="MoagemCaixa01"  type="text" name="MoagemCaixa01"> VPT-2030-SI-001  </div>          
            <div id="MoagemCaixa02"  type="text" name="MoagemCaixa02"> VPT-2030-CI-002  </div>
            <div id="MoagemCaixa03"  type="text" name="MoagemCaixa03"> VPT-2030-DI-001  </div>
            <div id="MoagemCaixa04"  type="text" name="MoagemCaixa04"> VPT-2030-MO-002  </div>
            <div id="MoagemCaixa05"  type="text" name="MoagemCaixa05"> VPT-2030-CX-001  </div>         
            <div id="MoagemCaixa06"  type="text" name="MoagemCaixa06"> 2040-CX-003      </div>
            <div id="MoagemCaixa07"  type="text" name="MoagemCaixa07"> 2040-BP-002      </div>
                        
        </div>   
    </body>
</html> 