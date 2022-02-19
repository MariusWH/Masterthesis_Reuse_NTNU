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
    public abstract class AbstractLineElement
    {
        // Variables
        public string ProfileName;
        public double CrossSectionArea;
        public double AreaMomentOfInertiaYY;
        public double AreaMomentOfInertiaZZ;
        public double PolarMomentOfInertia;
        public double YoungsModulus;
        protected double YieldStress;

        // Static Variables
        protected static Dictionary<string, double> CrossSectionAreaDictionary;
        protected static Dictionary<string, double> AreaMomentOfInertiaYYDictionary;
        protected static Dictionary<string, double> AreaMomentOfInertiaZZDictionary;
        protected static Dictionary<string, double> PolarMomentOfInertiaDictionary;


        // Constructor        
        protected AbstractLineElement(string profileName, double crossSectionArea, double areaMomentOfInertiaYY, 
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
        protected AbstractLineElement(string profileName)
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
        static AbstractLineElement()
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


    }



    internal abstract class InPlaceElement : AbstractLineElement
    {
        // Variables (Not inherited)
        public Point3d StartPoint;
        public Point3d EndPoint;
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
        public InPlaceElement(ref MaterialBank materialBank, int materialBankItemIndex, Point3d startPoint, Point3d endPoint)
            : base(materialBank.StockElementsInMaterialBank[materialBankItemIndex].ProfileName)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;
            IsFromMaterialBank = true;

            UpdateLocalStiffnessMatrix();
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

    internal class InPlaceBarElement3D : InPlaceElement
    {
        // Constructor
        public InPlaceBarElement3D(ref List<Point3d> FreeNodes, ref List<Point3d> SupportNodes, string profileName, Point3d startPoint,
            Point3d endPoint)
            : base(ref FreeNodes, ref SupportNodes, profileName, startPoint, endPoint)
        {

        }
        public InPlaceBarElement3D(ref MaterialBank materialBank, int materialBankItemIndex, Point3d startPoint, Point3d endPoint)
            : base(ref materialBank, materialBankItemIndex, startPoint, endPoint)
        {

        }


        // Overriden Methods
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

    internal class InPlaceBarElement2D : InPlaceBarElement3D
    {
        // Constructor
        public InPlaceBarElement2D(ref List<Point3d> FreeNodes, ref List<Point3d> SupportNodes, string profileName, Point3d startPoint, 
            Point3d endPoint)
            : base(ref FreeNodes, ref SupportNodes, profileName, startPoint, endPoint)
        {

        }
        public InPlaceBarElement2D(ref MaterialBank materialBank, int materialBankItemIndex, Point3d startPoint, Point3d endPoint)
            : base(ref materialBank, materialBankItemIndex, startPoint, endPoint)
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

    }




    internal class StockElement : AbstractLineElement
    {
        // Variables (Not inherited)
        private double ReusableElementLength;



        // Constructor
        public StockElement(string profileName, double reusableElementLength)
            : base(profileName)
        {
            ReusableElementLength = reusableElementLength;
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
        public double CheckUtilization(double normalForce)
        {
            return normalForce/(CrossSectionArea*YieldStress);
        }



    }


}
