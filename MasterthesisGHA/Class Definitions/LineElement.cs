using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;
using System.IO;
using System.Reflection;

namespace MasterthesisGHA
{
    public enum BucklingShape { pinnedpinned, pinnedfixed, fixedfixed };

    public abstract class LineElement
    {
        // Variables
        public string ProfileName;
        public double CrossSectionArea { get; set; }
        public double ShearAreaY;
        public double ShearAreaZ;
        public double PolarMomentOfInertiaX;
        public double AreaMomentOfInertiaY;
        public double AreaMomentOfInertiaZ;      
        public double SectionModulusX;
        public double SectionModulusY;
        public double SectionModulusZ;      
        public double YoungsModulus;
        public double YieldStress;
        public double PoissonsRatio;
        


        // Static Variables
        public static Dictionary<string, double> CrossSectionAreaDictionary;
        public static Dictionary<string, double> AreaMomentOfInertiaYDictionary;
        public static Dictionary<string, double> AreaMomentOfInertiaZDictionary;
        public static Dictionary<string, double> PolarMomentOfInertiaXDictionary;
        public static Dictionary<string, double> SectionModulusXDictionary;
        public static Dictionary<string, double> SectionModulusYDictionary;
        public static Dictionary<string, double> SectionModulusZDictionary;
        public static Dictionary<string, double> ShearAreaYDictionary;
        public static Dictionary<string, double> ShearAreaZDictionary;

        // Constructor        
        protected LineElement(string profileName, double crossSectionArea, double areaMomentOfInertiaYY, double areaMomentOfInertiaZZ,
            double sectionModulusX, double sectionModulusY, double sectionModulusZ, double shearAreaY,
            double shearAreaZ, double polarMomentOfInertia, double youngsModulus)
        {
            ProfileName = profileName;
            CrossSectionArea = crossSectionArea;
            AreaMomentOfInertiaY = areaMomentOfInertiaYY;
            AreaMomentOfInertiaZ = areaMomentOfInertiaZZ;
            PolarMomentOfInertiaX = polarMomentOfInertia;
            SectionModulusX = sectionModulusX;
            SectionModulusY = sectionModulusY;
            SectionModulusZ = sectionModulusZ;
            ShearAreaY = shearAreaY;
            ShearAreaZ = shearAreaZ;
            YoungsModulus = youngsModulus;
            YieldStress = 355;            
        }
        protected LineElement(string profileName)
            : this(profileName, CrossSectionAreaDictionary[profileName], AreaMomentOfInertiaYDictionary[profileName], 
                  AreaMomentOfInertiaZDictionary[profileName], SectionModulusXDictionary[profileName], SectionModulusYDictionary[profileName],
                  SectionModulusZDictionary[profileName], ShearAreaYDictionary[profileName], ShearAreaZDictionary[profileName], 
                  PolarMomentOfInertiaXDictionary[profileName], 210e3)
        {

        }

        // Get functions
        public virtual string getElementInfo()
        {
            return "Not Implemented";
        }

        // Static Constructor and Methods
        static LineElement()
        {
            CrossSectionAreaDictionary = new Dictionary<string, double>();
            AreaMomentOfInertiaYDictionary = new Dictionary<string, double>();
            AreaMomentOfInertiaZDictionary = new Dictionary<string, double>();
            PolarMomentOfInertiaXDictionary = new Dictionary<string, double>();
            SectionModulusXDictionary = new Dictionary<string, double>();
            SectionModulusYDictionary = new Dictionary<string, double>();
            SectionModulusZDictionary = new Dictionary<string, double>();
            ShearAreaYDictionary = new Dictionary<string, double>();
            ShearAreaZDictionary = new Dictionary<string, double>();

            ReadDictionaries();
        }
        protected static void ReadDictionariesSimple()
        {
            string profilesFromFile = "";
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = "MasterthesisGHA.Resources.Profiles.txt";
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream)) profilesFromFile = reader.ReadToEnd();
            var profilesArray = profilesFromFile.Split('\n');

            foreach (string line in profilesArray)
            {
                string[] lineArray = line.Split(','); // name,a,iyy,izz,it,wx,wy,wz

                string name = lineArray[0];
                double a = Convert.ToDouble(lineArray[1]);
                double iyy = Convert.ToDouble(lineArray[2]);
                double izz = Convert.ToDouble(lineArray[3]);
                double it = Convert.ToDouble(lineArray[4]);                
                /*double wx = Convert.ToDouble(lineArray[5]);
                double wy = Convert.ToDouble(lineArray[6]);
                double wz = Convert.ToDouble(lineArray[7]);*/
                
                CrossSectionAreaDictionary.Add(name, a);
                AreaMomentOfInertiaYDictionary.Add(name, iyy);
                AreaMomentOfInertiaZDictionary.Add(name, izz);
                PolarMomentOfInertiaXDictionary.Add(name, it);
                /*SectionModulusXDictionary.Add(name, wx);
                SectionModulusYDictionary.Add(name, wy);
                SectionModulusZDictionary.Add(name, wz);*/

            }
        }
        protected static void ReadDictionaries()
        {
            string profilesFromFile = "";
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = "MasterthesisGHA.Resources.ProfilesAdvanced.txt";
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream)) profilesFromFile = reader.ReadToEnd();
            var profilesArray = profilesFromFile.Split('\n');

            foreach (string line in profilesArray)
            {
                string[] lineArray = line.Split(',');
                // IPE80,80,46,3.8,5.2,5,6,0.328,764,358,478,0.8014,32.4,20.03,23.22,0.08489,10.5,3.691,5.818,6.727,1.77,115.1,135.2
                // Profile,
                // Depth h [mm],
                // Width b [mm],
                // Web thickness t_w [mm],
                // Flange thickness t_f [mm],
                // Root radius r [mm],
                // Weight m [kg/m],
                // Perimeter P [m],
                // Area A [mm^2],
                // Shear area z-z A_v_z for n=1.2 [mm^2],
                // Shear area y-y A_v_y [mm^2],
                // Second moment of area I_y [*10^6 mm^4],
                // Radius of gyration i_y [mm],
                // Elastic Section modulus W_el_y [*10^3 mm^3],
                // Plastic section modulus Wpl_y [*10^3 mm^3],
                // Second moment of area I_z [*10^6 mm^4],
                // radius of gyration i_z [mm],
                // Elastic section modulus W_el_z [*10^3 mm^3],
                // Plastic section modulus Wpl_z [*10^3 mm^3],
                // Torsion constant I_T [*10^3 mm^4],
                // Torsion modulus W_T [10^3 mm^3],
                // Warping constant I_w [*10^6 mm^6],
                // Warping modulus W_w [*10^3 mm^4]

                string profile = lineArray[0];
                double h = Convert.ToDouble(lineArray[1]);
                double b = Convert.ToDouble(lineArray[2]);
                double tw = Convert.ToDouble(lineArray[3]);
                double tf = Convert.ToDouble(lineArray[4]);
                double r = Convert.ToDouble(lineArray[5]);
                double w = Convert.ToDouble(lineArray[6]);
                double p = Convert.ToDouble(lineArray[7]);
                double a = Convert.ToDouble(lineArray[8]); // mm^2
                double avz = Convert.ToDouble(lineArray[9]); // mm^2
                double avy = Convert.ToDouble(lineArray[10]); // mm^2
                double iy = Convert.ToDouble(lineArray[11]); // 10^6 mm^4
                double iyg = Convert.ToDouble(lineArray[12]);
                double wely = Convert.ToDouble(lineArray[13]); // 10^3 mm^3
                double wply = Convert.ToDouble(lineArray[14]);
                double iz = Convert.ToDouble(lineArray[15]); // 10^6 mm^4
                double izg = Convert.ToDouble(lineArray[16]);
                double welz = Convert.ToDouble(lineArray[17]); // 10^3 mm^3
                double wplz = Convert.ToDouble(lineArray[18]);
                double it = Convert.ToDouble(lineArray[19]); // 10^3 mm^4
                double wt = Convert.ToDouble(lineArray[20]); // 10^3 mm^3
                double iw = Convert.ToDouble(lineArray[21]);
                double ww = Convert.ToDouble(lineArray[22]);

                CrossSectionAreaDictionary.Add(profile, a);
                AreaMomentOfInertiaYDictionary.Add(profile, 1e6 * iy);
                AreaMomentOfInertiaZDictionary.Add(profile, 1e6 * iz);
                PolarMomentOfInertiaXDictionary.Add(profile, 1e3 * it);
                SectionModulusXDictionary.Add(profile, 1e3 * wt);
                SectionModulusYDictionary.Add(profile, 1e3 * wely);
                SectionModulusZDictionary.Add(profile, 1e3 * welz);
                ShearAreaYDictionary.Add(profile, avy);
                ShearAreaZDictionary.Add(profile, avz);

            }
        }
        public static List<string> GetCrossSectionAreaSortedProfilesList()
        {
            List<string> profiles = CrossSectionAreaDictionary.Keys.ToList();
            return profiles.OrderBy(o => CrossSectionAreaDictionary[o]).ToList();
        }


        // Structural Analysis
        public virtual double getTotalUtilization(double axialLoad, double momentY, double momentZ, double shearY, double shearZ, double momentX)
        {
            return getAxialForceUtilization(axialLoad)
                + getBendingMomentUtilizationY(momentY)
                + getBendingMomentUtilizationZ(momentZ)
                + getShearForceUtilizationY(shearY)
                + getShearForceUtilizationZ(shearZ)
                + getTorsionalMomentUtilization(momentX);
        }
        public virtual double getAxialForceUtilization(double axialLoad)
        {
            if (axialLoad == double.NaN) axialLoad = 0;
            return Math.Abs(axialLoad / (CrossSectionArea * YieldStress));
        }
        public virtual double getBendingMomentUtilizationY(double momentY)
        {
            if (momentY == double.NaN) momentY = 0;
            return momentY / (SectionModulusY * YieldStress);
        }
        public virtual double getBendingMomentUtilizationZ(double momentZ)
        {
            if (momentZ == double.NaN) momentZ = 0;
            return momentZ / (SectionModulusZ * YieldStress);
        }
        public virtual double getShearForceUtilizationY(double shearForceY)
        {
            if (shearForceY == double.NaN) shearForceY = 0;
            return shearForceY / (ShearAreaY * YieldStress);
        }
        public virtual double getShearForceUtilizationZ(double shearForceZ)
        {
            if (shearForceZ == double.NaN) shearForceZ = 0;
            return shearForceZ / (ShearAreaZ * YieldStress);
        }
        public virtual double getTorsionalMomentUtilization(double momentX)
        {
            if (momentX == double.NaN) momentX = 0;
            return momentX / (SectionModulusX * YieldStress);
        }
        public virtual double getAxialBucklingUtilization(double axialLoad)
        {
            throw new NotImplementedException();
        }

    }



    public abstract class MemberElement : LineElement
    {
        // Variables (Not inherited)
        public readonly Point3d StartPoint;
        public readonly Point3d EndPoint;
        public int StartNodeIndex;
        public int EndNodeIndex;
        public Matrix<double> LocalStiffnessMatrix;
        public bool IsFromMaterialBank;
        public BucklingShape bucklingShape;

        // Constructor
        public MemberElement(ref List<Point3d> FreeNodes, ref List<Point3d> SupportNodes, string profileName, Point3d startPoint, Point3d endPoint,
            double crossSectionArea, double areaMomentOfInertiaYY, double areaMomentOfInertiaZZ,
            double sectionModulusX, double sectionModulusY, double sectionModulusZ, double shearAreaY,
            double shearAreaZ, double polarMomentOfInertia, double youngsModulus)
            : base(profileName, crossSectionArea, areaMomentOfInertiaYY, areaMomentOfInertiaZZ,
            sectionModulusX, sectionModulusY, sectionModulusZ, shearAreaY,
            shearAreaZ, polarMomentOfInertia, youngsModulus)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            IsFromMaterialBank = false;
            bucklingShape = BucklingShape.pinnedpinned;

            UpdateNodes(ref FreeNodes, ref SupportNodes, startPoint, endPoint);
            UpdateLocalStiffnessMatrix();
        }
        public MemberElement(ref List<Point3d> FreeNodes, ref List<Point3d> SupportNodes, string profileName, Point3d startPoint, Point3d endPoint)
            : this(ref FreeNodes, ref SupportNodes, profileName, startPoint, endPoint, CrossSectionAreaDictionary[profileName], AreaMomentOfInertiaYDictionary[profileName],
                  AreaMomentOfInertiaZDictionary[profileName], SectionModulusXDictionary[profileName], SectionModulusYDictionary[profileName],
                  SectionModulusZDictionary[profileName], ShearAreaYDictionary[profileName], ShearAreaZDictionary[profileName],
                  PolarMomentOfInertiaXDictionary[profileName], 210e3)
        {

        }

        // Constructor (From Material Bank)
        public MemberElement(ReuseElement stockElement, MemberElement member)
            : base(stockElement.ProfileName)
        {
            StartPoint = member.StartPoint;
            EndPoint = member.EndPoint;
            StartNodeIndex = member.StartNodeIndex;
            EndNodeIndex = member.EndNodeIndex;
            IsFromMaterialBank = true;
            bucklingShape = BucklingShape.pinnedpinned;

            UpdateLocalStiffnessMatrix();
        }

        // Equal Operators
        public static bool operator ==(MemberElement A, MemberElement B)
        {
            if (A.StartPoint != B.StartPoint ||
                A.EndPoint != B.EndPoint ||
                A.StartNodeIndex != B.StartNodeIndex ||
                A.EndNodeIndex != B.EndNodeIndex)
                return false;

            return true;
        }
        public static bool operator !=(MemberElement A, MemberElement B)
        {
            if (A == B)
                return false;

            return true;
        }


        // Virtual Get Functions
        public virtual Point3d getStartPoint()
        {
            return this.StartPoint;
        }
        public virtual Point3d getEndPoint()
        {
            return this.EndPoint;
        }
        public virtual int getStartNodeIndex()
        {
            return this.StartNodeIndex;
        }
        public virtual int getEndNodeIndex()
        {
            return this.EndNodeIndex;
        }
        public virtual Matrix<double> getLocalStiffnessMatrix()
        {
            return LocalStiffnessMatrix;
        }
        public virtual double getMass()
        {
            double density = 7800 / 1e9;
            return CrossSectionArea * StartPoint.DistanceTo(EndPoint) * density;
        }
        public virtual double getInPlaceElementLength()
        {
            return StartPoint.DistanceTo(EndPoint);
        }
        public override double getAxialBucklingUtilization(double axialLoad)
        {
            double minAreaMomentOfInertia = Math.Min(AreaMomentOfInertiaY, AreaMomentOfInertiaZ);
            double effectiveLoadFactor = 1.0;
            double eulerCriticalLoad = (Math.PI * Math.PI * YoungsModulus * minAreaMomentOfInertia)
                / (effectiveLoadFactor * StartPoint.DistanceTo(EndPoint) * StartPoint.DistanceTo(EndPoint));

            if (axialLoad == double.NaN) axialLoad = 0;
            if (axialLoad > 0)
                return 0;
            else
                return -axialLoad / eulerCriticalLoad;
        }

        // Methods
        protected void UpdateNodes(ref List<Point3d> FreeNodes, ref List<Point3d> SupportNodes, Point3d startPoint, Point3d endPoint)
        {
            if (!SupportNodes.Contains(startPoint))
            {
                if (!FreeNodes.Contains(startPoint))
                    FreeNodes.Add(startPoint);
                StartNodeIndex = FreeNodes.IndexOf(startPoint);
            }
            else
            { StartNodeIndex = -1; }

            if (!SupportNodes.Contains(endPoint))
            {
                if (!FreeNodes.Contains(endPoint))
                    FreeNodes.Add(endPoint);
                EndNodeIndex = FreeNodes.IndexOf(endPoint);
            }
            else
            { EndNodeIndex = -1; }
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

        // Virtual Methods
        protected virtual void UpdateLocalStiffnessMatrix()
        {
            throw new NotImplementedException();
        }

        // Info
        public virtual string getElementInfoString()
        {
            throw new NotImplementedException();
        }
        public override string getElementInfo()
        {
            string info = getElementInfoString() + " "; 
            info += "Profile=" + ProfileName + ", ";
            info += "Length=" + getStartPoint().DistanceTo(getEndPoint()).ToString() + ", ";
            info += "StartIndex=" + getStartNodeIndex().ToString() + ", ";
            info += "EndIndex=" + getEndNodeIndex().ToString() + ", ";
            info += "A=" + CrossSectionArea + ", ";
            info += "E=" + YoungsModulus + ", ";
            info += "StartPoint=" + getStartPoint().ToString() + ", ";
            info += "EndPoint=" + getEndPoint().ToString() + "";
            info += "\\"+"\\";

            return info;
        }

    }




    public class SpatialBar : MemberElement
    {
        // Constructor
        public SpatialBar(ref List<Point3d> FreeNodes, ref List<Point3d> SupportNodes, string profileName, Point3d startPoint, Point3d endPoint)
            : base(ref FreeNodes, ref SupportNodes, profileName, startPoint, endPoint)
        {

        }
        public SpatialBar(ReuseElement stockElement, MemberElement inPlaceElement)
            : base(stockElement, inPlaceElement)
        {

        }


        // Overriden Methods
        protected override void UpdateLocalStiffnessMatrix()
        {
            double elementLength = StartPoint.DistanceTo(EndPoint);
            double cx = (EndPoint.X - StartPoint.X) / elementLength;
            double cy = (EndPoint.Y - StartPoint.Y) / elementLength;
            double cz = (EndPoint.Z - StartPoint.Z) / elementLength;
            double EAL = CrossSectionArea * YoungsModulus / StartPoint.DistanceTo(EndPoint);

            Matrix<double> TranformationOrientation = Matrix<double>.Build.SparseOfArray(new double[,]
            {
                    {cx,cy,cz,0,0,0},
                    {cx,cy,cz,0,0,0},
                    {cx,cy,cz,0,0,0},
                    {0,0,0,cx,cy,cz},
                    {0,0,0,cx,cy,cz},
                    {0,0,0,cx,cy,cz}
            });

            Matrix<double> StiffnessMatrixBar = Matrix<double>.Build.SparseOfArray(new double[,]
            {
                    {1,0,0,-1,0,0},
                    {0,0,0,0,0,0},
                    {0,0,0,0,0,0},
                    {-1,0,0,1,0,0},
                    {0,0,0,0,0,0},
                    {0,0,0,0,0,0}
            });

            LocalStiffnessMatrix =  EAL * TranformationOrientation.Transpose()
                .Multiply(StiffnessMatrixBar.Multiply(TranformationOrientation));
        }                 
        public override string getElementInfoString()
        {
            return "BarMember3D,\t";
        }
    }


    public class PlanarBar : SpatialBar
    {
        // Constructor
        public PlanarBar(ref List<Point3d> FreeNodes, ref List<Point3d> SupportNodes, string profileName, Point3d startPoint, Point3d endPoint)
            : base(ref FreeNodes, ref SupportNodes, profileName, startPoint, endPoint)
        {

        }
        public PlanarBar(ReuseElement reuseElement, MemberElement member)
            : base(reuseElement, member)
        {

        }


        // Overriden Methods
        protected override void UpdateLocalStiffnessMatrix()
        {
            double dz = getEndPoint().Z - getStartPoint().Z;
            double dx = getEndPoint().X - getStartPoint().X;
            double rad = Math.Atan2(dz, dx);
            double EAL = CrossSectionArea * YoungsModulus / getStartPoint().DistanceTo(getEndPoint());
            double cc = Math.Cos(rad) * Math.Cos(rad) * EAL;
            double ss = Math.Sin(rad) * Math.Sin(rad) * EAL;
            double sc = Math.Sin(rad) * Math.Cos(rad) * EAL;

            LocalStiffnessMatrix = Matrix<double>.Build.SparseOfArray(new double[,]
            {
                    {cc,sc,-cc,-sc},
                    {sc,ss,-sc,-ss},
                    {-cc,-sc,cc,sc},
                    {-sc,-ss,sc,ss}
            });

            
        }
        public override string getElementInfoString()
        {
            return "BarMember2D";
        }
    }



    public class SpatialBeam : MemberElement
    {
        // Variables
        public double AlphaAngle;

        // Constructor
        public SpatialBeam(ref List<Point3d> FreeNodes, ref List<Point3d> SupportNodes, string profileName, Point3d startPoint, Point3d endPoint, double alphaAngle = 0)
            : base(ref FreeNodes, ref SupportNodes, profileName, startPoint, endPoint)
        {
            AlphaAngle = alphaAngle;
            bucklingShape = BucklingShape.fixedfixed;
        }
        public SpatialBeam(ReuseElement stockElement, SpatialBeam member)
            : base(stockElement, member)
        {
            AlphaAngle = member.AlphaAngle;
            bucklingShape = BucklingShape.fixedfixed;
        }


        // Overriden Methods
        protected override void UpdateLocalStiffnessMatrix()
        {
            Matrix<double> elementStiffnessMatrix = getElementStiffnessMatrix();
            Matrix<double> transformationMatrix = getTransformationMatrix();
            LocalStiffnessMatrix = transformationMatrix.Transpose().Multiply(elementStiffnessMatrix).Multiply(transformationMatrix);
        }
        public Matrix<double> getElementStiffnessMatrix() 
        {
            double elementLength = StartPoint.DistanceTo(EndPoint);
            double EL = YoungsModulus / Math.Pow(elementLength, 3);

            double c1 = CrossSectionArea * elementLength * elementLength;
            double c2 = 12 * AreaMomentOfInertiaZ;
            double c3 = 12 * AreaMomentOfInertiaY;
            double c4 = 6 * AreaMomentOfInertiaZ * elementLength;
            double c5 = 6 * AreaMomentOfInertiaY * elementLength;
            double c6 = 2 * AreaMomentOfInertiaZ * elementLength * elementLength;
            double c7 = 2 * AreaMomentOfInertiaY * elementLength * elementLength;
            double c8 = PolarMomentOfInertiaX * elementLength * elementLength / (2 * (1 + PoissonsRatio));

            Matrix<double> elementStiffnessMatrix = Matrix<double>.Build.SparseOfArray(new double[,]
            {
                    {c1,0,0,0,0,0,      -c1,0,0,0,0,0},
                    {0,c2,0,0,0,c4,     0,-c2,0,0,0,c4},
                    {0,0,c3,0,-c5,0,    0,0,-c3,0,-c5,0},
                    {0,0,0,c8,0,0,      0,0,0,-c8,0,0},
                    {0,0,-c5,0,2*c7,0,  0,0,c5,0,c7,0},
                    {0,c4,0,0,0,2*c6,   0,-c4,0,0,0,c6},

                    {-c1,0,0,0,0,0,     c1,0,0,0,0,0},
                    {0,-c2,0,0,0,-c4,   0,c2,0,0,0,-c4},
                    {0,0,-c3,0,c5,0,    0,0,c3,0,c5,0},
                    {0,0,0,-c8,0,0,     0,0,0,c8,0,0},
                    {0,0,-c5,0,c7,0,    0,0,c5,0,2*c7,0},
                    {0,c4,0,0,0,c6,     0,-c4,0,0,0,2*c6}

            });

            return EL * elementStiffnessMatrix;
        }
        public Matrix<double> getTransformationMatrix()
        {
            double elementLength = StartPoint.DistanceTo(EndPoint);
            double cosX = (EndPoint.X - StartPoint.X) / elementLength;
            double cosY = (EndPoint.Y - StartPoint.Y) / elementLength;
            double cosZ = (EndPoint.Z - StartPoint.Z) / elementLength;
            double cosRotation = Math.Cos(AlphaAngle);
            double sinRotation = Math.Sin(AlphaAngle);
            double cosXZ = Math.Sqrt(Math.Pow(cosX, 2) + Math.Pow(cosZ, 2));

            Matrix<double> transformationMatrix;

            if (Math.Round(cosXZ, 6) == 0)
            {
                transformationMatrix = Matrix<double>.Build.DenseOfArray(new double[,]
                {
                    { 0,                    cosY,   0},
                    { -cosY * cosRotation,  0,      sinRotation},
                    { cosY* sinRotation,    0,      cosRotation},
                });
            }
            else
            {
                transformationMatrix = Matrix<double>.Build.DenseOfArray(new double[,]
                {
                    {cosX, cosY,cosZ},
                    {(-cosX * cosY * cosRotation - cosZ * sinRotation) / cosXZ, cosXZ* cosRotation,(-cosY * cosZ * cosRotation + cosX * sinRotation)/ cosXZ},
                    {(cosX * cosY * sinRotation - cosZ * cosRotation) / cosXZ, -cosXZ * sinRotation, (cosY * cosZ * sinRotation + cosX * cosRotation) / cosXZ},
                });
            }

            transformationMatrix = transformationMatrix.DiagonalStack(transformationMatrix);
            transformationMatrix = transformationMatrix.DiagonalStack(transformationMatrix);

            return transformationMatrix;
        }
        public override string getElementInfoString()
        {
            return "BeamMember3D";
        }
        public override double getAxialBucklingUtilization(double axialLoad)
        {
            double minAreaMomentOfInertia = Math.Min(AreaMomentOfInertiaY, AreaMomentOfInertiaZ);
            double effectiveLoadFactor = 0.25;
            double eulerCriticalLoad = (Math.PI * Math.PI * YoungsModulus * minAreaMomentOfInertia)
                / (effectiveLoadFactor * StartPoint.DistanceTo(EndPoint) * StartPoint.DistanceTo(EndPoint));

            if (axialLoad == double.NaN) axialLoad = 0;
            if (axialLoad > 0)
                return 0;
            else
                return -axialLoad / eulerCriticalLoad;
        }
    }


    public class ReuseElement : LineElement
    {
        // Variables (Not inherited)
        private double ReusableElementLength;
        public bool IsInStructure;
        public double DistanceFabrication;
        public double DistanceBuilding;
        public double DistanceRecycling;

        // Constructor
        public ReuseElement(string profileName, double reusableElementLength, double distanceFabrication = 100, double distanceBuilding = 100, double distanceRecycling = 100)
            : base(profileName)
        {
            ReusableElementLength = reusableElementLength;
            IsInStructure = false;
            
            DistanceFabrication = distanceFabrication;
            DistanceBuilding = distanceBuilding;
            DistanceRecycling = distanceRecycling;
        }
        public ReuseElement(string profileName, double reusableElementLength, bool isInStructure, double distanceFabrication = 100, double distanceBuilding = 100, double distanceRecycling = 100)
            : this(profileName, reusableElementLength, distanceFabrication, distanceBuilding, distanceRecycling)
        {
            IsInStructure = isInStructure;
        }

        // Methods
        public override string getElementInfo()
        {
            string info = "Reuse Element: ";
            info += "Profile=" + ProfileName + ", ";
            info += "L=" + getReusableLength() + ", ";
            info += "A=" + CrossSectionArea + ", ";
            info += "Iyy=" + AreaMomentOfInertiaY + ", ";
            info += "Izz=" + AreaMomentOfInertiaZ + ", ";
            info += "Ip=" + PolarMomentOfInertiaX + ", ";
            info += "E=" + YoungsModulus + "";
            info += "\\"+"\\";

            return info;
        }
        public double getReusableLength()
        {
            return ReusableElementLength;
        }
        public double getAxialBucklingUtilization(double axialLoad, double inPlaceLength, BucklingShape shape = BucklingShape.pinnedpinned)
        {
            double weakI = Math.Min(AreaMomentOfInertiaY, AreaMomentOfInertiaZ);
            double effectiveLoadFactor = 1.0;
            switch (shape)
            {
                case BucklingShape.pinnedpinned: effectiveLoadFactor = 1.0; break;
                case BucklingShape.pinnedfixed: effectiveLoadFactor = 0.7; break;
                case BucklingShape.fixedfixed: effectiveLoadFactor = 0.5; break;
            }
            
            double eulerCriticalLoad = (Math.PI * Math.PI * YoungsModulus * weakI)
                / Math.Pow(effectiveLoadFactor * inPlaceLength, 2);

            if (axialLoad > 0)
                return 0;
            else
                return -axialLoad / eulerCriticalLoad;
        }
        public double getMass()
        {
            double density = 7800 / 1e9;
            return CrossSectionArea * ReusableElementLength * density;
        }
        public double getMass(double length)
        {
            double density = 7800 / 1e9;
            return CrossSectionArea * length * density;
        }
        public void setReusableLength(double newLength)
        {
            if (newLength < 0)
                throw new Exception("Reusable Length can not be less than zero!");
            ReusableElementLength = newLength;
        }

        // Copy
        public ReuseElement DeepCopy()
        {
            return new ReuseElement(this.ProfileName, this.ReusableElementLength, this.IsInStructure,
                this.DistanceFabrication, this.DistanceBuilding, this.DistanceRecycling);
        }

    }


}
