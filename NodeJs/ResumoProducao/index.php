<?php

if(isset($_COOKIE["session"]) == true)
{
	if(strlen($_COOKIE["session"]) ==  32)
	{
		setcookie("session", md5(time()) , time() + (86400 * 30), "/"); // 86400 = 1 day
		header("Location: ./global.php");
	}
}

if((isset($_POST["username"])) && (isset($_POST["pass"])))
{
	$usuario = $_POST["username"];
	$senha = $_POST["pass"];
	
	$allow_user = "vale_viga";
	$allow_pass = "v1l4vig1";
	
	if(($allow_user == $usuario) && ($allow_pass == $senha))
	{
		setcookie("session", md5(time()) , time() + (86400 * 30), "/"); // 86400 = 1 day
		header("Location: ./global.php");
	}
	else
	{
		echo "<script>setTimeout(function(){\$(\"#alert\").css(\"display\", \"none\");}, 2000); </script>";
		echo "<div id=\"alert\"> Usuário ou Senha Não Reconhecido </div>";
	}
}	
?>
<!DOCTYPE html>
<html lang="en">
<head>
	<title>Login V3</title>
	<meta charset="UTF-8">
	<meta name="viewport" content="width=device-width, initial-scale=1">
<!--===============================================================================================-->	
	<link rel="icon" type="image/png" href="images/icons/favicon.ico"/>
<!--===============================================================================================-->
	<link rel="stylesheet" type="text/css" href="vendor/bootstrap/css/bootstrap.min.css">
<!--===============================================================================================-->
	<link rel="stylesheet" type="text/css" href="fonts/font-awesome-4.7.0/css/font-awesome.min.css">
<!--===============================================================================================-->
	<link rel="stylesheet" type="text/css" href="fonts/iconic/css/material-design-iconic-font.min.css">
<!--===============================================================================================-->
	<link rel="stylesheet" type="text/css" href="vendor/animate/animate.css">
<!--===============================================================================================-->	
	<link rel="stylesheet" type="text/css" href="vendor/css-hamburgers/hamburgers.min.css">
<!--===============================================================================================-->
	<link rel="stylesheet" type="text/css" href="vendor/animsition/css/animsition.min.css">
<!--===============================================================================================-->
	<link rel="stylesheet" type="text/css" href="vendor/select2/select2.min.css">
<!--===============================================================================================-->	
	<link rel="stylesheet" type="text/css" href="vendor/daterangepicker/daterangepicker.css">
<!--===============================================================================================-->
	<link rel="stylesheet" type="text/css" href="css/util.css">
	<link rel="stylesheet" type="text/css" href="css/main.css">
<!--===============================================================================================-->

	<style>
		#alert{
			display: block;
			position: absolute;
			bottom: 1px;
			left:0px;
			width: 99.9vw;
			height: 22pt;
			font-size: 16pt;
			text-align: center;
			background-color: #f00;
			z-index: 1000;
			/*padding-bottom: 2px;*/
			padding:0px;
			margin:0px;
			color: #fff;
		}
	</style>
</head>
<body>
	
	<div class="limiter">
		<div class="container-login100" style="background-image: url('images/bg-01.jpg');">
			<div class="wrap-login100">
				<form class="login100-form validate-form" method="post" action="#">
					<span class="login100-form-logo">
						<!--<i class="zmdi zmdi-landscape"></i>-->
						<img  id="logoCemi" src="images/Cemi.jpeg">
					</span>

					<span class="login100-form-title p-b-34 p-t-27"> Cemi Process Optimization </span>

					<div class="wrap-input100 validate-input" data-validate = "Enter username">
						<input class="input100" type="text" name="username" placeholder="Username">
						<span class="focus-input100" data-placeholder="&#xf207;"></span>
					</div>

					<div class="wrap-input100 validate-input" data-validate="Enter password">
						<input class="input100" type="password" name="pass" placeholder="Password">
						<span class="focus-input100" data-placeholder="&#xf191;"></span>
					</div>

					<div class="contact100-form-checkbox">
						<input class="input-checkbox100" id="ckb1" type="checkbox" name="remember-me">
						<label class="label-checkbox100" for="ckb1">
							Remember me
						</label>
					</div>

					<div class="container-login100-form-btn">
						<button class="login100-form-btn">
							Login
						</button>
					</div>

					<!--div class="text-center p-t-90">
						<a class="txt1" href="#">
							Forgot Password?
						</a>
					</div-->
				</form>
			</div>
		</div>
	</div>
	

	<div id="dropDownSelect1"></div>
	
<!--===============================================================================================-->
	<script src="vendor/jquery/jquery-3.2.1.min.js"></script>
<!--===============================================================================================-->
	<script src="vendor/animsition/js/animsition.min.js"></script>
<!--===============================================================================================-->
	<script src="vendor/bootstrap/js/popper.js"></script>
	<script src="vendor/bootstrap/js/bootstrap.min.js"></script>
<!--===============================================================================================-->
	<script src="vendor/select2/select2.min.js"></script>
<!--===============================================================================================-->
	<script src="vendor/daterangepicker/moment.min.js"></script>
	<script src="vendor/daterangepicker/daterangepicker.js"></script>
<!--===============================================================================================-->
	<script src="vendor/countdowntime/countdowntime.js"></script>
<!--===============================================================================================-->
	<script src="js/main.js"></script>


</body>
</html>