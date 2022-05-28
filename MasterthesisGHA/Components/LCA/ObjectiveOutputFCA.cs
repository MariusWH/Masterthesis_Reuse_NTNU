using System;
using System.Collections.Generic;
using System.Linq;

using Grasshopper.Kernel;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;

namespace MasterthesisGHA
{
    public class ObjectiveOutputFCA : GH_Component
    {

        public ObjectiveOutputFCA()
          : base("ObjectiveOutputFCA", "ObjectiveFCA",
              "Financial Cost Analysis",
              "Master", "Objectives")
        {
        }


        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Structure Model", "Structure", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("MaterialBank Model", "MaterialBank", "", GH_ParamAccess.item);
            pManager.AddMatrixParameter("InsertionMatrix", "InsertionMatrix", "", GH_ParamAccess.item);
        }


        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Cost [NOK]", "Cost", "", GH_ParamAccess.item);
        }


        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Input
            Structure structure = new SpatialTruss();
            MaterialBank materialBank = new MaterialBank();
            Matrix insertionMatrix = new Matrix(0, 0);

            DA.GetData(0, ref structure);
            DA.GetData(1, ref materialBank);
            DA.GetData(2, ref insertionMatrix);

            // Code
            double carbon = ObjectiveFunctions.GlobalFCA(structure, materialBank, ElementCollection.RhinoToMathnetMatrix(insertionMatrix));

            // Output
            DA.SetData(0, carbon);
        }


        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.FCA;
            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("15895ED3-CF1A-46D8-88FF-120E507ED22A"); }
        }
    }
}