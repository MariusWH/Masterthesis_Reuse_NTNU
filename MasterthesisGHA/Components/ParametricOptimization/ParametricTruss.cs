using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace MasterthesisGHA.Components
{
    public class ParametricTruss : GH_Component
    {

        public ParametricTruss()
          : base("ParametricTruss", "ParametricTruss",
              "Description",
              "Master", "MethodTwo")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("2D/3D", "2D/3D", "2D (false) /3D (true)", GH_ParamAccess.item, false);
            pManager.AddPointParameter("StartPoint", "StartPoint", "StartPoint", GH_ParamAccess.item, new Point3d(0, 0, 0));
            pManager.AddPointParameter("EndPoint", "EndPoint", "EndPoint", GH_ParamAccess.item, new Point3d(1, 0, 0));
            pManager.AddIntegerParameter("LengthDivisions", "LengthDivisions", "LengthDivisions", GH_ParamAccess.item);
            pManager.AddNumberParameter("Heigth", "Height", "Height", GH_ParamAccess.item, 1000);
            pManager.AddNumberParameter("Width", "Width", "Width", GH_ParamAccess.item, 1000);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
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
            Point3d startPoint = new Point3d();
            Point3d endPoint = new Point3d();
            int lengthDivisions = 0;
            double height = 0;
            double width = 0;

            DA.GetData(0, ref is3D);
            DA.GetData(1, ref startPoint);
            DA.GetData(2, ref endPoint);
            DA.GetData(3, ref lengthDivisions);
            DA.GetData(4, ref height);
            DA.GetData(5, ref width);



            // CODE
            List<Line> geometryLines = new List<Line>();
            List<Point3d> supportPoints = new List<Point3d>();
            List<Line> loadLines = new List<Line>();


            // Type 0
            // if type 0

            Line baseLine = new Line(startPoint, endPoint);
            List<Point3d> basePointsLeft = new List<Point3d>();
            List<Point3d> basePointsRight = new List<Point3d>();
            List<Point3d> topPoints = new List<Point3d>();

            if (!is3D) // 2D - Type 0
            {

                for (int i = 0; i <= lengthDivisions; i++)
                {
                    basePointsLeft.Add(baseLine.PointAt(Convert.ToDouble(i) / Convert.ToDouble(lengthDivisions)));
                    topPoints.Add(baseLine.PointAt(Convert.ToDouble(i) / Convert.ToDouble(lengthDivisions)) + new Point3d(0,0,height));
                }
            
                for (int i = 0; i < basePointsLeft.Count-1; i++)
                {
                    geometryLines.Add(new Line(basePointsLeft[i], basePointsLeft[i + 1]));
                    geometryLines.Add(new Line(topPoints[i], topPoints[i+1]));
                    geometryLines.Add(new Line(basePointsLeft[i], topPoints[i]));

                    if ( i % 2 == 0 )
                    {
                        geometryLines.Add(new Line(basePointsLeft[i], topPoints[i + 1]));
                    }                 
                    else
                    {
                        geometryLines.Add(new Line(topPoints[i], basePointsLeft[i + 1]));
                    }                        

                    loadLines.Add(new Line(topPoints[i], topPoints[i + 1]));
                }

                geometryLines.Add(new Line(basePointsLeft[lengthDivisions], topPoints[lengthDivisions]));
                supportPoints.Add(basePointsLeft[0]);
                supportPoints.Add(basePointsLeft[lengthDivisions]);
            }



            else // 3D - Type 0
            {
                Vector3d direction = baseLine.Direction;
                direction.Unitize();
                direction.Rotate(Math.PI / 2, new Vector3d(0, 0, 1));

                for (int i = 0; i <= lengthDivisions; i++)
                {
                    basePointsLeft.Add(baseLine.PointAt(Convert.ToDouble(i) / Convert.ToDouble(lengthDivisions)) + new Point3d(direction) * width/2 );
                    basePointsRight.Add(baseLine.PointAt(Convert.ToDouble(i) / Convert.ToDouble(lengthDivisions)) + new Point3d(direction) * -width / 2);
                    topPoints.Add(baseLine.PointAt(Convert.ToDouble(i) / Convert.ToDouble(lengthDivisions)) + new Point3d(0, 0, height));
                }

                for (int i = 0; i < basePointsLeft.Count - 1; i++)
                {
                    geometryLines.Add(new Line(basePointsLeft[i], basePointsLeft[i + 1]));
                    geometryLines.Add(new Line(basePointsRight[i], basePointsRight[i + 1]));
                    geometryLines.Add(new Line(topPoints[i], topPoints[i + 1]));

                    geometryLines.Add(new Line(basePointsLeft[i], topPoints[i]));
                    geometryLines.Add(new Line(basePointsRight[i], topPoints[i]));
                    geometryLines.Add(new Line(basePointsLeft[i], basePointsRight[i]));

                    if (i % 2 == 0)
                    {
                        geometryLines.Add(new Line(basePointsLeft[i], topPoints[i + 1]));
                        geometryLines.Add(new Line(basePointsRight[i], topPoints[i + 1]));
                        geometryLines.Add(new Line(basePointsLeft[i], basePointsRight[i+1]));
                    }
                    else
                    {
                        geometryLines.Add(new Line(topPoints[i], basePointsLeft[i + 1]));
                        geometryLines.Add(new Line(topPoints[i], basePointsRight[i + 1]));
                        geometryLines.Add(new Line(basePointsLeft[i+1], basePointsRight[i]));
                    }
                    
                    loadLines.Add(new Line(topPoints[i], topPoints[i + 1]));
                }

                geometryLines.Add(new Line(basePointsLeft[lengthDivisions], topPoints[lengthDivisions]));
                geometryLines.Add(new Line(basePointsRight[lengthDivisions], topPoints[lengthDivisions]));

                supportPoints.Add(basePointsLeft[0]);
                supportPoints.Add(basePointsRight[0]);
                supportPoints.Add(basePointsLeft[lengthDivisions]);
                supportPoints.Add(basePointsRight[lengthDivisions]);


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
            get { return new Guid("DFE3835D-56F3-4DEA-B9F1-D253E798801B"); }
        }
    }
}