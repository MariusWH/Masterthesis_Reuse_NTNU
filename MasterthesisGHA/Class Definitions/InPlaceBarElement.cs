using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;

namespace MasterthesisGHA.Class_Definitions
{
    internal class InPlaceBarElement : Element
    {
        // Variables (Not inherited)
        public Point3d StartPoint;
        public Point3d EndPoint;
        public int StartNodeIndex;
        public int EndNodeIndex;
        public Matrix<double> StiffnessMatrix;


        // Static Variables
        public static List<InPlaceBarElement> ElementsInStructure;
        public static List<Point3d> FreeNodesInStructure;
        public static List<Point3d> SupportNodesInStructure;
        public static Matrix<double> GlobalStiffnessMatrix;



        // Constructors
        public InPlaceBarElement(string profileName, double crossSectionArea, double areaMomentOfInertiaYY,
            double areaMomentOfInertiaZZ, double polarMomentOfInertia, double youngsModulus, Point3d startPoint, Point3d endPoint) 
            : base(profileName, crossSectionArea, areaMomentOfInertiaYY, areaMomentOfInertiaZZ, polarMomentOfInertia, youngsModulus)
        {            
            StartPoint = startPoint;
            EndPoint = endPoint;

            if (!SupportNodesInStructure.Contains(startPoint))
            {
                if (!FreeNodesInStructure.Contains(startPoint))
                    FreeNodesInStructure.Add(startPoint);
                StartNodeIndex = FreeNodesInStructure.IndexOf(startPoint);
            }
            else
            { StartNodeIndex = -1; }

            if (!SupportNodesInStructure.Contains(endPoint))
            {
                if (!FreeNodesInStructure.Contains(endPoint))
                    FreeNodesInStructure.Add(endPoint);
                EndNodeIndex = FreeNodesInStructure.IndexOf(endPoint);
            }
            else
            { EndNodeIndex = -1; }

            StiffnessMatrix = GetLocalStiffnessMatrix();
        }

   


        // Methods
        public Matrix<double> GetLocalStiffnessMatrix()
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

            return EAL * TranformationOrientation.Transpose()
                .Multiply(StiffnessMatrixBar.Multiply(TranformationOrientation));
        }
        public void UpdateGlobalMatrix()
        {
            for(int row = 0; row < StiffnessMatrix.RowCount/2; row++)
            {
                for(int col = 0; col < StiffnessMatrix.ColumnCount/2; col++ )
                {
                    int dofsPerNode = 6;
                    GlobalStiffnessMatrix[dofsPerNode * StartNodeIndex + row, dofsPerNode * StartNodeIndex + col] += StiffnessMatrix[row, col];
                    GlobalStiffnessMatrix[dofsPerNode * StartNodeIndex + row, dofsPerNode * EndNodeIndex + col] += StiffnessMatrix[row, col + dofsPerNode];
                    GlobalStiffnessMatrix[dofsPerNode * EndNodeIndex + row, dofsPerNode * StartNodeIndex + col] += StiffnessMatrix[row + dofsPerNode, col];
                    GlobalStiffnessMatrix[dofsPerNode * EndNodeIndex + row, dofsPerNode * EndNodeIndex + col] += StiffnessMatrix[row + dofsPerNode, col + dofsPerNode];
                }
            }
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


        // Static Constructor
        static InPlaceBarElement()
        {
            ElementsInStructure = new List<InPlaceBarElement>();
            FreeNodesInStructure = new List<Point3d>();
            SupportNodesInStructure = new List<Point3d>();
            GlobalStiffnessMatrix = Matrix<double>.Build.Dense(0, 0);
        }
        
        
        // Static Methods





    }


}
