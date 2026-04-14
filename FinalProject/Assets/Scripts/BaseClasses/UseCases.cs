using Cysharp.Threading.Tasks;

public abstract class UseCase<TRequest, TResponse>
{
    public abstract TResponse Execute(TRequest request);
}

// For async use cases
public abstract class AsyncUseCase<TRequest, TResponse>
{
    public abstract UniTask<TResponse> ExecuteAsync(TRequest request);
}
