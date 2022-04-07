using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;
using System.Drawing;


namespace MasterthesisGHA
{
    public class LCA
    {
        // Constants
        public static List<List<double>> Constants;

        static LCA()
        {
            List<double> baseCaseConstants = new List<double>() { 0.287, 0.81, 0.110, 0.0011, 0.0011, 0.00011, 0.957, 0.00011, 0.110 };
            List<double> bestCaseConstants = new List<double>() { 0,0,0,0,0,0,0,0,0 };

            Constants = new List<List<double>>() { baseCaseConstants, bestCaseConstants };
        }
        



        // Methods
        public static double GlobalObjectiveFunctionLCA(Structure structure, MaterialBank materialBank, Matrix<double> insertionMatrix, bool bestCase = false)
        {
            double carbonEmissionTotal = 0;
            List<double> constants = new List<double>();
            if (bestCase) constants = Constants[1];
            else constants = Constants[0];
            
            for (int stockElementIndex = 0; stockElementIndex < materialBank.StockElementsInMaterialBank.Count; stockElementIndex++)
            {
                StockElement stockElement = materialBank.StockElementsInMaterialBank[stockElementIndex];
                double reuseLength = 0;
                double wasteLength = 0;
                bool cut = false;

                foreach (double insertionLength in insertionMatrix.Row(stockElementIndex))
                {
                    reuseLength += insertionLength;
                    if (insertionLength != 0)
                    {
                        if (!cut) cut = true;
                        else wasteLength += MaterialBank.cuttingLength;
                    }
                }

                double remainingLength = materialBank.StockElementsInMaterialBank[stockElementIndex].GetStockElementLength() - reuseLength;
                if (remainingLength < MaterialBank.minimumReusableLength)
                    wasteLength += remainingLength;

                carbonEmissionTotal +=
                    0.287 * stockElement.getMass() +
                    0.81 * stockElement.getMass(wasteLength) +
                    0.110 * stockElement.getMass(reuseLength) +
                    0.0011 * stockElement.getMass() * stockElement.DistanceFabrication +
                    0.0011 * stockElement.getMass(reuseLength) * stockElement.DistanceBuilding +
                    0.00011 * stockElement.getMass(wasteLength) * stockElement.DistanceRecycling;
            }

            for (int memberIndex = 0; memberIndex < structure.ElementsInStructure.Count; memberIndex++)
            {                
                if (insertionMatrix.Column(memberIndex).AbsoluteMaximum() != 0) continue;

                InPlaceElement member = structure.ElementsInStructure[memberIndex];
                carbonEmissionTotal +=
                    0.957 * member.getMass() +
                    0.00011 * member.getMass() * 1 + // * distanceBuilding
                    0.110 * member.getMass();
            }

            return carbonEmissionTotal;
        }
        public static double GlobalObjectiveFunctionLCA(Structure structure, MaterialBank materialBank, bool bestCase = false)
        {
            return 1.00 * structure.GetReusedMass() + 1.50 * structure.GetNewMass();
        }

    }
}
