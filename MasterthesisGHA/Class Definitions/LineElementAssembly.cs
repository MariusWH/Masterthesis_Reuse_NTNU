using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;

namespace MasterthesisGHA
{
    public abstract class ElementCollection
    {
        // Static Variables
        protected static System.Drawing.Color supportNodeColor;
        protected static System.Drawing.Color freeNodeColor;
        protected static System.Drawing.Color loadArrowColor;

        protected static System.Drawing.Color unverifiedMemberColor;
        protected static System.Drawing.Color verifiedMemberColor;
        protected static System.Drawing.Color overUtilizedMemberColor;      
        protected static System.Drawing.Color bucklingMemberColor;

        public static System.Drawing.Color reuseMemberColor;
        public static System.Drawing.Color newMemeberColorColor;

        public static List<System.Drawing.Color> materialBankColors;

        // Static Constructor
        static ElementCollection()
        {
            System.Drawing.Color darkerGrey = System.Drawing.Color.FromArgb(100, 100, 100);

            supportNodeColor = darkerGrey;
            freeNodeColor = System.Drawing.Color.DarkGray;
            loadArrowColor = System.Drawing.Color.White;

            verifiedMemberColor = System.Drawing.Color.White;
            verifiedMemberColor = System.Drawing.Color.Green;
            overUtilizedMemberColor = System.Drawing.Color.Red;
            bucklingMemberColor = System.Drawing.Color.Yellow;
            
            reuseMemberColor = System.Drawing.Color.Blue;
            materialBankColors = new List<System.Drawing.Color> 
            {                  
                System.Drawing.Color.DeepSkyBlue, 
                System.Drawing.Color.LightYellow,
                System.Drawing.Color.Salmon,
                System.Drawing.Color.MediumPurple,
                System.Drawing.Color.LightGreen
            };
        }

        // Static Methods
        public static Grasshopper.DataTree<StockElement> GetOutputDataTree(List<List<StockElement>> inputDataTree)
        {
            Grasshopper.DataTree<StockElement> dataTree = new Grasshopper.DataTree<StockElement>();

            int outerCount = 0;
            foreach (List<StockElement> list in inputDataTree)
            {
                int innerCount = 0;
                foreach (StockElement reusableElement in list)
                {
                    Grasshopper.Kernel.Data.GH_Path path = new Grasshopper.Kernel.Data.GH_Path(new int[] { outerCount });
                    dataTree.Insert(reusableElement, path, innerCount);
                    innerCount++;
                }
                outerCount++;
            }
            return dataTree;
        }
        public static Grasshopper.DataTree<T> GetOutputDataTree<T>(List<List<T>> inputDataTree)
        {
            Grasshopper.DataTree<T> dataTree = new Grasshopper.DataTree<T>();

            int outerCount = 0;
            foreach (List<T> list in inputDataTree)
            {
                int innerCount = 0;
                foreach (T element in list)
                {
                    Grasshopper.Kernel.Data.GH_Path path = new Grasshopper.Kernel.Data.GH_Path(new int[] { outerCount });
                    dataTree.Insert(element, path, innerCount);
                    innerCount++;
                }
                outerCount++;
            }
            return dataTree;
        }
        public static Grasshopper.DataTree<T> GetOutputDataTree<T>(IEnumerable<IEnumerable<T>> inputDataTree)
        {
            Grasshopper.DataTree<T> dataTree = new Grasshopper.DataTree<T>();

            int outerCount = 0;
            foreach (IEnumerable<T> list in inputDataTree)
            {
                int innerCount = 0;
                foreach (T element in list)
                {
                    Grasshopper.Kernel.Data.GH_Path path = new Grasshopper.Kernel.Data.GH_Path(new int[] { outerCount });
                    dataTree.Insert(element, path, innerCount);
                    innerCount++;
                }
                outerCount++;
            }
            return dataTree;
        }
    }






    public abstract class Structure : ElementCollection
    {      
        // Variables
        public List<InPlaceElement> ElementsInStructure;        
        public List<Point3d> FreeNodes;
        public List<Point3d> FreeNodesInitial;
        public List<Point3d> SupportNodes;
        public Matrix<double> GlobalStiffnessMatrix;
        public Vector<double> GlobalLoadVector;
        public Vector<double> GlobalDisplacementVector;
        public List<double> ElementAxialForce;
        public List<double> ElementUtilization;
        public List<System.Drawing.Color> StructureColors;
        public List<Brep> StructureVisuals;

        // Constructors
        static Structure()
        {
        }
        public Structure()
        {
            ElementsInStructure = new List<InPlaceElement>();
            FreeNodes = new List<Point3d>();
            FreeNodesInitial = new List<Point3d>();
            SupportNodes = new List<Point3d>();
            GlobalStiffnessMatrix = Matrix<double>.Build.Sparse(0,0);
            GlobalLoadVector = Vector<double>.Build.Sparse(0);
            GlobalDisplacementVector = Vector<double>.Build.Sparse(0);
            ElementAxialForce = new List<double>();
            ElementUtilization = new List<double>();
            StructureColors = new List<System.Drawing.Color>();
            StructureVisuals = new List<Brep>();
    }
        public Structure(List<Line> lines, List<string> profileNames, List<Point3d> supportPoints)
        {           
            StructureVisuals = new List<Brep>();
            StructureColors = new List<System.Drawing.Color>();
            FreeNodesInitial = new List<Point3d>();

            VerifyModel(ref lines, ref supportPoints);           

            ElementsInStructure = new List<InPlaceElement>();
            FreeNodes = new List<Point3d>();
            SupportNodes = supportPoints;

            ConstructElementsFromLines(lines, profileNames);

            int dofs = GetDofsPerNode() * FreeNodes.Count;
            GlobalStiffnessMatrix = Matrix<double>.Build.Dense(dofs, dofs);
            GlobalLoadVector = Vector<double>.Build.Dense(dofs);
            GlobalDisplacementVector = Vector<double>.Build.Dense(dofs);
            ElementAxialForce = new List<double>();
            ElementUtilization = new List<double>();

            // Stiffness Matrix
            RecalculateGlobalMatrix();

        }
        public Structure(Structure copyFromThis)
            : this()
        {
            ElementsInStructure = new List<InPlaceElement>(copyFromThis.ElementsInStructure);
            FreeNodes = new List<Point3d>(copyFromThis.FreeNodes);
            FreeNodesInitial = new List<Point3d>(copyFromThis.FreeNodesInitial);
            SupportNodes = new List<Point3d>(copyFromThis.SupportNodes);
            GlobalStiffnessMatrix = Matrix<double>.Build.SameAs(copyFromThis.GlobalStiffnessMatrix);
            GlobalLoadVector = Vector<double>.Build.SameAs(copyFromThis.GlobalLoadVector);
            GlobalDisplacementVector = Vector<double>.Build.SameAs(copyFromThis.GlobalDisplacementVector);
            ElementAxialForce = new List<double>(copyFromThis.ElementAxialForce);
            ElementUtilization = new List<double>(copyFromThis.ElementUtilization);
            StructureColors = new List<System.Drawing.Color>(copyFromThis.StructureColors);
            StructureVisuals = new List<Brep>(copyFromThis.StructureVisuals);
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
        public double GetNewMass()
        {
            double mass = 0;
            foreach (InPlaceElement element in ElementsInStructure)
                if (!element.IsFromMaterialBank)
                    mass += element.getMass();
            return mass;
        }
        public double GetReusedMass()
        {
            double mass = 0;
            foreach (InPlaceElement element in ElementsInStructure)
                if (element.IsFromMaterialBank)
                    mass += element.getMass();
            return mass;
        }
        public double GetTotalMass()
        {
            double mass = 0;
            foreach (InPlaceElement element in ElementsInStructure)
                mass += element.getMass();
            return mass;
        }
        public double GetStructureSize()
        {
            double xMin = FreeNodes[0].X;
            double xMax = FreeNodes[0].X;
            double yMin = FreeNodes[0].Y;
            double yMax = FreeNodes[0].Y;
            double zMin = FreeNodes[0].Z;
            double zMax = FreeNodes[0].Z;

            foreach (Point3d point in FreeNodes)
            {
                xMin = Math.Min(xMin, point.X);
                xMax = Math.Max(xMax, point.X);
                yMin = Math.Min(yMin, point.Y);
                yMax = Math.Max(yMax, point.Y);
                zMin = Math.Min(zMin, point.Z);
                zMax = Math.Max(zMax, point.Z);
            }

            return Math.Sqrt(
                (xMax - xMin) * (xMax - xMin) +
                (yMax - yMin) * (yMax - yMin) +
                (zMax - zMin) * (zMax - zMin) );
        }
        public double GetMaxLoad()
        {
            int dofsPerNode = GetDofsPerNode();
            double max = 0;
            double temp;

            for (int i = 0; i < dofsPerNode * FreeNodes.Count; i += dofsPerNode)
            {
                temp = 0;
                for (int j = 0; j < dofsPerNode; j++)
                {
                    temp += GlobalLoadVector[i + j] * GlobalLoadVector[i + j];
                }
                max = Math.Max(max, Math.Sqrt(temp));
            }
            return max;
        }
        public double GetMaxDisplacement()
        {
            int dofsPerNode = GetDofsPerNode();
            double max = 0;
            double temp;

            for (int i = 0; i < dofsPerNode * FreeNodes.Count; i += dofsPerNode)
            {
                temp = 0;
                for (int j = 0; j < dofsPerNode; j++)
                {
                    temp += GlobalDisplacementVector[i + j] * GlobalDisplacementVector[i + j];
                }
                max = Math.Max(max, Math.Sqrt(temp));
            }
            return max;
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
        public virtual string PrintStructureInfo()
        {
            throw new NotImplementedException();           
        }


        // Virtual Structural Analysis Methods
        protected virtual void RecalculateGlobalMatrix()
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
        public virtual void GetLoadVisuals(out List<Brep> geometry, out List<System.Drawing.Color> color, double size = -1, double maxLoad = -1, double maxDisplacement = -1)
        {
            throw new NotImplementedException();
        }
        public virtual void GetResultVisuals(out List<Brep> geometry, out List<System.Drawing.Color> color, int colorTheme, double size = -1, double maxDisplacement = -1)
        {
            throw new NotImplementedException();
        }     
        public virtual void GetVisuals(out List<Brep> geometry, out List<System.Drawing.Color> color, int colorTheme, double size, double maxDisplacement, double maxLoad)
        {
            geometry = new List<Brep>();
            color = new List<System.Drawing.Color>();
        }

        // Linear Element Replacement Method
        public void InsertNewElements()
        {
            List<string> areaSortedElements = AbstractLineElement.GetCrossSectionAreaSortedProfilesList();

            for (int i = 0; i < ElementsInStructure.Count; i++)
                InsertNewElement(i, areaSortedElements, Math.Abs(ElementAxialForce[i] / 355));
        }
        public void InsertMaterialBank(MaterialBank materialBank, out MaterialBank remainingMaterialBank)
        {
            List<List<StockElement>> possibleStockElements = PossibleStockElementForEachInPlaceElement(materialBank);

            for (int i = 0; i < ElementsInStructure.Count; i++)
            {
                List<StockElement> sortedStockElementList = new MaterialBank(possibleStockElements[i])
                    .getUtilizationThenLengthSortedMaterialBank(ElementAxialForce[i]);
                int index = sortedStockElementList.Count;
                while (index-- != 0)
                {
                    if (InsertStockElement(i, ref materialBank, sortedStockElementList[index], true))
                    {
                        break;
                    }
                }
            }

            materialBank.UpdateVisuals();
            remainingMaterialBank = materialBank.DeepCopy();
        }
        public void InsertMaterialBank(List<int> insertOrder, MaterialBank materialBank, out MaterialBank remainingMaterialBank)
        {
            if (insertOrder.Count != ElementsInStructure.Count)
                throw new Exception("InsertOrder contains " + insertOrder.Count + " elements, while the structure contains "
                    + ElementsInStructure.Count + " members");

            List<List<StockElement>> possibleStockElements = PossibleStockElementForEachInPlaceElement(materialBank);

            for (int i = 0; i < ElementsInStructure.Count; i++)
            {
                int elementIndex = insertOrder[i];
                List<StockElement> sortedStockElementList = new MaterialBank(possibleStockElements[elementIndex])
                    .getUtilizationThenLengthSortedMaterialBank(ElementAxialForce[elementIndex]);
                int index = sortedStockElementList.Count;
                while (index-- != 0)
                {
                    if (InsertStockElement(elementIndex, ref materialBank, sortedStockElementList[index], true))
                    {
                        break;
                    }
                }
            }

            materialBank.UpdateVisuals();
            remainingMaterialBank = materialBank.DeepCopy();
        }        
        public void InsertMaterialBankThenNewElements(MaterialBank materialBank, out MaterialBank remainingMaterialBank)
        {
            List<List<StockElement>> possibleStockElements = PossibleStockElementForEachInPlaceElement(materialBank);
            List<string> areaSortedElements = AbstractLineElement.GetCrossSectionAreaSortedProfilesList();

            for (int i = 0; i < ElementsInStructure.Count; i++)
            {
                List<StockElement> sortedStockElementList = new MaterialBank(possibleStockElements[i])
                    .getUtilizationThenLengthSortedMaterialBank(ElementAxialForce[i]);
                int index = sortedStockElementList.Count;
                bool insertNew = true;
                while (index-- != 0)
                {
                    if (InsertStockElement(i, ref materialBank, sortedStockElementList[index]))
                    {
                        insertNew = false;
                        break;
                    }
                }
                if (insertNew)
                    InsertNewElement(i, areaSortedElements, Math.Abs(ElementAxialForce[i] / 355));
            }

            materialBank.UpdateVisuals();
            remainingMaterialBank = materialBank.DeepCopy();
        }
        public void InsertMaterialBankThenNewElements(List<int> insertOrder, MaterialBank materialBank, out MaterialBank remainingMaterialBank)
        {
            if (insertOrder.Count != ElementsInStructure.Count)
                throw new Exception("InsertOrder contains " + insertOrder.Count + " elements, while the structure contains "
                    + ElementsInStructure.Count + " members");

            List<List<StockElement>> possibleStockElements = PossibleStockElementForEachInPlaceElement(materialBank);
            List<string> areaSortedElements = AbstractLineElement.GetCrossSectionAreaSortedProfilesList();

            for (int i = 0; i < ElementsInStructure.Count; i++)
            {
                int elementIndex = insertOrder[i];
                List<StockElement> sortedStockElementList = new MaterialBank(possibleStockElements[elementIndex])
                    .getUtilizationThenLengthSortedMaterialBank(ElementAxialForce[elementIndex]);
                int index = sortedStockElementList.Count;
                bool insertNew = true;
                while (index-- != 0)
                {
                    if (InsertStockElement(elementIndex, ref materialBank, sortedStockElementList[index]))
                    {
                        insertNew = false;
                        break;
                    }
                }
                if (insertNew)
                    InsertNewElement(elementIndex, areaSortedElements, Math.Abs(ElementAxialForce[elementIndex] / 355));
            }

            materialBank.UpdateVisuals();
            remainingMaterialBank = materialBank.DeepCopy();
        }







        // Brute Force Optimum Replacement      
        public IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> list, int length)
        {
            if (length == 1) 
                return list.Select(t => new T[] { t });

            return GetPermutations(list, length - 1).SelectMany(t => list.Where(e => !t.Contains(e)),
                    (t1, t2) => t1.Concat(new T[] { t2 }));
        }


        /*
        public List<List<int>> GetPermutations(List<List<int>> list, int length)
        {
            if (length == 1)
                return (List<List<int>>)list.Select(t => new List<List<int>>(){ t });

            return (List<List<int>>)GetPermutations(list, length - 1).SelectMany(t => list.Where(e => !t.Contains(e)),
                    (t1, t2) => t1.Concat(new List<List<int>>() { t2 }));
        }*/


        /*
        public List<List<int>> createAllOrderedLists(int listCount)
        {
            int factorial = Enumerable.Range(1, listCount).Aggregate(1, (p, item) => p * item);           
            List<List<int>> indexLists = new List<List<int>>(factorial);

            List<int> initalList = new List<int>();
            for (int i = 0; i < listCount; i++)
            {
                initalList.Add(i);
            }

            return GetPermutations(indexLists, listCount);
        }*/
        public void InsertMaterialBankBruteForce(MaterialBank materialBank, out MaterialBank remainingMaterialBank)
        {                       
            List<int> initalList = new List<int>();
            for (int i = 0; i < ElementsInStructure.Count; i++)
            {
                initalList.Add(i);
            }
            IEnumerable<IEnumerable<int>> allOrderedLists = GetPermutations(initalList, initalList.Count);
            int factorial = Enumerable.Range(1, initalList.Count).Aggregate(1, (p, item) => p * item);


            InsertNewElements();

            for (int i = 0; i < factorial; i++)
            {
                TrussModel3D tempCopy = new TrussModel3D(this);
                tempCopy.InsertMaterialBank(materialBank, out remainingMaterialBank);
                

                tempCopy.GetTotalMass();
            }








            remainingMaterialBank = materialBank.DeepCopy();

        }


        // Virtual Element Replacement Functions
        public virtual List<List<StockElement>> PossibleStockElementForEachInPlaceElement(MaterialBank materialBank)
        {
            throw new NotImplementedException();
        }
        public virtual bool InsertStockElement(int inPlaceElementIndex, ref MaterialBank materialBank, StockElement stockElement, bool keepCutOff = true)
        {
            throw new NotImplementedException();
        }
        public virtual void InsertNewElement(int inPlaceElementIndex, List<string> criteraSortedNewElements, double criteria)
        {
            throw new NotImplementedException();
        }      
        
    }







    public class TrussModel3D : Structure
    {
        // Constructors
        public TrussModel3D()
            : base()
        {

        }
        public TrussModel3D(List<Line> lines, List<string> profileNames, List<Point3d> supportPoints)
            : base(lines, profileNames, supportPoints)
        {
            

        }
        public TrussModel3D(Structure copyFromThis)
            : this()
        {
            ElementsInStructure = new List<InPlaceElement>( copyFromThis.ElementsInStructure );
            FreeNodes = new List<Point3d>( copyFromThis.FreeNodes );
            FreeNodesInitial = new List<Point3d>( copyFromThis.FreeNodesInitial );
            SupportNodes = new List<Point3d>( copyFromThis.SupportNodes );
            GlobalStiffnessMatrix = Matrix<double>.Build.SameAs(copyFromThis.GlobalStiffnessMatrix);
            GlobalLoadVector = Vector<double>.Build.SameAs(copyFromThis.GlobalLoadVector);
            GlobalDisplacementVector = Vector<double>.Build.SameAs(copyFromThis.GlobalDisplacementVector);
            ElementAxialForce = new List<double>( copyFromThis.ElementAxialForce );
            ElementUtilization = new List<double>( copyFromThis.ElementUtilization );
            StructureColors = new List<System.Drawing.Color>( copyFromThis.StructureColors );
            StructureVisuals = new List<Brep>( copyFromThis.StructureVisuals );
        }

        // General Methods
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
            //FreeNodesInitial = FreeNodes;
            FreeNodesInitial = FreeNodes.Select(node => new Point3d(node)).ToList();
        }
        public override string PrintStructureInfo()
        {
            string info = "3D Truss Structure:\n";
            foreach (InPlaceBarElement3D inPlaceBarElement3D in ElementsInStructure)
            {
                info += "\n" + inPlaceBarElement3D.getElementInfo();
            }
            return info;
        }
        protected override int GetDofsPerNode()
        {
            return 3;
        }       
        
        // Structural Analysis
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
        protected override void RecalculateGlobalMatrix()
        {
            GlobalStiffnessMatrix.Clear();
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
        public override void Solve()
        {
            GlobalDisplacementVector = GlobalStiffnessMatrix.Solve(GlobalLoadVector);
            int dofsPerNode = GetDofsPerNode();

            for (int i = 0; i < FreeNodes.Count; i++)
                FreeNodes[i] += new Point3d(GlobalDisplacementVector[dofsPerNode * i],
                    GlobalDisplacementVector[dofsPerNode * i + 1], GlobalDisplacementVector[dofsPerNode * i + 2]);

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

                ElementAxialForce.Add((element.CrossSectionArea * element.YoungsModulus) * (L1 - L0) / L0);
                ElementUtilization.Add(element.YoungsModulus * (L1 - L0) / L0 / element.YieldStress);
            }
        }

        // Visuals
        protected double getStructureSizeFactor(double factorOfLength, double structureSize)
        {
            return factorOfLength * structureSize;
        }
        protected double getDisplacementFactor(double factorOfLength, double structureSize, double maxDisplacement )
        {
            return factorOfLength * structureSize / maxDisplacement;
        }
        protected double getLoadFactor(double factorOfLength, double structureSize, double maxLoad)
        {
            return factorOfLength * structureSize / maxLoad;
        }
        public override void GetLoadVisuals(out List<Brep> geometry, out List<System.Drawing.Color> color, double size = -1, double maxLoad = -1, double maxDisplacement = -1)
        {
            geometry = new List<Brep>();
            color = new List<System.Drawing.Color>();

            double displacementFactor = getDisplacementFactor(0.02, size, maxDisplacement);
            double loadLineRadius = getStructureSizeFactor(2e-3, size);

            List<Point3d> freeNodeDisplacement = new List<Point3d>();
            for (int j = 0; j < FreeNodes.Count; j++)
            {
                freeNodeDisplacement.Add(displacementFactor * (new Point3d(FreeNodes[j] - FreeNodesInitial[j])));
            }

            int dofsPerNode = GetDofsPerNode();
            Vector<double> pointLoadVector = Vector<double>.Build.Dense(3 * FreeNodes.Count);
            Vector<double> momentLoadVector = Vector<double>.Build.Dense(3 * FreeNodes.Count);

            switch (dofsPerNode)
            {
                case 3:
                    pointLoadVector = GlobalLoadVector;
                    break;

                case 2:
                    for (int i = 0; i < FreeNodes.Count; i++)
                    {
                        pointLoadVector[3 * i] = GlobalLoadVector[2 * i];
                        pointLoadVector[3 * i + 2] = GlobalLoadVector[2 * i + 1];
                    }
                    break;
            }

            dofsPerNode = 3;
            for (int i = 0; i < FreeNodes.Count; i++)
            {
                Vector3d dir = new Vector3d(
                    pointLoadVector[dofsPerNode * i],
                    pointLoadVector[dofsPerNode * i + 1],
                    pointLoadVector[dofsPerNode * i + 2]);

                double arrowLength = Math.Sqrt(
                    pointLoadVector[dofsPerNode * i] * pointLoadVector[dofsPerNode * i] +
                    pointLoadVector[dofsPerNode * i + 1] * pointLoadVector[dofsPerNode * i + 1] +
                    pointLoadVector[dofsPerNode * i + 2] * pointLoadVector[dofsPerNode * i + 2]) *
                    getLoadFactor(0.1,size, maxLoad);

                dir.Unitize();                
                double coneHeight = 6 * loadLineRadius;
                double coneRadius = 3 * loadLineRadius;

                Point3d startPoint = new Point3d( FreeNodesInitial[i] + freeNodeDisplacement[i] );
                Point3d endPoint = startPoint + new Point3d(dir * arrowLength);
                Point3d arrowBase = endPoint + dir * coneHeight;
                Cylinder loadCylinder = new Cylinder(new Circle(new Plane(startPoint, dir), loadLineRadius), startPoint.DistanceTo(endPoint));
                Cone arrow = new Cone(new Plane(arrowBase, new Vector3d(
                    pointLoadVector[dofsPerNode * i],
                    pointLoadVector[dofsPerNode * i + 1],
                    pointLoadVector[dofsPerNode * i + 2])),
                    -coneHeight, coneRadius);

                StructureVisuals.Add(loadCylinder.ToBrep(true, true));
                StructureColors.Add(Structure.loadArrowColor);
                StructureVisuals.Add(arrow.ToBrep(true));
                StructureColors.Add(Structure.loadArrowColor);

                geometry.Add(loadCylinder.ToBrep(true, true));
                color.Add(Structure.loadArrowColor);
                geometry.Add(arrow.ToBrep(true));
                color.Add(Structure.loadArrowColor);
            }
        }
        public override void GetResultVisuals(out List<Brep> geometry, out List<System.Drawing.Color> color, int colorTheme, double size = -1, double maxDisplacement = -1)
        {
            geometry = new List<Brep>();
            color = new List<System.Drawing.Color>();

            double displacementFactor = getDisplacementFactor(0.02, size, maxDisplacement);
            double nodeRadius = getStructureSizeFactor(5e-3, size);
            List<Point3d> normalizedNodeDisplacement = new List<Point3d>();
            for (int j = 0; j < FreeNodes.Count; j++)
            {
                normalizedNodeDisplacement.Add(displacementFactor * (new Point3d(FreeNodes[j] - FreeNodesInitial[j])));
            }
           

            for (int i = 0; i < ElementsInStructure.Count; i++)
            {
                if (ElementsInStructure[i].IsFromMaterialBank)
                {
                    StructureColors.Add(reuseMemberColor);
                    color.Add(reuseMemberColor);
                }                    
                else if (ElementsInStructure[i].CheckAxialBuckling(ElementAxialForce[i]) > 1)
                {
                    StructureColors.Add(bucklingMemberColor);
                    color.Add(bucklingMemberColor);
                }                   
                else if (ElementsInStructure[i].CheckUtilization(ElementAxialForce[i]) > 1)
                {
                    StructureColors.Add(overUtilizedMemberColor);
                    color.Add(overUtilizedMemberColor);
                }             
                else
                {
                    StructureColors.Add(verifiedMemberColor);
                    color.Add(verifiedMemberColor);
                }
                    

                Point3d startOfElement = ElementsInStructure[i].getStartPoint();
                int startNodeIndex = ElementsInStructure[i].getStartNodeIndex();
                if (startNodeIndex != -1)
                    startOfElement += normalizedNodeDisplacement[startNodeIndex];
                    
                Point3d endOfElement = ElementsInStructure[i].getEndPoint();
                int endNodeIndex = ElementsInStructure[i].getEndNodeIndex();
                if (endNodeIndex != -1)
                    endOfElement += normalizedNodeDisplacement[endNodeIndex];

                Cylinder cylinder = new Cylinder(new Circle(new Plane(startOfElement, new Vector3d(endOfElement - startOfElement)), Math.Sqrt(ElementsInStructure[i].CrossSectionArea / Math.PI)), startOfElement.DistanceTo(endOfElement));
                geometry.Add(cylinder.ToBrep(true, true));
                
            }

            
            foreach (Point3d supportNode in SupportNodes)
            {               
                Sphere nodeSphere = new Sphere(supportNode, nodeRadius);
                geometry.Add(nodeSphere.ToBrep());
                color.Add(Structure.supportNodeColor);

                Plane conePlane = new Plane(supportNode + new Point3d(0, 0, -nodeRadius), new Vector3d(0, 0, -1));
                Cone pinnedCone = new Cone(conePlane, 2 * nodeRadius, 2 * nodeRadius);
                geometry.Add(pinnedCone.ToBrep(true));
                color.Add(Structure.supportNodeColor);
            }

            for (int i = 0; i < FreeNodesInitial.Count; i++)
            {
                Sphere nodeSphere = new Sphere(FreeNodesInitial[i]+normalizedNodeDisplacement[i], nodeRadius);
                geometry.Add(nodeSphere.ToBrep());
                color.Add(Structure.freeNodeColor);
            }

            StructureColors.AddRange(color);
            StructureVisuals.AddRange(geometry);

        }

        // Insert Material Bank Methods
        public override List<List<StockElement>> PossibleStockElementForEachInPlaceElement(MaterialBank materialBank)
        {
            List<List<StockElement>> reusablesSuggestionTree = new List<List<StockElement>>();
            int elementCounter = 0;
            foreach (InPlaceBarElement3D elementInStructure in ElementsInStructure)
            {
                List<StockElement> StockElementSuggestionList = new List<StockElement>();
                for (int i = 0; i < materialBank.StockElementsInMaterialBank.Count; i++)
                {
                    StockElement stockElement = materialBank.StockElementsInMaterialBank[i];

                    double lengthOfElement = elementInStructure.StartPoint.DistanceTo(elementInStructure.EndPoint);
                    if (stockElement.CheckUtilization(ElementAxialForce[elementCounter]) < 1
                        && stockElement.GetStockElementLength() > lengthOfElement)
                        StockElementSuggestionList.Add(stockElement);
                }
                reusablesSuggestionTree.Add(StockElementSuggestionList);
                elementCounter++;
            }
            return reusablesSuggestionTree;
        }
        public override bool InsertStockElement(int inPlaceElementIndex, ref MaterialBank materialBank, StockElement stockElement, bool keepCutOff = true)
        {
            if (inPlaceElementIndex < 0 || inPlaceElementIndex > ElementsInStructure.Count)
            {
                throw new Exception("The In-Place-Element index " + inPlaceElementIndex.ToString() + " is not valid!");
            }
            else if (materialBank.RemoveStockElementFromMaterialBank(stockElement, ElementsInStructure[inPlaceElementIndex], keepCutOff))
            {
                InPlaceElement temp = new InPlaceBarElement3D(stockElement, ElementsInStructure[inPlaceElementIndex]);
                ElementsInStructure.RemoveAt(inPlaceElementIndex);
                ElementsInStructure.Insert(inPlaceElementIndex, temp);
                return true;
            }
            return false;
        }
        public override void InsertNewElement(int inPlaceElementIndex, List<string> areaSortedNewElements, double minimumArea)
        {
            if (inPlaceElementIndex < 0 || inPlaceElementIndex > ElementsInStructure.Count)
            {
                throw new Exception("The In-Place-Element index " + inPlaceElementIndex.ToString() + " is not valid!");
            }
            else
            {
                string newProfile = areaSortedNewElements.First(o => AbstractLineElement.CrossSectionAreaDictionary[o] > minimumArea);
                StockElement newElement = new StockElement(newProfile, 0);
                InPlaceElement temp = new InPlaceBarElement3D(newElement, ElementsInStructure[inPlaceElementIndex]);
                temp.IsFromMaterialBank = false;
                ElementsInStructure.RemoveAt(inPlaceElementIndex);
                ElementsInStructure.Insert(inPlaceElementIndex, temp);
            }
        }

    }







    public class TrussModel2D : TrussModel3D
    {   
        // Constructors
        public TrussModel2D(List<Line> lines, List<string> profileNames, List<Point3d> supportPoints)
            : base(lines, profileNames, supportPoints)
        {

        }

        // Overriden Methods
        public override string PrintStructureInfo()
        {
            string info = "2D Truss Structure:\n";
            foreach (InPlaceBarElement2D inPlaceBarElement2D in ElementsInStructure)
            {
                info += "\n" + inPlaceBarElement2D.getElementInfo();
            }
            return info;
        }
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
            FreeNodesInitial = FreeNodes.Select(node => new Point3d(node)).ToList();
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
        protected override void RecalculateGlobalMatrix()
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

        // Insert Material Bank Methods
        public override List<List<StockElement>> PossibleStockElementForEachInPlaceElement(MaterialBank materialBank)
        {
            List<List<StockElement>> reusablesSuggestionTree = new List<List<StockElement>>();
            int elementCounter = 0;
            foreach (InPlaceBarElement2D elementInStructure in ElementsInStructure)
            {
                List<StockElement> StockElementSuggestionList = new List<StockElement>();
                for (int i = 0; i < materialBank.StockElementsInMaterialBank.Count; i++)
                {
                    StockElement stockElement = materialBank.StockElementsInMaterialBank[i];

                    double lengthOfElement = elementInStructure.StartPoint.DistanceTo(elementInStructure.EndPoint);
                    if (stockElement.CheckUtilization(ElementAxialForce[elementCounter]) < 1
                        && stockElement.GetStockElementLength() > lengthOfElement)
                        StockElementSuggestionList.Add(stockElement);
                }
                reusablesSuggestionTree.Add(StockElementSuggestionList);
                elementCounter++;
            }
            return reusablesSuggestionTree;
        }
        public override bool InsertStockElement(int inPlaceElementIndex, ref MaterialBank materialBank, StockElement stockElement, bool keepCutOff = true)
        {
            if (inPlaceElementIndex < 0 || inPlaceElementIndex > ElementsInStructure.Count)
            {
                throw new Exception("The In-Place-Element index " + inPlaceElementIndex.ToString() + " is not valid!");
            }
            else if (materialBank.RemoveStockElementFromMaterialBank(stockElement, ElementsInStructure[inPlaceElementIndex], keepCutOff))
            {
                InPlaceElement temp = new InPlaceBarElement2D(stockElement, ElementsInStructure[inPlaceElementIndex]);
                ElementsInStructure.RemoveAt(inPlaceElementIndex);
                ElementsInStructure.Insert(inPlaceElementIndex, temp);
                return true;
            }
            return false;
        }      
        public override void InsertNewElement(int inPlaceElementIndex, List<string> areaSortedNewElements, double minimumArea)
        {
            if (inPlaceElementIndex < 0 || inPlaceElementIndex > ElementsInStructure.Count)
            {
                throw new Exception("The In-Place-Element index " + inPlaceElementIndex.ToString() + " is not valid!");
            }
            else
            {
                string newProfile = areaSortedNewElements.First(o => AbstractLineElement.CrossSectionAreaDictionary[o] > minimumArea);
                StockElement newElement = new StockElement(newProfile, 0);
                InPlaceElement temp = new InPlaceBarElement2D(newElement, ElementsInStructure[inPlaceElementIndex]);
                temp.IsFromMaterialBank = false;
                ElementsInStructure.RemoveAt(inPlaceElementIndex);
                ElementsInStructure.Insert(inPlaceElementIndex, temp);
            }
        }

        // Unused
        private void CheckInputs(ref List<Line> lines, ref List<double> A, ref List<Point3d> anchoredPoints, ref List<double> loadList, 
            ref List<Vector3d> loadVecs, ref double E)
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
    }







    public class MaterialBank : ElementCollection
    {
        // Variables
        public List<StockElement> StockElementsInMaterialBank;
        public List<System.Drawing.Color> MaterialBankColors;
        public List<Brep> MaterialBankVisuals;

        // Constructors
        public MaterialBank() 
            : base()
        {
            StockElementsInMaterialBank = new List<StockElement>();
            MaterialBankColors = new List<System.Drawing.Color>();
            MaterialBankVisuals = new List<Brep>();
        }
        public MaterialBank(List<StockElement> stockElements)
            :this()
        {
            StockElementsInMaterialBank.AddRange(stockElements);
        }
        public MaterialBank(List<string> profiles, List<int> quantities, List<double> lengths)
            :this()
        {
            if (quantities.Count != lengths.Count || quantities.Count != profiles.Count)
                throw new Exception("Profiles, Quantities and Lengths lists needs to be the same length!");

            for (int i = 0; i < quantities.Count; i++)
            {
                for (int j = 0; j < quantities[i]; j++)
                    this.InsertStockElementIntoMaterialBank(new StockElement(profiles[i], lengths[i]));
            }
        }
        public MaterialBank(List<string> commands)
            :this()
        {
            foreach (string command in commands)
            {
                string[] commandArray = command.Split('x');
                int commandQuantities = Convert.ToInt32(commandArray[0]);
                string commandProfiles = commandArray[1];
                double commandLengths = Convert.ToDouble(commandArray[2]);

                for (int i = 0; i < commandQuantities; i++)
                    this.InsertStockElementIntoMaterialBank(new StockElement(commandProfiles, commandLengths));
            }

        }

        // Operator Overloads
        public static MaterialBank operator +(MaterialBank materialBankA, MaterialBank materialBankB)
        {
            MaterialBank returnMateralBank = new MaterialBank();
            returnMateralBank.StockElementsInMaterialBank.AddRange(materialBankA.StockElementsInMaterialBank);
            returnMateralBank.StockElementsInMaterialBank.AddRange(materialBankB.StockElementsInMaterialBank);
            returnMateralBank.MaterialBankColors.AddRange(materialBankA.MaterialBankColors);
            returnMateralBank.MaterialBankColors.AddRange(materialBankB.MaterialBankColors);
            returnMateralBank.MaterialBankVisuals.AddRange(materialBankA.MaterialBankVisuals);
            returnMateralBank.MaterialBankVisuals.AddRange(materialBankB.MaterialBankVisuals);

            return returnMateralBank;
        }
        
        // Deep Copy
        public MaterialBank DeepCopy()
        {
            MaterialBank returnMateralBank = new MaterialBank();
            returnMateralBank.StockElementsInMaterialBank.AddRange(StockElementsInMaterialBank);
            returnMateralBank.MaterialBankColors.AddRange(MaterialBankColors);
            returnMateralBank.MaterialBankVisuals.AddRange(MaterialBankVisuals);
            return returnMateralBank;
        }

        // Output Methods
        public string GetMaterialBankInfo()
        {
            string info = "Material Bank:\n";
            foreach ( StockElement stockElement in StockElementsInMaterialBank )
                info += "\n" + stockElement.getElementInfo();

            return info;
        }     
        public List<Brep> UpdateVisuals(int groupingMethod =  0)
        {
            if (groupingMethod < 0 || groupingMethod > 1)
                throw new Exception("Grouping Methods are: \n0 - By Length \n1 - By Area");
            else if (StockElementsInMaterialBank.Count == 0)
                return new List<Brep>();

            List<Brep> visuals = new List<Brep>();
            List<System.Drawing.Color> colors = new List<System.Drawing.Color>();
            
            int group = 0;
            double groupSpacing = 0;
            double unusedInstance = 0;
            double usedInstance = 0;
            double instanceSpacing = 100;
            double startSpacing = 100;

            if (groupingMethod == 0)
            {
                sortMaterialBankByLength();
            }
            else if (groupingMethod == 1)
            {
                sortMaterialBankByArea();
            }


            StockElement priorElement = StockElementsInMaterialBank[0];
            foreach (StockElement element in StockElementsInMaterialBank)
            {
                if (groupingMethod == 0)
                {
                    if (priorElement.GetStockElementLength() != element.GetStockElementLength())
                    {
                        group++;
                        groupSpacing += (Math.Sqrt(element.CrossSectionArea) + Math.Sqrt(element.CrossSectionArea)) / Math.PI + 2*instanceSpacing;
                        unusedInstance = 0;
                        usedInstance = 0;
                    }    
                }
                else if (groupingMethod == 1)
                {
                    if (priorElement.CrossSectionArea != element.CrossSectionArea)
                    {
                        group++;
                        groupSpacing += (Math.Sqrt(element.CrossSectionArea) + Math.Sqrt(element.CrossSectionArea)) / Math.PI + 2*instanceSpacing;
                        unusedInstance = 0;
                        usedInstance = 0;
                    }
                        
                }


                Plane basePlane = new Plane();
                colors.Add(ElementCollection.materialBankColors[group % ElementCollection.materialBankColors.Count]);

                if ( element.IsInStructure)
                {
                    basePlane = new Plane(new Point3d(usedInstance + startSpacing, groupSpacing, 0), new Vector3d(0, 0, 1));
                    usedInstance = usedInstance + 2 * Math.Sqrt(element.CrossSectionArea) / Math.PI + instanceSpacing;
                    colors[colors.Count - 1] = System.Drawing.Color.FromArgb(50, colors[colors.Count - 1]);
                }
                else
                {
                    basePlane = new Plane(new Point3d(-unusedInstance - startSpacing, groupSpacing, 0), new Vector3d(0, 0, 1));
                    unusedInstance = unusedInstance + 2 * Math.Sqrt(element.CrossSectionArea) / Math.PI + instanceSpacing;                   
                }

                Circle baseCircle = new Circle(basePlane, Math.Sqrt(element.CrossSectionArea) / Math.PI);
                Cylinder cylinder = new Cylinder(baseCircle, element.GetStockElementLength());
                visuals.Add(cylinder.ToBrep(true, true));
                

                priorElement = element;
                
            }

            MaterialBankVisuals = visuals;
            MaterialBankColors = colors;

            return visuals;
        }

        // Sorting Methods
        public void sortMaterialBankByLength()
        {
            StockElementsInMaterialBank = getLengthSortedMaterialBank();
        }
        public void sortMaterialBankByArea()
        {
            StockElementsInMaterialBank = getAreaSortedMaterialBank();
        }

        public List<StockElement> getLengthSortedMaterialBank()
        {
            return StockElementsInMaterialBank.OrderBy(o => o.GetStockElementLength()).ToList(); ;
        }
        public List<StockElement> getAreaSortedMaterialBank()
        {
            return StockElementsInMaterialBank.OrderBy(o => o.CrossSectionArea).ToList();
        }
        public List<StockElement> getUtilizationThenLengthSortedMaterialBank(double axialForce)
        {
            if (axialForce == 0)
                return StockElementsInMaterialBank.OrderBy(o => -o.CrossSectionArea).
                    ThenBy(o => -o.GetStockElementLength()).ToList();
            else
                return StockElementsInMaterialBank.OrderBy(o => Math.Abs( o.CheckUtilization(axialForce) )).
                    ThenBy(o => -o.GetStockElementLength()).ToList();
        }

        public List<StockElement> getLengthSortedMaterialBank(List<StockElement> stockElements)
        {
            return stockElements.OrderBy(o => o.GetStockElementLength()).ToList(); ;
        }
        public List<StockElement> getAreaSortedMaterialBank(List<StockElement> stockElements)
        {
            return stockElements.OrderBy(o => o.CrossSectionArea).ToList();
        }
        public List<StockElement> getUtilizationSortedMaterialBank(List<StockElement> stockElements, double axialForce)
        {
            if (axialForce == 0)
                return stockElements.OrderBy(o => -o.CrossSectionArea).ToList();
            else
                return stockElements.OrderBy(o => Math.Abs(o.CheckUtilization(axialForce))).ToList();
        }

        // Replace Methods
        public void ResetMaterialBank()
        {
            foreach (StockElement stockElement in StockElementsInMaterialBank)
                stockElement.IsInStructure = false;
        }
        private void InsertStockElementIntoMaterialBank(StockElement stockElement)
        {
            StockElementsInMaterialBank.Add(stockElement);
        }
        public bool RemoveStockElementFromMaterialBank(StockElement stockElement, InPlaceElement inPlaceElement, bool keepCutOff)
        {
            int index = StockElementsInMaterialBank.FindIndex(o => stockElement == o && !o.IsInStructure);
            if ( index == -1 )
            {
                return false;
            }  
            else if (keepCutOff)
            {
                StockElement temp = stockElement.DeepCopy();
                StockElement cutOffPart = new StockElement(temp.ProfileName,
                    temp.GetStockElementLength() - inPlaceElement.StartPoint.DistanceTo(inPlaceElement.EndPoint));
                cutOffPart.IsInStructure = false;
                StockElement insertPart = new StockElement(temp.ProfileName,
                    inPlaceElement.StartPoint.DistanceTo(inPlaceElement.EndPoint));
                insertPart.IsInStructure = true;

                StockElementsInMaterialBank[index] = insertPart;
                StockElementsInMaterialBank.Add(cutOffPart);
            }
            else
            {
                StockElementsInMaterialBank[index].IsInStructure = true;             
            }
            return true;
        }
        public bool ReduceStockElementInMaterialBank(StockElement stockElement, InPlaceElement inPlaceElement)
        {
            int index = StockElementsInMaterialBank.FindIndex(o => stockElement == o && !o.IsInStructure);
            if (index == -1)
                return false;
            else
            {
                StockElement temp = stockElement.DeepCopy();

                StockElement cutOffPart = new StockElement(temp.ProfileName, 
                    temp.GetStockElementLength() - inPlaceElement.StartPoint.DistanceTo(inPlaceElement.EndPoint));
                cutOffPart.IsInStructure = false;

                StockElement insertPart = new StockElement(temp.ProfileName,
                    inPlaceElement.StartPoint.DistanceTo(inPlaceElement.EndPoint));
                insertPart.IsInStructure = true;

                StockElementsInMaterialBank[index] = insertPart;
                StockElementsInMaterialBank.Add(cutOffPart);

                return true;
            }
        }

        // LCA
        public double GetMaterialBankMass()
        {
            double mass = 0;
            foreach ( StockElement stockElement in StockElementsInMaterialBank)
            {
                mass += stockElement.getMass();
            }
            return mass;
        }

    }


}
