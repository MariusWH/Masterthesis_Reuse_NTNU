using System;
using System.Collections.Generic;
using System.Linq;
using Grasshopper.Kernel;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;

namespace MasterthesisGHA.Components.MethodOne
{
    public class AllReuseMethodsPreAnalyzed : GH_Component
    {
        // Stored Variables


        public AllReuseMethodsPreAnalyzed()
          : base("All Reuse Methods Pre Analyzed", "ReusePreAnalyzed",
              "",
              "Master", "Reuse")
        {
        }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("2D/3D", "2D/3D", "2D (false) /3D (true)", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("InserMaterialBank", "InsertMB", "Insert Material Bank (true)", GH_ParamAccess.item, false);
            pManager.AddBooleanParameter("InserNewElements", "InsertNew", "Insert New Elements (true)", GH_ParamAccess.item, false);
            pManager.AddIntegerParameter("Method", "Method", "", GH_ParamAccess.item, 0);
            pManager.AddGenericParameter("MaterialBank", "MB", "", GH_ParamAccess.item);

            pManager.AddGenericParameter("Analyzed Structure", "Structure", "", GH_ParamAccess.item);

            pManager.AddBooleanParameter("CutMaterialBank", "CutMB", "", GH_ParamAccess.item, false);
            pManager.AddIntegerParameter("Search Iterations", "Iterations", "", GH_ParamAccess.item, 100);
            pManager.AddBooleanParameter("Cutting", "Cutting", "", GH_ParamAccess.item, true);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddTextParameter("Info", "Info", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("TotalMass", "TotalMass", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("ReusedMass", "ReusedMass", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("NewMass", "NewMass", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("Structure Data", "Structure", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("MaterialBank Data", "MaterialBank", "", GH_ParamAccess.item);
            pManager.AddMatrixParameter("Insertion Matrix", "IM", "", GH_ParamAccess.item);
            pManager.AddTextParameter("Results CSV", "Results", "", GH_ParamAccess.item);
            pManager.AddTextParameter("Full Search CSV", "Full Search", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("LCA", "LCA", "", GH_ParamAccess.item);
            pManager.AddNumberParameter("FCA", "FCA", "", GH_ParamAccess.item);
            pManager.AddTextParameter("Utilization", "", "", GH_ParamAccess.item);
            pManager.AddTextParameter("Profiles", "", "", GH_ParamAccess.tree);
            pManager.AddBrepParameter("DirectGeometry", "Geometry", "", GH_ParamAccess.list);
            pManager.AddColourParameter("DirectColors", "Colors", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("Area", "Area", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("Length", "Length", "", GH_ParamAccess.list);
        }


        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // INPUTS
            bool is3D = false;
            bool insertMaterialBank = false;
            bool insertNewElements = false;
            int optimizationMethod = 0;
            MaterialBank iMaterialBank = new MaterialBank();

            Structure structure = new SpatialTruss();

            bool cutMB = false;
            int maxSearchIterations = 0;
            bool cutting = true;

            DA.GetData(0, ref is3D);
            DA.GetData(1, ref insertMaterialBank);
            DA.GetData(2, ref insertNewElements);
            DA.GetData(3, ref optimizationMethod);
            DA.GetData(4, ref iMaterialBank);

            DA.GetData(5, ref structure);

            DA.GetData(6, ref cutMB);
            DA.GetData(7, ref maxSearchIterations);
            DA.GetData(8, ref cutting);

            // CODE
            //truss.Solve();
            //truss.Retracking();

            Matrix<double> insertionMatrix = Matrix<double>.Build.Sparse(structure.ElementsInStructure.Count, iMaterialBank.ElementsInMaterialBank.Count);
            MaterialBank inputMaterialBank = iMaterialBank.GetDeepCopy();
            MaterialBank outMaterialBank = iMaterialBank.GetDeepCopy();
            optimizationMethod = optimizationMethod % 4;
            string outputInfo = "";
            string resultCSV = "";
            string fullSearchCSV = "";
            double resultLCA = 0;
            double resultFCA = 0;

            if (insertNewElements)
            {
                structure.InsertNewElements();
                outputInfo += "Structure is optimized with new elements.\n";
            }
            if (insertMaterialBank)
            {
                outputInfo += "Material Bank is inserted with ";
                switch (optimizationMethod)
                {

                    case 0: // No Optimization 
                        structure.InsertMaterialBank(inputMaterialBank, out insertionMatrix, false, cutting);
                        outputInfo += "no optimization.\n";
                        break;

                    case 1: // Priority Matrix Optimization
                        structure.InsertMaterialBankByPriorityMatrix(out insertionMatrix, inputMaterialBank, out _, cutting); ;
                        outputInfo += "priority matrix optimization.\n";
                        break;

                    case 2: // Brute Force Optimization
                        structure.InsertMaterialBankByRandomPermutations(out insertionMatrix, maxSearchIterations, inputMaterialBank, out resultCSV, out fullSearchCSV, cutting);
                        outputInfo += "with " + maxSearchIterations.ToString() + " pseudo random permutations optimization.\n";
                        break;

                    case 3: // Branch and Bound Optimization
                        structure.InsertMaterialBankByBNB(inputMaterialBank, out insertionMatrix, maxSearchIterations, out resultCSV);
                        outputInfo += "insertion matrix method with branch and bound optimization.\n";
                        break;
                }
            }

            if (cutMB) structure.InsertReuseElementsFromInsertionMatrix(insertionMatrix, inputMaterialBank, out outMaterialBank);
            else outMaterialBank.InsertionMatrix = insertionMatrix;
            resultLCA = ObjectiveFunctions.GlobalLCA(structure, inputMaterialBank, insertionMatrix, ObjectiveFunctions.lcaMethod.simplified);
            resultFCA = ObjectiveFunctions.GlobalFCA(structure, inputMaterialBank, insertionMatrix, ObjectiveFunctions.bound.upperBound);

            structure.Solve();
            structure.Retracking();

            outputInfo += "\n\n" + structure.PrintProfilesInStructure() + "\n\n" + structure.PrintStructureInfo() + "\n\n" + outMaterialBank.GetMaterialBankInfo();

            string utilization = "";

            for (int i = 0; i < structure.ElementsInStructure.Count; i++)
                utilization += structure.ElementsInStructure[i].getTotalUtilization(structure.ElementAxialForcesX[i], structure.ElementMomentsY[i], structure.ElementMomentsZ[i], structure.ElementShearForcesY[i], structure.ElementShearForcesZ[i], structure.ElementTorsionsX[i]).ToString() + "\n";

            // Visuals
            inputMaterialBank.GetVisualsInsertionMatrix(out List<Brep> geometry, out List<System.Drawing.Color> colors, out _, insertionMatrix, 4);

            List<double> memberAreas = new List<double>();
            List<double> memberLengths = new List<double>();
            foreach (MemberElement member in structure.ElementsInStructure)
            {
                memberAreas.Add( member.CrossSectionArea );
                memberLengths.Add(member.getInPlaceElementLength());
            }

            // OUTPUTS
            DA.SetData(0, outputInfo);
            DA.SetData(1, structure.GetTotalMass());
            DA.SetData(2, structure.GetReusedMass());
            DA.SetData(3, structure.GetNewMass());
            DA.SetData(4, structure);
            DA.SetData(5, outMaterialBank);
            DA.SetData(6, ElementCollection.MathnetToRhinoMatrix(insertionMatrix));
            DA.SetData(7, resultCSV);
            DA.SetData(8, fullSearchCSV);
            DA.SetData(9, resultLCA);
            DA.SetData(10, resultFCA);
            DA.SetData(11, utilization);
            DA.SetDataTree(12, structure.PrintRangeOfMaterialBanksThree());
            DA.SetDataList(13, geometry);
            DA.SetDataList(14, colors);
            DA.SetDataList(15, memberAreas);
            DA.SetDataList(16, memberLengths);
        }
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return null;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("CF7521C9-B2CD-4779-B867-722604659DB1"); }
        }
    }
}