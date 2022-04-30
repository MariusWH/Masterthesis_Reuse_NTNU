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
        public List<Tuple<int, int>> path;
        public Matrix<double> reducedCostMatrix;
        public double costOfPath; // cost of uncompleted path
        public double lowerBoundCost; // lower bound cost of remaining paths
        public int vertex; // reuse element
        public int level; // position insertions

        public Node RootNode(Matrix<double> costMatrix, int position, int reuseElement)
        {
            Node newNode = new Node();
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
        public Node ChildNode(Node parentNode, int position, int reuseElement)
        {
            Node newNode = new Node();

            newNode.path = parentNode.path.ToList();
            newNode.path.Add(new Tuple<int, int>(position, reuseElement));

            newNode.reducedCostMatrix = parentNode.reducedCostMatrix.Clone();
            newNode.costOfPath = parentNode.costOfPath + newNode.reducedCostMatrix[position, reuseElement];

            for (int i = 0; i < newNode.reducedCostMatrix.ColumnCount; i++)
                newNode.reducedCostMatrix[position, i] = double.PositiveInfinity; // Remove position
            for (int i = 0; i < newNode.reducedCostMatrix.RowCount; i++)
                newNode.reducedCostMatrix[i, reuseElement] = double.PositiveInfinity; // Remove reuse element (Used)

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

            for (int j = 0; j < costMatrix.ColumnCount; j++)
            {
                cost = costMatrix.Column(j).AbsoluteMinimum();
                if (cost == double.PositiveInfinity) continue;
                reductionCost += cost;
                for (int i = 0; i < costMatrix.RowCount; i++) costMatrix[i, j] -= cost;
            }

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


        public Node SolveLinear(Matrix<double> costMatrix)
        {
            List<Node> liveNodes = new List<Node>();
            
            // Initial Live Nodes
            int position = 0;
            while(liveNodes.Count == 0)
            {
                for (int reuseElement = 0; reuseElement < costMatrix.ColumnCount; reuseElement++)
                {
                    if (costMatrix[position, reuseElement] == double.PositiveInfinity) continue;
                    liveNodes.Add(RootNode(costMatrix, position, reuseElement));
                }
                if(++position == costMatrix.RowCount) return RootNode(costMatrix, 0, 0);
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

                liveNodes = childNodes;
            }

            throw new Exception("Optimal Node Not Found!");
        }
        public Node Solve(Matrix<double> costMatrix)
        {
            List<Node> liveNodes = new List<Node>();

            // Initial Live Nodes
            int position = 0;
            while (liveNodes.Count == 0)
            {
                for (int reuseElement = 0; reuseElement < costMatrix.ColumnCount; reuseElement++)
                {
                    if (costMatrix[position, reuseElement] == double.PositiveInfinity) continue;
                    liveNodes.Add(RootNode(costMatrix, position, reuseElement));
                }
                if (++position == costMatrix.RowCount) return RootNode(costMatrix, 0, 0);
            }

            // Branch From Initial Live Nodes
            while (liveNodes.Count != 0)
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

                liveNodes.AddRange(childNodes); // Non-linear
            }

            throw new Exception("Optimal Node Not Found!");
        }

    }
    
}
