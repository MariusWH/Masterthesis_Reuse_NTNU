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

        public List<System.Drawing.Color> BrepColors;
        public List<Brep> BrepVisuals;





        public TrussModel2D(List<Line> lines, List<double> A, List<Point3d> anchoredPoints, List<double> loadList, List<Vector3d> loadVecs, double E)
        {
            CheckInputs(ref lines, ref A, ref anchoredPoints, ref loadList, ref loadVecs, ref E);

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
            N_out = new List<double>();

            BrepVisuals = new List<Brep>();
            BrepColors = new List<System.Drawing.Color>();

        }







        private void CheckInputs(ref List<Line> lines, ref List<double> A, ref List<Point3d> anchoredPoints, ref List<double> loadList, ref List<Vector3d> loadVecs, ref double E)
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
        }






        public void Assemble() // Improve this
        {
            foreach (Element element in Elements)
            {
                double dy = element.EndPoint.Y - element.StartPoint.Y;
                double dx = element.EndPoint.X - element.StartPoint.X;
                double rad = Math.Atan2(dy, dx);
                double EAL = (element.A * element.E / element.StartPoint.DistanceTo(element.EndPoint));
                double cc = Math.Cos(rad) * Math.Cos(rad) * EAL;
                double ss = Math.Sin(rad) * Math.Sin(rad) * EAL;
                double sc = Math.Sin(rad) * Math.Cos(rad) * EAL;

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
                double L0 = element.StartPoint.DistanceTo(element.EndPoint);
                double L1 = 0;

                if (element.StartNodeIndex == -1 && element.EndNodeIndex == -1)
                    L1 = L0;
                else if (element.EndNodeIndex == -1)
                    L1 = Nodes[element.StartNodeIndex].DistanceTo(element.EndPoint);
                else if (element.StartNodeIndex == -1)
                    L1 = element.StartPoint.DistanceTo(Nodes[element.EndNodeIndex]);
                else
                    L1 = Nodes[element.StartNodeIndex].DistanceTo(Nodes[element.EndNodeIndex]);

                double N_element = (element.A * element.E / L0) * (L1 - L0);
                N.Append(N_element);
                N_out.Add(N_element);
            }
        }





        public void GetResultVisuals()
        {
            double f_dim = 355;
            List<double> sigma = new List<double>();
            List<double> utilization = new List<double>();

            for (int i = 0; i < Elements.Count; i++)
            {
                sigma.Add(N_out[i] / Elements[i].A);
                utilization.Add(sigma[i] / f_dim);
                N_out[i] = utilization[i];

                if (utilization[i] > 1 || utilization[i] < -1)
                    { BrepColors.Add(System.Drawing.Color.Red); }
                else
                    { BrepColors.Add(System.Drawing.Color.Green); }


                Point3d startOfElement = Elements[i].StartPoint;
                if (Elements[i].StartNodeIndex != -1)
                    startOfElement = Nodes[Elements[i].StartNodeIndex];

                Point3d endOfElement = Elements[i].EndPoint;
                if (Elements[i].EndNodeIndex != -1)
                    endOfElement = Nodes[Elements[i].EndNodeIndex];

                Cylinder cylinder = new Cylinder(new Circle(new Plane(startOfElement, new Vector3d(endOfElement - startOfElement)), Math.Sqrt(Elements[i].A / Math.PI)), startOfElement.DistanceTo(endOfElement));
                Brep pipe = cylinder.ToBrep(true, true);
                BrepVisuals.Add(pipe);
            }
        }





        public void GetLoadVisuals()
        {
            for (int i = 0; i < Nodes.Count; i++)
            {
                Vector3d dir = new Vector3d(R0[2 * i], R0[2 * i + 1], 0);
                dir.Unitize();
                double arrowLength = Math.Sqrt(R0[2 * i] * R0[2 * i] + R0[2 * i + 1] * R0[2 * i + 1]) / 1000;
                double lineRadius = 20;

                System.Drawing.Color loadColor = System.Drawing.Color.AliceBlue;

                Point3d startPoint = Nodes[i];
                Point3d endPoint = Nodes[i] + new Point3d(dir * arrowLength);
                Point3d arrowBase = endPoint + dir * 4 * lineRadius;

                Cylinder loadCylinder = new Cylinder(new Circle(new Plane(startPoint, dir), lineRadius), startPoint.DistanceTo(endPoint));
                BrepVisuals.Add(loadCylinder.ToBrep(true, true));
                BrepColors.Add(loadColor);

                Cone arrow = new Cone(new Plane(arrowBase, new Vector3d(R0[2 * i], R0[2 * i + 1], 0)), -4 * lineRadius, 2 * lineRadius);
                BrepVisuals.Add(arrow.ToBrep(true));
                BrepColors.Add(loadColor);


            }
        }





        public string PrintInfo()
        {
            string info = "";
            foreach (Element element in Elements)
                info += element.getElementInfo() + "\n";
            return info;
        }















    }
}
