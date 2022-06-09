using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;
using System.Drawing;
using Gurobi;

namespace MasterthesisGHA
{
    public abstract class ElementCollection
    {
        // Attributes
        protected static Color supportNodeColor;
        protected static Color freeNodeColor;
        protected static Color pointLoadArrow;
        protected static Color momentArrow;
        protected static Color unverifiedMemberColor;
        protected static Color verifiedMemberColor;
        protected static Color overUtilizedMemberColor;      
        protected static Color bucklingMemberColor;

        public static Color reuseMemberColor;
        public static Color newMemberColor;
        public static Color totalUtilizationColor;
        public static Color axialStressColor;
        public static Color axialBucklingColor;
        public static Color shearStressColor;
        public static Color bendingMomentStressColor;
        public static Color torsionalMomentStressColor;

        public static List<Color> materialBankColors;
        protected static Color loadingPanelColor;
        protected static Color markedGeometry;
        protected static Color unmarkedGeometry;

        // Static Constructor
        static ElementCollection()
        {
            Color darkerGrey = System.Drawing.Color.FromArgb(100, 100, 100);

            supportNodeColor = darkerGrey;
            freeNodeColor = System.Drawing.Color.DarkGray;
            pointLoadArrow = darkerGrey;
            momentArrow = System.Drawing.Color.DarkGray;

            unverifiedMemberColor = System.Drawing.Color.White;
            verifiedMemberColor = System.Drawing.Color.Green;
            overUtilizedMemberColor = System.Drawing.Color.Red;
            bucklingMemberColor = System.Drawing.Color.Yellow;
            
            reuseMemberColor = System.Drawing.Color.LightPink;
            newMemberColor = System.Drawing.Color.LightGreen;
            totalUtilizationColor = Color.Blue;
            axialStressColor = Color.Azure;
            axialBucklingColor = Color.Orange;
            shearStressColor = Color.Crimson;
            bendingMomentStressColor = Color.Pink;
            torsionalMomentStressColor = Color.Indigo;

            materialBankColors = new List<Color> 
            {                  
                System.Drawing.Color.DeepSkyBlue, 
                System.Drawing.Color.LightYellow,
                System.Drawing.Color.Salmon,
                System.Drawing.Color.MediumPurple,
                System.Drawing.Color.LightGreen
            };
            
            loadingPanelColor = System.Drawing.Color.Aquamarine;

            markedGeometry = System.Drawing.Color.Blue;
            unmarkedGeometry = System.Drawing.Color.White;
        }

        // Static Methods
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
        public static Matrix MathnetToRhinoMatrix(Matrix<double> mathnetMatrix)
        {
            Matrix outputMatrix = new Matrix(mathnetMatrix.RowCount, mathnetMatrix.ColumnCount);
            for(int row = 0; row < mathnetMatrix.RowCount; row++)
            {
                for(int col = 0; col < mathnetMatrix.ColumnCount; col++)
                {
                    outputMatrix[row, col] = mathnetMatrix[row, col];
                }
            }
            return outputMatrix;
        }
        public static Matrix ListToRhinoMatrix(List<double> list)
        {
            Matrix outputMatrix = new Matrix(list.Count, 1);
            for (int row = 0; row < list.Count; row++)
            {
                outputMatrix[row,0] = list[row];
            }
            return outputMatrix;
        }
        public static Matrix<double> RhinoToMathnetMatrix(Matrix rhinoMatrix)
        {
            Matrix<double> outputMatrix = Matrix<double>.Build.Dense(rhinoMatrix.RowCount, rhinoMatrix.ColumnCount);
            for (int row = 0; row < rhinoMatrix.RowCount; row++)
            {
                for (int col = 0; col < rhinoMatrix.ColumnCount; col++)
                {
                    outputMatrix[row, col] = rhinoMatrix[row, col];
                }
            }
            return outputMatrix;
        }
        public static Vector3d MathnetToRhinoVector(Vector<double> mathnetVector)
        {
            return new Vector3d(mathnetVector[0], mathnetVector[1], mathnetVector[2]);
        }
        public static Vector<double> RhinoToMathnetVector(Vector3d rhinoVector)
        {
            Vector<double> outputVector = Vector<double>.Build.Dense(3);
            outputVector[0] = rhinoVector.X;
            outputVector[1] = rhinoVector.Y;
            outputVector[2] = rhinoVector.Z;

            return outputVector;
        }
        public static Vector3d Cross(Vector3d left, Vector3d right)
        {
            Vector3d result = new Vector3d();
            result[0] = left[1] * right[2] - left[2] * right[1];
            result[1] = -left[0] * right[2] + left[2] * right[0];
            result[2] = left[0] * right[1] - left[1] * right[0];

            return result;
        }

        // Visuals
        public virtual void GetVisuals(out List<Brep> geometry, out List<Color> color, out string codeInfo, int colorTheme, double size, double maxDisplacement, double maxAngle, double maxLoad, double maxMoment, double crossSectionSize = 1.0)
        {
            geometry = new List<Brep>();
            color = new List<Color>();
            codeInfo = "";
        }
        public virtual double GetSize()
        {
            throw new NotImplementedException();
        }
        public virtual double GetMaxLoad()
        {
            throw new NotImplementedException();
        }
        public virtual double GetMaxMoment()
        {
            throw new NotImplementedException();
        }
        public virtual double GetMaxDisplacement()
        {
            throw new NotImplementedException();
        }
        public virtual double GetMaxAngle()
        {
            throw new NotImplementedException();
        }
    }


    public abstract class Structure : ElementCollection
    {
        // Attributes
        public List<MemberElement> ElementsInStructure;        
        public List<Point3d> FreeNodes;
        public List<Point3d> FreeNodesInitial;
        public List<Point3d> SupportNodes;
        public Matrix<double> GlobalStiffnessMatrix;
        public Vector<double> GlobalLoadVector;
        public Vector<double> GlobalDisplacementVector;
        public List<double> ElementAxialForcesX;
        public List<double> ElementShearForcesY;
        public List<double> ElementShearForcesZ;
        public List<double> ElementTorsionsX;
        public List<double> ElementMomentsY;
        public List<double> ElementMomentsZ;
        public List<double> ElementUtilizations;
        public List<Color> StructureColors;
        public List<Brep> StructureVisuals;

        // Constructors
        public Structure()
        {
            ElementsInStructure = new List<MemberElement>();
            FreeNodes = new List<Point3d>();
            FreeNodesInitial = new List<Point3d>();
            SupportNodes = new List<Point3d>();
            GlobalStiffnessMatrix = Matrix<double>.Build.Sparse(0,0);
            GlobalLoadVector = Vector<double>.Build.Sparse(0);
            GlobalDisplacementVector = Vector<double>.Build.Sparse(0);
            ElementAxialForcesX = new List<double>();
            ElementShearForcesY = new List<double>();
            ElementShearForcesZ = new List<double>();
            ElementTorsionsX = new List<double>();
            ElementMomentsY = new List<double>();
            ElementMomentsZ = new List<double>();                      
            ElementUtilizations = new List<double>();
            StructureColors = new List<Color>();
            StructureVisuals = new List<Brep>();
        }
        public Structure(List<Line> lines, List<string> profileNames, List<Point3d> supportPoints)
        {           
            StructureVisuals = new List<Brep>();
            StructureColors = new List<Color>();
            FreeNodesInitial = new List<Point3d>();

            VerifyModel(ref lines, ref supportPoints);           

            ElementsInStructure = new List<MemberElement>();
            FreeNodes = new List<Point3d>();
            SupportNodes = supportPoints;

            ConstructElementsFromLines(lines, profileNames);

            int dofs = GetDofsPerNode() * FreeNodes.Count;
            GlobalStiffnessMatrix = Matrix<double>.Build.Dense(dofs, dofs);
            GlobalLoadVector = Vector<double>.Build.Dense(dofs);
            GlobalDisplacementVector = Vector<double>.Build.Dense(dofs);
            ElementAxialForcesX = new List<double>();
            ElementShearForcesY = new List<double>();
            ElementShearForcesZ = new List<double>();
            ElementTorsionsX = new List<double>();
            ElementMomentsY = new List<double>();
            ElementMomentsZ = new List<double>();
            ElementUtilizations = new List<double>();

            // Stiffness Matrix
            RecalculateGlobalMatrix();

        }
        public Structure(Structure copyFromThis)
            : this()
        {
            ElementsInStructure = new List<MemberElement>(copyFromThis.ElementsInStructure);
            FreeNodes = new List<Point3d>(copyFromThis.FreeNodes);
            FreeNodesInitial = new List<Point3d>(copyFromThis.FreeNodesInitial);
            SupportNodes = new List<Point3d>(copyFromThis.SupportNodes);
            GlobalStiffnessMatrix = Matrix<double>.Build.SameAs(copyFromThis.GlobalStiffnessMatrix);
            GlobalLoadVector = Vector<double>.Build.SameAs(copyFromThis.GlobalLoadVector);
            GlobalDisplacementVector = Vector<double>.Build.SameAs(copyFromThis.GlobalDisplacementVector);
            ElementAxialForcesX = new List<double>(copyFromThis.ElementAxialForcesX);
            ElementUtilizations = new List<double>(copyFromThis.ElementUtilizations);
            StructureColors = new List<Color>(copyFromThis.StructureColors);
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
            foreach (MemberElement element in ElementsInStructure)
                if (!element.IsFromMaterialBank)
                    mass += element.getMass();
            return mass;
        }
        public double GetReusedMass()
        {
            double mass = 0;
            foreach (MemberElement element in ElementsInStructure)
                if (element.IsFromMaterialBank)
                    mass += element.getMass();
            return mass;
        }
        public double GetTotalMass()
        {
            double mass = 0;
            foreach (MemberElement element in ElementsInStructure)
                mass += element.getMass();
            return mass;
        }
        public double GetStiffnessMartrixRank()
        {
            return GlobalStiffnessMatrix.Rank();
        }
        public double GetStiffnessMartrixNullity()
        {
            return GlobalStiffnessMatrix.RowCount - GetStiffnessMartrixRank();
        }
        public double GetStiffnessMatrixDeterminant()
        {
            return GlobalStiffnessMatrix.Determinant();
        }
        public double GetGravitationalAcceleration()
        {
            return 9.81;
        }

        // Structure Methods
        public virtual string PrintStructureInfo()
        {
            string info = "";
            if (GetDofsPerNode() == 3) info += "Spatial Truss Structure:\n";
            else if (GetDofsPerNode() == 2) info += "Planar Truss Structure:\n";
            else if (GetDofsPerNode() == 6) info += "Spatial Frame Structure: \n";
            info +=  "\\" + "\\" + "\n";
            int pos = 0;
            foreach (MemberElement element in ElementsInStructure)
            {
                info += "\nPosition:" + pos++.ToString() + ", Reuse/New:";
                if (element.IsFromMaterialBank) info += "Reuse, ";
                else info += "New, ";
                info += element.getElementInfo();
            }                
            return info;
        }
        public string PrintProfilesInStructure()
        {
            List<Tuple<string,double>> profilesLengths = new List<Tuple<string, double>>();
            List<int> amount = new List<int>();
            foreach (MemberElement element in ElementsInStructure)
            {
                Tuple<string, double> profilesLength = new Tuple<string, double>(element.ProfileName, element.getInPlaceElementLength());
                if (!profilesLengths.Contains(profilesLength))
                {
                    profilesLengths.Add(profilesLength);
                    amount.Add(1);
                }
                else amount[profilesLengths.IndexOf(profilesLength)]++;
            }
            //profilesLengths.OrderBy(o => LineElement.CrossSectionAreaDictionary[o.Item1]).ThenBy(o => o.Item2);

            string output = "";
            for(int i = 0; i < profilesLengths.Count; i++)
            {
                output += amount[i] + " x " + profilesLengths[i].Item1 + " x " + profilesLengths[i].Item2 + "\n";
            }

            return output;
        }
        public List<string> PrintProfilesInStructureList()
        {
            List<Tuple<string, double>> profilesLengths = new List<Tuple<string, double>>();
            List<int> amount = new List<int>();
            foreach (MemberElement element in ElementsInStructure)
            {
                Tuple<string, double> profilesLength = new Tuple<string, double>(element.ProfileName, element.getInPlaceElementLength());
                if (!profilesLengths.Contains(profilesLength))
                {
                    profilesLengths.Add(profilesLength);
                    amount.Add(1);
                }
                else amount[profilesLengths.IndexOf(profilesLength)]++;
            }
            List<Tuple<string, double>> sortedProfilesLengths = new List<Tuple<string, double>>();
            List<int> sortedAmount = new List<int>();
            while (profilesLengths.Count != 0)
            {
                int tempIndex = 0;
                double tempArea = LineElement.CrossSectionAreaDictionary[profilesLengths[0].Item1];
                double tempLength = profilesLengths[0].Item2;
                for (int i = 0; i < profilesLengths.Count; i++)
                {
                    if (LineElement.CrossSectionAreaDictionary[profilesLengths[i].Item1] < tempArea ||
                        (LineElement.CrossSectionAreaDictionary[profilesLengths[i].Item1] == tempArea && profilesLengths[i].Item2 < tempLength))
                    {
                        tempArea = LineElement.CrossSectionAreaDictionary[profilesLengths[i].Item1];
                        tempLength = profilesLengths[i].Item2;
                        tempIndex = i;
                    }
                }
                sortedProfilesLengths.Add(profilesLengths[tempIndex]);
                sortedAmount.Add(amount[tempIndex]);
                profilesLengths.RemoveAt(tempIndex);
                amount.RemoveAt(tempIndex);
            }

            List<string> output = new List<string>();
            for (int i = 0; i < sortedProfilesLengths.Count; i++)
            {
                output.Add(sortedAmount[i] + "x" + sortedProfilesLengths[i].Item1 + "x" + sortedProfilesLengths[i].Item2);
            }
            return output;
        }
        public Grasshopper.DataTree<string> PrintRangeOfMaterialBanksThree()
        {
            List<List<string>> outputs = new List<List<string>>();
            List<Tuple<string, double>> profilesLengths = new List<Tuple<string, double>>();
            List<int> amount = new List<int>();
            foreach (MemberElement element in ElementsInStructure)
            {
                Tuple<string, double> profilesLength = new Tuple<string, double>(element.ProfileName, element.getInPlaceElementLength());
                if (!profilesLengths.Contains(profilesLength))
                {
                    profilesLengths.Add(profilesLength);
                    amount.Add(1);
                }
                else amount[profilesLengths.IndexOf(profilesLength)]++;
            }
            List<Tuple<string, double>> sortedProfilesLengths = new List<Tuple<string, double>>();
            List<int> sortedAmount = new List<int>();
            while (profilesLengths.Count != 0)
            {
                int tempIndex = 0;
                double tempArea = LineElement.CrossSectionAreaDictionary[profilesLengths[0].Item1];
                double tempLength = profilesLengths[0].Item2;
                for (int i = 0; i < profilesLengths.Count; i++)
                {
                    if (LineElement.CrossSectionAreaDictionary[profilesLengths[i].Item1] < tempArea ||
                        (LineElement.CrossSectionAreaDictionary[profilesLengths[i].Item1] == tempArea && profilesLengths[i].Item2 < tempLength))
                    {
                        tempArea = LineElement.CrossSectionAreaDictionary[profilesLengths[i].Item1];
                        tempLength = profilesLengths[i].Item2;
                        tempIndex = i;
                    }
                }
                sortedProfilesLengths.Add(profilesLengths[tempIndex]);
                sortedAmount.Add(amount[tempIndex]);
                profilesLengths.RemoveAt(tempIndex);
                amount.RemoveAt(tempIndex);
            }

            List<string> output = new List<string>();
            List<string> outputHalf = new List<string>();
            List<string> outputDouble = new List<string>();
            for (int i = 0; i < sortedProfilesLengths.Count; i++)
            {
                output.Add(sortedAmount[i] + "x" + sortedProfilesLengths[i].Item1 + "x" + sortedProfilesLengths[i].Item2);
                outputHalf.Add(sortedAmount[i]/2 + "x" + sortedProfilesLengths[i].Item1 + "x" + sortedProfilesLengths[i].Item2);
                outputDouble.Add(sortedAmount[i]*2 + "x" + sortedProfilesLengths[i].Item1 + "x" + sortedProfilesLengths[i].Item2);
            }

            outputs.Add(output.ToList());
            outputs.Add(outputHalf.ToList());
            outputs.Add(outputDouble.ToList());

            return GetOutputDataTree<string>(outputs);
        }
        protected void ConstructElementsFromLines(List<Line> lines, List<string> profileNames)
        {
            int i_profile = 0;
            for (int i = 0; i < lines.Count; i++)
            {
                i_profile = Math.Min(i, profileNames.Count - 1);
                Point3d startPoint = lines[i].PointAt(0);
                Point3d endPoint = lines[i].PointAt(1);
                if (GetDofsPerNode() == 3)
                    ElementsInStructure.Add(new SpatialBar(ref FreeNodes, ref SupportNodes, profileNames[i_profile], startPoint, endPoint));
                else if (GetDofsPerNode() == 2)
                    ElementsInStructure.Add(new PlanarBar(ref FreeNodes, ref SupportNodes, profileNames[i_profile], startPoint, endPoint));
                else if (GetDofsPerNode() == 6)
                    ElementsInStructure.Add(new SpatialBeam(ref FreeNodes, ref SupportNodes, profileNames[i_profile], startPoint, endPoint));

            }
            FreeNodesInitial = FreeNodes.Select(node => new Point3d(node)).ToList();
        }
        protected virtual void VerifyModel(ref List<Line> lines, ref List<Point3d> anchoredPoints)
        {
            if (lines.Count == 0)
                throw new Exception("Line Input is not valid!");
            
            int minSupports = 0;

            if (GetDofsPerNode() == 2) minSupports = 2;
            else if (GetDofsPerNode() == 3) minSupports = 3;
            else if (GetDofsPerNode() == 6) minSupports = 1;

            if (anchoredPoints.Count < minSupports)
                throw new Exception("Anchored points needs to be at least 2 to prevent rigid body motion!");

        }

        // Specific Structure Type Methods
        protected virtual int GetDofsPerNode()
        {
            return 0;
        }


        // -- STRUCTURAL ANALYSIS --
        protected virtual void RecalculateGlobalMatrix()
        {
            throw new NotImplementedException();
        }
        public virtual void Solve()
        {
            throw new NotImplementedException();
        }
        public virtual void Retracking()
        {
            throw new NotImplementedException();
        }

        // Load Application
        public enum loadMethod { lumped, consistent };
        public virtual void ApplyLineLoad(double loadValue, Vector3d loadDirection, Vector3d distributionDirection, List<Line> loadElements, loadMethod method = loadMethod.consistent)
        {
            throw new NotImplementedException();
        }
        public virtual void ApplySelfWeight(loadMethod method = loadMethod.consistent)
        {
            int dofsPerNode = GetDofsPerNode();
            double gravitationalAcceleration = GetGravitationalAcceleration();

            foreach (MemberElement member in ElementsInStructure)
            {
                if (member.StartNodeIndex != -1)
                    GlobalLoadVector[dofsPerNode * member.StartNodeIndex + (dofsPerNode-1)] += -member.getMass() * gravitationalAcceleration / 2;
                if (member.EndNodeIndex != -1)
                    GlobalLoadVector[dofsPerNode * member.EndNodeIndex + (dofsPerNode - 1)] += -member.getMass() * gravitationalAcceleration / 2;
            }
        }
        
        // Results
        public Matrix GetTotalUtilization()
        {
            Matrix output = new Matrix(ElementsInStructure.Count, 1);
            for (int i = 0; i < ElementsInStructure.Count; i++)
            {
                if (GetDofsPerNode() <= 3) // Truss
                    output[i, 0] = ElementsInStructure[i].getTotalUtilization(
                        ElementAxialForcesX[i], 0, 0, 0, 0, 0);
                else // Frame
                    output[i, 0] = ElementsInStructure[i].getTotalUtilization(
                        ElementAxialForcesX[i], 
                        ElementMomentsY[i], 
                        ElementMomentsZ[i], 
                        ElementShearForcesY[i], 
                        ElementShearForcesZ[i], 
                        ElementTorsionsX[i]);
            }

            return output;
        }
        public Matrix GetAxialForceUtilization()
        {
            Matrix output = new Matrix(ElementsInStructure.Count, 1);
            for (int i = 0; i < ElementsInStructure.Count; i++)
                output[i, 0] = ElementsInStructure[i].getAxialForceUtilization(ElementAxialForcesX[i]);

            return output;
        }
        public Matrix GetAxialBucklingUtilization()
        {
            Matrix output = new Matrix(ElementsInStructure.Count, 1);
            for (int i = 0; i < ElementsInStructure.Count; i++)
                output[i, 0] = ElementsInStructure[i].getAxialBucklingUtilization(ElementAxialForcesX[i]);

            return output;
        }
        public Matrix GetShearStressUtilizationY()
        {
            Matrix output = new Matrix(ElementsInStructure.Count, 1);
            for (int i = 0; i < ElementsInStructure.Count; i++)
            {
                if (GetDofsPerNode() <= 3) // Truss
                    output[i, 0] = 0;
                else // Frame
                    output[i, 0] = ElementsInStructure[i].getShearForceUtilizationY(
                        ElementShearForcesY[i]);
            }

            return output;
        }
        public Matrix GetShearStressUtilizationZ()
        {
            Matrix output = new Matrix(ElementsInStructure.Count, 1);
            for (int i = 0; i < ElementsInStructure.Count; i++)
            {
                if (GetDofsPerNode() <= 3) // Truss
                    output[i, 0] = 0;
                else // Frame
                    output[i, 0] = ElementsInStructure[i].getShearForceUtilizationZ(
                        ElementShearForcesZ[i]);
            }

            return output;
        }
        public Matrix GetBendingMomentUtilizationY()
        {
            Matrix output = new Matrix(ElementsInStructure.Count, 1);
            for (int i = 0; i < ElementsInStructure.Count; i++)
            {
                if (GetDofsPerNode() <= 3) // Truss
                    output[i, 0] = 0;
                else // Frame
                    output[i, 0] = ElementsInStructure[i].getBendingMomentUtilizationY(
                        ElementMomentsY[i]);
            }

            return output;
        }
        public Matrix GetBendingMomentUtilizationZ()
        {
            Matrix output = new Matrix(ElementsInStructure.Count, 1);
            for (int i = 0; i < ElementsInStructure.Count; i++)
            {
                if (GetDofsPerNode() <= 3) // Truss
                    output[i, 0] = 0;
                else // Frame
                    output[i, 0] = ElementsInStructure[i].getBendingMomentUtilizationZ(
                        ElementMomentsZ[i]);
            }

            return output;
        }
        public Matrix GetTorsionalMomentUtilizationX()
        {
            Matrix output = new Matrix(ElementsInStructure.Count, 1);
            for (int i = 0; i < ElementsInStructure.Count; i++)
            {
                if (GetDofsPerNode() <= 3) // Truss
                    output[i, 0] = 0;
                else // Frame
                    output[i, 0] = ElementsInStructure[i].getTorsionalMomentUtilization(
                        ElementTorsionsX[i]);
            }

            return output;
        }


        // -- VISUALS --
        public override void GetVisuals(out List<Brep> geometry, out List<Color> color, out string codeInfo, int colorCode, double size, double maxDisplacement, double maxAngle, double maxLoad, double maxMoment, double crossSectionSize = 1.0)
        {
            geometry = new List<Brep>();
            color = new List<Color>();

            List<Brep> outGeometry = new List<Brep>();
            List<Color> outColor = new List<Color>();

            colorCode = colorCode % 11;
            List<string> codeInfos = new List<string>()
            {
                "0 - Discrete Member Verifiation (Red/Green/Yellow)",
                "1 - New and Reuse Members (White-Blue)",
                "2 - Continuous Member Stresses (White-Blue)",
                "3 - Continuous Member Buckling (White-Blue)",
                "4 - Nx (White-Blue)",
                "5 - Vy (White-Blue)",
                "6 - Vz (White-Blue)",
                "7 - Tx (White-Blue)",
                "8 - My (White-Blue)",
                "9 - Mz (White-Blue)",
                "10 - Continuous Node Displacements (White-Blue)"
            };
            codeInfo = codeInfos[colorCode];

            GetLoadVisuals(out outGeometry, out outColor, size, maxLoad, maxMoment, maxDisplacement, maxAngle, true);
            geometry.AddRange(outGeometry);
            color.AddRange(outColor);

            GetResultVisuals(out outGeometry, out outColor, colorCode, size, maxDisplacement, maxAngle, crossSectionSize);
            geometry.AddRange(outGeometry);
            color.AddRange(outColor);
        }
        public virtual void GetLoadVisuals(out List<Brep> geometry, out List<Color> color, double size = -1, double maxLoad = -1, double maxMoment = -1, double maxDisplacement = -1, double maxAngle = -1, bool inwardFacingArrows = true)
        {
            throw new NotImplementedException();
        }
        public virtual void GetResultVisuals(out List<Brep> geometry, out List<Color> color, int colorTheme, double size = -1, double maxDisplacement = -1, double maxAngle = -1, double crossSectionSize = 1.0)
        {
            throw new NotImplementedException();
        }
        public override double GetSize()
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
                (zMax - zMin) * (zMax - zMin));
        }
        public override double GetMaxLoad()
        {
            int dofsPerNode = GetDofsPerNode();
            double max = 0;
            double temp;

            int dimensions = 3;
            if (dofsPerNode <= 3) dimensions = dofsPerNode;

            for (int i = 0; i < dofsPerNode * FreeNodes.Count; i += dofsPerNode)
            {
                temp = 0;
                for (int j = 0; j < dimensions; j++)
                {
                    temp += GlobalLoadVector[i + j] * GlobalLoadVector[i + j];
                }
                max = Math.Max(max, temp);
            }

            return Math.Sqrt(max);
        }
        public override  double GetMaxMoment()
        {
            int dofsPerNode = GetDofsPerNode();
            double max = 0;
            double temp;

            int dimensions = 3;
            if (dofsPerNode <= 3) return 0.0;
            else if (dofsPerNode == 4) dimensions = 2;

            for (int i = 0; i < dofsPerNode * FreeNodes.Count; i += dofsPerNode)
            {
                temp = 0;
                for (int j = 0; j < dimensions; j++)
                {
                    temp += GlobalLoadVector[i + dimensions + j] * GlobalLoadVector[i + dimensions + j];
                }
                max = Math.Max(max, temp);
            }

            return Math.Sqrt(max);
        }
        public override double GetMaxDisplacement()
        {
            int dofsPerNode = GetDofsPerNode();
            double max = 0;
            double temp;

            int dimensions = 3;
            if (dofsPerNode <= 3) dimensions = dofsPerNode;

            for (int i = 0; i < dofsPerNode * FreeNodes.Count; i += dofsPerNode)
            {
                temp = 0;
                for (int j = 0; j < dimensions; j++) 
                    temp += GlobalDisplacementVector[i + j] * GlobalDisplacementVector[i + j];

                if (temp != double.NaN) max = Math.Max(max, temp);
            }
            return Math.Sqrt(max);
        }
        public override double GetMaxAngle()
        {
            int dofsPerNode = GetDofsPerNode();
            double max = 0;
            double temp;

            int dimensions = 3;
            if (dofsPerNode <= 3) return 0.0;
            else if (dofsPerNode == 4) dimensions = 2;

            for (int i = 0; i < dofsPerNode * FreeNodes.Count; i += dofsPerNode)
            {
                temp = 0;
                for (int j = 0; j < dimensions; j++)
                {
                    temp += GlobalDisplacementVector[i + dimensions + j] * GlobalDisplacementVector[i + dimensions + j];
                }
                max = Math.Max(max, temp);
            }

            return Math.Sqrt(max);
        }

        protected Color BlendColors(Color color, Color backColor, double amount)
        {
            if (amount < 0) amount = 0;
            else if (amount > 1) amount = 1;
            byte r = (byte)(color.R * amount + backColor.R * (1 - amount));
            byte g = (byte)(color.G * amount + backColor.G * (1 - amount));
            byte b = (byte)(color.B * amount + backColor.B * (1 - amount));
            return System.Drawing.Color.FromArgb(r, g, b);
        }
        protected double getStructureSizeFactor(double factorOfLength, double structureSize)
        {
            return factorOfLength * structureSize;
        }
        protected double getDisplacementFactor(double factorOfLength, double structureSize, double maxDisplacement)
        {
            if(maxDisplacement < 1) maxDisplacement = 1;
            return factorOfLength * structureSize / maxDisplacement;
        }
        protected double getAngleFactor(double maxDisplayAngle, double maxAngle)
        {
            if (maxAngle < Math.PI * 0.001) maxAngle = Math.PI * 0.001;
            return maxDisplayAngle / maxAngle;
        }
        protected double getLoadFactor(double factorOfLength, double structureSize, double maxLoad)
        {
            if(maxLoad < 1) maxLoad = 1;
            return factorOfLength * structureSize / maxLoad;
        }


        // -- MEMBER REPLACEMENT METHODS --

        // Direct
        public virtual List<List<ReuseElement>> GetReusabilityTree(MaterialBank materialBank)
        {
            throw new NotImplementedException();
        }
        protected virtual bool InsertReuseElementAndCutMaterialBank(int inPlaceElementIndex, ref MaterialBank materialBank, ReuseElement stockElement, bool keepCutOff = true)
        {
            throw new NotImplementedException();
        }
        protected virtual void InsertReuseElementAndCutMaterialBank(int positionIndex, int elementIndex, ref MaterialBank materialBank, bool keepCutOff = true)
        {
            throw new NotImplementedException();
        }
        protected virtual void InsertNewElement(int inPlaceElementIndex, List<string> sortedNewElements, double minimumArea)
        {
            throw new NotImplementedException();
        }

        // Matrix
        public virtual bool UpdateInsertionMatrix(ref Matrix<double> insertionMatrix, int stockElementIndex, int inPlaceElementIndex, MaterialBank materialBank, bool keepCutOff = true)
        {
            throw new NotImplementedException();
        }
        public virtual void InsertReuseElementsFromInsertionMatrix(Matrix<double> insertionMatrix, MaterialBank materialBank, out MaterialBank outMaterialBank)
        {
            if (insertionMatrix.RowCount != ElementsInStructure.Count) throw new Exception("# of rows in Insertion Matrix is not equal to positions in Structure");
            if (insertionMatrix.ColumnCount != materialBank.ElementsInMaterialBank.Count) throw new Exception("# of columns in Insertion Matrix is not equal to elements in Material Bank");

            outMaterialBank = materialBank.GetDeepCopy();

            bool keepCutOff = true;
            for (int row = 0; row < insertionMatrix.RowCount; row++)
            {
                for (int col = 0; col < insertionMatrix.ColumnCount; col++)
                {
                    if(insertionMatrix[row,col] != 0) InsertReuseElementAndCutMaterialBank(row, col, ref outMaterialBank, keepCutOff);
                }
            }
        }
        public Matrix<double> getStructuralIntegrityMatrix(MaterialBank materialBank)
        {
            Matrix<double> structuralIntegrityMatrix = Matrix<double>.Build.Dense(ElementsInStructure.Count,
                materialBank.ElementsInMaterialBank.Count);

            for (int i = 0; i < ElementsInStructure.Count; i++)
            {
                for (int j = 0; j < materialBank.ElementsInMaterialBank.Count; j++)
                {
                    MemberElement member = ElementsInStructure[i];
                    ReuseElement reuseElement = materialBank.ElementsInMaterialBank[j];

                    double axialForce = ElementAxialForcesX[i];
                    double shearY = 0;
                    double shearZ = 0;
                    double momentX = 0;
                    double momentY = 0;
                    double momentZ = 0;
                    if (GetDofsPerNode() > 3)
                    {
                        shearY = ElementShearForcesY[i];
                        shearZ = ElementShearForcesZ[i];
                        momentX = ElementTorsionsX[i];
                        momentY = ElementMomentsY[i];
                        momentZ = ElementMomentsZ[i];
                    }
                    

                    structuralIntegrityMatrix[i, j] =
                        ObjectiveFunctions.StructuralIntegrity(member, reuseElement, axialForce, momentY, momentZ, shearY, shearZ, momentX);
                }
            }
            return structuralIntegrityMatrix;
        }
        public Matrix<double> getSufficientLengthMatrix(MaterialBank materialBank)
        {
            Matrix<double> sufficientLengthMatrix = Matrix<double>.Build.Dense(ElementsInStructure.Count,
                materialBank.ElementsInMaterialBank.Count);

            for (int i = 0; i < ElementsInStructure.Count; i++)
            {
                for (int j = 0; j < materialBank.ElementsInMaterialBank.Count; j++)
                {
                    MemberElement member = ElementsInStructure[i];
                    ReuseElement reuseElement = materialBank.ElementsInMaterialBank[j];

                    if (member.getInPlaceElementLength() > reuseElement.getReusableLength()) sufficientLengthMatrix[i, j] = 0;
                    else sufficientLengthMatrix[i, j] = 1;

                }
            }
            return sufficientLengthMatrix;
        }
        public Matrix<double> getReusabilityMatrix(MaterialBank materialBank)
        {
            return getStructuralIntegrityMatrix(materialBank).PointwiseMultiply(getSufficientLengthMatrix(materialBank));
        }
        public Matrix<double> getObjectiveMatrix(MaterialBank materialBank, bool normalized = false)
        {
            Matrix<double> localLCA = Matrix<double>.Build.Dense(ElementsInStructure.Count,
                materialBank.ElementsInMaterialBank.Count);

            for (int i = 0; i < ElementsInStructure.Count; i++)
                for (int j = 0; j < materialBank.ElementsInMaterialBank.Count; j++)
                    localLCA[i, j] = ObjectiveFunctions.LocalCarbonReduction(ElementsInStructure[i], materialBank.ElementsInMaterialBank[j]);

            if (normalized) localLCA /= localLCA.Enumerate().Max();
            return localLCA;
        }
        public Matrix<double> getPriorityMatrix(MaterialBank materialBank)
        {
            return getReusabilityMatrix(materialBank).PointwiseMultiply(getObjectiveMatrix(materialBank));
        }
        public Matrix<double> getRelativeReusedLengthMatrix(MaterialBank materialBank)
        {
            Matrix<double> relativeLengthMatrix = Matrix<double>.Build.Sparse(ElementsInStructure.Count, materialBank.ElementsInMaterialBank.Count);

            for (int memberIndex = 0; memberIndex < ElementsInStructure.Count; memberIndex++)
            {
                MemberElement member = ElementsInStructure[memberIndex];
                double axialForce = ElementAxialForcesX[memberIndex];
                double shearY = 0;
                double shearZ = 0;
                double momentX = 0;
                double momentY = 0;
                double momentZ = 0;
                if (GetDofsPerNode() > 3)
                {
                    shearY = ElementShearForcesY[memberIndex];
                    shearZ = ElementShearForcesZ[memberIndex];
                    momentX = ElementTorsionsX[memberIndex];
                    momentY = ElementMomentsY[memberIndex];
                    momentZ = ElementMomentsZ[memberIndex];
                }

                for (int reuseIndex = 0; reuseIndex < materialBank.ElementsInMaterialBank.Count; reuseIndex++)
                {
                    ReuseElement reuseElement = materialBank.ElementsInMaterialBank[reuseIndex];
                    double memberLength = member.StartPoint.DistanceTo(member.EndPoint);

                    if (reuseElement.getTotalUtilization(axialForce, momentY, momentZ, shearY, shearZ, momentX) < 1 &&
                        reuseElement.getAxialBucklingUtilization(ElementAxialForcesX[memberIndex], memberLength) < 1 &&
                        reuseElement.getReusableLength() > memberLength)
                        relativeLengthMatrix[memberIndex, reuseIndex] = memberLength / reuseElement.getReusableLength();
                }
            }

            return relativeLengthMatrix;
        }
        public Matrix<double> getLocalLCAMatrixOLD(MaterialBank materialBank, bool normalized = true)
        {
            Matrix<double> localLCA = Matrix<double>.Build.Dense(ElementsInStructure.Count,
                materialBank.ElementsInMaterialBank.Count);

            for (int i = 0; i < ElementsInStructure.Count; i++)
            {
                for (int j = 0; j < materialBank.ElementsInMaterialBank.Count; j++)
                {
                    localLCA[i, j] =
                        ObjectiveFunctions.LocalLCAWithIntegrityAndLengthCheck(ElementsInStructure[i], materialBank.ElementsInMaterialBank[j], ElementAxialForcesX[i]);
                }
            }
            if (normalized) localLCA = localLCA / localLCA.Enumerate().Max();
            return localLCA;
        }


        // -- REUSE METHODS --

        // Heuristics
        public void InsertWithMaxReuseRate(MaterialBank materialBank, out Matrix<double> insertionMatrix, bool cutting)
        {
            Matrix<double> verificationMatrix = getStructuralIntegrityMatrix(materialBank);
            IEnumerable<int> insertionOrder = OptimumInsertOrderFromReuseRate(verificationMatrix);
            InsertMaterialBank(materialBank, insertionOrder, out insertionMatrix, cutting);
        }
        public void InsertMaterialBankByPriorityMatrix(MaterialBank materialBank, out MaterialBank remainingMaterialBank, out IEnumerable<int> optimumOrder)
        {
            Matrix<double> priorityMatrix = getObjectiveMatrix(materialBank);
            optimumOrder = OptimumInsertOrderFromLocalLCA(priorityMatrix).ToList();
            InsertMaterialBank(materialBank, optimumOrder, out remainingMaterialBank);
            remainingMaterialBank.UpdateVisualsMaterialBank();
        }
        public void InsertMaterialBankByPriorityMatrix(out Matrix<double> insertionMatrix, MaterialBank materialBank, out IEnumerable<int> optimumOrder, bool cutting)
        {
            Matrix<double> priorityMatrix = getObjectiveMatrix(materialBank);
            optimumOrder = OptimumInsertOrderFromLocalLCA(priorityMatrix).ToList();
            InsertMaterialBank(materialBank, optimumOrder, out insertionMatrix, cutting);
        }


        // Brute Force
        public void InsertMaterialBankByAllPermutations(MaterialBank materialBank, out MaterialBank remainingMaterialBank, out List<double> objectiveFunctionOutputs, out IEnumerable<IEnumerable<int>> allOrderedLists)
        {
            List<int> initalList = new List<int>();
            for (int i = 0; i < ElementsInStructure.Count; i++)
            {
                initalList.Add(i);
            }
            allOrderedLists = GetPermutations(initalList, initalList.Count);
            int factorial = Enumerable.Range(1, initalList.Count).Aggregate(1, (p, item) => p * item);
            if (factorial > 1e3 || factorial == 0)
                throw new Exception("Structure is top big to perform brute force calculation");

            InsertNewElements();
            SpatialTruss structureCopy = new SpatialTruss();

            objectiveFunctionOutputs = new List<double>();
            double objectiveFunction = 0;
            double optimum = 0;

            IEnumerable<int> optimumOrder = Enumerable.Empty<int>();

            bool firstRun = true;
            foreach (IEnumerable<int> list in allOrderedLists)
            {
                MaterialBank tempInputMaterialBank = materialBank.GetDeepCopy();
                MaterialBank tempOutputMaterialBank = materialBank.GetDeepCopy();

                structureCopy = new SpatialTruss(this);
                structureCopy.InsertMaterialBank(tempInputMaterialBank, list, out tempOutputMaterialBank);
                objectiveFunction = 1.00 * structureCopy.GetReusedMass() + 1.50 * structureCopy.GetNewMass();

                if ((objectiveFunction < optimum) || firstRun)
                {
                    optimum = objectiveFunction;
                    firstRun = false;
                    optimumOrder = list.ToList();
                }

                objectiveFunctionOutputs.Add(objectiveFunction);
            }

            InsertMaterialBank(materialBank, optimumOrder, out remainingMaterialBank);
            remainingMaterialBank.UpdateVisualsMaterialBank();
        }
        public void InsertMaterialBankByRandomPermutations(int maxIterations, MaterialBank materialBank, out MaterialBank remainingMaterialBank, out List<double> objectiveFunctionOutputs, out List<List<int>> shuffledLists, bool cutting)
        {
            double objectiveTreshold = 100000;
            double objectiveFunction = 0;
            objectiveFunctionOutputs = new List<double>();
            
            List<int> initalList = Enumerable.Range(0, ElementsInStructure.Count).ToList();
            InsertNewElements();

            SpatialTruss structureCopy;
            Random random = new Random();
            List<int> shuffledList;
            shuffledLists = new List<List<int>>();
            List<int> optimumOrder = new List<int>();

            double optimum = 0;
            int iterationsCounter = 0;
            bool firstRun = true;

            while (objectiveFunction < objectiveTreshold && iterationsCounter < maxIterations)
            {
                shuffledList = Shuffle(initalList, random).ToList();
                shuffledLists.Add(shuffledList.ToList());

                MaterialBank tempInputMaterialBank = materialBank.GetDeepCopy();

                structureCopy = new SpatialTruss(this);
                structureCopy.InsertMaterialBank(tempInputMaterialBank, shuffledList, out MaterialBank tempRemainingMaterialBank);

                objectiveFunction = ObjectiveFunctions.SimpleTestFunction(structureCopy, tempRemainingMaterialBank);

                objectiveFunctionOutputs.Add(objectiveFunction);

                if ((objectiveFunction < optimum) || firstRun)
                {
                    optimum = objectiveFunction;
                    firstRun = false;
                    optimumOrder = shuffledList.ToList();
                }

                iterationsCounter++;
            }

            InsertMaterialBank(materialBank, optimumOrder, out remainingMaterialBank);
            remainingMaterialBank.UpdateVisualsMaterialBank();
        }
        public void InsertMaterialBankByRandomPermutations(out Matrix<double> insertionMatrix, int iterations, MaterialBank materialBank, out string bestResultLog, out string fullSearchLog, bool cutting)
        {
            bool randomOrder = true;
            double bestResult = 0;
            fullSearchLog = "";
            bestResultLog = "";
            insertionMatrix = Matrix<double>.Build.Sparse(ElementsInStructure.Count, materialBank.ElementsInMaterialBank.Count);
            Matrix<double> tempInsertionMatrix = Matrix<double>.Build.Sparse(ElementsInStructure.Count, materialBank.ElementsInMaterialBank.Count);

            for (int i = 0; i < iterations; i++)
            {
                InsertMaterialBank(materialBank, out tempInsertionMatrix, randomOrder, cutting);
                double objectiveFunctionOutput = ObjectiveFunctions.GlobalLCA(this, materialBank, tempInsertionMatrix, ObjectiveFunctions.lcaMethod.simplified);

                string search = 
                    i.ToString() + "," +
                    ObjectiveFunctions.GlobalLCA(this, materialBank, tempInsertionMatrix, ObjectiveFunctions.lcaMethod.simplified).ToString() + "," +
                    ObjectiveFunctions.GlobalFCA(this, materialBank, tempInsertionMatrix, ObjectiveFunctions.bound.upperBound).ToString() + '\n';

                fullSearchLog += search;

                if (objectiveFunctionOutput < bestResult || bestResult == 0)
                {
                    bestResult = objectiveFunctionOutput;
                    tempInsertionMatrix.CopyTo(insertionMatrix);
                    bestResultLog += search;
                }
            }
        }

        // Branch & Bound
        public void InsertMaterialBankByBNB(MaterialBank inputMaterialBank, out MaterialBank outputMaterialBank)
        {
            Matrix<double> priorityMatrix = getPriorityMatrix(inputMaterialBank);
            Node node = new Node();
            Matrix<double> costMatrix = node.getCostMatrix(priorityMatrix);
            Node solutionNode = node.Solve(costMatrix, out _, this, inputMaterialBank);

            List<Tuple<int, int>> solutionPath = solutionNode.path;
            outputMaterialBank = inputMaterialBank.GetDeepCopy();
            foreach (Tuple<int, int> insertion in solutionPath)
            {
                InsertReuseElementAndCutMaterialBank(insertion.Item1, insertion.Item2, ref outputMaterialBank);
            }
        }
        public void InsertMaterialBankByBNB(MaterialBank inputMaterialBank, out Matrix<double> insertionMatrix, int maxIterations, out string resultLog)
        {
            Matrix<double> priorityMatrix = getPriorityMatrix(inputMaterialBank);
            Node node = new Node();
            node.materialBank = inputMaterialBank;
            node.structure = this;
            Matrix<double> costMatrix = node.getCostMatrix(priorityMatrix);
            Node solutionNode = node.Solve(costMatrix, out resultLog, this, inputMaterialBank, maxIterations);

            List<Tuple<int, int>> solutionPath = solutionNode.path;
            insertionMatrix = Matrix<double>.Build.Sparse(ElementsInStructure.Count, inputMaterialBank.ElementsInMaterialBank.Count);
            foreach (Tuple<int, int> insertion in solutionPath)
                insertionMatrix[insertion.Item1, insertion.Item2] = ElementsInStructure[insertion.Item1].getInPlaceElementLength();
        }



        // Protected Methods
        public void InsertNewElements()
        {
            List<string> areaSortedElements = LineElement.GetCrossSectionAreaSortedProfilesList();

            for (int i = 0; i < ElementsInStructure.Count; i++)
                InsertNewElement(i, areaSortedElements.ToList(), Math.Abs(ElementAxialForcesX[i] / 355));
        }
        public void InsertMaterialBank(MaterialBank materialBank, IEnumerable<int> insertOrder, out Matrix<double> insertionMatrix, bool cutting)
        {
            insertionMatrix = Matrix<double>.Build.Sparse(ElementsInStructure.Count, materialBank.ElementsInMaterialBank.Count);
            List<int> insertionList = insertOrder.ToList();

            if (insertionList.Count > ElementsInStructure.Count)
                throw new Exception("InsertOrder contains " + insertionList.Count + " elements, while the structure contains "
                    + ElementsInStructure.Count + " members");

            for (int i = 0; i < insertionList.Count; i++)
            {
                int memberIndex = insertionList[i];

                double axialForce = ElementAxialForcesX[memberIndex];
                double shearY = 0;
                double shearZ = 0;
                double momentX = 0;
                double momentY = 0;
                double momentZ = 0;
                if (GetDofsPerNode() > 3)
                {
                    shearY = ElementShearForcesY[memberIndex];
                    shearZ = ElementShearForcesZ[memberIndex];
                    momentX = ElementTorsionsX[memberIndex];
                    momentY = ElementMomentsY[memberIndex];
                    momentZ = ElementMomentsZ[memberIndex];
                }

                List<int> sortedIndexing = materialBank.getUtilizationThenLengthSortedMaterialBankIndexing(
                    axialForce, momentY, momentZ, shearY, shearZ, momentX);

                int indexingFromLast = sortedIndexing.Count;
                while (indexingFromLast-- != 0)
                {
                    int stockElementIndex = sortedIndexing[indexingFromLast];
                    int numberOfCuts = insertionMatrix.Column(stockElementIndex).Aggregate(0, (total, next) => next != 0 ? total + 1 : total);
                    if (numberOfCuts != 0) numberOfCuts--;

                    double usedLength = insertionMatrix.Column(stockElementIndex).Sum() + MaterialBank.cuttingLength * numberOfCuts;

                    if (!cutting && usedLength > 0) continue;

                    if (usedLength + ElementsInStructure[memberIndex].getInPlaceElementLength() > materialBank.ElementsInMaterialBank[stockElementIndex].getReusableLength()
                        || materialBank.ElementsInMaterialBank[stockElementIndex].getTotalUtilization(axialForce, momentY, momentZ, shearY, shearZ, momentX) > 1.0)
                        continue;

                    if (stockElementIndex != -1)
                    {
                        UpdateInsertionMatrix(ref insertionMatrix, stockElementIndex, memberIndex, materialBank, true);
                        break;
                    }
                }
            }
        }
        public void InsertMaterialBank(MaterialBank materialBank, out Matrix<double> insertionMatrix, bool randomOrder = false, bool cutting = true)
        {
            if (randomOrder)
            {
                Random random = new Random();
                List<int> initalList = Enumerable.Range(0, ElementsInStructure.Count).ToList();
                List<int> shuffledList = Shuffle(initalList, random).ToList();
                InsertMaterialBank(materialBank, shuffledList, out insertionMatrix, cutting);
            }
            else InsertMaterialBank(materialBank, Enumerable.Range(0, ElementsInStructure.Count), out insertionMatrix, cutting);
        }
        public void InsertMaterialBank(MaterialBank materialBank, IEnumerable<int> insertOrder, out MaterialBank remainingMaterialBank)
        {
            List<int> list = new List<int>();
            insertOrder.ToList().ForEach(x => list.Add(x));

            if (list.Count > ElementsInStructure.Count)
                throw new Exception("InsertOrder contains " + list.Count + " elements, while the structure contains "
                    + ElementsInStructure.Count + " members");

            List<List<ReuseElement>> possibleStockElements = GetReusabilityTree(materialBank);

            for (int i = 0; i < list.Count; i++)
            {
                int elementIndex = list[i];
                List<ReuseElement> sortedStockElementList = new MaterialBank(possibleStockElements[elementIndex])
                    .getUtilizationThenLengthSortedMaterialBank(ElementAxialForcesX[elementIndex]);
                int index = sortedStockElementList.Count;
                while (index-- != 0)
                {
                    if (InsertReuseElementAndCutMaterialBank(elementIndex, ref materialBank, sortedStockElementList[index], true)) break;
                }
            }

            materialBank.UpdateVisualsMaterialBank();
            remainingMaterialBank = materialBank.GetDeepCopy();
        }
        public void InsertMaterialBank(MaterialBank materialBank, out MaterialBank remainingMaterialBank)
        {
            InsertMaterialBank(materialBank, Enumerable.Range(0, ElementsInStructure.Count), out remainingMaterialBank);
        }
       
        public IEnumerable<int> OptimumInsertOrderFromReuseRate(Matrix<double> reusableMatrix)
        {
            List<int> order = new List<int>(reusableMatrix.RowCount);

            List<Tuple<int, Vector<double>>> indexedColumns = new List<Tuple<int, Vector<double>>>();
            for (int col = 0; col < reusableMatrix.ColumnCount; col++)
                indexedColumns.Add(new Tuple<int, Vector<double>>(col, reusableMatrix.Column(col)));

            indexedColumns.OrderBy(o => o.Item2.Sum());
            indexedColumns.ForEach(o => order.Add(o.Item1));

            return order;
        }
        protected IEnumerable<int> OptimumInsertOrderFromLocalLCA(Matrix<double> rankMatrix, int method = 0)
        {
            List<int> order = new List<int>(rankMatrix.RowCount);
            int rowCount = rankMatrix.RowCount;

            List<Tuple<int, int, double>> indexedRankMatrix = new List<Tuple<int, int, double>>();
            for (int row = 0; row < rankMatrix.RowCount; row++)
                for (int col = 0; col < rankMatrix.ColumnCount; col++)
                    indexedRankMatrix.Add(new Tuple<int, int, double>(row, col, rankMatrix[row, col]));

            switch (method)
            {
                case 0:
                    {
                        List<Tuple<int, int, double>> orderByGlobalMax = indexedRankMatrix.OrderBy(x => -x.Item3).ToList();
                        int tempRow;

                        while (rowCount-- != 0)
                        {
                            if (orderByGlobalMax.First().Item3 == -1)
                            {
                                break;
                            }
                            tempRow = orderByGlobalMax.First().Item1;
                            order.Add(orderByGlobalMax[0].Item1); // Get row index from global maximum
                            orderByGlobalMax.RemoveAll(x => x.Item1 == tempRow); // Remove all values from that row
                        }
                        break;
                    }

                case 1:
                    {
                        double max;
                        List<Tuple<int, int, double>> allMax;

                        while (rowCount-- != 0)
                        {
                            max = indexedRankMatrix.Where(x => x.Item3 >= 0).Max().Item3;
                            if (max == -1)
                            {
                                break;
                            }
                            allMax = indexedRankMatrix.Where(x => x.Item3 == max).ToList();
                            order.Add(allMax[0].Item1);
                            rankMatrix.ClearRow(allMax[0].Item1);
                        }
                        break;
                    }
            }
            return order;
        }
        public IEnumerable<int> OptimumInsertOrderFromReuseRateThenLocalLCA(Matrix<double> reusableMatrix)
        {
            List<int> order = new List<int>(reusableMatrix.RowCount);

            List<Tuple<int, Vector<double>>> indexedColumns = new List<Tuple<int, Vector<double>>>();
            for (int col = 0; col < reusableMatrix.ColumnCount; col++)
                indexedColumns.Add(new Tuple<int, Vector<double>>(col, reusableMatrix.Column(col)));

            indexedColumns.OrderBy(o => o.Item2.Sum()).ThenByDescending(o => o.Item2.Maximum());
            indexedColumns = indexedColumns.FindAll(o => o.Item2.Maximum() > 0); // iterate and remove used member positions
            indexedColumns.ForEach(o => order.Add(o.Item1));

            return order;
        }

        public IEnumerable<IEnumerable<T>> GetPermutations<T>(IEnumerable<T> list, int length)
        {
            if (length == 1)
                return list.Select(t => new T[] { t });

            return GetPermutations(list, length - 1).SelectMany(t => list.Where(e => !t.Contains(e)),
                    (t1, t2) => t1.Concat(new T[] { t2 }));
        }
        public IEnumerable<IEnumerable<int>> GetPermutations(int length)
        {
            IEnumerable<int> list = Enumerable.Range(0, length - 1);
            if (length == 1)
                return list.Select(t => new int[] { t });

            return GetPermutations(list, length - 1).SelectMany(t => list.Where(e => !t.Contains(e)),
                    (t1, t2) => t1.Concat(new int[] { t2 }));
        }
        public IEnumerable<T> Shuffle<T>(IEnumerable<T> source, Random rng)
        {
            T[] elements = source.ToArray();
            for (int i = elements.Length - 1; i >= 0; i--)
            {
                int swapIndex = rng.Next(i + 1);
                yield return elements[swapIndex];
                elements[swapIndex] = elements[i];
            }
        }
    }









    public class SpatialTruss : Structure
    {
        // Constructors
        public SpatialTruss()
            : base()
        {

        }
        public SpatialTruss(List<Line> lines, List<string> profileNames, List<Point3d> supportPoints)
            : base(lines, profileNames, supportPoints)
        {
            

        }
        public SpatialTruss(Structure copyFromThis)
            : this()
        {
            ElementsInStructure = copyFromThis.ElementsInStructure.ToList();
            FreeNodes = copyFromThis.FreeNodes.ToList();
            FreeNodesInitial = copyFromThis.FreeNodesInitial.ToList();
            SupportNodes = copyFromThis.SupportNodes.ToList();
            GlobalStiffnessMatrix = Matrix<double>.Build.SameAs(copyFromThis.GlobalStiffnessMatrix);
            GlobalLoadVector = Vector<double>.Build.SameAs(copyFromThis.GlobalLoadVector);
            GlobalDisplacementVector = Vector<double>.Build.SameAs(copyFromThis.GlobalDisplacementVector);
            ElementAxialForcesX = copyFromThis.ElementAxialForcesX.ToList();
            ElementUtilizations = copyFromThis.ElementUtilizations.ToList();
            StructureColors = copyFromThis.StructureColors.ToList();
            StructureVisuals = copyFromThis.StructureVisuals.ToList();

        }

        // General Methods
        protected override int GetDofsPerNode()
        {
            return 3;
        }       
        
        // Structural Analysis
        protected override void RecalculateGlobalMatrix()
        {
            GlobalStiffnessMatrix.Clear();
            foreach (MemberElement element in ElementsInStructure)
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
        public override void Retracking()
        {
            foreach (SpatialBar element in ElementsInStructure)
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

                ElementAxialForcesX.Add((element.CrossSectionArea * element.YoungsModulus) * (L1 - L0) / L0);
                ElementUtilizations.Add(element.YoungsModulus * (L1 - L0) / L0 / element.YieldStress);
            }
        }

        // Load Application
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
        public override void ApplyLineLoad(double loadValue, Vector3d loadDirection, Vector3d distributionDirection, List<Line> loadElements, loadMethod method = loadMethod.consistent)
        {
            loadDirection.Unitize();
            int dofsPerNode = GetDofsPerNode();
            foreach (SpatialBar element in ElementsInStructure)
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
        public virtual void ApplySnowLoadOnPanels(List<Triangle3d> panels, double loadValue = 3.0)
        {
            Vector3d loadDirection = new Vector3d(0, 0, -1);
            int dofs = GetDofsPerNode();

            foreach (Triangle3d panel in panels)
            {
                List<Point3d> panelPoints = new List<Point3d>() { panel.A, panel.B, panel.C };
                Vector3d outwardNormal = Vector3d.CrossProduct(panel.C - panel.A, panel.B - panel.A);
                outwardNormal.Unitize();
                if (outwardNormal * loadDirection < 0)
                {
                    for (int i = 0; i < FreeNodes.Count; i++)
                    {
                        if (panelPoints.Contains(FreeNodes[i]))
                        {
                            GlobalLoadVector[dofs * i + 2] += 
                                getConsistentLoadFactor(panel, FreeNodes[i]) * loadDirection.Z * loadValue * panel.Area * Math.Abs(outwardNormal * loadDirection);
                        }
                    }
                }
            }
        }
        protected double getLumpedLoadFactor()
        {
            return (double) 1.0/3.0;
        }
        protected double getConsistentLoadFactor(Triangle3d panel, Point3d node)
        {
            List<Point3d> panelPoints = new List<Point3d>() { panel.A, panel.B, panel.C };
            double thisInverseMomentArm = 1 / node.DistanceTo(panel.AreaCenter);
            double sumInverseMomentArms = 0;
            foreach (Point3d panelPoint in panelPoints)
                sumInverseMomentArms += 1 / panelPoint.DistanceTo(panel.AreaCenter);
            
            return thisInverseMomentArm/sumInverseMomentArms;
        }
        public virtual void ApplyWindLoadOnPanels(List<Triangle3d> panels, Vector3d loadDirection, double loadValue = 1.0)
        {
            int dofs = GetDofsPerNode();

            foreach (Triangle3d panel in panels)
            {
                List<Point3d> panelPoints = new List<Point3d>() { panel.A, panel.B, panel.C };
                Vector3d outwardNormal = Vector3d.CrossProduct(panel.C - panel.A, panel.B - panel.A);
                outwardNormal.Unitize();
                if (outwardNormal * loadDirection < 0)
                {
                    for (int i = 0; i < FreeNodes.Count; i++)
                    {
                        if (panelPoints.Contains(FreeNodes[i]))
                        {
                            double loadFactor = getConsistentLoadFactor(panel, FreeNodes[i]);
                            GlobalLoadVector[dofs * i + 0] += loadFactor * loadDirection.X * loadValue * panel.Area * Math.Abs(outwardNormal * loadDirection);
                            GlobalLoadVector[dofs * i + 1] += loadFactor * loadDirection.Y * loadValue * panel.Area * Math.Abs(outwardNormal * loadDirection);
                            GlobalLoadVector[dofs * i + 2] += loadFactor * loadDirection.Z * loadValue * panel.Area * Math.Abs(outwardNormal * loadDirection);
                        }
                    }
                }
            }

        }
        public virtual List<Triangle3d> GiftWrapLoadPanels(Vector3d loadDirection, out List<Brep> visuals, out List<Color> colors, out Circle circle, out List<Line> innerEdges, out List<Line> outerEdges, out List<Line> newEdges, out List<Point3d> closePoints, int returnCount, out List<Brep> liveVisuals, out List<Color> liveColors)
        {
            int debugCounter = 0;

            visuals = new List<Brep>();
            colors = new List<Color>();
            liveVisuals = new List<Brep>();
            liveColors = new List<Color>();
            liveColors.Add(System.Drawing.Color.DarkOrange);

            // Initialize
            double allowedError = 1e1;
            double adjustmentFactorVolumeError = 5;
            double panelLength = ElementsInStructure.Select(o => o.StartPoint.DistanceTo(o.EndPoint)).Max();
            List<Point3d> nodesCopy = FreeNodes.ToList();
            nodesCopy.AddRange(SupportNodes.ToList());
            circle = new Circle();
            closePoints = new List<Point3d>();

            innerEdges = new List<Line>();
            outerEdges = new List<Line>();
            newEdges = new List<Line>();
            List<Line> innerEdgesCopy = new List<Line>();
            List<Line> outerEdgesCopy = new List<Line>();
            List<Line> newEdgesCopy = new List<Line>();
            List<int> duplicateIndices = new List<int>();
            List<Line> duplicateLines = new List<Line>();
            List<int> overlapIndices = new List<int>();
            List<Line> overlapLines = new List<Line>();

            List<Triangle3d> innerPanels = new List<Triangle3d>();
            List<Triangle3d> outerPanels = new List<Triangle3d>();
            List<Triangle3d> newPanels = new List<Triangle3d>();

            int newCounter = 0;
            int newLimit = (int)1e3;
            bool first = true;

            while (true) // Until no new panels
            {
                if (++newCounter > newLimit)
                {
                    throw new Exception("New counter exceeds limit!");
                }

                // FIRST PANEL
                if (first)
                {
                    first = false;

                    List<Point3d> firstPoints = findFirstPanel(loadDirection, panelLength);
                    Triangle3d firstPanel = new Triangle3d(firstPoints[0], firstPoints[1], firstPoints[2]);
                    if (Vector3d.CrossProduct(firstPoints[1] - firstPoints[0], firstPoints[2] - firstPoints[0]) * loadDirection < 0)
                    {
                        firstPanel = new Triangle3d(firstPoints[0], firstPoints[2], firstPoints[1]);
                    }

                    outerPanels.Add(firstPanel);
                    outerEdges = new List<Line>()
                        {
                            firstPanel.AB,
                            firstPanel.BC,
                            firstPanel.CA
                        };

                    loadPanelVisuals(ref visuals, ref colors, ref innerPanels, ref outerPanels, ref newPanels);

                    if (++debugCounter >= returnCount) return innerPanels;

                }


                // CREATE NEW PANELS FROM OUTER PANELS
                foreach (Triangle3d panel in outerPanels)
                {
                    List<Line> panelEdgesInitial = new List<Line>()
                        {
                            panel.AB,
                            panel.BC,
                            panel.CA
                        };
                    List<Line> panelPerpendicularsInitial = new List<Line>
                        {
                            panel.PerpendicularAB,
                            panel.PerpendicularBC,
                            panel.PerpendicularCA
                        };
                    List<Line> panelEdges = new List<Line>();
                    List<Line> panelPerpendiculars = new List<Line>();


                    for (int i = 0; i < panelEdgesInitial.Count; i++)
                    {
                        Line o = panelEdgesInitial[i];

                        if (
                        !innerEdges.Contains(o) &&
                        !innerEdges.Contains(new Line(o.PointAt(1), o.PointAt(0))) &&
                        (
                        outerEdges.Contains(o) ||
                        outerEdges.Contains(new Line(o.PointAt(1), o.PointAt(0)))
                        ) &&
                        !newEdges.Contains(o) &&
                        !newEdges.Contains(new Line(o.PointAt(1), o.PointAt(0))))
                        {
                            panelEdges.Add(o);
                            panelPerpendiculars.Add(panelPerpendicularsInitial[i]);
                        }
                    }

                    Vector3d outwardNormal = Vector3d.CrossProduct(panel.C - panel.A, panel.B - panel.A);
                    outwardNormal.Unitize();

                    for (int panelEdgeIndex = 0; panelEdgeIndex < panelEdges.Count; panelEdgeIndex++)
                    {
                        Line edge = panelEdges[panelEdgeIndex];
                        Line perpendicular = panelPerpendiculars[panelEdgeIndex];
                        Point3d iPoint = perpendicular.PointAt(1);
                        Point3d oPoint = iPoint + 2 * (perpendicular.PointAt(0) - perpendicular.PointAt(1));
                        Point3d outwardsPoint = perpendicular.PointAt(0) + outwardNormal * perpendicular.Length;

                        circle = new Circle(iPoint, outwardsPoint, oPoint)
                        {
                            Radius = 100
                        };
                        double divisions = 1e5;
                        double degree = 0.1 * 2 * Math.PI;
                        double endDegree = 0.9 * 2 * Math.PI;
                        double addedDegree = (endDegree - degree) / divisions;
                        int pivotCounter = 0;
                        bool newPanelFound = false;

                        innerEdgesCopy = innerEdges.ToList();
                        closePoints = nodesCopy
                            .FindAll(
                            o =>
                            o.DistanceTo(edge.PointAt(0)) <= panelLength + allowedError &&
                            o.DistanceTo(edge.PointAt(1)) <= panelLength + allowedError &&
                            o.DistanceTo(edge.PointAt(0)) > 0 &&
                            o.DistanceTo(edge.PointAt(1)) > 0 &&
                            !innerEdgesCopy.Contains(new Line(edge.PointAt(0), o)) && // No inner edges
                            !innerEdgesCopy.Contains(new Line(o, edge.PointAt(0))) &&
                            !innerEdgesCopy.Contains(new Line(edge.PointAt(1), o)) &&
                            !innerEdgesCopy.Contains(new Line(o, edge.PointAt(0))))
                            .ToList(); // Check intersection


                        // PIVOT AROUND EDGE                     
                        while (!newPanelFound)
                        {
                            degree += addedDegree;
                            Point3d pivotPoint = circle.PointAt(degree);

                            if (closePoints.Count == 0) break;
                            else if (pivotCounter++ > divisions)
                            {
                                throw new Exception("Close points found, but no new panels added. " +
                                "Consider increasing allowed error.");
                            }


                            // NEW PANEL
                            foreach (Point3d node in closePoints)
                            {
                                Vector3d pivotPointToStartOfEdge = new Vector3d(edge.PointAt(0) - pivotPoint);
                                Vector3d pivotPointToEndOfEdge = new Vector3d(edge.PointAt(1) - pivotPoint);
                                Vector3d pivotPointToNode = new Vector3d(node - pivotPoint);

                                double tetrahedronVolume =
                                    1 / 6.0 * Vector3d.CrossProduct(pivotPointToStartOfEdge, pivotPointToEndOfEdge) * pivotPointToNode;

                                if (tetrahedronVolume < adjustmentFactorVolumeError * (allowedError * allowedError))
                                {
                                    addNewPanelWithEdges(edge, node, ref newPanels, ref outerEdges, ref newEdges);

                                    if (++debugCounter >= returnCount)
                                    {
                                        loadPanelVisuals(ref visuals, ref colors, ref innerPanels, ref outerPanels, ref newPanels);
                                        return innerPanels;
                                    }
                                    newPanelFound = true;
                                }

                                if (newPanelFound) break;
                            }
                        }
                    }
                }

                updatePanelsAndEdges(ref innerEdges, ref outerEdges, ref newEdges,
                    ref innerEdgesCopy, ref outerEdgesCopy, ref newEdgesCopy,
                    ref innerPanels, ref outerPanels, ref newPanels);

                if (newPanels.Count == 0)
                {
                    loadPanelVisuals(ref visuals, ref colors, ref innerPanels, ref outerPanels, ref newPanels);
                    return innerPanels;
                }

                newPanels.Clear();
                newEdges.Clear();

            }

        }
        protected virtual void loadPanelVisuals(ref List<Brep> visuals, ref List<Color> colors, ref List<Triangle3d> innerPanels, ref List<Triangle3d> outerPanels, ref List<Triangle3d> newPanels)
        {
            double arrowLength = 2e2;
            double coneHeight = 3e1;
            double radius = 1e1;

            Point3d startPoint;
            Point3d endPoint;
            Point3d arrowBase;
            Cylinder loadCylinder;
            Cone arrow;

            visuals.Clear();
            colors.Clear();

            Color colorBlue = Color.FromArgb(100,Color.Blue);
            Color colorGreen = Color.FromArgb(100, Color.Green);
            Color colorYellow = Color.FromArgb(100, Color.Yellow);

            foreach (Triangle3d temp in innerPanels)
            {
                Vector3d tempONormal = Vector3d.CrossProduct(temp.C - temp.A, temp.B - temp.A);
                tempONormal.Unitize();
                startPoint = new Point3d(temp.AreaCenter);
                endPoint = startPoint + new Point3d(tempONormal * arrowLength);
                arrowBase = endPoint + tempONormal * coneHeight;
                loadCylinder = new Cylinder(new Circle(new Plane(startPoint, tempONormal), radius),
                    startPoint.DistanceTo(endPoint));
                arrow = new Cone(new Plane(arrowBase, tempONormal), -coneHeight, 2 * radius);

                visuals.Add(Brep.CreateFromCornerPoints(temp.A, temp.B, temp.C, 0.0));
                visuals.Add(loadCylinder.ToBrep(true, true));
                visuals.Add(arrow.ToBrep(true));
                colors.Add(colorBlue);
                colors.Add(colorBlue);
                colors.Add(colorBlue);
            }

            foreach (Triangle3d temp in outerPanels)
            {
                Vector3d tempONormal = Vector3d.CrossProduct(temp.C - temp.A, temp.B - temp.A);
                tempONormal.Unitize();
                startPoint = new Point3d(temp.AreaCenter);
                endPoint = startPoint + new Point3d(tempONormal * arrowLength);
                arrowBase = endPoint + tempONormal * coneHeight;
                loadCylinder = new Cylinder(new Circle(new Plane(startPoint, tempONormal), radius),
                    startPoint.DistanceTo(endPoint));
                arrow = new Cone(new Plane(arrowBase, tempONormal), -coneHeight, 2 * radius);

                visuals.Add(Brep.CreateFromCornerPoints(temp.A, temp.B, temp.C, 0.0));
                visuals.Add(loadCylinder.ToBrep(true, true));
                visuals.Add(arrow.ToBrep(true));
                colors.Add(colorGreen);
                colors.Add(colorGreen);
                colors.Add(colorGreen);
            }

            foreach (Triangle3d temp in newPanels)
            {
                Vector3d tempONormal = Vector3d.CrossProduct(temp.C - temp.A, temp.B - temp.A);
                tempONormal.Unitize();
                startPoint = new Point3d(temp.AreaCenter);
                endPoint = startPoint + new Point3d(tempONormal * arrowLength);
                arrowBase = endPoint + tempONormal * coneHeight;
                loadCylinder = new Cylinder(new Circle(new Plane(startPoint, tempONormal), radius),
                    startPoint.DistanceTo(endPoint));
                arrow = new Cone(new Plane(arrowBase, tempONormal), -coneHeight, 2 * radius);

                visuals.Add(Brep.CreateFromCornerPoints(temp.A, temp.B, temp.C, 0.0));
                visuals.Add(loadCylinder.ToBrep(true, true));
                visuals.Add(arrow.ToBrep(true));
                colors.Add(colorYellow);
                colors.Add(colorYellow);
                colors.Add(colorYellow);
            }

            // Pivot Visuals
            /*if (pivotCounter % (divisions / 100) == 0)
            {
                visuals.Add(Brep.CreateFromCornerPoints(
                    edge.PointAt(0),
                    new Point3d(pivotPoint),
                    new Point3d(edge.PointAt(1)),
                    0.0));
                colors.Add(System.Drawing.Color.Orange);
            }*/
        }
        protected virtual List<Point3d> findFirstPanel(Vector3d loadDirection, double panelLength)
        {
            Plane zeroPlane = new Plane(new Point3d(0, 0, 0), -loadDirection);
            List<Point3d> nodesCopy = FreeNodes.ToList();
            nodesCopy.AddRange(SupportNodes.ToList());

            // Sort on loadDirection axis
            List<double> distances = new List<double>(nodesCopy.Count);
            foreach (Point3d node in nodesCopy)
            {
                distances.Add(zeroPlane.DistanceTo(node));
            }

            List<Point3d> panelCorners = new List<Point3d>();
            double initialDistance = distances[0];

            for (int n = 0; n < 3; n++)
            {
                panelCorners.Add(new Point3d());
                while (true)
                {
                    double minDistance = distances[0];
                    int minDistanceIndex = 0;
                    for (int i = 1; i < distances.Count; i++)
                    {
                        if (minDistance < distances[i])
                        {
                            minDistance = distances[i];
                            minDistanceIndex = i;
                        }
                    }
                    panelCorners[n] = nodesCopy[minDistanceIndex];
                    nodesCopy.RemoveAt(minDistanceIndex);
                    distances.RemoveAt(minDistanceIndex);

                    if (panelCorners[0].DistanceTo(panelCorners[n]) > panelLength)
                        continue;

                    break;
                }
            }

            return panelCorners;
        }
        protected virtual void addNewPanelWithEdges(Line edge, Point3d node, ref List<Triangle3d> newPanels, ref List<Line> newEdges, ref List<Line> tempEdges)
        {
            Triangle3d newPanel = new Triangle3d(
                                        new Point3d(edge.PointAt(0)),
                                        new Point3d(node),
                                        new Point3d(edge.PointAt(1)));
            newPanels.Add(newPanel);

            // Update edges                              
            newEdges.AddRange(new List<Line>() { newPanel.AB, newPanel.BC, newPanel.CA });
            tempEdges.AddRange(new List<Line>() { newPanel.AB, newPanel.BC });

            List<int> duplicateIndices = new List<int>();
            List<Line> duplicateLines = new List<Line>();
            for (int i = 0; i < newEdges.Count; i++)
            {
                for (int j = 0; j < newEdges.Count; j++)
                {
                    if (i == j || duplicateIndices.Contains(i) || duplicateIndices.Contains(j)) continue;
                    else if (newEdges[i] == newEdges[j] ||
                        newEdges[i] == new Line(newEdges[j].PointAt(1), newEdges[j].PointAt(0)))
                    {
                        duplicateIndices.Add(i);
                        duplicateIndices.Add(j);
                        duplicateLines.Add(new Line(newEdges[j].PointAt(0), newEdges[j].PointAt(1)));
                        duplicateLines.Add(new Line(newEdges[j].PointAt(1), newEdges[j].PointAt(0)));
                    }
                }
            }

            newEdges.RemoveAll(o => duplicateLines.Contains(o));
        }
        protected virtual void updatePanelsAndEdges(ref List<Line> innerEdges, ref List<Line> outerEdges, ref List<Line> newEdges, ref List<Line> innerEdgesCopy, ref List<Line> outerEdgesCopy, ref List<Line> newEdgesCopy, ref List<Triangle3d> innerPanels, ref List<Triangle3d> outerPanels, ref List<Triangle3d> newPanels)
        {
            innerPanels.AddRange(outerPanels.ToList());
            outerPanels = newPanels.ToList();

            outerEdges.Clear();
            foreach (Triangle3d tempPanel in outerPanels)
            {
                List<Line> e = new List<Line>()
                            {
                                tempPanel.AB,
                                tempPanel.BC,
                                tempPanel.CA
                            };

                innerEdgesCopy = innerEdges.ToList();
                outerEdges.AddRange(e.ToList());
            }
            innerEdges.Clear();
            foreach (Triangle3d innerPanel in innerPanels)
            {
                List<Line> e = new List<Line>()
                            {
                                innerPanel.AB,
                                innerPanel.BC,
                                innerPanel.CA
                            };

                innerEdgesCopy = innerEdges.ToList();
                innerEdges.AddRange(e);
            }

            List<int> duplicateIndices = new List<int>();
            List<Line> duplicateLines = new List<Line>();
            List<int> overlapIndices = new List<int>();
            List<Line> overlapLines = new List<Line>();

            for (int i = 0; i < outerEdges.Count; i++)
            {
                // New edge duplicates
                for (int j = 0; j < outerEdges.Count; j++)
                {
                    if (i == j || duplicateIndices.Contains(i) || duplicateIndices.Contains(j)) continue;
                    else if (outerEdges[i] == outerEdges[j] ||
                        outerEdges[i] == new Line(outerEdges[j].PointAt(1), outerEdges[j].PointAt(0)))
                    {
                        duplicateIndices.Add(i);
                        duplicateIndices.Add(j);
                        duplicateLines.Add(new Line(outerEdges[j].PointAt(0), outerEdges[j].PointAt(1)));
                        duplicateLines.Add(new Line(outerEdges[j].PointAt(1), outerEdges[j].PointAt(0)));
                    }
                }

                // New and inner overlap
                for (int j = 0; j < innerEdges.Count; j++)
                {
                    if (i == j || overlapIndices.Contains(i) || overlapIndices.Contains(j)) continue;
                    else if (outerEdges[i] == innerEdges[j] ||
                        outerEdges[i] == new Line(innerEdges[j].PointAt(1), innerEdges[j].PointAt(0)))
                    {
                        overlapIndices.Add(i);
                        overlapIndices.Add(j);
                        overlapLines.Add(new Line(innerEdges[j].PointAt(0), innerEdges[j].PointAt(1)));
                        overlapLines.Add(new Line(innerEdges[j].PointAt(1), innerEdges[j].PointAt(0)));
                    }
                }

            }

            outerEdges.RemoveAll(o => duplicateLines.Contains(o));
            outerEdges.RemoveAll(o => overlapLines.Contains(o));
            innerEdges.AddRange(duplicateLines);

        }

        // Visuals
        public override void GetLoadVisuals(out List<Brep> geometry, out List<Color> color, double size = -1, double maxLoad = -1, double maxMoment = -1, double maxDisplacement = -1, double maxAngle = -1, bool inwardFacingArrows = true)
        {
            geometry = new List<Brep>();
            color = new List<Color>();

            double displacementFactor = getDisplacementFactor(0.02, size, maxDisplacement);
            double loadLineRadius = getStructureSizeFactor(2e-3, size);
            double spacingLoadArrowToNode = 1e-2 * size;

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
                    pointLoadVector[dofsPerNode * i + 2] * pointLoadVector[dofsPerNode * i + 2]) 
                    * getLoadFactor(0.1,size, maxLoad);

                dir.Unitize();                
                double coneHeight = 6 * loadLineRadius;
                double coneRadius = 3 * loadLineRadius;

                Point3d startPoint;
                Point3d endPoint;
                Cylinder loadCylinder;
                Cone arrow;

                if (inwardFacingArrows)
                {
                    endPoint = new Point3d(FreeNodesInitial[i] + freeNodeDisplacement[i] - dir * spacingLoadArrowToNode);
                    startPoint = endPoint + new Point3d(-dir * (arrowLength+spacingLoadArrowToNode));
                    loadCylinder = new Cylinder(new Circle(new Plane(startPoint, dir), loadLineRadius),
                        startPoint.DistanceTo(endPoint)-coneHeight);
                    arrow = new Cone(new Plane(endPoint, new Vector3d(
                        pointLoadVector[dofsPerNode * i],
                        pointLoadVector[dofsPerNode * i + 1],
                        pointLoadVector[dofsPerNode * i + 2])),
                        -coneHeight, coneRadius);
                }
                else
                {
                    startPoint = new Point3d(FreeNodesInitial[i] + freeNodeDisplacement[i]);
                    endPoint = startPoint + new Point3d(dir * arrowLength);
                    loadCylinder = new Cylinder(new Circle(new Plane(startPoint, dir), loadLineRadius),
                        startPoint.DistanceTo(endPoint));
                    arrow = new Cone(new Plane(endPoint + dir * coneHeight, new Vector3d(
                        pointLoadVector[dofsPerNode * i],
                        pointLoadVector[dofsPerNode * i + 1],
                        pointLoadVector[dofsPerNode * i + 2])),
                        -coneHeight, coneRadius);
                }
               

                StructureVisuals.Add(loadCylinder.ToBrep(true, true));
                StructureColors.Add(Structure.pointLoadArrow);
                StructureVisuals.Add(arrow.ToBrep(true));
                StructureColors.Add(Structure.pointLoadArrow);

                geometry.Add(loadCylinder.ToBrep(true, true));
                color.Add(Structure.pointLoadArrow);
                geometry.Add(arrow.ToBrep(true));
                color.Add(Structure.pointLoadArrow);
            }
        }
        public override void GetResultVisuals(out List<Brep> geometry, out List<Color> color, int colorCode = 0, double size = -1, double maxDisplacement = -1, double maxAngle = -1, double crossSectionSize = 1.0)
        {
            geometry = new List<Brep>();
            color = new List<Color>();

            double displacementFactor = getDisplacementFactor(0.02, size, maxDisplacement);
            double initialNodeRadius = getStructureSizeFactor(8e-3, size);
            List<Point3d> normalizedNodeDisplacement = new List<Point3d>();
            for (int j = 0; j < FreeNodes.Count; j++)
                normalizedNodeDisplacement.Add(displacementFactor * (new Point3d(FreeNodes[j] - FreeNodesInitial[j])));
           
            for (int i = 0; i < ElementsInStructure.Count; i++)
            {
                // Color codes
                // 0 - Discrete Member Verifiation (Red/Green/Yellow)
                // 1 - New and Reuse Members (White-Blue)
                // 2 - Continuous Member Utilization (White-Blue)
                // 3 - Continuous Member Buckling (White-Blue)
                // 4 - Nx (White-Blue)
                // 5 - Vy (White-Blue)
                // 6 - Vz (White-Blue)
                // 7 - Tx (White-Blue)
                // 8 - My (White-Blue)
                // 9 - Mz (White-Blue)
                // 10 - Continuous Node Displacements (White-Blue)

                double utilizationBuckling = ElementsInStructure[i].getAxialBucklingUtilization(ElementAxialForcesX[i]);
                double utilizationStress = ElementsInStructure[i].getAxialForceUtilization(ElementAxialForcesX[i]);

                if (colorCode == 0)
                {
                    if (utilizationBuckling > 1) color.Add(bucklingMemberColor);
                    else if (utilizationStress > 1) color.Add(overUtilizedMemberColor);
                    else color.Add(verifiedMemberColor);
                }
                else if (colorCode == 1)
                {
                    if (ElementsInStructure[i].IsFromMaterialBank) color.Add(reuseMemberColor);
                    else color.Add(newMemberColor);
                }
                else if (colorCode == 2)
                {
                    if (utilizationStress <= 1 && utilizationStress >= -1) color.Add(BlendColors(totalUtilizationColor, unmarkedGeometry, Math.Abs(utilizationStress)));
                    else color.Add(totalUtilizationColor);
                }
                else if (colorCode == 3)
                {
                    if (utilizationBuckling <= 1) color.Add(BlendColors(axialBucklingColor, unmarkedGeometry, utilizationBuckling));
                    else color.Add(axialBucklingColor);
                }
                else if (colorCode == 4)
                {
                    if (utilizationStress <= 1 && utilizationStress >= -1) color.Add(BlendColors(axialStressColor, unmarkedGeometry, Math.Abs(utilizationStress)));
                    else color.Add(axialStressColor);
                }
                else color.Add(unmarkedGeometry);

                Point3d startOfElement = ElementsInStructure[i].getStartPoint();
                int startNodeIndex = ElementsInStructure[i].getStartNodeIndex();
                if (startNodeIndex != -1)
                    startOfElement += normalizedNodeDisplacement[startNodeIndex];
                    
                Point3d endOfElement = ElementsInStructure[i].getEndPoint();
                int endNodeIndex = ElementsInStructure[i].getEndNodeIndex();
                if (endNodeIndex != -1)
                    endOfElement += normalizedNodeDisplacement[endNodeIndex];

                Cylinder cylinder = new Cylinder(new Circle(new Plane(startOfElement, new Vector3d(endOfElement - startOfElement)), 
                    crossSectionSize*Math.Sqrt(ElementsInStructure[i].CrossSectionArea / Math.PI)), startOfElement.DistanceTo(endOfElement));
                geometry.Add(cylinder.ToBrep(true, true));   
            }
            
            foreach (Point3d supportNode in SupportNodes)
            {               
                Sphere nodeSphere = new Sphere(supportNode, initialNodeRadius);                
                Plane conePlane = new Plane(supportNode + new Point3d(0, 0, -initialNodeRadius), new Vector3d(0, 0, -1));
                Cone pinnedCone = new Cone(conePlane, 2 * initialNodeRadius, 2 * initialNodeRadius);
                geometry.Add(nodeSphere.ToBrep());
                geometry.Add(pinnedCone.ToBrep(true));
                color.Add(Structure.supportNodeColor);
                color.Add(Structure.supportNodeColor);
            }

            double nodeRadius = initialNodeRadius;
            double liveDisplacement = GetMaxDisplacement();
            for (int i = 0; i < FreeNodesInitial.Count; i++)
            {
                if (colorCode == 9)
                {
                    nodeRadius = 1.5 * initialNodeRadius;
                    double displacement = FreeNodes[i].DistanceTo(FreeNodesInitial[i]);                   
                    color.Add(BlendColors(markedGeometry, unmarkedGeometry, Math.Abs(displacement/liveDisplacement)));
                }
                else
                {
                    color.Add(Structure.freeNodeColor);
                }

                Sphere nodeSphere = new Sphere(FreeNodesInitial[i] + normalizedNodeDisplacement[i], nodeRadius);
                geometry.Add(nodeSphere.ToBrep());
            }
        }

        // -- MEMBER REPLACEMENT METHODS --
        public override List<List<ReuseElement>> GetReusabilityTree(MaterialBank materialBank)
        {
            List<List<ReuseElement>> reuseSuggestionTree = new List<List<ReuseElement>>();
            int memberIndex = 0;
            foreach (SpatialBar member in ElementsInStructure)
            {
                List<ReuseElement> reuseSuggestionList = new List<ReuseElement>();
                for (int i = 0; i < materialBank.ElementsInMaterialBank.Count; i++)
                {
                    ReuseElement reuseElement = materialBank.ElementsInMaterialBank[i];
                    double axialForce = ElementAxialForcesX[memberIndex];
                    double shearY = 0;
                    double shearZ = 0;
                    double momentX = 0;
                    double momentY = 0;
                    double momentZ = 0;
                    if (GetDofsPerNode() > 3)
                    {
                        shearY = ElementShearForcesY[memberIndex];
                        shearZ = ElementShearForcesZ[memberIndex];
                        momentX = ElementTorsionsX[memberIndex];
                        momentY = ElementMomentsY[memberIndex];
                        momentZ = ElementMomentsZ[memberIndex];
                    }

                    double lengthOfElement = member.StartPoint.DistanceTo(member.EndPoint);
                    if (reuseElement.getTotalUtilization(axialForce, momentY, momentZ, shearY, shearZ, momentX) < 1
                        && reuseElement.getReusableLength() > lengthOfElement)
                        reuseSuggestionList.Add(reuseElement);
                }
                reuseSuggestionTree.Add(reuseSuggestionList);
                memberIndex++;
            }
            return reuseSuggestionTree;
        }
        protected override bool InsertReuseElementAndCutMaterialBank(int positionIndex, ref MaterialBank materialBank, ReuseElement reuseElement, bool keepCutOff = true)
        {
            if (positionIndex < 0 || positionIndex > ElementsInStructure.Count)
            {
                throw new Exception("The In-Place-Element index " + positionIndex.ToString() + " is not valid!");
            }
            else if (materialBank.RemoveReuseElementFromMaterialBank(reuseElement, ElementsInStructure[positionIndex], keepCutOff))
            {
                MemberElement temp = new SpatialBar(reuseElement, ElementsInStructure[positionIndex]);
                ElementsInStructure.RemoveAt(positionIndex);
                ElementsInStructure.Insert(positionIndex, temp);
                return true;
            }
            return false;
        }
        protected override void InsertReuseElementAndCutMaterialBank(int positionIndex, int elementIndex, ref MaterialBank materialBank, bool keepCutOff = true)
        {
            materialBank.RemoveReuseElementFromMaterialBank(elementIndex, ElementsInStructure[positionIndex], keepCutOff);

            MemberElement temp = new SpatialBar(materialBank.ElementsInMaterialBank[elementIndex].DeepCopy(), ElementsInStructure[positionIndex]);
            ElementsInStructure.RemoveAt(positionIndex);
            ElementsInStructure.Insert(positionIndex, temp);
        }
        protected override void InsertNewElement(int memberIndex, List<string> sortedNewElements, double minimumArea)
        {
            if (memberIndex < 0 || memberIndex > ElementsInStructure.Count)
            {
                throw new Exception("The In-Place-Element index " + memberIndex.ToString() + " is not valid!");
            }
            else
            {
                while(true)
                {
                    if (sortedNewElements.Count == 0) return;
                    string newProfile = sortedNewElements[0];
                    sortedNewElements.RemoveAt(0);
                    ReuseElement newElement = new ReuseElement(newProfile, ElementsInStructure[memberIndex].getInPlaceElementLength());
                    MemberElement temp = new SpatialBar(newElement, ElementsInStructure[memberIndex]);
                    temp.IsFromMaterialBank = false;

                    if (temp.getTotalUtilization(ElementAxialForcesX[memberIndex], 0, 0, 0, 0, 0) > 1
                        || temp.getAxialBucklingUtilization(ElementAxialForcesX[memberIndex]) > 1) continue;
                    else
                    {
                        ElementsInStructure.RemoveAt(memberIndex);
                        ElementsInStructure.Insert(memberIndex, temp);
                        return;
                    }
                }
            }
        }
        public override bool UpdateInsertionMatrix(ref Matrix<double> insertionMatrix, int reuseElementIndex, int positionIndex, MaterialBank materialBank, bool keepCutOff = true)
        {
            if (positionIndex < 0 || positionIndex > ElementsInStructure.Count)
            {
                throw new Exception("The In-Place-Element index " + positionIndex.ToString() + " is not valid!");
            }
            else if( keepCutOff )
            {
                insertionMatrix[positionIndex, reuseElementIndex] =
                    ElementsInStructure[positionIndex].StartPoint.DistanceTo(ElementsInStructure[positionIndex].EndPoint);
                return true;
            }
            else
            {
                insertionMatrix[positionIndex, reuseElementIndex] =
                    materialBank.ElementsInMaterialBank[reuseElementIndex].getReusableLength();
                return true;
            }

        }
        
    }






    public class PlanarTruss : SpatialTruss
    {
        // Constructors
        public PlanarTruss(List<Line> lines, List<string> profileNames, List<Point3d> supportPoints)
            : base(lines, profileNames, supportPoints)
        {

        }

        // Overriden Methods
        protected override int GetDofsPerNode()
        {
            return 2;
        }
        public override void ApplyLineLoad(double loadValue, Vector3d loadDirection, Vector3d distributionDirection, List<Line> loadElements, loadMethod method = loadMethod.consistent)
        {
            loadDirection.Unitize();
            int dofsPerNode = GetDofsPerNode();
            foreach (SpatialBar element in ElementsInStructure)
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
            foreach (MemberElement element in ElementsInStructure)
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

        // -- MEMBER REPLACEMENT METHODS --
        public override List<List<ReuseElement>> GetReusabilityTree(MaterialBank materialBank)
        {
            List<List<ReuseElement>> reusablesSuggestionTree = new List<List<ReuseElement>>();
            int memberIndex = 0;
            foreach (PlanarBar elementInStructure in ElementsInStructure)
            {
                List<ReuseElement> StockElementSuggestionList = new List<ReuseElement>();
                for (int i = 0; i < materialBank.ElementsInMaterialBank.Count; i++)
                {
                    ReuseElement stockElement = materialBank.ElementsInMaterialBank[i];

                    double axialForce = ElementAxialForcesX[memberIndex];
                    double shearY = 0;
                    double shearZ = 0;
                    double momentX = 0;
                    double momentY = 0;
                    double momentZ = 0;
                    if (GetDofsPerNode() > 3)
                    {
                        shearY = ElementShearForcesY[memberIndex];
                        shearZ = ElementShearForcesZ[memberIndex];
                        momentX = ElementTorsionsX[memberIndex];
                        momentY = ElementMomentsY[memberIndex];
                        momentZ = ElementMomentsZ[memberIndex];
                    }

                    double lengthOfElement = elementInStructure.StartPoint.DistanceTo(elementInStructure.EndPoint);
                    if (stockElement.getTotalUtilization(axialForce, momentY, momentZ, shearY, shearZ, momentX) < 1
                        && stockElement.getReusableLength() > lengthOfElement)
                        StockElementSuggestionList.Add(stockElement);
                }
                reusablesSuggestionTree.Add(StockElementSuggestionList);
                memberIndex++;
            }
            return reusablesSuggestionTree;
        }
        protected override bool InsertReuseElementAndCutMaterialBank(int inPlaceElementIndex, ref MaterialBank materialBank, ReuseElement stockElement, bool keepCutOff = true)
        {
            if (inPlaceElementIndex < 0 || inPlaceElementIndex > ElementsInStructure.Count)
            {
                throw new Exception("The In-Place-Element index " + inPlaceElementIndex.ToString() + " is not valid!");
            }
            else if (materialBank.RemoveReuseElementFromMaterialBank(stockElement, ElementsInStructure[inPlaceElementIndex], keepCutOff))
            {
                MemberElement temp = new PlanarBar(stockElement, ElementsInStructure[inPlaceElementIndex]);
                ElementsInStructure.RemoveAt(inPlaceElementIndex);
                ElementsInStructure.Insert(inPlaceElementIndex, temp);
                return true;
            }
            return false;
        }
        protected override void InsertNewElement(int memberIndex, List<string> sortedNewElements, double minimumArea)
        {
            if (memberIndex < 0 || memberIndex > ElementsInStructure.Count)
            {
                throw new Exception("The In-Place-Element index " + memberIndex.ToString() + " is not valid!");
            }
            else
            {
                while (true)
                {
                    if (sortedNewElements.Count == 0) return;
                    string newProfile = sortedNewElements[0];
                    sortedNewElements.RemoveAt(0);
                    ReuseElement newElement = new ReuseElement(newProfile, ElementsInStructure[memberIndex].getInPlaceElementLength());
                    MemberElement temp = new PlanarBar(newElement, ElementsInStructure[memberIndex]);
                    temp.IsFromMaterialBank = false;

                    if (temp.getTotalUtilization(ElementAxialForcesX[memberIndex], 0, 0, 0, 0, 0) > 1
                        || temp.getAxialBucklingUtilization(ElementAxialForcesX[memberIndex]) > 1) continue;
                    else
                    {
                        ElementsInStructure.RemoveAt(memberIndex);
                        ElementsInStructure.Insert(memberIndex, temp);
                        return;
                    }
                }
            }
        }
    }




    public class SpatialFrame : Structure
    {
        // Constructors
        public SpatialFrame()
            : base()
        {
        }
        public SpatialFrame(List<Line> lines, List<string> profileNames, List<Point3d> supportPoints)
            : base(lines, profileNames, supportPoints)
        {
            
        }
        public SpatialFrame(SpatialFrame copyFromThis)
            : this()
        {
            ElementsInStructure = copyFromThis.ElementsInStructure.ToList();
            FreeNodes = copyFromThis.FreeNodes.ToList();
            FreeNodesInitial = copyFromThis.FreeNodesInitial.ToList();
            SupportNodes = copyFromThis.SupportNodes.ToList();
            GlobalStiffnessMatrix = Matrix<double>.Build.SameAs(copyFromThis.GlobalStiffnessMatrix);
            GlobalLoadVector = Vector<double>.Build.SameAs(copyFromThis.GlobalLoadVector);
            GlobalDisplacementVector = Vector<double>.Build.SameAs(copyFromThis.GlobalDisplacementVector);
            ElementAxialForcesX = copyFromThis.ElementAxialForcesX.ToList();
            ElementMomentsY = copyFromThis.ElementMomentsY.ToList();
            ElementMomentsZ = copyFromThis.ElementMomentsZ.ToList();
            ElementTorsionsX = copyFromThis.ElementTorsionsX.ToList();
            ElementShearForcesY = copyFromThis.ElementShearForcesY.ToList();
            ElementShearForcesZ = copyFromThis.ElementMomentsZ.ToList();
            ElementUtilizations = copyFromThis.ElementUtilizations.ToList();
            StructureColors = copyFromThis.StructureColors.ToList();
            StructureVisuals = copyFromThis.StructureVisuals.ToList();
        }

        // Spatial Frame Methods
        protected override int GetDofsPerNode()
        {
            return 6;
        }
        protected override void RecalculateGlobalMatrix()
        {
            GlobalStiffnessMatrix.Clear();
            foreach (MemberElement element in ElementsInStructure)
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
        public override void Retracking()
        {
            List<double> axialOne = new List<double>();

            foreach (SpatialBeam element in ElementsInStructure)
            {
                Vector<double> localDisplacementVector = Vector<double>.Build.Dense(new double[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0});
                Matrix<double> transformationMatrix = element.getTransformationMatrix();
                int dofsPerNode = GetDofsPerNode();

                if (element.StartNodeIndex == -1 && element.EndNodeIndex == -1)
                {
                    ElementAxialForcesX.Add(0);
                    ElementShearForcesY.Add(0);
                    ElementShearForcesZ.Add(0);
                    ElementTorsionsX.Add(0);
                    ElementMomentsY.Add(0);
                    ElementMomentsZ.Add(0);
                    continue;
                }

                if (element.StartNodeIndex != -1) 
                    for(int dof = 0; dof < dofsPerNode; dof++)
                        localDisplacementVector[dof] = GlobalDisplacementVector[dofsPerNode * element.StartNodeIndex + dof];

                if (element.EndNodeIndex != -1) 
                    for (int dof = 0; dof < dofsPerNode; dof++)
                        localDisplacementVector[dofsPerNode + dof] = GlobalDisplacementVector[dofsPerNode * element.EndNodeIndex + dof];

                localDisplacementVector = transformationMatrix.Multiply(localDisplacementVector);
                Vector<double> localLoadVector = element.getElementStiffnessMatrix().Multiply(localDisplacementVector);

                localLoadVector.PointwiseAbs();

                ElementAxialForcesX.Add(localLoadVector[6]); // (+) = Tension
                ElementShearForcesY.Add(Math.Max(Math.Abs(localLoadVector[1]), Math.Abs(localLoadVector[7])));
                ElementShearForcesZ.Add(Math.Max(Math.Abs(localLoadVector[2]), Math.Abs(localLoadVector[8])));
                //ElementShearForcesZ.Add(localLoadVector[8]);
                ElementTorsionsX.Add(Math.Max(Math.Abs(localLoadVector[3]), Math.Abs(localLoadVector[9])));
                ElementMomentsY.Add(Math.Max(Math.Abs(localLoadVector[4]), Math.Abs(localLoadVector[10])));
                //ElementMomentsY.Add(localLoadVector[4]);
                ElementMomentsZ.Add(Math.Max(Math.Abs(localLoadVector[5]), Math.Abs(localLoadVector[11])));
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
        public override void ApplyLineLoad(double loadValue, Vector3d loadDirection, Vector3d distributionDirection, List<Line> loadElements, loadMethod method = loadMethod.consistent)
        {
            loadDirection.Unitize();
            int dofsPerNode = GetDofsPerNode();
            int dimensions = dofsPerNode / 2;
            foreach (SpatialBeam element in ElementsInStructure)
            {
                if (new Vector3d(element.getEndPoint() - element.getStartPoint()) * distributionDirection < 0) distributionDirection.Reverse();
                double projectedElementLength = Math.Abs(element.ProjectedElementLength(distributionDirection));
                foreach (Line loadElement in loadElements)
                {
                    if (element.StartPoint == loadElement.PointAt(0) && element.EndPoint == loadElement.PointAt(1))
                    {
                        Vector3d momentDirection = Cross(loadDirection, distributionDirection);
                        momentDirection.Unitize();
                        if (element.StartNodeIndex != -1)
                        {
                            for (int dim = 0; dim < dimensions; dim++)
                            {
                                GlobalLoadVector[dofsPerNode * element.StartNodeIndex + dim]
                                    += loadValue * projectedElementLength * loadDirection[dim] / 2;
                                if (method == loadMethod.lumped) continue;
                                                                                             
                                GlobalLoadVector[dofsPerNode * element.StartNodeIndex + dimensions + dim]
                                    -= loadValue * projectedElementLength * projectedElementLength * momentDirection[dim] / 12;
                            }
                        }

                        if (element.EndNodeIndex != -1)
                        {
                            for (int dim = 0; dim < dimensions; dim++)
                            {
                                GlobalLoadVector[dofsPerNode * element.EndNodeIndex + dim]
                                    += loadValue * projectedElementLength * loadDirection[dim] / 2;
                                if (method == loadMethod.lumped) continue;

                                GlobalLoadVector[dofsPerNode * element.EndNodeIndex + dimensions + dim]
                                    += loadValue * projectedElementLength * projectedElementLength * momentDirection[dim] / 12;
                            }
                        }
                    }
                }
            }
        }
        public override void ApplySelfWeight(loadMethod method = loadMethod.consistent)
        {
            int dofsPerNode = GetDofsPerNode();
            double gravitationalAcceleration = GetGravitationalAcceleration();

            switch (method)
            {
                case loadMethod.lumped:
                    foreach (MemberElement member in ElementsInStructure)
                    {
                        if (member.StartNodeIndex != -1)
                            GlobalLoadVector[dofsPerNode * member.StartNodeIndex + (dofsPerNode / 2 - 1)] += -member.getMass() * gravitationalAcceleration / 2;
                        if (member.EndNodeIndex != -1)
                            GlobalLoadVector[dofsPerNode * member.EndNodeIndex + (dofsPerNode / 2 - 1)] += -member.getMass() * gravitationalAcceleration / 2;
                    }
                    return;

                case loadMethod.consistent:
                    Vector3d loadDirection = new Vector3d(0, 0, -1);
                    int dimensions = dofsPerNode / 2; 
                    foreach (MemberElement member in ElementsInStructure)
                    {
                        Vector3d distributionDirection = new Vector3d(member.getEndPoint() - member.getStartPoint());
                        Vector3d momentDirection = Cross(loadDirection, distributionDirection);
                        momentDirection.Unitize();
                        double projectedElementLength = Math.Abs(member.ProjectedElementLength(distributionDirection));
                        double loadValue = gravitationalAcceleration * member.getMass() / projectedElementLength;

                        if (member.StartNodeIndex != -1)
                        {
                            for (int dim = 0; dim < dimensions; dim++)
                            {
                                GlobalLoadVector[dofsPerNode * member.StartNodeIndex + dim]
                                    += loadValue * projectedElementLength * loadDirection[dim] / 2;

                                GlobalLoadVector[dofsPerNode * member.StartNodeIndex + dimensions + dim]
                                    -= loadValue * projectedElementLength * projectedElementLength * momentDirection[dim] / 12;
                            }
                        }

                        if (member.EndNodeIndex != -1)
                        {
                            for (int dim = 0; dim < dimensions; dim++)
                            {
                                GlobalLoadVector[dofsPerNode * member.EndNodeIndex + dim]
                                    += loadValue * projectedElementLength * loadDirection[dim] / 2;

                                GlobalLoadVector[dofsPerNode * member.EndNodeIndex + dimensions + dim]
                                    += loadValue * projectedElementLength * projectedElementLength * momentDirection[dim] / 12;
                            }
                        }
                    }
                    return;

            }
            
        }

        // Visuals
        public override void GetLoadVisuals(out List<Brep> geometry, out List<Color> color, double size = -1, double maxLoad = -1, double maxMoment = -1, double maxDisplacement = -1, double maxAngle = -1, bool inwardFacingArrows = true)
        {
            geometry = new List<Brep>();
            color = new List<Color>();

            double displacementFactor = getDisplacementFactor(0.02, size, maxDisplacement);
            double loadLineRadius = getStructureSizeFactor(2e-3, size);
            double spacingLoadArrowToNode = 1e-2 * size;

            List<Point3d> freeNodeDisplacement = new List<Point3d>();
            for (int j = 0; j < FreeNodes.Count; j++)
            {
                freeNodeDisplacement.Add(displacementFactor * (new Point3d(FreeNodes[j] - FreeNodesInitial[j])));
            }

            int dofsPerNode = GetDofsPerNode();
            Vector<double> pointLoadVector = Vector<double>.Build.Dense(3 * FreeNodes.Count);
            Vector<double> momentLoadVector = Vector<double>.Build.Dense(3 * FreeNodes.Count);


            int dimensions = 3;
            for (int i = 0; i < FreeNodes.Count; i++)
            {
                pointLoadVector[dimensions * i] = GlobalLoadVector[dofsPerNode * i];
                pointLoadVector[dimensions * i + 1] = GlobalLoadVector[dofsPerNode * i + 1];
                pointLoadVector[dimensions * i + 2] = GlobalLoadVector[dofsPerNode * i + 2];
                momentLoadVector[dimensions * i] = GlobalLoadVector[dofsPerNode * i + 3];
                momentLoadVector[dimensions * i + 1] = GlobalLoadVector[dofsPerNode * i + 4];
                momentLoadVector[dimensions * i + 2] = GlobalLoadVector[dofsPerNode * i + 5];
            }
            
            for (int i = 0; i < FreeNodes.Count; i++)
            {
                Vector3d pointLoadDirection = new Vector3d(
                    pointLoadVector[dimensions * i],
                    pointLoadVector[dimensions * i + 1],
                    pointLoadVector[dimensions * i + 2]);

                double pointLoadArrowLength = Math.Sqrt(
                    pointLoadVector[dimensions * i] * pointLoadVector[dimensions * i] +
                    pointLoadVector[dimensions * i + 1] * pointLoadVector[dimensions * i + 1] +
                    pointLoadVector[dimensions * i + 2] * pointLoadVector[dimensions * i + 2])
                    * getLoadFactor(0.1, size, maxLoad);

                Vector3d momentDirection = new Vector3d(
                    momentLoadVector[dimensions * i],
                    momentLoadVector[dimensions * i + 1],
                    momentLoadVector[dimensions * i + 2]);

                double momentArrowLength = Math.Sqrt(
                    momentLoadVector[dimensions * i] * momentLoadVector[dimensions * i] +
                    momentLoadVector[dimensions * i + 1] * momentLoadVector[dimensions * i + 1] +
                    momentLoadVector[dimensions * i + 2] * momentLoadVector[dimensions * i + 2])
                    * getLoadFactor(0.1, size, maxMoment);

                pointLoadDirection.Unitize();
                momentDirection.Unitize();
                double coneHeight = 6 * loadLineRadius;
                double coneRadius = 3 * loadLineRadius;

                Point3d pointLoadStartPoint;
                Point3d pointLoadEndPoint;
                Cylinder pointLoadCylinder;
                Cone pointLoadArrow;
                Point3d momentStartPoint;
                Point3d momentEndPoint;
                Cylinder momentCylinder;
                Cone momentArrowFirst;
                Cone momentArrowSecond;

                if (inwardFacingArrows)
                {
                    pointLoadEndPoint = new Point3d(FreeNodesInitial[i] + freeNodeDisplacement[i] - pointLoadDirection * spacingLoadArrowToNode);
                    pointLoadStartPoint = pointLoadEndPoint + new Point3d(-pointLoadDirection * (pointLoadArrowLength + spacingLoadArrowToNode));
                    pointLoadCylinder = new Cylinder(new Circle(new Plane(pointLoadStartPoint, pointLoadDirection), loadLineRadius),
                        pointLoadStartPoint.DistanceTo(pointLoadEndPoint) - coneHeight);
                    pointLoadArrow = new Cone(new Plane(pointLoadEndPoint, new Vector3d(
                        pointLoadVector[dimensions * i],
                        pointLoadVector[dimensions * i + 1],
                        pointLoadVector[dimensions * i + 2])),
                        -coneHeight, coneRadius);

                    momentEndPoint = new Point3d(FreeNodesInitial[i] + freeNodeDisplacement[i] - momentDirection * spacingLoadArrowToNode);
                    momentStartPoint = momentEndPoint + new Point3d(-momentDirection * (momentArrowLength + spacingLoadArrowToNode));
                    momentCylinder = new Cylinder(new Circle(new Plane(momentStartPoint, momentDirection), loadLineRadius),
                        momentStartPoint.DistanceTo(momentEndPoint) - coneHeight);
                    momentArrowFirst = new Cone(new Plane(momentEndPoint, new Vector3d(
                        momentLoadVector[dimensions * i],
                        momentLoadVector[dimensions * i + 1],
                        momentLoadVector[dimensions * i + 2])),
                        -coneHeight, coneRadius);
                    momentArrowSecond = new Cone(new Plane(momentEndPoint - momentDirection * coneHeight, new Vector3d(
                        momentLoadVector[dimensions * i],
                        momentLoadVector[dimensions * i + 1],
                        momentLoadVector[dimensions * i + 2])),
                        -coneHeight, coneRadius);
                }
                else
                {
                    pointLoadStartPoint = new Point3d(FreeNodesInitial[i] + freeNodeDisplacement[i]);
                    pointLoadEndPoint = pointLoadStartPoint + new Point3d(pointLoadDirection * pointLoadArrowLength);
                    pointLoadCylinder = new Cylinder(new Circle(new Plane(pointLoadStartPoint, pointLoadDirection), loadLineRadius),
                        pointLoadStartPoint.DistanceTo(pointLoadEndPoint));
                    pointLoadArrow = new Cone(new Plane(pointLoadEndPoint + pointLoadDirection * coneHeight, new Vector3d(
                        pointLoadVector[dimensions * i],
                        pointLoadVector[dimensions * i + 1],
                        pointLoadVector[dimensions * i + 2])),
                        -coneHeight, coneRadius);


                    momentEndPoint = new Point3d(FreeNodesInitial[i] + freeNodeDisplacement[i] - momentDirection * spacingLoadArrowToNode);
                    momentStartPoint = momentEndPoint + new Point3d(-momentDirection * (momentArrowLength + spacingLoadArrowToNode));
                    momentCylinder = new Cylinder(new Circle(new Plane(momentStartPoint, momentDirection), loadLineRadius),
                        momentStartPoint.DistanceTo(momentEndPoint) - coneHeight);
                    momentArrowFirst = new Cone(new Plane(momentEndPoint, new Vector3d(
                        momentLoadVector[dimensions * i],
                        momentLoadVector[dimensions * i + 1],
                        momentLoadVector[dimensions * i + 2])),
                        -coneHeight, coneRadius);
                    momentArrowSecond = new Cone(new Plane(momentEndPoint - momentDirection * coneHeight, new Vector3d(
                        momentLoadVector[dimensions * i],
                        momentLoadVector[dimensions * i + 1],
                        momentLoadVector[dimensions * i + 2])),
                        -coneHeight, coneRadius);

                }


                StructureVisuals.Add(pointLoadCylinder.ToBrep(true, true));
                StructureColors.Add(Structure.pointLoadArrow);
                StructureVisuals.Add(pointLoadArrow.ToBrep(true));
                StructureColors.Add(Structure.pointLoadArrow);

                StructureVisuals.Add(momentCylinder.ToBrep(true, true));
                StructureColors.Add(Structure.momentArrow);
                StructureVisuals.Add(momentArrowFirst.ToBrep(true));
                StructureColors.Add(Structure.momentArrow);
                StructureVisuals.Add(momentArrowSecond.ToBrep(true));
                StructureColors.Add(Structure.momentArrow);

                geometry.Add(pointLoadCylinder.ToBrep(true, true));
                color.Add(Structure.pointLoadArrow);
                geometry.Add(pointLoadArrow.ToBrep(true));
                color.Add(Structure.pointLoadArrow);

                geometry.Add(momentCylinder.ToBrep(true, true));
                color.Add(Structure.momentArrow);
                geometry.Add(momentArrowFirst.ToBrep(true));
                color.Add(Structure.momentArrow);
                geometry.Add(momentArrowSecond.ToBrep(true));
                color.Add(Structure.momentArrow);
            }
        }
        public override void GetResultVisuals(out List<Brep> geometry, out List<Color> color, int colorCode = 0, double size = -1, double maxDisplacement = -1, double maxAngle = -1, double crossSectionSize = 1.0)
        {
            geometry = new List<Brep>();
            color = new List<Color>();

            double displacementFactor = getDisplacementFactor(0.02, size, maxDisplacement);
            double angleFactor = getAngleFactor(0.2 * Math.PI, maxAngle);
            double initialNodeRadius = getStructureSizeFactor(8e-3, size);
            List<Point3d> normalizedNodeDisplacement = new List<Point3d>();            
            List<Point3d> normalizedNodeAngle = new List<Point3d>();

            
            for (int j = 0; j < FreeNodes.Count; j++)
            {
                normalizedNodeDisplacement.Add(displacementFactor * (new Point3d(FreeNodes[j] - FreeNodesInitial[j])));
                int dof = GetDofsPerNode() * j;
                normalizedNodeAngle.Add(angleFactor * (new Point3d(GlobalDisplacementVector[dof + 3], GlobalDisplacementVector[dof + 4], GlobalDisplacementVector[dof + 5])));
            }


            for (int i = 0; i < ElementsInStructure.Count; i++)
            {

                Point3d startOfElement = ElementsInStructure[i].getStartPoint();
                int startNodeIndex = ElementsInStructure[i].getStartNodeIndex();
                if (startNodeIndex != -1)
                    startOfElement += normalizedNodeDisplacement[startNodeIndex];

                Point3d endOfElement = ElementsInStructure[i].getEndPoint();
                int endNodeIndex = ElementsInStructure[i].getEndNodeIndex();
                if (endNodeIndex != -1)
                    endOfElement += normalizedNodeDisplacement[endNodeIndex];

                Vector3d startTangent = new Vector3d(endOfElement - startOfElement);
                Vector3d endTangent = new Vector3d(startOfElement - endOfElement);
                startTangent.Unitize();
                endTangent.Unitize();

                if (startNodeIndex != -1)
                {
                    startTangent.Rotate(normalizedNodeAngle[startNodeIndex].X, new Vector3d(1, 0, 0));
                    startTangent.Rotate(normalizedNodeAngle[startNodeIndex].Y, new Vector3d(0, 1, 0));
                    startTangent.Rotate(normalizedNodeAngle[startNodeIndex].Z, new Vector3d(0, 0, 1));
                }

                if (endNodeIndex != -1)
                {
                    endTangent.Rotate(normalizedNodeAngle[endNodeIndex].X, new Vector3d(1, 0, 0));
                    endTangent.Rotate(normalizedNodeAngle[endNodeIndex].Y, new Vector3d(0, 1, 0));
                    endTangent.Rotate(normalizedNodeAngle[endNodeIndex].Z, new Vector3d(0, 0, 1));
                }

                double dL = startOfElement.DistanceTo(endOfElement) * 0.1;
                Rhino.Geometry.Curve rail = Rhino.Geometry.Curve.CreateControlPointCurve(
                    new List<Point3d>() { startOfElement, startOfElement+startTangent*dL, endOfElement+endTangent*dL, endOfElement });


                List<Brep> cylinders = new List<Brep>();
                double dP = 0.1;
                for (double param = 0; param < 1; param += dP)
                {
                    Point3d start = rail.PointAt(param);
                    Point3d end = rail.PointAt(param+dP);
                    double radii = crossSectionSize * Math.Sqrt(ElementsInStructure[i].CrossSectionArea / Math.PI);

                    Cylinder cylinder = new Cylinder(new Circle(new Plane(start, new Vector3d(end - start)), radii), start.DistanceTo(end));
                    cylinders.Add(cylinder.ToBrep(true, true));
                    geometry.Add(cylinder.ToBrep(true,true));
                }

                // Color codes
                // 0 - Discrete Member Verifiation (Red/Green/Yellow)
                // 1 - New and Reuse Members (White-Blue)
                // 2 - Continuous Member Utilization (White-Blue)
                // 3 - Continuous Member Buckling (White-Blue)
                // 4 - Nx (White-Blue)
                // 5 - Vy (White-Blue)
                // 6 - Vz (White-Blue)
                // 7 - Tx (White-Blue)
                // 8 - My (White-Blue)
                // 9 - Mz (White-Blue)
                // 10 - Continuous Node Displacements (White-Blue)

                double utilizationBuckling = ElementsInStructure[i].getAxialBucklingUtilization(ElementAxialForcesX[i]);
                double utilizationTotal = ElementsInStructure[i].getTotalUtilization(ElementAxialForcesX[i], ElementMomentsY[i], ElementMomentsZ[i], ElementShearForcesY[i], 0, 0);
                double utilizationTorsionX = ElementsInStructure[i].getTorsionalMomentUtilization(ElementTorsionsX[i]);
                double utilizationMomentY = ElementsInStructure[i].getBendingMomentUtilizationY(ElementMomentsY[i]);
                double utilizationMomentZ = ElementsInStructure[i].getBendingMomentUtilizationZ(ElementMomentsZ[i]);
                double utilizationAxialX = ElementsInStructure[i].getAxialForceUtilization(ElementAxialForcesX[i]);
                double utilizationShearY = ElementsInStructure[i].getShearForceUtilizationY(ElementShearForcesY[i]);
                double utilizationShearZ = ElementsInStructure[i].getShearForceUtilizationZ(ElementShearForcesZ[i]);

                foreach ( Brep cylinder in cylinders)
                {
                    if (colorCode == 0)
                    {
                        if (utilizationBuckling > 1)
                            color.Add(bucklingMemberColor);
                        else if (utilizationTotal > 1)
                            color.Add(overUtilizedMemberColor);
                        else
                            color.Add(verifiedMemberColor);
                    }
                    else if (colorCode == 1)
                    {
                        if (ElementsInStructure[i].IsFromMaterialBank)
                            color.Add(reuseMemberColor);
                        else
                            color.Add(newMemberColor);
                    }
                    else if (colorCode == 2) color.Add(BlendColors(totalUtilizationColor, unmarkedGeometry, Math.Abs(utilizationTotal)));
                    else if (colorCode == 3) color.Add(BlendColors(axialBucklingColor, unmarkedGeometry, utilizationBuckling));
                    else if (colorCode == 4) color.Add(BlendColors(axialStressColor, unmarkedGeometry, Math.Abs(utilizationAxialX)));
                    else if (colorCode == 5) color.Add(BlendColors(shearStressColor, unmarkedGeometry, Math.Abs(utilizationShearY)));
                    else if (colorCode == 6) color.Add(BlendColors(shearStressColor, unmarkedGeometry, Math.Abs(utilizationShearZ)));
                    else if (colorCode == 7) color.Add(BlendColors(torsionalMomentStressColor, unmarkedGeometry, Math.Abs(utilizationTorsionX)));
                    else if (colorCode == 8) color.Add(BlendColors(bendingMomentStressColor, unmarkedGeometry, Math.Abs(utilizationMomentY)));
                    else if (colorCode == 9) color.Add(BlendColors(bendingMomentStressColor, unmarkedGeometry, Math.Abs(utilizationMomentZ)));                   
                    else color.Add(unmarkedGeometry);
                }
            }

            foreach (Point3d supportNode in SupportNodes)
            {
                double boxSize = initialNodeRadius;
                Plane boxPlane = new Plane(supportNode, new Vector3d(0, 0, 1));
                Point3d boxPoint1 = new Point3d(supportNode + new Point3d(boxSize, boxSize, boxSize));
                Point3d boxPoint2 = new Point3d(supportNode - new Point3d(boxSize, boxSize, boxSize));
                Box nodeBox = new Box(boxPlane, new List<Point3d>(){ boxPoint1, boxPoint2 });                                         
                geometry.Add(Brep.CreateFromBox(nodeBox));
                color.Add(Structure.supportNodeColor);
            }

            double nodeRadius = initialNodeRadius;
            double liveDisplacement = GetMaxDisplacement();
            for (int i = 0; i < FreeNodesInitial.Count; i++)
            {
                if (colorCode == 10)
                {
                    nodeRadius = 1.5 * initialNodeRadius;
                    double displacement = FreeNodes[i].DistanceTo(FreeNodesInitial[i]);
                    color.Add(BlendColors(markedGeometry, unmarkedGeometry, Math.Abs(displacement / liveDisplacement)));
                }
                else color.Add(Structure.freeNodeColor);

                Vector3d zLocal = new Vector3d(0,0,1);
                zLocal.Rotate(normalizedNodeAngle[i].X, new Vector3d(1, 0, 0));
                zLocal.Rotate(normalizedNodeAngle[i].Y, new Vector3d(0, 1, 0));
                zLocal.Rotate(normalizedNodeAngle[i].Z, new Vector3d(0, 0, 1));

                Vector3d diagonal = new Vector3d(1, 1, 1);
                diagonal.Rotate(normalizedNodeAngle[i].X, new Vector3d(1, 0, 0));
                diagonal.Rotate(normalizedNodeAngle[i].Y, new Vector3d(0, 1, 0));
                diagonal.Rotate(normalizedNodeAngle[i].Z, new Vector3d(0, 0, 1));
                double boxSize = 0.8 * nodeRadius;
                diagonal = diagonal * boxSize;
               
                Plane boxPlane = new Plane(FreeNodesInitial[i] + normalizedNodeDisplacement[i], zLocal);
                Point3d boxPoint1 = new Point3d(FreeNodesInitial[i] + normalizedNodeDisplacement[i] + diagonal);
                Point3d boxPoint2 = new Point3d(FreeNodesInitial[i] + normalizedNodeDisplacement[i] - diagonal);
                Box nodeBox = new Box(boxPlane, new List<Point3d>() { boxPoint1, boxPoint2 });
                geometry.Add(Brep.CreateFromBox(nodeBox));
            }
        }

        // Member Replacement
        public override bool UpdateInsertionMatrix(ref Matrix<double> insertionMatrix, int reuseElementIndex, int positionIndex, MaterialBank materialBank, bool keepCutOff = true)
        {
            if (positionIndex < 0 || positionIndex > ElementsInStructure.Count)
            {
                throw new Exception("The In-Place-Element index " + positionIndex.ToString() + " is not valid!");
            }
            else if (keepCutOff)
            {
                insertionMatrix[positionIndex, reuseElementIndex] =
                    ElementsInStructure[positionIndex].StartPoint.DistanceTo(ElementsInStructure[positionIndex].EndPoint);
                return true;
            }
            else
            {
                insertionMatrix[positionIndex, reuseElementIndex] =
                    materialBank.ElementsInMaterialBank[reuseElementIndex].getReusableLength();
                return true;
            }

        }
        protected override void InsertReuseElementAndCutMaterialBank(int positionIndex, int elementIndex, ref MaterialBank materialBank, bool keepCutOff = true)
        {
            materialBank.RemoveReuseElementFromMaterialBank(elementIndex, ElementsInStructure[positionIndex], keepCutOff);

            MemberElement temp = new SpatialBeam(materialBank.ElementsInMaterialBank[elementIndex].DeepCopy(), ElementsInStructure[positionIndex]);
            ElementsInStructure.RemoveAt(positionIndex);
            ElementsInStructure.Insert(positionIndex, temp);
        }
        protected override void InsertNewElement(int memberIndex, List<string> sortedNewElements, double minimumArea)
        {
            if (memberIndex < 0 || memberIndex > ElementsInStructure.Count)
            {
                throw new Exception("The In-Place-Element index " + memberIndex.ToString() + " is not valid!");
            }
            else
            {
                while (true)
                {
                    if (sortedNewElements.Count == 0) return;
                    string newProfile = sortedNewElements[0];
                    sortedNewElements.RemoveAt(0);
                    ReuseElement newElement = new ReuseElement(newProfile, ElementsInStructure[memberIndex].getInPlaceElementLength());
                    MemberElement temp = new SpatialBeam(newElement, ElementsInStructure[memberIndex]);
                    temp.IsFromMaterialBank = false;

                    if (temp.getTotalUtilization(ElementAxialForcesX[memberIndex], ElementMomentsY[memberIndex], ElementMomentsZ[memberIndex], ElementShearForcesY[memberIndex], ElementShearForcesZ[memberIndex], ElementTorsionsX[memberIndex]) > 1
                        || temp.getAxialBucklingUtilization(ElementAxialForcesX[memberIndex]) > 1) continue;
                    else
                    {
                        ElementsInStructure.RemoveAt(memberIndex);
                        ElementsInStructure.Insert(memberIndex, temp);
                        return;
                    }
                }
            }
        }

    }




    public class MaterialBank : ElementCollection
    {
        // Attributes
        public List<ReuseElement> ElementsInMaterialBank;
        public Matrix<double> InsertionMatrix;
        public List<Color> MaterialBankColors;
        public List<Brep> MaterialBankVisuals;
        public static double minimumReusableLength;
        public static double cuttingLength;

        // Constructors
        public MaterialBank() 
            : base()
        {
            ElementsInMaterialBank = new List<ReuseElement>();
            InsertionMatrix = Matrix<double>.Build.Dense(1,ElementsInMaterialBank.Count);
            MaterialBankColors = new List<Color>();
            MaterialBankVisuals = new List<Brep>();
        }
        public MaterialBank(List<ReuseElement> stockElements)
            :this()
        {
            ElementsInMaterialBank.AddRange(stockElements);
            InsertionMatrix = Matrix<double>.Build.Dense(1, ElementsInMaterialBank.Count);
        }
        public MaterialBank(List<string> profiles, List<int> quantities, List<double> lengths)
            :this()
        {
            if (quantities.Count != lengths.Count || quantities.Count != profiles.Count)
                throw new Exception("Profiles, Quantities and Lengths lists needs to be the same length!");

            for (int i = 0; i < quantities.Count; i++)
            {
                for (int j = 0; j < quantities[i]; j++)
                    this.InsertReuseElementIntoMaterialBank(new ReuseElement(profiles[i], lengths[i]));
            }
            InsertionMatrix = Matrix<double>.Build.Dense(1, ElementsInMaterialBank.Count);
        }
        public MaterialBank(List<string> profiles, List<int> quantities, List<double> lengths, List<double> distancesFabrication, List<double> distancesBuilding, List<double> distancesRecycling)
            : this()
        {
            if (quantities.Count != lengths.Count || quantities.Count != profiles.Count)
                throw new Exception("Profiles, Quantities and Lengths lists needs to be the same length!");

            while (distancesFabrication.Count > profiles.Count)
                distancesFabrication.RemoveAt(distancesFabrication.Count - 1);
            while (distancesBuilding.Count > profiles.Count)
                distancesBuilding.RemoveAt(distancesBuilding.Count - 1);
            while (distancesRecycling.Count > profiles.Count)
                distancesRecycling.RemoveAt(distancesRecycling.Count - 1);

            while (distancesFabrication.Count < profiles.Count)
                distancesFabrication.Add(distancesFabrication[distancesFabrication.Count-1]);
            while (distancesBuilding.Count < profiles.Count)
                distancesBuilding.Add(distancesBuilding[distancesBuilding.Count-1]);
            while (distancesRecycling.Count < profiles.Count)
                distancesRecycling.Add(distancesRecycling[distancesRecycling.Count-1]);

            for (int i = 0; i < quantities.Count; i++)
            {
                for (int j = 0; j < quantities[i]; j++)
                    this.InsertReuseElementIntoMaterialBank(
                        new ReuseElement(profiles[i], lengths[i], distancesFabrication[i], distancesBuilding[i], distancesRecycling[i]));
            }
            InsertionMatrix = Matrix<double>.Build.Dense(1, ElementsInMaterialBank.Count);

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
                    this.InsertReuseElementIntoMaterialBank(new ReuseElement(commandProfiles, commandLengths));
            }
            InsertionMatrix = Matrix<double>.Build.Dense(1, ElementsInMaterialBank.Count);
        }
        static MaterialBank()
        {
            minimumReusableLength = 50;
            cuttingLength = 20;
        }

        // Operator Overloads
        public static MaterialBank operator +(MaterialBank materialBankA, MaterialBank materialBankB)
        {
            MaterialBank returnMateralBank = new MaterialBank();
            returnMateralBank.ElementsInMaterialBank.AddRange(materialBankA.ElementsInMaterialBank);
            returnMateralBank.ElementsInMaterialBank.AddRange(materialBankB.ElementsInMaterialBank);
            returnMateralBank.MaterialBankColors.AddRange(materialBankA.MaterialBankColors);
            returnMateralBank.MaterialBankColors.AddRange(materialBankB.MaterialBankColors);
            returnMateralBank.MaterialBankVisuals.AddRange(materialBankA.MaterialBankVisuals);
            returnMateralBank.MaterialBankVisuals.AddRange(materialBankB.MaterialBankVisuals);

            returnMateralBank.InsertionMatrix = Matrix<double>.Build.Dense(1, returnMateralBank.ElementsInMaterialBank.Count);

            return returnMateralBank;
        }
        
        // Get Methods
        public MaterialBank GetDeepCopy()
        {
            MaterialBank returnMateralBank = new MaterialBank();

            returnMateralBank.ElementsInMaterialBank = ElementsInMaterialBank.ToList();
            returnMateralBank.MaterialBankColors = MaterialBankColors.ToList();
            returnMateralBank.MaterialBankVisuals = MaterialBankVisuals.ToList();
            returnMateralBank.InsertionMatrix = InsertionMatrix.Clone();

            return returnMateralBank;
        }
        public string GetMaterialBankInfo()
        {
            string info = "Material Bank:\n";
            foreach ( ReuseElement stockElement in ElementsInMaterialBank )
                info += "\n" + stockElement.getElementInfo();

            return info;
        }
        public double GetMaterialBankMass()
        {
            double mass = 0;
            foreach (ReuseElement stockElement in ElementsInMaterialBank)
            {
                mass += stockElement.getMass();
            }
            return mass;
        }
        
        // Sorting Methods
        public void sortMaterialBankByLength()
        {
            ElementsInMaterialBank = getLengthSortedMaterialBank();
        }
        public void sortMaterialBankByArea()
        {
            ElementsInMaterialBank = getAreaSortedMaterialBank();
        }
        public List<ReuseElement> getLengthSortedMaterialBank()
        {
            return ElementsInMaterialBank.OrderBy(o => o.getReusableLength()).ToList(); ;
        }
        public List<ReuseElement> getAreaSortedMaterialBank()
        {
            return ElementsInMaterialBank.OrderBy(o => o.CrossSectionArea).ToList();
        }
        public List<ReuseElement> getUtilizationThenLengthSortedMaterialBank(double axialForce, double momentY = 0, double momentZ = 0, double shearY = 0, double shearZ = 0, double momentX = 0)
        {
            if (axialForce == 0 && momentY == 0 && momentZ == 0)
                return ElementsInMaterialBank.OrderBy(o => -o.CrossSectionArea).
                    ThenBy(o => -o.getReusableLength()).ToList();
            else
                return ElementsInMaterialBank.OrderBy(o => Math.Abs( o.getTotalUtilization(axialForce,momentY,momentZ,shearY,shearZ,momentX) )).
                    ThenBy(o => -o.getReusableLength()).ToList();
        }
        public List<int> getUtilizationThenLengthSortedMaterialBankIndexing(double axialForce, double momentY = 0, double momentZ = 0, double shearY = 0, double shearZ = 0, double momentX = 0)
        {
            List<Tuple<int, double, double>> indexUtilizationLengthTuples = new List<Tuple<int, double, double>>();
            for (int i = 0; i < ElementsInMaterialBank.Count; i++)
                indexUtilizationLengthTuples.Add(new Tuple<int, double, double>(i, 
                    ElementsInMaterialBank[i].getTotalUtilization(axialForce, momentY, momentZ, shearY, shearZ, momentX), 
                    ElementsInMaterialBank[i].getReusableLength()));

            indexUtilizationLengthTuples.OrderBy(o => o.Item2).ThenBy(o => o.Item3);

            List<int> result = new List<int>();
            foreach (Tuple<int, double, double> tuple in indexUtilizationLengthTuples)
                result.Add(tuple.Item1);

            return result;
        }

        // Replace Methods By Cutting Reuse Members
        private void InsertReuseElementIntoMaterialBank(ReuseElement reuseElement)
        {
            ElementsInMaterialBank.Add(reuseElement);
        }
        public bool RemoveReuseElementFromMaterialBank(ReuseElement reuseElement, MemberElement memberElement, bool keepCutOff)
        {
            int index = ElementsInMaterialBank.FindIndex(o => reuseElement == o && !o.IsInStructure);
            if ( index == -1 )
            {
                return false;
            }  
            else if (keepCutOff)
            {
                ReuseElement temp = reuseElement.DeepCopy();

                ReuseElement cutOffPart = new ReuseElement(temp.ProfileName,
                    temp.getReusableLength() - memberElement.StartPoint.DistanceTo(memberElement.EndPoint));
                cutOffPart.IsInStructure = false;

                ReuseElement insertPart = new ReuseElement(temp.ProfileName,
                    memberElement.StartPoint.DistanceTo(memberElement.EndPoint));
                insertPart.IsInStructure = true;

                ElementsInMaterialBank[index] = insertPart;
                ElementsInMaterialBank.Add(cutOffPart);
            }
            else
            {
                ElementsInMaterialBank[index].IsInStructure = true;             
            }
            return true;
        }
        public bool RemoveReuseElementFromMaterialBank(int elementIndex, MemberElement memberElement, bool keepCutOff)
        {
            if (keepCutOff)
            {
                ReuseElement temp = ElementsInMaterialBank[elementIndex].DeepCopy();

                ReuseElement cutOffPart = new ReuseElement(temp.ProfileName,
                    temp.getReusableLength() - memberElement.StartPoint.DistanceTo(memberElement.EndPoint));
                cutOffPart.IsInStructure = false;

                ReuseElement insertPart = new ReuseElement(temp.ProfileName,
                    memberElement.StartPoint.DistanceTo(memberElement.EndPoint));
                insertPart.IsInStructure = true;

                ElementsInMaterialBank[elementIndex] = insertPart;
                ElementsInMaterialBank.Add(cutOffPart);
            }
            else
            {
                ElementsInMaterialBank[elementIndex].IsInStructure = true;
            }
            return true;
        }

        // Visuals
        public override void GetVisuals(out List<Brep> geometry, out List<Color> color, out string codeInfo, int colorCode, double size, double maxDisplacement, double maxAngle, double maxLoad, double maxMoment, double crossSectionSize = 1.0)
        {
            colorCode = colorCode % 6;
            if(colorCode < 4) UpdateVisualsMaterialBank(out geometry, out color, out codeInfo, colorCode);
            else UpdateVisualsInsertionMatrix(out geometry, out color, out codeInfo, colorCode);
        }
        public void UpdateVisualsMaterialBank(out List<Brep> geometry, out List<Color> color, out string codeInfo, int colorCode = 0)
        {
            geometry = new List<Brep>();
            color = new List<Color>();
            colorCode = colorCode % 4;
            List<string> codeInfos = new List<string>()
            {
                "0 - Sorted and grouped by length",
                "1 - Sorted and grouped by area",
                "2 - Sorted by lenght",
                "3 - Sorted by area"
            };
            codeInfo = codeInfos[colorCode];
          
            if (ElementsInMaterialBank.Count == 0)
                return;

            int groupingMethod = -1;
            if (colorCode == 0)
            {
                groupingMethod = 0;
                sortMaterialBankByLength();
            }
            else if (colorCode == 1)
            {
                groupingMethod = 1;
                sortMaterialBankByArea();
            }
            else if (colorCode == 2)
            {
                sortMaterialBankByLength();
            }
            else if (colorCode == 3)
            {
                sortMaterialBankByArea();
            }

            int group = 0;
            double groupSpacing = 0;
            double unusedInstance = 0;
            double usedInstance = 0;
            double instanceSpacing = 100;
            double startSpacing = 100;
             

            ReuseElement priorElement = ElementsInMaterialBank[0];
            foreach (ReuseElement element in ElementsInMaterialBank)
            {
                if (groupingMethod == 0)
                {
                    if (priorElement.getReusableLength() != element.getReusableLength())
                    {
                        group++;
                        groupSpacing += (Math.Sqrt(element.CrossSectionArea) + Math.Sqrt(element.CrossSectionArea)) / Math.PI + 2 * instanceSpacing;
                        unusedInstance = 0;
                        usedInstance = 0;
                    }
                }
                else if (groupingMethod == 1)
                {
                    if (priorElement.CrossSectionArea != element.CrossSectionArea)
                    {
                        group++;
                        groupSpacing += (Math.Sqrt(element.CrossSectionArea) + Math.Sqrt(element.CrossSectionArea)) / Math.PI + 2 * instanceSpacing;
                        unusedInstance = 0;
                        usedInstance = 0;
                    }
                }


                Plane basePlane = new Plane();

                if(colorCode == 0 || colorCode == 1)
                {
                    color.Add(ElementCollection.materialBankColors[group % ElementCollection.materialBankColors.Count]);

                    if (element.IsInStructure)
                    {
                        basePlane = new Plane(new Point3d(usedInstance + startSpacing, groupSpacing, 0), new Vector3d(0, 0, 1));
                        usedInstance = usedInstance + 2 * Math.Sqrt(element.CrossSectionArea) / Math.PI + instanceSpacing;
                        color[color.Count - 1] = System.Drawing.Color.FromArgb(50, color[color.Count - 1]);
                    }
                    else
                    {
                        basePlane = new Plane(new Point3d(-unusedInstance - startSpacing, groupSpacing, 0), new Vector3d(0, 0, 1));
                        unusedInstance = unusedInstance + 2 * Math.Sqrt(element.CrossSectionArea) / Math.PI + instanceSpacing;
                    }
                }
                else if (colorCode == 2 || colorCode == 3)
                {
                    basePlane = new Plane(new Point3d(- (usedInstance + unusedInstance) - startSpacing, groupSpacing, 0), new Vector3d(0, 0, 1));                    

                    if (element.IsInStructure)
                    {
                        usedInstance = usedInstance + 2 * Math.Sqrt(element.CrossSectionArea) / Math.PI + instanceSpacing;
                        color.Add(ElementCollection.materialBankColors[0]);
                    }
                    else
                    {                   
                        unusedInstance = unusedInstance + 2 * Math.Sqrt(element.CrossSectionArea) / Math.PI + instanceSpacing;
                        color.Add(ElementCollection.materialBankColors[1]);
                    }
                }

               
                Circle baseCircle = new Circle(basePlane, Math.Sqrt(element.CrossSectionArea) / Math.PI);
                Cylinder cylinder = new Cylinder(baseCircle, element.getReusableLength());
                geometry.Add(cylinder.ToBrep(true, true));

                priorElement = element;

            }

            MaterialBankVisuals = geometry;
            MaterialBankColors = color;

        }
        public void UpdateVisualsMaterialBank(int groupingMethod = 0)
        {
            this.UpdateVisualsMaterialBank(out _, out _, out _, groupingMethod);
        }
        public void UpdateVisualsInsertionMatrix(out List<Brep> geometry, out List<Color> color, out string codeInfo, int colorCode = 0)
        {
            geometry = new List<Brep>();
            color = new List<Color>();
            colorCode = colorCode % 2;
            List<string> codeInfos = new List<string>()
            {
                "0 - Sorted by length",
                "1 - Sorted by length and constant cross section"
            };
            codeInfo = codeInfos[colorCode];

            if (ElementsInMaterialBank.Count == 0)
                return;

            int groupingMethod = -1;
            if (colorCode == 0)
            {
                groupingMethod = 0;
            }

            int group = 0;
            double groupSpacing = 0;
            double unusedInstance = 0;
            double usedInstance = 0;
            double instanceSpacing = 100;
            double startSpacing = 100;

            List<Plane> basePlanes = new List<Plane>();
            List<Circle> baseCircles = new List<Circle>();
            List<Cylinder> cylinders = new List<Cylinder>();

            double crossSectionRadiusVisuals = 0;

            for (int i = 0; i < ElementsInMaterialBank.Count; i++)
            {
                if (colorCode == 0)
                    crossSectionRadiusVisuals = Math.Sqrt( ElementsInMaterialBank[i].CrossSectionArea ) / Math.PI;
                else if (colorCode == 1)
                {
                    double maxLength = 0;
                    ElementsInMaterialBank.ForEach(o => maxLength = Math.Max(maxLength, o.getReusableLength()));
                    crossSectionRadiusVisuals = maxLength * 5e-2;
                }


                basePlanes.Add( new Plane(new Point3d(-(usedInstance + unusedInstance) - startSpacing, groupSpacing, 0), new Vector3d(0, 0, 1)) );
                baseCircles.Add( new Circle(basePlanes[basePlanes.Count-1], crossSectionRadiusVisuals) );
                cylinders.Add( new Cylinder(baseCircles[baseCircles.Count-1], ElementsInMaterialBank[i].getReusableLength()) );
                geometry.Add(cylinders[cylinders.Count - 1].ToBrep(true, true));
                color.Add(ElementCollection.materialBankColors[0]);

                Vector<double> insertionVector = InsertionMatrix.Column(i);
                double usedLengthAccumulated = 0;
                bool isUsed = false;

                foreach(double usedLength in insertionVector)
                {
                    if (usedLength == 0) continue;
                    else isUsed = true;

                    basePlanes.Add( new Plane(new Point3d(-(usedInstance + unusedInstance) - startSpacing, groupSpacing, usedLengthAccumulated), new Vector3d(0, 0, 1)) );
                    baseCircles.Add( new Circle(basePlanes[basePlanes.Count-1], 1.1 * crossSectionRadiusVisuals) );
                    cylinders.Add( new Cylinder(baseCircles[baseCircles.Count-1], usedLength) );
                    geometry.Add(cylinders[cylinders.Count-1].ToBrep(true, true));
                    color.Add(ElementCollection.materialBankColors[3]);

                    usedLengthAccumulated += usedLength;

                    basePlanes.Add(new Plane(new Point3d(-(usedInstance + unusedInstance) - startSpacing, groupSpacing, usedLengthAccumulated), new Vector3d(0, 0, 1)));
                    baseCircles.Add(new Circle(basePlanes[basePlanes.Count - 1], 1.1 * crossSectionRadiusVisuals));
                    cylinders.Add(new Cylinder(baseCircles[baseCircles.Count - 1], cuttingLength));
                    geometry.Add(cylinders[cylinders.Count - 1].ToBrep(true, true));
                    color.Add(ElementCollection.materialBankColors[2]);

                    usedLengthAccumulated += cuttingLength;
                }

                if (isUsed)
                {
                    geometry.RemoveAt(geometry.Count - 1);
                    color.RemoveAt(color.Count - 1);
                }

                unusedInstance = unusedInstance + 2 * crossSectionRadiusVisuals + instanceSpacing;
            }

            MaterialBankVisuals = geometry;
            MaterialBankColors = color;
        }
        public void GetVisualsInsertionMatrix(out List<Brep> geometry, out List<Color> color, out string codeInfo, Matrix<double> insertionMatrix, int colorCode = 0)
        {
            geometry = new List<Brep>();
            color = new List<Color>();
            colorCode = colorCode % 2;
            List<string> codeInfos = new List<string>()
            {
                "0 - Sorted by length",
                "1 - Sorted by length and constant cross section"
            };
            codeInfo = codeInfos[colorCode];

            if (ElementsInMaterialBank.Count == 0)
                return;

            int groupingMethod = -1;
            if (colorCode == 0)
            {
                groupingMethod = 0;
            }

            int group = 0;
            double groupSpacing = 0;
            double unusedInstance = 0;
            double usedInstance = 0;
            double instanceSpacing = 100;
            double startSpacing = 100;

            List<Plane> basePlanes = new List<Plane>();
            List<Circle> baseCircles = new List<Circle>();
            List<Cylinder> cylinders = new List<Cylinder>();

            double crossSectionRadiusVisuals = 0;

            for (int i = 0; i < ElementsInMaterialBank.Count; i++)
            {
                if (colorCode == 0)
                    crossSectionRadiusVisuals = Math.Sqrt(ElementsInMaterialBank[i].CrossSectionArea) / Math.PI;
                else if (colorCode == 1)
                {
                    double maxLength = 0;
                    ElementsInMaterialBank.ForEach(o => maxLength = Math.Max(maxLength, o.getReusableLength()));
                    crossSectionRadiusVisuals = maxLength * 5e-2;
                }


                basePlanes.Add(new Plane(new Point3d(-(usedInstance + unusedInstance) - startSpacing, groupSpacing, 0), new Vector3d(0, 0, 1)));
                baseCircles.Add(new Circle(basePlanes[basePlanes.Count - 1], crossSectionRadiusVisuals));
                cylinders.Add(new Cylinder(baseCircles[baseCircles.Count - 1], ElementsInMaterialBank[i].getReusableLength()));
                geometry.Add(cylinders[cylinders.Count - 1].ToBrep(true, true));
                color.Add(ElementCollection.materialBankColors[0]);

                Vector<double> insertionVector = insertionMatrix.Column(i);
                double usedLengthAccumulated = 0;
                bool isUsed = false;

                foreach (double usedLength in insertionVector)
                {
                    if (usedLength == 0) continue;
                    else isUsed = true;

                    basePlanes.Add(new Plane(new Point3d(-(usedInstance + unusedInstance) - startSpacing, groupSpacing, usedLengthAccumulated), new Vector3d(0, 0, 1)));
                    baseCircles.Add(new Circle(basePlanes[basePlanes.Count - 1], 1.1 * crossSectionRadiusVisuals));
                    cylinders.Add(new Cylinder(baseCircles[baseCircles.Count - 1], usedLength));
                    geometry.Add(cylinders[cylinders.Count - 1].ToBrep(true, true));
                    color.Add(ElementCollection.materialBankColors[3]);

                    usedLengthAccumulated += usedLength;

                    basePlanes.Add(new Plane(new Point3d(-(usedInstance + unusedInstance) - startSpacing, groupSpacing, usedLengthAccumulated), new Vector3d(0, 0, 1)));
                    baseCircles.Add(new Circle(basePlanes[basePlanes.Count - 1], 1.1 * crossSectionRadiusVisuals));
                    cylinders.Add(new Cylinder(baseCircles[baseCircles.Count - 1], cuttingLength));
                    geometry.Add(cylinders[cylinders.Count - 1].ToBrep(true, true));
                    color.Add(ElementCollection.materialBankColors[2]);

                    usedLengthAccumulated += cuttingLength;
                }

                if (isUsed)
                {
                    geometry.RemoveAt(geometry.Count - 1);
                    color.RemoveAt(color.Count - 1);
                }

                unusedInstance = unusedInstance + 2 * crossSectionRadiusVisuals + instanceSpacing;
            }

            //MaterialBankVisuals = geometry;
            //MaterialBankColors = color;
        }
        public override double GetSize()
        {
            return -1;
        }
        public override double GetMaxLoad()
        {
            return -1;
        }
        public override double GetMaxMoment()
        {
            return -1;
        }
        public override double GetMaxDisplacement()
        {
            return -1;
        }
        public override double GetMaxAngle()
        {
            return -1;
        }

    }
}
