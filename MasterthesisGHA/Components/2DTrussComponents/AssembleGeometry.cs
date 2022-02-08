using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace MasterthesisGHA.Components
{
    public class AssembleGeometry : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public AssembleGeometry()
          : base("Assemble Model Geometry", "Model Geometry",
              "",
              "Master", "2DTruss")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Input Elements", "Elements", "Lines for creating truss geometry", GH_ParamAccess.list);
            pManager.AddPointParameter("Input Support Points", "Supports", "Support points restricted from translation but free to rotate", GH_ParamAccess.list);
            pManager.AddNumberParameter("Nodal Loading [N]", "Load", "Nodal loads by numeric values (x1, y1, x2, y2, ..)", GH_ParamAccess.list, new List<double> { 0 });
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {

        }


        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // INPUTS

            List<Line> iLines = new List<Line>();
            List<Point3d> iAnchoredPoints = new List<Point3d>();
            List<double> iA = new List<double>();
            double iE = 0;

            DA.GetDataList(0, iLines);
            DA.GetDataList(1, iAnchoredPoints);
            DA.GetDataList(2, iA);
            DA.GetData(3, ref iE);



            // CODE







            // OUTPUTS












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
            get { return new Guid("2EED3182-EC79-4207-9D7C-58952E502A44"); }
        }
    }
}