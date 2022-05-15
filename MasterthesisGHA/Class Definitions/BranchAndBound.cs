using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace MasterthesisGHA
{
    class Node
    {
        public Structure structure;
        public MaterialBank materialBank;

        public List<Tuple<int, int>> path;
        public Matrix<double> reducedCostMatrix;
        public double costOfPath; // cost of uncompleted path
        public double lowerBoundCost; // lower bound cost of remaining paths
        public int vertex; // reuse element
        public int level; // position insertions

        public Node RootNode(Matrix<double> costMatrix, int position, int reuseElement, Structure structure, MaterialBank materialBank)
        {
            Node newNode = new Node();
            newNode.structure = structure;
            newNode.materialBank = materialBank;
            newNode.costOfPath = costMatrix[position, reuseElement]; // Root node cost;
            newNode.lowerBoundCost = 0;
            newNode.vertex = reuseElement;
            newNode.level = position;
            newNode.path = new List<Tuple<int, int>>();
            newNode.path.Add(new Tuple<int, int>(position, reuseElement));
            newNode.reducedCostMatrix = costMatrix.Clone();

            for (int j = 0; j < costMatrix.ColumnCount; j++)
                newNode.reducedCostMatrix[position, j] = double.PositiveInfinity; // Remove position
            for (int i = 0; i < costMatrix.RowCount; i++)
                newNode.reducedCostMatrix[i, reuseElement] = double.PositiveInfinity; // Remove reuse element (Used)

            newNode.reducedCostMatrix = costMatrixReduction(newNode.reducedCostMatrix, out double reductionCost);
            newNode.lowerBoundCost += reductionCost; // Root reduction cost

            return newNode;
        }
        public Node ChildNode(Node parentNode, int position, int reuseElement, bool keepCutOff = true)
        {
            Node newNode = new Node();

            newNode.structure = parentNode.structure;
            newNode.materialBank = parentNode.materialBank;

            newNode.path = parentNode.path.ToList();
            newNode.path.Add(new Tuple<int, int>(position, reuseElement));

            newNode.reducedCostMatrix = parentNode.reducedCostMatrix.Clone();
            newNode.costOfPath = parentNode.costOfPath + newNode.reducedCostMatrix[position, reuseElement];

            switch (keepCutOff)
            {
                case true:
                    for (int i = 0; i < newNode.reducedCostMatrix.ColumnCount; i++)
                        newNode.reducedCostMatrix[position, i] = double.PositiveInfinity; // Remove position
                    double usedLength = 0;
                    for (int i = 0; i < parentNode.path.Count; i++)
                        if (parentNode.path[i].Item2 == reuseElement) 
                            usedLength += parentNode.structure.ElementsInStructure[parentNode.path[i].Item1].getInPlaceElementLength();
                    for (int i = 0; i < newNode.reducedCostMatrix.RowCount; i++)
                    {
                        if (newNode.structure.ElementsInStructure[i].getInPlaceElementLength() > newNode.materialBank.ElementsInMaterialBank[reuseElement].getReusableLength() - usedLength
                            && newNode.reducedCostMatrix[i, reuseElement] != double.PositiveInfinity)
                        {
                            newNode.reducedCostMatrix[i, reuseElement] =
                                ObjectiveFunctions.wasteCostReduction(newNode.materialBank.ElementsInMaterialBank[reuseElement],
                                newNode.structure.ElementsInStructure[i].getInPlaceElementLength()); // Update reuse element
                        }
                        else
                        {
                            newNode.reducedCostMatrix[i, reuseElement] = double.PositiveInfinity;
                        }
                    }
                    break;

                case false:
                    for (int i = 0; i < newNode.reducedCostMatrix.ColumnCount; i++)
                        newNode.reducedCostMatrix[position, i] = double.PositiveInfinity; // Remove position
                    for (int i = 0; i < newNode.reducedCostMatrix.RowCount; i++)
                        newNode.reducedCostMatrix[i, reuseElement] = double.PositiveInfinity; // Remove reuse element (Used)
                    break;
            }


            newNode.reducedCostMatrix = costMatrixReduction(newNode.reducedCostMatrix, out double reductionCost);
            newNode.lowerBoundCost = reductionCost;

            newNode.vertex = reuseElement;
            newNode.level = position;

            return newNode;
        }
        public Matrix<double> costMatrixReduction(Matrix<double> costMatrix, out double reductionCost)
        {
            reductionCost = 0;
            double cost;

            for (int i = 0; i < costMatrix.RowCount; i++)
            {
                cost = costMatrix.Row(i).AbsoluteMinimum();
                if (cost == double.PositiveInfinity) continue;
                reductionCost += cost;
                for (int j = 0; j < costMatrix.ColumnCount; j++) costMatrix[i, j] -= cost;
            }

            /*
            for (int j = 0; j < costMatrix.ColumnCount; j++)
            {
                cost = costMatrix.Column(j).AbsoluteMinimum();
                if (cost == double.PositiveInfinity) continue;
                reductionCost += cost;
                for (int i = 0; i < costMatrix.RowCount; i++) costMatrix[i, j] -= cost;
            }
            */
            return costMatrix;
        }
        public Matrix<double> getCostMatrix(Matrix<double> priorityMatrix)
        {
            double optimalValue = priorityMatrix.Enumerate().Max();
            Matrix<double> costMatrix = Matrix<double>.Build.Sparse(priorityMatrix.RowCount, priorityMatrix.ColumnCount);

            for (int row = 0; row < priorityMatrix.RowCount; row++)
                for (int col = 0; col < priorityMatrix.ColumnCount; col++)
                {
                    if (priorityMatrix[row, col] == 0) costMatrix[row, col] = double.PositiveInfinity;
                    else costMatrix[row, col] = optimalValue - priorityMatrix[row, col];
                }

            return costMatrix;
        }

        public enum bnbMethod { linearSearch, fullSearch }
        public Node Solve(Matrix<double> costMatrix, out string resultLog, Structure structure, MaterialBank materialBank, int maxIterations = 1000, bnbMethod method = bnbMethod.fullSearch)
        {
            resultLog = "";

            switch (method)
            {
                case bnbMethod.linearSearch: return SolveLinearSearch(costMatrix, structure, materialBank);
                case bnbMethod.fullSearch: return SolveFullSearch(costMatrix, structure, materialBank, out resultLog, maxIterations);
            }
            throw new Exception("BnB search method not found");
                
        }
        public Node SolveLinearSearch(Matrix<double> costMatrix, Structure structure, MaterialBank materialBank)
        {
            List<Node> liveNodes = new List<Node>();
            
            // Initial Live Nodes
            int position = 0;
            while(liveNodes.Count == 0)
            {
                for (int reuseElement = 0; reuseElement < costMatrix.ColumnCount; reuseElement++)
                {
                    if (costMatrix[position, reuseElement] == double.PositiveInfinity) continue;
                    liveNodes.Add(RootNode(costMatrix, position, reuseElement, structure, materialBank));
                }
                if(++position == costMatrix.RowCount) return RootNode(costMatrix, 0, 0, structure, materialBank);
            }
            
            // Branch From Initial Live Nodes
            while(liveNodes.Count != 0)
            {
                liveNodes = liveNodes.OrderBy(o => o.lowerBoundCost + o.costOfPath).ToList();
                Node minimumCostNode = liveNodes[0];
                liveNodes.RemoveAt(0);

                if (liveNodes.Count > 5000) return minimumCostNode;

                position = minimumCostNode.path[minimumCostNode.path.Count - 1].Item1 + 1;
                bool noChildNodes = true;
                List<Node> childNodes = new List<Node>();
                while (noChildNodes)
                {
                    for (int reuseElement = 0; reuseElement < costMatrix.ColumnCount; reuseElement++)
                    {
                        Node child = new Node();
                        if (minimumCostNode.reducedCostMatrix[position, reuseElement] == double.PositiveInfinity) continue;
                        else
                        {
                            noChildNodes = false;
                            childNodes.Add(ChildNode(minimumCostNode, position, reuseElement));
                        }
                    }

                    if (++position == minimumCostNode.reducedCostMatrix.RowCount - 1) 
                        return minimumCostNode;
                }

                liveNodes = childNodes; // Linear (Delete all parent e-nodes)
            }

            throw new Exception("Optimal Node Not Found!");
        }
        public Node SolveFullSearch(Matrix<double> costMatrix, Structure structure, MaterialBank materialBank, out string fullSearchLog, int maxIterations = 1000, int maxLiveNodes = 5000, bool printLog = true)
        {
            fullSearchLog = "";
            int i = 0;

            List<Node> liveNodes = new List<Node>();
            Node bestLeafNode = new Node();
            Node minimumCostNode = new Node();
            bool firstLeafNode = true;

            // Initial Live Nodes 
            int position = 0;
            while (liveNodes.Count == 0)
            {
                for (int reuseElement = 0; reuseElement < costMatrix.ColumnCount; reuseElement++)
                {
                    if (costMatrix[position, reuseElement] == double.PositiveInfinity) continue;
                    liveNodes.Add(RootNode(costMatrix, position, reuseElement, structure, materialBank));
                }
                if (++position == costMatrix.RowCount) return RootNode(costMatrix, 0, 0, structure, materialBank);
            }

            // Branch From Initial Live Nodes
            while (liveNodes.Count != 0)
            {
                liveNodes = liveNodes.OrderBy(o => o.lowerBoundCost + o.costOfPath).ToList();
                minimumCostNode = liveNodes[0];
                liveNodes.RemoveAt(0);                
                

                if (printLog == true)
                {
                    List<Tuple<int, int>> solutionPath = minimumCostNode.path;
                    Matrix<double> insertionMatrix = Matrix<double>.Build.Sparse(costMatrix.RowCount, costMatrix.ColumnCount);
                    foreach (Tuple<int, int> insertion in solutionPath)
                        insertionMatrix[insertion.Item1, insertion.Item2] = structure.ElementsInStructure[insertion.Item1].getInPlaceElementLength();
                    
                    fullSearchLog +=
                        i++.ToString() + "," +
                        ObjectiveFunctions.GlobalLCA(structure, materialBank, insertionMatrix, ObjectiveFunctions.lcaMethod.simplified).ToString() + "," +
                        ObjectiveFunctions.GlobalFCA(structure, materialBank, insertionMatrix, ObjectiveFunctions.bound.upperBound).ToString() + '\n';
                }

                if (i > maxIterations)
                {
                    if (!firstLeafNode) return bestLeafNode;
                    else return minimumCostNode;
                }

                //if (liveNodes.Count > maxLiveNodes)
                //    return minimumCostNode;

                position = minimumCostNode.path[minimumCostNode.path.Count - 1].Item1 + 1;

                bool noChildNodes = true;
                List<Node> childNodes = new List<Node>();
                while (noChildNodes)
                {
                    for (int reuseElement = 0; reuseElement < costMatrix.ColumnCount; reuseElement++)
                    {
                        Node child = ChildNode(minimumCostNode, position, reuseElement);
                        if (minimumCostNode.reducedCostMatrix[position, reuseElement] == double.PositiveInfinity) continue;
                        else
                        {
                            noChildNodes = false;
                            childNodes.Add(child);
                        }

                        if (position + 1 == minimumCostNode.reducedCostMatrix.RowCount - 1)
                        {
                            if (firstLeafNode)
                            {
                                firstLeafNode = false;
                                bestLeafNode = child;
                            }
                            else if (bestLeafNode.costOfPath > child.costOfPath) bestLeafNode = child;
                            //return minimumCostNode;
                        }
                    }

                    /*
                    position++;
                    for (int reuseElement = 0; reuseElement < costMatrix.ColumnCount; reuseElement++)
                    {
                        if (minimumCostNode.reducedCostMatrix[position-1, reuseElement] != double.PositiveInfinity)
                        {
                            position--;
                            break;
                        }     
                    }*/

                    if (++position == costMatrix.RowCount) break;     
                }
                if (position == costMatrix.RowCount) break;

                // Narrowed search
                int searchDepth = 1;
                childNodes = childNodes.OrderBy(o => o.lowerBoundCost + o.costOfPath).ToList();
                for (int j = 0; j < searchDepth; j++) liveNodes.Add(childNodes[j]);

                // Full Search
                //liveNodes.AddRange(childNodes); 
            }

            if (!firstLeafNode) return bestLeafNode;
            else return minimumCostNode;

        }
        
        public Node SolveFullSearchWithCutting(Matrix<double> costMatrix, int maxLiveNodes = 5000)
        {
            List<Node> liveNodes = new List<Node>();

            // Initial Live Nodes 
            int position = 0;
            while (liveNodes.Count == 0)
            {
                for (int reuseElement = 0; reuseElement < costMatrix.ColumnCount; reuseElement++)
                {
                    if (costMatrix[position, reuseElement] == double.PositiveInfinity) continue;
                    liveNodes.Add(RootNode(costMatrix, position, reuseElement, structure, materialBank));
                }
                if (++position == costMatrix.RowCount) return RootNode(costMatrix, 0, 0, structure, materialBank);
            }

            // Branch From Initial Live Nodes
            while (liveNodes.Count != 0)
            {
                liveNodes = liveNodes.OrderBy(o => o.lowerBoundCost + o.costOfPath).ToList();
                Node minimumCostNode = liveNodes[0];
                liveNodes.RemoveAt(0);

                if (liveNodes.Count > maxLiveNodes)
                    return minimumCostNode;

                position = minimumCostNode.path[minimumCostNode.path.Count - 1].Item1 + 1;
                bool noChildNodes = true;
                List<Node> childNodes = new List<Node>();
                while (noChildNodes)
                {
                    for (int reuseElement = 0; reuseElement < costMatrix.ColumnCount; reuseElement++)
                    {
                        Node child = new Node();
                        if (minimumCostNode.reducedCostMatrix[position, reuseElement] == double.PositiveInfinity) continue;
                        else
                        {
                            noChildNodes = false;
                            childNodes.Add(ChildNode(minimumCostNode, position, reuseElement));
                        }
                    }

                    if (++position == minimumCostNode.reducedCostMatrix.RowCount - 1)
                        return minimumCostNode;
                }

                liveNodes.AddRange(childNodes); // Non-linear
            }

            throw new Exception("Optimal Node Not Found!");
        }

    }

    

}
