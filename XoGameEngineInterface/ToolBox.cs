using System.Collections.Generic;

namespace XoGameEngineInterface
{
    public static class ToolBox
    {
        public static string BytetoChar(byte sign)
        {
            switch (sign)
            {
                case 0:
                    return "-";
                case 1:
                    return "X";
                case 2:
                    return "O";
                default:
                    return "Error";
            }
        }
    }

    public struct XoField
    {
        public byte[,] F;
    }

    public struct XoMove
    {
        public byte line, column;
    }

    public class Position
    {
        public XoField Field;
        public byte Evaluation; // 0 - Draw: 1 - X; 2 - O
        public List<Position> Next;

        public Position()
        {
            Field = new XoField();
            Field.F = new byte[3, 3];
            Evaluation = 255;
            Next = new List<Position>();
        }

        public Position(Position source)
        {
            Field = new XoField();

            Field.F = new byte[3, 3];
            for (int i = 0; i <= 2; i++)
            {
                for (int j = 0; j <= 2; j++)
                {
                    Field.F[i, j] = source.Field.F[i, j];
                }
            }

            Evaluation = 255;

            Next = new List<Position>();
        }
    }
}
