using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace MasterthesisGHA.Components
{
    public class ParametricTrussForLists : GH_Component
    {

        public ParametricTrussForLists()
          : base("ParametricTrussForLists", "ParametricTrussForLists",
              "Description",
              "Master", "MethodTwo")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("2D/3D", "2D/3D", "2D (false) /3D (true)", GH_ParamAccess.list, false);
            pManager.AddPointParameter("StartPoints", "StartPoints", "", GH_ParamAccess.list, new Point3d(0, 0, 0));
            pManager.AddPointParameter("EndPoints", "EndPoints", "", GH_ParamAccess.list, new Point3d(1, 0, 0));
            pManager.AddIntegerParameter("LengthDivisions", "LengthDivisions", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("Heigths", "Heights", "", GH_ParamAccess.list, 1000);
            pManager.AddNumberParameter("Widths", "Widths", "", GH_ParamAccess.list, 1000);
        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Structure Lines", "GeometryLines", "GeometryLines", GH_ParamAccess.tree);
            pManager.AddPointParameter("Structure Supports", "Supports", "Supports", GH_ParamAccess.tree);
            pManager.AddLineParameter("Load Lines", "LoadLines", "LoadLines", GH_ParamAccess.tree);
            pManager.AddIntegerParameter("Iterations", "Iterations", "", GH_ParamAccess.item);
        }

        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // INPUT
            List<bool> is3DList = new List<bool>();
            List<Point3d> startPointList = new List<Point3d>();
            List<Point3d> endPointList = new List<Point3d>();
            List<int> lengthDivisionsList = new List<int>();
            List<double> heightList = new List<double>();
            List<double> widthList = new List<double>();

            DA.GetDataList(0, is3DList);
            DA.GetDataList(1, startPointList);
            DA.GetDataList(2, endPointList);
            DA.GetDataList(3, lengthDivisionsList);
            DA.GetDataList(4, heightList);
            DA.GetDataList(5, widthList);


            int iterations = 
                is3DList.Count * 
                startPointList.Count * 
                endPointList.Count * 
                lengthDivisionsList.Count * 
                heightList.Count * 
                widthList.Count;

            if (iterations > 1e6)
            {
                throw new Exception("Max iterations of 1000 is exceeded!");
            }





            // CODE
            List<List<Line>> geometryLinesTree = new List<List<Line>>();
            List<List<Point3d>> supportPointsTree = new List<List<Point3d>>();
            List<List<Line>> loadLinesTree = new List<List<Line>>();

            int iteration = 0;
            foreach (bool is3D in is3DList)
            {
                foreach (Point3d startPoint in startPointList)
                {
                    foreach (Point3d endPoint in endPointList)
                    {
                        foreach (int lengthDivisions in lengthDivisionsList)
                        {
                            foreach (double height in heightList)
                            {
                                foreach (double width in widthList)
                                {

                                    List<Line> geometryLines = new List<Line>();
                                    List<Point3d> supportPoints = new List<Point3d>();
                                    List<Line> loadLines = new List<Line>();


                                    Line baseLine = new Line(startPoint, endPoint);
                                    List<Point3d> basePointsLeft = new List<Point3d>();
                                    List<Point3d> basePointsRight = new List<Point3d>();
                                    List<Point3d> topPoints = new List<Point3d>();

                                    if (!is3D) // 2D - Type 0
                                    {

                                        for (int i = 0; i <= lengthDivisions; i++)
                                        {
                                            basePointsLeft.Add(baseLine.PointAt(Convert.ToDouble(i) / Convert.ToDouble(lengthDivisions)));
                                            topPoints.Add(baseLine.PointAt(Convert.ToDouble(i) / Convert.ToDouble(lengthDivisions)) + new Point3d(0, 0, height));
                                        }

                                        for (int i = 0; i < basePointsLeft.Count - 1; i++)
                                        {
                                            geometryLines.Add(new Line(basePointsLeft[i], basePointsLeft[i + 1]));
                                            geometryLines.Add(new Line(topPoints[i], topPoints[i + 1]));
                                            geometryLines.Add(new Line(basePointsLeft[i], topPoints[i]));

                                            if (i % 2 == 0)
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
                                            basePointsLeft.Add(baseLine.PointAt(Convert.ToDouble(i) / Convert.ToDouble(lengthDivisions)) + new Point3d(direction) * width / 2);
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
                                                geometryLines.Add(new Line(basePointsLeft[i], basePointsRight[i + 1]));
                                            }
                                            else
                                            {
                                                geometryLines.Add(new Line(topPoints[i], basePointsLeft[i + 1]));
                                                geometryLines.Add(new Line(topPoints[i], basePointsRight[i + 1]));
                                                geometryLines.Add(new Line(basePointsLeft[i + 1], basePointsRight[i]));
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

                                    geometryLinesTree.Add(new List<Line>());
                                    supportPointsTree.Add(new List<Point3d>());
                                    loadLinesTree.Add(new List<Line>());

                                    geometryLines.ForEach(line => geometryLinesTree[iteration].Add(line));

                                    //geometryLinesTree[iteration] = geometryLines;
                                    supportPointsTree[iteration] = supportPoints;
                                    loadLinesTree[iteration] = loadLines;

                                    iteration++;
                                }
                            }
                        }
                    }
                }
            }



            // OUTPUT

            Grasshopper.DataTree<Line> geometryLinesOutTree = ElementCollection.GetOutputDataTree(geometryLinesTree);
            Grasshopper.DataTree<Point3d> supportPointsOutTree = ElementCollection.GetOutputDataTree(supportPointsTree);
            Grasshopper.DataTree<Line> loadLinesOutTree = ElementCollection.GetOutputDataTree(loadLinesTree);
           
            DA.SetDataTree(0, geometryLinesOutTree);
            DA.SetDataTree(1, supportPointsOutTree);
            DA.SetDataTree(2, loadLinesOutTree);
            DA.SetData(3, iterations);


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
            get { return new Guid("B945CAF7-6A3F-474E-8D9C-FEC3BA2913C5"); }
        }
    }
}