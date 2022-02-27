using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace MasterthesisGHA.Components
{
    public class LCA : GH_Component
    {
        public LCA()
          : base("LCA", "LCA",
              "Life Cycle Assessment",
              "Master", "LCA")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Distance Fabrication", "Df", "Distance to fabrication site in km", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Distance Building", "Db", "Distance to building site in km", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Distance Recycling", "Dr", "Distance to recycling site in km", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Mass of Element Stock", "Stock", "Mass of deconstructed element stock", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Mass of Reusable Elements", "Reusable elements", "Mass of reusable elements to be used in final assembly", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Mass of New Elements", "New Elements", "Mass of new elements to be used to final assembly", GH_ParamAccess.item, 0);
            pManager.AddNumberParameter("Mass of waste", "Waste", "Mass of scrap-off waste related to manufacturing", GH_ParamAccess.item, 0);

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddNumberParameter("GHG Elements", "GHG Elements", "kg CO2 equivalent for Elements", GH_ParamAccess.item);
            pManager.AddNumberParameter("GHG Transport", "GHG Transport", "kg CO2 equivalent for Transport", GH_ParamAccess.item);
            pManager.AddNumberParameter("LCA", "LCA", "Total kg CO2 equivalent for the Life Cycle analysis", GH_ParamAccess.item);
            pManager.AddNumberParameter("LCA New Elements", "LCA", "Total kg CO2 equivalent for the Life Cycle analysis if the elements are new", GH_ParamAccess.item);
        }

        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            double Df = 0;
            double Db = 0;
            double Dr = 0;
            double GHGElements = 0; //GHG emissions related to disassembly and preparation of reusable elements
            double GHGTransport = 0; //GHG emissions related to transport of elements
            double LCA = 0; //Total kg CO2 equivalent in the LCA calculation
            double Mstock = 100; //Mass of stock
            double Mreuse = 50; //Mass of reusable elements
            double Mnew = 50; // Mass of new elements
            double Mwaste = 0; //Mass of cut-off scrap
            double LCAnew = 0; // Total kg CO2 equivalent in LCA calculation under the assumption that all elements used are new

            //Inputs
            DA.GetData(0, ref Df);
            DA.GetData(1, ref Db);
            DA.GetData(2, ref Dr);
            DA.GetData(3, ref Mstock);
            DA.GetData(4, ref Mreuse);
            DA.GetData(5, ref Mnew);
            DA.GetData(6, ref Mwaste);


            //Code
            GHGElements = 0.287 * Mstock + 0.81 * Mwaste + 0.110 * (Mreuse);
            GHGTransport = 0.00011 * Mstock * Df + 0.00011 * (Mreuse) * Db + 0.00011 * Mwaste * Dr;
            LCA = GHGElements + GHGTransport;

            LCAnew = 0.957 * Mnew + 0.00011 * Mnew * Db + 0.110 * Mnew;


            //Outputs
            DA.SetData(0, GHGElements);
            DA.SetData(1, GHGTransport);
            DA.SetData(2, LCA);
            DA.SetData(3, LCAnew);
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
            get { return new Guid("9CE7CCCD-87FF-4F2E-96AA-B231E7556198"); }
        }
    }
}