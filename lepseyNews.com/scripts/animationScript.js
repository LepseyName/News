function buttonClick(event){
	if (event.currentTarget  != event.target)
		return;
	element = event.target;
	
	labels = event;
	
	if(element.parentElement.id == "categorie"){
		if(element.id != defaultType){
			smallNewsClear();
			defaultType = element.id;
			getNews(socketMy, 10, 0);
		}
	}else if(element.parentElement.id == "sort"){
		if(element.id != defaultSort){
			smallNewsClear();
			defaultSort = element.id;
			getNews(socketMy, 10, 0);
		}
	}else if(element.parentElement.id == "EditorAndAdminInterface"){
		if(element.id != defaultInterface){
			smallNewsClear();
			defaultInterface = element.id;
			getNews(socketMy, 10, 0);
		}
	}
	
	if( element.classList.toString().indexOf("radioButtonDown") >= 0){
		
	}else if( element.classList.toString().indexOf("radioButton") >= 0){	
				var allRadioButton = element.parentElement.children;
				for(var i=0; i<allRadioButton.length; i++){
					if (allRadioButton[i].classList.toString().indexOf("radioButtonDown") >=0){
						allRadioButton[i].classList.add("radioButton");
						allRadioButton[i].classList.remove("radioButtonDown");
					}
				}
				element.classList.add("radioButtonDown");
				element.classList.remove("radioButton");
	}else if( element.classList.toString().indexOf("buttonDown") >= 0){
				element.classList.remove("buttonDown");
				element.classList.add("button");
	}else if( element.classList.toString().indexOf("button") >= 0){
				element.classList.add("buttonDown");
				element.classList.remove("button");
	}
}

function setCooka(name, cooka){
	document.cookie = name + "=" + cooka;
	cookiesMy = cooka;
}

function getCooka(name){	
	let all = document.cookie.split(' ').join('').split(';');
	let m = new Map();
	for(let i=0; i<all.length; i++)
		m.set(all[i].split('=')[0], all[i].split('=')[1]);
	
	if(name == undefined)
		return m;
	else 
		return m.get(name); 
}

function gootLogin(name, photo, isAdmin, cooka){
	closeLogin();
	document.getElementById("headerLoginImg").src = "_data/photo/" + photo;
	
	document.getElementById("UserInfoImgSrc").src = "_data/photo/" + photo;
	document.getElementById("UserInfoName").innerHTML = name;
	document.getElementById("UserInfoIsAdmin").innerHTML =  (isAdmin === "True") ? "Admin" : "Editor";
	
	bindOnID("headerLoginImg", userClick);
	bindOnID("UserInfo", userClick);
	setCooka("123user2cooka", cooka);
	
	User.name = name;
	User.photo = photo;
	User.isAdmin = isAdmin;
	
	if(isAdmin === "True")
		adminInterfaceOpen();
	else
		editorInterfaceOpen();
}

function showComplaints(news, complaintArray){
	document.getElementById("fullNews").style.display = "none";
	document.getElementById("smallNews").style.display = "none";
	document.getElementById("Complaints").style.display = "block";
	
	nameNewsC.innerHTML = news[1];
	infoTypeC.innerHTML = news[2];
	infoViewsC.innerHTML = news[3];
	infoDescriptionC.innerHTML = news[4];
	
	complaintsFeed.innerHTML = "";
	for(let i=0; i<complaintArray.length; i++)
		complaintsFeed.innerHTML += "<div class=\"complaintSmall\"><div class=\"radioButton\" onclick=\"deleteComplain(" + news[0] + "," + complaintArray[i][0] + ")\">удалит</div><div class=\"complaintName\">" + complaintArray[i][1] + "^" + complaintArray[i][2] + "</div><div class=\"complaintDescription\">" + complaintArray[i][3] + "</div></div>";
}

function closeShowComplaints(){
	document.getElementById("fullNews").style.display = "none";
	document.getElementById("smallNews").style.display = "block";
	document.getElementById("Complaints").style.display = "none";
}

function editorInterfaceOpen(){	
	document.getElementById("add").style.display = "none";
	document.getElementById("EditorAndAdminInterface").style.display = "block";
	document.getElementById("AllNews").style.display = "block";
	document.getElementById("MyNews").style.display = "block";
	document.getElementById("MyEditors").style.display = "none";
	document.getElementById("MyAdmins").style.display = "none";
}

function adminInterfaceOpen(){
	editorInterfaceOpen();
	document.getElementById("MyEditors").style.display = "block";
	document.getElementById("MyAdmins").style.display = "block";
}


function selectImage(image, src){
	if(!src && imageAdd.files && imageAdd.files[0]){
		src = imageAdd.files[0]; 		
	}
	let reader = new FileReader();
	reader.onload = function(e){
		image.src = e.target.result;
	}
	
	reader.readAsDataURL(src);
}

