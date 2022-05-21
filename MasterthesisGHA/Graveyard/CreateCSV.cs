/*using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace MasterthesisGHA.Components.ParametricOptimization
{
    public class CreateCSV : GH_Component
    {
        public CreateCSV()
          : base("Create CSV Dataset", "CSV Dataset",
              "Creates CSV Dataset of Structural Analysis Results",
              "Master", "ML")
        {
        }

        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {
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
            /*
            pManager.AddPointParameter("Free Nodes", "Nodes", "Free nodes as list of points", GH_ParamAccess.list);
            pManager.AddMatrixParameter("Stiffness Matrix", "K", "Stiffness matrix as matrix", GH_ParamAccess.item);
            pManager.AddMatrixParameter("Displacement Vector", "r", "Displacement vector as matrix", GH_ParamAccess.item);
            pManager.AddMatrixParameter("Load Vector", "R", "Load vector as matrix", GH_ParamAccess.item);
            pManager.AddNumberParameter("Axial Forces", "N", "Member axial forces as list of values", GH_ParamAccess.list);
            pManager.AddGenericParameter("Model Data", "Model", "", GH_ParamAccess.list);

            pManager.AddNumberParameter("Heights", "", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("Widths", "", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("Lengths", "", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("Displacements", "", "", GH_ParamAccess.list);
            pManager.AddNumberParameter("Masses", "", "", GH_ParamAccess.list);

            pManager.AddTextParameter("csv", "", "", GH_ParamAccess.item);
            

            pManager.AddIntegerParameter("# of structures", "#", "", GH_ParamAccess.item);
            pManager.AddTextParameter("csv", "csv", "", GH_ParamAccess.item);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // INPUT

            // Truss Geometries
            bool is3D = true;
            List<Point3d> startPointList = new List<Point3d>();
            List<Point3d> endPointList = new List<Point3d>();
            List<int> lengthDivisionsList = new List<int>();
            List<double> heightList = new List<double>();
            List<double> widthList = new List<double>();
            DA.GetData(0, ref is3D);
            DA.GetDataList(1, startPointList);
            DA.GetDataList(2, endPointList);
            DA.GetDataList(3, lengthDivisionsList);
            DA.GetDataList(4, heightList);
            DA.GetDataList(5, widthList);

            // Loading and Profiles
            string iProfile = "";
            double iLineLoadValue = 0;
            Vector3d iLineLoadDirection = new Vector3d();
            Vector3d iLineLoadDistribution = new Vector3d();
            bool iApplySelfWeight = true;
            DA.GetData(6, ref iProfile);
            DA.GetData(7, ref iLineLoadValue);
            DA.GetData(8, ref iLineLoadDirection);
            DA.GetData(9, ref iLineLoadDistribution);
            DA.GetData(10, ref iApplySelfWeight);



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

            List<List<Line>> geometryLinesTree = new List<List<Line>>();
            List<List<Point3d>> supportPointsTree = new List<List<Point3d>>();
            List<List<Line>> loadLinesTree = new List<List<Line>>();


            string csv = "";
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


                                /*
                                geometryLinesTree.Add(new List<Line>());
                                supportPointsTree.Add(new List<Point3d>());
                                loadLinesTree.Add(new List<Line>());

                                geometryLines.ForEach(line => geometryLinesTree[iteration].Add(line));
                                supportPoints.ForEach(pt => supportPointsTree[iteration].Add(pt));
                                loadLines.ForEach(line => loadLinesTree[iteration].Add(line));
                                */

                                /*
                                switch (is3D)
                                {
                                    default:
                                        trusses.Add(new SpatialTruss(geometryLines, iProfiles, supportPoints));
                                        break;
                                    case false:
                                        trusses.Add(new PlanarTruss(geometryLines, iProfiles, supportPoints));
                                        break;
                                }



                                // Line Load
                                trusses[trusses.Count - 1].ApplyLineLoad(iLineLoadValue, iLineLoadDirection, iLineLoadDistribution, loadLines);

                                // Self Weigth
                                if (iApplySelfWeight)
                                {
                                    trusses[trusses.Count - 1].ApplySelfWeight();
                                }

                                trusses[trusses.Count - 1].Solve();
                                trusses[trusses.Count - 1].Retracking();



                                // Write Results to CSV
                                double trussHeight = height;
                                double trussWidth = width;
                                double trussLength = startPoint.DistanceTo(endPoint);
                                double maxDisplacement = trusses[trusses.Count - 1].GetMaxDisplacement();
                                double LCA = ObjectiveFunctions.GlobalLCA(trusses[trusses.Count - 1]);

                                csv += trussHeight.ToString() + "," +
                                    trussWidth.ToString() + "," +
                                    trussLength.ToString() + "," +
                                    maxDisplacement.ToString() + "," +
                                    LCA.ToString() + "\n";
                                

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
                                double trussHeight = height;
                                double trussWidth = width;
                                double trussLength = startPoint.DistanceTo(endPoint);
                                double maxDisplacement = truss.GetMaxDisplacement();
                                double LCA = ObjectiveFunctions.GlobalLCA(truss);

                                csv += trussHeight.ToString() + "," +
                                    trussWidth.ToString() + "," +
                                    trussLength.ToString() + "," +
                                    maxDisplacement.ToString() + "," +
                                    LCA.ToString() + "\n";

                                // DO SHIT HERE


                                /*
                                 * 
                                 * 
                                 * 
                                 * 
                                 */

                                /*
                                trussHeights.Add(trussHeight);
                                trussWidths.Add(trussWidth);
                                trussLengths.Add(trussLength);
                                maxDisplacements.Add(maxDisplacement);
                                totalMasses.Add(totalMass);
                                


                            }
                        }
                    }
                }
            }


            // OUTPUT
            /*
            DA.SetDataList(0, trusses[0].FreeNodes);
            DA.SetData(1, trusses[0].GetStiffnessMatrix());
            DA.SetData(2, trusses[0].GetDisplacementVector());
            DA.SetData(3, trusses[0].GetLoadVector());
            DA.SetDataList(4, trusses[0].ElementAxialForcesX);
            DA.SetDataList(5, trusses);
            DA.SetDataList(6, trussHeights);
            DA.SetDataList(7, trussWidths);
            DA.SetDataList(8, trussLengths);
            DA.SetDataList(9, maxDisplacements);
            DA.SetDataList(10, totalMasses);
            DA.SetData(11, csv);
            

            DA.SetData(0, numberOfStructures);
            DA.SetData(1, csv);




            /*
            // Grasshopper Trees to Nested Lists
            IList<List<Grasshopper.Kernel.Types.GH_Line>> iLinesNested = iLinesTree.Branches;
            IList<List<Grasshopper.Kernel.Types.GH_String>> iProfilesNested = iProfilesTree.Branches;
            IList<List<Grasshopper.Kernel.Types.GH_Point>> iSupportsNested = iSupportsTree.Branches;
            IList<List<Grasshopper.Kernel.Types.GH_Number>> iLoadNested = iLoadTree.Branches;
            IList<List<Grasshopper.Kernel.Types.GH_Vector>> iLoadVecsNested = iLoadVecsTree.Branches;
            IList<List<Grasshopper.Kernel.Types.GH_Line>> iLinesToLoadNested = iLinesToLoadTree.Branches;

            TrussModel3D truss;
            for (int i = 0; i < iLinesNested.Count; i++)
            {
                List<Grasshopper.Kernel.Types.GH_Line> iLines = iLinesNested[i];
                List<Grasshopper.Kernel.Types.GH_String> iProfiles = iProfilesNested[i];
                List<Grasshopper.Kernel.Types.GH_Point> iSupports = iSupportsNested[i];
                List<Grasshopper.Kernel.Types.GH_Number> iLoad = iLoadNested[i];
                List<Grasshopper.Kernel.Types.GH_Vector> iLoadVecs = iLoadVecsNested[i];
                List<Grasshopper.Kernel.Types.GH_Line> iLinesToLoad = iLinesToLoadNested[i];
    
            }
            






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
            get { return new Guid("CE78D6BD-0F1B-4DE2-AA2D-D38BAFAB84A2"); }
        }
    }
}*/