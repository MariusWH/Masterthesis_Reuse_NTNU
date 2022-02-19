using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using System.IO;
using System.Reflection;

namespace MasterthesisGHA
{
    public class MaterialBankComponent : GH_Component
    {
        public MaterialBankComponent()
          : base("Reusable Stock of Elements", "MaterialBank",
              "Description",
              "Master", "MethodOne")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {            
            pManager.AddTextParameter("Section", "SectionName", "SectionName", GH_ParamAccess.list, "IPE200");
            pManager.AddIntegerParameter("Amount", "Amount", "Amount", GH_ParamAccess.list, 0);
            pManager.AddNumberParameter("Length", "Length", "Length", GH_ParamAccess.list, 1000);
            pManager.AddTextParameter("CommandInput", "Command", "Input as: Amount x SectionName x Length (ex.: 10xIPE200x1000)",
                GH_ParamAccess.list, "0xIPE200x1000");
        }

 
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("MaterialBank", "MaterialBank", "MaterialBank", GH_ParamAccess.item);
            pManager.AddTextParameter("Info", "Info", "Info", GH_ParamAccess.item);
            pManager.AddBrepParameter("StockVisuals", "StockVisuals", "StockVisuals", GH_ParamAccess.list);
            pManager.AddColourParameter("StockColour", "StockColour", "StockColour", GH_ParamAccess.list);
        }


        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // INPUTS
            List<string> profiles = new List<string>();
            List<int> quantities = new List<int>();
            List<double> lengths = new List<double>();
            List<string> inputCommands = new List<string>();

            DA.GetDataList(0, profiles);
            DA.GetDataList(1, quantities);
            DA.GetDataList(2, lengths);
            DA.GetDataList(3, inputCommands);



            // CODE
            if (quantities.Count != lengths.Count || quantities.Count != profiles.Count)
                throw new Exception("Profiles, Quantities and Lengths lists needs to be the same length!");

            MaterialBank materialBank = new MaterialBank();
            //List<StockElement> materialBank = new List<StockElement>();           

            for (int i = 0; i < quantities.Count; i++)
            {
                for (int j = 0; j < quantities[i]; j++)
                    materialBank.InsertStockElementIntoMaterialBank(new StockElement(profiles[i], lengths[i]));
            }
           

            foreach ( string command in inputCommands )
            {
                string[] commandArray = command.Split('x');
                int commandQuantities = Convert.ToInt32(commandArray[0]);
                string commandProfiles = commandArray[1];
                double commandLengths = Convert.ToDouble(commandArray[2]);

                for (int i = 0; i < commandQuantities; i++)
                    materialBank.InsertStockElementIntoMaterialBank(new StockElement(commandProfiles, commandLengths));                       
            }


            





            // OUTPUTS
            List<System.Drawing.Color> colors = new List<System.Drawing.Color>();

            DA.SetData(0, materialBank);
            DA.SetData(1, materialBank.GetMaterialBankInfo());
            DA.SetDataList(2, materialBank.VisualizeMaterialBank(0, out colors));
            DA.SetDataList(3, colors);






        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.MaterialBank;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("EC30A7CE-C83A-4A4D-AA49-3D5B7AFF7A00"); }
        }
    }
}