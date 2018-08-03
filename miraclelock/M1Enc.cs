using System;
using System.Runtime.InteropServices;

namespace miraclelock
{
    internal class M1Enc
    {
        [DllImport("proRFL.dll", EntryPoint = "initializeUSB")]
        public static extern int InitializeUSB(byte aType);

        [DllImport("proRFL.dll", EntryPoint = "ReadCard")]
        public static extern int ReadCard(byte flagusb, byte[] carddata);

        [DllImport("proRFL.dll", EntryPoint = "GetCardTypeByCardDataStr")]
        public static extern int GetCardTypeByCardDataStr(byte[] carddata, byte[] cardtype);

        [DllImport("proRFL.dll", EntryPoint = "GetGuestLockNoByCardDataStr")]
        public static extern int GetGuestLockNoByCardDataStr(int dlscoid, byte[] carddata, byte[] lockno);

        [DllImport("proRFL.dll", EntryPoint = "GetGuestETimeByCardDataStr")]
        public static extern int GetGuestETimeByCardDataStr(int dlscoid, byte[] carddata, byte[] ETime);

        [DllImport("proRFL.dll", EntryPoint = "CardErase")]
        public static extern int CardErase(int d12, int dlscoid, byte[] cardbuf);

        [DllImport("proRFL.dll", EntryPoint = "GuestCard")]
        public static extern int GuestCard(byte d12, int dlscoid, byte cardno, byte dai, byte llock, byte pdoors, char[] Bdate, char[] EDate, char[] RoomNo, byte[] cardhexstr);
        // llock: 0---can't open counter lock 1---could open counter lock
        // pdoors: 0---can't open public door lock 8---can open public door lock
        // weekmask: fixed to be 127
        // STime: start time "0835" presents 08:35       

        [DllImport("proRFL.dll", EntryPoint = "BuildingCard")]
        public static extern int BuildingCard(byte d12, int dlscoid, byte cardno, byte dai, byte llock, byte pdoors, byte weekmask, char[] Bdate, char[] Stime, char[] EDate, char[] RoomNo, byte[] cardhexstr);
        
        //lost card
        [DllImport("proRFL.dll", EntryPoint = "LimitCard")]
        public static extern int LimitCard(byte d12, int dlsCoID, byte CardNo, byte dai, char[] Bdate, byte[] LCardNo, byte[] cardHexStr);

        public static string Card_type(byte cardtype)
        {
            string r;
            switch (Convert.ToInt16(((Char)cardtype).ToString(), 16))
            {
                case 0:
                case 48:
                    r = "System";
                    break;
                case 1:
                case 49:
                    r = "Record";
                    break;
                case 2:
                case 50:
                    r = "Lock No";
                    break;
                case 3:
                case 51:
                    r = "Clock";
                    break;
                case 4:
                case 52:
                    r = "Lost";
                    break;
                case 5:
                case 53:
                    r = "GS";
                    break;
                case 6:
                case 54:
                    r = "Гость";
                    break;
                case 7:
                case 55:
                    r = "Terminate";
                    break;
                case 8:
                case 56:
                    r = "Group";
                    break;
                case 9:
                case 57:
                    r = "Unknown";
                    break;
                case 10:
                case 58:
                    r = "Emergency";
                    break;
                case 11:
                case 59:
                    r = "Master";
                    break;
                case 12:
                case 60:
                    r = "Карта перснала (здание/комплекс)";
                    break;
                case 13:
                case 61:
                    r = "Карта персонала (этаж/корпус)";
                    break;
                case 14:
                case 62:
                    r = "Unknown";
                    break;
                case 15:
                case 63:
                    r = "Чистая карта";
                    break;
                default:
                    r = "##" + Convert.ToInt16((Char)cardtype).ToString() + "##";
                    break;
            }
            return r;
        }
    }
}