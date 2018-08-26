using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading;
using static IBControll.KnownFolders;

namespace IBControll
{
    internal class Udmd
    {
        static Aes a = new Aes();
        static SqlQuerys sql = new SqlQuerys();
        static Program p = new Program();
        static LoginSql l = new LoginSql();
        static Menu m = new Menu();

        public string NewFileName;
        public long FileSize;

        //Ha nincs szerver mappa akkor létrehoz egyet
        //------------------------
        public void CreateDirectory()
        {
            var server = GetPath(KnownFolder.Desktop) + "\\szerver";
            var d = new DirectoryInfo(server);
            if (d.Exists==false)
            {
                d.Create();
            }
        }
        //------------------------

        //Feltöltés
        //------------------------
        public void Feltolt()
        {
            //Csak ha Dokumentumtároló mappában vagyunk
            if (sql.GetDoktároló(sql.actp(), sql.actpid()))
            {
                try
                {
                    //Feltölteni kívánt fájl bekérése
                    //------------------------
                    Console.WriteLine("\nAdd meg a feltölteni kívánt fájl elérési útját, vagy csak dobd ide!");
                    string oldPath = null;
                    while (string.IsNullOrEmpty(oldPath) || string.IsNullOrWhiteSpace(oldPath))
                    {
                        oldPath = Console.ReadLine();
                        if (string.IsNullOrEmpty(oldPath) || string.IsNullOrWhiteSpace(oldPath))
                        {
                            Console.WriteLine("Ez a mező nem lehet üres. \n" +
                                              "Add meg a feltölteni kívánt fájl elérési útját, vagy csak dobd ide!");
                        }
                    }
                    //------------------------

                    var f1 = new FileInfo(oldPath);
                    FileSize = new FileInfo(oldPath).Length;
                    var filename = f1.Name;
                    string newpath = GetPath(KnownFolder.Desktop) + @"\szerver\";

                    //Ha létezik feltöltjük
                    //------------------------
                    if (f1.Exists)
                        try
                        {
                            //Titkosítás és Feltöltés
                            a.EncryptFile(oldPath, sql.GetUsersPassword(), newpath);
                            NewFileName = a.EncryptText(filename, sql.GetUsersPassword());
                            //Fájl adatainak eltárolása adatbázisban
                            sql.FileInfoToDb(filename, NewFileName, FileSize);
                            Console.WriteLine("\nFájl feltöltve!");
                            //Eredeti fájl kitörlése
                            f1.Delete();
                            //Visszalépés
                            Thread.Sleep(2000);
                            sql.OpenFolder(sql.actp());
                            m.SetInRoot(true);
                        }
                        catch (Exception ex)
                        {
                            //Hibaüzenet kiírása
                            Console.WriteLine(ex.Message);
                            //Visszalépés
                            Thread.Sleep(2000);
                            sql.OpenFolder(sql.actp());
                            m.SetInRoot(true);
                        }
                }
                catch (Exception e)
                {
                    //Hibaüzenet kiírása
                    Console.WriteLine(e.Message);
                    Thread.Sleep(2000);
                    Console.Clear();
                    //Újrapróbálkozás
                    Feltolt();
                }
            }
            else
            {
                //Hibaüzenet kiírása
                Console.WriteLine("Csak dokumentumtárolóba tölthetsz fel fájlokat!");
                //Visszalépés
                Thread.Sleep(2000);
                m.SetInRoot(true);
                sql.OpenFolder(sql.actp());
            }
        }
        //------------------------

        //Letöltés
        //------------------------
        public void Letolt(string fajlnev)
        {
            //Letöltési mappa
            var downloadsPath = GetPath(KnownFolder.Downloads) + "\\";
            //Új fájlnév meghatározása (eredeti titkosítása)
            var newfilename = a.EncryptText(fajlnev, sql.GetUsersPassword());
            //Karaktercsere / --> _ (/ nem engedélyezett a fájlnevekben)
            newfilename = newfilename.Replace("/", "_");
            var fajlut = GetPath(KnownFolder.Desktop)+@"\szerver\"+newfilename;
            var f1 = new FileInfo(fajlut);

            //Ha a fájl létezik a szerver mappában letöltjük
            //------------------------
            if (f1.Exists)
                try
                {
                    //Fájl dekódolása, letöltése
                    a.DecryptFile(fajlut, sql.GetUsersPassword(), downloadsPath);
                    Console.WriteLine("Letöltöttem!");
                    Thread.Sleep(2000);
                    //Visszalépés
                    sql.OpenFolder(sql.actp());
                    m.SetInRoot(true);
                }
                catch (Exception e)
                {
                    //Hibaüzenet kiírása
                    Console.WriteLine(e.Message);
                }
            //------------------------
            Console.ReadKey();
        }
        //------------------------

        //Fájl Módosítása
        //------------------------
        public void ModifyFile(string fajlnev)
        {
            //Ha a letöltést választottuk
            //------------------------
            if (m.Modify_Download_Menu(fajlnev) == false)
            {
                Letolt(fajlnev);
            }
            //------------------------

            //Ha a módosítást
            //------------------------
            else
            {
                //Kitöröljük az eredeti fájlt, és egy újat töltünk fel helyette
                p.Con.Open();
                //EREDETI FÁJL TÖRLÉSE
                //------------------------
                var dt = new DataTable();
                string query =
                    "SELECT EncryptedName FROM Documents WHERE username = @username AND FileName = @filename";
                var sda = new SqlDataAdapter(query, p.Con);
                //--
                sda.SelectCommand.Parameters.AddWithValue("@username", SqlDbType.VarChar);
                sda.SelectCommand.Parameters["@username"].Value = p.Getlgduser();
                //--
                sda.SelectCommand.Parameters.AddWithValue("@filename", SqlDbType.VarChar);
                sda.SelectCommand.Parameters["@filename"].Value = fajlnev;
                sda.Fill(dt);
                //--
                string encfname = dt.Rows[0][0].ToString();
                encfname = encfname.Replace("/", "_");
                var f1 = new FileInfo(GetPath(KnownFolder.Desktop) + @"\szerver\" + encfname);
                f1.Delete();
                dt.Clear();
                //------------------------

                //TÖRLÉS ADATBÁZISBÓL
                //------------------------
                string del =
                    "DELETE FROM Documents WHERE username = @username AND FileName = @filename";
                var delete = new SqlCommand(del, p.Con);
                //SQL Injection Protection
                delete.Parameters.Add("@username", SqlDbType.VarChar);
                delete.Parameters["@username"].Value = p.Getlgduser();
                //SQL Injection Protection
                delete.Parameters.Add("@filename", SqlDbType.VarChar);
                delete.Parameters["@filename"].Value = fajlnev;
                delete.ExecuteNonQuery();
                //------------------------

                //ÚJ FELTÖLTÉSE
                //------------------------
                Feltolt();
                //------------------------   
            }
            //------------------------
        }
        //------------------------
    }
}//namespace IBControll