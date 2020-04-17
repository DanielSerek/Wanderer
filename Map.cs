using System;

namespace Wanderer
{
    public class Map
    {

        public enum TileType
        {
            Wall,
            Floor,
            Flooded
        }

        public TileType[,] GameMap = null;
        public int MapSize;
        private Drawer drawer;
        // Holds information how many floor tiles were created, is needed for floodFill method 
        private int floorCount; 

        public Map(Drawer drawer, int mapSize)
        {
            this.drawer = drawer;
            MapSize = mapSize;
            // Generate the virtual GameMap
            GameMap = new TileType[MapSize, MapSize];
        }

        // Creates a game map
        public void CreateMap(int wallsPercentage)
        {
            int chf = 0; // Provides information how many tiles have been flooded
            // Generate new maps until the number of flooded tiles floor tiles
            do
            {
                floorCount = 0;
                floorCount = GenerateRandomMap(wallsPercentage);
                Position pos = RandomFreeCell();
                FloodFill(pos);
                chf = CheckFloodFill();
            } while (chf != floorCount);
            ChangeFloodedToFloor();
            PrintMap();
        }

        // If the map doesn't pass floodfill method, a new map is generated
        int GenerateRandomMap(int wallsPercentage)
        {
            Random random = new Random();
            int randomNumber;

            for (int i = 0; i < MapSize; i++)
            {
                for (int j = 0; j < MapSize; j++)
                {
                    randomNumber = random.Next(1, 100);
                    if (randomNumber <= wallsPercentage)
                    {
                        GameMap[i, j] = TileType.Wall;
                    }
                    else
                    {
                        GameMap[i, j] = TileType.Floor;
                        floorCount++;
                    }
                }
            }
            return floorCount;
        }

        // Resolves boundary conditions (not to get outside of the array) and returns a type of a tile
        public TileType GetTile(int x, int y)
        {
            if (x < 0 || x >= MapSize ||
                y < 0 || y >= MapSize) return TileType.Wall;
            return GameMap[x, y];
        }

        public void PrintMap()
        {
            for (int i = 0; i < MapSize; i++)
            {
                for (int j = 0; j < MapSize; j++)
                {
                    if (GameMap[i, j] == TileType.Wall) drawer.DrawMapImage(Drawer.ImgType.Wall, new Position(i,j));
                    else drawer.DrawMapImage(Drawer.ImgType.Floor, new Position(i,j));
                }
            }
        }

        // Change of flooded tiles back to floor tiles
        void ChangeFloodedToFloor()
        {
            for (int i = 0; i < MapSize; i++)
            {
                for (int j = 0; j < MapSize; j++)
                {
                    if (GameMap[i, j] == TileType.Flooded)
                        GameMap[i, j] = TileType.Floor;
                }
            }
        }


        // Count how many tiles have been flooded
        int CheckFloodFill()
        {
            int count = 0;
            foreach (var tile in GameMap)
            {
                if (tile == TileType.Flooded) count++;
            }
            return count;
        }

        void FloodFill(Position pos)
        {
            // Base cases 
            if (pos.X < 0 || pos.X >= MapSize ||
                pos.Y < 0 || pos.Y >= MapSize)
                return;
            if (GameMap[pos.X, pos.Y] == TileType.Flooded || GameMap[pos.X, pos.Y] == TileType.Wall)
                return;

            // Replace the tile type at (x, y) 
            GameMap[pos.X, pos.Y] = TileType.Flooded;

            // Recur for north, east, south and west 
            FloodFill(new Position(pos.X + 1, pos.Y));
            FloodFill(new Position(pos.X - 1, pos.Y));
            FloodFill(new Position(pos.X, pos.Y + 1));
            FloodFill(new Position(pos.X, pos.Y - 1));
        }
             
        //Generate a random free cell 
        public Position RandomFreeCell()
        {
            int i, j;
            Random random = new Random();
            do
            {
                i = random.Next(1, MapSize);
                j = random.Next(1, MapSize);
            } while (GetTile(i, j) != TileType.Floor);
            return new Position(i, j);
        }

        //private void IterateThroughMap(MapIter handleCell)
        //{
        //    for (int i = 0; i < Size.X; i++)
        //    {
        //        for (int j = 0; j < Size.Y; j++)
        //        {
        //            Point pos = new Point(i, j);
        //            handleCell(pos);
        //        }
        //    }
        //}

        //IterateThroughMap((pos) => {
        //    if (rand.Next() % 100 < wallProbability) SetTile(pos, TileType.Wall);
        //    else SetTile(pos, TileType.Floor);
        //    if (!CheckMapContinuity()) SetTile(pos, TileType.Floor);
        //});
    }
}
