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
        public static List<double> constantsFinancialCostAnalysis;
        static ObjectiveFunctions()
        {
            List<double> constantsAdvancedLCA = new List<double>() { 0.287, 0.00081, 0.110, 0.0011, 0.0011, 0.00011, 0.957, 0.00011, 0.110 };
            List<double> constantsSimplifiedLCA = new List<double>() { 0.21, 0.00081, 0, 0, 0, 0, 1.75, 0, 0 };
            List<double> constantsFinancialCostAnalysis = new List<double>() { 0, 0, 0 };
        }


        public enum reuseRateMethod { byCount, byMass };
        public enum lcaMethod { simplified, advanced };



        // Global Objectives (Structure)
        public static double ReuseRate(Structure structure, MaterialBank materialBank, Matrix<double> insertionMatrix, reuseRateMethod method = reuseRateMethod.byCount)
        {
            switch (method)
            {
                case reuseRateMethod.byCount:
                    int inserts = 0;
                    for (int i = 0; i < insertionMatrix.ColumnCount; i++)
                        if (insertionMatrix.Column(i).AbsoluteMaximum() > 0) inserts++;
                    return inserts / insertionMatrix.ColumnCount;

                case reuseRateMethod.byMass:
                    double reuseMass = 0;
                    double newMass = 0;
                    for (int i = 0; i < insertionMatrix.ColumnCount; i++)
                    {
                        double maxInsertedLength = insertionMatrix.Column(i).AbsoluteMaximum();
                        if (maxInsertedLength > 0)
                        {
                            foreach (double insertedLength in insertionMatrix.Column(i))
                                reuseMass += materialBank.ElementsInMaterialBank[i].getMass(insertedLength);
                        }                          
                        else newMass += structure.ElementsInStructure[i].getMass();
                    }                    
                    return reuseMass / (reuseMass + newMass);

            }
            return 0;
        }
        public static double GlobalLCA(Structure structure, MaterialBank materialBank, Matrix<double> insertionMatrix, lcaMethod method = lcaMethod.simplified)
        {
            List<double> constants = new List<double>();
            switch (method)
            {
                case lcaMethod.simplified:
                    constants = new List<double>() { 0.21, 0.00081, 0, 0, 0, 0, 1.75, 0, 0 };
                    break;
                case lcaMethod.advanced:
                    constants = new List<double>() { 0.287, 0.00081, 0.110, 0.0011, 0.0011, 0.00011, 0.957, 0.00011, 0.110 };
                    break;
            }

            double carbonEmissionTotal = 0;
          
            for (int stockElementIndex = 0; stockElementIndex < materialBank.ElementsInMaterialBank.Count; stockElementIndex++)
            {
                ReuseElement stockElement = materialBank.ElementsInMaterialBank[stockElementIndex];
                double reuseLength = 0;
                double wasteLength = 0;
                bool cut = false;

                foreach (double insertionLength in insertionMatrix.Column(stockElementIndex))
                {
                    reuseLength += insertionLength;
                    if (insertionLength != 0)
                    {
                        if (!cut) cut = true;
                        else wasteLength += MaterialBank.cuttingLength;
                    }
                }

                double remainingLength = materialBank.ElementsInMaterialBank[stockElementIndex].getReusableLength() - reuseLength;
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
                if (insertionMatrix.Row(memberIndex).AbsoluteMaximum() != 0) continue;

                MemberElement member = structure.ElementsInStructure[memberIndex];
                carbonEmissionTotal +=
                    constants[6] * member.getMass() +
                    constants[7] * member.getMass() * 1 + // * distanceBuilding
                    constants[8] * member.getMass();
            }

            return carbonEmissionTotal;
        }
        public static double GlobalFCA(Structure structure, MaterialBank materialBank, Matrix<double> insertionMatrix)
        {
            double totalCost = 0;

            for (int stockElementIndex = 0; stockElementIndex < materialBank.ElementsInMaterialBank.Count; stockElementIndex++)
            {
                ReuseElement stockElement = materialBank.ElementsInMaterialBank[stockElementIndex];
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

                double remainingLength = materialBank.ElementsInMaterialBank[stockElementIndex].getReusableLength() - reuseLength;
                if (remainingLength < MaterialBank.minimumReusableLength)
                    wasteLength += remainingLength;

                totalCost +=
                    ObjectiveFunctions.constantsFinancialCostAnalysis[0] * stockElement.getMass(reuseLength + wasteLength);
            }

            for (int memberIndex = 0; memberIndex < structure.ElementsInStructure.Count; memberIndex++)
            {
                if (insertionMatrix.Column(memberIndex).AbsoluteMaximum() != 0) continue;

                MemberElement member = structure.ElementsInStructure[memberIndex];
                totalCost +=
                    ObjectiveFunctions.constantsFinancialCostAnalysis[1] * member.getMass() +
                    ObjectiveFunctions.constantsFinancialCostAnalysis[1] * member.getMass() * 1 + // * distanceBuilding
                    ObjectiveFunctions.constantsFinancialCostAnalysis[1] * member.getMass();
            }

            return totalCost;
        }
        public static double SimpleTestFunction(Structure structure, MaterialBank materialBank)
        {
            return 1.00 * structure.GetReusedMass() + 1.50 * structure.GetNewMass();
        }


        // Local Objectives (Member)
        public static double LengthCheck(MemberElement member, ReuseElement stockElement)
        {
            if (member.getInPlaceElementLength() < stockElement.getReusableLength()) return 1;
            else return 0;
        }
        public static double StructuralIntegrity(MemberElement member, ReuseElement stockElement, double axialForce, double momentY, double momentZ, double momentX)
        {
            double axialUtilization = stockElement.getNormalStressUtilization(axialForce); // + getBendingMomentUtilization(double momentY, double momemntZ)
            // -> shearUtilizatiuon
            // -> combinedUtilization

            double bucklingUtilization = member.getAxialBucklingUtilization(axialForce);

            if (axialUtilization > 1) return 0;
            else if (bucklingUtilization > 1) return 0;
            else return 1;
        }
        public static double LocalLCA(MemberElement member, ReuseElement stockElement, double axialForce, lcaMethod method = lcaMethod.simplified)
        {
            List<double> constants = new List<double>();
            switch (method)
            {
                case lcaMethod.simplified:
                    constants = new List<double>() { 0.21, 0.00081, 0, 0, 0, 0, 1.75, 0, 0 }; ;
                    break;
                case lcaMethod.advanced:
                    constants = new List<double>() { 0.287, 0.00081, 0.110, 0.0011, 0.0011, 0.00011, 0.957, 0.00011, 0.110 };
                    break;
                default:
                    throw new Exception("Method not found!");
            }

            double reuseLength = member.getInPlaceElementLength();
            double wasteLength = stockElement.getReusableLength() - member.getInPlaceElementLength();
            if (wasteLength < 0) return 0;

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
            if (carbonEmissionReduction < 0) return -1;
            else return carbonEmissionReduction;

        }

        


        // Combinatorial Problem
        public static double RelativeLength(MemberElement member, ReuseElement stockElement)
        {
            return member.getInPlaceElementLength() / stockElement.getReusableLength();
        }



        // Combined Objective Function (Old Method)
        public static double LocalLCAWithIntegrityAndLengthCheck(MemberElement member, ReuseElement stockElement, double axialForce, lcaMethod method = lcaMethod.simplified)
        {
            List<double> constants = new List<double>();
            switch (method)
            {
                case lcaMethod.simplified:
                    constants = new List<double>() { 0.21, 0.00081, 0, 0, 0, 0, 1.75, 0, 0 };
                    break;
                case lcaMethod.advanced:
                    constants = new List<double>() { 0.287, 0.00081, 0.110, 0.0011, 0.0011, 0.00011, 0.957, 0.00011, 0.110 };
                    break;
            }

            double reuseLength = member.getInPlaceElementLength();
            double wasteLength = stockElement.getReusableLength() - member.getInPlaceElementLength();

            if (wasteLength < 0 || stockElement.getNormalStressUtilization(axialForce) > 1 || stockElement.getAxialBucklingUtilization(axialForce, reuseLength) > 1)
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
