﻿using System;
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
        public readonly List<OLDInPlaceElement> Elements;
        public readonly List<Point3d> FreeNodes;
        public readonly List<Point3d> PinnedNodes;

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
            OLDInPlaceElement.resetStatic(); // Instance counter to zero

            Elements = new List<OLDInPlaceElement>();
            FreeNodes = new List<Point3d>();
            PinnedNodes = anchoredPoints;

            for (int i = 0; i < lines.Count; i++)
            {
                Point3d startPoint = lines[i].PointAt(0);
                Point3d endPoint = lines[i].PointAt(1);
                Elements.Add(new OLDInPlaceElement(startPoint, endPoint, ref FreeNodes, anchoredPoints, E, A[i]));
            }

            int dofs = 2 * FreeNodes.Count;

            while (loadList.Count < dofs)
                loadList.Add(0);

            while (loadList.Count > dofs)
                loadList.RemoveAt(loadList.Count - 1);


            if (loadVecs.Count <= FreeNodes.Count)
                for (int i = 0; i < loadVecs.Count; i++)
                {
                    loadList[2 * i] += loadVecs[i].X;
                    loadList[2 * i + 1] += loadVecs[i].Y;
                }

            R0 = Vector<double>.Build.Dense(dofs);
            for (int i = 0; i < FreeNodes.Count; i++)
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





        public TrussModel2D(List<OLDInPlaceElement> elements, List<Point3d> supportPoints, List<double> loads )
        {
            OLDInPlaceElement.resetStatic();

            Elements = elements;
            FreeNodes = new List<Point3d>();
            PinnedNodes = supportPoints;

                // SORT ELEMENTS HERE            
            
            for (int i = 0; i < Elements.Count; i++)
            {
                Point3d startPoint = elements[i].StartPoint;
                Point3d endPoint = elements[i].EndPoint;

                if (!PinnedNodes.Contains(startPoint))
                {
                    if (!FreeNodes.Contains(startPoint))
                        FreeNodes.Add(startPoint);
                    Elements[i].StartNodeIndex = FreeNodes.IndexOf(startPoint);
                }

                if (!PinnedNodes.Contains(endPoint))
                {
                    if (!FreeNodes.Contains(endPoint))
                        FreeNodes.Add(endPoint);
                    Elements[i].EndNodeIndex = FreeNodes.IndexOf(endPoint);
                }
            }


            

            int dofs = 2 * FreeNodes.Count;
            R0 = Vector<double>.Build.Dense(dofs);

            while (loads.Count < dofs)
                loads.Add(0);

            while (loads.Count > dofs)
                loads.RemoveAt(loads.Count - 1);

            for (int i = 0; i < FreeNodes.Count; i++)
            {
                R0[2 * i] = loads[2 * i];
                R0[2 * i + 1] = loads[2 * i + 1];
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
            foreach (OLDInPlaceElement element in Elements)
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

            for (int i = 0; i < FreeNodes.Count; i++)
                FreeNodes[i] += new Point3d(r[2 * i], r[2 * i + 1], 0);

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
            foreach (OLDInPlaceElement element in Elements)
            {
                double L0 = element.StartPoint.DistanceTo(element.EndPoint);
                double L1 = 0;

                if (element.StartNodeIndex == -1 && element.EndNodeIndex == -1)
                    L1 = L0;
                else if (element.EndNodeIndex == -1)
                    L1 = FreeNodes[element.StartNodeIndex].DistanceTo(element.EndPoint);
                else if (element.StartNodeIndex == -1)
                    L1 = element.StartPoint.DistanceTo(FreeNodes[element.EndNodeIndex]);
                else
                    L1 = FreeNodes[element.StartNodeIndex].DistanceTo(FreeNodes[element.EndNodeIndex]);

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

            System.Drawing.Color pinnedNodeColor = System.Drawing.Color.AliceBlue;
            System.Drawing.Color freeNodeColor = System.Drawing.Color.AliceBlue;

            for (int i = 0; i < Elements.Count; i++)
            {
                sigma.Add(N_out[i] / Elements[i].A);
                utilization.Add(sigma[i] / f_dim);
                //N_out[i] = utilization[i];

                if (utilization[i] > 1 || utilization[i] < -1)
                    { BrepColors.Add(System.Drawing.Color.Red); }
                else
                    { BrepColors.Add(System.Drawing.Color.Green); }


                Point3d startOfElement = Elements[i].StartPoint;
                if (Elements[i].StartNodeIndex != -1)
                    startOfElement = FreeNodes[Elements[i].StartNodeIndex];

                Point3d endOfElement = Elements[i].EndPoint;
                if (Elements[i].EndNodeIndex != -1)
                    endOfElement = FreeNodes[Elements[i].EndNodeIndex];

                Cylinder cylinder = new Cylinder(new Circle(new Plane(startOfElement, new Vector3d(endOfElement - startOfElement)), Math.Sqrt(Elements[i].A / Math.PI)), startOfElement.DistanceTo(endOfElement));
                BrepVisuals.Add(cylinder.ToBrep(true, true));
            }

            foreach (Point3d pinnedNode in PinnedNodes)
            {
                double nodeRadius = 40;
                Sphere nodeSphere = new Sphere(pinnedNode, nodeRadius);
                BrepVisuals.Add(nodeSphere.ToBrep());
                BrepColors.Add(pinnedNodeColor);

                Plane conePlane = new Plane(pinnedNode + new Point3d(0,-nodeRadius,0), new Vector3d(0, -1, 0));
                Cone pinnedCone = new Cone(conePlane, 2*nodeRadius, 2*nodeRadius);
                BrepVisuals.Add(pinnedCone.ToBrep(true));
                BrepColors.Add(pinnedNodeColor);

            }

            foreach (Point3d freeNode in FreeNodes)
            {
                double nodeRadius = 40;
                Sphere nodeSphere = new Sphere(freeNode, nodeRadius);
                BrepVisuals.Add(nodeSphere.ToBrep());
                BrepColors.Add(freeNodeColor);

            }


        }





        public void GetLoadVisuals()
        {
            for (int i = 0; i < FreeNodes.Count; i++)
            {
                Vector3d dir = new Vector3d(R0[2 * i], R0[2 * i + 1], 0);
                dir.Unitize();
                double arrowLength = Math.Sqrt(R0[2 * i] * R0[2 * i] + R0[2 * i + 1] * R0[2 * i + 1]) / 1000;
                double lineRadius = 20;

                System.Drawing.Color loadColor = System.Drawing.Color.AliceBlue;

                Point3d startPoint = FreeNodes[i];
                Point3d endPoint = FreeNodes[i] + new Point3d(dir * arrowLength);
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
            foreach (OLDInPlaceElement element in Elements)
                info += element.getElementInfo() + "\n";
            return info;
        }






        public void ApplyLineLoad(double loadValue, Vector3d loadDirection, Vector3d distributionDirection, List<Line> loadElements)
        {
            loadDirection.Unitize();
            foreach (OLDInPlaceElement element in Elements)
            {
                foreach (Line loadElement in loadElements)
                {
                    if (element.StartPoint == loadElement.PointAt(0) && element.EndPoint == loadElement.PointAt(1))
                    {
                        if (element.StartNodeIndex != -1)
                        {
                            R0[2 * element.StartNodeIndex] += loadValue * Math.Abs(element.ProjectedElementLength(distributionDirection)) * loadDirection[0] / 2;
                            R0[2 * element.StartNodeIndex + 1] += loadValue * Math.Abs(element.ProjectedElementLength(distributionDirection)) * loadDirection[1] / 2;
                        }

                        if (element.EndNodeIndex != -1)
                        {
                            R0[2 * element.EndNodeIndex] += loadValue * Math.Abs(element.ProjectedElementLength(distributionDirection)) * loadDirection[0] / 2;
                            R0[2 * element.EndNodeIndex + 1] += loadValue * Math.Abs(element.ProjectedElementLength(distributionDirection)) * loadDirection[1] / 2;
                        }
                    }
                }
            }
        }






        public string writeElementOutput()  // "Element #1 {  }"
        {
            string output = "ELEMENTS: \n\n";
            foreach (OLDInPlaceElement element in Elements)
            {
                output += "Element #" + element.instanceID + "{ ";
                output += "E[MPa]=" + element.E + ", ";
                output += "A[mm^2]=" + element.A + ", ";
                output += "I[mm^4]=" + element.I + ", ";
                output += "StartPoint[mm,mm]=(" + element.StartPoint.X + "," + element.StartPoint.Y + ")" + ", ";
                output += "EndPoint[mm,mm]=(" + element.EndPoint.X + "," + element.EndPoint.Y + ")" + ", ";
                output += "FreeNodeIndexing[#,#]=(" + element.StartNodeIndex + "," + element.EndNodeIndex + ")";
                output += " }\n";
            }
            return output;
        }






    }
}
