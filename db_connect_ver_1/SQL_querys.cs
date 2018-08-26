using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading;

namespace IBControll
{
    class SqlQuerys
    {       
        private static int sorszam; 

        static Menu m = new Menu();
        static Program p = new Program();

        public string GetUsersFullName()
        {
            string query = "SELECT Name FROM Login WHERE username=@UserName;";
            SqlCommand command = new SqlCommand(query, p.Con);
            command.Parameters.Add("@UserName", SqlDbType.VarChar);
            command.Parameters["@UserName"].Value = p.Getlgduser();
            try
            {
                string fullname = command.ExecuteScalar().ToString();
                p.Con.Close();
                return fullname;
            }
            catch (Exception ex)
            {
                p.Con.Close();
                return (ex.Message);
            }
        }

        public string GetUsersPassword()
        {
            p.Con.Open();
            string query = "SELECT Jelszo FROM Login WHERE username=@UserName;";
            SqlCommand command = new SqlCommand(query, p.Con);
            command.Parameters.Add("@UserName", SqlDbType.VarChar);
            command.Parameters["@UserName"].Value = p.Getlgduser();
            try
            {
                string pass = command.ExecuteScalar().ToString();
                p.Con.Close();
                return pass;
            }
            catch (Exception ex)
            {
                p.Con.Close();
                return (ex.Message);
            }
        }

        public string FileOrFolder(string name)
        {
            var dt = new DataTable();
            string query = "SELECT FileName FROM Documents WHERE FileName=@FileName;";
            var sda = new SqlDataAdapter(query,p.Con);
            sda.SelectCommand.Parameters.AddWithValue("@FileName", SqlDbType.VarChar);
            sda.SelectCommand.Parameters["@FileName"].Value = name;
            sda.Fill(dt);
           
            if (dt.Rows.Count == 0)
            {
                return "Folder";
            }
            return "File";
        }

        public void UjMappa()
        {
            p.Con.Open();
            //Mappanév bekérése
            //------------------------
            bool foglalt = false;
            Console.WriteLine("\nMi legyen a mappa neve?");
            string folderName = null;

            //Mappanév ellenőrzött bekérése
            //------------------------
            while (true)
            {
                folderName = Console.ReadLine();

                // if folder name is null or empty or whitespace, ask for a new folder name
                if (string.IsNullOrEmpty(folderName) || string.IsNullOrWhiteSpace(folderName))
                {
                    Console.WriteLine("Ez a mező nem lehet üres. \nÚj mappa neve:");
                    //(Can't be null. \nNew foldername:)
                }
                //if folder name already exists, ask for a new one
                else if (p.FolderList.Contains(folderName))
                {
                    Console.WriteLine("Ez a mappanév egyszer már szerepel ebben a környezetben. Kérlek válassz újat!\nÚj mappa neve:");
                    //(That name is already taken. \nNew foldername:)
                }
                else //Folder Name is valid
                    break; //proceed to do stuff with the folder name
            }
            //------------------------

            //Dokumentumtároló-e?
            //------------------------
            Console.WriteLine("Dokumentumtároló legyen-e?");
            bool doktár;
            if (m.Yesnomenu() == true)
            {
                doktár = true;
            }
            else { doktár = false; }
            //------------------------

            using (var newfolder = new SqlCommand(
                "INSERT INTO Folders (username,FolderName,Parent_id,Doc_Container) " +
                "VALUES " + "(@username,@FolderName,@Parent_id,@Doc_Container)", p.Con))
            {
                //SQL Injection Protection
                newfolder.Parameters.Add("@username", SqlDbType.VarChar);
                newfolder.Parameters["@username"].Value = p.Getlgduser();
                //SQL Injection Protection
                newfolder.Parameters.Add("@FolderName", SqlDbType.VarChar);
                newfolder.Parameters["@FolderName"].Value = folderName;
                //SQL Injection Protection
                newfolder.Parameters.Add("@Parent_id", SqlDbType.Int);
                newfolder.Parameters["@Parent_id"].Value = p.actualparentid;
                //SQL Injection Protection
                newfolder.Parameters.Add("@Doc_Container", SqlDbType.Bit);
                newfolder.Parameters["@Doc_Container"].Value = doktár;
                newfolder.ExecuteNonQuery();
            }
            p.Con.Close();
        }

        public void FileInfoToDb(string OldFileName, string NewFileName, long FileSize)
        {
            p.Con.Open();
            using (var todb =
                new SqlCommand(
                    "INSERT INTO Documents (Folder_id,username,FileName,UploadDate,EncryptedName,FileSize) VALUES " +
                    "(@Folder_id,@username,@FileName,@UploadDate,@EncryptedName,@FileSize)", p.Con))
            {
                todb.Parameters.Add("@Folder_id", SqlDbType.Int);
                todb.Parameters["@Folder_id"].Value = p.actualparentid;                
                //SQL Injection Protection
                todb.Parameters.Add("@username", SqlDbType.VarChar);
                todb.Parameters["@username"].Value = p.Getlgduser();
                //SQL Injection Protection
                todb.Parameters.Add("@FileName", SqlDbType.VarChar);
                todb.Parameters["@FileName"].Value = OldFileName;
                //SQL Injection Protection
                todb.Parameters.Add("@UploadDate", SqlDbType.VarChar);
                todb.Parameters["@UploadDate"].Value = DateTime.Now;
                //SQL Injection Protection
                todb.Parameters.Add("@EncryptedName", SqlDbType.VarChar);
                todb.Parameters["@EncryptedName"].Value = NewFileName;
                //SQL Injection Protection
                todb.Parameters.Add("@FileSize", SqlDbType.Int);
                todb.Parameters["@FileSize"].Value = FileSize;
                todb.ExecuteNonQuery();
            }
            p.Con.Close();
        }     


        #region openfolder
        public void OpenFolder(string mappanev)
        {            
            //Megnézzük, hogy a Mappa nevének mi az id-ja.
            int actualid = 0;
            actualid = KiaBarátom(mappanev);
            //------------------------
            p.actualparent = mappanev;
            p.actualparentid = actualid;
            //------------------------
            p.FolderList.Clear();
            int sorszam = 0;
            var dt = new DataTable();
            //------------------------

            SearchForFolder(actualid);
            SearchForFile(actualid);
            AddOptions();
            //------------------------
            p.Con.Close();
            Console.Clear();
            m.DoMenu(p.FolderList);

           
            void SearchForFolder(int id)
            {
                //var dt = new DataTable();
                string query = "SELECT  FolderName, Folder_id FROM Folders WHERE Parent_id = @Folder_id AND username = @username";
                var sda = new SqlDataAdapter(query, p.Con);
                //SQL Injection Protection
                sda.SelectCommand.Parameters.AddWithValue("@username", SqlDbType.VarChar);
                sda.SelectCommand.Parameters["@username"].Value = p.Getlgduser();
                //SQL Injection Protection
                sda.SelectCommand.Parameters.AddWithValue("@Folder_id", SqlDbType.Int);
                sda.SelectCommand.Parameters["@Folder_id"].Value = id;
                sda.Fill(dt);
                
                sorszam = dt.Rows.Count;
                for (int i = 0; i < sorszam; i++)
                {
                    p.FolderList.Add(dt.Rows[i][0].ToString());
                }
                p.IdList.Clear();
                for (int i = 0; i < sorszam; i++)
                {
                    p.IdList.Add((int)dt.Rows[i][1]);
                }
                dt.Clear();
            }

            void SearchForFile(int id)
            {

                Tester(id);


                dt.Clear();
            }

            void AddOptions()
            {
                p.FolderList.Add("---------");
                p.FolderList.Add("Új Mappa");
                p.FolderList.Add("Feltöltés");
                p.FolderList.Add("Vissza");
                p.FolderList.Add("Kilépés");
            }
        }
        public void Tester(int id)
        {

            int sorszam = 0;
            var dt = new DataTable();
            string query = "SELECT FileName FROM Documents WHERE Folder_id = @Folder_id AND username = @username";
            var sda = new SqlDataAdapter(query, p.Con);
            //SQL Injection Protection
            sda.SelectCommand.Parameters.AddWithValue("@username", SqlDbType.VarChar);
            sda.SelectCommand.Parameters["@username"].Value = p.Getlgduser();
            //SQL Injection Protection
            sda.SelectCommand.Parameters.AddWithValue("@Folder_id", SqlDbType.Int);
            sda.SelectCommand.Parameters["@Folder_id"].Value = id;
            sda.Fill(dt);
           // Console.WriteLine("A talált fájlok:");
            //m.DebugTable(dt);
            sorszam = dt.Rows.Count;

            for (int i = 0; i < sorszam; i++)
            {
                p.FolderList.Add(dt.Rows[i][0].ToString());
            }
            dt.Clear();
        }
#endregion
        

        public void GetRootDirectory()
        {
            p.Con.Open();
            
            Console.WriteLine("Üdv {0}!\n", GetUsersFullName());

            DataTable dt = new DataTable();
            
            var query = "SELECT Folder_id FROM Folders WHERE (Parent_id IS NULL OR Parent_id = 0) AND username = @username";
            var sda = new SqlDataAdapter(query, p.Con);

            //SQL Injection Protection
            sda.SelectCommand.Parameters.AddWithValue("@username", SqlDbType.VarChar);
            sda.SelectCommand.Parameters["@username"].Value = p.Getlgduser();
            sda.Fill(dt);
            if (dt.Rows.Count == 0)
            {
                Console.WriteLine("Még nincsenek feltöltött elemek!");
            }

            sorszam = dt.Rows.Count;

            for (var j = 0; j < sorszam; j++)
            {
                p.IdList.Add((int)dt.Rows[j][0]);
            }
            foreach (var t in p.IdList)
            {
                p.FolderList.Add(GetFName(t));
            }

            p.FolderList.Add("---------");
            p.FolderList.Add("Új Mappa");
            p.FolderList.Add("Feltöltés");
            p.FolderList.Add("Vissza");
            p.FolderList.Add("Kilépés");
            //m.DebugTable(dt);                   
            p.Con.Close();
            dt.Clear();
            m.DoMenu(p.FolderList);
        }

        public string GetFName(int id)
        {
            p.Con.Open();
            const string query = "SELECT FolderName FROM Folders WHERE Folder_id = @Folder_id AND username = @username";
            var cmd = new SqlCommand(query,p.Con);

            cmd.Parameters.AddWithValue("@Folder_id", SqlDbType.Int);
            cmd.Parameters["@Folder_id"].Value = id;
            cmd.Parameters.AddWithValue("@username", SqlDbType.VarChar);
            cmd.Parameters["@username"].Value = p.Getlgduser();

            try
            {
                var name = cmd.ExecuteScalar().ToString();
                p.Con.Close();
                return name;
            }
            catch (Exception e)
            {
                p.Con.Close();
                Console.WriteLine(e);
                return null;
            }            
        }

        public bool GetDoktároló(string parentname, int parentid)
        {
            var dt = new DataTable();
            string query =
                "SELECT Doc_Container FROM Folders WHERE Folder_id = @parentid AND FolderName = @parentname";
            var sda = new SqlDataAdapter(query, p.Con);
            //SQL Injection Protection
            sda.SelectCommand.Parameters.AddWithValue("@parentid", SqlDbType.Int);
            sda.SelectCommand.Parameters["@parentid"].Value = p.GetActPid();
            //SQL Injection Protection
            sda.SelectCommand.Parameters.AddWithValue("@parentname", SqlDbType.VarChar);
            sda.SelectCommand.Parameters["@parentname"].Value = parentname;

            try
            {
                sda.Fill(dt);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            //BUG parentid = 0
            if (dt.Rows[0][0].ToString() == "False")
            {
                return false;
            }
            else
            {
                return true;
            }

            
            //m.DebugTable(dt);
        }

        public int KiaBarátom(string name)
        {
            int actualid = 0;            
            for (int i = 0; i < p.FolderList.Count; i++)
            {
                if (p.FolderList[i] == name)
                {
                    actualid = p.IdList[i];
                    break;
                }
            }
            return actualid;
        }

        public void ChangeFolderName()
        {
            //Mappa nevének ellenőrzött bekérése
            //------------------------
            Console.WriteLine("\nAdja meg a mappa nevét, amit át akar nevezni:");
            string mappanev = null;  
            //--
            while (true)
            {
                mappanev = Console.ReadLine();

                //Null vagy whitespace ellenőrzés
                //------------------------
                if (string.IsNullOrEmpty(mappanev) || string.IsNullOrWhiteSpace(mappanev))
                {
                    Console.WriteLine("Ez a mező nem lehet üres. \nAdja meg a mappa nevét, amit át akar nevezni:");
                }
                //------------------------

                //Van-e ilyen mappa? ellenőrzés
                //------------------------
                else if (p.FolderList.Contains(mappanev) == false)
                {
                    Console.WriteLine("Nincs ilyen nevű mappa a környezetben.");
                }
                //------------------------
                else
                    break; 
            }
            //------------------------

            //Megnézzük, hogy a Mappa nevének mi az id-ja.
            //------------------------
            var id = KiaBarátom(mappanev);
            //------------------------

            //Új mappanév ellenőrzött bekérése
            //------------------------
            Console.WriteLine("Kérem adja meg a mappa új nevét:");
            string newname = null;
            while (string.IsNullOrEmpty(newname) || string.IsNullOrWhiteSpace(newname))
            {
                newname = Console.ReadLine();
                if (string.IsNullOrEmpty(newname) || string.IsNullOrWhiteSpace(newname))
                {
                    Console.WriteLine("Ez a mező nem lehet üres. \nKérem adja meg a mappa új nevét:");
                }
            }
            //------------------------

            //Parancs végrehajtása
            //------------------------
            p.Con.Open();
            const string query = "UPDATE Folders SET FolderName = @newname WHERE username = @username " +
                                 "AND FolderName = @foldername AND Folder_id = @id;";
            var rename = new SqlCommand(query, p.Con);
            //--
            rename.Parameters.Add("@username", SqlDbType.VarChar);
            rename.Parameters["@username"].Value = p.Getlgduser();
            //--
            rename.Parameters.Add("@foldername", SqlDbType.VarChar);
            rename.Parameters["@foldername"].Value = mappanev;
            //--
            rename.Parameters.Add("@newname", SqlDbType.VarChar);
            rename.Parameters["@newname"].Value = newname;
            //--
            rename.Parameters.Add("@id", SqlDbType.Int);
            rename.Parameters["@id"].Value = id;
            //--
            rename.ExecuteNonQuery();
            p.Con.Close();
            //------------------------

            //Parancs kész, Visszalépés
            //------------------------
            Console.WriteLine("Mappa átnevezve!");
            Thread.Sleep(2000);
            Console.Clear();
            OpenFolder(actp());
            m.SetInRoot(true);
            //------------------------
        }


        //---------------------------------------
        public string actp(){return p.actualparent;}
        public int actpid(){return p.actualparentid;}
        public string actchild(){return p.actualchild;}
        public int actchildid(){return p.actualchildid;}
        //---------------------------------------
    }
}//namespace IBControll