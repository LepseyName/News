using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
    public struct ServerHeaders
    {
        //service
        public  string EndSeans;                 
        public  string Good;                
        public  string EndMessages;                
        public  string Separator;               
        public  string ReplaceSeparator;                 
        
        //headers
            //user
            public  string GetAllNews;                    
            public  string GetNews;                    
            public  string Complaint;
            //editor
            public string GetImage;
            public  string GetAllMyNews;       
            public  string AnyNews;                       
            public  string DeleteNews;
            //admin
            public  string GetAllMyEditors;
            public  string AnyEditors;
            public  string DeleteEditor;
            //lodin_logout
            public  string LogIn;
            public  string LogUot;
            public string Key; 

        //bytes
            //service
            public byte[] EndSeansByte;                                
            public byte[] EndMessagesByte;               
            public byte[] SeparatorByte;                               

        public bool headersToByte() {
            if (this.EndSeans != null)
                this.EndSeansByte = Encoding.UTF8.GetBytes(this.EndSeans);
            else
                return false;
            if (this.EndMessages != null)
                this.EndMessagesByte = Encoding.UTF8.GetBytes(this.EndMessages);
            else
                return false;
            if (this.Separator != null)
                this.SeparatorByte = Encoding.UTF8.GetBytes(this.Separator);
            else
                return false;
           
            return true;
        }
    }
}
