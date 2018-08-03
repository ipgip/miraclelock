using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace miraclelock
{
    static class Program
    {
        public static Hashtable Config = new Hashtable();
        public static int HotelId;
        static int Building = 0;
        static int Floor = 0;
        static int Room = 0;

        public static bool LicenseFlag = false;


        /// <summary>
        /// Главная точка входа для приложения.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            if (File.Exists(@"c:\Pos\Locks.xml"))
            {
                Parse_Config(@"c:\Pos\Locks.xml");
            }
            else
            {
                MessageBox.Show("Нет конфигурационного файла");
                return;
            }
            try
            {
                Thread.CurrentThread.CurrentUICulture = new CultureInfo(Config["Lang"].ToString());
            }
            catch (Exception)
            {
                MessageBox.Show("<Lang> отсутствует в конфигурационном файле. Используется язык по умолчанию");
            }

            if ((!Config.ContainsKey("License")) || (!Licensing.CheckLicense(Config["License"].ToString())))
            {

                MessageBox.Show($"Ошибка {Licensing.CPU()}: Нет лиензии");
                using (StreamWriter w = new StreamWriter(@"C:\pos\Error.txt"))
                {
                    w.WriteLine($"Ошибка {Licensing.CPU()}: Нет лиензии");
                }
                return;
            }

            HotelId = int.Parse(Config["HId"].ToString());
            if (HotelId == 0)
            {
                MessageBox.Show("Не задан идентификатор отеля");
                return;
            }
            try
            {
                using (LockDbDataContext Db = new LockDbDataContext(Convert.ToString(Config["ConnectionString"])))
                {
                    if (!Db.DatabaseExists())
                    {
                        Db.CreateDatabase();
                        MessageBox.Show("База данных создана. Заполните базу.");
                        return;
                    }
                }
            }
            catch (Exception err)
            {
                MessageBox.Show($"Ошибка SQL {err.Message}");
                return;

            }
            // Проверяем не запущен ли уже экземпляр программы
            using (var mutex = new Mutex(false, "Lock"))
            {
                if (mutex.WaitOne(TimeSpan.FromSeconds(3))) // Подождать три секунды - вдруг предыдущий экземпляр еще закрывается
                {
                    Application.Run(new Form1() { WindowState = FormWindowState.Maximized });
                }
                else
                {
                    MessageBox.Show("Другая копия программы запущена на этом компьютере. Используйте Alt+Tab для переключения между приложениями");
                    return;
                }
            }
        }

        /// <summary>
        /// Чтение и разбор конфигурацинного файла
        /// </summary>
        /// <param name="p">Путь</param>
        private static void Parse_Config(string p)
        {
            XElement doc = XElement.Load(p);
            foreach (XElement n in doc.Elements())
            {
                Config.Add(n.Name.LocalName, n.Value);
            }
        }

        public static byte[] ReadLockNo(byte[] cardbuf)
        {
            //cardbuf = new byte[250];

            //if (M1Enc.InitializeUSB(1) != 0)
            //{
            //    MessageBox.Show("Проблемы с авторизатором");
            //    return null;
            //}
            byte[] lockno = new byte[8];
            //if ((M1Enc.ReadCard(1, cardbuf) == 0) /*&& (cardbuf[5] != 48)*/)
            //{
            //    string Signature = Encoding.ASCII.GetString(cardbuf);
            //    if (Signature.Length > 6)
            //    {
            //        if (Signature.Substring(0, 6) == "551501")
            //        {
            if (cardbuf != null)
                M1Enc.GetGuestLockNoByCardDataStr(Program.HotelId, cardbuf, lockno);
            return lockno;
            //        }
            //    }
            //}
            //return null;
        }

        public static string ReadCard(out byte[] cardbuf)
        {
            cardbuf = null;
            if (M1Enc.InitializeUSB(1) != 0)
            {
                MessageBox.Show("Проблемы с авторизатором");
                return string.Empty;
            }
            string Crd = string.Empty;
            cardbuf = new byte[250];
            if ((M1Enc.ReadCard(1, cardbuf) == 0) /*&& (cardbuf[5] != 48)*/)
            {
                string Signature = Encoding.ASCII.GetString(cardbuf);
                if (Signature.Length > 6)
                {
                    if (Signature.Substring(0, 6) == "551501")
                    {
                        return Encoding.ASCII.GetString(cardbuf).Substring(24, 8);
                    }
                    else
                    {
                        //MessageBox.Show("В авторизаторе неверная карта");
                        return string.Empty;
                    }
                }
                else
                    return string.Empty;
            }
            else
                return string.Empty;
        }

        public static int IshueCard(int B, int F, int R, DateTime D)
        {
            Building = B;
            Floor = F;
            Room = R;
            return IshueCard(D);
        }

        public static int IshueCard(DateTime D)
        {
            byte[] cardbuf = new byte[250];
            int ret = 0;
            string Bdate = DateTime.Now.ToString("yyMMddHHmm");
            string Edate = D.ToString("yyMMddHHmm");
            byte[] lockno = new byte[8];

            if (M1Enc.InitializeUSB(1) != 0)
            {
                MessageBox.Show("Проблемы с авторизатором");
                return 1;
            }
            if ((Building != 0) && (Floor != 0) && (Room != 0))
            {
                if ((M1Enc.ReadCard(1, cardbuf) == 0) && (cardbuf[5] != 48))
                {
                    byte b = cardbuf[5];
                    if (M1Enc.GetGuestLockNoByCardDataStr(Program.HotelId, cardbuf, lockno) == 0)
                    {
                        string R = Encoding.ASCII.GetString(lockno).Substring(0, 6);
                        Building = int.Parse(R.Substring(0, 2));
                        Floor = int.Parse(R.Substring(2, 2));
                        Room = int.Parse(R.Substring(4, 2));
                    }
                }
            }
            return M1Enc.GuestCard(1, Program.HotelId,
                                   1, 0, 0, 0, Bdate.ToCharArray(0, 10), Edate.ToCharArray(0, 10),
                                   ($"{Building:00}{Floor:00}{Room:00}" + "99").ToCharArray(0, 8), cardbuf);

            //return 1;
        }
    }

    public class Licensing
    {
        public static bool CheckLicense(string Cpu)
        {
            if (Cpu != CPU())
            {
                Program.LicenseFlag = false;
                return (Math.Abs((DateTime.Now - StartDate()).Days) < 7);
            }
            else
            {
                Program.LicenseFlag = true;
                return true;
            }
        }

        public static string CPU()
        {
            string serial = String.Empty;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT ProcessorId FROM Win32_Processor");
            //Caption,CurrentClockSpeed,LoadPercentage,Manufacturer,MaxClockSpeed,Name,ProcessorId,SocketDesignation,SystemName,Version FROM Win32_Processor");
            foreach (ManagementObject queryObj in searcher.Get())
            {
                serial = queryObj["ProcessorId"].ToString();
            }
            return serial;
        }

        public static DateTime StartDate()
        {
            DateTime ret = DateTime.MinValue;
            try
            {
                ret = File.GetLastWriteTime(Application.ExecutablePath).Date;
                //ret = File.GetCreationTime(Application.ExecutablePath).Date;
            }
            catch (Exception err)
            {
                MessageBox.Show($"{err.Message}");
            }
            return ret;
        }

    }

}
