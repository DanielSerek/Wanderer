using System;

namespace Wanderer.characters
{
    class Boss : Character
    {
        public Boss(Position position, Map map, Drawer drawer, GameControl gameControl, string id) : base(position, map, drawer, gameControl, id)
        {
            Random random = new Random();
            MaxHP = 2 * GameControl.Level * random.Next(1, 7) + random.Next(1, 7);
            CurrentHP = MaxHP;
            DP = GameControl.Level / 2 * random.Next(1, 7) + random.Next(1, 7) / 2;
            SP = GameControl.Level * random.Next(1, 7) + GameControl.Level;
        }

        public override void Move(Direction dir)
        {
            base.Move(dir);
            Drawer.ImgType img_type = Drawer.ImgType.BossDown;
            if (dir == Direction.East)  img_type = Drawer.ImgType.BossRight;
            if (dir == Direction.North) img_type = Drawer.ImgType.BossUp;
            if (dir == Direction.South) img_type = Drawer.ImgType.BossDown;
            if (dir == Direction.West)  img_type = Drawer.ImgType.BossLeft;
            drawer.MoveImage(this, img_type);
        }

        public void MoveBoss()
        {
            SetDirection();
            Move(Dir);
        }
    }
}
