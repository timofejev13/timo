﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace gorodnitsev_Redaktor
{
    public partial class Form1 : Form
    {
        bool drawing;
        int historyCounter=0;

        Image imgOriginal;
        GraphicsPath currentPath;
        Point oldLocation;
        public static Pen currentPen;
        Color historyColor;
        List<Image> History;

        int figuri = 0;
        int locallX = 0;
        int locallY = 0;
        int locallXO = 0;
        int locallXY = 0;

        bool bucket = false;

        public Form1()
        {
            InitializeComponent();
            drawing = false;
            currentPen = new Pen(Color.Black);
            currentPen.Width = trackBar1.Value;
            picDrawingSurface.MouseDown += PicDrawingSurface_MouseDown1;
            picDrawingSurface.MouseUp += PicDrawingSurface_MouseUp;
            picDrawingSurface.MouseMove += PicDrawingSurface_MouseMove;
            History = new List<Image>();
            History.Add(new Bitmap(655, 416));

            trackBar2.Minimum = -2;
            trackBar2.Maximum = 10;


        }

        private void PicDrawingSurface_MouseMove(object sender, MouseEventArgs e)
        {
            label1.Text = e.X.ToString() + ", " + e.Y.ToString();
            if (drawing && !bucket)
            {
                if (figuri == 0)
                {
                    Graphics g = Graphics.FromImage(picDrawingSurface.Image);
                    currentPath.AddLine(oldLocation, e.Location);
                    g.DrawPath(currentPen, currentPath);
                    oldLocation = e.Location;
                    g.Dispose();
                    picDrawingSurface.Invalidate();
                }
                else
                {
                    locallX = oldLocation.X;
                    locallY = oldLocation.Y;
                    locallXO = e.Location.X-oldLocation.X;
                    locallXY = e.Location.Y-oldLocation.Y;
                }
                
            }
            
        }
        void FloodFill(Bitmap bitmap, int x, int y, Color color)
        {
            BitmapData data = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadWrite, PixelFormat.Format32bppArgb);
            int[] bits = new int[data.Stride / 4 * data.Height];
            Marshal.Copy(data.Scan0, bits, 0, bits.Length);

            LinkedList<Point> check = new LinkedList<Point>();
            int floodTo = color.ToArgb();
            int floodFrom = bits[x + y * data.Stride / 4];
            bits[x + y * data.Stride / 4] = floodTo;

            if (floodFrom != floodTo)
            {
                check.AddLast(new Point(x, y));
                while (check.Count > 0)
                {
                    Point cur = check.First.Value;
                    check.RemoveFirst();

                    foreach (Point off in new Point[] {
                new Point(0, -1), new Point(0, 1),
                new Point(-1, 0), new Point(1, 0)})
                    {
                        Point next = new Point(cur.X + off.X, cur.Y + off.Y);
                        if (next.X >= 0 && next.Y >= 0 &&
                            next.X < data.Width &&
                            next.Y < data.Height)
                        {
                            if (bits[next.X + next.Y * data.Stride / 4] == floodFrom)
                            {
                                check.AddLast(next);
                                bits[next.X + next.Y * data.Stride / 4] = floodTo;
                            }
                        }
                    }
                }
            }

            Marshal.Copy(bits, 0, data.Scan0, bits.Length);
            bitmap.UnlockBits(data);

            picDrawingSurface.Image = bitmap;
        }

        private void PicDrawingSurface_MouseUp(object sender, MouseEventArgs e)
        {
            if (bucket)
            {
                FloodFill(new Bitmap(picDrawingSurface.Image), oldLocation.X, oldLocation.Y, historyColor);
            }
            else if (figuri == 1)
            {
                Graphics g = Graphics.FromImage(picDrawingSurface.Image);
                currentPath.AddRectangle(new Rectangle(locallX, locallY, locallXO, locallXY));
                g.DrawPath(currentPen, currentPath);
                oldLocation = e.Location;
                g.Dispose();
                picDrawingSurface.Invalidate();
            }
            else if(figuri == 2)
            {
                Graphics g = Graphics.FromImage(picDrawingSurface.Image);
                currentPath.AddEllipse(locallX, locallY, locallXO, locallXY);
                g.DrawPath(currentPen, currentPath);
                oldLocation = e.Location;
                g.Dispose();
                picDrawingSurface.Invalidate();

            }
            else if (figuri == 3)
            {
                Graphics g = Graphics.FromImage(picDrawingSurface.Image);
                g.DrawLine(currentPen, new Point(locallX, locallY),new Point(locallX+ locallXO, locallY+ locallXY));
                oldLocation = e.Location;
                g.Dispose();
                picDrawingSurface.Invalidate();

            }

            History.RemoveRange(historyCounter + 1, History.Count - historyCounter - 1);
            History.Add(new Bitmap(picDrawingSurface.Image));
            if (historyCounter + 1 < 10) historyCounter++;
            if (History.Count - 1 == 10) History.RemoveAt(0);
            drawing = false;
            try
            {
                currentPath.Dispose();
            }
            catch { };
            imgOriginal = picDrawingSurface.Image;
        }
        Image Zoom(Image img, int size)
        {
            Bitmap pic = new Bitmap(img, img.Width + (img.Width * size / 10), img.Height + (img.Height * size / 10));
            Graphics g = Graphics.FromImage(pic);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            return pic;
        }

        private void PicDrawingSurface_MouseDown1(object sender, MouseEventArgs e)
        {
            if(picDrawingSurface.Image == null)
            {
                MessageBox.Show("Сначала создайте новый файл!");
                return;
            }
            if (e.Button == MouseButtons.Left)
            {
                drawing = true;
                oldLocation = e.Location;
                currentPath = new GraphicsPath();
            }
        }
        private void TrackBar2_Scroll(object sender, EventArgs e)
        {
        }


        private void ToolStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void AboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Информация о приложении и разработчике");
        }

        private void OpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog OP = new OpenFileDialog();
            OP.Filter= "JPEG Image|*.jpg|Bitmap Image|*.bmp|GIF Image|*.gif|PNG Image|*.png";
            OP.Title = "Open an Image File";
            OP.FilterIndex = 1;

            if (OP.ShowDialog() != DialogResult.Cancel)
                picDrawingSurface.Load(OP.FileName);

            picDrawingSurface.AutoSize = true;
        }

        private void NewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            History.Clear();
            historyCounter = 0;


            if (picDrawingSurface.Image != null)
            {
                var result = MessageBox.Show("Сохранить текущее изображение перед созданием нового рисунка?", "Передупреждение", MessageBoxButtons.YesNoCancel);

                switch (result)
                {
                    case DialogResult.No: break;
                    case DialogResult.Yes: SaveToolStripMenuItem_Click(sender, e); break;
                    case DialogResult.Cancel: return;
                }

            }
            Bitmap pic = new Bitmap(655, 416);
            picDrawingSurface.Image = pic;
            History.Add(new Bitmap(picDrawingSurface.Image));
        }

        private void SaveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            SaveFileDialog SaveDlg = new SaveFileDialog();
            SaveDlg.Filter = "JPEG Image|*.jpg|Bitmap Image|*.bmp|GIF Image|*.gif|PNG Image|*.png";
            SaveDlg.Title = "Save an Image File";
            SaveDlg.FilterIndex = 4;
            

            SaveDlg.ShowDialog();
            if (SaveDlg.FileName != "")
            {
                System.IO.FileStream fs = (System.IO.FileStream)SaveDlg.OpenFile();

                switch (SaveDlg.FilterIndex)
                {
                    case 1:
                        this.picDrawingSurface.Image.Save(fs, ImageFormat.Jpeg);
                        break;

                    case 2:
                        this.picDrawingSurface.Image.Save(fs, ImageFormat.Bmp);
                        break;
                    case 3:
                        this.picDrawingSurface.Image.Save(fs, ImageFormat.Gif);
                        break;
                    case 4:
                        this.picDrawingSurface.Image.Save(fs, ImageFormat.Png);
                        break;
                }

                fs.Close();
            }
            Graphics g = Graphics.FromImage(picDrawingSurface.Image);
            g.Clear(Color.White);
            g.DrawImage(picDrawingSurface.Image, 0, 0, 750, 500);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void ToolStripButton5_Click(object sender, EventArgs e)
        {
            if (picDrawingSurface.Image != null)
            {
                var result = MessageBox.Show("Сохранить текущее изображение перед выходом?", "Передупреждение", MessageBoxButtons.YesNoCancel);

                switch (result)
                {
                    case DialogResult.No: Application.Exit(); break;
                    case DialogResult.Yes: SaveToolStripMenuItem_Click(sender, e); Application.Exit(); break; 
                    case DialogResult.Cancel: return;

                }

            }
            else
            {
                Application.Exit();
            }
            
        }

        private void ToolStripButton1_Click(object sender, EventArgs e)
        {


            if (picDrawingSurface.Image != null)
            {
                var result = MessageBox.Show("Сохранить текущее изображение перед созданием нового рисунка?", "Передупреждение", MessageBoxButtons.YesNoCancel);

                switch (result)
                {
                    case DialogResult.No: break;
                    case DialogResult.Yes: SaveToolStripMenuItem_Click(sender, e); break;
                    case DialogResult.Cancel: return;
                }

            }
            Bitmap pic = new Bitmap(655, 416);
            picDrawingSurface.Image = pic;
        }

        private void ToolStripButton2_Click(object sender, EventArgs e)
        {
            SaveFileDialog SaveDlg = new SaveFileDialog();
            SaveDlg.Filter = "JPEG Image|*.jpg|Bitmap Image|*.bmp|GIF Image|*.gif|PNG Image|*.png";
            SaveDlg.Title = "Save an Image File";
            SaveDlg.FilterIndex = 4;

            SaveDlg.ShowDialog();
            if (SaveDlg.FileName != "")
            {
                System.IO.FileStream fs = (System.IO.FileStream)SaveDlg.OpenFile();

                switch (SaveDlg.FilterIndex)
                {
                    case 1:
                        this.picDrawingSurface.Image.Save(fs, ImageFormat.Jpeg);
                        break;

                    case 2:
                        this.picDrawingSurface.Image.Save(fs, ImageFormat.Bmp);
                        break;
                    case 3:
                        this.picDrawingSurface.Image.Save(fs, ImageFormat.Gif);
                        break;
                    case 4:
                        this.picDrawingSurface.Image.Save(fs, ImageFormat.Png);
                        break;
                }

                fs.Close();
            }
        }

        private void ToolStripButton3_Click(object sender, EventArgs e)
        {
            OpenFileDialog OP = new OpenFileDialog();
            OP.Filter = "JPEG Image|*.jpg|Bitmap Image|*.bmp|GIF Image|*.gif|PNG Image|*.png";
            OP.Title = "Open an Image File";
            OP.FilterIndex = 1;

            if (OP.ShowDialog() != DialogResult.Cancel)
                picDrawingSurface.Load(OP.FileName);

            picDrawingSurface.AutoSize = true;
        }

        private void Label1_Click(object sender, EventArgs e)
        {

        }

        private void TrackBar1_Scroll(object sender, EventArgs e)
        {
            currentPen.Width = trackBar1.Value;
        }

        private void UndoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (History.Count != 0 && historyCounter != 0)
            {
                picDrawingSurface.Image = new Bitmap(History[--historyCounter]);
            }
            else MessageBox.Show("История пуста");
        }

        private void RenoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (historyCounter < History.Count - 1)
            {
                picDrawingSurface.Image = new Bitmap(History[++historyCounter]);
            }
            else MessageBox.Show("История пуста");
        }

        private void SolidToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentPen.DashStyle = DashStyle.Solid;

            solidToolStripMenuItem.Checked = true;
            dotToolStripMenuItem.Checked = false;
            dashDotDotToolStripMenuItem.Checked = false;
            fuguresToolStripMenuItem.Checked = false;
            eclipse.Checked = false;
            line.Checked = false;
            figuri = 0;

        }

        private void DotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentPen.DashStyle = DashStyle.Dot;

            solidToolStripMenuItem.Checked = false;
            dotToolStripMenuItem.Checked = true;
            dashDotDotToolStripMenuItem.Checked = false;
            fuguresToolStripMenuItem.Checked = false;
            eclipse.Checked = false;
            line.Checked = false;
            figuri = 0;
        }

        private void DashDotDotToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentPen.DashStyle = DashStyle.DashDotDot;

            solidToolStripMenuItem.Checked = false;
            dotToolStripMenuItem.Checked = false;
            dashDotDotToolStripMenuItem.Checked = true;
            fuguresToolStripMenuItem.Checked = false;
            eclipse.Checked = false;
            line.Checked = false;
            figuri = 0;
        }

        private void ToolStripButton4_Click(object sender, EventArgs e)
        {
            Colors f = new Colors(historyColor);
            f.Owner = this;
            f.ShowDialog();
        }

        private void ColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Colors f = new Colors(historyColor);
            f.Owner = this;
            f.ShowDialog();
        }

        private void FuguresToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void FuguresToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            currentPen.DashStyle = DashStyle.Solid;

            solidToolStripMenuItem.Checked = false;
            dotToolStripMenuItem.Checked = false;
            dashDotDotToolStripMenuItem.Checked = false;
            fuguresToolStripMenuItem.Checked = true;
            eclipse.Checked = false;
            line.Checked = false;
            figuri = 1;
        }
        private void TrackBar2_Scroll_1(object sender, EventArgs e)
        {
            picDrawingSurface.Image = Zoom(imgOriginal, trackBar2.Value);
        }

        private void EclipseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentPen.DashStyle = DashStyle.Solid;

            solidToolStripMenuItem.Checked = false;
            dotToolStripMenuItem.Checked = false;
            dashDotDotToolStripMenuItem.Checked = false;
            fuguresToolStripMenuItem.Checked = false;
            eclipse.Checked = true;
            line.Checked = false;
            figuri = 2;
        }

        private void Line_Click(object sender, EventArgs e)
        {
            currentPen.DashStyle = DashStyle.Solid;

            solidToolStripMenuItem.Checked = false;
            dotToolStripMenuItem.Checked = false;
            dashDotDotToolStripMenuItem.Checked = false;
            fuguresToolStripMenuItem.Checked = false;
            eclipse.Checked = false;
            line.Checked = true;
            figuri = 3;
        }

        private void ToolStripButton6_Click(object sender, EventArgs e)
        {
            bucket = true;
            Colors AddRec = new Colors(historyColor);
            AddRec.Owner = this;
            AddRec.ShowDialog();
        }
    }
}
