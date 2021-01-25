class USER{};
var isSocketOpen=false, isCrypted = false, User = new USER();
var separator = "|sepSEP|",
    ALL_NEWS  = "ALL_NEWS",
	ALL_MY_NEWS = "ALL_MY_NEWS",
	OK = "OK101OK",
	DeleteNews = "DEL_NEWS",
	GetNews = "NEWS",
	Complaint = "COMPLAINT",
	Key = "KEY",
	LogIn = "LOGIN",
	AnyNews = "NEW_OR_REGEDIT_NEWS",
	GetImage = "IMAGE",
	GetAllMyEditors = "ALL_MY_EDITORS",
	AnyEditors = "NEW_OR_REGEDIT_EDITOR",
	DeleteEditor = "DEL_EDITOR",
    endMessage= "ENDend";
	
var labels, newsFeed, loginButton, fullNewsId, cookiesMy;

var defaultType ="ALL",
	defaultInterface = "AllNews",
    defaultSort ="TIME";
	
var uploadFiles = new Map();
let isAdminEditorHTML = false;

function sleep(milliseconds) {
  const date = Date.now();
  let currentDate = null;
  do {
    currentDate = Date.now();
  } while (currentDate - date < milliseconds);
}

	
function getSocket(){
	var socket = new WebSocket("ws://localhost:1300");
	isSocketOpen=true;
	socket.onopen = function(e) {
		console.log("8======================D");
		isSocketOpen=true;	
	};

	socket.onmessage = function(event) {
		message = event.data.toString();
		if(message.indexOf(endMessage) > 0){
			message = message.substring(0, message.length - endMessage.length);
			array = message.split(separator);
			console.log("array: " + array);
			
			if(array[0] == ALL_NEWS && array.length >=2){
				var countNews = Number(array[1]);
				if (array.length != 0 && array.length >= countNews*6){
					var allNews = new Array();
					
					for(var i=2;countNews > 0;countNews--, i=i+6){
						var news = new Array();
						news.push(array[i + 0]);
						news.push(array[i + 1]);
						news.push(array[i + 2]);
						news.push(array[i + 3]);
						news.push(array[i + 4]);
						news.push(array[i + 5]);
						allNews.push(news);
					}
					while( newsFeed == undefined)sleep(100);
					addNews(allNews);
				}
			}else if(array[0] == GetNews && array.length >=2){
				if (array[1]== OK && array.length >= 10) {
					var news = new Array();
					for(var i=0; i<array.length - 2; i++)
						news.push(array[2 + i]);
					if(typeof(createFullNews) != "undefined"){
						createFullNews(news);
					}else{
						openNews(news);
					}
				}else{

				}	
			}else if(array[0] == ALL_MY_NEWS && array.length >=2){
				var countNews = Number(array[1]);
				if (array.length != 0 && array.length >= countNews*6){
					var allNews = new Array();
					
					for(var i=2;countNews > 0;countNews--, i=i+6){
						var news = new Array();
						news.push(array[i + 0]);
						news.push(array[i + 1]);
						news.push(array[i + 2]);
						news.push(array[i + 3]);
						news.push(array[i + 4]);
						news.push(array[i + 5]);
						allNews.push(news);
					}
					while( newsFeed == undefined)sleep(100);
					addMyNews(allNews);
				}
			}else if(array[0] == Complaint ){
				if(array.length >=8){
					var news = new Array();
					
					news.push(array[1]);
					news.push(array[2]);
					news.push(array[3]);
					news.push(array[4]);
					news.push(array[5]);
					news.push(array[6]);
					
					let countComplaint = Number(array[7]);
					let complaintArray = new Array();
					for(let i=0; i<countComplaint; i++){
						let buffer  = new Array();
						buffer.push(array[8 + 4*i + 0]);
						buffer.push(array[8 + 4*i + 1]);
						buffer.push(array[8 + 4*i + 2]);
						buffer.push(array[8 + 4*i + 3]);
						complaintArray.push(buffer);
					}
					console.log(news, complaintArray);				
					showComplaints(news, complaintArray);
				}else if(array.length >=2){
					if (array[1]== OK) {
						showComplaintStatus("Ваша жалоба записана. Ожидайте ответа");
					}else{
						showComplaintStatus(array[1]);
					}
				}
					
			}else if(array[0] == LogIn){
				if(array.length ==5)
					gootLogin(array[1], array[2], array[3], array[4]);
				if(array.length ==2)
					errorLogin(array[1]);
				
			}else if(array[0] == GetImage){
				if(array.length == 2){
					if(uploadFiles.get(array[1]) != undefined)
						socketMy = uploadImage(socketMy, array[1], cookiesMy);
				}
			}else if(array[0] == GetAllMyEditors){
				if(array.length >= 3){
					let count = Number(array[2]);
					let edit = new Array();
					for(let i=0; i<count; i++){
						let e = new Array();
						e.push(array[3 + i*6]);
						e.push(array[4 + i*6]);
						e.push(array[5 + i*6]);
						e.push(array[6 + i*6]);
						e.push(array[7 + i*6]);
						e.push(array[8 + i*6]);
						edit.push(e);
					}
					showAllEditors(edit);
				}
			}else if(array[0] == AnyEditors){
				if(array.length == 2){
					if(array[1] == OK)
						showEditorsGood();
					else
						showEditorsError(array[1]);
				}
				
				if(array.length == 8){
					let edit = new Array();
					for(let i=0; i<7; i++)	edit.push(array[i + 1]);
					showEditors(edit);
				}
			}
			
		}
		
	};

	socket.onclose = function(event) {
	  if (event.wasClean) {
	  } else {
	  }
	  isSocketOpen=false;
	};

	socket.onerror = function(error) {
		isSocketOpen=false;
	};
	return socket;
}

function sendOnServer(sock, message, isBadState = false){
	console.log("send: 1");
	if(sock==undefined || isSocketOpen == false || isBadState == true){
		sock = getSocket();
		setTimeout(sendOnServer, 1000, sock, message);
		return;
	}
	console.log("stat: " + sock.readyState);
	if (sock.readyState!=1){
		console.log("not send");
		setTimeout(sendOnServer, 1000, sock, message, true);
		return;
	}
	
	sock.send(message + endMessage);	
	console.log("send: " + message + endMessage);
	socketMy = sock;
	return sock;
}

function openSecretLine(){
	if(!isCrypted){
		
	}
}

function getAllMyEditors(sock, isAdmin, count, offset, cooka){
	let message = GetAllMyEditors + separator + isAdmin +separator + count +separator + offset + separator + cooka;
	return sendOnServer(sock, message);
}

function login(sock, name, password){
	let message = LogIn + separator + name + separator + password;
	return sendOnServer(sock, message);
}

function loginCooka(sock, cooka){
	let message = LogIn + separator + OK + separator + true + separator + cooka;
	return sendOnServer(sock, message);
}

function logout(sock, name, cooka){
	let message = LogIn + separator + OK + separator + false + separator + cooka;
	return sendOnServer(sock, message);
}	

function getNews(sock, count, offset){
	let message;
	if(getCooka("123user2cooka") != undefined)
		cookiesMy = getCooka("123user2cooka");
	if(defaultInterface === "MyNews" && cookiesMy != undefined)
		message = ALL_MY_NEWS + separator + cookiesMy + separator + defaultType + separator + defaultSort + separator + Number(count) + separator + Number(offset) + separator + false;
	else
		message = ALL_NEWS + separator + defaultType + separator + defaultSort + separator + Number(count) + separator + Number(offset);
	return sendOnServer(sock, message);
}

function getOneNews(sock, id){
	let message = GetNews + separator + Number(id) ;
	return sendOnServer(sock, message);	
}

function getComplaint(sock, complaint){
	let message = Complaint + separator + complaint[0] + separator + complaint[1] + separator + complaint[2] + separator + complaint[3];
	return sendOnServer(sock, message);	
}

function deleteNews(sock, id, cooka){
	let message = DeleteNews + separator + id + separator + cooka;
	return sendOnServer(sock, message);	
}

function uploadNews(sock, news, cooka){
	let message;
	if(news[0] != undefined)
		message = AnyNews + separator + Number(news[0]) + separator + news[1] + separator + news[2] + separator + news[3]  + separator + news[4]  + separator + news[5] + separator + cooka;
	else
		message = AnyNews + separator + news[1] + separator + news[2] + separator + news[3]  + separator + news[4]  + separator + news[5] + separator + cooka;
	return sendOnServer(sock, message);	
}

function uploadImage(sock, name, cooka){
	let f = uploadFiles.get(name);
	if(f != undefined){
		let reader = new FileReader();
		reader.readAsDataURL(f);
		
		reader.onload = function() {
			let data = reader.result;
			while(data.indexOf(',')>0)
				data = data.substring(data.indexOf(',') + 1);
			let message = GetImage + separator + name + separator + data + separator + cooka;
			return sendOnServer(sock, message);	
		};

		reader.onerror = function() {
			console.log(reader.error);
		};
	}
}

function getComplaintsNews(sock, id, cooka){
	let message = Complaint + separator + id + separator + cooka;
	return sendOnServer(sock, message);	
}

function deleteComplaintsNews(sock, idNews, id, cooka){
	let message = Complaint + separator + idNews + separator + id +separator + cooka;
	return sendOnServer(sock, message);	
}

function getOneUser(sock, id, cooka){
	let message = AnyEditors + separator + id + separator + cooka;
	return sendOnServer(sock, message);	
}

function createEditors(sock, user, cooka){
	let message = AnyEditors + separator + user[1] + separator + user[2] + separator + user[3] + separator + user[4] + separator + user[5] + separator + cooka;
	return sendOnServer(sock, message);	
}

function updateEditors(sock, user, cooka){
	let message = AnyEditors + separator + user[0] + separator + user[1] + separator + user[2] + separator + user[3] + separator + user[4] + separator + user[5] + separator + cooka;
	return sendOnServer(sock, message);	
}

function deleteEditors(sock, userId, isAll, cooka){
	let message = DeleteEditor + separator + userId + separator + isAll + separator + cooka;
	return sendOnServer(sock, message);	
}

//first news
var socketMy = getSocket();
