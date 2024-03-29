﻿using Avalonia.Media;
using System;
using System.Collections.Generic;
using Wanderer.characters;

namespace Wanderer
{
    public class GameControl
    {
        public static int Level;
        public Character Player = null;
        public Character Boss = null;
        public List<Skeleton> Skeletons = new List<Skeleton>();
        public List<Loot> Loots = new List<Loot>();
        private Map map;
        private Drawer drawer;
        private bool levelPreparation;
        private bool enemyInfoDisplayed;
        private bool bossGenerated;

        public GameControl(Map map, Drawer drawer)
        {
            Level = 1;
            this.map = map;
            this.drawer = drawer;
            GeneratePlayer();
            GenerateSkeletons(3);
            this.drawer.SideBar();
            PlayerStatusDisplay(GetPlayerInfo());
        }

        private void GeneratePlayer()
        {
            Position pos = map.RandomFreeCell();
            Player = new Player(pos, map, drawer, this, "Player");
            drawer.DrawImage(Player, Drawer.ImgType.HeroDown);
        }

        // Generate the player in next levels
        private void PlacePlayer()
        {
            // Find an empty random cell in the map
            Player.Position = FindEmptyPosition();
            drawer.DrawImage(Player, Drawer.ImgType.HeroDown);
        }

        private void GenerateSkeletons(int num)
        {
            // Generate Skeletons 
            Position pos;
            for (int i = 0; i < num; i++)
            {
                pos = FindEmptyPosition();
                // Add Skeleton to the list of Skeletons
                Skeletons.Add(new Skeleton(pos, map, drawer, this, "Skeleton" + i.ToString()));
                drawer.DrawImage("Skeleton" + i.ToString(), Drawer.ImgType.SkeletonDown, pos);
                (Skeletons[i]).NavigateEnemyToPlayer(Player, map);
            }
        }

        private void GenerateBoss()
        {
            // Generate a Boss
            Position pos = FindEmptyPosition();
            Boss = new Boss(pos, map, drawer, this, "Boss");
            drawer.DrawImage(Boss, Drawer.ImgType.BossDown);
        }

        private Position FindEmptyPosition()
        {
            Position pos;
            do
            {
                pos = map.RandomFreeCell();
            } while (CheckCollissions(pos));
            return pos;
        }

        public void PlayerMove()
        {
            Player.Move(Player.Dir);
        }

        // Player attacks an enemy
        public void AttackEnemy()
        {
            Fight(Player, GetCharacter());
        }

        // Enemy attacks a player
        public void AttackPlayer()
        {
            Fight(GetCharacter(), Player);
        }

        // If the player is next to the boss, the boss starts to attack the player
        public void BossAttacks()
        {
            if (Boss is null) return;
                if (StandingNext((Character)Boss))
            {
                Fight(Boss, Player);
            }
            
            if (Skeletons.Count < 1)
            {
                 Boss.NavigateEnemyToPlayer(Player, map);
                ((Boss)Boss).MoveBoss();
            }
        }

        public void GetSkeletonInfo()
        {
            Character enemy = GetCharacter();

            // Display enemy information
            if (FacingTowardsEnemy(enemy) && !enemyInfoDisplayed)
            {
                string[] tb = { $"{enemy.CurrentHP} / {enemy.MaxHP}", $"{enemy.SP}", $"{enemy.DP}" };
                // Headline
                drawer.AddTextBlocks("Enemy", "Beast", 25, 350, 620);
                // Individual information
                for  (int i = 0; i < tb.Length; i++)
                {   
                    drawer.AddTextBlocks("Enemy" + i.ToString(), tb[i], 20, 400 + i * 50, 670);
                }
                enemyInfoDisplayed = true;
                drawer.DisplayEnemySidebarImages(true);
            }
            // Update information about enemy
            if (FacingTowardsEnemy(enemy) && enemyInfoDisplayed)
            {
                string[] tb = { $"{enemy.CurrentHP} / {enemy.MaxHP}", $"{enemy.SP}", $"{enemy.DP}" };
                for (int i = 0; i < tb.Length; i++)
                {
                    drawer.TextBlocks["Enemy" + i.ToString()].Text = tb[i];
                }
            }
            // Remove enemy information from sidebar
            if (enemyInfoDisplayed && !FacingTowardsEnemy(enemy))
            {
                //Remove Enemy image from the TextBlocks collection
                foreach (var item in drawer.TextBlocks)
                {
                    if (item.Key.Contains("Enemy"))
                    {
                        drawer.Canvas.Children.Remove(item.Value);
                        drawer.TextBlocks.Remove(item.Key);
                    }
                }
                drawer.DisplayEnemySidebarImages(false);
                enemyInfoDisplayed = false;
            }
        }

        // Update status about the player
        public void ShowStatus()
        {
            drawer.UpdateStatusText(GetPlayerInfo());
        }
        
        // Check free place in the map, not to create two Skeletons on the same tile
        private bool CheckCollissions(Position position)
        {
            for (int i = 0; i < Skeletons.Count; i++)
            {
                if (Skeletons[i].Position == position) return true;
            }
            if (Player.Position == position) return true;
            if (Boss != null && (Boss.Position == position)) return true;
            return false;
        }
        
        public void DefinePathsForSkeletons()
        {
            for (int i = 0; i < Skeletons.Count; i++)
            {
                (Skeletons[i]).NavigateEnemyToPlayer(Player, map);
            }
        }

        public void MoveSkeletons()
        {
            for (int i = 0; i < Skeletons.Count; i++)
            {
                if (Skeletons[i].PathPositions.Count <= 0)
                    Skeletons[i].NavigateEnemyToPlayer(Player, map);
                Skeletons[i].MoveSkeleton();
                if (StandingNext(Skeletons[i])) AttackPlayer(); 
            } 
        }

        // Get the character next to the position where a Player is
        private Character GetCharacter()
        {
            foreach (var skeleton in Skeletons)
            {
                if (StandingNext(skeleton) && FacingTowardsEnemy(skeleton)) return skeleton;
            }
            if (Boss == null) return null;
            if (StandingNext(Boss)) return Boss;
            return null;
        }
        
        // Check if someone is standing next to the player
        private bool StandingNext(Character ch)
        {
            if (((Player.Position.X == ch.Position.X - 1) || (Player.Position.X == ch.Position.X + 1)) && (Player.Position.Y == ch.Position.Y)) return true;
            else if (((Player.Position.Y == ch.Position.Y - 1) || (Player.Position.Y == ch.Position.Y + 1)) && (Player.Position.X == ch.Position.X)) return true;
            else return false;
        }

        // Check whether the cell is occupied by any character
        public bool IsCellFree(Position position)
        {
            if (Player.Position == position) return false;
            for (int i = 0; i < Skeletons.Count; i++)
            {
                if (Skeletons[i].Position == position) return false;
            }
            if (Boss != null && (Boss.Position == position)) return false;
            return true;
        }

        // Check if player is facing towards the enemy
        private bool FacingTowardsEnemy(Character ch)
        {
            if (ch == null) return false;
            if ((Player.Dir == Character.Direction.East)  && (ch.Position.X == Player.Position.X + 1)) return true;
            if ((Player.Dir == Character.Direction.West)  && (ch.Position.X == Player.Position.X - 1)) return true;
            if ((Player.Dir == Character.Direction.North) && (ch.Position.Y == Player.Position.Y - 1)) return true;
            if ((Player.Dir == Character.Direction.South) && (ch.Position.Y == Player.Position.Y + 1)) return true;
            return false;
        }

        // Fight between the player and the enemy
        private void Fight(Character attacker, Character defender)
        {
            if (defender == null || attacker == null) return;
            if (attacker is Player && !FacingTowardsEnemy(defender)) return;
            int SV = 0; // Strike Value
            Random random = new Random();
         
            // Strike by the attacker
            SV = attacker.SP + 2 * random.Next(7);
            if (SV > defender.DP)
            {
                defender.CurrentHP -= SV - defender.DP;
                if (attacker is Skeleton || attacker is Boss) drawer.RedScreen();
            }
            drawer.UpdateStatusText(GetPlayerInfo());

            if (defender.CurrentHP < 1)
            {
                defender.CurrentHP = 0;
                if (!(defender is Player)) RenderReward(defender);
                defender.RemoveImage();
                if (defender is Skeleton) Skeletons.Remove((Skeleton)defender);
                if (defender is Boss) Boss = null;
            }
            GetSkeletonInfo();
        }

        // Generates a reward after killing a skeleton
        private void RenderReward(Character defender)
        {
            Random random = new Random();
            int rnd = random.Next(100);
            if (defender is Skeleton)
            {
                if(rnd >= 50)
                {
                    Loots.Add(new Loot(defender.Position, "firstAid" + defender.Id));
                    drawer.DrawImage("firstAid" + defender.Id, Drawer.ImgType.FirstAid, defender.Position);
                }
                if(rnd >= 0 && rnd < 25)
                {
                    Loots.Add(new Loot(defender.Position, "armour" + defender.Id));
                    drawer.DrawImage("armour" + defender.Id, Drawer.ImgType.Armour, defender.Position);
                }
                if (rnd >= 25 && rnd < 50)
                {
                    Loots.Add(new Loot(defender.Position, "weapon" + defender.Id));
                    drawer.DrawImage("weapon" + defender.Id, Drawer.ImgType.Weapon, defender.Position);
                }
            }
            if(defender is Boss)
            {
                Loots.Add(new Loot(defender.Position, "potion" + defender.Id));
                drawer.DrawImage("potion" + defender.Id, Drawer.ImgType.Potion, defender.Position);
            }
        }

        public void GrabLoot()
        {
            Random random = new Random(); 
            for (int i = 0; i < Loots.Count; i++)
            {
                if (Player.Position == Loots[i].Position)
                {
                    drawer.RemoveImage(Loots[i]);
                    if (Loots[i].Id.Contains("firstAid"))
                    {
                        int rnd = random.Next(1, 101);
                        if (rnd <= 10) Player.CurrentHP = Player.MaxHP;
                        if (rnd > 10 && rnd <= 40) Player.CurrentHP += (Player.MaxHP - Player.CurrentHP) / 2;
                        if (rnd > 50) Player.CurrentHP += (Player.MaxHP - Player.CurrentHP) / 5;
                    }
                    if (Loots[i].Id.Contains("armour"))Player.DP = RandomGenerator(Player.DP);
                    if (Loots[i].Id.Contains("weapon")) Player.SP = RandomGenerator(Player.SP);
                    if (Loots[i].Id.Contains("potion"))
                    {
                        int rnd = random.Next(1, 101);
                        if (rnd <= 10) Player.MaxHP += 3 * random.Next(1, 7);
                        if (rnd > 10 && rnd <= 40) Player.MaxHP += 2 * random.Next(1, 7);
                        if (rnd > 50) Player.MaxHP += random.Next(1, 7);
                        Player.CurrentHP = Player.MaxHP;
                    }
                    Loots.RemoveAt(i);
                }
            }
            drawer.UpdateStatusText(GetPlayerInfo());
        }

        private int RandomGenerator(int input)
        {
            Random random = new Random(); 
            int rnd = random.Next(1, 101);
            if (rnd <= 10) return input += Level * random.Next(1, 7); ;
            if (rnd > 10 && rnd <= 50) return input += random.Next(1, 7);
            if (rnd > 50) return ++input;
            else return 0;
        }

        // Checks if conditions for creation of a boss and the next level were met and sets a new level
        public void CheckStatus()
        {
            if (levelPreparation)
            {
                CreateNewLevel();
                return;
            }
            if (Skeletons.Count < 1 && bossGenerated && Boss == null && Loots.Count < 1)
            {
                drawer.Loading();
                levelPreparation = true;
                return;
            }
            if (Skeletons.Count < 1 && !bossGenerated)
            {
                GenerateBoss();
                bossGenerated = true;
            }
            if (Player != null && Player.CurrentHP <= 0)
            {
                drawer.UpdateStatusText(GetPlayerInfo());
                Player = null;
                MainWindow.Timer.Stop();
                drawer.GameOver();
            }
        }
        
        public string[] GetPlayerInfo()
        {
            string[] info = new string[] { $"Floor {Level}", $"{Player.CurrentHP} / {Player.MaxHP}", $"{Player.DP}", $"{Player.SP}"};
            return info;
        }

        // The method is used to display player's status
        public void PlayerStatusDisplay(string[] tb)
        {
            drawer.AddTextBlocks("0", tb[0], 25, 10, 620);
            for (int i = 1; i < tb.Length; i++)
            {
                drawer.AddTextBlocks(i.ToString(), tb[i], 20, 8 + i * 50, 670);
            }
        }

        private void CreateNewLevel()
        {
            Level++;
            Boss = null;
            bossGenerated = false;
            MainWindow.GameSpeed *= 0.9; //Makes movemment of skeletons faster
            levelPreparation = false;
            Skeletons.Clear();
            drawer.Images.Clear();
            drawer.Canvas.Children.Clear();
            map.CreateMap(58);
            GenerateSkeletons(2 + Level);
            PlacePlayer();
            drawer.PrepareDrawer();
            PlayerStatusDisplay(GetPlayerInfo());
        }
    }
}
