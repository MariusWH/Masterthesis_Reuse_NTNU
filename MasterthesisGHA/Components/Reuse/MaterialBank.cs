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
          : base("Material Bank", "MaterialBank",
              "Description",
              "Master", "Reuse")
        {
        }
        protected override void RegisterInputParams(GH_InputParamManager pManager)
        {            
            pManager.AddTextParameter("Section", "SectionName", "SectionName", GH_ParamAccess.list, "IPE200");
            pManager.AddIntegerParameter("Amount", "Amount", "Amount", GH_ParamAccess.list, 0);
            pManager.AddNumberParameter("Length", "Length", "Length", GH_ParamAccess.list, 1000);           
            pManager.AddNumberParameter("Fabrication Distance", "Fabrication", "Transport distance for fabrication used in LCA", GH_ParamAccess.list, 100);
            pManager.AddNumberParameter("Building Distance", "Building", "Transport distance for building used in LCA", GH_ParamAccess.list, 100);
            pManager.AddNumberParameter("Recycling Distance", "Recycling", "Transport distance for recycling used in LCA", GH_ParamAccess.list, 100);
            pManager.AddTextParameter("CommandInput", "Command", "Input as: Amount x SectionName x Length (ex.: 10xIPE200x1000)", GH_ParamAccess.list, "0xIPE200x1000");
            pManager.AddBooleanParameter("VisualsMethod", "Visuals", "Group Visuals by lengths (true) or cross section (false)", GH_ParamAccess.item, true);
        }
        protected override void RegisterOutputParams(GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("MaterialBank", "MaterialBank", "MaterialBank", GH_ParamAccess.item);
            pManager.AddTextParameter("Info", "Info", "Info", GH_ParamAccess.item);
            pManager.AddBrepParameter("StockVisuals", "StockVisuals", "StockVisuals", GH_ParamAccess.list);
            pManager.AddColourParameter("StockColour", "StockColour", "StockColour", GH_ParamAccess.list);
            pManager.AddNumberParameter("Mass", "Material Bank Mass", "Initial mass of all elements in Material Bank", GH_ParamAccess.item);
        }

        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            // INPUTS
            List<string> profiles = new List<string>();
            List<int> quantities = new List<int>();
            List<double> lengths = new List<double>();
            List<double> fabricationDistances = new List<double>();
            List<double> buildingDistances = new List<double>();
            List<double> recyclingDistances = new List<double>();
            List<string> inputCommands = new List<string>();
            bool groupMethod = true;

            DA.GetDataList(0, profiles);
            DA.GetDataList(1, quantities);
            DA.GetDataList(2, lengths);
            DA.GetDataList(3, fabricationDistances);
            DA.GetDataList(4, buildingDistances);
            DA.GetDataList(5, recyclingDistances);
            DA.GetDataList(6, inputCommands);
            DA.GetData(7, ref groupMethod);

            // CODE
            MaterialBank materialBankA = new MaterialBank(profiles, quantities, lengths, 
                fabricationDistances, buildingDistances, recyclingDistances);
            MaterialBank materialBankB = new MaterialBank(inputCommands);
            MaterialBank materialBank = materialBankA + materialBankB;

            if(groupMethod == true)
                materialBank.UpdateVisualsMaterialBank(0);
            else
                materialBank.UpdateVisualsMaterialBank(1);

            // OUTPUTS
            DA.SetData(0, materialBank);
            DA.SetData(1, materialBank.GetMaterialBankInfo());
            DA.SetDataList(2, materialBank.MaterialBankVisuals);
            DA.SetDataList(3, materialBank.MaterialBankColors);
            DA.SetData(4, materialBank.GetMaterialBankMass());
        }

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return Properties.Resources.MaterialBank1;
            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("EC30A7CE-C83A-4A4D-AA49-3D5B7AFF7A00"); }
        }
    }
}