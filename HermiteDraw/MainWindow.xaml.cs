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

        Pen yellow = new Pen(Color.Yellow);
        Pen pRed = new Pen(Color.Red);
        Brush red = new SolidBrush(Color.Red);
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
            g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

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
                        g.DrawLine(yellow, ph1, ph2);
                        ph1 = ph2;
                    }
                    g.DrawLine(pRed, pointP[i-1], pointT[i-1]);
                    g.DrawLine(pRed, pointP[i], pointT[i]);
                    g.FillRectangle(red, pointP[i-1].X - 3, pointP[i-1].Y - 3, 6, 6);
                    g.FillRectangle(red, pointP[i].X - 3, pointP[i].Y - 3, 6, 6);
                    g.FillRectangle(green, pointT[i-1].X - 3, pointT[i-1].Y - 3, 6, 6);
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
            PointF mouse = new PointF(e.X, e.Y);
            for(int i =0;i<pointP.Count;i++)
            {
                if(Find(pointP[i],mouse,5))//checks if user clicked on a P point
                {
                    found = 1;
                    foundP = i;
                    break;
                }
                else if(Find(pointT[i],mouse,5))//checks if user clicked on a T point
                {
                    found = 2;
                    foundT=i;
                    break;
                }
            }
            if(found==-1)//creating new P and T point
            {
                pointP.Add(mouse);
                pointT.Add(new PointF(mouse.X - 30, mouse.Y - 100));
                if (pointT.Count > 1) draw = true;
                found = 0;
            }
           
        }

        private void Canvas_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
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
            else if (found ==0 )
            {
                PointF mouse = new PointF(e.X, e.Y);
                pointP[pointP.Count-1] = mouse;
                pointT[pointT.Count-1] = new PointF(mouse.X - 30, mouse.Y - 100);
            }
                Canvas.Refresh();
            }
        

        private void Canvas_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            found = -1;
            foundP = -1;
            foundT =- 1;
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
    }
    
}
