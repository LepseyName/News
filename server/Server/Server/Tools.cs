using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;

namespace Server
{
    public static class Tools
    {
        public static bool isGoodURL(string url)
        {
            string goodChar = "0123456789qwertyuiopasdfghjklzxcvbnm./-_";
            for (int i = 0; i < url.Length; i++)
                if (goodChar.IndexOf(url[i]) < 0)
                    return false;

            for (int i = 0; i < url.Length - 1; i++)
                if (url[i] == '.' || url[i] == '/')
                    if (url[i + 1] == '.' || url[i + 1] == '/')
                        return false;

            string domen = url.IndexOf('/') >= 0 ? url.Substring(0, url.IndexOf('/')) : url;
            if (domen.IndexOf('.') <= 0 || domen.LastIndexOf('.') == domen.Length - 1)
                return false;
            return true;
        }

        static public byte[] concateBytes(byte[] array1, byte[] array2, int count =-1)
        {
            if (count < 0) count = array2.Length;
            if (count > array2.Length) count = array2.Length;
            byte[] newArray = new byte[array1.Length + count];
            
            for (int i = 0; i < array1.Length; i++)
                newArray[i] = array1[i];

            for (int i = 0; i < count; i++)
                newArray[array1.Length + i] = array2[i];
            return newArray;
        }

        public static int FirstIndexOf(byte[] array, byte[] subArray)
        {
            if (array == null || subArray == null)
                throw new ArgumentNullException("Null");
            if (array.Length == 0 || subArray.Length == 0)
                return -1;
            if (array.Length < subArray.Length)
                return -1;


            bool isFinde = true;
            for (int i = 0; i < array.Length - subArray.Length + 1; i++)
            {
                isFinde = true;
                for (int j = 0; j < subArray.Length; j++)
                    if (array[i + j] != subArray[j])
                    {
                        isFinde = false;
                        break;
                    }
                if (isFinde) return i;
            }
            return -1;
        }

        public static byte[] SubBytes(byte[] array, long first = 0, long count = -1)
        {
            if (array == null)
                throw new ArgumentNullException("Null");
            if (array.Length == 0 || first < 0)
                throw new ArgumentException("Empity");
            if (count < 0)
                count = array.Length - first;

            byte[] subArray = new byte[count];
            for (int i = 0; i < count; i++)
                subArray[i] = array[first + i];

            return subArray;
        }

        public static byte[][] Split(byte[] array, byte[] separator, bool notEmpity = false)
        {
            if (array == null || separator == null)
                throw new ArgumentNullException("Null");
            if (array.Length == 0 || separator.Length == 0)
                return new byte[][] { };
            if (array.Length < separator.Length)
                return new byte[][] { };

            List<byte[]> list = new List<byte[]>();

            while (array.Length > 0)
            {
                int index = FirstIndexOf(array, separator);
                if (index < 0)
                {
                    if (notEmpity)
                    {
                        if (array.Length > 0) list.Add(array);
                    }
                    else
                        list.Add(array);
                    break;
                }
                else
                {
                    if (notEmpity)
                    {
                        byte[] b = SubBytes(array, 0, index);
                        if (b.Length > 0) list.Add(b);
                    }
                    else
                        list.Add(SubBytes(array, 0, index));

                    array = SubBytes(array, index + separator.Length);
                }
            }
            return list.ToArray();
        }

        public static byte[] ConcatByte(byte[] array1, byte[] array2, int count = -1)
        {

            if (count < 0) count = array2.Length;
            if (count > array2.Length) count = array2.Length;
            byte[] newArray = new byte[array1.Length + count];
            for (int i = 0; i < array1.Length; i++)
                newArray[i] = array1[i];

            for (int i = 0; i < count; i++)
                newArray[array1.Length + i] = array2[i];
            return newArray;
        }

        public static bool validateEmail(string email)
        {
            string goodChar = "qwertyuiopasdfghjklzxcvbnm1234567890._-+'@";

            email = email.ToLower();
            for (int i = 0; i < email.Length; i++)
                if (goodChar.IndexOf(email[i]) == -1)
                    return false;

            int indexS = email.IndexOf('@');
            if (indexS == -1 || indexS == 0 || indexS == email.Length-1)
                return false;

            string name = email.Substring(0, indexS), host = email.Substring(indexS+1);

            try
            {
                PingReply r = (new Ping()).Send(host);
                if (r.Status != IPStatus.Success)
                    return false;
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public static string byteArrayToHexString(byte[] array)
        {
            const string word = "0123456789abcdef";
            StringBuilder s = new StringBuilder(2 * array.Length);
            byte right, left;

            for (int i=0; i< array.Length; i++)
            {
                right = (byte)(array[i] & 15);
                left  = (byte)((array[i]>>4) & 15);

                s.Append(word[left]);
                s.Append(word[right]);
            }
            return s.ToString();
        }

        public static byte[] hexStringToByteArray(string hex)
        {
            hex = hex.ToLower();
            const string word = "0123456789abcdef";
            byte[] array = new byte[hex.Length/2];
            byte right, left;

            for (int i = 0; i < hex.Length-1; i+=2)
            {
                right = (byte)(word.IndexOf(hex[i+1]));
                left = (byte)(word.IndexOf(hex[i]));

                array[i / 2] = (byte)( (left<<4) | right);
            }
            return array;
        }

        public static string RandomString(int len)
        {
            StringBuilder s = new StringBuilder("", len);
            string symb = "ABCDEFGHIJKLMNOPQRSTUVWXYZqwertyuiopasdfghjklzxcvbnm0123456789";
            Random rnd = new Random();

            for (int i = 0; i < len; i++)
                s.Append(symb[rnd.Next(0, symb.Length)]);
            return s.ToString();
        }

    }
}
