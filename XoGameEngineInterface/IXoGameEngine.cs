namespace XoGameEngineInterface
{
    public interface IXoGameEngine
    {
        Position AnalysePosition(XoField field);

        XoMove GetMove(XoField field);
    }
}
