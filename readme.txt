

DATABASE
	create database bntunews;
	mysql.exe -u root -p bntunews < ~dump.sql
        create user 'server1'@'localhost' DENTIFIED BY 'q4A5dEltA';                 // login and password check in init file
	grant update, select, insert, delete on bntunews.* to 'server1'&'localhost';
        FLUSH PRIVILEGES;

SERVER
      server\Server\Server\bin\Debug\netcoreapp3.1 ====> init - this a initialize file server

base_path_image = C:\openserver\domains\lepseyNews.com\_data\photo ---address save photo for site
data_base_adress  = localhost
data_base_name = bntunews
data_base_login   = server1
data_base_password= q4A5dEltA
general_port    = 2345
all_port_users  = 1300
all_port_editors= 2700
all_port_admins = 23451
log_file_name  = logServer.txt 						--- full XYETA no nado
server_initiale= server_init.txt					--- HEADERS INFA DONT update!

SITE....
instal OpenServer(only basic modules other ne nado!)
in the tray windows flag => rights button => papka s proektami
LepseyNews.com relocate in this papka s proektami
in the tray windows flag => rights button => start


!execute in order!

full database
site all before start
server init file (base_path_image now enter and database data(only MYSQL!))
server start
site start