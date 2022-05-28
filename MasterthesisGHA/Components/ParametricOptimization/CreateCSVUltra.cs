using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using System.IO;
using System.Text;

namespace MasterthesisGHA.Components.ParametricOptimization
{
    public class CreateCSVUltra : GH_Component
    {
        public CreateCSVUltra()
          : base("csvUltra", "CSV ULTRA",
              "Creates CSV Dataset of Structural Analysis Results",
              "Master", "ML")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Run", "Run", "", GH_ParamAccess.item, false);
            pManager.AddTextParameter("Filepath", "Filepath", "", GH_ParamAccess.item);
            pManager.AddBooleanParameter("2D/3D", "2D/3D", "2D (false) /3D (true)", GH_ParamAccess.item, false);
            pManager.AddPointParameter("StartPoints", "StartPoints", "", GH_ParamAccess.list, new Point3d(0, 0, 0));
            pManager.AddPointParameter("EndPoints", "EndPoints", "", GH_ParamAccess.list, new Point3d(1, 0, 0));
            pManager.AddIntegerParameter("LengthDivisions", "LengthDivisions", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("Heigths", "Heights", "", GH_ParamAccess.list, 1000);
            pManager.AddNumberParameter("Widths", "Widths", "", GH_ParamAccess.list, 1000);
            pManager.AddTextParameter("Bar Profiles", "Profiles", "Profile of each geometry line member as list", GH_ParamAccess.item);
            pManager.AddNumberParameter("Line Load Value", "LL Value", "", GH_ParamAccess.item, 0);
            pManager.AddVectorParameter("Line Load Direction", "LL Direction", "", GH_ParamAccess.item, new Vector3d(0, 0, -1));
            pManager.AddVectorParameter("Line Load Distribution Direction", "LL Distribution Direction", "", GH_ParamAccess.item, new Vector3d(1, 0, 0));
            pManager.AddBooleanParameter("Apply Self Weight", "Self Weigth", "", GH_ParamAccess.item, false);

        }

        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddIntegerParameter("# of structures", "#", "", GH_ParamAccess.item);
            pManager.AddTextParameter("csv", "csv", "", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // INPUT

            // Run
            bool run = false;
            string filepath = "";
            DA.GetData(0, ref run);
            DA.GetData(1, ref filepath);

            // Truss Geometries
            bool is3D = true;
            List<Point3d> startPointList = new List<Point3d>();
            List<Point3d> endPointList = new List<Point3d>();
            List<int> lengthDivisionsList = new List<int>();
            List<double> heightList = new List<double>();
            List<double> widthList = new List<double>();
            
            DA.GetData(2, ref is3D);
            DA.GetDataList(3, startPointList);
            DA.GetDataList(4, endPointList);
            DA.GetDataList(5, lengthDivisionsList);
            DA.GetDataList(6, heightList);
            DA.GetDataList(7, widthList);

            // Loading and Profiles
            string iProfile = "";
            double iLineLoadValue = 0;
            Vector3d iLineLoadDirection = new Vector3d();
            Vector3d iLineLoadDistribution = new Vector3d();
            bool iApplySelfWeight = true;
            DA.GetData(8, ref iProfile);
            DA.GetData(9, ref iLineLoadValue);
            DA.GetData(10, ref iLineLoadDirection);
            DA.GetData(11, ref iLineLoadDistribution);
            DA.GetData(12, ref iApplySelfWeight);



            // CODE

            // Parametric Truss Lists
            int numberOfStructures =
                startPointList.Count *
                endPointList.Count *
                lengthDivisionsList.Count *
                heightList.Count *
                widthList.Count;


            if (numberOfStructures > 1e7)
            {
                throw new Exception("Max iterations of 1000 is exceeded!");
            }

            FileStream fs = File.OpenWrite(filepath);


            string csv = "";
            if (run)
            {
                List<List<Line>> geometryLinesTree = new List<List<Line>>();
                List<List<Point3d>> supportPointsTree = new List<List<Point3d>>();
                List<List<Line>> loadLinesTree = new List<List<Line>>();


                
                List<string> iProfiles = new List<string> { iProfile };
                List<Structure> trusses = new List<Structure>();

                List<double> trussHeights = new List<double>();
                List<double> trussWidths = new List<double>();
                List<double> trussLengths = new List<double>();
                List<double> maxDisplacements = new List<double>();
                List<double> totalMasses = new List<double>();

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


                                    

                                    Structure truss;
                                    switch (is3D)
                                    {
                                        default:
                                            truss = new SpatialTruss(geometryLines, iProfiles, supportPoints);
                                            break;
                                        case false:
                                            truss = new PlanarTruss(geometryLines, iProfiles, supportPoints);
                                            break;
                                    }

                                    truss.ApplyLineLoad(iLineLoadValue, iLineLoadDirection, iLineLoadDistribution, loadLines);
                                    if (iApplySelfWeight) truss.ApplySelfWeight();
                                    truss.Solve();
                                    truss.Retracking();

                                    // Write Results to CSV
                                    double trussLength = startPoint.DistanceTo(endPoint);
                                    double maxDisplacement = truss.GetMaxDisplacement();
                                    double LCA = ObjectiveFunctions.GlobalLCA(truss);

                                    csv = height.ToString() + "," +
                                        width.ToString() + "," +
                                        trussLength.ToString() + "," +
                                        lengthDivisions.ToString() + "," +
                                        maxDisplacement.ToString() + "," +
                                        LCA.ToString() + "\n";

                                    
                                    byte[] bytes = Encoding.UTF8.GetBytes(csv);
                                    fs.Write(bytes, 0, bytes.Length);


                                }
                            }
                        }
                    }
                }
            }

            fs.Close();

            // OUTPUT
            DA.SetData(0, numberOfStructures);
            DA.SetData(1, csv);

        }
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.csvUltra;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("7D0E7AAF-0522-4025-8E95-D9695A80C35B"); }
        }
    }
}