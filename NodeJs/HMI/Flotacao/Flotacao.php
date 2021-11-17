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
     
        <script src="./values.js">      </script>
       
        <link rel="stylesheet" href="./bodyFlotacao.css">
        <link rel="stylesheet" href="./containerFlotacao.css">
        <link rel="stylesheet" href="./labelFlotacao.css">
        <link rel="stylesheet" href="./menuFlotacao.css">
        <link rel="stylesheet" href="./caixaFlotacao.css">
    </head>


    <body>
        <div>
            <div> <img id="bodyFlotacao"    src="../../ImagensHMI/Flotacao.png"> </div> <!--HMI--> 
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
                            <li>    <a href="./Flotacao.php">                                                           Flotação                            </a></li>
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
            <label id="ControleAvancadoDeProcessos" type="text" name="ControleAvancadoDeProcessos"> Controle Avançado de Processos  </label>
            <label id="FlotacaoSupervisaoGeral"     type="text" name="FlotacaoSupervisaoGeral">     Flotação - Supervisão Geral     </label>

            <!--SUBTITULO-->
            <label id="Rougher"     type="text" name="Rougher">     ROUGHER     </label>
            <label id="Cleaner"     type="text" name="Cleaner">     CLEANER     </label>
            <label id="Scavenger"   type="text" name="Scavenger">   SCAVENGER   </label>
          
            <!--LABEL-->
            <label id="AlimentacaoFlotacao"         type="text" name="Alimentacao Flotacao">        Alimentação Flotação              </label>
            <label id="ConcentracaoFlotacao"        type="text" name="ConcentracaoFlotacao">        Concentração Flotação             </label>
            <label id="Rejeita"                    type="text" name="Rejeita">                      Rejeito                           </label>

            <!--NOME DOS ESQUIPAMENTOS-->
            <div id="FlotacaoCaixa01"  type="text" name="FlotacaoCaixa01"> 2040-CR-001      </div>          
            <div id="FlotacaoCaixa02"  type="text" name="FlotacaoCaixa02"> 2040-CX-004      </div>
            <div id="FlotacaoCaixa03"  type="text" name="FlotacaoCaixa03"> 2040-CF-001      </div>
            <div id="FlotacaoCaixa04"  type="text" name="FlotacaoCaixa04"> 2040-CF-002      </div>
            <div id="FlotacaoCaixa05"  type="text" name="FlotacaoCaixa05"> 2040-CF-003      </div>
            <div id="FlotacaoCaixa06"  type="text" name="FlotacaoCaixa06"> 2040-CF-004      </div>          
            <div id="FlotacaoCaixa07"  type="text" name="FlotacaoCaixa07"> 2040-CF-005      </div>
            <div id="FlotacaoCaixa08"  type="text" name="FlotacaoCaixa08"> 2040-CX-002      </div>
            <div id="FlotacaoCaixa09"  type="text" name="FlotacaoCaixa09"> 2040-CF-006      </div>
            <div id="FlotacaoCaixa10"  type="text" name="FlotacaoCaixa10"> 2040-CF-007      </div>
            <div id="FlotacaoCaixa11"  type="text" name="FlotacaoCaixa11"> 2040-CR-008      </div>          
            <div id="FlotacaoCaixa12"  type="text" name="FlotacaoCaixa12"> 2040-CX-001      </div>
            <div id="FlotacaoCaixa13"  type="text" name="FlotacaoCaixa13"> 2040-CX-003      </div>
                               
        </div>
    </body>
</html> 