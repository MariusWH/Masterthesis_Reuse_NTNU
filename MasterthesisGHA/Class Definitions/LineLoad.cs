using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace MasterthesisGHA.Class_Definitions
{
    internal class LineLoad
    {
        double LoadValue;
        Vector3d LoadDirection;
        Vector3d DistributionDirection;
        List<Line> LoadElements;



        // Constructor
        public LineLoad(double loadValue, Vector3d loadDirection, Vector3d distributionDirection, List<Line> loadElements)
        {

            this.LoadValue = loadValue;
            this.LoadDirection = loadDirection;
            this.DistributionDirection = distributionDirection;
            this.LoadElements = loadElements;



        }







    }
}
