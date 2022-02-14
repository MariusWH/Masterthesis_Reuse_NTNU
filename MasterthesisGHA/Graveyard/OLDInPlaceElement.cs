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
    internal class OLDInPlaceElement
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



        // Static
        static OLDInPlaceElement()
        {
            instanceCounter = 0;
        }
        public static void resetStatic()
        {
            instanceCounter = 0;
        }


        // Constructors
        public OLDInPlaceElement(Point3d startPoint, Point3d endPoint, ref List<Point3d> nodes, List<Point3d> anchored, double e = 1, double a = 1, double i = 1)
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
        public OLDInPlaceElement(Line line, double e = 1, double a = 1, double i = 1)
        {
            this.instanceID = instanceCounter++;

            StartPoint = line.PointAt(0);
            EndPoint = line.PointAt(1);

            StartNodeIndex = -1;
            EndNodeIndex = -1;

            I = i;
            A = a;
            E = e;

            k = getLocalStiffness();

        }


        // Methods
        private void CheckInputs(ref List<Line> lines, ref List<double> A, ref List<double> E)
        {
            if (lines.Count == 0)
                throw new Exception("Line Input is not valid!");

            if (A.Count == 1)
            {
                List<double> A_constValue = new List<double>();
                for (int i = 0; i < lines.Count; i++)
                    A_constValue.Add(A[0]);
                A = A_constValue;
            }
            else if (A.Count != lines.Count)
                throw new Exception("A is wrong size! Input list with same length as Lines or constant value!");

            if (E.Count == 1)
            {
                List<double> E_constValue = new List<double>();
                for (int i = 0; i < lines.Count; i++)
                    E_constValue.Add(A[0]);
                E = E_constValue;
            }
            else if (E.Count != lines.Count)
                throw new Exception("E is wrong size! Input list with same length as Lines or constant value!");

            foreach(double e in E)
            {
                if (e < 0 || e > 1000e3)
                    throw new Exception("E-modulus can not be less than 0 or more than 1000 GPa!");
            }
            
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
