function openCloseComplaint(event){
	var element = document.getElementById("complaint");
	
	if(getComputedStyle(element).display != "none"){
		element.style.display = "none";
	}else{
		element.style.display = "block";
	}
}

function createComplaint(){
	console.log("complaint create...");
	var complaint = new Array(4);
	
	if(fullNewsId != undefined)
		complaint[0] = fullNewsId;
	else{
		console.log("id not good");
		return;	
	}
	
	
	var email = document.getElementById("complaintEmail").value;
	if(validateEmail(email))
		complaint[1] = email; 
	else{
		console.log("email not good");
		return;
	}
	
	
	var name = document.getElementById("complaintName").value;
	complaint[2] = name;
	
	var text = document.getElementById("complaintText_").value;
	complaint[3] = text;
	
	socketMy = getComplaint(socketMy, complaint);	
}

function closeFullNews(event){
	var element = document.getElementById("fullNews");
	
	var element2 = document.getElementById("smallNews");
	
	
	if(getComputedStyle(element).display != "none"){
		element.style.display = "none";
		element2.style.display = "block";
	}

}

function openFullNews(id){
	socketMy = getOneNews(socketMy, id);
}

function createFullNews(news){
	var elem;
	
	elem = document.getElementById("info").innerHTML = "<div id=\"infoCreater\"><img src=\"_data/photo/" + news[10] +"\"><p>" + news[9] + "</p></div><div id=\"infoType\"><img src=\"source/type.png\"><p>" + news[4] + "</p></div><div id=\"infoViews\"><img src=\"source/views.png\"><p>" + news[7] + "</p></div><div id=\"infoData\"><img src=\"source/datetime.png\"><p>" + news[8] + "</p></div><div id=\"infoComplaint\"><img src=\"source/Complaint.png\"><div id=\"infoComplaintButton\" class=\"button\" onclick =\"openCloseComplaint()\">Жалоба</div></div><div id=\"infoBack\"><img src=\"source/Complaint.png\"><div class=\"button backButton\" onclick=\"closeFullNews()\">Вернуться назад</div></div>";
		
	elem = document.getElementById("news").innerHTML = "<div id=\"newsHeader\" style=\"background-image: url('_data/photo/" + news[2] + "');\">" + news[1] + "</div><div id=\"newsText\">" + news[3] + "</div><div id=\"backButton\" class=\"button backButton\" onclick=\"closeFullNews()\">Вернуться назад</div>";
	
	var element = document.getElementById("fullNews");
	var element2 = document.getElementById("smallNews");
	
	
	if(getComputedStyle(element).display == "none"){
		element.style.display = "block";
		element2.style.display = "none";
	}
	
	fullNewsId=news[0];
}

function showComplaintStatus(message){
	let el = document.getElementById("complaintStatus");
	
	el.style.display="block";
	el.innerHTML = message;
	
	setTimeout(closeComplaintStatus, 5000);
}

function closeComplaintStatus(){
	console.log("close status");
	let el = document.getElementById("complaintStatus");
	el.style.display="none";
	el.innerHTML = "";	
}