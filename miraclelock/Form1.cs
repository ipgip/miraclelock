using Depo;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace miraclelock
{
    public enum RoomStates { Vacant = 0, Busy, Repair }

    public partial class Form1 : Form
    {

        List<GraphicsPath> P = new List<GraphicsPath>();
        //List<HotelRooms> HR = new List<HotelRooms>();
        Color ColorVacant;
        Color ColorBusy;
        Color ColorRepair;
        HotelRooms _GP;
        string Card = string.Empty;

        public Form1()
        {
            InitializeComponent();
        }

        public void DrawRooms(List<HotelRooms> HR)
        {
            using (Graphics G = panel1.CreateGraphics())
            {
                //G.Clear(SystemColors.Control);
                using (LockDbDataContext Db = new LockDbDataContext(Convert.ToString(Program.Config["ConnectionString"])))
                {
                    Db.Refresh(System.Data.Linq.RefreshMode.OverwriteCurrentValues);
                    LoadRoomState();
                    Point StartPoint = new Point(70, 20);
                    foreach (var L in HR)
                    {
                        if (L.State != RoomStates.Busy)
                        {
                            G.FillPath(new SolidBrush(State2Color((int)L.State)), L.GP);
                        }
                        else
                        {
                            if (L.Co >= DateTime.Now)
                            {
                                G.FillPath(new SolidBrush(State2Color((int)L.State)), L.GP);
                            }
                            else
                            {
                                G.FillPath(new HatchBrush(HatchStyle.DarkVertical, State2Color((int)L.State)), L.GP);
                            }
                        }

                        G.DrawPath(Pens.LightGray, L.GP);
                        G.DrawString(L.HumanReadableRoom.Trim(), new Font(SystemFonts.DefaultFont.FontFamily, 18), Brushes.Black, L.GP.PathPoints.First());
                    }
                    //G.DrawLine(Pens.Black, 0, startPoint.Y - 30, panel1.Width, startPoint.Y - 30);
                }
            }
        }

        private Point NewLine(Point startPoint)
        {
            startPoint.X = Convert.ToInt32(Program.Config["StartX"] ?? 70);
            startPoint.Y += Convert.ToInt32(Program.Config["StepY"] ?? 90);
            return startPoint;
        }

        private Point NewPosition(Point StartPoint)
        {
            StartPoint.X += Convert.ToInt32(Program.Config["StepX"] ?? 70);
            if (StartPoint.X > panel1.Width - Convert.ToInt32(Program.Config["StartX"] ?? 70))
            {
                StartPoint.X = Convert.ToInt32(Program.Config["StartX"] ?? 70);
                StartPoint.Y += Convert.ToInt32(Program.Config["StepY"] ?? 90);
            }
            return StartPoint;
        }


        private Color State2Color(int S)
        {
            Color ret;
            switch ((RoomStates)S)
            {
                case RoomStates.Vacant:
                    ret = ColorVacant;
                    break;
                case RoomStates.Busy:
                    ret = ColorBusy;
                    break;
                case RoomStates.Repair:
                    ret = ColorRepair;
                    break;
                default:
                    ret = SystemColors.Control;
                    break;
            }
            return ret;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ColorVacant = Color.FromName(Convert.ToString(Program.Config["Vacant"] ?? "YellowGreen"));
            ColorBusy = Color.FromName(Convert.ToString(Program.Config["Busy"] ?? "LightCyan"));
            ColorRepair = Color.FromName(Convert.ToString(Program.Config["Repair"] ?? "LightPink"));
            timer1.Interval = int.Parse(Convert.ToString(Program.Config["UpdateInterval"] ?? "10000"));
            Возврат_депозита.Visible = Convert.ToBoolean(Program.Config["Depo"] ?? false);
            Пополнение_депозита.Visible = Convert.ToBoolean(Program.Config["Depo"] ?? false);
            toolStripButton1.Visible = false;

            LoadRoomState();
            timer1.Start();
        }

        private List<HotelRooms> LoadRoomState()
        {
            List<HotelRooms> HR = new List<HotelRooms>();
            using (LockDbDataContext Db = new LockDbDataContext(Convert.ToString(Program.Config["ConnectionString"])))
            {
                //HR.Clear();
                var _S = Db.Rooms.OrderBy(x => x.HumanRoomNumber).GroupBy(x => x.Floor.Value);
                if (_S.Count() > 0)
                {
                    Point StartPoint = new Point(Convert.ToInt32(Program.Config["StartX"] ?? 70), Convert.ToInt32(Program.Config["StartY"] ?? 50));
                    //Point StartPoint = new Point(70, 50);
                    HotelRooms Room = new HotelRooms();
                    foreach (var L in _S)
                    {
                        foreach (var N in L)
                        {
                            Rectangle R = new Rectangle(StartPoint, new Size(Convert.ToInt32(Program.Config["SizeX"] ?? 50), Convert.ToInt32(Program.Config["SizeY"] ?? 50)));
                            GraphicsPath GP = new GraphicsPath();
                            GP.AddRectangle(R);
                            P.Add(GP);
                            DateTime Checkout;
                            if (N.Cards.Count() > 0)
                            {
                                Checkout = N.Cards.Max(x => x.Co.Value);
                            }
                            //Checkout = N.Cards.First().Co.Value;
                            else
                            {
                                Checkout = DateTime.MaxValue;
                            }

                            Room = new HotelRooms
                            {
                                HumanReadableRoom = N.HumanRoomNumber,
                                Room = N.Room.Value,
                                Building = N.Building.Value,
                                Floor = N.Floor.Value,
                                GP = GP,
                                State = (RoomStates)N.State.Value,
                                Co = Checkout//N.Cards.First().Ci.Value
                            };
                            HR.Add(Room);
                            StartPoint = NewPosition(StartPoint);
                        }
                        StartPoint = NewLine(StartPoint);
                    }
                }
            }
            return HR;
        }

        private void ToolStrip1_Paint(object sender, PaintEventArgs e)
        {
        }

        private void Panel1_Paint(object sender, PaintEventArgs e)
        {
            if (!DesignMode)
            {
                DrawRooms(LoadRoomState());
            }
        }

        private void Panel1_MouseClick(object sender, MouseEventArgs e)
        {
            Point Click = new Point(e.X, e.Y);
            MenuItem P1 = new MenuItem("Записать карту");
            P1.Click += P1_Click;
            MenuItem P2 = new MenuItem("Выселить");
            P2.Click += P2_Click;
            MenuItem P3 = new MenuItem("Блокировать утерянную карту");
            P3.Click += P3_Click;
            ContextMenu CM = new ContextMenu(new MenuItem[] { P1, P2, P3 });
            // опредление шейпа в который кликнули
            List<HotelRooms> HR = LoadRoomState();
            foreach (HotelRooms GP in HR)
            {
                if (GP.GP.IsVisible(new PointF(e.X, e.Y)))
                {
                    _GP = GP;
                    if (e.Button == MouseButtons.Right)
                    {
                        CM.Show(panel1, Click);
                    }
                    else
                    {
                        //MessageBox.Show($"{GP.HumanReadableRoom} {GP.Building:00}{GP.Flour:00}{GP.Room:00}");
                        if (GP.State == RoomStates.Vacant)
                            (new Поселение(GP, HR) { StartPosition = FormStartPosition.CenterScreen }).Show();
                        if (GP.State == RoomStates.Busy)
                            (new Выселение(HR, GP) { StartPosition = FormStartPosition.CenterScreen }).Show();
                    }
                }
            }
        }

        private void P3_Click(object sender, EventArgs e)
        {
            DateTime Co = DateTime.MinValue;
            int Id = 0;
            using (LockDbDataContext Db = new LockDbDataContext(Convert.ToString(Program.Config["ConnectionString"])))
            {
                Id = Db.Rooms.Where(x => x.Building.Value == _GP.Building && x.Floor.Value == _GP.Floor && x.Room.Value == _GP.Room).First().Id;
                Выбор_карты f = new Выбор_карты()
                {
                    Tag = Id
                };
                if (f.ShowDialog() == DialogResult.OK)
                {
                    var _C = Db.Cards.Where(x => x.Id == Convert.ToInt32(f.Tag));
                    if (_C.Count() > 0)
                    {
                        LostForm lf = new LostForm() { Tag = _C.First().Card.Trim() };
                        if (lf.ShowDialog() == DialogResult.OK)
                        {
                            Card = _C.First().Card;
                            Co = _C.First().Co.Value;
                            Db.Cards.DeleteOnSubmit(_C.First());
                            Db.SubmitChanges();
                        }
                    }
                }
                PregareNewDepo(_GP.Building, _GP.Floor, _GP.Room, Co);
            }
        }

        private string PregareNewDepo(int Building, int Floor, int Room, DateTime Co)
        {
            //выпускаем новую карту номера
            string Card1 = string.Empty;
            if (Program.IshueCard(Building, Floor, Room, Co) == 0)
            {
                byte[] cardbuf = new byte[250];
                Card1 = Program.ReadCard(out cardbuf);
            }
            // меняем депозитную карту
            if (Convert.ToBoolean(Program.Config["Depo"] ?? false))
            {
                IDepo c = (new Depo.Depo()).Connect(Convert.ToString(Program.Config["DepoServerAddress"]));
                if ((Card != string.Empty) && (Card1 != string.Empty))
                {
                    if (c.CheckAmount(Card, out decimal Amount) == Results.Succsess)
                    {
                        if (Amount >= 0)
                        {
                            if (c.ChangeCard(Card, Card1) != Results.Succsess)
                            {
                                MessageBox.Show("Ошибка изменения номера карты");
                            }
                        }
                    }
                }
            }
            return Card1;
        }

        private void P2_Click(object sender, EventArgs e)
        {
            (new Выселение(LoadRoomState(), _GP) { StartPosition = FormStartPosition.CenterScreen }).Show();
        }

        private void P1_Click(object sender, EventArgs e)
        {

            (new Поселение(_GP, LoadRoomState()) { StartPosition = FormStartPosition.CenterScreen }).Show();
        }

        private void Timer1_Tick(object sender, EventArgs e)
        {
            DrawRooms(LoadRoomState());
            if (Convert.ToBoolean(Program.Config["AutoCheckOut"] ?? false))
            {
                using (LockDbDataContext Db = new LockDbDataContext(Convert.ToString(Program.Config["ConnectionString"])))
                {
                    var R = Db.Rooms.Where(x => x.State.Value != (int)RoomStates.Vacant && x.Cards.Max(y => y.Co.Value) < DateTime.Now);
                    if (R.Count() > 0)
                    {
                        foreach (var r in R)
                        {
                            r.State = (int)RoomStates.Vacant;
                        }
                        Db.SubmitChanges();
                    }
                }
            }
        }

        /// <summary>
        /// Info
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Info_Click(object sender, EventArgs e)
        {
            // читаем карту в считывателе
            Card = Program.ReadCard(out byte[] cardbuf);
            //byte[] lockno = Program.ReadLockNo(cardbuf);

            if (Card == string.Empty)
            {
                MessageBox.Show("Перед операцией должна быть положена карта в авторизатор");
                return;
            }
            else
            {
                //string R = Encoding.ASCII.GetString(lockno).Substring(0, 6);
                //if (!int.TryParse(R.Substring(0, 2), out int Building)) { Building = 0; }
                //if (!int.TryParse(R.Substring(2, 2), out int Floor)) { Floor = 0; }
                //if (!int.TryParse(R.Substring(4, 2), out int Room)) { Room = 0; }

                //(new Продление(Card, HR.Find(x => x.Building == Building && x.Floor == Floor && x.Room == Room)?.HumanReadableRoom) { StartPosition = FormStartPosition.CenterScreen }).Show();
                //(new Пополнение_депозита(Card) { StartPosition = FormStartPosition.CenterScreen }).Show();
                (new Info(LoadRoomState(), Card, cardbuf) { StartPosition = FormStartPosition.CenterScreen }).Show();
            }
        }

        private void ToolStripButton3_Click(object sender, EventArgs e)
        {
            (new Staff(LoadRoomState()) { StartPosition = FormStartPosition.CenterScreen }).Show();
        }

        // Стереть карту
        private void ToolStripButton1_Click_1(object sender, EventArgs e)
        {
            (new Стереть_карту() { StartPosition = FormStartPosition.CenterScreen }).Show();
        }

        private void Выезд_Click(object sender, EventArgs e)
        {
            // читаем карту в считывателе
            Card = Program.ReadCard(out byte[] cardbuf);
            // Определяем номер
            byte[] lockno = Program.ReadLockNo(cardbuf);

            if (Card == string.Empty)
            {
                MessageBox.Show("Перед операцией должна быть положена карта в авторизатор");
                return;
            }
            else
            {
                string R = Encoding.ASCII.GetString(lockno).Substring(0, 6);
                if (!int.TryParse(R.Substring(0, 2), out int Building)) { Building = 0; }
                if (!int.TryParse(R.Substring(2, 2), out int Floor)) { Floor = 0; }
                if (!int.TryParse(R.Substring(4, 2), out int Room)) { Room = 0; }
                string HumanReadableRoom = LoadRoomState()?.Find(x => x.Building == Building && x.Floor == Floor && x.Room == Room)?.HumanReadableRoom;

                (new Выселение(LoadRoomState(), LoadRoomState()?.Find(x => x.Building == Building && x.Floor == Floor && x.Room == Room)) { StartPosition = FormStartPosition.CenterScreen }).Show();
            }
        }

        private void Пополнение_депозита_Click(object sender, EventArgs e)
        {
            // читаем карту в считывателе
            Card = Program.ReadCard(out byte[] cardbuf);
            //byte[] lockno = Program.ReadLockNo(cardbuf);

            if (Card == string.Empty)
            {
                MessageBox.Show("Перед операцией должна быть положена карта в авторизатор");
                return;
            }
            else
            {
                //string R = Encoding.ASCII.GetString(lockno).Substring(0, 6);
                //if (!int.TryParse(R.Substring(0, 2), out int Building)) { Building = 0; }
                //if (!int.TryParse(R.Substring(2, 2), out int Floor)) { Floor = 0; }
                //if (!int.TryParse(R.Substring(4, 2), out int Room)) { Room = 0; }

                //(new Продление(Card, HR.Find(x => x.Building == Building && x.Floor == Floor && x.Room == Room)?.HumanReadableRoom) { StartPosition = FormStartPosition.CenterScreen }).Show();
                (new Пополнение_депозита(Card) { StartPosition = FormStartPosition.CenterScreen }).Show();
            }

        }

        private void Возврат_депозита_Click(object sender, EventArgs e)
        {
            // читаем карту в считывателе
            Card = Program.ReadCard(out byte[] cardbuf);
            if (Card == string.Empty)
            {
                MessageBox.Show("Перед операцией должна быть положена карта в авторизатор");
                return;
            }
            else
            {
                (new Возврат_депозита(Card) { StartPosition = FormStartPosition.CenterScreen }).Show();
            }
        }

        /// <summary>
        /// Продление
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Prolongation_Click(object sender, EventArgs e)
        {
            // читаем карту в считывателе
            Card = Program.ReadCard(out byte[] cardbuf);
            byte[] lockno = Program.ReadLockNo(cardbuf);

            if (Card == string.Empty)
            {
                MessageBox.Show("Перед операцией должна быть положена карта в авторизатор");
                return;
            }
            else
            {
                string R = Encoding.ASCII.GetString(lockno).Substring(0, 6);
                if (!int.TryParse(R.Substring(0, 2), out int Building)) { Building = 0; }
                if (!int.TryParse(R.Substring(2, 2), out int Floor)) { Floor = 0; }
                if (!int.TryParse(R.Substring(4, 2), out int Room)) { Room = 0; }
                string HumanReadableRoom = LoadRoomState()?.Find(x => x.Building == Building && x.Floor == Floor && x.Room == Room)?.HumanReadableRoom;
                (new Продление(Card, HumanReadableRoom) { StartPosition = FormStartPosition.CenterScreen }).Show();
            }
        }

        // Перевыпуск карты для управления депозитом (не номерной)
        private void ToolStripButton5_Click(object sender, EventArgs e)
        {
            DateTime Co = DateTime.MinValue;
            using (LockDbDataContext Db = new LockDbDataContext(Convert.ToString(Program.Config["ConnectionString"])))
            {
                var I = Db.Cards.Where(x => !x.RoomId.HasValue);
                if (I.Count() > 0)
                {
                    SelectDepo f = new SelectDepo() { ShowIcon = false, ShowInTaskbar = false, StartPosition = FormStartPosition.CenterScreen, Tag = I };
                    if (f.ShowDialog() == DialogResult.OK)
                    {
                        var _C = Db.Cards.Where(x => x.Id == Convert.ToInt32(f.Tag));
                        if (_C.Count() > 0)
                        {
                            Card = _C.First().Card;
                            Co = _C.First().Co.Value;
                            //_GP.Building = 99;
                            //_GP.Floor = 99;
                            //_GP.Room = 99;
                            //_C.First().Co = DateTime.Now;
                            string Card1 = PregareNewDepo(99, 99, 99, Co);
                            _C.First().Card = Card1;
                            Db.SubmitChanges();
                        }
                    }
                }
            }
        }
    }
}
