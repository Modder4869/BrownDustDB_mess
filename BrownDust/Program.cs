using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Community.CsharpSqlite;
using static Community.CsharpSqlite.Sqlite3;

namespace tool
{
    class Program
    {
        static string GetSHA1ToBuff(byte[] pBuff)
        {
            using (SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider())
            {
                byte[] hashBytes = sha1.ComputeHash(pBuff);
                string hashString = BitConverter.ToString(hashBytes).Replace("-", string.Empty);
                return hashString;
            }
        }
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: browndust <database path> <output path> <key>");
                return;
            }

            string dbPath = args[0];
            string filePath = args[1];
            string key = args[2];

            //byte[] buffer = Encoding.UTF8.GetBytes("spdhdnlwmrpavmtm");
            byte[] buffer = Encoding.UTF8.GetBytes(key);
            string sha1Hash = GetSHA1ToBuff(buffer);
            Console.WriteLine($"SHA-1 Hash: {sha1Hash}");

            Sqlite3.sqlite3 db;
            int rc = Sqlite3.sqlite3_open(dbPath, out db);
            if (rc != SQLITE_OK)
            {
                Console.WriteLine($"Failed to open database: {sqlite3_errmsg(db)}");
                return;
            }

            Sqlite3.sqlite3_key(db, sha1Hash, sha1Hash.Length);
            try
            {
                Sqlite3.sqlite3_rekey(db, "", 0);
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error during rekey: {e.Message}");
            }

            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                foreach (var data in Sqlite3.dataList.Skip(1))
                {
                    fs.Write(data, 0, data.Length);
                }
            }

            Console.WriteLine($"Binary data written to file: {filePath}");
        }
    }
}
