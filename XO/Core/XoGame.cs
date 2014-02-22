using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using XoGameEngineInterface;

namespace XO.Core
{
    class XoGame
    {
        private readonly byte _type;
        private XoField _field;
        static List<string> dllPaths;

        public XoGame(byte type)
        {
            _type = type;
            _field.F = new byte[3, 3];
        }

        private void HumansGame()
        {
            do
            {
                MakeMove(HumanMove());
            } while (Finished == 3);

            PrintField(true);

            if (Finished == 0)
            {
                Console.WriteLine("Ничья.");
            }
            else
            {
                Console.WriteLine("Победа! Выиграли - " + ToolBox.BytetoChar(Finished));
            }

            Console.ReadKey();
        }

        private void MakeMove(XoMove move)
        {
            _field.F[move.line, move.column] = DetectMove(_field);
        }

        private static byte DetectMove(XoField field) // Следующий ход делают: 1 - X; 2 - O
        {
            byte xcol = 0,
                 ocol = 0;
            for (int i = 0; i <= 2; i++)
            {
                for (int j = 0; j <= 2; j++)
                {
                    switch (field.F[i, j])
                    {
                        case 1:
                            xcol++;
                            break;
                        case 2:
                            ocol++;
                            break;
                    }
                }
            }

            return (byte)((xcol > ocol) ? 2 : 1);
        }

        private void PrintField(bool clearConsole)
        {
            if (clearConsole)
            {
                Console.Clear();
            }

            for (int i = 0; i <= 2; i++)
            {
                for (int j = 0; j <= 2; j++)
                {
                    Console.Write(ToolBox.BytetoChar(_field.F[i, j]) + " ");
                }
                Console.WriteLine();
            }
        }


        private void HumanVsSystem(bool humanPlaysForX)
        {
            var engines = GetXoGameEngines();

            var myEngineInfo = ChooseEngine(engines, "Выберите игровой движок: ", true);

            var myEngine = LoadEngine(myEngineInfo);

            bool correctEngineMove;

            do
            {
                correctEngineMove = true;

                if (((DetectMove(_field) == 1) && (humanPlaysForX)) || // Ходят Х и человек за Х
                    ((DetectMove(_field) == 2) && (!humanPlaysForX)))   // Ходят О и человек не за Х
                {
                    MakeMove(HumanMove());
                }
                else
                {
                    var engineMove = myEngine.GetMove(_field);
                    if (CheckMove(engineMove, _field))
                    {
                        MakeMove(engineMove);
                    }
                    else
                    {
                        PrintField(true);

                        Console.WriteLine("Ошибка движка. Попытка сделать ход {" +
                            (engineMove.line + 1) + "," + (engineMove.column + 1) + "}");
                        correctEngineMove = false;
                    }
                }
            } while ((Finished == 3) && (correctEngineMove));
            
            if (correctEngineMove)
            {
                PrintField(true);

                if (Finished == 0)
                {
                    Console.WriteLine("Ничья.");
                }
                else
                {
                    Console.WriteLine("Победа! Выиграли - " + ToolBox.BytetoChar(Finished));
                }
            }

            Console.ReadKey();
        }

        private static bool CheckMove(XoMove engineMove, XoField field)
        {
            return field.F[engineMove.line, engineMove.column] == 0;
        }

        private IXoGameEngine LoadEngine(EngineInfo myEngineInfo)
        {
            var assembly = Assembly.LoadFrom(myEngineInfo.DllPath);

            var engineType = 
                assembly.GetTypes().FirstOrDefault
                (dllType => dllType.GetInterface("XoGameEngineInterface.IXoGameEngine") != null);

            return (IXoGameEngine)Activator.CreateInstance(engineType);
        }

        private static EngineInfo ChooseEngine(List<EngineInfo> engines,
                                               string prompt,
                                               bool showEnginesList)
        {
            if (showEnginesList)
            {
                for (int i = 0; i < engines.Count; i++)
                {
                    Console.WriteLine((i + 1).ToString() + ") " +
                                      engines[i].Name + " (" +
                                      engines[i].DllPath + ")");
                }
            }

            Console.Write(prompt);

            var engineIndex = int.Parse(Console.ReadLine()) - 1;

            return engines[engineIndex];
        }

        private static List<EngineInfo> GetXoGameEngines()
        {
            var engines = new List<EngineInfo>();

            string executingPath = Path.GetDirectoryName(
                Assembly.GetExecutingAssembly().Location);

            //var dlls = Directory.GetFiles(executingPath, "*.dll");

            for (int i = 0; i < 3; i++)
            {
                executingPath = Path.GetDirectoryName(executingPath);
            }

            dllPaths = new List<string>();

            DirSearch(executingPath, "*.dll");

            foreach (var dllPath in dllPaths)
            {
                var assembly = Assembly.LoadFrom(dllPath);

                var engineType = assembly.GetTypes().FirstOrDefault
                    (dllType => dllType.GetInterface("XoGameEngineInterface.IXoGameEngine") != null);

                if (engineType != null)
                {
                    engines.Add(new EngineInfo { Name = engineType.Name, DllPath = dllPath });
                }
            }


            return engines;
        }

        static void DirSearch(string searchDir,string searchPattern)
        {
            if (dllPaths == null)
                dllPaths = new List<string>();

            try
            {
                foreach (var directory in Directory.GetDirectories(searchDir))
                {
                    dllPaths.AddRange(Directory.GetFiles(directory, searchPattern));
                    DirSearch(directory, searchPattern);
                }
            }
            catch (System.Exception excpt)
            {
                Console.WriteLine(excpt.Message);
            }
        }

        private XoMove HumanMove()
        {
            var result = new XoMove();

            do
            {
                PrintField(true);

                Console.Write("Введите строку хода: ");
                result.line = (byte)(byte.Parse(Console.ReadLine()) - 1);

                Console.Write("Введите столбец хода: ");
                result.column = (byte)(byte.Parse(Console.ReadLine()) - 1);

                if (_field.F[result.line, result.column] != 0)
                {
                    Console.WriteLine("Неправильный ход.");
                    Console.ReadKey();
                }
            } while (_field.F[result.line, result.column] != 0);

            return result;
        }

        private void SystemsGame()
        {
            var engines = GetXoGameEngines();

            var enginePlaysXInfo =
                ChooseEngine(engines, "Выберите игровой движок для крестиков: ", true);

            var enginePlaysOInfo =
                ChooseEngine(engines, "Выберите игровой движок для ноликов: ", false);

            var enginePlaysX = LoadEngine(enginePlaysXInfo);
            var enginePlaysO = LoadEngine(enginePlaysOInfo);

            var engineMove = new XoMove();
            bool correctMove;
            do
            {
                switch (DetectMove(_field))
                {
                    case 1: // X
                        engineMove = enginePlaysX.GetMove(_field);
                        break;
                    case 2: // O
                        engineMove = enginePlaysO.GetMove(_field);
                        break;
                }

                correctMove = CheckMove(engineMove, _field);
                if (correctMove)
                    MakeMove(engineMove);

                PrintField(true);

                Console.WriteLine("Для продолжения нажмите любую клавишу...");
                Console.ReadKey();

            } while ((Finished == 3) && (correctMove));

            PrintField(true);

            if (!correctMove)
            {
                Console.WriteLine("Попытка сделать ход в непустую ячейку: {0}, {1}",
                    engineMove.line, engineMove.column);
                return;
            }

            if (Finished == 0)
            {
                Console.WriteLine("Ничья.");
            }
            else
            {
                Console.WriteLine("Победа! Выиграли - " + ToolBox.BytetoChar(Finished));
            }

            Console.ReadKey();
        }

        private byte Finished
        {
            get
            {
                for (int i = 0; i <= 2; i++)
                {
                    if (((_field.F[i, 0] == _field.F[i, 1]) &&
                        (_field.F[i, 1] == _field.F[i, 2])) &&
                        (_field.F[i, 0] != 0))
                    {
                        return _field.F[i, 0];
                    }
                }

                for (int i = 0; i <= 2; i++)
                {
                    if (((_field.F[0, i] == _field.F[1, i]) &&
                        (_field.F[1, i] == _field.F[2, i])) &&
                        (_field.F[0, i] != 0))
                    {
                        return _field.F[0, i];
                    }
                }

                if (((_field.F[0, 0] == _field.F[1, 1]) &&
                    (_field.F[1, 1] == _field.F[2, 2])) &&
                    (_field.F[0, 0] != 0))
                {
                    return _field.F[0, 0];
                }

                if (((_field.F[0, 2] == _field.F[1, 1]) &&
                    (_field.F[1, 1] == _field.F[2, 0])) &&
                    (_field.F[0, 2] != 0))
                {
                    return _field.F[0, 2];
                }

                for (int i = 0; i <= 2; i++)
                {
                    for (int j = 0; j <= 2; j++)
                    {
                        if (_field.F[i, j] == 0)
                            return 3;
                    }
                }

                return 0;
            }
        }

        public void RunGame()
        {
            switch (_type)
            {
                case 1:
                    HumansGame();
                    break;
                case 2:
                    HumanVsSystem(humanPlaysForX: true);
                    break;
                case 3:
                    HumanVsSystem(humanPlaysForX: false);
                    break;
                case 4:
                    SystemsGame();
                    break;
                case 5:
                    EnginesTest();
                    break;
            }
        }

        private void EnginesTest()
        {
            var engines = GetXoGameEngines();

            var engineInfo1 =
                ChooseEngine(engines, "Выберите первый игровой движок: ", true);

            var engineInfo2 =
                ChooseEngine(engines, "Выберите второй игровой движок: ", false);

            Console.Write("Введите количество туров: ");
            var roundQuantity = int.Parse(Console.ReadLine());

            var engine1 = LoadEngine(engineInfo1);
            var engine2 = LoadEngine(engineInfo2);

            int engine1Wins = 0,   // Побед первого движка
                engine2Wins = 0,   // Побед второго движка
                engine1Faults = 0, // Ошибок первого движка
                engine2Faults = 0, // Ошибок второго движка
                draws = 0;         // Ничьих
                

            var engineMove = new XoMove();
            byte correctMoves;

            for (int i = 1; i < (roundQuantity * 2) + 1; i++)
            {
                _field = new XoField {F = new byte[,] {{0, 0, 0}, {0, 0, 0}, {0, 0, 0}}};

                do
                {
                    correctMoves = 0;

                    switch (DetectMove(_field))
                    {
                        case 1: // X
                            engineMove = i % 2 == 1 ?
                                     engine1.GetMove(_field) :
                                     engine2.GetMove(_field);
                            
                            break;
                        case 2: // O
                            engineMove = i % 2 == 1 ?
                                     engine2.GetMove(_field) :
                                     engine1.GetMove(_field);
                            break;
                    }

                    if (CheckMove(engineMove, _field))
                        MakeMove(engineMove);
                    else
                        correctMoves = (byte)(((DetectMove(_field)) == 1) ? 
                                              ((i % 2 == 1) ? 1 : 2) :
                                              ((i % 2 == 1) ? 2 : 1));

                } while ((Finished == 3) && (correctMoves == 0));

                if (correctMoves != 0)
                {
                    if (correctMoves == 1) // Ошибся 1-й движок
                        engine1Faults++;
                    else // Второй
                        engine2Faults++;
                }
                else
                {
                    switch (Finished)
                    {
                        case 0:
                            draws++;
                            break;
                        case 1:
                            if (i%2 == 1)
                            {
                                engine1Wins++;
                            }
                            else
                            {
                                engine2Wins++;
                            }
                            break;
                        case 2:
                            if (i%2 == 1)
                            {
                                engine2Wins++;
                            }
                            else
                            {
                                engine1Wins++;
                            }
                            break;
                    }
                }

                Console.Clear();
                Console.WriteLine("Всего игр = {0}.", i);
                Console.WriteLine("Ничьих = {0}", draws);
                Console.WriteLine("Побед {0} = {1}", engineInfo1.Name, engine1Wins);
                Console.WriteLine("Побед {0} = {1}", engineInfo2.Name, engine2Wins);
                Console.WriteLine("Ошибок {0} = {1}", engineInfo1.Name, engine1Faults);
                Console.WriteLine("Ошибок {0} = {1}", engineInfo2.Name, engine2Faults);
            }

            Console.ReadKey();
        }
    }
}
