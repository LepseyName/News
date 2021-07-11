function createSmallNews(array){
	var news = "<div class=\"newsSmall\" onclick=\"openFullNews(" +  array[0] + ")\"><div class=\"newsSmallName\" \">" + array[1] + "</div><div class=\"newsSmallDescription\">" + array[2] + "</div><div class=\"newsSmallImage\"><img src=\"_data/photo/" + array[3] + "\"></div><div class=\"newsSmallViews\"><img src=\"source/views.png\">" + array[4] + "</div><div class=\"newsSmallNameComplaint\">пожаловаться</div></div>";
	return news;
}

function createSmallMyNews(array){
	var news = "<div class=\"newsSmall\"><div class=\"newsSmallEditorsButtons\"><div class=\"button\" onclick=\"deleteMyNews(" + array[0] +")\">Удалить</div><div class=\"button\" onclick=\"window.open('\editor.html?newsid=" + array[0] + "')\">Редактировать</div><div class=\"button\" onclick=\"complaintsNews(" + array[0] + ")\">Жалобы</div></div><div class=\"newsSmallName\">" + array[1] + "</div><div class=\"newsSmallDescription\">" + array[2] + "</div><div class=\"newsSmallImage\"><img src=\"_data/photo/" + array[3] + "\"></div><div class=\"newsSmallViews\"><img src=\"source/views.png\">" + array[4] + "</div><div class=\"newsSmallNameComplaint\">пожаловаться</div></div>";
	return news;
}

function createSmallMyEditor(array){
	var news;
	if(array[2] == "True")
		news = "<div class=\"editSmall\" onclick=\"window.open('user.html?userid=" + array[0] + "')\"><div class=\"editSmallImage\"><img src=\"_data/photo/" + array[1] + "\"></div><div class=\"editSmallName\">" + array[3] + "</div><div class=\"editSmallNews\">News " + array[4] + "</div><div class=\"editSmallNews\">Admins " + array[5] + "</div><div class=\"editSmallEditorsButtons\"><div class=\"button\" onclick=\"deleteMyUser(" + array[0] + ")\">удалить</div><div class=\"button\" onclick=\"window.open('user.html?userid=" + array[0] + "')\">Редактировать</div></div></div>";
	else news = "<div class=\"editSmall\" onclick=\"window.open('user.html?userid=" + array[0] + "')\"><div class=\"editSmallImage\"><img src=\"_data/photo/" + array[1] + "\"></div><div class=\"editSmallName\">" + array[3] + "</div><div class=\"editSmallNews\">News " + array[4] + "</div><div class=\"editSmallEditorsButtons\"><div class=\"button\" onclick=\"deleteMyUser(" + array[0] + ")\">удалить</div><div class=\"button\" onclick=\"window.open('user.html?userid=" + array[0] + "')\">Редактировать</div></div></div>";
	
	return news;
}

function setDefault(){
	document.getElementById(defaultType).classList.add("radioButtonDown");
	document.getElementById(defaultSort).classList.add("radioButtonDown");
}	

function addNews(newsArray){
	console.log("news count: " + newsArray.length);
	while( newsFeed == undefined);
	for(var i=0; i<newsArray.length; i++){
		newsFeed.innerHTML = newsFeed.innerHTML + createSmallNews(newsArray[i]);
	}
}

function addMyNews(newsArray){
	console.log("MyNews count: " + newsArray.length);
	while( newsFeed == undefined);
	for(var i=0; i<newsArray.length; i++){
		newsFeed.innerHTML = newsFeed.innerHTML + createSmallMyNews(newsArray[i]);
	}
}

function showAllEditors(edit){
	while( newsFeed == undefined);
	for(var i=0; i<edit.length; i++){
		newsFeed.innerHTML = newsFeed.innerHTML + createSmallMyEditor(edit[i]);
	}
}

function smallNewsClear(){
	while( newsFeed == undefined);
	newsFeed.innerHTML = "";
}

function deleteMyNews(id){
	deleteNews(socketMy, id, getCooka("123user2cooka"))
}

function complaintsNews(id){
	getComplaintsNews(socketMy, id, getCooka("123user2cooka"));
}

function deleteComplain(idNews, id){
	deleteComplaintsNews(socketMy, idNews, id, getCooka("123user2cooka"));
	complaintsNews(idNews);
}

function getEditors(isAdmin){
	smallNewsClear();
	getAllMyEditors(socketMy, isAdmin, 10, 0, getCooka("123user2cooka"));
}
