function deleteMyUser(userId){
	let isAll = confirm("Удалить всех его админов и редакторов с новостями?(отмена они останутся)");
	deleteEditors(socketMy, userId, isAll, getCooka("123user2cooka"));
}

function clickOnType(isE){
	if (isAdminEditorHTML == isE){
		if(isE){
			isAdminEditorHTML = false;
			document.getElementById("isAdmin").classList.add("radioButton");
			document.getElementById("isAdmin").classList.remove("radioButtonDown");
			
			document.getElementById("isEditor").classList.remove("radioButton");
			document.getElementById("isEditor").classList.add("radioButtonDown");
		}else{
			isAdminEditorHTML = true;
			document.getElementById("isAdmin").classList.remove("radioButton");
			document.getElementById("isAdmin").classList.add("radioButtonDown");
			
			document.getElementById("isEditor").classList.remove("radioButtonDown");
			document.getElementById("isEditor").classList.add("radioButton");
			
		}
	}
}

function click(event){
	console.log("------------");
	if (event.currentTarget  != event.target)
		return;
	PopUpWindowAlias.style.display = "none";
	Pictures.style.display = "none";
	Tables.style.display = "none";
	Alias.style.display = "none";
}

function bindOnID(ID, func, isElement=true){
	var label = document.getElementById(ID);

	label.onclick = function(event){ func(event);};
}

function loginClick(event){
	console.log("loginClick");
    if (event.currentTarget  != event.target)
		return;
    element = event.currentTarget;
	
	if(element.id == "headerLoginImg"){
		document.getElementById("LOGIN_B").style.display = "block";
	}else if(element.id == "LOGIN_B"){
		element.style.display = "none";
	}
}

function validateEmail(email) {
  const re = /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
  return re.test(email);
}

function bindOnClassName(className, func){
	labels = document.getElementsByClassName(className);

	for(var i=0; i<labels.length; i++){
		labels[i].onclick = function(event){ func(event);};
	}
}

function bindOnID(ID, func, isElement=true){
	var label = document.getElementById(ID);

	label.onclick = function(event){ func(event);};
}


function loginClick(event){
	console.log("loginClick");
    if (event.currentTarget  != event.target)
		return;
    element = event.currentTarget;
	
	if(element.id == "headerLoginImg"){
		document.getElementById("LOGIN_B").style.display = "block";
	}else if(element.id == "LOGIN_B"){
		element.style.display = "none";
	}
}

function userClick(event){
	console.log("userClick");
    if (event.currentTarget  != event.target)
		return;
    element = event.currentTarget;
	
	if(element.id == "headerLoginImg"){
		document.getElementById("UserInfo").style.display = "block";
	}else if(element.id == "UserInfo"){
		element.style.display = "none";
	}
}

function chechString(str){
	let goodChar = "qwertyuiopasdfghjklzxcvbnm1234567890_";
	str = str.toLowerCase();
	for(let i=0; i<str.length; i++)
		if(goodChar.indexOf(str[i]) < 0)
			return false;
	return true;
}

function logIn(){
	let name = document.getElementById("login").value;
	let password = document.getElementById("password").value;
	
	if(name.length < 8 || name.length > 20){
		errorLogin("Логин должен содержать от 8 до 20 символов");
		return;
	}
	
	if(password.length < 8 || password.length > 20){
		errorLogin("Пароль должен содержать от 8 до 20 символов");
		return;
	}
	
	if(!chechString(name)){
		errorLogin("Логин может содержать только латинские буквы, цифры и знак подчёркивания");
		return;
	}
	
	if(!chechString(password)){
		errorLogin("Пароль может содержать только латинские буквы, цифры и знак подчёркивания");
		return;
	}
	
	login(socketMy, name, password);
}

function logOut(){	
	logout(socketMy, name, getCooka("123user2cooka"));
}

function closeLogin(){
    element = document.getElementById("LOGIN_B");
	element.style.display = "none";
	document.getElementById("login").value = "";
	document.getElementById("password").value = "";
	
}

function errorLogin(message){
	document.getElementById("errorLoginMessage").innerHTML = message;
}

function getAllUrlParams(url) {
  // извлекаем строку из URL или объекта window
  var queryString = url ? url.split('?')[1] : window.location.search.slice(1);
 // объект для хранения параметров
  var obj = {};
  // если есть строка запроса
  if (queryString) {
    queryString = queryString.split('#')[0];  // данные после знака # будут опущены    
    var arr = queryString.split('&'); 		// разделяем параметры
   for (var i=0; i<arr.length; i++) {
      // разделяем параметр на ключ => значение
      var a = arr[i].split('=');
      // обработка данных вида: list[]=thing1&list[]=thing2
	var paramNum = undefined;
	 var paramName = a[0];
    if(paramName.indexOf('[') > 0 && paramName.indexOf(']') > 0){
		paramNum = Number(paramName.substring(paramName.indexOf('['), paramName.indexOf(']')));
	}		
      var paramValue = typeof(a[1])==='undefined' ? false : a[1];
      // преобразование регистра
      paramName = paramName.toLowerCase();
      paramValue = paramValue.toLowerCase();
      // если ключ параметра уже задан
      if (obj[paramName]) {
        // преобразуем текущее значение в массив
        if (typeof obj[paramName] === 'string') {
          obj[paramName] = [obj[paramName]];
        }
        // если не задан индекс...
        if (typeof paramNum === 'undefined') {
          // помещаем значение в конец массива
          obj[paramName].push(paramValue);
        }else {
          // размещаем элемент по заданному индексу
          obj[paramName][paramNum] = paramValue;
        }
      }else {
        obj[paramName] = paramValue;
      }
    }
  } 
  return obj;
}