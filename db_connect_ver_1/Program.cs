using System;
using System.IO;
using System.Collections.Generic;
using System.Data.SqlClient;


namespace IBControll
{
    class Program
    {
        //Listák és változók a mappák közti navigáláshoz
        //------------------------
        public List<string> FolderList = new List<string>();
        public  List<int> IdList = new List<int>();

        public  int  actualparentid;
        public  string actualparent;
        public int actualchildid;
        public string actualchild;
        //------------------------

        //Adatbázishoz csatlakozás
        //------------------------
        private static readonly string Fullpath = Path.GetFullPath(@"../../login.mdf");
        public SqlConnection Con = new SqlConnection($@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=
                                                        {Fullpath};Integrated Security=True");
        //------------------------

        static SqlQuerys sql = new SqlQuerys();
        static LoginSql l = new LoginSql();
        static LoginMenu m = new LoginMenu();
        static Udmd u = new Udmd();
        private static string lgdinuser;          

        private static void Main(string[] args)
        {            
            //Szerver mappa
            u.CreateDirectory();
            //Logó kiiratás
            m.PrintLogo();
            //Bejelentkezés
            l.StartLogin();
			Console.ReadLine();
        }

        //Sorok törlése a Console-ról
        //------------------------
        public void ClearConsole()
        {
            var clc = Console.CursorTop;
            Console.SetCursorPosition(0,Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0,clc);
        }
        //------------------------

        //Változók értékeit változtató illetve visszaadó függvények
        //------------------------
        public void Setlgduser(string nev)
        {            
            lgdinuser = nev;
        }
        public string Getlgduser()
        {
            return lgdinuser;
        }
        public int GetActPid()
        {
            return actualparentid;
        }
        public string GetActParent()
        {
            return actualparent;
        }
        public int GetActChildId()
        {
            return actualchildid;
        }
        public string GetActChild()
        {
            return actualchild;
        }
        //------------------------

    }
}//namespace IBControll



