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
    public abstract class Element
    {
        // Variables
        public string ProfileName;
        public double CrossSectionArea;
        public double AreaMomentOfInertiaYY;
        public double AreaMomentOfInertiaZZ;
        public double PolarMomentOfInertia;
        public double YoungsModulus;

        // Static Variables
        protected static Dictionary<string, double> CrossSectionAreaDictionary;
        protected static Dictionary<string, double> AreaMomentOfInertiaYYDictionary;
        protected static Dictionary<string, double> AreaMomentOfInertiaZZDictionary;
        protected static Dictionary<string, double> PolarMomentOfInertiaDictionary;



        // Constructors
        static Element()
        {
            CrossSectionAreaDictionary = new Dictionary<string, double>();
            AreaMomentOfInertiaYYDictionary = new Dictionary<string, double>();
            AreaMomentOfInertiaZZDictionary = new Dictionary<string, double>();
            PolarMomentOfInertiaDictionary = new Dictionary<string, double>();

            ReadDictionaries();
        }
        
        protected Element(string profileName, double crossSectionArea, double areaMomentOfInertiaYY, 
            double areaMomentOfInertiaZZ, double polarMomentOfInertia, double youngsModulus)
        {
            ProfileName = profileName;
            CrossSectionArea = crossSectionArea;
            AreaMomentOfInertiaYY = areaMomentOfInertiaYY;
            AreaMomentOfInertiaZZ = areaMomentOfInertiaZZ;
            PolarMomentOfInertia = polarMomentOfInertia;
            YoungsModulus = youngsModulus;


        }



        // Methods
        public virtual Point3d getStartPoint()
        {
            throw new NotImplementedException();
        }
        public virtual Point3d getEndPoint()
        {
            throw new NotImplementedException();
        }
        public virtual int getStartNodeIndex()
        {
            throw new NotImplementedException();
        }
        public virtual int getEndNodeIndex()
        {
            throw new NotImplementedException();
        }
        public virtual Matrix<double> getLocalStiffnessMatrix()
        {
            throw new NotImplementedException();
        }
        public virtual string getElementInfo()
        {
            return "Not Implemented";
        }


        public virtual void UpdateLocalStiffnessMatrix()
        {
            throw new NotImplementedException();
        }
        public virtual double ProjectedElementLength(Vector3d distributionDirection)
        {
            throw new NotImplementedException();
        }
        



        // Static Methods
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


    internal class InPlaceBarElement : Element
    {
        // Variables (Not inherited)
        public Point3d StartPoint;
        public Point3d EndPoint;
        public int StartNodeIndex;
        public int EndNodeIndex;
        public Matrix<double> LocalStiffnessMatrix;


        // Static Variables




        // Constructors
        public InPlaceBarElement( ref List<Point3d> FreeNodes, ref List<Point3d> SupportNodes, string profileName, double crossSectionArea, 
            double areaMomentOfInertiaYY, double areaMomentOfInertiaZZ, double polarMomentOfInertia, double youngsModulus, Point3d startPoint, 
            Point3d endPoint)
            : base(profileName, crossSectionArea, areaMomentOfInertiaYY, areaMomentOfInertiaZZ, polarMomentOfInertia, youngsModulus)
        {
            StartPoint = startPoint;
            EndPoint = endPoint;

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

            UpdateLocalStiffnessMatrix();

        }

        public InPlaceBarElement(ref List<Point3d> FreeNodes, ref List<Point3d> SupportNodes, string profileName, Point3d startPoint, 
            Point3d endPoint)
            : this(ref FreeNodes, ref SupportNodes, profileName, CrossSectionAreaDictionary[profileName], AreaMomentOfInertiaYYDictionary[profileName], 
                  AreaMomentOfInertiaZZDictionary[profileName], PolarMomentOfInertiaDictionary[profileName],210e3,startPoint,endPoint)
        {

        }



        // Methods
        public override Point3d getStartPoint()
        {
            return this.StartPoint;
        }
        public override Point3d getEndPoint()
        {
            return this.EndPoint;
        }
        public override int getStartNodeIndex()
        {
            return this.StartNodeIndex;
        }
        public override int getEndNodeIndex()
        {
            return this.EndNodeIndex;
        }
        public override Matrix<double> getLocalStiffnessMatrix()
        {
            return LocalStiffnessMatrix;
        }


        public override void UpdateLocalStiffnessMatrix()
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


        // Static Constructor


        // Static Methods



    }





    internal class InPlaceBarElement2D : InPlaceBarElement
    {
        // Constructor
        public InPlaceBarElement2D(ref List<Point3d> FreeNodes, ref List<Point3d> SupportNodes, string profileName, double crossSectionArea, 
            double areaMomentOfInertiaYY, double areaMomentOfInertiaZZ, double polarMomentOfInertia, double youngsModulus, Point3d startPoint, 
            Point3d endPoint)
            : base(ref FreeNodes, ref SupportNodes, profileName, crossSectionArea, areaMomentOfInertiaYY, areaMomentOfInertiaZZ, 
                  polarMomentOfInertia, youngsModulus, startPoint, endPoint)
        {

        }

        public InPlaceBarElement2D(ref List<Point3d> FreeNodes, ref List<Point3d> SupportNodes, string profileName, Point3d startPoint, 
            Point3d endPoint)
            : base(ref FreeNodes, ref SupportNodes, profileName, startPoint, endPoint)
        {

        }


        // Overloaded Methods
        public override void UpdateLocalStiffnessMatrix()
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

        /*
        public override void UpdateGlobalMatrix(ref Matrix<double> GlobalStiffnessMatrix)
        {
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
        */




    }














    internal class StockElement // : Element
    {

    }


}
