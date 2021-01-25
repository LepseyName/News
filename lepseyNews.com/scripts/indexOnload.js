window.onload = function(){

	bindOnClassName("radioButton", buttonClick);
	bindOnClassName("radioButtonDown", buttonClick);
	
	bindOnID("LOGIN_B", loginClick);
	bindOnID("headerLoginImg", loginClick);
	bindOnID("infoComplaintButton", openCloseComplaint);
	
	bindOnClassName("backButton", closeFullNews);
	
	newsFeed = document.getElementById("newsFeed");
	
	setDefault();
	cookiesMy = getCooka("123user2cooka");
	if(cookiesMy != undefined && cookiesMy.length > 0)
		loginCooka(socketMy, cookiesMy);
	
	
		
	getNews(socketMy, 10, 0);
}