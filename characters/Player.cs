using System;

namespace Wanderer
{
    public class Player : Character
    {
        public Player(Position position, Map map, Drawer drawer, GameControl gameControl, string id) : base(position, map, drawer, gameControl, id)
        {
            Random random = new Random();
            MaxHP = 20 + 3 * random.Next(1, 7);
            CurrentHP = MaxHP;
            DP = 2 * random.Next(1, 7);
            SP = 5 + random.Next(1, 7);
        }

        public override void Move(Direction dir)
        {
            base.Move(dir);

            Drawer.ImgType img_type = Drawer.ImgType.HeroDown;

            switch (dir)
            {
                case Direction.West:
                    img_type = Drawer.ImgType.HeroLeft;
                    drawer.MoveImage(this, img_type);
                    break;

                case Direction.East:
                    img_type = Drawer.ImgType.HeroRight;
                    drawer.MoveImage(this, img_type);
                    break;

                case Direction.North:
                    img_type = Drawer.ImgType.HeroUp;
                    drawer.MoveImage(this, img_type);
                    break;

                case Direction.South:
                    img_type = Drawer.ImgType.HeroDown;
                    drawer.MoveImage(this, img_type);
                    break;
            }

        }
    }
}
