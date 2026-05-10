using Shared;

namespace OrderApi.Repositories;

public class ProductCacheRepository
{
    private readonly List<Product> _products = [];

    public List<Product> GetAll()
        => _products;

    public Product? GetById(int id)
    {
        return _products
            .FirstOrDefault(x => x.Id == id);
    }

    public void Add(Product product)
    {
        _products.Add(product);
    }

    public void Delete(int id)
    {
        var product =
            _products.FirstOrDefault(x => x.Id == id);

        if(product is not null)
        {
            _products.Remove(product);
        }
    }
}