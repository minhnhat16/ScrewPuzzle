using System;
using System.Collections.Generic;

public class ServiceLocator: SingletonMono<ServiceLocator>  
{
    private Dictionary<Type, object> services = new Dictionary<Type, object>();

    public void Register<T>(T service)
    {
        services[typeof(T)] = service;
    }

    public T Get<T>()
    {
        return (T)services[typeof(T)];
    }
}