using System;
using System.IO;

namespace IBControll
{
    class LoginMenu
    {
        private static readonly string[] Menuk1 = { "Bejelentkezés", "Regisztráció", "Kilépés" };
        private static readonly string[] Menuk2 = { "Újrapróbálkozás", "Regisztráció", "Kilépés" };

        static SqlQuerys sql = new SqlQuerys();
        static Menu m = new Menu();
        static LoginSql l = new LoginSql();
        static Udmd u = new Udmd();
        static Program p = new Program();

        //LOGO kiiratása
        //------------------------
        public void PrintLogo()
        {
            string path = @"..\..\1.txt";

            var logo = new FileInfo(path);
            if (logo.Exists)
            {
                Console.SetWindowSize(100, 44);
                var reader = new StreamReader(path);
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    Console.WriteLine(line);
                }
                Console.WriteLine("\n\n");
            }
            else
            {
                Console.WriteLine("IB Control.");
            }            
        }
        //------------------------

        // menük közti váltás
        //------------------------
        public void ArraySelect(string chosen)
        {
            string[] selected = {"a","b","c"};
            int n;
            if (chosen=="login")
            {
                for (n = 0; n < Menuk1.Length; n++)
                {
                    selected[n]=Menuk1[n];
                }
            }
            if (chosen == "badlogin")
            {
                for (n = 0; n < Menuk2.Length; n++)
                {
                    selected[n] = Menuk2[n];
                }
            }
            DoMenu(selected,chosen);
        }
        //------------------------

        //Menü
        //------------------------
        public static void DoMenu(string[] selected,string chosen)
        {            
            int selectedItem = 0;

            if (chosen == "menu")
            {
                Console.Clear();
                sql.GetRootDirectory();
            }

            while (true)
            {
                //Menüpontok kiiratása
                //------------------------
                for (int i = 0; i < selected.Length; i++)
                {
                    if (selectedItem == i)
                    {
                        Console.BackgroundColor = ConsoleColor.Gray;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.WriteLine(selected[i]);
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine(selected[i]);
                    }                    
                }
                //------------------------

                //Navigálás
                //------------------------
                ConsoleKeyInfo cki = Console.ReadKey();

                switch (cki.Key)
                {
                    #region nyilak kezelése
                    case ConsoleKey.UpArrow:
                        if ((selectedItem - 1) < 0)
                        {
                            selectedItem = selected.Length - 1;
                        }
                        else
                        {
                            selectedItem--;
                        }

                    break;
                    case ConsoleKey.DownArrow:
                        if ((selectedItem + 1) > selected.Length - 1)
                        {
                            selectedItem = 0;
                        }
                        else
                        {
                            selectedItem++;
                        }
                    break;
                    #endregion

                    case ConsoleKey.Enter:

                        //Ha a választott menü a Bejelentkezés-Regisztráció-Kilépés
                        //------------------------
                        if (chosen == "login") 
                        {
                            if (selected[selectedItem] == selected[0])
                            {
                               Console.Clear();
                               l.DoLogin();
                            }
                            if (selected[selectedItem] == selected[1])
                            {
                                Console.Clear();
                                l.DoReg();
                            }
                            if (selected[selectedItem] == selected[2])
                            {
                                Environment.Exit(0);
                            }
                        }
                        //------------------------

                        //Ha a választott menü a Újrapróbálkozás-Regisztráció-Kilépés
                        //------------------------
                        if (chosen == "badlogin") 
                        {
                            if (selected[selectedItem] == selected[0])
                            {
                                Console.Clear();
                                l.DoLogin();
                            }
                            if (selected[selectedItem] == selected[1])
                            {
                                Console.Clear();
                                l.DoReg();
                            }
                            if (selected[selectedItem] == selected[2])
                            {
                                Environment.Exit(0);
                            }
                        }
                        //------------------------
                        break;                        
                }
                //Sorok törlése (majd ujra kiiratása) nyilgombok lenyomása után
                //------------------------
                Console.SetCursorPosition(0, Console.CursorTop - selected.Length);
                p.ClearConsole();
                //------------------------
                //------------------------
            }
        }
    }
}//namespace IBControll
