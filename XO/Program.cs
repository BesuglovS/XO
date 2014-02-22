using System;
using XO.Core;

namespace XO
{
    class Program
    {
        static void Main()
        {
            byte gameType = 0;

            do
            {
                Console.Clear();
                Console.WriteLine("Welcome!");
                Console.WriteLine("1 - 2 человека");
                Console.WriteLine("2 - Человек против системы");
                Console.WriteLine("3 - Система против человека");
                Console.WriteLine("4 - 2 системы");
                Console.WriteLine("5 - Тест двух движков");
                Console.WriteLine("0 - Выход");
                Console.Write("Введите тип игры: ");

                bool gameTypeOK;
                do
                {
                    gameTypeOK = true;
                    try
                    {
                        gameType = byte.Parse(Console.ReadLine());
                    }
                    catch
                    {
                        Console.WriteLine("Неправильный формат.");
                        gameTypeOK = false;
                    }
                } while (gameTypeOK == false);

                var myGame = new XoGame(gameType);
                myGame.RunGame();
            } while (gameType != 0);
        }
    }
}
