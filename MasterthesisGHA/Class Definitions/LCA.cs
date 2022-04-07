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





        // Methods
        public static double GlobalObjectiveFunctionLCA(Structure structure, MaterialBank materialBank, Matrix<double> insertionMatrix)
        {
            double carbonEmissionTotal = 0;

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
                InPlaceElement member = structure.ElementsInStructure[memberIndex];

                carbonEmissionTotal +=
                    0.957 * member.getMass() +
                    0.00011 * member.getMass() * 1 + // * distanceBuilding
                    0.110 * member.getMass();
            }

            return carbonEmissionTotal;
        }


    }
}
