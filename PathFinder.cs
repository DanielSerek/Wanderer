using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using static Wanderer.Map;

namespace Wanderer
{
    /*
     * The A* algorithm - the explanation can be found here:
     * https://gigi.nullneuron.net/gigilabs/a-pathfinding-example-in-c/
     * https://www.youtube.com/watch?v=-L-WgKMFuhE&ab_channel=SebastianLague
    */
    public class PathFinder
    {
        private List<Position> positions;
        private int X;
        private int Y;
        private int F;  // G + H
        private int G;  //The G score is the distance from the starting point
        private int H;  //H score is the estimated distance from the destination (calculated as the city block distance) 
        private PathFinder Parent;

        // The pathfinding is used by enemies (currentPos) to find the shortest path to the player
        public List<Position> PathFinding(Position currentPos, Position playerPos, Map map)
        {
            PathFinder current = null;
            PathFinder start = new PathFinder { X = currentPos.X, Y = currentPos.Y };
            PathFinder target = new PathFinder { X = playerPos.X, Y = playerPos.Y };
            List<PathFinder> openList = new List<PathFinder>();
            List<PathFinder> closedList = new List<PathFinder>();
            int g = 0; // number of steps needed to find the player

            // start by adding the original position to the open list
            openList.Add(start);

            while (openList.Count > 0)
            {
                // get the square with the lowest F score
                current = openList.First(l => l.F == openList.Min(x => x.F));

                // add the current square to the closed list
                closedList.Add(current);

                // remove it from the open list
                openList.Remove(current);

                // if we added the destination to the closed list, we've found a path
                if (closedList.FirstOrDefault(l => l.X == playerPos.X && l.Y == playerPos.Y) != null) break;
                
                List<PathFinder> adjacentSquares = GetWalkableAdjacentSquares(current.X, current.Y, map);
                g++;

                foreach (var adjacentSquare in adjacentSquares)
                {
                    // if this adjacent square is already in the closed list, ignore it
                    if (closedList.FirstOrDefault(l => l.IsEqual(adjacentSquare)) != null) continue;

                    // if it's not in the open list...
                    if (openList.FirstOrDefault(l => l.IsEqual(adjacentSquare)) == null)
                    {
                        // compute its score, set the parent
                        adjacentSquare.G = g;
                        adjacentSquare.H = ComputeHScore(adjacentSquare.X,
                            adjacentSquare.Y, target.X, target.Y);
                        adjacentSquare.F = adjacentSquare.G + adjacentSquare.H;
                        adjacentSquare.Parent = current;

                        // and add it to the open list
                        openList.Insert(0, adjacentSquare);
                    }
                    else
                    {
                        // If it is in the open list, test if using the current G score makes the adjacent square's F score
                        // lower, if yes update the parent because it means it's a better path
                        if (g + adjacentSquare.H < adjacentSquare.F)
                        {
                            adjacentSquare.G = g;
                            adjacentSquare.F = adjacentSquare.G + adjacentSquare.H;
                            adjacentSquare.Parent = current;
                        }
                    }
                }
            }

            PathFinder currentTile;
            List<PathFinder> ReversedSteps = new List<PathFinder>();
            currentTile = closedList[closedList.Count - 1];
            ReversedSteps.Add(currentTile);

            // Put all the parents in the list to find the route
            while (!(currentTile.X == currentPos.X && currentTile.Y == currentPos.Y)) 
            {
                currentTile = currentTile.Parent;
                ReversedSteps.Add(currentTile.Parent);
            } 
            ReversedSteps.Reverse();

            // Transform Pathfinder positions into a simple list of positions
            positions = new List<Position>();
            foreach (var item in ReversedSteps)
            {
                if(item != null) positions.Add(new Position(item.X, item.Y));
            }
            positions.RemoveAt(0);
            return positions;
        }

        static int ComputeHScore(int x, int y, int targetX, int targetY)
        {
            return Math.Abs(targetX - x) + Math.Abs(targetY - y);
        }

        static List<PathFinder> GetWalkableAdjacentSquares(int x, int y, Map map)
        {
        
            List<PathFinder> proposedLocations = new List<PathFinder>();
            if (y > 0) proposedLocations.Add(new PathFinder() { X = x, Y = y - 1});
            if (x > 0) proposedLocations.Add(new PathFinder() { X = x - 1, Y = y});
            if (y < map.MapSize) proposedLocations.Add(new PathFinder() { X = x, Y = y + 1});
            if (x < map.MapSize) proposedLocations.Add(new PathFinder() { X = x + 1, Y = y});

            return proposedLocations.Where(l => map.GetTile(new Position(l.X, l.Y)) == TileType.Floor).ToList();
        }
        private bool IsEqual(PathFinder p)
        {
            if (this == null) return false;
            return (X == p.X && Y == p.Y);
        }
    }
}
