let userId = undefined;
let parentId = undefined;

function showEditors(edit){
	userId = edit[0];
	parentId = edit[1];
	
	userNameText.value = edit[3];
	userLoginText.value = edit[2];
	clickOnType(! Boolean(edit[5]));
	
	newsCreationButton.innerHTML = "Сохранить изменения";
	newsCreationButton.onclick = function(e){
		createUser(userId);
	}
	
	let i = document.getElementById('userImage');
	i.src = "_data/photo/" + edit[4]
}

function createUser(userId){
	let user = new Array();
	let name = userNameText.value,
	    login = userLoginText.value,
		password = userPasswordText.value,
		passwordRepeat = userPasswordRepeatText.value,
		photo = imageAdd.files && imageAdd.files[0] ? imageAdd.files[0] : "_",
		isAmin = isAdminEditorHTML;
	
	if(name.length < 10 || name.length > 20){
		showEditorsError("Имя должно быть от 10 до 20 символов!");
		return;
	}
	
	if(login.length < 8 || login.length > 20){
		showEditorsError("Логин должен быть от 8 до 20 символов!");
		return;
	}
	
	if(password != passwordRepeat){
		showEditorsError("Пароли не совпадают!");
		return;
	}
	
	if(password.length < 8 || password.length > 20){
		showEditorsError("Пароль должен быть от 8 до 20 символов");
		return;
	}
	
	if(!chechString(login)){
		showEditorsError("Логин должен содержать латинские символы, цифры и знак подчеркивания!");
		return;
	}
	
	
	if( photo == "_" || photo.size < 2000 || photo.size > 4_000_000){
		photo = userImage.src!=document.location.href? userImage.src : "";
		if(photo == undefined || photo == ""){
			showEditorsError("Фото должно быть от 2кб до 4Мб!");
			return;
		}
		
	}else{
		let name = Math.random().toString(36).replace('.', '') + photo.name.substring(photo.name.indexOf('.'));
		while(uploadFiles.get(name) != undefined)
			name = Math.random().toString(36).replace('.', '') + photo.name.substring(photo.name.indexOf('.'));
		uploadFiles.set(name, photo);
		photo=name;
	}
	
	user.push( userId );
	user.push(name);
	user.push(login);
	user.push(password); 	
	user.push(photo);
	user.push(isAmin);
	
	if(userId == undefined)
		createEditors(socketMy, user, getCooka("123user2cooka"));
	else
		updateEditors(socketMy, user, getCooka("123user2cooka"));
}

function showEditorsGood(){
	newsErrorLabelText.innerHTML = "Успешно выполнено!";
}

function showEditorsError(message){
	newsErrorLabelText.innerHTML = message;
}

window.onload = function(){
	bindOnID("LOGIN_B", loginClick);
	bindOnID("headerLoginImg", loginClick);
	
    cookiesMy = getCooka("123user2cooka");
	if(cookiesMy != undefined && cookiesMy.length > 0)
		loginCooka(socketMy, cookiesMy);
	//PopUpWindowAlias.onclick = function(event){ click(event);};

	param = getAllUrlParams();
	if(param['userid']){
		id = Number(param['userid']);
		getOneUser(socketMy, id, cookiesMy);
	}
}