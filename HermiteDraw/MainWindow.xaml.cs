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
    public partial class MainWindow : Window
    {
      //  PointF p0, t0, p1, t1;

        List<PointF> pointP;
        List<PointF> pointT;
        bool draw;

        Pen previewPBlue = new Pen(Color.FromArgb(120,37,24,209));
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
            InitializeComponent();
            draw = false;
           

        }
        Graphics g;

        private void Canvas_Paint(object sender, PaintEventArgs e)
        {

            //TODO: vonalakat átlehessen kötni másik ponthoz. a módosítandó pontok legyenek mindig preview színűek.
            //TODO: második kattintásra a T pont mozgatása. DONE
            //TODO: színválasztó implementálása.DONE
            //TODO: zoom
            //TODO: vonalakra lehessen duplán kattintani. legyen egyértelmű hogy kivan jelölve. színváltoztatás, vastagság állítása(görgővel is).
            //TODO: pontok törlése.
            //TODO: save-load.
            g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            if(draw && pointP.Count==1)
            {
                g.DrawLine(pRed, pointP[0], pointT[0]);
                g.FillRectangle(red, pointP[0].X - 3, pointP[0].Y - 3, 6, 6);
                g.FillRectangle(green, pointT[0].X - 3, pointT[0].Y - 3, 6, 6);

            }

            if(draw)
            {
                for (int i = 1; i < pointP.Count; i++)
                {
                    
                    double a = 0;
                    double b = 1;
                    double h = (b - a) / 100;
                    double t = a;
                    PointF ph1, ph2;
                    ph1 = HermitePoint(t,pointP[i-1],pointP[i],pointT[i-1],pointT[i]);
                    while (t < b)
                    {
                        t += h;
                        ph2 = HermitePoint(t, pointP[i - 1], pointP[i], pointT[i - 1], pointT[i]);
                        if(previewPoint && i==foundP)
                        {
                            g.DrawLine(new Pen(Color.FromArgb(120,ColorPicker.R,ColorPicker.G,ColorPicker.B)), ph1, ph2);
                        }
                        else if(pickingLine && i==foundP)
                        {
                            g.DrawLine(new Pen(Color.FromArgb(255, ColorPicker.R, ColorPicker.G, ColorPicker.B), 4), ph1, ph2);
                        }
                        else
                            g.DrawLine(new Pen(Color.FromArgb(255,ColorPicker.R,ColorPicker.G,ColorPicker.B),4), ph1, ph2);
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
        private void Canvas_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {

            if (!previewPoint)
            {
                previewTPoint = false;
                PointF mouse = new PointF(e.X, e.Y);
                for (int i = 0; i < pointP.Count-1; i++)
                {
                    if(pickingLine)
                    {
                        double a = 0;
                        double b = 1;
                        double h = (b - a) / 100;
                        double t = a;
                        PointF ph1, ph2;
                        ph1 = HermitePoint(t, pointP[i], pointP[i+1], pointT[i], pointT[i+1]);
                        while (t < b)
                        {
                            t += h;
                            ph2 = HermitePoint(t, pointP[i], pointP[i+1], pointT[i], pointT[i+1]);
                            if (Find(ph1, mouse, 5))
                            {
                                foundP = i+1;
                                break;
                            }
                            ph1 = ph2;
                        }
                    }


                    if (Find(pointP[i], mouse, 5))//checks if user clicked on a P point
                    {
                        found = 1;
                        foundP = i;
                        previewPoint = true;
                        break;
                    }
                    else if (Find(pointT[i], mouse, 5))//checks if user clicked on a T point
                    {
                        found = 2;
                        foundT = i;
                        previewPoint = true;
                        break;
                    }
                }
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
            else if(found==2)
            {
                PointF mouse = new PointF(e.X, e.Y);
                pointT[foundT] = mouse;
            }
          
           else if(previewPoint)
            {
                PointF mouse = new PointF(e.X, e.Y);
                pointP[pointP.Count - 1] = mouse;
                pointT[pointT.Count - 1] = new PointF(mouse.X - 30, mouse.Y - 30);
            }
            else if(previewTPoint && !previewPoint )
            {
                PointF mouse = new PointF(e.X, e.Y);
                pointT[pointT.Count - 1] = mouse;
            }
                Canvas.Refresh();
            }
        

        private void Canvas_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            found = -1;
            foundP = -1;
            foundT =- 1;
            previewPoint = false;

            if (pointP.Count == 1 && !previewTPoint)//this automatically creates a second T and P point after finalizing the first points, without having to press the add the button.
            {
                previewPoint = true;
                previewTPoint = true;
                pointP.Add(new PointF());
                pointT.Add(new PointF());
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

        bool previewPoint = false;
        bool previewTPoint = false;
        private void HermiteButton_Click(object sender, RoutedEventArgs e)
        {
            previewPoint = true;
            previewTPoint = true;
            draw = true;
            pointP.Add(new PointF());
            pointT.Add(new PointF());
            foundP = pointP.Count - 1;
            
        }

        bool pickingLine;
        private void LinePicker_Click(object sender, RoutedEventArgs e)
        {
            pickingLine = true;
        }
    }

    
}
