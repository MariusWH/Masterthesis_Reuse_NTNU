using System;
using System.Collections.Generic;
using System.Linq;

using Grasshopper.Kernel;
using Rhino.Geometry;
using MathNet.Numerics.LinearAlgebra;

namespace MasterthesisGHA
{
    public class ObjectiveOutputLCA : GH_Component
    {

        public ObjectiveOutputLCA()
          : base("ObjectiveOutputLCA", "ObjectiveLCA",
              "Life Cycle Assessment",
              "Master", "LCA")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddGenericParameter("Structure Model", "Structure", "", GH_ParamAccess.item);
            pManager.AddGenericParameter("MaterialBank Model", "MaterialBank", "", GH_ParamAccess.item);
            pManager.AddMatrixParameter("InsertionMatrix", "InsertionMatrix", "", GH_ParamAccess.item);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("Carbon Equivalents", "Carbon", "", GH_ParamAccess.item);
        }


        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // Input
            Structure structure = new TrussModel3D();
            MaterialBank materialBank = new MaterialBank();
            Matrix insertionMatrix = new Matrix(0,0);

            DA.GetData(0, ref structure);
            DA.GetData(1, ref materialBank);
            DA.GetData(2, ref insertionMatrix);


            // Code
            double carbon = LCA.GlobalObjectiveFunctionLCA(structure, materialBank, ElementCollection.RhinoToMathnetMatrix(insertionMatrix));

            // Output
            DA.SetData(0, carbon);

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
        public override Guid ComponentGuid
        {
            get { return new Guid("23361999-C86C-4549-BF08-78E95CFC2EC5"); }
        }
    }
}