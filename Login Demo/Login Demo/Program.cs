using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Data;

namespace Login_Demo
{
    /// GLOBAL VARIABLES ///
    class Global
    {
        public static bool terminate = false;
    }
    
    /// FUNCTION CLASSES ///

    // Delay Functions
    public class Delay
    {
        public static void ms1500()
        {
            Thread.Sleep(1500);

        }

        public static void ms750()
        {
            Thread.Sleep(750);
        }

        public static void ms500()
        {
            Thread.Sleep(500);
        }

        public static void ms300()
        {
            Thread.Sleep(300);
        }
    }

    // Message Functions
    public class Message
    {
        public static void Loading()
        {
            Console.WriteLine("\nLoading...\n");
        }

        public static void Invalid()
        {
            Console.WriteLine("\nInvalid Input, Reloading...\n");
        }

        public static void Invalid(string item)
        {
            Console.WriteLine("\nInvalid {0}...\n", item);
        }

        public static void Quiting()
        {
            Console.WriteLine("\nQuiting...\n");

        }

        public static void Unavailable()
        {
            Console.WriteLine("\nFunction Unavailable...\n");
        }

        public static void Login(string status)
        {
            Console.Write("\nLog In {0}...\n", status);
        }

        public  static void Taken(string input)
        {
            Console.WriteLine("\n{0} Taken, Re-enter...\n", input);
        }

        public static void Authenticate()
        {
            Console.WriteLine("\nAuthenticating...\n");
        }
    }

    // Input Functions
    public class Input
    {
        public static string Options()
        {
            Console.Write("Input Option: ");
            return Console.ReadLine();
        }

        public static string Password()
        {
            StringBuilder passwordBuilder = new StringBuilder();
            bool continueReading = true;
            char newLineChar = '\r';
            while (continueReading)
            {
                ConsoleKeyInfo consoleKeyInfo = Console.ReadKey(true);
                char passwordChar = consoleKeyInfo.KeyChar;

                if (passwordChar == newLineChar)
                {
                    continueReading = false;
                }
                else
                {
                    passwordBuilder.Append(passwordChar.ToString());
                }
            }

            return passwordBuilder.ToString();
        }

    }

    // UI Functions
    public class Ui
    {
        public static int uiLength = 60;
        public static int uiHeight = 5;

        public static void Closing(int top, int bottom)
        {
            for (int i = 0; i < top; i++)
            {
                Console.WriteLine("");
            }
            Console.WriteLine(" {0}", String.Concat(Enumerable.Repeat("#", uiLength)));
            for (int i = 0; i < bottom; i++)
            {
                Console.WriteLine("");
            }
        }

        public static void Middle(int rows)
        {
            for (int i = 0; i < rows; i++)
            {
                Console.WriteLine(" #{0}#", String.Concat(Enumerable.Repeat(" ", uiLength - 2)));
            }

        }

        public static void MiddleLeftText(string text)
        {
            Console.WriteLine(" # {0}{1}#", text, String.Concat(Enumerable.Repeat(" ", uiLength - text.Length - 3)));

        }

        public static void MiddleCenterTitleText(string text)
        {
            Console.WriteLine(" #{0}#", String.Concat(Enumerable.Repeat(" ", uiLength - 2)));
            if (text.Length % 2 == 0)
            {
                Console.WriteLine(" #{1}{0}{1}#", text, String.Concat(Enumerable.Repeat(" ", (uiLength - text.Length - 2) / 2)));
            }
            else
            {
                Console.WriteLine(" #{1}{0}{1} #", text, String.Concat(Enumerable.Repeat(" ", (uiLength - text.Length - 2) / 2)));
            }
            Console.WriteLine(" #{0}#", String.Concat(Enumerable.Repeat(" ", uiLength - 2)));
        }

        public static void Page(string title, string[] options)
        {
            Ui.Closing(1, 0);
            Ui.MiddleCenterTitleText(title);
            foreach (string option in options)
            {
                Ui.MiddleLeftText(option);
            }
            Ui.Middle(uiHeight - options.Length);
            Ui.Closing(0, 1);
        }
    }

    // Password Functions
    public class Password
    {
        public static string HashSalt(string input)
        {
            byte[] salt;
            new RNGCryptoServiceProvider().GetBytes(salt = new byte[16]);

            var pbkdf2 = new Rfc2898DeriveBytes(input, salt, 100000);
            byte[] hash = pbkdf2.GetBytes(20);

            byte[] hashBytes = new byte[36];
            Array.Copy(salt, 0, hashBytes, 0, 16);
            Array.Copy(hash, 0, hashBytes, 16, 20);

            string savedPasswordHash = Convert.ToBase64String(hashBytes);

            return savedPasswordHash;
        }

        public static bool DecryptHashSalt(string input, string savedInput)
        {
            // Extract the bytes
            byte[] hashBytes = Convert.FromBase64String(savedInput);
            // Get the salt
            byte[] salt = new byte[16];
            Array.Copy(hashBytes, 0, salt, 0, 16);
            // Compute the hash on the password the user entered
            var pbkdf2 = new Rfc2898DeriveBytes(input, salt, 100000);
            byte[] hash2 = pbkdf2.GetBytes(20);
            // Compare the results
            for (int i = 0; i < 20; i++)
                if (hashBytes[i + 16] != hash2[i])
                    return false;
            return true;
        }

    }

    // Database Functions
    public class DB
    {
        
        public static MySqlConnection Connection()
        {
            string connStr = "server=localhost;user=XXXXX;database=logindb;port=3306;password=XXXXX";
            MySqlConnection conn = new MySqlConnection(connStr);

            return conn;
        }

        public static MySqlCommand Command(string query, MySqlConnection conn)
        {
            MySqlCommand cmd = new MySqlCommand(query, conn);
            return cmd;
        }


        public static void InsertDelete(string query)
        {
            MySqlConnection conn = Connection();
            try
            {
                conn.Open();
                Command(query, conn).ExecuteNonQuery();
                conn.Close();

            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
        }

        public static DataTable Select(string query)
        {
            MySqlConnection conn = Connection();
            try
            {
                conn.Open();
                MySqlDataAdapter dataAdapter = new MySqlDataAdapter();
                dataAdapter.SelectCommand = Command(query, conn);
                DataTable table = new DataTable();
                dataAdapter.Fill(table);
                return table;
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
        }

        public static void Update(string query)
        {
            MySqlConnection conn = Connection();
            MySqlTransaction tr = null;
            try
            {
                conn.Open();
                tr = conn.BeginTransaction();
                MySqlCommand cmd = new MySqlCommand();
                cmd.Connection = conn;
                cmd.Transaction = tr;
                cmd.CommandText = query;
                cmd.ExecuteNonQuery();
                tr.Commit();

            }
            catch
            {
                tr.Rollback();
            }
            finally
            {
                if (conn != null)
                    conn.Close();
            }
        }

        public static string SingleItem(DataTable table)
        {
            foreach (DataRow dataRow in table.Rows)
            {
                foreach (var item in dataRow.ItemArray)
                {
                    return (string)item;
                }
            }

            return null;
        }

    }

    internal class Program
    {
        static void Main(string[] args)
        {
            StartPage();
            
        }

        // Pages
        static void StartPage()
        {
            Global.terminate = false;

            while (!Global.terminate)
            {
                Console.Clear();
                Ui.Page("Log In Demo", new string[] {"1) Log In", "2) Create Account", "3) Quit"});
                string input = Input.Options();

                switch (input)
                {
                    case "1":
                        Global.terminate = true;
                        Message.Loading();
                        Delay.ms500();
                        LoginPage();
                        break;
                    case "2":
                        // FIX: additional character limit restrictions + email @ / .com check
                        Message.Loading();
                        Delay.ms500();
                        Console.Write("First Name: "); string firstName = Console.ReadLine();
                        Console.Write("Last Name: "); string lastName = Console.ReadLine();

                        // email check
                        string email = "", username = "";
                        bool check = false;
                        while (!check)
                        {
                            Console.Write("Email: "); email = Console.ReadLine();
                            if (DB.SingleItem(DB.Select($"SELECT email FROM users WHERE email = '{email.ToLower()}'")) == null)
                            {
                                check = true;
                            }
                            else
                            {
                                Message.Taken("Email");
                            }
                        }

                        // username check
                        check = false;
                        while (!check)
                        {
                            Console.Write("Username: "); username = Console.ReadLine();
                            if (DB.SingleItem(DB.Select($"SELECT username FROM users WHERE username = '{username.ToLower()}'")) == null)
                            {
                                check = true;
                            }
                            else
                            {
                                Message.Taken("Username");
                            }
                        }

                        Console.Write("Password: "); string password = Password.HashSalt(Input.Password());

                        // get userid
                        int userid = 0;
                        try
                        {
                            userid = Convert.ToInt32(DB.SingleItem(DB.Select("SELECT max(userid) FROM users")));
                            userid++;
                        }
                        catch
                        {

                        }
                        // fields(varchar length): userid(6) , username(20), password(48), firstname(20), lastlast(20), email(50)
                        DB.InsertDelete($"INSERT INTO users VALUES ('{userid.ToString("D6")}','{username}','{password}','{firstName}','{lastName}','{email}')");
                        Console.WriteLine("\n\nRegistered!\n");
                        Delay.ms500();
                        break;
                    case "3":
                        Global.terminate = true;
                        Message.Quiting();
                        Delay.ms500();
                        break;
                    default:
                        Message.Invalid();
                        Delay.ms500();
                        break;
                }

            }
            
        }

        static void LoginPage()
        {
            Global.terminate = false;
            string username = "";
            string password = "";

            while (!Global.terminate)
            {
                Console.Clear();
                Ui.Page("Login", new string[] { "1) Enter Username / Password", "2) Forgot Username / Password", "3) Back", "4) Quit" });
                string input = Input.Options();

                switch (input)
                {
                    case "1":
                        // check username and password
                        int tries = 0;
                        while (tries < 3)
                        {
                            Console.Write("\nUsername: "); username = Console.ReadLine();
                            Console.Write("Password: "); password = Input.Password();
                            Console.WriteLine("");

                            // check password from database
                            bool authorized = false;
                            try
                            {
                                string databasePassword = DB.SingleItem(DB.Select($"SELECT password FROM users WHERE username = '{username}'"));
                                authorized = Password.DecryptHashSalt(password, databasePassword);

                            } catch
                            {

                            }

                            if (authorized)
                            {
                                Global.terminate = true;
                                tries = 5;
                                Message.Login("Success");
                                Delay.ms500();
                                MainPage(DB.SingleItem(DB.Select($"SELECT userid FROM users WHERE username = '{username}'")),
                                         DB.SingleItem(DB.Select($"SELECT firstName FROM users WHERE username = '{username}'")));
                            }
                            else
                            {
                                tries++;
                                if (tries == 3)
                                {
                                    Global.terminate = true;
                                    Message.Login("Failed");
                                    Message.Quiting();
                                    Delay.ms500();
                                }
                                else
                                {
                                    Message.Login("Invalid");
                                }
                            }
                        }
                        Delay.ms500();
                        break;
                    case "2":
                        Global.terminate = true;
                        Message.Loading();
                        Delay.ms500();
                        ForgotPage();
                        break;
                    case "3":
                        Global.terminate = true;
                        Message.Loading();
                        Delay.ms500();
                        StartPage();
                        break;
                    case "4":
                        Global.terminate = true;
                        Message.Quiting();
                        Delay.ms500();
                        break;
                    default:
                        Message.Invalid();
                        Delay.ms500();
                        break;
                }
            }
        }

        static void ForgotPage()
        {
            Global.terminate = false;

            while (!Global.terminate)
            {
                Console.Clear();
                Ui.Page("Forgot Username / Password", new string[] { "1) Forgot Username", "2) Forgot Password", "3) Back" });
                string input = Input.Options();

                string username = "", email = "";

                switch (input)
                {
                    

                    case "1":
                        Console.Write("\nEnter Email to retrieve Username: "); email = Console.ReadLine();
                        var returnEmail = DB.SingleItem(DB.Select($"SELECT username FROM users WHERE email = '{email.ToLower()}'"));
                        if (returnEmail == null)
                        {
                            Message.Invalid("Email");
                            Delay.ms500();
                        }
                        else
                        {
                            Console.WriteLine("\n" + returnEmail);
                            Delay.ms1500();
                        }
                        
                        break;
                    case "2":
                        Console.Write("\nEnter Email to reset Password: "); email = Console.ReadLine();
                        Console.Write("Enter Username to reset Password: "); username = Console.ReadLine();
                        if (DB.SingleItem(DB.Select($"SELECT password FROM users WHERE email = '{email.ToLower()}' AND username = '{username.ToLower()}'")) == null)
                        {
                            Message.Invalid("Email / Username");

                        }
                        else
                        {
                            Console.Write("\nEnter new Password: "); string password = Password.HashSalt(Input.Password());
                            DB.Update($"UPDATE users SET password='{password}' WHERE username='{username}'");
                            Console.WriteLine("\n\nPassword Updated...");
                        }
                        Delay.ms500();
                        break;
                    case "3":
                        Global.terminate = true;
                        Message.Loading();
                        Delay.ms500();
                        LoginPage();
                        break;
                    default:
                        break;
                }
            }

        }

        static void MainPage(string userid, string firstName)
        {
            Global.terminate = false;

            while (!Global.terminate)
            {
                Console.Clear();
                Ui.Page($"Welcome {firstName}!", new string[] { "1) Log Out", "2) Delete Account", "3) Quit" });
                string input = Input.Options();

                switch (input)
                {
                    case "1":
                        Global.terminate = true;
                        Message.Loading();
                        Delay.ms500();
                        LoginPage();
                        break;
                    case "2":
                        Global.terminate = true;
                        DB.InsertDelete($"DELETE FROM users WHERE userid='{userid}';");
                        Console.WriteLine("\nAccount Deleted!\n");
                        Delay.ms500();
                        StartPage();
                        break;
                    case "3":
                        Global.terminate = true;
                        Message.Quiting();
                        Delay.ms500();
                        break;
                    default:
                        Message.Invalid();
                        Delay.ms500();
                        break;
                }

            }
            
        }
    }
}
