using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;

namespace MasterthesisGHA
{
    internal class Load
    {
        List<double> R0;
        

        public Load(List<double> loadList)
        {
            R0 = new List<double>();
        }

        public Load(double loadValue, Vector3d loadDirection, Vector3d distributionDirection, List<Line> loadElements)
        {
            R0 = new List<double>();
        }









    }
}
