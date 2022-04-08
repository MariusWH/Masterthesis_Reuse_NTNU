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
    public class ObjectiveFunctions
    {
        // Constants
        public static List<double> constantsAdvancedLCA;
        public static List<double> constantsSimplifiedLCA;
        static ObjectiveFunctions()
        {
            List<double> constantsAdvancedLCA = new List<double>() { 0.287, 0.00081, 0.110, 0.0011, 0.0011, 0.00011, 0.957, 0.00011, 0.110 };
            List<double> constantsSimplifiedLCA = new List<double>() { 0.21, 0.00081, 0, 0, 0, 0, 1.75, 0, 0 };
        }
        


        // Methods
        public static double GlobalLCA(Structure structure, MaterialBank materialBank, Matrix<double> insertionMatrix, string method = "simplified")
        {
            List<double> constants = new List<double>();
            switch (method)
            {
                case "simplified":
                    constants = constantsSimplifiedLCA;
                    break;
                case "advanced":
                    constants = constantsAdvancedLCA;
                    break;
            }

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
                    constants[0] * stockElement.getMass() +
                    constants[1] * stockElement.getMass(wasteLength) +
                    constants[2] * stockElement.getMass(reuseLength) +
                    constants[3] * stockElement.getMass() * stockElement.DistanceFabrication +
                    constants[4] * stockElement.getMass(reuseLength) * stockElement.DistanceBuilding +
                    constants[5] * stockElement.getMass(wasteLength) * stockElement.DistanceRecycling;
            }

            for (int memberIndex = 0; memberIndex < structure.ElementsInStructure.Count; memberIndex++)
            {                
                if (insertionMatrix.Column(memberIndex).AbsoluteMaximum() != 0) continue;

                InPlaceElement member = structure.ElementsInStructure[memberIndex];
                carbonEmissionTotal +=
                    constants[6] * member.getMass() +
                    constants[7] * member.getMass() * 1 + // * distanceBuilding
                    constants[8] * member.getMass();
            }

            return carbonEmissionTotal;
        }     
        public static double GlobalLCA(Structure structure, MaterialBank materialBank)
        {
            return 1.00 * structure.GetReusedMass() + 1.50 * structure.GetNewMass();
        }
        public static double LocalLCA(InPlaceElement member, StockElement stockElement, double axialForce, string method = "simplified")
        {
            List<double> constants = new List<double>();
            switch (method)
            {
                case "simplified":
                    constants = constantsSimplifiedLCA;
                    break;
                case "advanced":
                    constants = constantsAdvancedLCA;
                    break;
            }

            double reuseLength = member.getInPlaceElementLength();
            double wasteLength = stockElement.GetStockElementLength() - member.getInPlaceElementLength();

            if (wasteLength < 0 || stockElement.CheckUtilization(axialForce) > 1 || stockElement.CheckAxialBuckling(axialForce, reuseLength) > 1)
            {
                return -1;
            }
            
            double reuseElementLCA =
                constants[0] * stockElement.getMass() +
                constants[1] * stockElement.getMass(wasteLength) +
                constants[2] * stockElement.getMass(reuseLength) +
                constants[3] * stockElement.getMass() * stockElement.DistanceFabrication +
                constants[4] * stockElement.getMass(reuseLength) * stockElement.DistanceBuilding +
                constants[5] * stockElement.getMass(wasteLength) * stockElement.DistanceRecycling;

            double newElementLCA =
                constants[6] * member.getMass() +
                constants[7] * member.getMass() * 1 + // * distanceBuilding
                constants[8] * member.getMass();


            double carbonEmissionReduction = newElementLCA - reuseElementLCA;
            if (carbonEmissionReduction < 0)
            {
                return -1;
            }
            else
            {
                return carbonEmissionReduction;
            }

        }





    }
}
