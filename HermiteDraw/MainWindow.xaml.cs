using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;

using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Drawing;
using System.Windows.Forms;

namespace HermiteDraw
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    class myColor
    {
        byte _R;
        public byte R { get { return _R; } set { _R = value; } }

        byte _G;
        public byte G { get { return _G; } set { _G = value; } }

        byte _B;
        public byte B { get { return _B; } set { _B = value; } }

        public myColor(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }
    }

    public partial class MainWindow : Window
    {
        //  PointF p0, t0, p1, t1;

        List<PointF> pointP;
        List<PointF> pointT;
        List<float> thickness;
        List<myColor> colors;
        bool draw;

        Pen previewPBlue = new Pen(Color.FromArgb(120, 37, 24, 209));
        Pen pBlue = new Pen(Color.FromArgb(255, 37, 24, 209));
        Pen previewPRed = new Pen(Color.FromArgb(120, 255, 0, 0));
        Pen pRed = new Pen(Color.Red);
        Brush previewBRed = new SolidBrush(Color.FromArgb(120, 255, 0, 0));
        Brush red = new SolidBrush(Color.Red);
        Brush previewBGreen = new SolidBrush(Color.FromArgb(120, 0, 255, 0));
        Brush green = new SolidBrush(Color.Green);



        public MainWindow()
        {

            pointP = new List<PointF>();
            pointT = new List<PointF>();
            thickness = new List<float>();
            colors = new List<myColor>();

            selectedLines = new List<int>();
            InitializeComponent();
            ColorPicker.R = 0; ColorPicker.G = 0; ColorPicker.B = 255;
            draw = false;


        }
        Graphics g;

        private void Canvas_Paint(object sender, PaintEventArgs e)
        {

            //TODO: második kattintásra a T pont mozgatása. DONE
            //TODO: színválasztó implementálása.DONE
            //TODO: zoom
            //TODO: vonalakra lehessen duplán kattintani. legyen egyértelmű hogy kivan jelölve. színváltoztatás, vastagság állítása(görgővel is).
            //TODO: contextmenu canvason néhol bugos. színválasztás.
            //TODO: pontok törlése. DONE
            //TODO: save-load.
            //TODO több pont kijelölése DONE
            //geciszarul néz ki
            g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            if (draw && pointP.Count == 1)
            {
                g.DrawLine(pRed, pointP[0], pointT[0]);
                g.FillRectangle(red, pointP[0].X - 3, pointP[0].Y - 3, 6, 6);
                g.FillRectangle(green, pointT[0].X - 3, pointT[0].Y - 3, 6, 6);

            }

            if (draw)
            {
                for (int i = 1; i < pointP.Count; i++)
                {

                    double a = 0;
                    double b = 1;
                    double h = (b - a) / 100;
                    double t = a;
                    PointF ph1, ph2;
                    ph1 = HermitePoint(t, pointP[i - 1], pointP[i], pointT[i - 1], pointT[i]);
                    while (t < b)
                    {
                        t += h;
                        ph2 = HermitePoint(t, pointP[i - 1], pointP[i], pointT[i - 1], pointT[i]);
                        if (previewPoint && i == foundP) //more transparent curve when previewing, moving a point.
                        {
                            g.DrawLine(new Pen(Color.FromArgb(120, colors[i-1].R,colors[i-1].G,colors[i-1].B), thickness[i - 1]), ph1, ph2);
                        }
                        else if (pickingLine && selectedLines.Contains(i-1))//making a curve thicker when clicking on it
                        {
                            g.DrawLine(new Pen(Color.FromArgb(255, colors[i - 1].R, colors[i - 1].G, colors[i - 1].B), thickness[i - 1] + 2), ph1, ph2);
                        }
                        else //drawing the line
                            g.DrawLine(new Pen(Color.FromArgb(255, colors[i - 1].R, colors[i - 1].G, colors[i - 1].B), thickness[i-1]), ph1, ph2);
                        

                        ph1 = ph2;
                    }


                    g.DrawLine(pRed, pointP[i - 1], pointT[i - 1]);
                    g.DrawLine(pRed, pointP[i], pointT[i]);
                    g.FillRectangle(red, pointP[i - 1].X - 3, pointP[i - 1].Y - 3, 6, 6);
                    g.FillRectangle(red, pointP[i].X - 3, pointP[i].Y - 3, 6, 6);
                    g.FillRectangle(green, pointT[i - 1].X - 3, pointT[i - 1].Y - 3, 6, 6);
                    g.FillRectangle(green, pointT[i].X - 3, pointT[i].Y - 3, 6, 6);
                }
            }

        }

        int found = -1;
        //0 found no P or T
        //1 found P
        //2 found T
        int foundP = -1;
        //Index of P in the List
        int foundT = -1;
        //index of T in the list
        List<int> selectedLines;
        private void Canvas_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            PointF mouse = new PointF(e.X, e.Y);
            if (e.Button == MouseButtons.Right)  //opening context menu
            {
                winFormHost.ContextMenu.IsOpen = true;
                for (int i = 0; i < pointP.Count; i++)
                {

                    if (Find(pointP[i], mouse, 5) || Find(pointT[i], mouse, 5))//checks if user clicked on a point with right click
                    {
                        found = 3;
                        foundP = i;
                        pickingLine = false;
                        if (e.Button == MouseButtons.Right)
                            conMenu.IsEnabled = true;
                        break;

                    }
                }
            }

            else if (!previewPoint)
            {
                previewTPoint = false;
                foundP = -1;

                bool foundCurve = false;

                for (int i = 0; i < pointP.Count - 1; i++) //when the user wants to highlight a curve, this checks which curve is that
                {
                    if (pickingLine)
                    {
                        double a = 0;
                        double b = 1;
                        double h = (b - a) / 100;
                        double t = a;
                        PointF ph1, ph2;
                        ph1 = HermitePoint(t, pointP[i], pointP[i + 1], pointT[i], pointT[i + 1]);
                        while (t < b)
                        {
                            t += h;
                            ph2 = HermitePoint(t, pointP[i], pointP[i + 1], pointT[i], pointT[i + 1]);
                            if (Find(ph1, mouse, 5))
                            {
                                if (selectedLines.Contains(i))
                                    selectedLines.Remove(i);
                                else
                                    selectedLines.Add(i);
                                foundCurve = true;
                                break;
                            }


                            ph1 = ph2;
                        }

                    }
                }
                if (!foundCurve)
                    selectedLines.Clear();
                for (int i = 0; i < pointP.Count; i++)
                {

                    if (Find(pointP[i], mouse, 5))//checks if user clicked on a P point
                    {
                        found = 1;
                        foundP = i;
                        previewPoint = true;
                        pickingLine = false;
                        if (e.Button == MouseButtons.Right)
                            conMenu.IsEnabled = true;
                        break;

                    }
                    else if (Find(pointT[i], mouse, 5))//checks if user clicked on a T point
                    {
                        found = 2;
                        foundT = i;
                        previewPoint = true;
                        pickingLine = false;
                        if (e.Button == MouseButtons.Right)
                            conMenu.IsEnabled = true;
                        break;
                    }
                    
                }
                
            }
            if(isCtrl)
            {
                HermiteButton_Click(sender, new RoutedEventArgs());
                
            }

        }

        private void Canvas_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            //moving existing points
            if (found == 1)
            {
                PointF mouse = new PointF(e.X, e.Y);
                pointP[foundP] = mouse;
            }
            else if (found == 2)
            {
                PointF mouse = new PointF(e.X, e.Y);
                pointT[foundT] = mouse;
            }

            //previewing    
            else if (previewPoint)
            {
                PointF mouse = new PointF(e.X, e.Y);
                pointP[pointP.Count - 1] = mouse;
                pointT[pointT.Count - 1] = new PointF(mouse.X - 30, mouse.Y - 30);
            }
            else if (previewTPoint && !previewPoint)
            {
                PointF mouse = new PointF(e.X, e.Y);
                pointT[pointT.Count - 1] = mouse;
            }
            Canvas.Refresh();
        }


        private void Canvas_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!pickingLine && !isCtrl && e.Button != MouseButtons.Right)
            {
                found = -1;
                foundP = -1;
                foundT = -1;
                previewPoint = false;
                pickingLine = false;
                selectedLines.Clear();
            }

            if (pointP.Count == 1 && !previewTPoint)//this automatically creates a second T and P point after finalizing the first points, without having to press the add the button.
            {
                previewPoint = true;
                previewTPoint = true;
                pointP.Add(new PointF());
                pointT.Add(new PointF());
                thickness.Add(1);
                foundP = pointP.Count - 1;

            }
           
        }



        bool Find(PointF target, PointF mouse, int distance)
        {
            return Math.Abs(target.X - mouse.X) < distance &&
                Math.Abs(target.Y - mouse.Y) < distance;
        }

        double H0(double t)
        {
            return 2 * t * t * t - 3 * t * t + 1;
        }
        double H1(double t)
        {
            return -2 * t * t * t + 3 * t * t;
        }
        double H2(double t)
        {
            return t * t * t - 2 * t * t + t;
        }
        double H3(double t)
        {
            return t * t * t - t * t;
        }

        PointF HermitePoint(double t, PointF p0, PointF p1, PointF t0, PointF t1)
        {
            return new PointF(
                (float)(H0(t) * p0.X + H1(t) * p1.X + H2(t) * t0.X + H3(t) * t1.X),
                (float)(H0(t) * p0.Y + H1(t) * p1.Y + H2(t) * t0.Y + H3(t) * t1.Y));
        }

        void DeleteCurve(int indexOfPToDelete)
        {
            pointP.RemoveAt(indexOfPToDelete);
            pointT.RemoveAt(indexOfPToDelete);
            thickness.RemoveAt(indexOfPToDelete);
            colors.RemoveAt(indexOfPToDelete);
            pickingLine = false;
            Canvas.Refresh();
        }

        bool previewPoint = false;
        bool previewTPoint = false;
        //adding a hermite curve button
        private void HermiteButton_Click(object sender, RoutedEventArgs e)
        {
            previewPoint = true;
            previewTPoint = true;
            draw = true;
            pointP.Add(new PointF());
            pointT.Add(new PointF());
            thickness.Add(1);
            colors.Add(new myColor(ColorPicker.R, ColorPicker.G, ColorPicker.B));
            foundP = pointP.Count - 1;
            pickingLine = false;

        }

        bool pickingLine;
        //clicking on pick a line button
        private void LinePicker_Click(object sender, RoutedEventArgs e)
        {
            if (pickingLine) pickingLine = false;
            else
                pickingLine = true;


        }

        bool isCtrl = false;
        //is the user pressing ctrl (ctrl+click on canvas = placing point
        private void WindowsFormsHost_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (foundP != -1 && pickingLine && e.Key == Key.Delete)
            {
                DeleteCurve(foundP + 1);
                foundP = -1;
            }
            if (e.Key == Key.LeftCtrl)
                isCtrl = true;
        }

        private void winFormHost_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.LeftCtrl)
                isCtrl = false;
        }

        //context menu opening
        private void conMenu_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (foundP == -1 && !pickingLine)
            {
                menuDeleteCurve.IsEnabled = false;
            }
            else
            {
                menuDeleteCurve.IsEnabled = true;
            }

            if (found == 3)
            {
                menuDeletePoint.IsEnabled = true;
            }
            else menuDeletePoint.IsEnabled = false;


        }

        private void conMenuDelete_Click(object sender, RoutedEventArgs e)
        {
            DeleteCurve(foundP + 1);
            foundP = -1;
        }

        //click on delete point button
        private void menuDeletePoint_Click(object sender, RoutedEventArgs e)
        {
            DeleteCurve(foundP);
            foundP = -1;
        }

        //when the user selected a curve and using mousewheel
        private void Canvas_MouseWheel(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Delta > 0 && selectedLines.Count>0 )
            {
                for(int i =0;i<selectedLines.Count;i++)
                {
                    if (thickness[selectedLines[i]] <= 10)
                        thickness[selectedLines[i]] += (float)0.25;
                }

                Canvas.Refresh();
            }
            else if (e.Delta < 0 && selectedLines.Count > 0)
            {
                for (int i = 0; i < selectedLines.Count; i++)
                {
                    if (thickness[selectedLines[i]] >= 0.5)
                        thickness[selectedLines[i]] -= (float)0.25;
                }
                Canvas.Refresh();
            }
        }


        //changing color of the selected curves when user changes the color on the colorpicker
        private void ColorPicker_SelectedColorChanged(object sender, RoutedPropertyChangedEventArgs<System.Windows.Media.Color?> e)
        {
            if(selectedLines.Count >0)
            {
                for (int i = 0; i < selectedLines.Count; i++)
                {
                    colors[selectedLines[i]].R = ColorPicker.R;
                    colors[selectedLines[i]].G = ColorPicker.G;
                    colors[selectedLines[i]].B = ColorPicker.B;
                }
                Canvas.Refresh();
            }
        }

        //saving file
        private void saveButton_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Text file | *.txt";
            saveFileDialog.FileName = "save.txt";
            saveFileDialog.Title = "Save file";
            saveFileDialog.ShowDialog();

            if(saveFileDialog.FileName!="")
            {
                System.IO.StreamWriter sw = new System.IO.StreamWriter(saveFileDialog.OpenFile());

                sw.WriteLine("pointP");
                for(int i =0;i<pointP.Count;i++)
                {
                    sw.WriteLine(pointP[i].X + " " + pointP[i].Y);
                }
                sw.WriteLine("pointT");
                for(int i=0;i<pointT.Count;i++)
                {
                    sw.WriteLine(pointT[i].X + " " + pointT[i].Y);
                }
                sw.WriteLine("thickness");
                for(int i=0;i<thickness.Count;i++)
                {
                    sw.WriteLine(thickness[i]);
                }
                sw.WriteLine("color");
                for(int i=0;i<colors.Count;i++)
                {
                    sw.WriteLine(colors[i].R + " " + colors[i].G + " " + colors[i].B);
                }
                sw.WriteLine("end");
                sw.Dispose();
                sw.Close();
            }
        }
        //loading file
        private void loadButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFD = new OpenFileDialog();
            openFD.Filter = "Text file | *.txt";
            openFD.FileName = "save.txt";
            openFD.Title = "Open file";
            openFD.ShowDialog();

            if(openFD.FileName!="")
            {
                clearButton_Click(sender, e);
                System.IO.StreamReader sr = new System.IO.StreamReader(openFD.OpenFile());

                List<string> lines = new List<string>();
                while (!sr.EndOfStream)
                    lines.Add(sr.ReadLine());

                for(int i=0;i<lines.Count;i++)
                {
                    if(lines[i]=="pointP")
                    {
                        while(lines[i+1] !="pointT")
                        {
                            i++;
                            string[] temp = lines[i].Split(' ');
                            pointP.Add(new PointF(Convert.ToInt16(temp[0]), Convert.ToInt16(temp[1])));
                        }
                    }
                    else if(lines[i]=="pointT")
                    {
                        while(lines[i+1]!="thickness")
                        {
                            i++;
                            string[] temp = lines[i].Split(' ');
                            pointT.Add(new PointF(Convert.ToInt16(temp[0]), Convert.ToInt16(temp[1])));
                        }
                    }
                    else if (lines[i] == "thickness")
                    {
                        while (lines[i + 1] != "color")
                        {
                            i++;
                            thickness.Add(float.Parse(lines[i]));
                        }
                    }
                    else if (lines[i] == "color")
                    {
                        while (lines[i + 1] != "end")
                        {
                            i++;
                            string[] temp = lines[i].Split(' ');
                            colors.Add(new myColor(Convert.ToByte(temp[0]), Convert.ToByte(temp[1]), Convert.ToByte(temp[2])));
                        }
                    }
                }
                Canvas.Refresh();
            }
        }
        //clearing canvas
        private void clearButton_Click(object sender, RoutedEventArgs e)
        {
            pointP.Clear();
            pointT.Clear();
            thickness.Clear();
            colors.Clear();
            pickingLine = false;
            Canvas.Refresh();
        }
    }


}