let newsId = undefined;


			   
			   
function EditorExecCommand(command_param, param)
{
	newTextArea.focus();
	param == undefined ? newTextArea.document.execCommand(command_param) : newTextArea.document.execCommand(command_param, true, param);
}

function enterLink(){
	PopUpWindowAlias.style.display = "block";
	Alias.style.display = "block";
}

function closeLink(){
	EditorExecCommand('createLink', link.value);
	link.value = "";
	PopUpWindowAlias.style.display = "none";
	Alias.style.display = "none";
}

function enterPicture(){
	PopUpWindowAlias.style.display = "block";
	Pictures.style.display = "block";
}


			   
function closeEnterPicture(){
	let s = "<img src=\" ";
	s += pictureLink.value + "\" ";
	
	s += "style=\" ";
	if(pictureWidth.value != "") s += " width: " + pictureWidth.value + "px;";
	if(pictureHeight.value != "") s += " height: " + pictureHeight.value + "px;";
	if(pictureVertical.value != "") s += " margin-top: " + pictureVertical.value + "px; margin-bottom: " + pictureVertical.value + "px;";
	if(pictureGorizontal.value != "") s += " margin-left: " + pictureGorizontal.value + "px; margin-right: " + pictureGorizontal.value + "px;";
	if(pictureBorder.value != "") s += " border-width: " + pictureBorder.value + "px;";
	if(pictureRadius.value != "") s += " border-radius: " + pictureRadius.value + "px;";
	if(pictureStyle.value != "") s+= pictureStyle.value;	
	s+= "\">";

	EditorExecCommand('insertHTML', s);
	pictureLink.value = "";
	pictureWidth.value= "";
	pictureHeight.value= "";
	pictureVertical.value = "";
	pictureGorizontal.value = "";
	pictureBorder.value = "";
	pictureRadius.value = "";
	pictureStyle = "";

	PopUpWindowAlias.style.display = "none";
	Pictures.style.display = "none";
}

function enterTable(){
	PopUpWindowAlias.style.display = "block";
	Tables.style.display = "block";
}

function closeEnterTable(){
	let x, y;
	if(tableWidth.value != "") x = Number(tableWidth.value);
	else return;
	if(tableHeight.value != "")y = Number(tableHeight.value);
	else return;
	
	if(x<1 || y<1) return;

	let td = "<td style=\"";
	td += " border: " + tableBorderType.value + " 1px blue;";
	if(tableVertical.value != "") td += " padding-top: " + tableVertical.value + "px; padding-bottom: " + tableVertical.value + "px;";
	if(tableGorizontal.value != "") td += " padding-left: " + tableGorizontal.value + "px; padding-right: " + tableGorizontal.value + "px;";
	if(tableBorder.value != "") td += " border-width: " + tableBorder.value + "px;";
					
	td+= "\">";
	
	let table = "<table>";
	for(let i =0; i<y; i++){
		table += "\n<tr>";
		for(let j =0; j<x; j++){
			table += td;
			table += "</td>\n";
		}
		table += "</tr>\n";
	}
	table+="</table>";
	
	console.log(table);
	EditorExecCommand('insertHTML', table);
	
	tableWidth.value = "";
	tableHeight.value= "";
	tableVertical.value = "";
	tableGorizontal.value = "";
	tableBorder.value = "";

	PopUpWindowAlias.style.display = "none";
	Tables.style.display = "none";
}

function initializeColor(element){
	let text = element.innerHTML;
	
	for(let i = 0; i<256; i+=64 )
		for(let j = 0; j<256; j+=64 )
			for(let k = 0; k<256; k+=64 ){
				let rgb = "rgb(" + i + "," + j + "," + k + ")";
				text += "<option style=\" width: 10px; background-color: " + rgb + "; color: " + rgb + ";\">" + rgb + "</option>\n";
			}
			
	element.innerHTML=text;
}

function enterTextColor(){
	textColor.style.background=textColor.value;
	EditorExecCommand('foreColor', textColor.value);
}

function enterBackgroundColor(){
	backgroundColor.style.background=backgroundColor.value;
	EditorExecCommand('backColor', backgroundColor.value);
}

function createNews(){
	errorPrint("");
	let name = newsNameIn.value, 
		description = newsDescriptionText.value,
		photo = imageAdd.files && imageAdd.files[0] ? imageAdd.files[0] : "_",
		category = newsTypeSelect.value,
		text = new XMLSerializer().serializeToString(newTextArea.document);
	
	if(name.length < 10 ||  name.length > 20){
		errorPrint("Название  должно быть от 10 до 20 символов!");
		return;
	}
	
	if(description.length < 50 ||  description.length > 255){
		errorPrint("Описание должно быть от 50 до 255 символов!");
		return;
	}
	
	let body = newTextArea.document.body.innerHTML;
	if( body.length < 50 || body.length > 40_960){
		errorPrint("Текст должен быть от 50 до 40960!");
		return;
	}
	
	
	if( photo == "_" || photo.size < 2000 || photo.size > 4_000_000){
		photo = newsImage.src!=document.location.href? newsImage.src : "";
		if(photo == undefined || photo == ""){
			errorPrint("Выберите изображение!Фото должно быть от 2кб до 4Мб!");
			return;
		}
		
	}else{
		let name = Math.random().toString(36).replace('.', '') + photo.name.substring(photo.name.indexOf('.'));
		while(uploadFiles.get(name) != undefined)
			name = Math.random().toString(36).replace('.', '') + photo.name.substring(photo.name.indexOf('.'));
		uploadFiles.set(name, photo);
		photo=name;
	}
	
	let news = new Array(6);
	news[0] = newsId;
	news[1] = name;
	news[2] = description;
	news[3] = photo;
	switch(category){
		case 'Политика': news[4] = "POLITICS"; break;
		case 'Экономика': news[4] = "ECONOMY"; break;
		case 'Спорт': news[4] = "SPORT"; break;
		case 'Общество': news[4] = "SOCIETY"; break;
		default: errorPrint("Неверный тип новости!"); return;
	}

	news[5] = text;
	
	socketMy = uploadNews(socketMy, news, getCooka("123user2cooka"));
}

function errorPrint(message){
	newsErrorLabelText.innerHTML = message;
}

function openNews(news){
	
	newsNameIn.value = news[1];
	newsDescriptionText.value = news[5];
	newsImage.src = "_data/photo/" + news[2];
	switch(news[4]){
		case "POLITICS": newsTypeSelect.value = 'Политика'; break;
		case "ECONOMY": newsTypeSelect.value = 'Экономика'; break;
		case "SPORT": newsTypeSelect.value = 'Спорт'; break;
		case "SOCIETY": newsTypeSelect.value = 'Общество'; break;
		default: errorPrint("Неверный тип новости!"); return;
	}
    newTextArea.document.open();
	newTextArea.document.writeln(news[3]);
	newTextArea.document.close();
	newsId = news[0];
}

function showEditorsError(message){
	errorPrint(message);
}