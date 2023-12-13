using System.Diagnostics;
using System.Drawing.Imaging;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Xml;
using static System.Windows.Forms.LinkLabel;

namespace BlinkMeasure
{
    public partial class Form1 : Form
    {
        private static int floaterBorderWidth = 2;
        private static Pen floaterBorderPen = new Pen(Color.Red, floaterBorderWidth)
        {
            DashStyle = System.Drawing.Drawing2D.DashStyle.Dash,
        };

        private Form? floater;
        private Point pt;
        private Size sz;
        private RGB? lastAvg;
        private double threshold;
        private int count;
        //private Bitmap? lastImg;
        private DateTime startTime;
        private List<LineItem> lines = new List<LineItem>();
        private bool measuring = false;
        private bool stop = true;
        private string? folder;
        private bool deltaFirst;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            numLeft.Value = decimal.Parse((string)Application.UserAppDataRegistry.GetValue("left", "100"));
            numTop.Value = decimal.Parse((string)Application.UserAppDataRegistry.GetValue("top", "100"));
            numWidth.Value = decimal.Parse((string)Application.UserAppDataRegistry.GetValue("width", "100"));
            numHeight.Value = decimal.Parse((string)Application.UserAppDataRegistry.GetValue("height", "100"));
            numDelta.Value = decimal.Parse((string)Application.UserAppDataRegistry.GetValue("trigger", "20"));
            txtFolder.Text = (string)Application.UserAppDataRegistry.GetValue("folder", "c:\\blink-measure\\1");
            radFirst.Checked = bool.Parse((string)Application.UserAppDataRegistry.GetValue("deltaFirst", "true"));
            radLast.Checked = !radFirst.Checked;

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.UserAppDataRegistry.SetValue("left", numLeft.Value);
            Application.UserAppDataRegistry.SetValue("top", numTop.Value);
            Application.UserAppDataRegistry.SetValue("width", numWidth.Value);
            Application.UserAppDataRegistry.SetValue("height", numHeight.Value);
            Application.UserAppDataRegistry.SetValue("trigger", numDelta.Value);
            Application.UserAppDataRegistry.SetValue("folder", txtFolder.Text);
            Application.UserAppDataRegistry.SetValue("deltaFirst", radFirst.Checked.ToString());
        }


        private void button1_Click(object sender, EventArgs e)
        {
            this.createFloater();
        }

        private void createFloater()
        {
            this.pt = new Point((int)numLeft.Value, (int)numTop.Value);
            this.sz = new Size((int)numWidth.Value, (int)numHeight.Value);

            if (this.floater == null)
            {
                this.floater = new Form()
                {
                    BackColor = Color.Blue,
                    ShowIcon = false,
                    ShowInTaskbar = false,
                    MaximizeBox = false,
                    MinimizeBox = false,
                    ControlBox = false,
                    StartPosition = FormStartPosition.Manual,
                    Text = "",
                    FormBorderStyle = FormBorderStyle.None,
                    HelpButton = false,
                    TopMost = true,
                    AllowTransparency = true,
                    TransparencyKey = Color.Blue,
                };

                this.floater.Paint += Floater_Paint;
            }

            if (!this.floater.Visible)
                this.floater.Show();

            Application.DoEvents();

            this.floater.Left = pt.X - floaterBorderWidth;
            this.floater.Top = pt.Y - floaterBorderWidth;
            this.floater.Width = sz.Width + (floaterBorderWidth * 2);
            this.floater.Height = sz.Height + (floaterBorderWidth * 2);

            this.floater.Invalidate();

            Application.DoEvents();

            var img = this.grabFrame();

            var c = this.getAverageRGB(img);
            txtR.Text = c.r.ToString();
            txtG.Text = c.g.ToString();
            txtB.Text = c.b.ToString();

            frameOne.BackgroundImage = img;
            frameTwo.BackgroundImage = null;

        }

        private void Floater_Paint(object? sender, PaintEventArgs e)
        {
            e.Graphics.DrawRectangle(floaterBorderPen, 1, 1, this.floater!.Width - floaterBorderWidth, this.floater!.Height - floaterBorderWidth);
        }

        private Bitmap grabFrame()
        {
            // copy pixels
            var img = new Bitmap(sz.Width, sz.Height);
            using (var gr = Graphics.FromImage(img))
            {
                gr.CopyFromScreen(pt.X, pt.Y, 0, 0, img.Size, CopyPixelOperation.SourceCopy);
            }

            return img;
        }

        private RGB getAverageRGB(Bitmap img)
        {
            // get average RGB values
            var c = new RGB();
            for (var x = 0; x < sz.Width; x++)
            {
                for (var y = 0; y < sz.Height; y++)
                {
                    var p = img.GetPixel(x, y);
                    c.r += p.R;
                    c.g += p.G;
                    c.b += p.B;
                }
            }

            var nn = sz.Width * sz.Height;

            c.r /= nn;
            c.g /= nn;
            c.b /= nn;

            return c;
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            if (this.floater == null) this.createFloater();

            var img = this.grabFrame();

            if (this.lastAvg == null)
            {
                frameOne.BackgroundImage = img;
                this.lastAvg = this.getAverageRGB(img);
                return;
            }

            // diff threshold 

            frameOne.BackgroundImage = frameTwo.BackgroundImage;
            frameTwo.BackgroundImage = img;
            var avg = this.getAverageRGB(img);

            var delta = new RGB()
            {
                r = Math.Abs(lastAvg.r - avg.r),
                g = Math.Abs(lastAvg.g - avg.g),
                b = Math.Abs(lastAvg.b - avg.b),
            };

            txtR.Text = delta.r.ToString();
            txtG.Text = delta.g.ToString();
            txtB.Text = delta.b.ToString();


            txtDiff.Text = delta.sum.ToString();

            this.lastAvg = avg;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.startStuff();
            this.Text = "Trigger testing...";
            this.measuring = false;
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.button4.Enabled = false;
            this.button5.Enabled = true;
            this.measuring = true;
            this.startStuff();
            this.Text = "Measuring ...";

        }


        private void button5_Click(object sender, EventArgs e)
        {
            this.button5.Enabled = false;
            this.button4.Enabled = true;
            this.stop = true;
        }


        private void startStuff()
        {
            if (this.floater == null)
                this.createFloater();

            this.stop = false;
            this.lines.Clear();
            this.count = 0;
            this.threshold = (double)numDelta.Value;
            this.startTime = DateTime.Now;
            this.folder = txtFolder.Text;
            this.deltaFirst = radFirst.Checked;

            var img = this.grabFrame();
            this.lastAvg = this.getAverageRGB(img);
            frameOne.BackgroundImage = img;
            frameTwo.BackgroundImage = null;

            Task.Run(new Action(this.doStuff));
        }

        private void doStuff()
        {
            ++this.count;
            var img = this.grabFrame();
            var avg = this.getAverageRGB(img);

            var delta = new RGB()
            {
                r = Math.Abs(lastAvg!.r - avg.r),
                g = Math.Abs(lastAvg!.g - avg.g),
                b = Math.Abs(lastAvg!.b - avg.b),
            };

            var line = new LineItem()
            {
                duration = DateTime.Now.Subtract(this.startTime),
                delta = delta.sum,
            };
            this.lines.Add(line);

            if (delta.sum > this.threshold || this.lines.Count == 1)
            {
                line.img = img;

                if (!measuring)
                {
                    this.stop = true;
                }
            }

            if (!deltaFirst)
                lastAvg = avg;

            if (stop)
            {
                if (this.measuring)
                    this.endStuff();

                this.Invoke(new Action(() =>
                {
                    this.Text = $"count: {this.count}, {DateTime.Now.Subtract(this.startTime)}";

                    txtR.Text = delta.r.ToString();
                    txtG.Text = delta.g.ToString();
                    txtB.Text = delta.b.ToString();

                    txtDiff.Text = delta.sum.ToString();
                    frameTwo.BackgroundImage = img;
                }));
            }
            else
            {
                this.BeginInvoke(new Action(() =>
                {
                    txtDiff.Text = delta.sum.ToString();
                }));

                this.doStuff();
            }
        }

        private void endStuff()
        {
            Directory.CreateDirectory(this.folder!);
            var str = new StringBuilder();
            str.AppendLine("duration (seconds),delta,image path");

            foreach (var line in this.lines)
            {
                var duration = line.duration.TotalSeconds.ToString("N3"); // }." + (line.duration.Milliseconds / 1000.0).ToString("N3");
                var imgPath = "";
                if (line.img != null)
                {
                    imgPath = Path.Combine(this.folder!, $"img-{duration}.png");
                    line.img.Save(imgPath, ImageFormat.Png);
                }

                str.AppendLine($"{duration},{line.delta},{imgPath}");
            }

            var csvPath = Path.Combine(this.folder!, $"data.csv");
            File.WriteAllText(csvPath, str.ToString());

            this.Invoke(new Action(() =>
            {
                this.Text = "Stopped";

                var info = new ProcessStartInfo(this.folder!);
                info.UseShellExecute = true;
                Process.Start(info);
            }));
        }
    }

    class LineItem
    {
        public TimeSpan duration;
        public double delta;
        public Bitmap? img;

        public override string ToString()
        {
            return $"{duration.TotalMilliseconds} / {delta} / {img != null}";
        }
    }

    class RGB
    {
        public double r;
        public double g;
        public double b;

        public double sum { get => this.r + this.g + this.b; }
    }
}