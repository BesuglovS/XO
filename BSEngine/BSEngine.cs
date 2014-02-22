using System;
using System.Collections.Generic;
using XoGameEngineInterface;

namespace BSEngine
{
    public class DefaultEngine : IXoGameEngine
    {
        public Position AnalysePosition(XoField field)
        {
            // Построение дерева

            // Создаём корневой узел
            var root = new Position {Field = {F = field.F}};

            // Создаём дерево
            CreatePositionsBranches(root);

            return root;
        }

        public XoMove GetMove(XoField field)
        {
            var root = AnalysePosition(field);

            // Создаём список вариантов
            var moves = new List<XoMove>();
            foreach (var nextChoice in root.Next)
            {
                // Если оценки совпали это лучший ход
                if (nextChoice.Evaluation == root.Evaluation)
                {
                    // Ищем что же за ход это был
                    var move = new XoMove { line = 255, column = 255 };
                    for (byte i = 0; i <= 2; i++)
                    {
                        for (byte j = 0; j <= 2; j++)
                        {
                            if (nextChoice.Field.F[i, j] != root.Field.F[i, j])
                            {
                                move.line = i;
                                move.column = j;
                            }
                        }
                    }
                    moves.Add(move);
                }
            }

            // Выбираем случайный
            var rnd1 = new Random();
            var randMoveIndex = rnd1.Next(0, moves.Count - 1);

            return moves[randMoveIndex];
        }

        private void CreatePositionsBranches(Position root)
        {
            // Создаём список пустых ячеек
            var freeCells = new List<XoMove>();
            for (byte i = 0; i <= 2; i++)
            {
                for (byte j = 0; j <= 2; j++)
                {
                    if (root.Field.F[i, j] == 0)
                    {
                        freeCells.Add(new XoMove { line = i, column = j });
                    }
                }
            }


            // Создаём ветви
            // Не просматриваем продолжение выигранных партий
            if (!WonPosition(root))
            {
                foreach (var move in freeCells)
                {
                    var nextPosition = new Position(root);
                    nextPosition.Field.F[move.line, move.column] = DetectMove(nextPosition.Field);

                    root.Next.Add(nextPosition);

                    CreatePositionsBranches(nextPosition);
                }
            }

            switch (root.Next.Count)
            {
                case 0: // В случае если позиция конечная даём оценку.
                    root.Evaluation = EvaluateEndPosition(root);
                    break;
                case 1: // Вариантов нет. Ход только один.
                    root.Evaluation = root.Next[0].Evaluation;
                    break;
                default: // 2 и более
                    // Находим лучший вариант
                    byte bestChance = 255;
                    switch (DetectMove(root.Field))
                    {
                        case 1: // X  1 <= 0 <= 2
                            foreach (var position in root.Next)
                            {
                                switch (position.Evaluation)
                                {
                                    case 1:
                                        bestChance = 1;
                                        break;
                                    case 0:
                                        if (bestChance != 1)
                                            bestChance = 0;
                                        break;
                                    case 2:
                                        if (bestChance == 255)
                                            bestChance = 2;
                                        break;
                                }
                            }
                            break;
                        case 2: // O  2 <= 0 <= 1
                            foreach (var position in root.Next)
                            {
                                switch (position.Evaluation)
                                {
                                    case 2:
                                        bestChance = 2;
                                        break;
                                    case 0:
                                        if (bestChance != 2)
                                            bestChance = 0;
                                        break;
                                    case 1:
                                        if (bestChance == 255)
                                            bestChance = 1;
                                        break;
                                }
                            }
                            break;
                    }

                    root.Evaluation = bestChance;
                    break;
            }
        }

        private bool WonPosition(Position root)
        {
            for (int i = 0; i <= 2; i++)
            {
                if (((root.Field.F[i, 0] == root.Field.F[i, 1]) &&
                     (root.Field.F[i, 1] == root.Field.F[i, 2])) &&
                     (root.Field.F[i, 0] != 0))
                {
                    return true;
                }
            }

            for (int i = 0; i <= 2; i++)
            {
                if (((root.Field.F[0, i] == root.Field.F[1, i]) &&
                     (root.Field.F[1, i] == root.Field.F[2, i])) &&
                     (root.Field.F[0, i] != 0))
                {
                    return true;
                }
            }

            if (((root.Field.F[0, 0] == root.Field.F[1, 1]) &&
                 (root.Field.F[1, 1] == root.Field.F[2, 2])) &&
                 (root.Field.F[0, 0] != 0))
            {
                return true;
            }

            if (((root.Field.F[0, 2] == root.Field.F[1, 1]) &&
                 (root.Field.F[1, 1] == root.Field.F[2, 0])) &&
                 (root.Field.F[0, 2] != 0))
            {
                return true;
            }

            return false;
        }

        private byte DetectMove(XoField field) // Следующий ход делают: 1 - X; 2 - O
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

        private byte EvaluateEndPosition(Position root)
        {
            // Горизонтальные линии
            for (int i = 0; i < 3; i++)
            {
                if ((root.Field.F[i, 0] == root.Field.F[i, 1]) &&
                    (root.Field.F[i, 1] == root.Field.F[i, 2]))
                {
                    return root.Field.F[i, 0];
                }
            }

            // Вериткальные линии
            for (int i = 0; i < 3; i++)
            {
                if ((root.Field.F[0, i] == root.Field.F[1, i]) &&
                    (root.Field.F[1, i] == root.Field.F[2, i]))
                {
                    return root.Field.F[0, i];
                }
            }

            // Диагонали
            if ((root.Field.F[0, 0] == root.Field.F[1, 1]) &&
                (root.Field.F[1, 1] == root.Field.F[2, 2]))
            {
                return root.Field.F[0, 0];
            }

            if ((root.Field.F[2, 0] == root.Field.F[1, 1]) &&
                (root.Field.F[1, 1] == root.Field.F[0, 2]))
            {
                return root.Field.F[2, 0];
            }

            return 0;
        }

    }
}