using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Configuration;

namespace AppConexion
{
    class Program
    {
        static void Main(string[] args)
        {
            string usernameKey = Encrypt("Usernadwadwame"); 
            Console.WriteLine(usernameKey);
            string passwordKey = Encrypt("Password");
            Console.WriteLine(passwordKey);
            string connectionString = Encrypt("Data Source=BQV-LAB-SALA807\\MSSQLSERVER2; Database=db_integracion; User Id=sa; Password=123456789;");
            string sCadenaConexion = connectionString;
            Console.WriteLine(sCadenaConexion);

            string desUser = Decrypt(usernameKey);
            Console.WriteLine(desUser);
            string desPassword = Decrypt(passwordKey);
            Console.WriteLine(desPassword);
            string desCadenaConexion = Decrypt(sCadenaConexion);
            Console.WriteLine( desCadenaConexion);


            using (SqlConnection conn = new SqlConnection(sCadenaConexion))
            {
                SqlDataAdapter da = new SqlDataAdapter("SELECT * FROM tb_personal", conn);
                DataSet ds = new DataSet();
                da.Fill(ds);

                DataTable resultadoBD = ds.Tables[0];
                string PathTXT = "C:\\txtRepositorio\\bdProductos.txt";

                using (StreamWriter sw = new StreamWriter(PathTXT, false))
                {
                    for (int i = 0; i < resultadoBD.Columns.Count; i++)
                    {
                        sw.Write(resultadoBD.Columns[i].ColumnName + ";");
                    }
                    sw.WriteLine();

                    foreach (DataRow row in resultadoBD.Rows)
                    {
                        object[] array = row.ItemArray;
                        for (int i = 0; i < array.Length - 1; i++)
                        {
                            sw.Write(array[i].ToString() + "; ");
                        }
                        sw.Write(array[array.Length - 1].ToString()); // Avoid trailing semicolon
                        sw.WriteLine();
                    }
                }
            }
        }
        static string GetAppSetting(string key)
        {
            string value = ConfigurationManager.AppSettings[key];
            if (string.IsNullOrEmpty(value))
            {
                throw new Exception($"AppSetting '{key}' is missing or empty.");
            }
            return Encrypt(value);
        }
        static string Encrypt(string clearText)
        {
            string encryptionKey = "INOVAAYASA2022";
            byte[] clearBytes = System.Text.Encoding.Unicode.GetBytes(clearText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(encryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(clearBytes, 0, clearBytes.Length);
                        cs.Close();
                    }
                    clearText = Convert.ToBase64String(ms.ToArray());
                }
            }
            return clearText;
        }

        static string Decrypt(string cipherText)
        {
            string encryptionKey = "INOVAAYASA2022";
            byte[] cipherBytes = Convert.FromBase64String(cipherText);
            using (Aes encryptor = Aes.Create())
            {
                Rfc2898DeriveBytes pdb = new Rfc2898DeriveBytes(encryptionKey, new byte[] { 0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76 });
                encryptor.Key = pdb.GetBytes(32);
                encryptor.IV = pdb.GetBytes(16);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(cipherBytes, 0, cipherBytes.Length);
                        cs.Close();
                    }
                    cipherText = Encoding.Unicode.GetString(ms.ToArray());
                }
            }
            return cipherText;
        }
    }
}