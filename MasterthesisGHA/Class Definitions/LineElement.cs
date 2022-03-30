﻿using System;
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
    public abstract class LineElement
    {
        // Variables
        public string ProfileName;
        public double CrossSectionArea { get; set; }
        public double AreaMomentOfInertiaYY;
        public double AreaMomentOfInertiaZZ;
        public double PolarMomentOfInertia;
        public double YoungsModulus;
        public double YieldStress;

        // Static Variables
        public static Dictionary<string, double> CrossSectionAreaDictionary;
        public static Dictionary<string, double> AreaMomentOfInertiaYYDictionary;
        public static Dictionary<string, double> AreaMomentOfInertiaZZDictionary;
        public static Dictionary<string, double> PolarMomentOfInertiaDictionary;


        // Constructor        
        protected LineElement(string profileName, double crossSectionArea, double areaMomentOfInertiaYY, 
            double areaMomentOfInertiaZZ, double polarMomentOfInertia, double youngsModulus)
        {
            ProfileName = profileName;
            CrossSectionArea = crossSectionArea;
            AreaMomentOfInertiaYY = areaMomentOfInertiaYY;
            AreaMomentOfInertiaZZ = areaMomentOfInertiaZZ;
            PolarMomentOfInertia = polarMomentOfInertia;
            YoungsModulus = youngsModulus;
            YieldStress = 355;
        }
        protected LineElement(string profileName)
            : this(profileName, CrossSectionAreaDictionary[profileName], AreaMomentOfInertiaYYDictionary[profileName], 
                  AreaMomentOfInertiaZZDictionary[profileName], PolarMomentOfInertiaDictionary[profileName], 210e3)
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
            AreaMomentOfInertiaYYDictionary = new Dictionary<string, double>();
            AreaMomentOfInertiaZZDictionary = new Dictionary<string, double>();
            PolarMomentOfInertiaDictionary = new Dictionary<string, double>();

            ReadDictionaries();
        }
        protected static void ReadDictionaries()
        {
            string profilesFromFile = "";
            var assembly = Assembly.GetExecutingAssembly();
            string resourceName = "MasterthesisGHA.Resources.Profiles.txt";
            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            using (StreamReader reader = new StreamReader(stream))
            {
                profilesFromFile = reader.ReadToEnd();
            }
            var profilesArray = profilesFromFile.Split('\n');
            List<string> profiles = new List<string>();

            foreach (string line in profilesArray)
            {
                string[] lineArray = line.Split(','); // name,a,iyy,izz,it

                string name = lineArray[0];
                double a = Convert.ToDouble(lineArray[1]);
                double iyy = Convert.ToDouble(lineArray[2]);
                double izz = Convert.ToDouble(lineArray[3]);
                double it = Convert.ToDouble(lineArray[4]);

                CrossSectionAreaDictionary.Add(name, a);
                AreaMomentOfInertiaYYDictionary.Add(name, iyy);
                AreaMomentOfInertiaZZDictionary.Add(name, izz);
                PolarMomentOfInertiaDictionary.Add(name, it);

            }
        }
        public static List<string> GetCrossSectionAreaSortedProfilesList()
        {
            List<string> profiles = CrossSectionAreaDictionary.Keys.ToList();
            profiles.OrderBy(o => CrossSectionAreaDictionary[o]);
            return profiles;
        }
    }



    public abstract class InPlaceElement : LineElement
    {
        // Variables (Not inherited)
        public readonly Point3d StartPoint;
        public readonly Point3d EndPoint;
        public int StartNodeIndex;
        public int EndNodeIndex;
        public Matrix<double> LocalStiffnessMatrix;
        public bool IsFromMaterialBank;

        // Constructor
        public InPlaceElement(ref List<Point3d> FreeNodes, ref List<Point3d> SupportNodes, string profileName, double crossSectionArea,
            double areaMomentOfInertiaYY, double areaMomentOfInertiaZZ, double polarMomentOfInertia, double youngsModulus, Point3d startPoint,
            Point3d endPoint)
            : base(profileName, crossSectionArea, areaMomentOfInertiaYY, areaMomentOfInertiaZZ, polarMomentOfInertia, youngsModulus)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            IsFromMaterialBank = false;

            UpdateNodes(ref FreeNodes, ref SupportNodes, startPoint, endPoint);
            UpdateLocalStiffnessMatrix();
        }
        public InPlaceElement(ref List<Point3d> FreeNodes, ref List<Point3d> SupportNodes, string profileName, Point3d startPoint,
            Point3d endPoint)
            : this(ref FreeNodes, ref SupportNodes, profileName, CrossSectionAreaDictionary[profileName], AreaMomentOfInertiaYYDictionary[profileName],
                  AreaMomentOfInertiaZZDictionary[profileName], PolarMomentOfInertiaDictionary[profileName], 210e3, startPoint, endPoint)
        {

        }

        // Constructor (From Material Bank)
        public InPlaceElement(StockElement stockElement, InPlaceElement inPlaceElement)
            : base(stockElement.ProfileName)
        {
            StartPoint = inPlaceElement.StartPoint;
            EndPoint = inPlaceElement.EndPoint;
            StartNodeIndex = inPlaceElement.StartNodeIndex;
            EndNodeIndex = inPlaceElement.EndNodeIndex;
            IsFromMaterialBank = true;

            UpdateLocalStiffnessMatrix();
        }

        // Equal Operators
        public static bool operator ==(InPlaceElement A, InPlaceElement B)
        {
            if (A.StartPoint != B.StartPoint ||
                A.EndPoint != B.EndPoint ||
                A.StartNodeIndex != B.StartNodeIndex ||
                A.EndNodeIndex != B.EndNodeIndex)
                return false;

            return true;
        }
        public static bool operator !=(InPlaceElement A, InPlaceElement B)
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
        public virtual double CheckUtilization(double axialLoad)
        {
            return Math.Abs( axialLoad / (CrossSectionArea * YieldStress) );
        }
        public virtual double CheckAxialBuckling(double axialLoad)
        {
            double minAreaMomentOfInertia = Math.Min(AreaMomentOfInertiaYY, AreaMomentOfInertiaZZ);
            double effectiveLoadFactor = 1.0;
            double eulerCriticalLoad = (Math.PI * Math.PI * YoungsModulus * minAreaMomentOfInertia)
                / (effectiveLoadFactor * StartPoint.DistanceTo(EndPoint));
            
            if (axialLoad > 0)
                return 0;
            else
                return -axialLoad / eulerCriticalLoad;
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


        // Virtual Methods
        protected virtual void UpdateNodes(ref List<Point3d> FreeNodes, ref List<Point3d> SupportNodes, Point3d startPoint, Point3d endPoint)
        {
            throw new NotImplementedException();
        }
        protected virtual void UpdateLocalStiffnessMatrix()
        {
            throw new NotImplementedException();
        }
        public virtual double ProjectedElementLength(Vector3d distributionDirection)
        {
            throw new NotImplementedException();
        }


    }




    public class InPlaceBarElement3D : InPlaceElement
    {
        // Constructor
        public InPlaceBarElement3D(ref List<Point3d> FreeNodes, ref List<Point3d> SupportNodes, string profileName, Point3d startPoint,
            Point3d endPoint)
            : base(ref FreeNodes, ref SupportNodes, profileName, startPoint, endPoint)
        {

        }
        public InPlaceBarElement3D(StockElement stockElement, InPlaceElement inPlaceElement)
            : base(stockElement, inPlaceElement)
        {

        }


        // Overriden Methods
        public override string getElementInfo()
        {
            string info = "InPlaceBarElement3D{ ";
            info += "{start=" + getStartPoint().ToString() + "}, ";
            info += "{end=" + getEndPoint().ToString() + "}, ";
            info += "{startIndex=" + getStartNodeIndex().ToString() + "}, ";
            info += "{endIndex=" + getEndNodeIndex().ToString() + "}, ";
            info += "{A=" + CrossSectionArea + "}, ";
            info += "{E=" + YoungsModulus + "}";
            info += " }";

            return info;
        }
        protected override void UpdateNodes(ref List<Point3d> FreeNodes, ref List<Point3d> SupportNodes, Point3d startPoint, Point3d endPoint)
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
        public override double ProjectedElementLength(Vector3d distributionDirection)
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




    public class InPlaceBarElement2D : InPlaceBarElement3D
    {
        // Constructor
        public InPlaceBarElement2D(ref List<Point3d> FreeNodes, ref List<Point3d> SupportNodes, string profileName, Point3d startPoint, 
            Point3d endPoint)
            : base(ref FreeNodes, ref SupportNodes, profileName, startPoint, endPoint)
        {

        }
        public InPlaceBarElement2D(StockElement stockElement, InPlaceElement inPlaceElement)
            : base(stockElement, inPlaceElement)
        {

        }


        // Overriden Methods
        public override string getElementInfo()
        {
            string info = "InPlaceBarElement2D{ ";
            info += "{start=" + getStartPoint().ToString() + "}, ";
            info += "{end=" + getEndPoint().ToString() + "}, ";
            info += "{startIndex=" + getStartNodeIndex().ToString() + "}, ";
            info += "{endIndex=" + getEndNodeIndex().ToString() + "}, ";
            info += "{A=" + CrossSectionArea + "}, ";
            info += "{E=" + YoungsModulus + "}";
            info += " }";

            return info;
        }
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

    }




    public class StockElement : LineElement
    {
        // Variables (Not inherited)
        private double ReusableElementLength;
        public bool IsInStructure;
        public double DistanceFabrication;
        public double DistanceBuilding;
        public double DistanceRecycling;

        // Constructor
        public StockElement(string profileName, double reusableElementLength, 
            double distanceFabrication = 100, double distanceBuilding = 100, double distanceRecycling = 100)
            : base(profileName)
        {
            ReusableElementLength = reusableElementLength;
            IsInStructure = false;
            
            DistanceFabrication = distanceFabrication;
            DistanceBuilding = distanceBuilding;
            DistanceRecycling = distanceRecycling;
        }
        public StockElement(string profileName, double reusableElementLength, bool isInStructure,
            double distanceFabrication = 100, double distanceBuilding = 100, double distanceRecycling = 100)
            : this(profileName, reusableElementLength, distanceFabrication, distanceBuilding, distanceRecycling)
        {
            IsInStructure = isInStructure;
        }

        // Methods
        public override string getElementInfo()
        {
            string info = "StockElement{ ";
            info += "{L=" + GetStockElementLength() + "}, ";
            info += "{A=" + CrossSectionArea + "}, ";
            info += "{Iyy=" + AreaMomentOfInertiaYY + "}, ";
            info += "{Izz=" + AreaMomentOfInertiaZZ + "}, ";
            info += "{Ip=" + PolarMomentOfInertia + "}, ";
            info += "{E=" + YoungsModulus + "}";
            info += " }";

            return info;
        }
        public double GetStockElementLength()
        {
            return ReusableElementLength;
        }
        public void SetStockElementLength(double newLength)
        {
            if (newLength < 0)
                throw new Exception("Reusable Length can not be less than zero!");
            ReusableElementLength = newLength;
        }      
        public double CheckUtilization(double axialLoad)
        {
            return axialLoad/(CrossSectionArea*YieldStress);
        }
        public double CheckAxialBuckling(double axialLoad, double inPlaceLength)
        {
            double weakI = Math.Min(AreaMomentOfInertiaYY, AreaMomentOfInertiaZZ);
            double effectiveLoadFactor = 1.0;
            double eulerCriticalLoad = (Math.PI * Math.PI * YoungsModulus * weakI)
                / (effectiveLoadFactor * inPlaceLength);

            if (axialLoad > 0)
                return 0;
            else
                return -axialLoad / eulerCriticalLoad;
        }
        public virtual double getMass()
        {
            double density = 7800 / 1e9;
            return CrossSectionArea * ReusableElementLength * density;
        }
        public virtual double getMass(double length)
        {
            double density = 7800 / 1e9;
            return CrossSectionArea * length * density;
        }
        

        // Copy
        public StockElement DeepCopy()
        {
            return new StockElement(this.ProfileName, this.ReusableElementLength, this.IsInStructure,
                this.DistanceFabrication, this.DistanceBuilding, this.DistanceRecycling);
        }

    }


}
