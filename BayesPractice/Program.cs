using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;


using System.Threading.Tasks;

namespace BayesPractice
{

    public class Node
    {

        public bool safe;
        public bool hasPit;
        public int posX, posY;
        public double pitProb;
        public IEnumerable<Node> neighbors;
        public bool hasBreeze;
        public bool visited;
        public bool wasReached;
    }

    
    class Program
    {
        
        public static void linkNodes(List<Node> board, int boardSize)
        {
            
            for (int j = 0; j < boardSize; j++)
            {
                for (int k = 0; k < boardSize; k++)
                {
                    Node node = new Node();
                    node.posX = k;
                    node.posY = j;
                    board.Add(node);
                }
            }

            foreach (Node curNode in board) //Assign neighbor nodes to each node
            {
                curNode.neighbors = board.Where(nodes => ((nodes.posX == curNode.posX + 1 || nodes.posX == curNode.posX - 1) && (nodes.posY == curNode.posY)) || 
                    ((nodes.posY == curNode.posY + 1 || nodes.posY == curNode.posY - 1) && nodes.posX == curNode.posX));
            }
        }

        public static bool canReach(List<Node> Frontier, List<Node> KnownBreeze)
        {
            bool allReached = true;
            List<Node> breeze = new List<Node>();
            for (int i = 0; i < KnownBreeze.Count; i++)
            {
                Node newNode = new Node();
                newNode.posX = KnownBreeze[i].posX;
                newNode.posY = KnownBreeze[i].posY;
                breeze.Add(newNode);
            }
            foreach (Node node in breeze)
            {
                node.neighbors = Frontier.Where(nodes => ((nodes.posX == node.posX + 1 || nodes.posX == node.posX - 1) && (nodes.posY == node.posY)) ||
                    ((nodes.posY == node.posY + 1 || nodes.posY == node.posY - 1) && nodes.posX == node.posX));
                foreach (Node neighbor in node.neighbors)
                {
                    if (neighbor.hasPit)
                    {
                        node.wasReached = true;
                    }
                }
            }        
            foreach (Node node in breeze)
            {
                if (node.wasReached == false)
                {
                    allReached = false;
                    break;
                }
            }
            return allReached;
        }

        public static void populateBoard(List<Node> board)
        {
            Random rand = new Random();
            foreach (Node node in board)
            {
                node.hasPit = false;
                if (node.posX == 0 && node.posY == 3)
                {
                }
                else 
                {
                    if (rand.Next(5) == 0)
                    {
                        node.hasPit = true;
                    }
                }
            }
        }

        public static List<Node> calculateProb(List<Node> Frontier, List<Node> KnownBreeze, Node curPosition)
        {

            double pitProb = 0.2;
            double noPitProb = 0.8;
            int[] maxMod = new int[Frontier.Count]; //Establish the minimum maxMod values needed to establish logic table
            int[] maxVal = new int[Frontier.Count]; //Establish the minimum maxVal values needed to establish logic table
            if (Frontier.Count == 3)
            {
                Console.WriteLine();
            }



            List<Node> consideredNodes = new List<Node>();
            foreach (Node node in Frontier)
            {

                if (curPosition.posX + 1 == node.posX && curPosition.posY == node.posY)
                {
                    consideredNodes.Add(node);
                }
                else if (curPosition.posX - 1 == node.posX && curPosition.posY == node.posY)
                {
                    consideredNodes.Add(node);   
                }
                else if (curPosition.posY + 1 == node.posY && curPosition.posX == node.posX)
                {
                    consideredNodes.Add(node);
                }
                else if (curPosition.posY - 1 == node.posY && curPosition.posX == node.posX)
                {
                    consideredNodes.Add(node);
                }


            }

            

            Console.WriteLine("Considered Nodes: ");
            List<Node>[] possibleFronts = new List<Node>[(int)Math.Pow(2, Frontier.Count)];
            for (int i = 0; i < possibleFronts.Length; i++)//Setup Array of pit possibilities
            {
                List<Node> newRow = new List<Node>();
                for (int j = 0; j < Frontier.Count; j++)
                {
                    Node newNode = new Node();
                    newNode.posX = Frontier[j].posX;
                    newNode.posY = Frontier[j].posY;
                    newRow.Add(newNode);
                }
                possibleFronts[i] = newRow;
            }


            //I AM REALLY FUCKING PROUD OF THE FOLLOWING! IT TOOK A LONG TIME TO FIGURE OUT DISCRETELY

            for (int i = 0; i < Frontier.Count; i++)//Populate values for maxMod and maxVal
            {
                maxMod[i] = (int)Math.Pow(2, Frontier.Count - i);
                maxVal[i] = (int)Math.Pow(2, Frontier.Count - (i + 1));
            }
            foreach (Node node in consideredNodes)
            {
                double currentPitPositive = 0.0;
                double currentPitNegative = 0.0;
                for (int i = 0; i < (int)Math.Pow(2, Frontier.Count); i++)
                {
                    for (int j = 0; j < Frontier.Count; j++)
                    {
                        
                        if (possibleFronts[i][0].posX != node.posX && possibleFronts[i][0].posY != node.posY)//Put considered node in [0] index for possibleFronts for math reasons.
                        {
                            int consideredIndex = possibleFronts[i].FindIndex(nodes => nodes.posX == node.posX && nodes.posY == node.posY);
                            swapToFront(possibleFronts[i], consideredIndex);
                        }
                        if ((i % maxMod[j]) < maxVal[j])
                        {
                            possibleFronts[i][j].hasPit = false;
                        }
                        else
                        {
                            possibleFronts[i][j].hasPit = true;
                        }
                    }
                    double rowValue = 1.0; //Reset value for new row

                       if (canReach(possibleFronts[i], KnownBreeze))//Check if iteration of pit combinations reaches all known breezes.
                    {
                        if (possibleFronts[i][0].hasPit == true)//Calculate the probability of the iteration and add to the sum of positive pit values
                        {
                            for (int j = 1; j < possibleFronts[i].Count; j++)
                            {
                                switch (possibleFronts[i][j].hasPit)
                                {
                                    case true:
                                        rowValue = rowValue * pitProb;
                                        break;
                                    case false:
                                        rowValue = rowValue * noPitProb;
                                        break;
                                }
                            }
                            currentPitPositive += rowValue;
                        }
                        else if (possibleFronts[i][0].hasPit == false)//Calculate the probability of the iteration and add to the sum of negative pit values
                        {
                            for (int j = 1; j < possibleFronts[i].Count; j++)
                            {
                                switch (possibleFronts[i][j].hasPit)
                                {
                                    case true:
                                        rowValue = rowValue * pitProb;
                                        break;
                                    case false:
                                        rowValue = rowValue * noPitProb;
                                        break;
                                }
                            }
                            currentPitNegative += rowValue;
                        }
                        
                    }
                    
                }
                
                currentPitPositive = currentPitPositive * 0.2;
                currentPitNegative = currentPitNegative * 0.8;
                node.pitProb = (currentPitPositive / (currentPitPositive + currentPitNegative));
                Console.WriteLine("Pit ({0},{1}) Pit Probability = {2}", node.posX, node.posY, node.pitProb);

            }


            return consideredNodes;




        }

        private static void swapToFront(List<Node> list, int consideredIndex)
        {
            Node tempNode = list[0];
            list[0] = list[consideredIndex];
            list[consideredIndex] = tempNode;
        }

        static void Main(string[] args)
        {

            char input = ' ';
            int boardSize = 4;
            List<Node> board = new List<Node>();
            List<Node> frontier = new List<Node>();
            List<Node> knownBreeze = new List<Node>();
            List<Node> consideredNodes = new List<Node>();
            linkNodes(board, boardSize);
            populateBoard(board);
            Node curNode = board[board.FindIndex(nodez => nodez.posX == 0 && nodez.posY == 3)];
            curNode.visited = true;
            
            Console.WriteLine("Current Position: X: {0}   Y: {1}", curNode.posX, curNode.posY);
            while (input != 'p')
            {
                
                foreach (Node nodes in curNode.neighbors)
                {
                    if (nodes.hasPit)
                    {
                        curNode.hasBreeze = true;
                        if (!knownBreeze.Exists(breezeNode => breezeNode == curNode))
                        {
                            knownBreeze.Add(curNode);
                        }
                        foreach (Node nodez in curNode.neighbors)
                        {
                            if (!frontier.Exists(frontierNode => frontierNode == nodez) && nodez.visited == false)
                            {
                                frontier.Add(nodez);
                            }
                        }
                    }
                }
                if (curNode.hasBreeze == false)
                {
                    foreach (Node neighbor in curNode.neighbors)
                    {
                        neighbor.safe = true; //Mark adjacent nodes to safe squares as not belonging in frontier.
                    }
                }
                for (int i = 0; i < frontier.Count; i++)
                {
                    if (frontier[i].safe == true)
                    {
                        frontier.RemoveAt(i);
                    }
                }
                Console.WriteLine("Current Frontier:");
                foreach (Node node in frontier)
                {
                    Console.WriteLine("Position: X: {0}   Y: {1}", node.posX, node.posY);
                }
                Console.WriteLine("Known Breezes:");
                foreach (Node breeze in knownBreeze)
                {
                    Console.WriteLine("Position: X: {0}   Y: {1}", breeze.posX, breeze.posY);
                }

                if (curNode.hasBreeze)
                {
                    consideredNodes = calculateProb(frontier, knownBreeze, curNode);
                }
                switch (input = (char)Console.ReadKey(true).KeyChar)
                {
                    case 'a':
                        try
                        {
                            curNode = board[board.FindIndex(nodez => nodez.posX == curNode.posX - 1 && nodez.posY == curNode.posY)];
                            Console.WriteLine("Current Position: X: {0}   Y: {1}", curNode.posX, curNode.posY);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("OUT OF BOUNDS");
                            Console.WriteLine("Current Position: X: {0}   Y: {1}", curNode.posX, curNode.posY);
                        }

                        break;
                    case 'd':
                        try
                        {
                            curNode = board[board.FindIndex(nodez => nodez.posX == curNode.posX + 1 && nodez.posY == curNode.posY)];
                            Console.WriteLine("Current Position: X: {0}   Y: {1}", curNode.posX, curNode.posY);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("OUT OF BOUNDS");
                            Console.WriteLine("Current Position: X: {0}   Y: {1}", curNode.posX, curNode.posY);
                        }

                        break;
                    case 'w':
                        try
                        {
                            curNode = board[board.FindIndex(nodez => nodez.posX == curNode.posX && nodez.posY == curNode.posY - 1)];
                            Console.WriteLine("Current Position: X: {0}   Y: {1}", curNode.posX, curNode.posY);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("OUT OF BOUNDS");
                            Console.WriteLine("Current Position: X: {0}   Y: {1}", curNode.posX, curNode.posY);
                        }

                        break;
                    case 's':
                        try
                        {
                            curNode = board[board.FindIndex(nodez => nodez.posX == curNode.posX && nodez.posY == curNode.posY + 1)];
                            Console.WriteLine("Current Position: X: {0}   Y: {1}", curNode.posX, curNode.posY);
                        }
                        catch (Exception e)
                        {
                            Console.WriteLine("OUT OF BOUNDS");
                            Console.WriteLine("Current Position: X: {0}   Y: {1}", curNode.posX, curNode.posY);
                        }

                        break;
                }

            }
        }


    }
}
