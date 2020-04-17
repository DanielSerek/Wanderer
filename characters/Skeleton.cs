using System;
using System.Collections.Generic;

namespace Wanderer.characters
{
    public class Skeleton : Character
    {
        public Skeleton(Position position, Map map, Drawer drawer, GameControl gameControl, string id) : base(position, map, drawer, gameControl, id)
        {
            Random random = new Random();
            MaxHP = 2 * GameControl.Level * random.Next(1, 7);
            CurrentHP = MaxHP;
            DP = random.Next(1, 7) + GameControl.Level;
            SP = GameControl.Level * random.Next(1, 7);
        }

        public override void Move(Direction dir)
        {
            base.Move(dir);
            Drawer.ImgType img_type = Drawer.ImgType.SkeletonDown;
            if (dir == Direction.East)  img_type = Drawer.ImgType.SkeletonRight;
            if (dir == Direction.North) img_type = Drawer.ImgType.SkeletonUp;
            if (dir == Direction.South) img_type = Drawer.ImgType.SkeletonDown;
            if (dir == Direction.West)  img_type = Drawer.ImgType.SkeletonLeft;
            drawer.MoveImage(this, img_type);
        }

        public void MoveSkeleton()
        {
            SetDirection();
            Move(Dir);
        }
    }
}
