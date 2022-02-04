using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;




namespace MasterthesisGHA
{
    internal class TrussModel2D
    {
        public readonly List<Element> Elements;
        public readonly List<Point3d> Nodes;

        public Matrix<double> K;
        public Vector<double> R0;
        public Vector<double> r;
        public Vector<double> N;

        public Matrix K_out;
        public Matrix r_out;
        public List<double> N_out;

        


        public TrussModel2D(List<Line> lines, List<double> A, List<Point3d> anchoredPoints, List<double> loadList, List<Vector3d> loadVecs, double E )
        {
            if (lines.Count == 0)
                throw new Exception("Line Input is not valid!");

            if (anchoredPoints.Count < 2)
                throw new Exception("Anchored points needs to be at least 2 to prevent rigid body motion!");

            if (A.Count == 1)
            {
                List<double> iA_constValue = new List<double>();
                for (int i = 0; i < lines.Count; i++)
                    iA_constValue.Add(A[0]);
                A = iA_constValue;
            }
            else if (A.Count != lines.Count)
                throw new Exception("A is wrong size! Input list with same length as Lines or constant value!");


            Elements = new List<Element>();
            Nodes = new List<Point3d>();


            for (int i = 0; i < lines.Count; i++)
            {
                Point3d startPoint = lines[i].PointAt(0);
                Point3d endPoint = lines[i].PointAt(1);
                Elements.Add(new Element(startPoint, endPoint, ref Nodes, anchoredPoints, E, A[i]));
            }

            int dofs = 2 * Nodes.Count;
            
            while (loadList.Count < dofs)
                loadList.Add(0);

            while (loadList.Count > dofs)
                loadList.RemoveAt(loadList.Count - 1);


            if (loadVecs.Count <= Nodes.Count)
                for (int i = 0; i < loadVecs.Count; i++)
                {
                    loadList[2 * i] += loadVecs[i].X;
                    loadList[2 * i + 1] += loadVecs[i].Y;
                }

            R0 = Vector<double>.Build.Dense(dofs);
            for (int i = 0; i < Nodes.Count; i++)
            {
                R0[2 * i] = loadList[2 * i];
                R0[2 * i + 1] = loadList[2 * i + 1];
            }

            K = Matrix<double>.Build.Dense(dofs, dofs);           
            r = Vector<double>.Build.Dense(dofs);
            N = Vector<double>.Build.Dense(0);

            K_out = new Matrix(dofs, dofs);
            r_out = new Matrix(dofs, 1);
            //N_out = new Matrix(Elements.Count, 1);
            N_out = new List<double>();

        }




        public void Assemble()
        {
            foreach (Element element in Elements)
            {
                double dy = element.EndPoint.Y - element.StartPoint.Y;
                double dx = element.EndPoint.X - element.StartPoint.X;
                double rad = Math.Atan2(dy, dx);                            
                double EAL = (element.A * element.E / element.Length());
                double cc = Math.Cos(rad)*Math.Cos(rad)*EAL;
                double ss = Math.Sin(rad)*Math.Sin(rad)*EAL;
                double sc = Math.Sin(rad)*Math.Cos(rad)*EAL;

                int startIndex = element.StartNodeIndex;
                int endIndex = element.EndNodeIndex;

                if (startIndex != -1)
                {
                    K[2 * startIndex, 2 * startIndex] += cc;
                    K[2 * startIndex + 1, 2 * startIndex + 1] += ss;
                    K[2 * startIndex + 1, 2 * startIndex] += sc;
                    K[2 * startIndex, 2 * startIndex + 1] += sc;
                }
                if (endIndex != -1)
                {
                    K[2 * endIndex, 2 * endIndex] += cc;
                    K[2 * endIndex + 1, 2 * endIndex + 1] += ss;
                    K[2 * endIndex + 1, 2 * endIndex] += sc;
                    K[2 * endIndex, 2 * endIndex + 1] += sc;
                }
                if (startIndex != -1 && endIndex != -1)
                {
                    K[2 * startIndex, 2 * endIndex] += -cc;
                    K[2 * startIndex + 1, 2 * endIndex + 1] += -ss;
                    K[2 * startIndex + 1, 2 * endIndex] += -sc;
                    K[2 * startIndex, 2 * endIndex + 1] += -sc;

                    K[2 * endIndex, 2 * startIndex] += -cc;
                    K[2 * endIndex + 1, 2 * startIndex + 1] += -ss;
                    K[2 * endIndex + 1, 2 * startIndex] += -sc;
                    K[2 * endIndex, 2 * startIndex + 1] += -sc;
                }
                
            }
        }




        public void Solve()
        {
            r = K.Solve(R0);

            for (int i = 0; i < Nodes.Count; i++)
                Nodes[i] += new Point3d(r[2 * i], r[2 * i + 1], 0);

            for (int i = 0; i < r.Count; i++)
                r_out[i, 0] = r[i];

            for (int i = 0; i < K.RowCount; i++)
            {
                for (int j = 0; j < K.ColumnCount; j++)
                    K_out[i, j] = K[i, j];
            }
        }




        public void Retracking()
        {
            foreach (Element element in Elements)
            {
                double L0 = element.Length();
                double L1 = 0;

                if (element.StartNodeIndex == -1 && element.EndNodeIndex == -1)
                    L1 = L0;                   
                else if(element.EndNodeIndex == -1)
                    L1 = Nodes[element.StartNodeIndex].DistanceTo(element.EndPoint);
                else if(element.StartNodeIndex == -1)
                    L1 = element.StartPoint.DistanceTo(Nodes[element.EndNodeIndex]);
                else
                    L1 = Nodes[element.StartNodeIndex].DistanceTo(Nodes[element.EndNodeIndex]);

                double N_element = (element.A * element.E / L0) * (L1 - L0);
                N.Append(N_element);
                //N_out[count++, 0] = N_element;
                N_out.Add(N_element);
            }
        }


        public void ResultVisuals(out List<System.Drawing.Color> colors)
        {
            //Interval N_interval = new Interval(N.Min(), N.Max());

            double min = N_out[0];
            double max = N_out[0];

            double f_dim = 355;
            List<double> sigma = new List<double>();
            List<double> utilization = new List<double>();
            colors = new List<System.Drawing.Color>();

            for (int i = 1; i < Elements.Count; i++)
            {
                if (N_out[i] < min)
                    min = N_out[i];
                if (N_out[i] > max)
                    max = N_out[i];
            }

            for (int i = 0; i < Elements.Count; i++)
            {
                sigma.Add( N_out[i] / Elements[i].A );
                utilization.Add(sigma[i] / f_dim);
                
                N_out[i] = utilization[i];

                
                int r = 0;
                int g = 0;
                int b = 0;
                int alfa = 255;

                if (utilization[i] > 1 || utilization[i] < -1)
                {
                    r = 255;
                    g = 0;
                    b = 0;
                }
                else
                {
                    r = 0;
                    g = 255;
                    b = 0;
                }

                colors.Add(System.Drawing.Color.FromArgb(alfa,r,g,b));
            }                
        }



        public string PrintInfo()
        {
            string info = "";
            foreach (Element element in Elements)
                info += element.elementInfo + "\n";
            return info;
        }      


    }
}
