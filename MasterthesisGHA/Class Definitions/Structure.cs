﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;

namespace MasterthesisGHA
{
    internal abstract class ElementCollection
    {
        // Static Variables
        protected static System.Drawing.Color supportNodeColor;
        protected static System.Drawing.Color freeNodeColor;
        protected static System.Drawing.Color loadArrowColor;
        protected static System.Drawing.Color overUtilizedMemberColor;
        protected static System.Drawing.Color underUtilizedMemberColor;
        protected static System.Drawing.Color memberFromReusableStockColor;


        // Static Constructor
        static ElementCollection()
        {
            supportNodeColor = System.Drawing.Color.Gray;
            freeNodeColor = System.Drawing.Color.AliceBlue;
            loadArrowColor = System.Drawing.Color.White;
            overUtilizedMemberColor = System.Drawing.Color.LightPink;
            underUtilizedMemberColor = System.Drawing.Color.LightGreen;
            memberFromReusableStockColor = System.Drawing.Color.LightYellow;
        }


        // Constructor

    }


    internal abstract class Structure : ElementCollection
    {      
        // Variables
        public List<InPlaceElement> ElementsInStructure;
        public List<Point3d> FreeNodes;
        public List<Point3d> SupportNodes;

        public Matrix<double> GlobalStiffnessMatrix;
        public Vector<double> GlobalLoadVector;
        public Vector<double> GlobalDisplacementVector;

        public List<double> N_out;

        public List<System.Drawing.Color> StructureColors;
        public List<Brep> StructureVisuals;


        // Constructors
        static Structure()
        {
            
        }
        public Structure(List<Line> lines, List<string> profileNames, List<Point3d> supportPoints)
        {           
            StructureVisuals = new List<Brep>();
            StructureColors = new List<System.Drawing.Color>();

            VerifyModel(ref lines, ref supportPoints);

            ElementsInStructure = new List<InPlaceElement>();
            FreeNodes = new List<Point3d>();
            SupportNodes = supportPoints;

            ConstructElementsFromLines(lines, profileNames);

            int dofs = GetDofsPerNode() * FreeNodes.Count;
            GlobalStiffnessMatrix = Matrix<double>.Build.Dense(dofs, dofs);
            GlobalLoadVector = Vector<double>.Build.Dense(dofs);
            GlobalDisplacementVector = Vector<double>.Build.Dense(dofs);
            N_out = new List<double>();

            // Stiffness Matrix
            UpdateGlobalMatrix();

        }


        // Get Functions
        public Matrix GetStiffnessMatrix()
        {
            Matrix K_out = new Matrix(GlobalStiffnessMatrix.RowCount, GlobalStiffnessMatrix.ColumnCount);
            for (int i = 0; i < GlobalStiffnessMatrix.RowCount; i++)
                for (int j = 0; j < GlobalStiffnessMatrix.ColumnCount; j++)
                    K_out[i, j] = GlobalStiffnessMatrix[i, j];
            return K_out;
        }
        public Matrix GetLoadVector()
        {
            Matrix R_out = new Matrix(GlobalLoadVector.Count,1);
            for ( int i = 0; i < GlobalLoadVector.Count; i++)
                R_out[i,0] = GlobalLoadVector[i];
            return R_out;
        }
        public Matrix GetDisplacementVector()
        {
            Matrix r_out = new Matrix(GlobalDisplacementVector.Count, 1);
            for (int i = 0; i < GlobalDisplacementVector.Count; i++)
                r_out[i, 0] = GlobalDisplacementVector[i];
            return r_out;
        }


        // Virtual Methods
        protected virtual void VerifyModel(ref List<Line> lines, ref List<Point3d> anchoredPoints)
        {
            throw new NotImplementedException();

        }
        protected virtual void ConstructElementsFromLines(List<Line> lines, List<string> profileNames)
        {
            throw new NotImplementedException();
        }
        protected virtual int GetDofsPerNode()
        {
            return 0;
        }



        // Virtual Structural Analysis Methods
        protected virtual void UpdateGlobalMatrix()
        {
            throw new NotImplementedException();
        }
        public virtual void Solve()
        {
            throw new NotImplementedException();
        }      
        public virtual void ApplyLineLoad(double loadValue, Vector3d loadDirection, Vector3d distributionDirection, List<Line> loadElements)
        {
            throw new NotImplementedException();
        }
        public virtual void GetLoadVisuals()
        {
            throw new NotImplementedException();
        }
        public virtual void GetResultVisuals()
        {
            throw new NotImplementedException();
        }


        // Virtual Replace Elements
        public virtual void InsertStockElementIntoStructure(int inPlaceElementIndex, ref MaterialBank materialBank, int materialBankElementIndex)
        {
            throw new NotImplementedException();
        }
        public void RemoveStockElementFromStructure()
        {
            throw new NotImplementedException();
        }


    }

    internal class TrussModel3D : Structure
    {
        // Constructors
        public TrussModel3D(List<Line> lines, List<string> profileNames, List<Point3d> supportPoints)
            : base(lines, profileNames, supportPoints)
        {
            

        }


        // New Methods
        protected override void VerifyModel(ref List<Line> lines, ref List<Point3d> anchoredPoints)
        {
            if (lines.Count == 0)
                throw new Exception("Line Input is not valid!");

            if (anchoredPoints.Count < 2)
                throw new Exception("Anchored points needs to be at least 2 to prevent rigid body motion!");

        }
        protected override void ConstructElementsFromLines(List<Line> lines, List<string> profileNames)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                Point3d startPoint = lines[i].PointAt(0);
                Point3d endPoint = lines[i].PointAt(1);
                ElementsInStructure.Add(new InPlaceBarElement3D(ref FreeNodes, ref SupportNodes, profileNames[i], startPoint, endPoint));
            }
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
        public void Retracking()
        {
            foreach (InPlaceBarElement3D element in ElementsInStructure)
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

        

        // Overriden Methods
        protected override int GetDofsPerNode()
        {
            return 3;
        }       
        public override void ApplyLineLoad(double loadValue, Vector3d loadDirection, Vector3d distributionDirection, List<Line> loadElements)
        {
            loadDirection.Unitize();
            int dofsPerNode = GetDofsPerNode();
            foreach (InPlaceBarElement3D element in ElementsInStructure)
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
        public override void Solve()
        {
            GlobalDisplacementVector = GlobalStiffnessMatrix.Solve(GlobalLoadVector);
            int dofsPerNode = GetDofsPerNode();
            
            for (int i = 0; i < FreeNodes.Count; i++)
                FreeNodes[i] += new Point3d(GlobalDisplacementVector[dofsPerNode * i], 
                    GlobalDisplacementVector[dofsPerNode * i + 1], GlobalDisplacementVector[dofsPerNode * i + 2]);

        }
        protected override void UpdateGlobalMatrix()
        {
            foreach (InPlaceElement element in ElementsInStructure)
            {
                Matrix<double> LocalStiffnessMatrix = element.getLocalStiffnessMatrix();
                for (int row = 0; row < LocalStiffnessMatrix.RowCount / 2; row++)
                {
                    for (int col = 0; col < element.getLocalStiffnessMatrix().ColumnCount / 2; col++)
                    {
                        int dofsPerNode = GetDofsPerNode();
                        int StartNodeIndex = element.getStartNodeIndex();
                        int EndNodeIndex = element.getEndNodeIndex();
                        if (StartNodeIndex != -1)
                            GlobalStiffnessMatrix[dofsPerNode * StartNodeIndex + row, dofsPerNode * StartNodeIndex + col]
                                += LocalStiffnessMatrix[row, col];
                        if (EndNodeIndex != -1)
                            GlobalStiffnessMatrix[dofsPerNode * EndNodeIndex + row, dofsPerNode * EndNodeIndex + col]
                                += LocalStiffnessMatrix[row + dofsPerNode, col + dofsPerNode];

                        if (StartNodeIndex != -1 && EndNodeIndex != -1)
                        {
                            GlobalStiffnessMatrix[dofsPerNode * StartNodeIndex + row, dofsPerNode * EndNodeIndex + col]
                                += LocalStiffnessMatrix[row, col + dofsPerNode];
                            GlobalStiffnessMatrix[dofsPerNode * EndNodeIndex + row, dofsPerNode * StartNodeIndex + col]
                                += LocalStiffnessMatrix[row + dofsPerNode, col];

                        }

                    }
                }
            }           
        }
        public override void GetLoadVisuals()
        {
            for (int i = 0; i < FreeNodes.Count; i++)
            {
                int dofsPerNode = GetDofsPerNode();
                Vector3d dir = new Vector3d(GlobalLoadVector[dofsPerNode * i], GlobalLoadVector[dofsPerNode * i + 1],
                    GlobalLoadVector[dofsPerNode * i + 2]);
                dir.Unitize();
                double arrowLength = Math.Sqrt(GlobalLoadVector[dofsPerNode * i] * GlobalLoadVector[dofsPerNode * i]
                    + GlobalLoadVector[dofsPerNode * i + 1] * GlobalLoadVector[dofsPerNode * i + 1]
                    + GlobalLoadVector[dofsPerNode * i + 2] * GlobalLoadVector[dofsPerNode * i + 2]) / 100;
                double lineRadius = 20;

                Point3d startPoint = FreeNodes[i];
                Point3d endPoint = FreeNodes[i] + new Point3d(dir * arrowLength);
                Point3d arrowBase = endPoint + dir * 4 * lineRadius;

                Cylinder loadCylinder = new Cylinder(new Circle(new Plane(startPoint, dir), lineRadius), startPoint.DistanceTo(endPoint));
                StructureVisuals.Add(loadCylinder.ToBrep(true, true));
                StructureColors.Add(Structure.loadArrowColor);

                Cone arrow = new Cone(new Plane(arrowBase, new Vector3d(GlobalLoadVector[dofsPerNode * i], GlobalLoadVector[dofsPerNode * i + 1], GlobalLoadVector[dofsPerNode * i + 2])), -4 * lineRadius, 2 * lineRadius);
                StructureVisuals.Add(arrow.ToBrep(true));
                StructureColors.Add(Structure.loadArrowColor);
            }
        }
        public override void GetResultVisuals()
        {
            double f_dim = 355;
            List<double> sigma = new List<double>();
            List<double> utilization = new List<double>();

            for (int i = 0; i < ElementsInStructure.Count; i++)
            {
                sigma.Add(N_out[i] / ElementsInStructure[i].CrossSectionArea);
                utilization.Add(sigma[i] / f_dim);
                //N_out[i] = utilization[i];

                if (utilization[i] > 1 || utilization[i] < -1)
                { StructureColors.Add(overUtilizedMemberColor); }
                else
                { StructureColors.Add(underUtilizedMemberColor); }


                Point3d startOfElement = ElementsInStructure[i].getStartPoint();
                if (ElementsInStructure[i].getStartNodeIndex() != -1)
                    startOfElement = FreeNodes[ElementsInStructure[i].getStartNodeIndex()];

                Point3d endOfElement = ElementsInStructure[i].getEndPoint();
                if (ElementsInStructure[i].getEndNodeIndex() != -1)
                    endOfElement = FreeNodes[ElementsInStructure[i].getEndNodeIndex()];

                Cylinder cylinder = new Cylinder(new Circle(new Plane(startOfElement, new Vector3d(endOfElement - startOfElement)), Math.Sqrt(ElementsInStructure[i].CrossSectionArea / Math.PI)), startOfElement.DistanceTo(endOfElement));
                StructureVisuals.Add(cylinder.ToBrep(true, true));
            }

            foreach (Point3d supportNode in SupportNodes)
            {
                double nodeRadius = 40;
                Sphere nodeSphere = new Sphere(supportNode, nodeRadius);
                StructureVisuals.Add(nodeSphere.ToBrep());
                StructureColors.Add(Structure.supportNodeColor);

                Plane conePlane = new Plane(supportNode + new Point3d(0, 0, -nodeRadius), new Vector3d(0, 0, -1));
                Cone pinnedCone = new Cone(conePlane, 2 * nodeRadius, 2 * nodeRadius);
                StructureVisuals.Add(pinnedCone.ToBrep(true));
                StructureColors.Add(Structure.supportNodeColor);

            }

            foreach (Point3d freeNode in FreeNodes)
            {
                double nodeRadius = 40;
                Sphere nodeSphere = new Sphere(freeNode, nodeRadius);
                StructureVisuals.Add(nodeSphere.ToBrep());
                StructureColors.Add(Structure.freeNodeColor);

            }


        }

        // Replace Element
        public override void InsertStockElementIntoStructure(int inPlaceElementIndex, ref MaterialBank materialBank, int materialBankElementIndex)
        {
            if (inPlaceElementIndex < 0 || inPlaceElementIndex > ElementsInStructure.Count)
                throw new Exception("The In-Place-Element index " + inPlaceElementIndex.ToString() + " is not valid!");

            ElementsInStructure.Add(new InPlaceBarElement3D(ref materialBank, materialBankElementIndex, 
                ElementsInStructure[inPlaceElementIndex].getStartPoint(), ElementsInStructure[inPlaceElementIndex].getEndPoint()));


        }


        // Unused Methods
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
        public string writeElementOutput()  // "Element #1 {  }"
        {
            string output = "ELEMENTS: \n\n";
            /*
            foreach (InPlaceBarElement element in ElementsInStructure)
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
            */
            return output;
        }
    }

    internal class TrussModel2D : TrussModel3D
    {   
        // Constructors
        public TrussModel2D(List<Line> lines, List<string> profileNames, List<Point3d> supportPoints)
            : base(lines, profileNames, supportPoints)
        {

        }
               

        // Overriden Methods
        protected override int GetDofsPerNode()
        {
            return 2;
        }
        protected override void ConstructElementsFromLines(List<Line> lines, List<string> profileNames)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                Point3d startPoint = lines[i].PointAt(0);
                Point3d endPoint = lines[i].PointAt(1);
                ElementsInStructure.Add(new InPlaceBarElement2D(ref FreeNodes, ref SupportNodes, profileNames[i], startPoint, endPoint));
            }
        }
        public override void ApplyLineLoad(double loadValue, Vector3d loadDirection, Vector3d distributionDirection, List<Line> loadElements)
        {
            loadDirection.Unitize();
            int dofsPerNode = GetDofsPerNode();
            foreach (InPlaceBarElement3D element in ElementsInStructure)
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
                                += loadValue * Math.Abs(element.ProjectedElementLength(distributionDirection)) * loadDirection[2] / 2;                          
                        }

                        if (element.EndNodeIndex != -1)
                        {
                            GlobalLoadVector[dofsPerNode * element.EndNodeIndex]
                                += loadValue * Math.Abs(element.ProjectedElementLength(distributionDirection)) * loadDirection[0] / 2;
                            GlobalLoadVector[dofsPerNode * element.EndNodeIndex + 1]
                                += loadValue * Math.Abs(element.ProjectedElementLength(distributionDirection)) * loadDirection[2] / 2;
                        }
                    }
                }
            }
        }
        public override void Solve()
        {
            GlobalDisplacementVector = GlobalStiffnessMatrix.Solve(GlobalLoadVector);
            int dofsPerNode = GetDofsPerNode();

            for (int i = 0; i < FreeNodes.Count; i++)
                FreeNodes[i] += new Point3d(GlobalDisplacementVector[dofsPerNode * i], 0, GlobalDisplacementVector[dofsPerNode * i + 1]);

        }
        protected override void UpdateGlobalMatrix()
        {
            foreach (InPlaceElement element in ElementsInStructure)
            {
                Matrix<double> LocalStiffnessMatrix = element.getLocalStiffnessMatrix();
                for (int row = 0; row < LocalStiffnessMatrix.RowCount / 2; row++)
                {
                    for (int col = 0; col < element.getLocalStiffnessMatrix().ColumnCount / 2; col++)
                    {
                        int dofsPerNode = GetDofsPerNode();
                        int StartNodeIndex = element.getStartNodeIndex();
                        int EndNodeIndex = element.getEndNodeIndex();

                        if (StartNodeIndex != -1)
                            GlobalStiffnessMatrix[dofsPerNode * StartNodeIndex + row, dofsPerNode * StartNodeIndex + col]
                                += LocalStiffnessMatrix[row, col];

                        if (EndNodeIndex != -1)
                            GlobalStiffnessMatrix[dofsPerNode * EndNodeIndex + row, dofsPerNode * EndNodeIndex + col]
                                += LocalStiffnessMatrix[row + dofsPerNode, col + dofsPerNode];

                        if (StartNodeIndex != -1 && EndNodeIndex != -1)
                        {
                            GlobalStiffnessMatrix[dofsPerNode * StartNodeIndex + row, dofsPerNode * EndNodeIndex + col]
                                += LocalStiffnessMatrix[row, col + dofsPerNode];
                            GlobalStiffnessMatrix[dofsPerNode * EndNodeIndex + row, dofsPerNode * StartNodeIndex + col]
                                += LocalStiffnessMatrix[row + dofsPerNode, col];
                        }

                    }
                }
            }
        }
        public override void GetLoadVisuals()
        {
            for (int i = 0; i < FreeNodes.Count; i++)
            {
                int dofsPerNode = GetDofsPerNode();

                Vector3d dir = new Vector3d(GlobalLoadVector[dofsPerNode * i], 0, GlobalLoadVector[dofsPerNode * i + 1]);
                dir.Unitize();
                double arrowLength = Math.Sqrt(GlobalLoadVector[dofsPerNode * i] * GlobalLoadVector[dofsPerNode * i] +
                    GlobalLoadVector[dofsPerNode * i + 1] * GlobalLoadVector[dofsPerNode * i + 1]) / 100;
                double lineRadius = 20;

                System.Drawing.Color loadColor = System.Drawing.Color.AliceBlue;

                Point3d startPoint = FreeNodes[i];
                Point3d endPoint = FreeNodes[i] + new Point3d(dir * arrowLength);
                Point3d arrowBase = endPoint + dir * 4 * lineRadius;

                Cylinder loadCylinder = new Cylinder(new Circle(new Plane(startPoint, dir), lineRadius), startPoint.DistanceTo(endPoint));
                StructureVisuals.Add(loadCylinder.ToBrep(true, true));
                StructureColors.Add(loadColor);

                Cone arrow = new Cone(new Plane(arrowBase, new Vector3d(GlobalLoadVector[dofsPerNode * i], 0, GlobalLoadVector[dofsPerNode * i + 1])), -4 * lineRadius, 2 * lineRadius);
                StructureVisuals.Add(arrow.ToBrep(true));
                StructureColors.Add(loadColor);
            }
        }


        // Unused
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
        public void Assemble()
        {
            foreach (InPlaceElement element in ElementsInStructure)
            {
                double dz = element.getEndPoint().Z - element.getStartPoint().Z;
                double dx = element.getEndPoint().X - element.getStartPoint().X;
                double rad = Math.Atan2(dz, dx);
                double EAL = (element.CrossSectionArea * element.YoungsModulus / element.getStartPoint().DistanceTo(element.getEndPoint()));
                double cc = Math.Cos(rad) * Math.Cos(rad) * EAL;
                double ss = Math.Sin(rad) * Math.Sin(rad) * EAL;
                double sc = Math.Sin(rad) * Math.Cos(rad) * EAL;

                int startIndex = element.getStartNodeIndex();
                int endIndex = element.getEndNodeIndex();

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
        public string PrintInfo()
        {
            string info = "";
            foreach (AbstractLineElement element in ElementsInStructure)
                info += element.getElementInfo() + "\n";
            return info;
        } 

    }



    internal class MaterialBank : ElementCollection
    {
        // Variables
        public List<StockElement> StockElementsInMaterialBank;
        public List<System.Drawing.Color> MaterialBankColors;
        public List<Brep> MaterialBankVisuals;



        // Constructor
        public MaterialBank() 
            : base()
        {
            StockElementsInMaterialBank = new List<StockElement>();
            MaterialBankColors = new List<System.Drawing.Color>();
            MaterialBankVisuals = new List<Brep>();
        }



        // Replace Elements
        public void InsertStockElementIntoMaterialBank(InPlaceElement inPlaceElement)
        {
            throw new NotImplementedException();
        }
        public void RemoveStockElementFromMaterialBank(StockElement stockElement)
        {
            throw new NotImplementedException();
        }



        public List<Brep> VisualizeMaterialBank(int groupingMethod)
        {
            if (groupingMethod < 0 || groupingMethod > 1)
                throw new Exception("Grouping Methods are: \n0 - By Length \n1 - By Profile");

            List<Brep> outList = new List<Brep>();
            double group = -1;
            double instance = 0;
            double spacing = 100;

            if (groupingMethod == 0)
            {
                // sort by length
            }
            else if (groupingMethod == 1)
            {
                // sort by profile
            }

            StockElement priorElement = StockElementsInMaterialBank[0];
            foreach (StockElement element in StockElementsInMaterialBank)
            {
                if (groupingMethod == 0)
                {
                    if (priorElement.GetStockElementLength() != element.GetStockElementLength())
                        group++;
                }
                else if (groupingMethod == 1)
                {
                    if (priorElement.ProfileName != element.ProfileName)
                        group++;
                }

                Plane basePlane = new Plane(new Point3d(instance, 0, group), new Vector3d(0, 1, 0));
                Circle baseCircle = new Circle(basePlane, Math.Sqrt(element.CrossSectionArea) / Math.PI);
                Cylinder cylinder = new Cylinder(baseCircle, element.GetStockElementLength());
                outList.Add(cylinder.ToBrep(true, true));
                instance = instance + 2 * Math.Sqrt(element.CrossSectionArea) / Math.PI + spacing;

                priorElement = element;
                
            }

            return outList;
        }




    }


}
