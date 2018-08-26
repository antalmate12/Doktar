using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Mail;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace IBControll
{

    class LoginSql
    {
        //static Program p = new Program();
        static string checklogin = "SELECT count(*) FROM Login WHERE username=@username AND Password=@pass;";
        private static string select;
        string name,email,username,pass;
        
        public DataTable datatable = new DataTable();

        static Menu menu = new Menu();
        static Program p = new Program();
        static LoginMenu m = new LoginMenu();
        private string loggedinusername;


        #region Checkers

        //Megnézi, hogy a felhasználónév foglalt-e
        //------------------------
        public bool IsUsernameAlreadyTaken(string username)
        {
            SqlDataAdapter sda = new SqlDataAdapter("SELECT count(*) FROM Login WHERE username='" + username + "'", p.Con);
            sda.Fill(datatable);
            if (datatable.Rows[0][0].ToString() == "1")
            {
                datatable.Clear();
                return true;
            }
            else
            {
                datatable.Clear();
                return false;
            }           
        }
        //------------------------

        //Megnézi, hogy az email cím valós-e
        //------------------------
        public bool IsValidEmail(string email) 
        {
            try
            {
                var addr = new MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        //------------------------
        #endregion

        #region login

        //Bejelentkezés elkezdése
        //------------------------
        public void StartLogin()
        {
            try
            {
                //Csatlakozás az adatbázishoz
                p.Con.Open();
            }
            catch
            {
                //Ha nem tud csatlakozni
                Console.WriteLine("Nem sikerült csatlakozni az adatbázishoz :'(");
                Thread.Sleep(2000);
                Environment.Exit(0);                
            }
            select = "login";
            m.ArraySelect(select);
        }


        //Bejelentkezés
        //------------------------
        public void DoLogin()
        {
            select = null;
            username = null;
            pass = null;
            datatable.Clear();
            Console.WriteLine("Bejelentkezés...\n");

            //Felhasználónév
            //------------------------
            Console.WriteLine("Felhasználónév:");
            while (string.IsNullOrEmpty(username) || string.IsNullOrWhiteSpace(username))
            {
                username = Console.ReadLine();
                if (string.IsNullOrEmpty(username) || string.IsNullOrWhiteSpace(username))
                {
                    Console.WriteLine("Ez a mező nem lehet üres. \nFelhasználónév:");
                }
            }
            //------------------------

            //Jelszó
            //------------------------
            Console.WriteLine("Jelszó:");
            while (string.IsNullOrEmpty(pass) || string.IsNullOrWhiteSpace(pass))
            {
                pass = Orb.App.Console.ReadPassword();
          
                if (string.IsNullOrEmpty(pass) || string.IsNullOrWhiteSpace(pass))
                {
                    Console.WriteLine("Ez a mező nem lehet üres. \nJelszó:");
                }
            }
            //------------------------

            //Bejelentkezés SQl
            //------------------------
            loggedinusername = username;
            p.Setlgduser(loggedinusername);
            var sda = new SqlDataAdapter(checklogin, p.Con);
            //SQL Injection Protection
            sda.SelectCommand.Parameters.AddWithValue("@username", SqlDbType.VarChar);
            sda.SelectCommand.Parameters["@username"].Value = username;
            //SQL Injection Protection
            sda.SelectCommand.Parameters.AddWithValue("@pass", SqlDbType.VarChar);
            sda.SelectCommand.Parameters["@pass"].Value = pass;
            //------------------------
            sda.Fill(datatable);
                //Ha sikerült a bejelentkezés
            if (datatable.Rows[0][0].ToString() == "1")
            {
                p.Con.Close();
                select = "menu";
                //Főmenü
                m.ArraySelect(select);      
            }
                //Ha nem sikerült
            else
            {
                select = "badlogin";
                //Újrapróbálkozós-Regisztráció menü
                m.ArraySelect(select);
            }
            //------------------------
        }
        //------------------------

        //Regisztráció
        //------------------------
        public void DoReg()
        {
            select = null;
            username = null;
            pass = null;
            datatable.Clear();
            Console.WriteLine("Regisztráció...");

            //Mindent ellenőrzötten kérünk be. Nem lehet null vagy whitespace.
            //Nem lehet már használatban lévő felhasználónév

            //Felhasználónév megadása
            //------------------------
            Console.WriteLine("Adjon meg egy felhasználónevet:");
            while (string.IsNullOrEmpty(username) || IsUsernameAlreadyTaken(username) || string.IsNullOrWhiteSpace(username))
            {
                username = Console.ReadLine();
                if (string.IsNullOrEmpty(username) || string.IsNullOrWhiteSpace(username))
                {
                    Console.WriteLine("Ez a mező nem lehet üres. \nAdjon meg egy felhasználónevet:");
                }
                if (IsUsernameAlreadyTaken(username) && !string.IsNullOrEmpty(username))
                {
                    Console.WriteLine("Ez a név már foglalt. \nAdjon meg egy másik felhasználónevet:");
                }
            }
            //------------------------

            //Vezetéknév megadása
            //------------------------
            string vnev = null, knev = null;
            Console.WriteLine("Adja meg a vezetéknevét:");
            while (string.IsNullOrEmpty(vnev) || string.IsNullOrWhiteSpace(vnev))
            {
                vnev = Console.ReadLine();
                if (string.IsNullOrEmpty(vnev) || string.IsNullOrWhiteSpace(vnev))
                {
                    Console.WriteLine("Ez a mező nem lehet üres. \nAdja meg a vezetéknevét:");
                }
            }
            //------------------------

            //Keresztnév megadása
            //------------------------
            Console.WriteLine("Adja meg a keresztnevét:");
            while (string.IsNullOrEmpty(knev) || string.IsNullOrWhiteSpace(knev))
            {
                knev = Console.ReadLine();
                if (string.IsNullOrEmpty(knev) || string.IsNullOrWhiteSpace(knev))
                {
                    Console.WriteLine("Ez a mező nem lehet üres. \nAdja meg a keresztnevét:");
                }
            }
            //------------------------

            //Email megadás
            //------------------------
            Console.WriteLine("Adja meg az email címét:");
            while (string.IsNullOrEmpty(email) || IsValidEmail(email) == false)
            {
                email = Console.ReadLine();

                if (string.IsNullOrEmpty(email))
                {
                    Console.WriteLine("Ez a mező nem lehet üres. \nAdja meg az email címét:");
                }
                //Plusz ellenőrzés. Valós formátumú email cím-e
                if (IsValidEmail(email) == false && !string.IsNullOrEmpty(email))
                {
                    Console.WriteLine("Hibás formátumú email cím. \nAdja meg az email címét:");
                }
            }
            //------------------------

            //Jelszó megadása
            //------------------------
            Console.WriteLine("Adjon meg egy jelszót:");
            while (string.IsNullOrEmpty(pass) || string.IsNullOrWhiteSpace(pass))
            {
                pass = Orb.App.Console.ReadPassword();
                if (string.IsNullOrEmpty(pass) || string.IsNullOrWhiteSpace(pass))
                {
                    Console.WriteLine("Ez a mező nem lehet üres. \nAdjon meg egy jelszót:");
                }
            }
            //------------------------


            //TELJES NÉV FORMÁZÁSA
            //------------------------
            //mindenből kisbetű
            vnev = vnev.ToLower();
            knev = knev.ToLower();
            //első betűkből nagybetű
            var sb1 = new StringBuilder(vnev);
            var sb2 = new StringBuilder(knev);
            sb1[0] = char.ToUpper(vnev[0]);
            sb2[0] = char.ToUpper(knev[0]);
            vnev = sb1.ToString();
            knev = sb2.ToString();
            name = vnev + " " + knev;
            //------------------------

            //Regisztráció SQL kód
            //------------------------
            #region register sql
            var reg = new SqlCommand("INSERT INTO Login (username,Name,Email,Password) VALUES (@username,@name,@email,@pass)", p.Con);
            //SQL Injection Protection
            reg.Parameters.Add("@username", SqlDbType.VarChar);
            reg.Parameters["@username"].Value = username;
            //SQL Injection Protection
            reg.Parameters.Add("@name", SqlDbType.VarChar);
            reg.Parameters["@name"].Value = name;
            //SQL Injection Protection
            reg.Parameters.Add("@email", SqlDbType.VarChar);
            reg.Parameters["@email"].Value = email;
            //SQL Injection Protection
            reg.Parameters.Add("@pass", SqlDbType.VarChar);
            reg.Parameters["@pass"].Value = pass;
            //--
            reg.ExecuteNonQuery();
            Console.WriteLine("Sikeres Regisztráció!");

            #endregion
            //------------------------

            username = null;
            name = null;
            pass = null;
            email = null;

            Thread.Sleep(1000);
            Console.Clear();
            //Bejelentkezés a sikeres regisztráció után
            DoLogin();
        }

        #endregion

    }
}//namespace IBControll

//A jelszóért felelős metódusok
//------------------------
#region namespace for password 

namespace Orb.App
{
    public static class Console
    {
        public static string ReadPassword(char mask)
        {
            const int ENTER = 13, BACKSP = 8, CTRLBACKSP = 127;
            int[] filtered = { 0, 27, 9, 10 /*, 32 space, if you care */ }; 


            SecureString securePass = new SecureString();

            char chr = (char)0;

            while ((chr = System.Console.ReadKey(true).KeyChar) != ENTER)
            {
                if (((chr == BACKSP) || (chr == CTRLBACKSP))
                    && (securePass.Length > 0))
                {
                    System.Console.Write("\b \b");
                    securePass.RemoveAt(securePass.Length - 1);

                }
                // Don't append * when length is 0 and backspace is selected
                else if (((chr == BACKSP) || (chr == CTRLBACKSP)) && (securePass.Length == 0))
                {
                }

                // Don't append when a filtered char is detected
                else if (filtered.Count(x => chr == x) > 0)
                {
                }

                // Append and write * mask
                else
                {
                    securePass.AppendChar(chr);
                    System.Console.Write(mask);
                }
            }

            System.Console.WriteLine();
            IntPtr ptr = new IntPtr();
            ptr = Marshal.SecureStringToBSTR(securePass);
            string plainPass = Marshal.PtrToStringBSTR(ptr);
            Marshal.ZeroFreeBSTR(ptr);
            return plainPass;
        }

        public static string ReadPassword()
        {
            return ReadPassword('*');
        }
    }
}//namespace Orb.App
#endregion
//------------------------
