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
        // A delegate to be used in the iteration through the map
        private delegate void MapIter(Position pos);
        // Holds information how many floor tiles were created, is needed for floodFill method 
        private int floorCount; 

        public Map(Drawer drawer, int mapSize)
        {
            this.drawer = drawer;
            MapSize = mapSize;
            // Generate the virtual GameMap
            GameMap = new TileType[MapSize, MapSize];
        }

        private void IterateThroughMap(MapIter handleCell)
        {
            for (int i = 0; i < MapSize; i++)
            {
                for (int j = 0; j < MapSize; j++)
                {
                    Position pos = new Position(i, j);
                    handleCell(pos);
                }
            }
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
            IterateThroughMap((pos) => 
            {
                randomNumber = random.Next(1, 100);
                if (randomNumber <= wallsPercentage) SetTile(pos, TileType.Wall);
                else
                {
                    SetTile(pos, TileType.Floor);
                    floorCount++;
                }
            });
            return floorCount;
        }

        public void SetTile(Position pos, TileType type)
        {
            GameMap[pos.X, pos.Y] = type;
        }


        // Resolves boundary conditions (not to get outside of the array) and returns a type of a tile
        public TileType GetTile(Position pos)
        {
            if (pos.X < 0 || pos.X >= MapSize ||
                pos.Y < 0 || pos.Y >= MapSize) return TileType.Wall;
            return GameMap[pos.X, pos.Y];
        }

        public void PrintMap()
        {
            IterateThroughMap((pos) =>
            {
                if (GetTile(pos) == TileType.Wall) drawer.DrawMapImage(Drawer.ImgType.Wall, pos);
                else drawer.DrawMapImage(Drawer.ImgType.Floor, pos);
            });
        }

        // Change of flooded tiles back to floor tiles
        void ChangeFloodedToFloor()
        {
            IterateThroughMap((pos) =>
            {
                if (GetTile(pos) == TileType.Flooded) SetTile(pos, TileType.Floor);
            });
        }


        // Count how many tiles have been flooded
        int CheckFloodFill()
        {
            int count = 0;
            IterateThroughMap((pos) =>
            {
                if (GetTile(pos) == TileType.Flooded) count++;
            });
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
            } while (GetTile(new Position(i, j)) != TileType.Floor);
            return new Position(i, j);
        }
    }
}
