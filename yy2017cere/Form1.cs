using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using NLog;

namespace yy2017cere
{
    public partial class Form1 : Form
    {
        public string ConString { get; set; } = "Data Source='192.168.1.55';User Id='root';Password='123456';Database=crawler;charset=utf8mb4";
        public const string DatabaseName = "crawler";
        public const string DataTableName = "yy2017cere";
        private Logger logger = LogManager.GetLogger("yy2017");
        public Form1()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;//使别的线程可以访问
        }

        private void startupdata()
        {
            var t = new Thread(updata);
            t.Start();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            startupdata();

        }

        private string phase = "1";
        private string round = "1";

        private string curYY;

        private JObject getInfo(string cid)
        {
            var http = new HttpHelper();
            var item = new HttpItem
            {
                Url = "http://actgw-static.yy.com/getUnionFeatsTotalRank/?callback=getUnionFeatsTotalRank&data={\"__yyp_max__\":190,\"__yyp_min__\":1607,\"num\":500,\"actid\":20171201,\"extendInfo\":{\"phase\":\"" + phase + "\",\"round\":\"" + round + "\",\"channelId\":\"" + cid + "\",\"terminal\":\"4\"},\"type\":15}&_=1512098004532",
                Method = "GET",

            };
            var html = http.GetHtml(item).Html;

            html = html.Replace("getUnionFeatsTotalRank(", string.Empty);
            html = html.Replace(");", string.Empty);

            return JObject.Parse(html); ;
        }

        private JObject getInfo(int cid)
        {
            return getInfo(cid.ToString());
        }

        private void getCurRound()
        {
            var http = new HttpHelper();
            var item = new HttpItem
            {
                Url = "http://actgw-static.yy.com/getCurrPhaseAndRound1/?callback=getCurrPhaseAndRound1&data={\"__yyp_max__\":190,\"__yyp_min__\":1607,\"num\":10,\"actid\":20171201,\"extendInfo\":{},\"type\":0}&_=1512280191502",
                Method = "GET",

            };


            var html = http.GetHtml(item).Html;

            html = html.Replace("getCurrPhaseAndRound1(", string.Empty);
            html = html.Replace(");", string.Empty);
            var json = JObject.Parse(html);
            phase = json["extendInfo"]["cCurrphase"].Value<string>();
            round = json["extendInfo"]["cCurround"].Value<string>();
            toolStripLabel2.Text = $"phase:{phase},round:{round}";
        }

        private void updata()
        {
            var sw = new Stopwatch();
            sw.Start();
            listView1.BeginUpdate();
            listView1.Items.Clear();

            MySqlConnection con = null;
            string timestamp = null;

            MySqlTransaction tran = null;

            if (checkBox1.Checked)
            {
                con = new MySqlConnection(ConString);
                con.Open();
                timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                tran = con.BeginTransaction();
            }

            getCurRound();

            foreach (string cid in checkedListBox1.CheckedItems)
            {

                var json = getInfo(cid);

                foreach (var chid in json["topChid"])
                {
                    var rank = chid["rank"].Value<string>();
                    var nick = chid["anNick"].Value<string>();
                    nick = Encoding.UTF8.GetString(Convert.FromBase64String(nick));
                    var num = chid["anNumber"].Value<string>();
                    var yy = chid["yy"].Value<string>();
                    var shortroom = chid["shortroom"].Value<string>();

                    if (yy == curYY)
                    {
                        var url = chid["anImage"].Value<string>();

                        if (pictureBox1.ImageLocation != url)
                        {
                            pictureBox1.ImageLocation = url;
                            pictureBox1.Update();
                        }

                        label1.Text = $"排名:{rank}\r\n{nick}\r\n总票数:{num}";
                    }

                    var lvi = new ListViewItem(rank);
                    lvi.SubItems.Add(nick);
                    lvi.SubItems.Add(chid["islive"].Value<string>());
                    lvi.SubItems.Add(num);
                    lvi.SubItems.Add(cid);
                    lvi.SubItems.Add(yy);
                    lvi.SubItems.Add(shortroom);
                    lvi.SubItems.Add(round);
                    lvi.SubItems.Add(phase);

                    lvi.Tag = yy;
                    listView1.Items.Add(lvi);

                    if (checkBox1.Checked)
                    {
                        var keys = "timestamp,rank,nick,islive,number,cid,yy,round,phase";
                        var values = $"\"{timestamp}\"";
                        values += $",\"{rank}\"";
                        values += $",\"{nick}\"";
                        values += $",\"{chid["islive"].Value<string>()}\"";
                        values += $",\"{num}\"";
                        values += $",\"{cid}\"";
                        values += $",\"{yy}\"";
                        values += $",\"{round}\"";
                        values += $",\"{phase}\"";

                        var cmd = con.CreateCommand();
                        cmd.Transaction = tran;
                        cmd.CommandText = $"INSERT INTO {DataTableName} ({keys}) VALUES({values});";
                        cmd.ExecuteNonQuery();
                    }
                }

            }

            if (checkBox1.Checked)
            {
                tran.Commit();
                con.Close();
            }

            listView1.EndUpdate();
            sw.Stop();
            toolStripLabel1.Text = "time:" + sw.ElapsedMilliseconds / 1000.0 + "秒";



        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            startupdata();
        }

        private void freezeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0) return;
            curYY = listView1.SelectedItems[0].Tag.ToString();
            startupdata();
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            label3.Text = trackBar1.Value + "秒";
            timer1.Interval = trackBar1.Value * 1000;
            toolStripProgressBar1.Maximum = trackBar1.Value;
            toolStripProgressBar1.Value = trackBar1.Value;
        }

        private void timer2_Tick(object sender, EventArgs e)
        {

            toolStripProgressBar1.Value--;
            if (toolStripProgressBar1.Value <= 0)
                toolStripProgressBar1.Value = toolStripProgressBar1.Maximum;
        }
    }
}
