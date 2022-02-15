using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;

namespace MasterthesisGHA
{
    internal abstract class Structure
    {

        // Static
        protected static List<Structure> AllStructures;
        protected static int structureCount;
        static Structure()
        {
            structureCount = 0;
            AllStructures = new List<Structure>();
        }

        // Variables
        public List<System.Drawing.Color> BrepColors;
        public List<Brep> BrepVisuals;

                
        // Constructors
        public Structure(List<Line> lines, List<Point3d> supportPoints)
        {
            VerifyModel(ref lines, ref supportPoints);
            BrepVisuals = new List<Brep>();
            BrepColors = new List<System.Drawing.Color>();
        }

        // Methods
        private void VerifyModel(ref List<Line> lines, ref List<Point3d> anchoredPoints)
        {
            if (lines.Count == 0)
                throw new Exception("Line Input is not valid!");

            if (anchoredPoints.Count < 2)
                throw new Exception("Anchored points needs to be at least 2 to prevent rigid body motion!");
            
        }
        protected virtual int GetDofsPerNode()
        {
            return 0;
        }


    }




    internal class TrussModel3D : Structure
    {
        // Variables (not inherited)
        public readonly List<InPlaceBarElement> ElementsInStructure;
        public readonly List<Point3d> FreeNodes;
        public readonly List<Point3d> SupportNodes;

        public Matrix<double> GlobalStiffnessMatrix;
        public Vector<double> GlobalLoadVector;
        public Vector<double> GlobalDisplacementVector;

        public Matrix K_out;
        public Matrix r_out;
        public List<double> N_out;
     


        // Constructors
        public TrussModel3D(List<Line> lines, List<string> profileNames, List<Point3d> anchoredPoints)
            : base(lines, anchoredPoints)
        {

            ElementsInStructure = new List<InPlaceBarElement>();
            FreeNodes = new List<Point3d>();
            SupportNodes = anchoredPoints;

            for (int i = 0; i < lines.Count; i++)
            {
                Point3d startPoint = lines[i].PointAt(0);
                Point3d endPoint = lines[i].PointAt(1);
                ElementsInStructure.Add(new InPlaceBarElement(ref FreeNodes, ref SupportNodes, profileNames[i], startPoint, endPoint));
            }

            int dofs = GetDofsPerNode() * FreeNodes.Count;
            GlobalStiffnessMatrix = Matrix<double>.Build.Dense(dofs, dofs);
            GlobalDisplacementVector = Vector<double>.Build.Dense(dofs);
            K_out = new Matrix(dofs, dofs);
            r_out = new Matrix(dofs, 1);
            N_out = new List<double>();

            // Stiffness Matrix
            foreach (InPlaceBarElement element in ElementsInStructure)
                element.UpdateGlobalMatrix( ref GlobalStiffnessMatrix );


        }


        // Methods
        private void VerifyElementProperties(ref List<Line> lines, ref List<double> A, ref double E)
        {
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
        protected override int GetDofsPerNode()
        {
            return 3;
        }
        public void ApplyNodalLoads(List<double> loadList, List<Vector3d> loadVecs)
        {
            int dofsPerNode = GetDofsPerNode();
            int dofs = dofsPerNode * FreeNodes.Count;

            while (loadList.Count < dofs)
                loadList.Add(0);
            while (loadList.Count > dofs)
                loadList.RemoveAt(loadList.Count - 1);

            if (loadVecs.Count <= FreeNodes.Count)
                for (int i = 0; i < loadVecs.Count; i++)
                {
                    loadList[dofsPerNode * i] += loadVecs[i].X;
                    loadList[dofsPerNode * i + 1] += loadVecs[i].Y;
                    loadList[dofsPerNode * i + 2] += loadVecs[i].Z;
                }
            
            GlobalLoadVector = Vector<double>.Build.Dense(dofs);
            for (int i = 0; i < FreeNodes.Count; i++)
                for (int j = 0; j < dofsPerNode; j++)
                    GlobalLoadVector[dofsPerNode * i + j] = loadList[dofsPerNode * i + j];


        }
        public void ApplyLineLoad(double loadValue, Vector3d loadDirection, Vector3d distributionDirection, List<Line> loadElements)
        {
            loadDirection.Unitize();
            int dofsPerNode = GetDofsPerNode();
            foreach (InPlaceBarElement element in ElementsInStructure)
            {
                foreach (Line loadElement in loadElements)
                {
                    if (element.StartPoint == loadElement.PointAt(0) && element.EndPoint == loadElement.PointAt(1))
                    {
                        if (element.StartNodeIndex != -1)
                        {
                            GlobalLoadVector[dofsPerNode * element.StartNodeIndex] 
                                += loadValue * Math.Abs(element.ProjectedElementLength(distributionDirection)) * loadDirection[0] / 2;
                            GlobalLoadVector[dofsPerNode * element.StartNodeIndex + 1] 
                                += loadValue * Math.Abs(element.ProjectedElementLength(distributionDirection)) * loadDirection[1] / 2;
                            GlobalLoadVector[dofsPerNode * element.StartNodeIndex + 2]
                                += loadValue * Math.Abs(element.ProjectedElementLength(distributionDirection)) * loadDirection[2] / 2;
                        }

                        if (element.EndNodeIndex != -1)
                        {
                            GlobalLoadVector[dofsPerNode * element.EndNodeIndex] 
                                += loadValue * Math.Abs(element.ProjectedElementLength(distributionDirection)) * loadDirection[0] / 2;
                            GlobalLoadVector[dofsPerNode * element.EndNodeIndex + 1] 
                                += loadValue * Math.Abs(element.ProjectedElementLength(distributionDirection)) * loadDirection[1] / 2;
                            GlobalLoadVector[dofsPerNode * element.EndNodeIndex + 2]
                                += loadValue * Math.Abs(element.ProjectedElementLength(distributionDirection)) * loadDirection[2] / 2;
                        }
                    }
                }
            }
        }
        public void Solve()
        {
            GlobalDisplacementVector = GlobalStiffnessMatrix.Solve(GlobalLoadVector);
            int dofsPerNode = GetDofsPerNode();
            
            for (int i = 0; i < FreeNodes.Count; i++)
                FreeNodes[i] += new Point3d(GlobalDisplacementVector[dofsPerNode * i], 
                    GlobalDisplacementVector[dofsPerNode * i + 1], GlobalDisplacementVector[dofsPerNode * i + 2]);

            for (int i = 0; i < GlobalDisplacementVector.Count; i++)
                r_out[i, 0] = GlobalDisplacementVector[i];

            for (int i = 0; i < GlobalStiffnessMatrix.RowCount; i++)
            {
                for (int j = 0; j < GlobalStiffnessMatrix.ColumnCount; j++)
                    K_out[i, j] = GlobalStiffnessMatrix[i, j];
            }
        }
        public void Retracking()
        {
            foreach (InPlaceBarElement element in ElementsInStructure)
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

                double N_element = (element.CrossSectionArea * element.YoungsModulus / L0) * (L1 - L0);
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

            for (int i = 0; i < ElementsInStructure.Count; i++)
            {
                sigma.Add(N_out[i] / ElementsInStructure[i].CrossSectionArea);
                utilization.Add(sigma[i] / f_dim);
                //N_out[i] = utilization[i];

                if (utilization[i] > 1 || utilization[i] < -1)
                { BrepColors.Add(System.Drawing.Color.Red); }
                else
                { BrepColors.Add(System.Drawing.Color.Green); }


                Point3d startOfElement = ElementsInStructure[i].StartPoint;
                if (ElementsInStructure[i].StartNodeIndex != -1)
                    startOfElement = FreeNodes[ElementsInStructure[i].StartNodeIndex];

                Point3d endOfElement = ElementsInStructure[i].EndPoint;
                if (ElementsInStructure[i].EndNodeIndex != -1)
                    endOfElement = FreeNodes[ElementsInStructure[i].EndNodeIndex];

                Cylinder cylinder = new Cylinder(new Circle(new Plane(startOfElement, new Vector3d(endOfElement - startOfElement)), Math.Sqrt(ElementsInStructure[i].CrossSectionArea / Math.PI)), startOfElement.DistanceTo(endOfElement));
                BrepVisuals.Add(cylinder.ToBrep(true, true));
            }

            foreach (Point3d supportNode in SupportNodes)
            {
                double nodeRadius = 40;
                Sphere nodeSphere = new Sphere(supportNode, nodeRadius);
                BrepVisuals.Add(nodeSphere.ToBrep());
                BrepColors.Add(pinnedNodeColor);

                Plane conePlane = new Plane(supportNode + new Point3d(0, 0, -nodeRadius), new Vector3d(0, 0, -1));
                Cone pinnedCone = new Cone(conePlane, 2 * nodeRadius, 2 * nodeRadius);
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
                int dofsPerNode = GetDofsPerNode();
                Vector3d dir = new Vector3d(GlobalLoadVector[dofsPerNode * i], GlobalLoadVector[dofsPerNode * i + 1], 
                    GlobalLoadVector[dofsPerNode * i + 2]);
                dir.Unitize();
                double arrowLength = Math.Sqrt(GlobalLoadVector[dofsPerNode * i] * GlobalLoadVector[dofsPerNode * i] 
                    + GlobalLoadVector[dofsPerNode * i + 1] * GlobalLoadVector[dofsPerNode * i + 1]
                    + GlobalLoadVector[dofsPerNode * i + 2] * GlobalLoadVector[dofsPerNode * i + 2]) / 1000;
                double lineRadius = 20;

                System.Drawing.Color loadColor = System.Drawing.Color.AliceBlue;

                Point3d startPoint = FreeNodes[i];
                Point3d endPoint = FreeNodes[i] + new Point3d(dir * arrowLength);
                Point3d arrowBase = endPoint + dir * 4 * lineRadius;

                Cylinder loadCylinder = new Cylinder(new Circle(new Plane(startPoint, dir), lineRadius), startPoint.DistanceTo(endPoint));
                BrepVisuals.Add(loadCylinder.ToBrep(true, true));
                BrepColors.Add(loadColor);

                Cone arrow = new Cone(new Plane(arrowBase, new Vector3d(GlobalLoadVector[dofsPerNode * i], GlobalLoadVector[dofsPerNode * i + 1], GlobalLoadVector[dofsPerNode * i + 2])), -4 * lineRadius, 2 * lineRadius);
                BrepVisuals.Add(arrow.ToBrep(true));
                BrepColors.Add(loadColor);
            }
        }


    }



    internal class TrussModel2D : Structure
    {
        public readonly List<OLDInPlaceElement> ElementsInStructure;
        public readonly List<Point3d> FreeNodes;
        public readonly List<Point3d> SupportNodes;

        public Matrix<double> GlobalStiffnessMatrix;
        public Vector<double> GlobalLoadVector;
        public Vector<double> GlobalDisplacementVector;

        public Matrix K_out;
        public Matrix r_out;
        public List<double> N_out;

        public List<System.Drawing.Color> BrepColors;
        public List<Brep> BrepVisuals;



        // Constructors
        public TrussModel2D(List<Line> lines, List<double> A, List<Point3d> supportPoints, List<double> loadList, List<Vector3d> loadVecs, double E)
            : base(lines, supportPoints)
        {
            CheckInputs(ref lines, ref A, ref supportPoints, ref loadList, ref loadVecs, ref E);
            OLDInPlaceElement.resetStatic(); // Instance counter to zero

            ElementsInStructure = new List<OLDInPlaceElement>();
            FreeNodes = new List<Point3d>();
            SupportNodes = supportPoints;

            for (int i = 0; i < lines.Count; i++)
            {
                Point3d startPoint = lines[i].PointAt(0);
                Point3d endPoint = lines[i].PointAt(1);
                ElementsInStructure.Add(new OLDInPlaceElement(startPoint, endPoint, ref FreeNodes, supportPoints, E, A[i]));
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

            GlobalLoadVector = Vector<double>.Build.Dense(dofs);
            for (int i = 0; i < FreeNodes.Count; i++)
            {
                GlobalLoadVector[2 * i] = loadList[2 * i];
                GlobalLoadVector[2 * i + 1] = loadList[2 * i + 1];
            }

            GlobalStiffnessMatrix = Matrix<double>.Build.Dense(dofs, dofs);
            GlobalDisplacementVector = Vector<double>.Build.Dense(dofs);

            K_out = new Matrix(dofs, dofs);
            r_out = new Matrix(dofs, 1);
            N_out = new List<double>();

            BrepVisuals = new List<Brep>();
            BrepColors = new List<System.Drawing.Color>();

        }

        /*
        public TrussModel2D(List<OLDInPlaceElement> elements, List<Point3d> supportPoints, List<double> loads)
            :base()
        {
            OLDInPlaceElement.resetStatic();

            ElementsInStructure = elements;
            FreeNodes = new List<Point3d>();
            SupportNodes = supportPoints;

            // SORT ELEMENTS HERE            

            for (int i = 0; i < ElementsInStructure.Count; i++)
            {
                Point3d startPoint = elements[i].StartPoint;
                Point3d endPoint = elements[i].EndPoint;

                if (!SupportNodes.Contains(startPoint))
                {
                    if (!FreeNodes.Contains(startPoint))
                        FreeNodes.Add(startPoint);
                    ElementsInStructure[i].StartNodeIndex = FreeNodes.IndexOf(startPoint);
                }

                if (!SupportNodes.Contains(endPoint))
                {
                    if (!FreeNodes.Contains(endPoint))
                        FreeNodes.Add(endPoint);
                    ElementsInStructure[i].EndNodeIndex = FreeNodes.IndexOf(endPoint);
                }
            }




            int dofs = 2 * FreeNodes.Count;
            GlobalLoadVector = Vector<double>.Build.Dense(dofs);

            while (loads.Count < dofs)
                loads.Add(0);

            while (loads.Count > dofs)
                loads.RemoveAt(loads.Count - 1);

            for (int i = 0; i < FreeNodes.Count; i++)
            {
                GlobalLoadVector[2 * i] = loads[2 * i];
                GlobalLoadVector[2 * i + 1] = loads[2 * i + 1];
            }



            GlobalStiffnessMatrix = Matrix<double>.Build.Dense(dofs, dofs);
            GlobalDisplacementVector = Vector<double>.Build.Dense(dofs);

            K_out = new Matrix(dofs, dofs);
            r_out = new Matrix(dofs, 1);
            N_out = new List<double>();

            BrepVisuals = new List<Brep>();
            BrepColors = new List<System.Drawing.Color>();


        }
        */


        // Methods
        protected override int GetDofsPerNode()
        {
            return 2;
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
            foreach (OLDInPlaceElement element in ElementsInStructure)
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
                    GlobalStiffnessMatrix[2 * startIndex, 2 * startIndex] += cc;
                    GlobalStiffnessMatrix[2 * startIndex + 1, 2 * startIndex + 1] += ss;
                    GlobalStiffnessMatrix[2 * startIndex + 1, 2 * startIndex] += sc;
                    GlobalStiffnessMatrix[2 * startIndex, 2 * startIndex + 1] += sc;
                }
                if (endIndex != -1)
                {
                    GlobalStiffnessMatrix[2 * endIndex, 2 * endIndex] += cc;
                    GlobalStiffnessMatrix[2 * endIndex + 1, 2 * endIndex + 1] += ss;
                    GlobalStiffnessMatrix[2 * endIndex + 1, 2 * endIndex] += sc;
                    GlobalStiffnessMatrix[2 * endIndex, 2 * endIndex + 1] += sc;
                }
                if (startIndex != -1 && endIndex != -1)
                {
                    GlobalStiffnessMatrix[2 * startIndex, 2 * endIndex] += -cc;
                    GlobalStiffnessMatrix[2 * startIndex + 1, 2 * endIndex + 1] += -ss;
                    GlobalStiffnessMatrix[2 * startIndex + 1, 2 * endIndex] += -sc;
                    GlobalStiffnessMatrix[2 * startIndex, 2 * endIndex + 1] += -sc;

                    GlobalStiffnessMatrix[2 * endIndex, 2 * startIndex] += -cc;
                    GlobalStiffnessMatrix[2 * endIndex + 1, 2 * startIndex + 1] += -ss;
                    GlobalStiffnessMatrix[2 * endIndex + 1, 2 * startIndex] += -sc;
                    GlobalStiffnessMatrix[2 * endIndex, 2 * startIndex + 1] += -sc;
                }
            }
        }
        public void Solve()
        {
            GlobalDisplacementVector = GlobalStiffnessMatrix.Solve(GlobalLoadVector);

            for (int i = 0; i < FreeNodes.Count; i++)
                FreeNodes[i] += new Point3d(GlobalDisplacementVector[2 * i], GlobalDisplacementVector[2 * i + 1], 0);

            for (int i = 0; i < GlobalDisplacementVector.Count; i++)
                r_out[i, 0] = GlobalDisplacementVector[i];

            for (int i = 0; i < GlobalStiffnessMatrix.RowCount; i++)
            {
                for (int j = 0; j < GlobalStiffnessMatrix.ColumnCount; j++)
                    K_out[i, j] = GlobalStiffnessMatrix[i, j];
            }
        }
        public void Retracking()
        {
            foreach (OLDInPlaceElement element in ElementsInStructure)
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
                N_out.Add(N_element);
            }
        }
        public void GetResultVisuals()
        {
            double f_dim = 355;
            List<double> sigma = new List<double>();
            List<double> utilization = new List<double>();

            System.Drawing.Color supportNodeColor = System.Drawing.Color.AliceBlue;
            System.Drawing.Color freeNodeColor = System.Drawing.Color.AliceBlue;

            for (int i = 0; i < ElementsInStructure.Count; i++)
            {
                sigma.Add(N_out[i] / ElementsInStructure[i].A);
                utilization.Add(sigma[i] / f_dim);
                //N_out[i] = utilization[i];

                if (utilization[i] > 1 || utilization[i] < -1)
                { BrepColors.Add(System.Drawing.Color.Red); }
                else
                { BrepColors.Add(System.Drawing.Color.Green); }


                Point3d startOfElement = ElementsInStructure[i].StartPoint;
                if (ElementsInStructure[i].StartNodeIndex != -1)
                    startOfElement = FreeNodes[ElementsInStructure[i].StartNodeIndex];

                Point3d endOfElement = ElementsInStructure[i].EndPoint;
                if (ElementsInStructure[i].EndNodeIndex != -1)
                    endOfElement = FreeNodes[ElementsInStructure[i].EndNodeIndex];

                Cylinder cylinder = new Cylinder(new Circle(new Plane(startOfElement, new Vector3d(endOfElement - startOfElement)), Math.Sqrt(ElementsInStructure[i].A / Math.PI)), startOfElement.DistanceTo(endOfElement));
                BrepVisuals.Add(cylinder.ToBrep(true, true));
            }

            foreach (Point3d supportNode in SupportNodes)
            {
                double nodeRadius = 40;
                Sphere nodeSphere = new Sphere(supportNode, nodeRadius);
                BrepVisuals.Add(nodeSphere.ToBrep());
                BrepColors.Add(supportNodeColor);

                Plane conePlane = new Plane(supportNode + new Point3d(0, -nodeRadius, 0), new Vector3d(0, -1, 0));
                Cone pinnedCone = new Cone(conePlane, 2 * nodeRadius, 2 * nodeRadius);
                BrepVisuals.Add(pinnedCone.ToBrep(true));
                BrepColors.Add(supportNodeColor);

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
                Vector3d dir = new Vector3d(GlobalLoadVector[2 * i], GlobalLoadVector[2 * i + 1], 0);
                dir.Unitize();
                double arrowLength = Math.Sqrt(GlobalLoadVector[2 * i] * GlobalLoadVector[2 * i] + GlobalLoadVector[2 * i + 1] * GlobalLoadVector[2 * i + 1]) / 1000;
                double lineRadius = 20;

                System.Drawing.Color loadColor = System.Drawing.Color.AliceBlue;

                Point3d startPoint = FreeNodes[i];
                Point3d endPoint = FreeNodes[i] + new Point3d(dir * arrowLength);
                Point3d arrowBase = endPoint + dir * 4 * lineRadius;

                Cylinder loadCylinder = new Cylinder(new Circle(new Plane(startPoint, dir), lineRadius), startPoint.DistanceTo(endPoint));
                BrepVisuals.Add(loadCylinder.ToBrep(true, true));
                BrepColors.Add(loadColor);

                Cone arrow = new Cone(new Plane(arrowBase, new Vector3d(GlobalLoadVector[2 * i], GlobalLoadVector[2 * i + 1], 0)), -4 * lineRadius, 2 * lineRadius);
                BrepVisuals.Add(arrow.ToBrep(true));
                BrepColors.Add(loadColor);
            }
        }
        public string PrintInfo()
        {
            string info = "";
            foreach (OLDInPlaceElement element in ElementsInStructure)
                info += element.getElementInfo() + "\n";
            return info;
        }
        public void ApplyLineLoad(double loadValue, Vector3d loadDirection, Vector3d distributionDirection, List<Line> loadElements)
        {
            loadDirection.Unitize();
            foreach (OLDInPlaceElement element in ElementsInStructure)
            {
                foreach (Line loadElement in loadElements)
                {
                    if (element.StartPoint == loadElement.PointAt(0) && element.EndPoint == loadElement.PointAt(1))
                    {
                        if (element.StartNodeIndex != -1)
                        {
                            GlobalLoadVector[2 * element.StartNodeIndex] += loadValue * Math.Abs(element.ProjectedElementLength(distributionDirection)) * loadDirection[0] / 2;
                            GlobalLoadVector[2 * element.StartNodeIndex + 1] += loadValue * Math.Abs(element.ProjectedElementLength(distributionDirection)) * loadDirection[1] / 2;
                        }

                        if (element.EndNodeIndex != -1)
                        {
                            GlobalLoadVector[2 * element.EndNodeIndex] += loadValue * Math.Abs(element.ProjectedElementLength(distributionDirection)) * loadDirection[0] / 2;
                            GlobalLoadVector[2 * element.EndNodeIndex + 1] += loadValue * Math.Abs(element.ProjectedElementLength(distributionDirection)) * loadDirection[1] / 2;
                        }
                    }
                }
            }
        }
        public string writeElementOutput()  // "Element #1 {  }"
        {
            string output = "ELEMENTS: \n\n";
            foreach (OLDInPlaceElement element in ElementsInStructure)
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
