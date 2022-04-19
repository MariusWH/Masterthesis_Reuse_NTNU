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
        public double cost; // lower bound
        public int vertex; // reuse element
        public int level; // position insertions

        public Node RootNode(Matrix<double> costMatrix, int position, int reuseElement)
        {
            Node newNode = new Node();

            newNode.path = new List<Tuple<int, int>>();
            newNode.path.Add(new Tuple<int, int>(position, reuseElement));

            newNode.reducedCostMatrix = costMatrix.Clone();
            newNode.cost = 0;
            newNode.cost += newNode.reducedCostMatrix[position, reuseElement];

            for (int i = 0; i < costMatrix.ColumnCount; i++)
                newNode.reducedCostMatrix[position, i] = double.PositiveInfinity; // Remove position
            for (int i = 0; i < costMatrix.RowCount; i++)
                newNode.reducedCostMatrix[i, reuseElement] = double.PositiveInfinity; // Remove reuse element (Used)

            newNode.reducedCostMatrix = costMatrixReduction(newNode.reducedCostMatrix, out double reductionCost);
            newNode.cost += reductionCost;

            newNode.vertex = reuseElement;
            newNode.level = position;

            return newNode;
        }
        public Node ChildNode(Node parentNode, int position, int reuseElement)
        {
            Node newNode = new Node();

            newNode.path = parentNode.path.ToList();
            newNode.path.Add(new Tuple<int, int>(position, reuseElement));

            newNode.reducedCostMatrix = parentNode.reducedCostMatrix.Clone();
            newNode.cost = parentNode.cost;
            newNode.cost += newNode.reducedCostMatrix[position, reuseElement];

            for (int i = 0; i < newNode.reducedCostMatrix.ColumnCount; i++)
                newNode.reducedCostMatrix[position, i] = double.PositiveInfinity; // Remove position
            for (int i = 0; i < newNode.reducedCostMatrix.RowCount; i++)
                newNode.reducedCostMatrix[i, reuseElement] = double.PositiveInfinity; // Remove reuse element (Used)

            newNode.reducedCostMatrix = costMatrixReduction(newNode.reducedCostMatrix, out double reductionCost);
            newNode.cost += reductionCost;

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
                reductionCost += cost;
                for (int j = 0; j < costMatrix.ColumnCount; j++) if (costMatrix[i, j] != double.PositiveInfinity) costMatrix[i, j] -= cost;
            }

            for (int j = 0; j < costMatrix.ColumnCount; j++)
            {
                cost = costMatrix.Column(j).AbsoluteMinimum();
                reductionCost += cost;
                for (int i = 0; i < costMatrix.RowCount; i++) if (costMatrix[i, j] != double.PositiveInfinity) costMatrix[i, j] -= cost;
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


        public Node Solve(Matrix<double> costMatrix)
        {
            Queue<Node> liveNodes = new Queue<Node>();

            // Initial Live Nodes
            int position = 0;
            while(liveNodes.Count == 0)
            {
                for (int reuseElement = 0; reuseElement < costMatrix.ColumnCount; reuseElement++)
                {
                    if (costMatrix[position, reuseElement] == double.PositiveInfinity) continue;
                    List<Tuple<int, int>> path = new List<Tuple<int, int>>();
                    path.Add(new Tuple<int, int>(position, reuseElement));
                    liveNodes.Enqueue(RootNode(costMatrix, position, reuseElement));
                }
                if(++position == costMatrix.RowCount) return RootNode(costMatrix, 0, 0);
            }
            
            // Branch From Initial Live Nodes
            while(liveNodes.Count != 0)
            {
                Node liveNode = liveNodes.Dequeue();
                if (liveNodes.Count > 1000)
                {
                    return liveNodes.OrderBy(o => o.cost).First();
                }

                position = liveNode.path[liveNode.path.Count - 1].Item1 + 1;
                bool noChildNodes = true;
                while (noChildNodes)
                {
                    for (int reuseElement = 0; reuseElement < costMatrix.ColumnCount; reuseElement++)
                    {
                        if (costMatrix[position, reuseElement] == double.PositiveInfinity) continue;
                        else noChildNodes = false;

                        List<Tuple<int, int>> path = new List<Tuple<int, int>>();
                        path.Add(new Tuple<int, int>(position, reuseElement));
                        liveNodes.Enqueue(ChildNode(liveNode, position, reuseElement));
                    }

                    if (++position == liveNode.reducedCostMatrix.RowCount - 1) return liveNode;
                }   
            }

            throw new Exception("Optimal Node Not Found!");
        }


    }
    
}
