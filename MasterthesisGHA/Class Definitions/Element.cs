using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using Rhino.Geometry;
using MathNet.Numerics;


namespace MasterthesisGHA
{
    internal class Element
    {
        public Point3d StartPoint;
        public Point3d EndPoint;

        public int StartNodeIndex;
        public int EndNodeIndex;

        public double I;
        public double A;
        public double E;

        private static int instanceCounter;
        private readonly int instanceID;

        public string elementInfo;



        public Element(Point3d startPoint, Point3d endPoint, ref List<Point3d> nodes, List<Point3d> anchored, double e = 1, double a = 1, double i = 1)
        {
            this.instanceID = instanceCounter++;
            elementInfo = "eID:" + this.ID().ToString();

            StartPoint = startPoint;
            EndPoint = endPoint;
            I = i;
            A = a;
            E = e;

            if (!anchored.Contains(startPoint))
            {
                if (!nodes.Contains(startPoint))
                    nodes.Add(startPoint);
                StartNodeIndex = nodes.IndexOf(startPoint);
                
            }
            else
            {
                StartNodeIndex = -1;
            }

            if (!anchored.Contains(endPoint))
            {
                if (!nodes.Contains(endPoint))
                    nodes.Add(endPoint);
                EndNodeIndex = nodes.IndexOf(endPoint);
                
            }
            else
            {
                EndNodeIndex = -1;
            }

            
            elementInfo += ", [i1,i2] = [" + StartNodeIndex.ToString() + "," + EndNodeIndex.ToString() + "]";

            double dy = EndPoint.Y - StartPoint.Y;
            double dx = EndPoint.X - StartPoint.X;
            double rad = Math.Atan2(dy, dx);

            //double EAL = (EA / Length());
            double EAL = 1.0;

            double cc = Math.Cos(rad) * Math.Cos(rad) * EAL;
            double ss = Math.Sin(rad) * Math.Sin(rad) * EAL;
            double sc = Math.Sin(rad) * Math.Cos(rad) * EAL;

            elementInfo += ", cc = " + cc + ", ss = " + ss + ", sc = " + sc; 

        }




        public double Length()
        { return StartPoint.DistanceTo(EndPoint); }

        public int ID()
        { return instanceID; }


    }
}
