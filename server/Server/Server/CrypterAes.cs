using System;

using System.Text;
using System.Security;
using System.IO;
using System.Security.Cryptography;

namespace Server
{
    public class CrypterAES
    {
        private static int keySize = 192;
        private static CipherMode mode = CipherMode.CBC;
        private static PaddingMode padding = PaddingMode.Zeros;

        private System.Security.Cryptography.Aes AES;

        public CrypterAES()
        {
            this.AES = System.Security.Cryptography.Aes.Create();
            this.AES.Mode = CrypterAES.mode;
            this.AES.Padding = CrypterAES.padding;
            this.AES.KeySize = CrypterAES.keySize;
            this.AES.GenerateIV();
            this.AES.GenerateKey();


        }

        public CrypterAES(int mode, int padding, byte[] key, byte[] IV)
        {
            if (key == null || key.Length <= 0)
                throw new ArgumentNullException("Key");
            if (IV == null || IV.Length <= 0)
                throw new ArgumentNullException("IV");

            this.AES = System.Security.Cryptography.Aes.Create();

            this.setMode(mode);
            this.setPadding(padding);

            this.AES.KeySize = key.Length * 8;
            this.AES.Key = key;
            this.AES.IV = IV;
        }

        public static void setPadding(PaddingMode i)
        {
            if (i != null) CrypterAES.padding = i;
        }

        public static void setMode(CipherMode i)
        {
            if (i != null) CrypterAES.mode = i;
        }

        public void setPadding(int i)
        {
            switch (i)
            {
                case 1:
                    {
                        this.AES.Padding = PaddingMode.ANSIX923;
                        break;
                    }
                case 2:
                    {
                        this.AES.Padding = PaddingMode.ISO10126;
                        break;
                    }
                case 3:
                    {
                        this.AES.Padding = PaddingMode.PKCS7;
                        break;
                    }
                case 4:
                    {
                        this.AES.Padding = PaddingMode.Zeros;
                        break;
                    }
            }
        }

        public void setMode(int i)
        {
            switch (i)
            {
                case 1:
                    {
                        this.AES.Mode = CipherMode.CBC;
                        break;
                    }
                case 2:
                    {
                        this.AES.Mode = CipherMode.CFB;
                        break;
                    }
                case 3:
                    {
                        this.AES.Mode = CipherMode.CTS;
                        break;
                    }
                case 4:
                    {
                        this.AES.Mode = CipherMode.ECB;
                        break;
                    }
                case 5:
                    {
                        this.AES.Mode = CipherMode.OFB;
                        break;
                    }
            }
        }

        public static void setKeySize(int i)
        {
            if (i == 128 || i == 192 || i == 256)
                CrypterAES.keySize = i;
        }

        public static int getPadding()
        {
            switch (CrypterAES.padding)
            {
                case PaddingMode.ANSIX923:
                    {
                        return 1;
                    }
                case PaddingMode.ISO10126:
                    {
                        return 2;
                    }
                case PaddingMode.PKCS7:
                    {
                        return 3;
                    }
                case PaddingMode.Zeros:
                    {
                        return 4;
                    }
                default:
                    {
                        return 0;
                    }
            }
        }

        public int getPaddin()
        {
            switch (this.AES.Padding)
            {
                case PaddingMode.ANSIX923:
                    {
                        return 1;
                    }
                case PaddingMode.ISO10126:
                    {
                        return 2;
                    }
                case PaddingMode.PKCS7:
                    {
                        return 3;
                    }
                case PaddingMode.Zeros:
                    {
                        return 4;
                    }
                default:
                    {
                        return 0;
                    }
            }
        }

        public static int getMode()
        {
            switch (CrypterAES.mode)
            {
                case CipherMode.CBC:
                    {
                        return 1;
                    }
                case CipherMode.CFB:
                    {
                        return 2;
                    }
                case CipherMode.CTS:
                    {
                        return 3;
                    }
                case CipherMode.ECB:
                    {
                        return 4;
                    }
                case CipherMode.OFB:
                    {
                        return 5;
                    }
                default:
                    {
                        return 0;
                    }
            }
        }

        public int getMod()
        {
            switch (this.AES.Mode)
            {
                case CipherMode.CBC:
                    {
                        return 1;
                    }
                case CipherMode.CFB:
                    {
                        return 2;
                    }
                case CipherMode.CTS:
                    {
                        return 3;
                    }
                case CipherMode.ECB:
                    {
                        return 4;
                    }
                case CipherMode.OFB:
                    {
                        return 5;
                    }
                default:
                    {
                        return 0;
                    }
            }
        }

        public byte[] getKey()
        {
            return AES.Key;
        }

        public byte[] getIV()
        {
            return AES.IV;
        }

        public byte[] CoderBytes(byte[] input)
        {
            //Проверка аргументов
            if (input == null || input.Length <= 0)
                return null;

            byte[] coderByte;

            //Создаем поток для шифрования
            using (var msEncrypt = new MemoryStream())
            {
                using (var csEncrypt = new CryptoStream(msEncrypt, AES.CreateEncryptor(AES.Key, AES.IV), CryptoStreamMode.Write))
                {
                    csEncrypt.Write(input, 0, input.Length);
                }
                coderByte = msEncrypt.ToArray();
            }

            return coderByte;
        }

        public byte[] DecoderBytes(byte[] inputBytes)
        {
            Byte[] outputBytes = inputBytes;

            byte[] decode = new byte[] { };

            using (MemoryStream memoryStream = new MemoryStream(outputBytes))
            {
                using (CryptoStream cryptoStream = new CryptoStream(memoryStream, AES.CreateDecryptor(AES.Key, AES.IV), CryptoStreamMode.Read))
                {
                    int len = 1024, count = len;
                    byte[] buffer = new byte[len];

                    while (count != 0)
                    {
                        count = cryptoStream.Read(buffer, 0, len);
                        decode = Tools.ConcatByte(decode, buffer, count);
                    }
                }
            }

            return decode;
        }


    }
}
