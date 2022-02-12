using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;

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
        public static List<Element> AvaliableProfiles;
        




        // Constructors
        static Element()
        { AvaliableProfiles = GetAvailableProfiles(); }
        
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


        // Static Methods
        protected static List<Element> GetAvailableProfiles()
        {
            return new List<Element>(); // Read from file
        }







    }
}
