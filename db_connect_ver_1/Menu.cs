using System;
using System.Collections.Generic;
using System.Data;
using System.Threading;

namespace IBControll
{
    class Menu
    {        
        public Udmd u = new Udmd();
        public SqlQuerys sql = new SqlQuerys();
        public LoginSql l = new LoginSql();
        public Program p = new Program();

        private static bool inroot = true;

        //Módosítja az 'inroot' változó értékét
        //------------------------
        public void SetInRoot(bool tf)
        {
            if (tf == true)
            {
                inroot = true;
            }
            if (tf == false)
            {
                inroot = false;
            }             
        }
        //------------------------

        //Főmenü
        //------------------------
        public void DoMenu(List<string> selected)
        {
            int db = selected.Count;
            int selectedItem = 0;

            bool loggedin = true;
            bool changed_db = false;


            while (loggedin)
            {
                //Elemek kiiratása
                //------------------------
                for (int i = 0; i < selected.Count; i++)
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

                ConsoleKeyInfo cki = Console.ReadKey();

                //Navigáció
                //------------------------
                switch (cki.Key)
                {
                    //Nyílgombok kezelése
                    //------------------------
                    #region nyilak kezelése
                    case ConsoleKey.UpArrow:
                        if ((selectedItem - 1) < 0)
                        {
                            selectedItem = selected.Count - 1;
                        }
                        else
                        {
                            selectedItem--;
                        }

                        break;
                    case ConsoleKey.DownArrow:
                        if ((selectedItem + 1) > selected.Count - 1)
                        {
                            selectedItem = 0;
                        }
                        else
                        {
                            selectedItem++;
                        }
                        break;
                    #endregion
                    //------------------------

                    //Ha Entert ütünk a tartalomban
                    //------------------------
                    case ConsoleKey.Enter:
                                           
                        if (selected[selectedItem] != selected[selected.Count - 1] 
                            && selected[selectedItem] != selected[selected.Count - 2] 
                            && selected[selectedItem] != selected[selected.Count - 3] 
                            && selected[selectedItem] != selected[selected.Count - 4] 
                            && selected[selectedItem] != selected[selected.Count - 5])
                        {
                            string filename = (selected[selectedItem]);
                            if (sql.FileOrFolder(filename) == "File")
                            {
                                u.ModifyFile(filename);
                            }
                            else
                            {
                                inroot = false;
                                sql.OpenFolder(selected[selectedItem]);                          
                            }
                        }

                        //Ha Entert ütünk a menüpontokban
                        //------------------------
                        #region MenüPontok                            
                        //Új Mappa
                        //------------------------
                        if (selected[selectedItem] == selected[selected.Count - 4])
                        {
                            if (inroot)
                            {
                                sql.UjMappa();
                                sql.OpenFolder(sql.actchild());
                            }
                            else
                            {
                                if (sql.GetDoktároló(sql.actp(), sql.actpid()) == false)
                                {
                                    sql.UjMappa();
                                    sql.OpenFolder(sql.actchild());
                                }
                                else
                                {
                                    Console.WriteLine("Dokumentumtároló mappába nem hozhatsz létre almappát," +
                                                      " csak dokumentumot tölthetsz fel bele.");
                                    Thread.Sleep(2000);
                                    sql.OpenFolder(sql.actp());
                                    inroot = true;
                                }
                            }                                                   
                        }
                        //------------------------

                        //Feltöltés
                        //------------------------
                        if (selected[selectedItem] == selected[selected.Count - 3])
                            {
                                if (inroot==false)
                                {
                                    SetInRoot(true);
                                    u.Feltolt();                                
                                }
                                else
                                {
                                    Console.WriteLine("Gyökérkönyvtárba nem tölthetsz fel fájlokat!");
                                    Thread.Sleep(2000);
                                    //sql.OpenFolder(sql.actp());
                                    Clear(1);
                                    inroot = true;
                                }
                            }
                        //------------------------

                        //Vissza
                        //------------------------
                        if (selected[selectedItem] == selected[selected.Count - 2])
                        {
                            sql.OpenFolder(sql.actp());
                            inroot = true;
                        }
                        //------------------------

                        //Kijelentkezés - Kilépés
                        //------------------------
                        if (selected[selectedItem] == selected[selected.Count - 1])
                        {
                            Environment.Exit(0);
                        }
                        //------------------------
                        #endregion
                        //------------------------

                        break;
                    //------------------------

                    //Backspace-re visszalépés
                    //------------------------
                    case ConsoleKey.Backspace:
                        sql.OpenFolder(sql.actp());
                        inroot = true;
                    break;
                    //------------------------

                    //F2-re átnevezhetünk egy mappát
                    //------------------------
                    case ConsoleKey.F2:
                        if (Rename_Exit() == true)
                        {
                            sql.ChangeFolderName();
                        }
                        else
                        {
                            sql.OpenFolder(sql.actp());
                            SetInRoot(true);
                        }
                        
                    break;
                    //------------------------

                }
                //------------------------

                Clear(db);
            }
        }

        //Egy függvény amivel kiirathatunk egy DataTable minden értékét.
        //Teszteléshez használva
        //------------------------
        public void DebugTable(DataTable table)
        {
            Console.WriteLine("--- DebugTable(" + table.TableName + ") ---");
            int zeilen = table.Rows.Count;
            int spalten = table.Columns.Count;

            // Header
            for (int i = 0; i < table.Columns.Count; i++) //columns.count az a oszlopszám
            {
                string s = table.Columns[i].ToString();
                Console.Write($"{s,-20} | ");
            }
            Console.Write(Environment.NewLine);
            for (int i = 0; i < table.Columns.Count; i++)
            {
                Console.Write("---------------------|-");
            }
            Console.Write(Environment.NewLine);

            // Data
            for (int i = 0; i < zeilen; i++) //zeilen = kiiratott sorok db.
            {
                DataRow row = table.Rows[i];
                //Console.WriteLine("{0} {1} ", row[0], row[1]);
                for (int j = 0; j < spalten; j++) //spalten = kiiratott oszlopok db
                {
                    string s = row[j].ToString();
                    if (s.Length > 30) s = s.Substring(0, 30) + "...";
                    Console.Write($"{s,-20} | ");
                }
                Console.Write(Environment.NewLine);
            }
            for (int i = 0; i < table.Columns.Count; i++)
            {
                Console.Write("---------------------|-");
            }

            Console.Write(Environment.NewLine);
            DataRow row2 = table.Rows[0];
            Console.WriteLine("\n\n\n\n\n\n");
            Console.WriteLine(row2.ToString());
        }
        //------------------------

        //Több sor törlése a Console-ról
        //------------------------
        public void Clear(int db)
        {
            for (int i = 0; i < db; i++)
            {
                Console.SetCursorPosition(0, Console.CursorTop - 1);
                p.ClearConsole();
            }
        }
        //------------------------

        //Igen-Nem Menü
        //------------------------
        public bool Yesnomenu()
        {
            string[] tomb = { "Igen", "Nem", "Vissza"};
            int db = tomb.Length;
            int selectedItem = 0;

            while (true)
            {
                //Elemek kiiratása
                //------------------------
                for (int i = 0; i < db; i++)
                {
                    if (selectedItem == i)
                    {
                        Console.BackgroundColor = ConsoleColor.Gray;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.WriteLine(tomb[i]);
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine(tomb[i]);
                    }
                }
                //------------------------

                ConsoleKeyInfo cki = Console.ReadKey();

                //Gombok kezelése
                //------------------------
                switch (cki.Key)
                {
                    //Nyílgombok
                    //------------------------
                    #region nyilak kezelése
                    case ConsoleKey.UpArrow:
                        if ((selectedItem - 1) < 0)
                        {
                            selectedItem = db - 1;
                        }
                        else
                        {
                            selectedItem--;
                        }

                        break;
                    case ConsoleKey.DownArrow:
                        if ((selectedItem + 1) > db - 1)
                        {
                            selectedItem = 0;
                        }
                        else
                        {
                            selectedItem++;
                        }
                        break;
                    #endregion
                    //------------------------

                    //Menüpontokra Enter
                    //------------------------
                    case ConsoleKey.Enter:
                        if (tomb[selectedItem] == tomb[0])
                        {
                            return true;
                        }
                        if (tomb[selectedItem] == tomb[1])
                        {
                            return false;
                        }
                        if (tomb[selectedItem] == tomb[2])
                        {
                            sql.OpenFolder(sql.actp());
                            inroot = true;
                        }
                        break;
                    //------------------------
                }
                //------------------------

                //Console sorok törlése
                //------------------------
                Console.SetCursorPosition(0, Console.CursorTop - 3);
                p.ClearConsole();
                //------------------------
            }
        }

        //Módosítás-Letöltés Menü
        //------------------------
        public bool Modify_Download_Menu(string filename)
        {
            string[] tomb = { "Letöltés", "Módosítás", "Vissza" };
            int db = tomb.Length;
            int selectedItem = 0;

            while (true)
            {
                //Elemek kiiratása
                //------------------------
                for (int i = 0; i < db; i++)
                {
                    if (selectedItem == i)
                    {
                        Console.BackgroundColor = ConsoleColor.Gray;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.WriteLine(tomb[i]);
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine(tomb[i]);
                    }
                }
                //------------------------
                ConsoleKeyInfo cki = Console.ReadKey();

                //Gombok kezelése
                //------------------------
                switch (cki.Key)
                {
                    //Nyílgombok
                    //------------------------
                    #region nyilak kezelése
                    case ConsoleKey.UpArrow:
                        if ((selectedItem - 1) < 0)
                        {
                            selectedItem = db - 1;
                        }
                        else
                        {
                            selectedItem--;
                        }

                        break;
                    case ConsoleKey.DownArrow:
                        if ((selectedItem + 1) > db - 1)
                        {
                            selectedItem = 0;
                        }
                        else
                        {
                            selectedItem++;
                        }
                        break;
                    #endregion
                    //------------------------

                    //Menüpontokra Enter
                    //------------------------
                    case ConsoleKey.Enter:
                        if (tomb[selectedItem] == tomb[0])
                        {
                            return false;
                        }
                        if (tomb[selectedItem] == tomb[1])
                        {
                            return true;
                        }
                        if (tomb[selectedItem] == tomb[2])
                        {
                            sql.OpenFolder(sql.actp());
                            inroot = true;
                        }
                        break;
                    //------------------------
                }
                //------------------------

                //Sorok törlése
                //------------------------
                Console.SetCursorPosition(0, Console.CursorTop - 3);
                p.ClearConsole();
                //------------------------
            }
        }
        //------------------------

        //Átnevezés Menü
        //------------------------
        public bool Rename_Exit()
        {
            string[] tomb = {"Átnevezés", "Vissza"};
            int db = tomb.Length;
            int selectedItem = 0;

            while (true)
            {
                //Elemek kiiratása
                //------------------------
                for (int i = 0; i < db; i++)
                {
                    if (selectedItem == i)
                    {
                        Console.BackgroundColor = ConsoleColor.Gray;
                        Console.ForegroundColor = ConsoleColor.Black;
                        Console.WriteLine(tomb[i]);
                        Console.ResetColor();
                    }
                    else
                    {
                        Console.WriteLine(tomb[i]);
                    }
                }

                ConsoleKeyInfo cki = Console.ReadKey();


                //Gombok kezelése
                //------------------------
                switch (cki.Key)
                {
                    //Nyílgombok
                    //------------------------
                    #region nyilak kezelése
                    case ConsoleKey.UpArrow:
                        if ((selectedItem - 1) < 0)
                        {
                            selectedItem = db - 1;
                        }
                        else
                        {
                            selectedItem--;
                        }

                        break;
                    case ConsoleKey.DownArrow:
                        if ((selectedItem + 1) > db - 1)
                        {
                            selectedItem = 0;
                        }
                        else
                        {
                            selectedItem++;
                        }
                        break;
                    #endregion
                    //------------------------

                    //Menüpontokra Enter
                    //------------------------
                    case ConsoleKey.Enter:
                        if (tomb[selectedItem] == tomb[0])
                        {
                            return true;
                        }
                        if (tomb[selectedItem] == tomb[1])
                        {
                            return false;
                        }
                        if (tomb[selectedItem] == tomb[2])
                        {
                            sql.OpenFolder(sql.actp());
                            SetInRoot(true);
                        }
                        break;
                    //------------------------
                }
                //------------------------

                //Sorok törlése
                //------------------------
                Console.SetCursorPosition(0, Console.CursorTop - 2);
                p.ClearConsole();
                //------------------------
            }
        }
        //------------------------

    }
}//namespace IBControll
