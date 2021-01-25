window.onload = function(){
	bindOnID("LOGIN_B", loginClick);
	bindOnID("headerLoginImg", loginClick);
	
	newTextArea.document.designMode = "on";
	newTextArea.document.open();
	newTextArea.document.writeln('<!DOCTYPE html><html><head><meta http-equiv="Content-Type" content="text/html; charset=UTF-8"></head><body contenteditable="true" spellcheck="false"></body></html>');
	newTextArea.document.close();
	EditorExecCommand('enableObjectResizing');
	EditorExecCommand('styleWithCSS', true);

	PopUpWindowAlias.onclick = function(event){ click(event);};
	
	bindOnID("LOGIN_B", loginClick);
	bindOnID("headerLoginImg", loginClick);
	cookiesMy = getCooka("123user2cooka");
	if(cookiesMy != undefined && cookiesMy.length > 0)
		loginCooka(socketMy, cookiesMy);
	
	param = getAllUrlParams();
	if(param['newsid']){
		id = Number(param['newsid']);
		getOneNews(socketMy, id);
	}
}