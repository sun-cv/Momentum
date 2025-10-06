

namespace Momentum
{
    

public interface IVisitor {
    void Visit<T>(T visitable) where T : IVisitable;

}

public interface IVisitable {
    void Accept(IVisitor visitor);
}

}