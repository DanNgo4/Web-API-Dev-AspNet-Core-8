namespace DependencyInjectionDemo.Interfaces;

public interface IService
{
    string Name { get; }
    
    string SayHello();
}
