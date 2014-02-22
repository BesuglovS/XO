using System;
using System.Collections.Generic;
using XoGameEngineInterface;

namespace DeathEngine
{
    public class DeathEngine : IXoGameEngine
    {
        public Position AnalysePosition(XoField field)
        {
            return null;
        }

        XoMove IXoGameEngine.GetMove(XoField field)
        {
            var freeCellMoves = new List<XoMove>();

            for (byte i = 0; i < 3; i++)
            {
                for (byte j = 0; j < 3; j++)
                {
                    if (field.F[i, j] == 0)
                    {
                        freeCellMoves.Add(new XoMove { line = i, column = j });
                    }
                }
            }

            var rnd1 = new Random();
            var randMoveIndex = rnd1.Next(0, freeCellMoves.Count - 1);

            return freeCellMoves[randMoveIndex];
        }
    }
}
