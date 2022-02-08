using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace MasterthesisGHA.Components._2DTrussComponents
{
    public class Elements : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Elements()
          : base("Elements with defined placement", "Elements",
              "",
              "Master", "2DTruss")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddLineParameter("Input Lines", "Lines", "Lines for creating truss geometry", GH_ParamAccess.list, new Line());          
            pManager.AddNumberParameter("Cross Section Area [mm^2]", "A", "Cross Section Area by indivial member values (list) or constant value", GH_ParamAccess.list, 10000);
            pManager.AddNumberParameter("Young's Modulus [N/mm^2]", "E", "Young's Modulus for all members", GH_ParamAccess.item, 210e3);
        }


        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Elements in geometry", "Elements", "Elements", GH_ParamAccess.list);
        }


        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
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
            get { return new Guid("0195B5A8-4929-4A2A-B2EE-1573D496CB5D"); }
        }
    }
}