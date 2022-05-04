using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace MasterthesisGHA.Components
{
    public class PerformanceTruss : GH_Component
    {

        public PerformanceTruss()
          : base("PerformanceTruss", "PerformanceTruss",
              "Description",
              "Master", "MethodTwo")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("2D/3D", "2D/3D", "2D (false) /3D (true)", GH_ParamAccess.item, false);
            pManager.AddNumberParameter("xSize", "xSize", "xSize", GH_ParamAccess.list);
            pManager.AddNumberParameter("ySize", "ySize", "ySize", GH_ParamAccess.list);
            pManager.AddNumberParameter("zSize", "zSize", "zSize", GH_ParamAccess.list);
            pManager.AddIntegerParameter("xDivisions", "xDivisions", "xDivisions", GH_ParamAccess.item);
            pManager.AddIntegerParameter("yDivisions", "yDivisions", "yDivisions", GH_ParamAccess.item);
            pManager.AddIntegerParameter("zDivisions", "zDivisions", "zDivisions", GH_ParamAccess.item);

        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Structure Lines", "GeometryLines", "GeometryLines", GH_ParamAccess.list);
            pManager.AddPointParameter("Structure Supports", "Supports", "Supports", GH_ParamAccess.list);
            pManager.AddLineParameter("Load Lines", "LoadLines", "LoadLines", GH_ParamAccess.list);
        }

        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // INPUT
            bool is3D = false;

            double xSize = 0;
            double ySize = 0;
            double zSize = 0;

            int xDiv = 0;
            int yDiv = 0;
            int zDiv = 0;

            DA.GetData(0, ref is3D);
            DA.GetData(1, ref xSize);
            DA.GetData(2, ref ySize);
            DA.GetData(3, ref zSize);
            DA.GetData(4, ref xDiv);
            DA.GetData(5, ref yDiv);
            DA.GetData(6, ref zDiv);

            

            // CODE
            List<Line> geometryLines = new List<Line>();
            List<Point3d> supportPoints = new List<Point3d>();
            List<Line> loadLines = new List<Line>();


            double xAdd = xSize / xDiv;
            double yAdd = ySize / yDiv;
            double zAdd = zSize / zDiv;

            if (!is3D) // 2D - Type 0
            {
                for (double z = 0; z < zDiv; z += zAdd)
                {
                    for (double y = 0; y < yDiv; y += yAdd)
                    {
                        for (double x = 0; x < xDiv; x += xAdd)
                        {

                        }
                    }
                }
            }



            else // 3D - Type 0
            {
                

            }





            // OUTPUT
            DA.SetDataList(0, geometryLines);
            DA.SetDataList(1, supportPoints);
            DA.SetDataList(2, loadLines);
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
            get { return new Guid("1F71FB72-0FA0-4AB8-B95B-601C9E7624E0"); }
        }
    }
}