using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;


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

        public Matrix<double> k;

        private static int instanceCounter;
        public readonly int instanceID;


        public Element(Point3d startPoint, Point3d endPoint, ref List<Point3d> nodes, List<Point3d> anchored, double e = 1, double a = 1, double i = 1)
        {
            this.instanceID = instanceCounter++;

            StartPoint = startPoint;
            EndPoint = endPoint;
            I = i;
            A = a;
            E = e;
            k = Matrix<double>.Build.Dense(4,4);

            if (!anchored.Contains(startPoint))
            {
                if (!nodes.Contains(startPoint))
                    nodes.Add(startPoint);
                StartNodeIndex = nodes.IndexOf(startPoint);               
            }
            else
            { StartNodeIndex = -1; }

            if (!anchored.Contains(endPoint))
            {
                if (!nodes.Contains(endPoint))
                    nodes.Add(endPoint);
                EndNodeIndex = nodes.IndexOf(endPoint);               
            }
            else
            { EndNodeIndex = -1; }

        }







        public Matrix<double> getLocalStiffness()
        {
            double dy = EndPoint.Y - StartPoint.Y;
            double dx = EndPoint.X - StartPoint.X;
            double rad = Math.Atan2(dy, dx);
            double EAL = (A * E / StartPoint.DistanceTo(EndPoint));
            double cc = Math.Cos(rad) * Math.Cos(rad) * EAL;
            double ss = Math.Sin(rad) * Math.Sin(rad) * EAL;
            double sc = Math.Sin(rad) * Math.Cos(rad) * EAL;

            double[,] k_array = { {cc,sc,-cc,-sc},
                                  {sc,ss,-sc,-ss},
                                  {-cc,-sc,cc,sc},
                                  {-sc,-ss,sc,ss} };

            return Matrix<double>.Build.DenseOfArray(k_array);
        }




        public string getElementInfo()
        {          
            Matrix<double> k = getLocalStiffness();

            string elementInfo = "eID:" + this.instanceID.ToString();
            elementInfo += ", [i1,i2] = [" + StartNodeIndex.ToString() + "," + EndNodeIndex.ToString() + "]";
            elementInfo += ", cc = " + k[0,0] + ", ss = " + k[1,1] + ", sc = " + k[0,1];

            return elementInfo;
        }




        public double ProjectedElementLength(Vector3d distributionDirection)
        {
            double elementLength = StartPoint.DistanceTo(EndPoint);
            Vector3d elementDirection = new Vector3d(EndPoint - StartPoint);

            elementDirection.Unitize();
            distributionDirection.Unitize();

            Vector<double> elementDirectionMathNet = Vector<double>.Build.Dense(3);
            Vector<double> projectionDirectionMathNet = Vector<double>.Build.Dense(3);

            for (int i = 0; i < 3; i++)
            {
                elementDirectionMathNet[i] = elementDirection[i];
                projectionDirectionMathNet[i] = distributionDirection[i];
            }
         
            return elementLength * elementDirectionMathNet.ConjugateDotProduct(projectionDirectionMathNet);
        }





    }
}
